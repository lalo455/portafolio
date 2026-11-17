package com.benegedeniz.budsdynamiceq.ui.settings

import android.content.Context
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.benegedeniz.budsdynamiceq.R
import com.benegedeniz.budsdynamiceq.di.ServiceLocator
import com.benegedeniz.budsdynamiceq.rules.PersonalSoundProfile
import com.benegedeniz.budsdynamiceq.rules.PersonalSoundProfile.Profile

/**
 * "My sound": a short adaptive A/B test played over the user's own music.
 *
 * For each dimension (bass, treble/brightness, vocal presence) we run 3 forced-choice rounds:
 * B = A ± step, steps shrink (e.g. 4 → 2 → 1 dB). Choosing B moves there and keeps the
 * direction; choosing A (or "no difference") flips the direction for the next round.
 * Forced-choice comparisons are how listener preference is measured in headphone research
 * and avoid the bias of dragging sliders while looking at numbers.
 */
private enum class Dimension(val titleRes: Int, val hintRes: Int, val steps: IntArray, val min: Int, val max: Int) {
    BASS(R.string.sound_profile_step_bass, R.string.sound_profile_hint_bass, intArrayOf(4, 2, 1), -4, 8),
    TREBLE(R.string.sound_profile_step_treble, R.string.sound_profile_hint_treble, intArrayOf(3, 2, 1), -4, 6),
    PRESENCE(R.string.sound_profile_step_presence, R.string.sound_profile_hint_presence, intArrayOf(2, 1, 1), -3, 4);
}

private fun Profile.get(d: Dimension) = when (d) {
    Dimension.BASS -> bass
    Dimension.TREBLE -> treble
    Dimension.PRESENCE -> presence
}

private fun Profile.with(d: Dimension, v: Int) = when (d) {
    Dimension.BASS -> copy(bass = v)
    Dimension.TREBLE -> copy(treble = v)
    Dimension.PRESENCE -> copy(presence = v)
}

@Composable
fun SoundProfileWizard(onDismiss: () -> Unit) {
    val context = LocalContext.current
    val prefs = remember(context) { context.getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE) }
    val controller = remember(context) { ServiceLocator.provideBudsController(context) }
    val isConnected by controller.isConnected.collectAsState()
    val model by controller.effectiveModel.collectAsState()

    val dims = Dimension.entries
    var dimIndex by remember { mutableIntStateOf(0) }
    var round by remember { mutableIntStateOf(0) }
    var direction by remember { mutableIntStateOf(1) }
    // Start from neutral so the test isn't anchored on a previous result.
    var current by remember { mutableStateOf(Profile.NEUTRAL) }
    var listeningToB by remember { mutableStateOf(false) }
    var finished by remember { mutableStateOf(false) }

    val dim = dims[dimIndex]
    val step = dim.steps[round]
    val candidate = run {
        var v = (current.get(dim) + direction * step).coerceIn(dim.min, dim.max)
        // At a range edge, test the other side instead of comparing a value with itself.
        if (v == current.get(dim)) v = (current.get(dim) - direction * step).coerceIn(dim.min, dim.max)
        current.with(dim, v)
    }

    fun preview(p: Profile) = controller.previewCustomBands(PersonalSoundProfile.baseBands(p))

    // Apply the option being listened to whenever it changes.
    LaunchedEffect(current, candidate, listeningToB, finished) {
        if (!finished) preview(if (listeningToB) candidate else current)
    }

    fun close() {
        controller.endEqPreview()
        ServiceLocator.notifyAutoEqSettingsChanged()
        onDismiss()
    }
    DisposableEffect(Unit) { onDispose { controller.endEqPreview() } }

    fun choose(pickedB: Boolean) {
        if (pickedB) current = candidate else direction = -direction
        listeningToB = false
        if (round < dim.steps.lastIndex) {
            round++
        } else if (dimIndex < dims.lastIndex) {
            dimIndex++
            round = 0
            direction = 1
        } else {
            finished = true
            PersonalSoundProfile.save(prefs, current)
            prefs.edit().putString(PersonalSoundProfile.PREF_MODE, PersonalSoundProfile.MODE_CURVES).apply()
        }
    }

    val totalRounds = dims.sumOf { it.steps.size }
    val doneRounds = dims.take(dimIndex).sumOf { it.steps.size } + round

    AlertDialog(
        onDismissRequest = { close() },
        title = { Text(stringResource(R.string.sound_profile_title)) },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                when {
                    !model.supportsCustomEqualizer -> Text(stringResource(R.string.sound_profile_unsupported))
                    !isConnected -> Text(stringResource(R.string.sound_profile_connect_first))
                    finished -> {
                        Text(stringResource(R.string.sound_profile_done))
                        Text(
                            profileSummary(current),
                            fontWeight = FontWeight.Bold,
                            color = MaterialTheme.colorScheme.primary
                        )
                    }
                    else -> {
                        Text(stringResource(R.string.sound_profile_intro), style = MaterialTheme.typography.bodySmall)
                        LinearProgressIndicator(
                            progress = { doneRounds.toFloat() / totalRounds },
                            modifier = Modifier.fillMaxWidth()
                        )
                        Text(stringResource(dim.titleRes), fontWeight = FontWeight.Bold)
                        Text(
                            stringResource(dim.hintRes),
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp), modifier = Modifier.fillMaxWidth()) {
                            FilterChip(
                                selected = !listeningToB,
                                onClick = { listeningToB = false },
                                label = { Text(stringResource(R.string.sound_profile_listen_a)) },
                                modifier = Modifier.weight(1f)
                            )
                            FilterChip(
                                selected = listeningToB,
                                onClick = { listeningToB = true },
                                label = { Text(stringResource(R.string.sound_profile_listen_b)) },
                                modifier = Modifier.weight(1f)
                            )
                        }
                        Text(
                            stringResource(R.string.sound_profile_which),
                            textAlign = TextAlign.Center,
                            modifier = Modifier.fillMaxWidth()
                        )
                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp), modifier = Modifier.fillMaxWidth()) {
                            Button(onClick = { choose(false) }, modifier = Modifier.weight(1f), shape = RoundedCornerShape(14.dp)) {
                                Text(stringResource(R.string.sound_profile_prefer_a))
                            }
                            Button(onClick = { choose(true) }, modifier = Modifier.weight(1f), shape = RoundedCornerShape(14.dp)) {
                                Text(stringResource(R.string.sound_profile_prefer_b))
                            }
                        }
                        TextButton(onClick = { choose(false) }, modifier = Modifier.align(Alignment.CenterHorizontally)) {
                            Text(stringResource(R.string.sound_profile_no_difference))
                        }
                    }
                }
            }
        },
        confirmButton = {
            TextButton(onClick = { close() }) {
                Text(stringResource(if (finished) android.R.string.ok else android.R.string.cancel))
            }
        }
    )
}

@Composable
fun profileSummary(p: Profile): String {
    fun f(v: Int) = if (v > 0) "+$v" else v.toString()
    return stringResource(R.string.sound_profile_summary, f(p.bass), f(p.treble), f(p.presence))
}
