package com.benegedeniz.budsdynamiceq.service

import android.content.Context
import com.benegedeniz.budsdynamiceq.bluetooth.BudsController
import com.benegedeniz.budsdynamiceq.data.RulesRepository
import com.benegedeniz.budsdynamiceq.data.model.EqPreset
import com.benegedeniz.budsdynamiceq.data.model.NoiseControlMode
import com.benegedeniz.budsdynamiceq.media.MediaObserver
import com.benegedeniz.budsdynamiceq.rules.RulesEngine
import com.benegedeniz.budsdynamiceq.rules.GenreEqMapper
import com.benegedeniz.budsdynamiceq.rules.PersonalSoundProfile
import com.benegedeniz.budsdynamiceq.data.model.EqRule
import com.benegedeniz.budsdynamiceq.media.GenreFetchState
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.flow.debounce
import kotlinx.coroutines.launch

@OptIn(kotlinx.coroutines.FlowPreview::class)
class RulesCoordinator(
    private val context: Context,
    private val scope: CoroutineScope,
    private val budsController: BudsController,
    private val mediaObserver: MediaObserver,
    private val rulesRepository: RulesRepository,
    /** Noise-control mode chosen from Wi-Fi vs mobile data, or null when that feature is off. */
    private val networkNcOverride: kotlinx.coroutines.flow.Flow<NoiseControlMode?> = kotlinx.coroutines.flow.flowOf(null)
) {
    private val rulesEngine = RulesEngine()

    companion object {
        /** Id of the synthetic rule shown in the UI while Auto EQ (genre/BPM) is in control. */
        const val AUTO_GENRE_RULE_ID = "auto_genre_eq"
    }

    fun start() {
        scope.launch {
            var lastPushedEq: EqPreset? = null
            var lastPushedNc: NoiseControlMode? = null
            var lastPushedBands: List<Int>? = null
            var lastAutoVersion = -1
            var wasConnected = false
            var wasBothInEar = false

            val ruleStateFlow = combine(
                mediaObserver.currentMetadata.debounce(400L),
                rulesRepository.rules,
                budsController.manualPreset,
                // Network-based mode replaces the manual default (rules still win below).
                // Debounced so a quick Wi-Fi ↔ data handover doesn't toggle ANC twice.
                combine(budsController.manualNoiseControl, networkNcOverride.debounce(1500L)) { manual, network ->
                    network ?: manual
                }
            ) { metadata, rulesList, manualEq, manualNc ->
                object {
                    val metadata = metadata
                    val rulesList = rulesList
                    val manualEq = manualEq
                    val manualNc = manualNc
                }
            }
            
            val ruleEvalFlow = combine(
                budsController.isConnected,
                budsController.placementL,
                budsController.placementR,
                ruleStateFlow,
                com.benegedeniz.budsdynamiceq.di.ServiceLocator.autoEqSettingsVersion
            ) { connected, pL, pR, ruleState, autoVersion ->
                object {
                    val connected = connected
                    val pL = pL
                    val pR = pR
                    val ruleState = ruleState
                    val autoVersion = autoVersion
                }
            }

            ruleEvalFlow.collect { state ->
                if (!state.connected) {
                    wasConnected = false
                    return@collect
                }
                
                val justConnected = !wasConnected
                wasConnected = true

                val isLWorn = state.pL == com.benegedeniz.budsdynamiceq.data.model.PlacementState.WEARING
                val isRWorn = state.pR == com.benegedeniz.budsdynamiceq.data.model.PlacementState.WEARING
                val bothInEar = isLWorn && isRWorn
                val justPutBothInEar = bothInEar && !wasBothInEar
                wasBothInEar = bothInEar

                val metadata = state.ruleState.metadata
                val manualEq = state.ruleState.manualEq
                val manualNc = state.ruleState.manualNc

                val explicitRule = rulesEngine.evaluate(metadata, state.ruleState.rulesList)

                // Auto EQ by genre/BPM applies only when no user rule matched.
                val prefs = context.getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE)
                val autoEqEnabled = prefs.getBoolean(GenreEqMapper.PREF_ENABLED, false)
                if (autoEqEnabled && explicitRule == null && !justConnected && !justPutBothInEar &&
                    metadata?.genreFetchState == GenreFetchState.LOADING
                ) {
                    // Wait for the genre lookup instead of flipping to the manual preset and back.
                    return@collect
                }
                val localized = com.benegedeniz.budsdynamiceq.util.LanguageUtils.setLocale(context)
                fun autoLabel(name: String, bpm: Int?) = if (bpm != null) {
                    localized.getString(com.benegedeniz.budsdynamiceq.R.string.auto_eq_rule_label_bpm, name, bpm)
                } else {
                    localized.getString(com.benegedeniz.budsdynamiceq.R.string.auto_eq_rule_label, name)
                }

                // "Curves" mode: personal profile + gentle genre tilt as a 9-band custom EQ.
                // It also applies with no song/genre (just the personal curve).
                val useCurves = autoEqEnabled && explicitRule == null &&
                    PersonalSoundProfile.usesCurves(prefs, budsController.effectiveModel.value.supportsCustomEqualizer)
                val decision = if (autoEqEnabled && explicitRule == null && metadata != null) {
                    GenreEqMapper.decide(prefs, metadata.genre, metadata.bpm)
                } else null

                var autoBands: List<Int>? = null
                val autoRule = if (useCurves) {
                    val profile = PersonalSoundProfile.load(prefs)
                    autoBands = if (decision != null) {
                        PersonalSoundProfile.bandsFor(profile, decision.category, decision.bpm)
                    } else {
                        PersonalSoundProfile.baseBands(profile)
                    }
                    EqRule(
                        id = AUTO_GENRE_RULE_ID,
                        keyword = if (decision != null) {
                            autoLabel(localized.getString(decision.category.labelRes), decision.bpm)
                        } else {
                            autoLabel(localized.getString(com.benegedeniz.budsdynamiceq.R.string.sound_profile_name), null)
                        },
                        preset = EqPreset.DEFAULT,
                        priority = -1
                    )
                } else decision?.let {
                    EqRule(
                        id = AUTO_GENRE_RULE_ID,
                        keyword = autoLabel(localized.getString(it.category.labelRes), it.bpm),
                        preset = it.preset,
                        priority = -1
                    )
                }
                val matchingRule = explicitRule ?: autoRule

                val eqToSend = when {
                    autoBands != null -> null
                    matchingRule != null && matchingRule.preset != EqPreset.DEFAULT -> matchingRule.preset
                    else -> manualEq
                }
                val ncToSend = if (matchingRule != null && matchingRule.noiseControl != NoiseControlMode.DEFAULT) matchingRule.noiseControl else manualNc

                val versionChanged = state.autoVersion != lastAutoVersion
                lastAutoVersion = state.autoVersion
                val settingsChanged = eqToSend != lastPushedEq || ncToSend != lastPushedNc ||
                    autoBands != lastPushedBands || versionChanged

                if (settingsChanged || justConnected || justPutBothInEar) {
                    budsController.setLastMatchedRule(matchingRule)
                    if (autoBands != null) {
                        budsController.sendAutoCustomBands(autoBands)
                    } else if (eqToSend != null) {
                        budsController.sendEqualizer(eqToSend)
                    }
                    if (ncToSend != null) budsController.sendNoiseControl(ncToSend)
                    
                    if (settingsChanged && !justConnected && !justPutBothInEar) {
                        if (prefs.getBoolean("rule_toast_enabled", true)) {
                            android.os.Handler(android.os.Looper.getMainLooper()).post {
                                val localizedContext = com.benegedeniz.budsdynamiceq.util.LanguageUtils.setLocale(context)
                                val eqName = if (autoBands != null) matchingRule?.keyword ?: "" else eqToSend?.let { localizedContext.getString(it.displayNameRes) } ?: ""
                                val ncName = ncToSend?.let { localizedContext.getString(it.displayNameRes) } ?: ""
                                val toastMessage = listOf(eqName, ncName).filter { it.isNotEmpty() }.joinToString(" & ")
                                
                                val triggerName = matchingRule?.keyword ?: localizedContext.getString(com.benegedeniz.budsdynamiceq.R.string.global_defaults)
                                val finalMessage = localizedContext.getString(
                                    com.benegedeniz.budsdynamiceq.R.string.rule_applied_toast,
                                    toastMessage.ifEmpty { triggerName }
                                )
                                android.widget.Toast.makeText(localizedContext, finalMessage, android.widget.Toast.LENGTH_SHORT).show()
                            }
                        }
                    }
                    
                    lastPushedEq = eqToSend
                    lastPushedNc = ncToSend
                    lastPushedBands = autoBands
                } else if (budsController.lastMatchedRule.value != matchingRule) {
                    // Update UI state even if settings didn't change, so Equalizer card disables correctly
                    budsController.setLastMatchedRule(matchingRule)
                }
            }
        }
    }
}
