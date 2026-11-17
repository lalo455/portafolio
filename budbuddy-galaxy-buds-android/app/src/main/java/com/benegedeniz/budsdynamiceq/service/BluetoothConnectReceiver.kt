package com.benegedeniz.budsdynamiceq.service

import android.bluetooth.BluetoothDevice
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.os.Build
import com.benegedeniz.budsdynamiceq.util.PermissionManager

class BluetoothConnectReceiver : BroadcastReceiver() {
    override fun onReceive(context: Context, intent: Intent) {
        if (intent.action == BluetoothDevice.ACTION_ACL_CONNECTED) {
            val device: BluetoothDevice? = if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE, BluetoothDevice::class.java)
            } else {
                @Suppress("DEPRECATION")
                intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE)
            }
            val prefs = context.getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE)
            val autoConnect = prefs.getBoolean("auto_connect", true)
            val savedMacAddress = prefs.getString("saved_mac_address", null)

            if (autoConnect && savedMacAddress != null && device != null && device.address == savedMacAddress) {
                if (PermissionManager.hasRequiredPermissions(context)) {
                    val serviceIntent = Intent(context, BudsService::class.java).apply {
                        action = "com.benegedeniz.budsdynamiceq.AUTO_CONNECT"
                    }
                    startServiceSafely(context, serviceIntent, "ACL_CONNECTED")
                }
            }
        }
    }
}

/**
 * Starts [BudsService] from a broadcast. On Android 12+ a background foreground-service start
 * can throw ForegroundServiceStartNotAllowedException (e.g. when the app is battery-restricted);
 * catching it keeps the receiver from crashing the process.
 */
internal fun startServiceSafely(context: Context, intent: Intent, source: String) {
    try {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            context.startForegroundService(intent)
        } else {
            context.startService(intent)
        }
    } catch (e: IllegalStateException) {
        // ForegroundServiceStartNotAllowedException extends IllegalStateException.
        android.util.Log.w("BudsService", "Could not start service from $source: ${e.message}")
    } catch (e: SecurityException) {
        android.util.Log.w("BudsService", "Missing permission to start service from $source: ${e.message}")
    }
}
