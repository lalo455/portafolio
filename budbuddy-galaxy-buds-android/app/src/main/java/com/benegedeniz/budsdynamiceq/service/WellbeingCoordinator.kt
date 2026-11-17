package com.benegedeniz.budsdynamiceq.service

import android.Manifest
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.os.Build
import android.util.Log
import androidx.core.app.NotificationCompat
import androidx.core.app.NotificationManagerCompat
import androidx.core.content.ContextCompat
import com.benegedeniz.budsdynamiceq.MainActivity
import com.benegedeniz.budsdynamiceq.R
import com.benegedeniz.budsdynamiceq.bluetooth.BudsController
import com.benegedeniz.budsdynamiceq.data.model.PlacementState
import com.benegedeniz.budsdynamiceq.gesture.GestureActionExecutor
import com.benegedeniz.budsdynamiceq.gesture.QuaternionSample
import com.benegedeniz.budsdynamiceq.util.LanguageUtils
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import kotlin.math.abs
import kotlin.math.acos
import kotlin.math.min

/**
 * Wellbeing extras that Samsung phones get from Galaxy Wearable:
 *  - Per-earbud low battery alert (notification + optional spoken warning).
 *  - Neck stretch reminder after a period of wearing the buds without moving the head much.
 *    Head movement is read from the buds' IMU only when it is already running (head gestures
 *    enabled), so this feature never keeps the sensor on by itself; otherwise it is wear-time based.
 */
