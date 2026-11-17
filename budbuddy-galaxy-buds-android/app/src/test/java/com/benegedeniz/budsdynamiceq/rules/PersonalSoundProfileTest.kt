package com.benegedeniz.budsdynamiceq.rules

import com.benegedeniz.budsdynamiceq.rules.GenreEqMapper.Category
import com.benegedeniz.budsdynamiceq.rules.PersonalSoundProfile.Profile
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class PersonalSoundProfileTest {

    @Test
    fun neutralProfile_unknownGenre_isFlat() {
        assertEquals(List(9) { 0 }, PersonalSoundProfile.baseBands(Profile.NEUTRAL))
    }

    @Test
    fun bassPreference_liftsLowBandsOnly() {
        val bands = PersonalSoundProfile.baseBands(Profile(bass = 4, treble = 0, presence = 0))
        assertEquals(4, bands[0])          // 63 Hz full shelf
        assertTrue(bands[1] in 3..4)       // 125 Hz
        assertTrue(bands.drop(4).all { it == 0 }) // 1 kHz and up untouched
    }

    @Test
    fun trebleAndPresence_shapeUpperBands() {
        val bands = PersonalSoundProfile.baseBands(Profile(bass = 0, treble = 3, presence = 2))
        assertEquals(0, bands[0])
        assertEquals(3, bands[8])          // 16 kHz full treble shelf
        assertTrue(bands[5] >= 2)          // 2 kHz presence peak
    }

    @Test
    fun genreTilts_areGentle() {
        Category.entries.forEach { cat ->
            assertTrue("$cat tilt too strong", PersonalSoundProfile.genreTilt(cat).all { it in -4..4 })
        }
    }

    @Test
    fun totals_areClampedForHeadroom() {
        val extreme = Profile(bass = 8, treble = 6, presence = 4)
        val bands = PersonalSoundProfile.bandsFor(extreme, Category.ELECTRONIC, 140)
        assertTrue(bands.all { it in -8..8 })
    }
}
