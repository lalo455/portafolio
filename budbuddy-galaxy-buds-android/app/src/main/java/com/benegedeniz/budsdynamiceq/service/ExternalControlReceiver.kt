package com.benegedeniz.budsdynamiceq.service

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.util.Log
import com.benegedeniz.budsdynamiceq.data.model.EqPreset
import com.benegedeniz.budsdynamiceq.data.model.NoiseControlMode
import com.benegedeniz.budsdynamiceq.di.ServiceLocator
import com.benegedeniz.budsdynamiceq.rules.GenreEqMapper

/**
 * Lets automation apps (Tasker, MacroDroid, Automate...) control the buds with broadcast intents.
 * Disabled unless the user turns on "Allow external control" in Settings.
 *
 * Package: com.benegedeniz.budsdynamiceq
 *  - ...action.SET_NOISE_CONTROL   extra "mode"    = OFF | NOISE_CANCELLATION | TRANSPARENT | ADAPTIVE
 *  - ...action.TOGGLE_NOISE_CONTROL
 *  - ...action.SET_EQ              extra "preset"  = NORMAL | BASS_BOOST | SOFT | DYNAMIC | CLEAR | TREBLE_BOOST | CUSTOM_1..3
 *  - ...action.SET_AUTO_EQ         extra "enabled" = true | false
 *  - ...action.CONNECT
 */
class ExternalControlReceiver : BroadcastReceiver() {

    companion object {
        const val PREF_ENABLED = "external_control_enabled"
        private const val PREFIX = "com.benegedeniz.budsdynamiceq.action."
        const val ACTION_SET_NC = PREFIX + "SET_NOISE_CONTROL"
        const val ACTION_TOGGLE_NC = PREFIX + "TOGGLE_NOISE_CONTROL"
        const val ACTION_SET_EQ = PREFIX + "SET_EQ"
        const val ACTION_SET_AUTO_EQ = PREFIX + "SET_AUTO_EQ"
        const val ACTION_CONNECT = PREFIX + "CONNECT"
        private const val TAG = "ExternalControl"
    }

    override fun onReceive(context: Context, intent: Intent) {
        val prefs = context.getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE)
        if (!prefs.getBoolean(PREF_ENABLED, false)) {
            Log.w(TAG, "Ignoring ${intent.action}: external control is disabled in settings")
            return
        }
        val controller = ServiceLocator.provideBudsController(context)

        when (intent.action) {
            ACTION_CONNECT -> {
                val serviceIntent = Intent(context, BudsService::class.java).apply {
                    action = "com.benegedeniz.budsdynamiceq.AUTO_CONNECT"
                }
                startServiceSafely(context, serviceIntent, "ExternalControl")
            }
            ACTION_SET_AUTO_EQ -> {
                prefs.edit().putBoolean(GenreEqMapper.PREF_ENABLED, intent.getBooleanExtra("enabled", true)).apply()
            }
            ACTION_TOGGLE_NC -> if (controller.isConnected.value) controller.toggleNoiseControl()
            ACTION_SET_NC -> {
                val mode = parseEnum<NoiseControlMode>(intent.getStringExtra("mode"))
                    ?.takeIf { it != NoiseControlMode.DEFAULT && it != NoiseControlMode.IGNORE }
                if (mode == null) {
                    Log.w(TAG, "Invalid or missing 'mode' extra")
                } else if (controller.isConnected.value) {
                    controller.sendNoiseControl(mode)
                }
            }
            ACTION_SET_EQ -> {
                val preset = parseEnum<EqPreset>(intent.getStringExtra("preset"))
                    ?.takeIf { it != EqPreset.DEFAULT && it != EqPreset.IGNORE }
                if (preset == null) {
                    Log.w(TAG, "Invalid or missing 'preset' extra")
                } else {
                    // Same path as choosing the default EQ in the app, so rules keep precedence.
                    prefs.edit().putString("default_preset", preset.name).apply()
                    controller.setManualPreset(preset)
                }
            }
        }
    }

    private inline fun <reified T : Enum<T>> parseEnum(value: String?): T? =
        value?.trim()?.uppercase()?.let { v -> enumValues<T>().firstOrNull { it.name == v } }
}
