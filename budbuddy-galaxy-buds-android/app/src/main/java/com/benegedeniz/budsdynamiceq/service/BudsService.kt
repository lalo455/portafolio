package com.benegedeniz.budsdynamiceq.service

import android.app.Service
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.os.Build
import android.os.IBinder
import android.util.Log
import androidx.core.content.ContextCompat
import com.benegedeniz.budsdynamiceq.R
import com.benegedeniz.budsdynamiceq.data.model.EqPreset
import com.benegedeniz.budsdynamiceq.data.model.NoiseControlMode
import com.benegedeniz.budsdynamiceq.di.ServiceLocator
import com.benegedeniz.budsdynamiceq.gesture.GestureActionExecutor
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.cancel
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.collectLatest
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.launch
import com.benegedeniz.budsdynamiceq.gesture.TtsManager

class BudsService : Service() {

    override fun attachBaseContext(newBase: Context) {
        super.attachBaseContext(com.benegedeniz.budsdynamiceq.util.LanguageUtils.setLocale(newBase))
    }

    companion object {
        private const val TAG = "BudsService"
        private val RECONNECT_BACKOFF_MS = longArrayOf(3_000, 10_000, 30_000)
    }

    private val scope = CoroutineScope(Dispatchers.IO + kotlinx.coroutines.SupervisorJob())
    private val transientNotificationFlow = MutableStateFlow<Pair<String, String>?>(null)
    
    private lateinit var notificationManagerHelper: NotificationManagerHelper
    private var notificationCoordinator: NotificationCoordinator? = null
    private var actionExecutor: GestureActionExecutor? = null
    private var networkNcMonitor: NetworkNcMonitor? = null



    private val toggleReceiver = object : BroadcastReceiver() {
        override fun onReceive(context: Context?, intent: Intent?) {
            Log.d(TAG, "toggleReceiver onReceive action=${intent?.action}")
            if (intent?.action == "com.benegedeniz.budsdynamiceq.TOGGLE_NC") {
                val budsController = ServiceLocator.provideBudsController(this@BudsService)
                val currentNc = budsController.activeNoiseControl.value
                val pL = budsController.placementL.value
                val pR = budsController.placementR.value
                val oneEarbudEnabled = budsController.oneEarbudNoiseControlEnabled.value
                
                val wearingOne = (pL == com.benegedeniz.budsdynamiceq.data.model.PlacementState.WEARING && pR != com.benegedeniz.budsdynamiceq.data.model.PlacementState.WEARING) || 
                                 (pR == com.benegedeniz.budsdynamiceq.data.model.PlacementState.WEARING && pL != com.benegedeniz.budsdynamiceq.data.model.PlacementState.WEARING)
                                 
                val effectiveModel = budsController.effectiveModel.value
                val nextMode = if (wearingOne && !oneEarbudEnabled) {
                    if (currentNc == NoiseControlMode.TRANSPARENT) NoiseControlMode.OFF else if (effectiveModel.supportsTransparencyNC) NoiseControlMode.TRANSPARENT else NoiseControlMode.OFF
                } else {
                    if (currentNc == NoiseControlMode.NOISE_CANCELLATION) {
                        if (effectiveModel.supportsTransparencyNC) NoiseControlMode.TRANSPARENT else NoiseControlMode.OFF
                    } else if (currentNc == NoiseControlMode.TRANSPARENT) {
                        NoiseControlMode.NOISE_CANCELLATION
                    } else {
                        NoiseControlMode.NOISE_CANCELLATION
                    }
                }
                
                Log.d(TAG, "Toggle nextMode: $nextMode, wearingOne: $wearingOne, oneEarbudEnabled: $oneEarbudEnabled")
                budsController.sendNoiseControl(nextMode)
            }
        }
    }

    override fun onCreate() {
        super.onCreate()
        
        val filter = IntentFilter("com.benegedeniz.budsdynamiceq.TOGGLE_NC")
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            registerReceiver(toggleReceiver, filter, Context.RECEIVER_NOT_EXPORTED)
        } else {
            @Suppress("UnspecifiedRegisterReceiverFlag")
            registerReceiver(toggleReceiver, filter)
        }

        notificationManagerHelper = NotificationManagerHelper(this)
        notificationManagerHelper.createNotificationChannel()
        startForeground(1, notificationManagerHelper.buildNotification(
            titleText = getString(R.string.connecting_97), 
            ruleNcText = getString(R.string.waiting_for_connection), 
            hardwareNcText = "", 
            lBatteryText = "", 
            rBatteryText = "", 
            isLWorn = false,
            isRWorn = false,
            isConnected = false,
            toggleButtonText = ""
        ))
        