class WellbeingCoordinator(
    private val context: Context,
    private val scope: CoroutineScope,
    private val budsController: BudsController,
    private val actionExecutor: GestureActionExecutor
) {
    companion object {
        private const val TAG = "WellbeingCoordinator"
        private const val CHANNEL_ID = "buds_alerts_channel"
        private const val NOTIF_ID_BATTERY_L = 2001
        private const val NOTIF_ID_BATTERY_R = 2002
        private const val NOTIF_ID_NECK = 2003

        const val PREF_LOW_BATTERY_ENABLED = "low_battery_alert_enabled"
        const val PREF_LOW_BATTERY_THRESHOLD = "low_battery_threshold"
        const val PREF_LOW_BATTERY_SPEAK = "low_battery_speak"
        const val PREF_NECK_ENABLED = "neck_reminder_enabled"
        const val PREF_NECK_MINUTES = "neck_reminder_minutes"

        const val DEFAULT_LOW_BATTERY_THRESHOLD = 15
        const val DEFAULT_NECK_MINUTES = 45

        /** Rotation (degrees) within a short window that counts as "moved/stretched the neck". */
        private const val STRETCH_ANGLE_DEG = 35.0
        private const val MOVEMENT_WINDOW_MS = 20_000L
        /** Taking the buds off for this long resets the neck timer. */
        private const val WEAR_BREAK_RESET_MS = 60_000L
    }

    private val prefs = context.getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE)

    fun start() {
        createChannel()
        startLowBatteryMonitor(isLeft = true)
        startLowBatteryMonitor(isLeft = false)
        startNeckReminder()
    }

    // ---------------------------------------------------------------- Low battery

    private fun startLowBatteryMonitor(isLeft: Boolean) {
        val levelFlow = if (isLeft) budsController.batteryL else budsController.batteryR
        val chargingFlow = if (isLeft) budsController.chargingL else budsController.chargingR
        val placementFlow = if (isLeft) budsController.placementL else budsController.placementR

        scope.launch {
            var alerted = false
            combine(levelFlow, chargingFlow, placementFlow) { level, charging, placement ->
                Triple(level, charging, placement)
            }.collect { (level, charging, placement) ->
                if (!prefs.getBoolean(PREF_LOW_BATTERY_ENABLED, true) || level < 0) return@collect
                val threshold = prefs.getInt(PREF_LOW_BATTERY_THRESHOLD, DEFAULT_LOW_BATTERY_THRESHOLD)
                val inCase = placement == PlacementState.CASE || placement == PlacementState.CLOSED_CASE

                if (charging || inCase || level > threshold + 5) {
                    // Hysteresis: re-arm only after a real recharge, not on 1% jitter.
                    if (alerted) cancel(if (isLeft) NOTIF_ID_BATTERY_L else NOTIF_ID_BATTERY_R)
                    alerted = false
                    return@collect
                }
                if (!alerted && level in 1..threshold) {
                    alerted = true
                    val loc = LanguageUtils.setLocale(context)
                    val side = loc.getString(if (isLeft) R.string.wellbeing_left_earbud else R.string.wellbeing_right_earbud)
                    val message = loc.getString(R.string.wellbeing_low_battery_text, side, level)
                    notify(
                        if (isLeft) NOTIF_ID_BATTERY_L else NOTIF_ID_BATTERY_R,
                        loc.getString(R.string.wellbeing_low_battery_title),
                        message
                    )
                    if (prefs.getBoolean(PREF_LOW_BATTERY_SPEAK, true) && placement == PlacementState.WEARING) {
                        actionExecutor.speak(message)
                    }
                }
            }
        }
    }

    // ---------------------------------------------------------------- Neck reminder

    @Volatile private var timerStartMs = 0L

    private fun startNeckReminder() {
        // Movement detection from the IMU stream (only flows while the sensor is already on).
        scope.launch {
            var windowRef: QuaternionSample? = null
            var windowStartMs = 0L
            budsController.spatialDataFlow.collect { sample ->
                val now = System.currentTimeMillis()
                val ref = windowRef
                if (ref == null || now - windowStartMs > MOVEMENT_WINDOW_MS) {
                    windowRef = sample
                    windowStartMs = now
                    return@collect
                }
                if (angleBetweenDeg(ref, sample) >= STRETCH_ANGLE_DEG) {
                    if (timerStartMs != 0L) {
                        Log.d(TAG, "Head movement detected, resetting neck timer")
                        timerStartMs = now
                    }
                    windowRef = sample
                    windowStartMs = now
                }
            }
        }

        scope.launch {
            var lastWornMs = 0L
            while (isActive) {
                delay(30_000)
                val now = System.currentTimeMillis()
                val worn = budsController.isConnected.value &&
                    (budsController.placementL.value == PlacementState.WEARING ||
                        budsController.placementR.value == PlacementState.WEARING)

                if (!prefs.getBoolean(PREF_NECK_ENABLED, false)) {
                    timerStartMs = 0L
                    continue
                }
                if (!worn) {
                    if (timerStartMs != 0L && now - lastWornMs > WEAR_BREAK_RESET_MS) timerStartMs = 0L
                    continue
                }
                lastWornMs = now
                if (timerStartMs == 0L) {
                    timerStartMs = now
                    continue
                }
                val minutes = prefs.getInt(PREF_NECK_MINUTES, DEFAULT_NECK_MINUTES)
                if (now - timerStartMs >= minutes * 60_000L) {
                    timerStartMs = now
                    val loc = LanguageUtils.setLocale(context)
                    val text = loc.getString(R.string.wellbeing_neck_text, minutes)
                    notify(NOTIF_ID_NECK, loc.getString(R.string.wellbeing_neck_title), text)
                    actionExecutor.speak(text)
                }
            }
        }
    }

    private fun angleBetweenDeg(a: QuaternionSample, b: QuaternionSample): Double {
        val dot = abs(a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z).toDouble()
        return Math.toDegrees(2.0 * acos(min(1.0, dot)))
    }

    // ---------------------------------------------------------------- Notifications

    private fun createChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val loc = LanguageUtils.setLocale(context)
            val channel = NotificationChannel(
                CHANNEL_ID,
                loc.getString(R.string.wellbeing_channel_name),
                NotificationManager.IMPORTANCE_DEFAULT
            )
            context.getSystemService(NotificationManager::class.java).createNotificationChannel(channel)
        }
    }

    private fun notify(id: Int, title: String, text: String) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU &&
            ContextCompat.checkSelfPermission(context, Manifest.permission.POST_NOTIFICATIONS) != PackageManager.PERMISSION_GRANTED
        ) return
        val pi = PendingIntent.getActivity(
            context, id, Intent(context, MainActivity::class.java), PendingIntent.FLAG_IMMUTABLE
        )
        val notification = NotificationCompat.Builder(context, CHANNEL_ID)
            .setSmallIcon(R.mipmap.ic_launcher)
            .setContentTitle(title)
            .setContentText(text)
            .setStyle(NotificationCompat.BigTextStyle().bigText(text))
            .setContentIntent(pi)
            .setAutoCancel(true)
            .build()
        try {
            NotificationManagerCompat.from(context).notify(id, notification)
        } catch (e: SecurityException) {
            Log.w(TAG, "Notification permission missing: ${e.message}")
        }
    }

    private fun cancel(id: Int) {
        NotificationManagerCompat.from(context).cancel(id)
    }
}
