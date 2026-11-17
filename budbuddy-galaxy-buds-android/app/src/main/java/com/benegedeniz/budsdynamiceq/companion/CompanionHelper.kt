package com.benegedeniz.budsdynamiceq.companion

import android.annotation.SuppressLint
import android.companion.AssociationInfo
import android.companion.AssociationRequest
import android.companion.BluetoothDeviceFilter
import android.companion.CompanionDeviceManager
import android.content.Context
import android.content.IntentSender
import android.content.pm.PackageManager
import android.os.Build
import android.os.Handler
import android.os.Looper
import android.util.Log

/**
 * Associates the saved Galaxy Buds with this app through Android's Companion Device Manager.
 *
 * Why it matters on a non-Samsung phone (e.g. Pixel):
 *  - Companion apps may start foreground services from the background, so the app can
 *    reconnect when the buds come back even if Android restricts background starts.
 *  - Companion apps may start activities from the background, which head-gesture actions
 *    need to open the assistant (Gemini) or launch apps.
 *  - REQUEST_COMPANION_RUN_IN_BACKGROUND exempts the app from battery optimizations.
 *  - The system binds [BudsCompanionService] when the buds connect (Android 12+).
 */
object CompanionHelper {
    private const val TAG = "CompanionHelper"

    fun isSupported(context: Context): Boolean =
        context.packageManager.hasSystemFeature(PackageManager.FEATURE_COMPANION_DEVICE_SETUP)

    private fun manager(context: Context): CompanionDeviceManager? =
        if (isSupported(context)) context.getSystemService(CompanionDeviceManager::class.java) else null

    fun isAssociated(context: Context, mac: String?): Boolean {
        if (mac == null) return false
        val cdm = manager(context) ?: return false
        return try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                cdm.myAssociations.any { it.deviceMacAddress?.toString()?.equals(mac, ignoreCase = true) == true }
            } else {
                @Suppress("DEPRECATION")
                cdm.associations.any { it.equals(mac, ignoreCase = true) }
            }
        } catch (e: Exception) {
            Log.w(TAG, "Could not read associations: ${e.message}")
            false
        }
    }

    /**
     * Starts the association flow. [onPending] receives the IntentSender for the system
     * confirmation dialog, which the caller must launch from an Activity.
     */
    fun requestAssociation(
        context: Context,
        mac: String,
        onPending: (IntentSender) -> Unit,
        onFailure: (CharSequence?) -> Unit
    ) {
        val cdm = manager(context) ?: return onFailure(null)
        val request = AssociationRequest.Builder()
            .addDeviceFilter(BluetoothDeviceFilter.Builder().setAddress(mac).build())
            .setSingleDevice(true)
            .build()

        val callback = object : CompanionDeviceManager.Callback() {
            // API 33+ calls onAssociationPending; older versions call onDeviceFound.
            override fun onAssociationPending(intentSender: IntentSender) = onPending(intentSender)

            @Deprecated("Replaced by onAssociationPending on API 33+")
            override fun onDeviceFound(intentSender: IntentSender) = onPending(intentSender)

            override fun onAssociationCreated(associationInfo: AssociationInfo) {
                Log.i(TAG, "Association created: ${associationInfo.id}")
                startObservingPresence(context, mac)
            }

            override fun onFailure(error: CharSequence?) {
                Log.w(TAG, "Association failed: $error")
                onFailure(error)
            }
        }

        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                cdm.associate(request, context.mainExecutor, callback)
            } else {
                @Suppress("DEPRECATION")
                cdm.associate(request, callback, Handler(Looper.getMainLooper()))
            }
        } catch (e: Exception) {
            Log.e(TAG, "associate() failed", e)
            onFailure(e.message)
        }
    }

    /** Asks the system to bind [BudsCompanionService] whenever the buds connect/disconnect. */
    @SuppressLint("MissingPermission")
    fun startObservingPresence(context: Context, mac: String?) {
        if (mac == null || Build.VERSION.SDK_INT < Build.VERSION_CODES.S) return
        val cdm = manager(context) ?: return
        if (!isAssociated(context, mac)) return
        try {
            @Suppress("DEPRECATION")
            cdm.startObservingDevicePresence(mac)
        } catch (e: Exception) {
            Log.w(TAG, "startObservingDevicePresence failed: ${e.message}")
        }
    }

    fun disassociate(context: Context, mac: String?) {
        if (mac == null) return
        val cdm = manager(context) ?: return
        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                cdm.myAssociations
                    .filter { it.deviceMacAddress?.toString()?.equals(mac, ignoreCase = true) == true }
                    .forEach { cdm.disassociate(it.id) }
            } else {
                @Suppress("DEPRECATION")
                cdm.disassociate(mac)
            }
        } catch (e: Exception) {
            Log.w(TAG, "disassociate failed: ${e.message}")
        }
    }
}