        val budsController = ServiceLocator.provideBudsController(this)
        val rulesRepository = ServiceLocator.provideRulesRepository(this)
        val mediaObserver = ServiceLocator.provideMediaObserver(this)
        val gestureRepo = ServiceLocator.provideGestureRepository(this)
        val wearStateRepo = ServiceLocator.provideWearStateRepository(this)
        val gestureDetector = ServiceLocator.provideGestureDetector(this)
        val noiseDetector = ServiceLocator.provideNoiseDetector(this)
        val actionExecutor = GestureActionExecutor(this, budsController)
        this.actionExecutor = actionExecutor

        // Keep the companion-presence observation alive (it is per association, but re-asking is harmless).
        com.benegedeniz.budsdynamiceq.companion.CompanionHelper.startObservingPresence(
            this, budsController.savedDeviceMac.value
        )

        budsController.startAutoConnect()
        mediaObserver.startObserving()

        // Sync manual defaults from SharedPreferences
        val prefs = getSharedPreferences("BudsPrefs", MODE_PRIVATE)
        val savedPresetName = prefs.getString("default_preset", null)
        val resolvedPreset = savedPresetName?.let {
            try { EqPreset.valueOf(it) } catch (_: IllegalArgumentException) { null }
        }
        budsController.setManualPreset(resolvedPreset ?: EqPreset.NORMAL)

        val savedNcName = prefs.getString("default_nc", null)
        val resolvedNc = savedNcName?.let {
            try { NoiseControlMode.valueOf(it) } catch (_: IllegalArgumentException) { null }
        }
        budsController.setManualNoiseControl(resolvedNc ?: NoiseControlMode.IGNORE)

        ServiceLocator.initFromPrefs(this)
        val headShakeEnabledFlow = ServiceLocator.headShakeEnabled
        val requireBothEarbudsFlow = ServiceLocator.requireBothEarbuds
        val pauseMediaOnConversationFlow = ServiceLocator.pauseMediaOnConversation

        scope.launch {
            gestureRepo.loadGestures()
            wearStateRepo.loadActions()
        }

        scope.launch {
            delay(2000)
            // If an established link drops (SPP hiccup, buds briefly out of range), retry from
            // inside the running service with backoff. Restarting the service later from a
            // background broadcast can be blocked by Android 12+ FGS start restrictions.
            var wasConnected = false
            var reconnectAttempt = 0
            combine(
                budsController.isConnected,
                budsController.isConnecting
            ) { connected, connecting ->
                Pair(connected, connecting)
            }.collectLatest { (connected, connecting) ->
                if (connected) {
                    wasConnected = true
                    reconnectAttempt = 0
                    return@collectLatest
                }
                if (connecting) return@collectLatest

                val canRetry = wasConnected &&
                    !budsController.disconnectedByUser &&
                    budsController.isBluetoothEnabled() &&
                    reconnectAttempt < RECONNECT_BACKOFF_MS.size
                if (canRetry) {
                    val wait = RECONNECT_BACKOFF_MS[reconnectAttempt++]
                    Log.i(TAG, "Link lost, reconnect attempt $reconnectAttempt in ${wait}ms")
                    delay(wait)
                    budsController.startAutoConnect()
                } else {
                    Log.i(TAG, "Not connected and not connecting, stopping service.")
                    stopSelf()
                }
            }
        }

        val gestureCoordinator = GestureCoordinator(
            scope = scope,
            budsController = budsController,
            gestureRepo = gestureRepo,
            gestureDetector = gestureDetector,
            noiseDetector = noiseDetector,
            headShakeEnabledFlow = headShakeEnabledFlow,
            requireBothEarbudsFlow = requireBothEarbudsFlow
        )
        gestureCoordinator.start()

        val wearStateManager = WearStateManager(
            scope = scope,
            budsController = budsController,
            wearStateRepo = wearStateRepo,
            actionExecutor = actionExecutor
        )
        wearStateManager.start()

        val imuManager = ImuManager(
            context = this,
            scope = scope,
            budsController = budsController,
            gestureDetector = gestureDetector,
            noiseDetector = noiseDetector
        )
        imuManager.start()

        val networkNcMonitor = ServiceLocator.provideNetworkNcMonitor(this).also { it.start() }
        this.networkNcMonitor = networkNcMonitor

        val rulesCoordinator = RulesCoordinator(
            context = this,
            scope = scope,
            budsController = budsController,
            mediaObserver = mediaObserver,
            rulesRepository = rulesRepository,
            networkNcOverride = networkNcMonitor.ncOverride
        )
        rulesCoordinator.start()

