package com.benegedeniz.budsdynamiceq.companion

import android.companion.AssociationInfo
import android.companion.CompanionDeviceService
import android.companion.DevicePresenceEvent
import android.content.Context
import android.content.Intent
import android.os.Build
import android.util.Log
import androidx.annotation.RequiresApi
import com.benegedeniz.budsdynamiceq.service.BudsService
import com.benegedeniz.budsdynamiceq.service.startServiceSafely
import com.benegedeniz.budsdynamiceq.util.PermissionManager

/**
 * Bound by the system when the associated buds connect (see [CompanionHelper.startObservingPresence]).
 * Being a companion app, we're allowed to start the foreground service from here even when
 * the app is in the background, which is more reliable than the ACL_CONNECTED broadcast.
 */
@RequiresApi(Build.VERSION_CODES.S)
class BudsCompanionService : CompanionDeviceService() {

    @Deprecated("Used on Android 12")
    override fun onDeviceAppeared(address: String) {
        onBudsAppeared(address)
    }

    @Deprecated("Used on Android 13-15")
    @RequiresApi(Build.VERSION_CODES.TIRAMISU)
    override fun onDeviceAppeared(associationInfo: AssociationInfo) {
        onBudsAppeared(associationInfo.deviceMacAddress?.toString())
    }

    @RequiresApi(Build.VERSION_CODES.BAKLAVA)
    override fun onDevicePresenceEvent(event: DevicePresenceEvent) {
        when (event.event) {
            DevicePresenceEvent.EVENT_BT_CONNECTED,
            DevicePresenceEvent.EVENT_BLE_APPEARED -> onBudsAppeared(null)
        }
    }

    private fun onBudsAppeared(address: String?) {
        val prefs = getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE)
        val savedMac = prefs.getString("saved_mac_address", null) ?: return
        if (address != null && !address.equals(savedMac, ignoreCase = true)) return
        if (!prefs.getBoolean("auto_connect", true)) return
        if (!PermissionManager.hasRequiredPermissions(this)) return

        Log.i("BudsCompanionService", "Associated buds appeared, starting service")
        val intent = Intent(this, BudsService::class.java).apply {
            action = "com.benegedeniz.budsdynamiceq.AUTO_CONNECT"
        }
        startServiceSafely(this, intent, "CompanionDeviceService")
    }
}
