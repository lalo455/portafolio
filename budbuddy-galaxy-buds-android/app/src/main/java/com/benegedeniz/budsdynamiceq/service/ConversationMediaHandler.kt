package com.benegedeniz.budsdynamiceq.service

import android.content.Context
import android.media.AudioManager
import android.util.Log
import com.benegedeniz.budsdynamiceq.gesture.GestureActionExecutor
import com.benegedeniz.budsdynamiceq.media.MediaObserver
import kotlin.math.max
import kotlin.math.roundToInt

/**
 * What to do with media when the buds switch to Transparency because you start talking
 * (the buds' "Voice detect"/conversation mode) — and how to undo it when they switch back.
 *
 *  - PAUSE: pause playback, resume afterwards (unless you paused/played manually meanwhile).
 *  - DUCK:  lower the media volume, restore it afterwards (unless you changed it meanwhile).
 */
class ConversationMediaHandler(
    context: Context,
    private val actionExecutor: GestureActionExecutor,
    private val mediaObserver: MediaObserver
) {
    companion object {
        private const val TAG = "ConversationMedia"
        const val PREF_ACTION = "conversation_media_action"
        const val ACTION_PAUSE = "pause"
        const val ACTION_DUCK = "duck"
        /** Ducked volume as a fraction of the volume before the conversation. */
        private const val DUCK_FRACTION = 0.25
    }

    private val prefs = context.getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE)
    private val audioManager = context.getSystemService(AudioManager::class.java)

    private var pausedByUs = false
    private var savedVolume: Int? = null
    private var duckedVolume: Int? = null

    val isActive: Boolean get() = pausedByUs || savedVolume != null

    fun onConversationStart() {
        if (isActive) return
        when (prefs.getString(PREF_ACTION, ACTION_PAUSE)) {
            ACTION_DUCK -> {
                val current = audioManager.getStreamVolume(AudioManager.STREAM_MUSIC)
                if (current > 1) {
                    val target = max(1, (current * DUCK_FRACTION).roundToInt())
                    audioManager.setStreamVolume(AudioManager.STREAM_MUSIC, target, 0)
                    savedVolume = current
                    duckedVolume = target
                    Log.i(TAG, "Conversation: ducked volume $current → $target")
                }
            }
            else -> {
                if (mediaObserver.isCurrentlyPlaying()) {
                    actionExecutor.triggerPause()
                    pausedByUs = true
                    Log.i(TAG, "Conversation: paused media")
                }
            }
        }
    }

    fun onConversationEnd() {
        val saved = savedVolume
        if (saved != null) {
            // Don't fight the user: restore only if they didn't touch the volume meanwhile.
            if (audioManager.getStreamVolume(AudioManager.STREAM_MUSIC) == duckedVolume) {
                audioManager.setStreamVolume(AudioManager.STREAM_MUSIC, saved, 0)
                Log.i(TAG, "Conversation ended: restored volume to $saved")
            }
        }
        if (pausedByUs) {
            // respectManualIntent: skip if the user paused manually during the conversation.
            actionExecutor.triggerPlay(respectManualIntent = true)
            Log.i(TAG, "Conversation ended: resumed media")
        }
        clear()
    }

    /** Link lost mid-conversation: put the volume back but don't auto-resume playback. */
    fun onDisconnected() {
        savedVolume?.let { audioManager.setStreamVolume(AudioManager.STREAM_MUSIC, it, 0) }
        clear()
    }

    private fun clear() {
        pausedByUs = false
        savedVolume = null
        duckedVolume = null
    }
}
