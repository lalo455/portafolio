package com.benegedeniz.budsdynamiceq.rules

import com.benegedeniz.budsdynamiceq.rules.GenreEqMapper.Category
import org.junit.Assert.assertEquals
import org.junit.Test

class GenreEqMapperTest {

    @Test
    fun classify_commonItunesGenres() {
        assertEquals(Category.ROCK, GenreEqMapper.classify("Hard Rock"))
        assertEquals(Category.ROCK, GenreEqMapper.classify("Alternative"))
        assertEquals(Category.URBAN, GenreEqMapper.classify("Hip-Hop/Rap"))
        assertEquals(Category.URBAN, GenreEqMapper.classify("R&B/Soul"))
        assertEquals(Category.ELECTRONIC, GenreEqMapper.classify("Dance"))
        assertEquals(Category.CLASSICAL_JAZZ, GenreEqMapper.classify("Soundtrack"))
        assertEquals(Category.ACOUSTIC, GenreEqMapper.classify("Singer/Songwriter"))
        assertEquals(Category.SPOKEN, GenreEqMapper.classify("Podcasts"))
        assertEquals(Category.LATIN_REGIONAL, GenreEqMapper.classify("Regional Mexican"))
    }

    @Test
    fun classify_specificBeforeGeneric() {
        // "Urbano latino" must be urban, not generic latin; "Pop Latino" must be pop.
        assertEquals(Category.URBAN, GenreEqMapper.classify("Urbano latino"))
        assertEquals(Category.POP, GenreEqMapper.classify("Pop Latino"))
        assertEquals(Category.ELECTRONIC, GenreEqMapper.classify("Tropical House"))
    }

    @Test
    fun classify_unknownOrEmpty() {
        assertEquals(Category.OTHER, GenreEqMapper.classify(null))
        assertEquals(Category.OTHER, GenreEqMapper.classify(""))
        assertEquals(Category.OTHER, GenreEqMapper.classify("Gospel Choir Specials"))
    }
}
