package com.benegedeniz.budsdynamiceq.service

import android.content.Context
import android.content.SharedPreferences
import android.net.ConnectivityManager
import android.net.Network
import android.net.NetworkCapabilities
import android.util.Log
import com.benegedeniz.budsdynamiceq.data.model.NoiseControlMode
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

/**
 * Picks a noise-control mode from the phone's network:
 *  - Wi-Fi  → you're at home / a safe place (default: noise cancelling)
 *  - Mobile data → you're out and about (default: transparency, to hear traffic)
 *
 * Uses only network type, so no location permission is needed (reading the Wi-Fi name would).
 * The result feeds RulesCoordinator, where explicit music rules still take priority.
 */
class NetworkNcMonitor(private val context: Context) {

    companion object {
        private const val TAG = "NetworkNcMonitor"
        const val PREF_ENABLED = "network_nc_enabled"
        const val PREF_WIFI_MODE = "network_nc_wifi"
        const val PREF_MOBILE_MODE = "network_nc_mobile"
        val DEFAULT_WIFI_MODE = NoiseControlMode.NOISE_CANCELLATION
        val DEFAULT_MOBILE_MODE = NoiseControlMode.TRANSPARENT

        fun modeFor(prefs: SharedPreferences, key: String, default: NoiseControlMode): NoiseControlMode =
            prefs.getString(key, null)?.let { runCatching { NoiseControlMode.valueOf(it) }.getOrNull() } ?: default
    }

    enum class NetworkKind { WIFI, MOBILE, NONE }

    private val prefs = context.getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE)
    private val connectivity = context.getSystemService(ConnectivityManager::class.java)

    @Volatile private var currentKind = NetworkKind.NONE

    private val _ncOverride = MutableStateFlow<NoiseControlMode?>(null)
    /** Mode to use instead of the manual default, or null when the feature is off / offline. */
    val ncOverride: StateFlow<NoiseControlMode?> = _ncOverride.asStateFlow()

    private val callback = object : ConnectivityManager.NetworkCallback() {
        override fun onCapabilitiesChanged(network: Network, caps: NetworkCapabilities) {
            currentKind = when {
                caps.hasTransport(NetworkCapabilities.TRANSPORT_WIFI) ||
                    caps.hasTransport(NetworkCapabilities.TRANSPORT_ETHERNET) -> NetworkKind.WIFI
                caps.hasTransport(NetworkCapabilities.TRANSPORT_CELLULAR) -> NetworkKind.MOBILE
                else -> currentKind
            }
            recompute()
        }

        override fun onLost(network: Network) {
            currentKind = NetworkKind.NONE
            recompute()
        }
    }

    private val prefsListener = SharedPreferences.OnSharedPreferenceChangeListener { _, key ->
        if (key == PREF_ENABLED || key == PREF_WIFI_MODE || key == PREF_MOBILE_MODE) recompute()
    }

    fun start() {
        prefs.registerOnSharedPreferenceChangeListener(prefsListener)
        try {
            connectivity?.registerDefaultNetworkCallback(callback)
        } catch (e: Exception) {
            Log.e(TAG, "Could not observe network", e)
        }
        recompute()
    }

    fun stop() {
        prefs.unregisterOnSharedPreferenceChangeListener(prefsListener)
        try {
            connectivity?.unregisterNetworkCallback(callback)
        } catch (_: Exception) { }
        _ncOverride.value = null
    }

    private fun recompute() {
        val mode = if (!prefs.getBoolean(PREF_ENABLED, false)) null else when (currentKind) {
            NetworkKind.WIFI -> modeFor(prefs, PREF_WIFI_MODE, DEFAULT_WIFI_MODE)
            NetworkKind.MOBILE -> modeFor(prefs, PREF_MOBILE_MODE, DEFAULT_MOBILE_MODE)
            // Briefly offline while switching Wi-Fi ↔ data: keep the last mode instead of flapping.
            NetworkKind.NONE -> _ncOverride.value
        }
        if (_ncOverride.value != mode) {
            Log.i(TAG, "Network $currentKind → noise control override: $mode")
            _ncOverride.value = mode
        }
    }
}
