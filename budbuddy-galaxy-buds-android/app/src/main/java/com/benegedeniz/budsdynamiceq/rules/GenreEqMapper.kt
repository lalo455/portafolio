package com.benegedeniz.budsdynamiceq.rules

import android.content.SharedPreferences
import androidx.annotation.StringRes
import com.benegedeniz.budsdynamiceq.R
import com.benegedeniz.budsdynamiceq.data.model.EqPreset

/**
 * Automatic EQ: maps the current song's genre (from media metadata or the iTunes lookup)
 * and, optionally, its tempo (BPM from Deezer) to an equalizer preset.
 *
 * Works with any player (YouTube Music, Spotify, Amazon...) because it only needs the
 * title/artist that the player publishes in its media session.
 */
object GenreEqMapper {

    const val PREF_ENABLED = "auto_genre_eq_enabled"
    const val PREF_USE_BPM = "auto_genre_eq_use_bpm"
    private const val PREF_MAP_PREFIX = "auto_genre_eq_map_"

    /** Above this tempo, "neutral" categories get a more energetic preset. */
    const val FAST_BPM = 118
    /** Below this tempo, "neutral" categories get a softer preset. */
    const val SLOW_BPM = 85

    /**
     * Matching is done in declaration order, so more specific categories come first
     * (e.g. "Urbano latino" must hit URBAN before LATIN, "Pop Latino" must hit POP).
     */
    enum class Category(
        @param:StringRes val labelRes: Int,
        val defaultPreset: EqPreset,
        val keywords: List<String>,
        /** Whether tempo may override the preset for this category. */
        val tempoSensitive: Boolean = false
    ) {
        SPOKEN(R.string.genre_cat_spoken, EqPreset.TREBLE_BOOST,
            listOf("podcast", "spoken", "audiobook", "talk", "comedy", "news", "audiolibro", "hablado")),
        URBAN(R.string.genre_cat_urban, EqPreset.BASS_BOOST,
            listOf("hip-hop", "hip hop", "rap", "trap", "reggaeton", "reggaetón", "urban", "r&b", "soul", "funk", "dancehall", "drill")),
        ELECTRONIC(R.string.genre_cat_electronic, EqPreset.BASS_BOOST,
            listOf("electronic", "electrónica", "electronica", "dance", "edm", "house", "techno", "trance", "dubstep", "drum", "bass")),
        ROCK(R.string.genre_cat_rock, EqPreset.DYNAMIC,
            listOf("rock", "metal", "punk", "grunge", "alternative", "alternativ", "hardcore", "emo")),
        CLASSICAL_JAZZ(R.string.genre_cat_classical_jazz, EqPreset.CLEAR,
            listOf("classical", "clásica", "clasica", "jazz", "blues", "soundtrack", "score", "instrumental", "new age", "ambient", "opera", "ópera", "piano", "orchestral")),
        ACOUSTIC(R.string.genre_cat_acoustic, EqPreset.SOFT,
            listOf("singer/songwriter", "singer-songwriter", "folk", "acoustic", "acústic", "country", "bossa")),
        POP(R.string.genre_cat_pop, EqPreset.NORMAL,
            listOf("pop", "k-pop", "j-pop"), tempoSensitive = true),
        LATIN_REGIONAL(R.string.genre_cat_latin, EqPreset.NORMAL,
            listOf("latin", "latino", "regional mexican", "mexicana", "mexican", "salsa", "cumbia", "banda", "ranchera", "bachata", "tropical", "merengue", "norteño", "norteno", "corrido", "mariachi", "bolero"),
            tempoSensitive = true),
        /** Genre unknown or unrecognised: only tempo can drive the choice. */
        OTHER(R.string.genre_cat_other, EqPreset.NORMAL, emptyList(), tempoSensitive = true);
    }

    fun classify(genre: String?): Category {
        val g = genre?.trim()?.lowercase() ?: return Category.OTHER
        if (g.isEmpty()) return Category.OTHER
        return Category.entries.firstOrNull { cat -> cat.keywords.any { g.contains(it) } } ?: Category.OTHER
    }

    fun presetFor(prefs: SharedPreferences, category: Category): EqPreset {
        val stored = prefs.getString(PREF_MAP_PREFIX + category.name, null)
        return stored?.let { runCatching { EqPreset.valueOf(it) }.getOrNull() } ?: category.defaultPreset
    }

    fun setPresetFor(prefs: SharedPreferences, category: Category, preset: EqPreset) {
        prefs.edit().putString(PREF_MAP_PREFIX + category.name, preset.name).apply()
    }

    data class Decision(val category: Category, val preset: EqPreset, val bpm: Int?)

    /**
     * Returns the preset to apply, or null when there is nothing to go on
     * (no genre and no usable tempo) so the caller falls back to the manual preset.
     */
    fun decide(prefs: SharedPreferences, genre: String?, bpm: Int?): Decision? {
        val category = classify(genre)
        val useBpm = prefs.getBoolean(PREF_USE_BPM, true)
        val tempo = bpm?.takeIf { useBpm && it > 0 }

        if (category == Category.OTHER && tempo == null) return null

        var preset = presetFor(prefs, category)
        // Tempo only nudges categories whose preset the user left at the default,
        // so an explicit user mapping is always respected.
        val userCustomised = prefs.contains(PREF_MAP_PREFIX + category.name)
        if (tempo != null && category.tempoSensitive && !userCustomised) {
            preset = when {
                tempo >= FAST_BPM -> EqPreset.DYNAMIC
                tempo <= SLOW_BPM -> EqPreset.SOFT
                else -> preset
            }
        }
        return Decision(category, preset, tempo)
    }
}