        notificationCoordinator = NotificationCoordinator(
            context = this,
            scope = scope,
            budsController = budsController,
            transientNotificationFlow = transientNotificationFlow,
            notificationManagerHelper = notificationManagerHelper
        )
        notificationCoordinator!!.start()

        val widgetCoordinator = WidgetCoordinator(
            context = this,
            scope = scope,
            budsController = budsController
        )
        widgetCoordinator.start()

        WellbeingCoordinator(
            context = this,
            scope = scope,
            budsController = budsController,
            actionExecutor = actionExecutor
        ).start()

        // Handle gesture detection feedback
        scope.launch {
            gestureDetector.detectedGesture.collect { gesture ->
                try {
                    if (gestureDetector.isTrainingMode) return@collect
                    val localizedContext = com.benegedeniz.budsdynamiceq.util.LanguageUtils.setLocale(this@BudsService)
                    transientNotificationFlow.value = localizedContext.getString(R.string.service_gesture_detected) to localizedContext.getString(R.string.service_flow_sequence, gesture.name)
                    actionExecutor.execute(gesture.actions, gesture.playChime)
                    transientNotificationFlow.value = localizedContext.getString(R.string.service_gesture_detected) to localizedContext.getString(R.string.service_flow_complete, gesture.name)
                    delay(1000)
                    transientNotificationFlow.value = null
                } catch (e: Exception) {
                    e.printStackTrace()
                    transientNotificationFlow.value = null
                }
            }
        }

        // Handle noise detection feedback
        scope.launch {
            noiseDetector.noiseDetected.collect { noiseProfile ->
                try {
                    if (gestureDetector.isTrainingMode) return@collect
                    transientNotificationFlow.value = getString(R.string.service_movement_cancelled) to getString(R.string.service_filtering_noise, noiseProfile.name)
                    delay(1000)
                    transientNotificationFlow.value = null
                } catch (e: Exception) {
                    e.printStackTrace()
                    transientNotificationFlow.value = null
                }
            }
        }

        // Handle conversation mode (pause on transparency) and auto play on ANC
        val playMediaOnAncFlow = ServiceLocator.playMediaOnAnc
        val conversationHandler = ConversationMediaHandler(this, actionExecutor, mediaObserver)
        scope.launch {
            var previousNcMode: NoiseControlMode? = null
            combine(
                budsController.activeNoiseControl,
                pauseMediaOnConversationFlow,
                playMediaOnAncFlow
            ) { ncMode, pauseOnTransparency, playOnAnc ->
                Triple(ncMode, pauseOnTransparency, playOnAnc)
            }.collect { (ncMode, pauseOnTransparency, playOnAnc) ->
                if (ncMode == null) {
                    // Disconnected: undo any ducking, forget the previous mode.
                    conversationHandler.onDisconnected()
                    previousNcMode = null
                    return@collect
                }
                if (previousNcMode != null && ncMode != previousNcMode) {
                    if (pauseOnTransparency && ncMode == NoiseControlMode.TRANSPARENT) {
                        // Pause or lower the volume, depending on the user's choice.
                        conversationHandler.onConversationStart()
                    } else if (previousNcMode == NoiseControlMode.TRANSPARENT && conversationHandler.isActive) {
                        // Conversation over: resume playback / restore the volume.
                        conversationHandler.onConversationEnd()
                    } else if (playOnAnc && ncMode == NoiseControlMode.NOISE_CANCELLATION) {
                        actionExecutor.triggerPlay()
                    }
                }
                previousNcMode = ncMode
            }
        }
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        if (intent?.action == "com.benegedeniz.budsdynamiceq.AUTO_CONNECT") {
            ServiceLocator.provideBudsController(this).startAutoConnect()
        }
        return START_STICKY
    }

    override fun onBind(intent: Intent?): IBinder? = null

    override fun onTaskRemoved(rootIntent: Intent?) {
        super.onTaskRemoved(rootIntent)
        val gestureDetector = ServiceLocator.provideGestureDetector(this)
        gestureDetector.isTrainingMode = false
    }

    override fun onDestroy() {
        super.onDestroy()
        notificationCoordinator?.stop()
        actionExecutor?.release()
        networkNcMonitor?.stop()
        try { unregisterReceiver(toggleReceiver) } catch (_: IllegalArgumentException) {}
        scope.cancel()
        
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
            stopForeground(STOP_FOREGROUND_REMOVE)
        } else {
            @Suppress("DEPRECATION")
            stopForeground(true)
        }
        
        val manager = getSystemService(android.app.NotificationManager::class.java)
        manager.cancel(1)
        
        val mediaObserver = ServiceLocator.provideMediaObserver(this)
        val budsController = ServiceLocator.provideBudsController(this)
        mediaObserver.stopObserving()
        budsController.disconnect(forget = false)
    }
}
