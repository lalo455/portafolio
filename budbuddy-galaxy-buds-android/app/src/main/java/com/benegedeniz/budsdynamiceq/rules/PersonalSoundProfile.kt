package com.benegedeniz.budsdynamiceq.rules

import android.content.SharedPreferences
import com.benegedeniz.budsdynamiceq.rules.GenreEqMapper.Category
import kotlin.math.roundToInt

/**
 * "Curves" mode of Auto EQ: builds a 9-band custom EQ (63 Hz … 16 kHz) per song as
 *
 *     personal preference (from the "My sound" A/B test) + gentle genre tilt (+ tempo nudge)
 *
 * Rationale (Harman research, Olive & Welti): most listeners prefer roughly the same neutral
 * target regardless of genre, and what differs most between people is how much bass and
 * treble they like. So the personal layer carries the weight and genre tilts stay small
 * (±1–4 dB). No model calibration layer is applied: published Buds4 Pro measurements
 * (RTINGS, SoundGuys, Notebookcheck) agree it's already near-neutral/warm and disagree
 * with each other by about as much as any correction would change.
 */
object PersonalSoundProfile {

    const val PREF_MODE = "auto_eq_mode"
    const val MODE_CURVES = "curves"
    const val MODE_PRESETS = "presets"

    const val PREF_BASS = "sound_profile_bass"
    const val PREF_TREBLE = "sound_profile_treble"
    const val PREF_PRESENCE = "sound_profile_presence"
    const val PREF_DONE = "sound_profile_done"

    /** Band centres: 63, 125, 250, 500, 1k, 2k, 4k, 8k, 16k Hz. */
    private const val BANDS = 9
    /** Keep total boost moderate so the buds' DSP has headroom (Samsung's own Bass boost is ~+6 dB). */
    private const val MAX_GAIN = 8
    private const val MIN_GAIN = -8

    data class Profile(val bass: Int, val treble: Int, val presence: Int) {
        companion object { val NEUTRAL = Profile(0, 0, 0) }
    }

    fun load(prefs: SharedPreferences) = Profile(
        prefs.getInt(PREF_BASS, 0),
        prefs.getInt(PREF_TREBLE, 0),
        prefs.getInt(PREF_PRESENCE, 0)
    )

    fun save(prefs: SharedPreferences, p: Profile) {
        prefs.edit()
            .putInt(PREF_BASS, p.bass)
            .putInt(PREF_TREBLE, p.treble)
            .putInt(PREF_PRESENCE, p.presence)
            .putBoolean(PREF_DONE, true)
            .apply()
    }

    fun usesCurves(prefs: SharedPreferences, modelSupportsCustom: Boolean): Boolean =
        modelSupportsCustom && prefs.getString(PREF_MODE, MODE_CURVES) == MODE_CURVES

    /** Personal layer: low shelf (bass), high shelf (treble) and a presence bell around 2 kHz. */
    fun personalBands(p: Profile): DoubleArray {
        val bassShape = doubleArrayOf(1.0, 0.85, 0.45, 0.1, 0.0, 0.0, 0.0, 0.0, 0.0)
        val trebleShape = doubleArrayOf(0.0, 0.0, 0.0, 0.0, 0.0, 0.15, 0.55, 0.9, 1.0)
        val presenceShape = doubleArrayOf(0.0, 0.0, 0.0, 0.1, 0.45, 1.0, 0.6, 0.1, 0.0)
        return DoubleArray(BANDS) { i ->
            p.bass * bassShape[i] + p.treble * trebleShape[i] + p.presence * presenceShape[i]
        }
    }

    /** Gentle genre tilts on top of the personal curve (dB per band). */
    fun genreTilt(category: Category): IntArray = when (category) {
        Category.SPOKEN -> intArrayOf(-3, -2, -1, 0, 1, 2, 2, 0, -1)        // less rumble, clearer voice
        Category.URBAN -> intArrayOf(4, 3, 1, 0, 0, 0, 0, 1, 0)             // sub/kick weight
        Category.ELECTRONIC -> intArrayOf(4, 3, 0, -1, 0, 0, 1, 2, 1)        // sub + air
        Category.ROCK -> intArrayOf(2, 1, 0, -1, 0, 1, 2, 1, 0)              // mild V: punch + bite
        Category.CLASSICAL_JAZZ -> intArrayOf(0, 0, 0, 0, 0, 0, 0, 1, 1)     // keep it neutral
        Category.ACOUSTIC -> intArrayOf(0, 1, 1, 0, 0, 1, 1, 0, 0)           // body + detail
        Category.POP -> intArrayOf(1, 1, 0, 0, 0, 1, 1, 1, 0)
        Category.LATIN_REGIONAL -> intArrayOf(2, 2, 0, 0, 0, 1, 1, 0, 0)     // bass lines, brass/accordion presence
        Category.OTHER -> IntArray(BANDS)
    }

    /** Small tempo nudge for categories where tempo says something (pop, latin, unknown). */
    fun tempoNudge(category: Category, bpm: Int?): IntArray {
        if (bpm == null || !category.tempoSensitive) return IntArray(BANDS)
        return when {
            bpm >= GenreEqMapper.FAST_BPM -> intArrayOf(1, 1, 0, 0, 0, 0, 1, 1, 0)
            bpm <= GenreEqMapper.SLOW_BPM -> intArrayOf(0, 0, 1, 0, 0, 0, 0, -1, -1)
            else -> IntArray(BANDS)
        }
    }

    fun bandsFor(profile: Profile, category: Category, bpm: Int?): List<Int> {
        val personal = personalBands(profile)
        val tilt = genreTilt(category)
        val nudge = tempoNudge(category, bpm)
        return List(BANDS) { i ->
            (personal[i] + tilt[i] + nudge[i]).roundToInt().coerceIn(MIN_GAIN, MAX_GAIN)
        }
    }

    /** Curve used while no genre/tempo is known: just the personal preference. */
    fun baseBands(profile: Profile): List<Int> = bandsFor(profile, Category.OTHER, null)
}
