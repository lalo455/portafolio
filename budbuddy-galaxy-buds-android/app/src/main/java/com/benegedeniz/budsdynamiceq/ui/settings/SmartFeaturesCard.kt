package com.benegedeniz.budsdynamiceq.ui.settings

import android.content.Context
import android.content.Intent
import android.net.Uri
import android.os.PowerManager
import android.provider.Settings
import android.widget.Toast
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.IntentSenderRequest
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.expandVertically
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.animation.shrinkVertically
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.AutoAwesome
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.KeyboardArrowDown
import androidx.compose.material.icons.filled.KeyboardArrowUp
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.hapticfeedback.HapticFeedbackType
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalHapticFeedback
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.LifecycleEventObserver
import com.benegedeniz.budsdynamiceq.R
import com.benegedeniz.budsdynamiceq.companion.CompanionHelper
import com.benegedeniz.budsdynamiceq.data.model.EqPreset
import com.benegedeniz.budsdynamiceq.di.ServiceLocator
import com.benegedeniz.budsdynamiceq.rules.GenreEqMapper
import com.benegedeniz.budsdynamiceq.rules.PersonalSoundProfile
import com.benegedeniz.budsdynamiceq.service.ExternalControlReceiver
import com.benegedeniz.budsdynamiceq.service.NetworkNcMonitor
import com.benegedeniz.budsdynamiceq.data.model.NoiseControlMode
import com.benegedeniz.budsdynamiceq.service.WellbeingCoordinator
import com.benegedeniz.budsdynamiceq.ui.theme.StatusActiveGreen

/**
 * Settings card for features that recover Samsung-only functionality on other phones:
 * companion association, battery optimization, automatic EQ by genre/BPM, wellbeing
 * reminders and automation (Tasker) access.
 */
@Composable
fun SmartFeaturesCard() {
    val context = LocalContext.current
    val haptic = LocalHapticFeedback.current
    val prefs = remember(context) { context.getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE) }
    val budsController = remember(context) { ServiceLocator.provideBudsController(context) }
    val savedMac by budsController.savedDeviceMac.collectAsState()

    var expanded by remember { mutableStateOf(false) }

    // Re-check system state (association, battery exemption) whenever we return to the screen.
    var resumeTick by remember { mutableIntStateOf(0) }
    @Suppress("DEPRECATION")
    val lifecycleOwner = androidx.compose.ui.platform.LocalLifecycleOwner.current
    DisposableEffect(lifecycleOwner) {
        val observer = LifecycleEventObserver { _, event -> if (event == Lifecycle.Event.ON_RESUME) resumeTick++ }
        lifecycleOwner.lifecycle.addObserver(observer)
        onDispose { lifecycleOwner.lifecycle.removeObserver(observer) }
    }
    val isAssociated = remember(resumeTick, savedMac) { CompanionHelper.isAssociated(context, savedMac) }
    val ignoringBatteryOpt = remember(resumeTick) {
        context.getSystemService(PowerManager::class.java)?.isIgnoringBatteryOptimizations(context.packageName) == true
    }

    var autoEq by remember { mutableStateOf(prefs.getBoolean(GenreEqMapper.PREF_ENABLED, false)) }
    var autoEqBpm by remember { mutableStateOf(prefs.getBoolean(GenreEqMapper.PREF_USE_BPM, true)) }
    var showGenreDialog by remember { mutableStateOf(false) }
    var showWizard by remember { mutableStateOf(false) }
    var autoEqMode by remember {
        mutableStateOf(prefs.getString(PersonalSoundProfile.PREF_MODE, PersonalSoundProfile.MODE_CURVES) ?: PersonalSoundProfile.MODE_CURVES)
    }
    var networkNc by remember { mutableStateOf(prefs.getBoolean(NetworkNcMonitor.PREF_ENABLED, false)) }
    var wifiMode by remember {
        mutableStateOf(NetworkNcMonitor.modeFor(prefs, NetworkNcMonitor.PREF_WIFI_MODE, NetworkNcMonitor.DEFAULT_WIFI_MODE))
    }
    var mobileMode by remember {
        mutableStateOf(NetworkNcMonitor.modeFor(prefs, NetworkNcMonitor.PREF_MOBILE_MODE, NetworkNcMonitor.DEFAULT_MOBILE_MODE))
    }
    var neckEnabled by remember { mutableStateOf(prefs.getBoolean(WellbeingCoordinator.PREF_NECK_ENABLED, false)) }
    var neckMinutes by remember { mutableIntStateOf(prefs.getInt(WellbeingCoordinator.PREF_NECK_MINUTES, WellbeingCoordinator.DEFAULT_NECK_MINUTES)) }
    var batteryEnabled by remember { mutableStateOf(prefs.getBoolean(WellbeingCoordinator.PREF_LOW_BATTERY_ENABLED, true)) }
    var batteryThreshold by remember { mutableIntStateOf(prefs.getInt(WellbeingCoordinator.PREF_LOW_BATTERY_THRESHOLD, WellbeingCoordinator.DEFAULT_LOW_BATTERY_THRESHOLD)) }
    var batterySpeak by remember { mutableStateOf(prefs.getBoolean(WellbeingCoordinator.PREF_LOW_BATTERY_SPEAK, true)) }
    var externalControl by remember { mutableStateOf(prefs.getBoolean(ExternalControlReceiver.PREF_ENABLED, false)) }
    var showTaskerHelp by remember { mutableStateOf(false) }

    val associationLauncher = rememberLauncherForActivityResult(
        ActivityResultContracts.StartIntentSenderForResult()
    ) {
        resumeTick++
        CompanionHelper.startObservingPresence(context, savedMac)
    }

    Card(
        shape = RoundedCornerShape(24.dp),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant),
        modifier = Modifier.fillMaxWidth()
    ) {
        Column(modifier = Modifier.fillMaxWidth()) {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .clickable {
                        haptic.performHapticFeedback(HapticFeedbackType.TextHandleMove)
                        expanded = !expanded
                    }
                    .padding(20.dp),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                    Icon(Icons.Default.AutoAwesome, null, tint = MaterialTheme.colorScheme.primary, modifier = Modifier.size(24.dp))
                    Text(
                        text = stringResource(R.string.smart_features),
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.onSurface
                    )
                }
                Icon(
                    if (expanded) Icons.Default.KeyboardArrowUp else Icons.Default.KeyboardArrowDown,
                    null,
                    tint = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }

            AnimatedVisibility(visible = expanded, enter = fadeIn() + expandVertically(), exit = fadeOut() + shrinkVertically()) {
                Column(
                    modifier = Modifier.fillMaxWidth().padding(horizontal = 20.dp).padding(bottom = 20.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    HorizontalDivider(color = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.15f))

                    // --- Companion device association
                    SectionLabel(stringResource(R.string.smart_section_connection))
                    StatusActionRow(
                        title = stringResource(R.string.companion_title),
                        description = stringResource(R.string.companion_desc),
                        done = isAssociated,
                        doneLabel = stringResource(R.string.companion_linked),
                        actionLabel = stringResource(R.string.companion_link),
                        actionEnabled = savedMac != null && CompanionHelper.isSupported(context),
                        onAction = {
                            val mac = savedMac ?: return@StatusActionRow
                            CompanionHelper.requestAssociation(
                                context, mac,
                                onPending = { sender ->
                                    associationLauncher.launch(IntentSenderRequest.Builder(sender).build())
                                },
                                onFailure = { err ->
                                    Toast.makeText(
                                        context,
                                        context.getString(R.string.companion_failed, err ?: "—"),
                                        Toast.LENGTH_LONG
                                    ).show()
                                }
                            )
                        }
                    )
                    if (savedMac == null) {
                        HintText(stringResource(R.string.companion_needs_device))
                    }

                    StatusActionRow(
                        title = stringResource(R.string.battery_opt_title),
                        description = stringResource(R.string.battery_opt_desc),
                        done = ignoringBatteryOpt,
                        doneLabel = stringResource(R.string.battery_opt_done),
                        actionLabel = stringResource(R.string.battery_opt_action),
                        actionEnabled = true,
                        onAction = { openBatterySettings(context) }
                    )

                    // --- Auto EQ
                    SectionLabel(stringResource(R.string.smart_section_sound))
                    SwitchRow(
                        title = stringResource(R.string.auto_eq_title),
                        description = stringResource(R.string.auto_eq_desc),
                        checked = autoEq
                    ) {
                        haptic.performHapticFeedback(HapticFeedbackType.TextHandleMove)
                        autoEq = it
                        prefs.edit().putBoolean(GenreEqMapper.PREF_ENABLED, it).apply()
                        ServiceLocator.notifyAutoEqSettingsChanged()
                    }
                    if (autoEq) {
                        val model by budsController.effectiveModel.collectAsState()
                        val curvesAvailable = model.supportsCustomEqualizer
                        // Mode: personal 9-band curves (recommended) or Samsung's fixed presets.
                        if (curvesAvailable) {
                            Text(
                                stringResource(R.string.auto_eq_mode_title),
                                style = MaterialTheme.typography.bodyLarge,
                                fontWeight = FontWeight.Bold
                            )
                            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                FilterChip(
                                    selected = autoEqMode == PersonalSoundProfile.MODE_CURVES,
                                    onClick = {
                                        autoEqMode = PersonalSoundProfile.MODE_CURVES
                                        prefs.edit().putString(PersonalSoundProfile.PREF_MODE, autoEqMode).apply()
                                        ServiceLocator.notifyAutoEqSettingsChanged()
                                    },
                                    label = { Text(stringResource(R.string.auto_eq_mode_curves)) }
                                )
                                FilterChip(
                                    selected = autoEqMode == PersonalSoundProfile.MODE_PRESETS,
                                    onClick = {
                                        autoEqMode = PersonalSoundProfile.MODE_PRESETS
                                        prefs.edit().putString(PersonalSoundProfile.PREF_MODE, autoEqMode).apply()
                                        ServiceLocator.notifyAutoEqSettingsChanged()
                                    },
                                    label = { Text(stringResource(R.string.auto_eq_mode_presets)) }
                                )
                            }
                            HintTextNeutral(
                                stringResource(
                                    if (autoEqMode == PersonalSoundProfile.MODE_CURVES) R.string.auto_eq_mode_curves_desc
                                    else R.string.auto_eq_mode_presets_desc
                                )
                            )
                        }

                        if (curvesAvailable && autoEqMode == PersonalSoundProfile.MODE_CURVES) {
                            val profileDone = remember(resumeTick, showWizard) { prefs.getBoolean(PersonalSoundProfile.PREF_DONE, false) }
                            val profile = remember(resumeTick, showWizard) { PersonalSoundProfile.load(prefs) }
                            Button(
                                onClick = { showWizard = true },
                                modifier = Modifier.fillMaxWidth(),
                                shape = RoundedCornerShape(14.dp)
                            ) {
                                Text(stringResource(R.string.sound_profile_button), fontWeight = FontWeight.Bold)
                            }
                            HintTextNeutral(
                                if (profileDone) profileSummary(profile)
                                else stringResource(R.string.sound_profile_not_done)
                            )
                        }

                        SwitchRow(
                            title = stringResource(R.string.auto_eq_bpm_title),
                            description = stringResource(R.string.auto_eq_bpm_desc),
                            checked = autoEqBpm
                        ) {
                            haptic.performHapticFeedback(HapticFeedbackType.TextHandleMove)
                            autoEqBpm = it
                            prefs.edit().putBoolean(GenreEqMapper.PREF_USE_BPM, it).apply()
                            ServiceLocator.notifyAutoEqSettingsChanged()
                        }
                        if (!curvesAvailable || autoEqMode == PersonalSoundProfile.MODE_PRESETS) {
                            OutlinedButton(
                                onClick = { showGenreDialog = true },
                                modifier = Modifier.fillMaxWidth(),
                                shape = RoundedCornerShape(14.dp)
                            ) {
                                Text(stringResource(R.string.auto_eq_configure), fontWeight = FontWeight.Bold)
                            }
                        }
                    }

                    // --- Noise control by network (Wi-Fi = home/safe place, mobile data = outside)
                    SwitchRow(
                        title = stringResource(R.string.network_nc_title),
                        description = stringResource(R.string.network_nc_desc),
                        checked = networkNc
                    ) {
                        haptic.performHapticFeedback(HapticFeedbackType.TextHandleMove)
                        networkNc = it
                        prefs.edit().putBoolean(NetworkNcMonitor.PREF_ENABLED, it).apply()
                    }
                    if (networkNc) {
                        val model by budsController.effectiveModel.collectAsState()
                        val modes = remember(model) {
                            buildList {
                                add(NoiseControlMode.NOISE_CANCELLATION)
                                if (model.supportsAdaptiveNC) add(NoiseControlMode.ADAPTIVE)
                                if (model.supportsTransparencyNC) add(NoiseControlMode.TRANSPARENT)
                                add(NoiseControlMode.OFF)
                            }
                        }
                        NcModePicker(
                            label = stringResource(R.string.network_nc_wifi),
                            selected = wifiMode,
                            modes = modes
                        ) {
                            wifiMode = it
                            prefs.edit().putString(NetworkNcMonitor.PREF_WIFI_MODE, it.name).apply()
                        }
                        NcModePicker(
                            label = stringResource(R.string.network_nc_mobile),
                            selected = mobileMode,
                            modes = modes
                        ) {
                            mobileMode = it
                            prefs.edit().putString(NetworkNcMonitor.PREF_MOBILE_MODE, it.name).apply()
                        }
                    }

                    // --- Wellbeing
                    SectionLabel(stringResource(R.string.smart_section_wellbeing))
                    SwitchRow(
                        title = stringResource(R.string.neck_title),
                        description = stringResource(R.string.neck_desc),
                        checked = neckEnabled
                    ) {
                        haptic.performHapticFeedback(HapticFeedbackType.TextHandleMove)
                        neckEnabled = it
                        prefs.edit().putBoolean(WellbeingCoordinator.PREF_NECK_ENABLED, it).apply()
                    }
                    if (neckEnabled) {
                        ChoiceChips(
                            options = listOf(30, 45, 60, 90),
                            selected = neckMinutes,
                            label = { stringResource(R.string.minutes_short, it) }
                        ) {
                            neckMinutes = it
                            prefs.edit().putInt(WellbeingCoordinator.PREF_NECK_MINUTES, it).apply()
                        }
                    }

                    SwitchRow(
                        title = stringResource(R.string.low_battery_title),
                        description = stringResource(R.string.low_battery_desc),
                        checked = batteryEnabled
                    ) {
                        haptic.performHapticFeedback(HapticFeedbackType.TextHandleMove)
                        batteryEnabled = it
                        prefs.edit().putBoolean(WellbeingCoordinator.PREF_LOW_BATTERY_ENABLED, it).apply()
                    }
                    if (batteryEnabled) {
                        ChoiceChips(
                            options = listOf(10, 15, 20, 30),
                            selected = batteryThreshold,
                            label = { "$it%" }
                        ) {
                            batteryThreshold = it
                            prefs.edit().putInt(WellbeingCoordinator.PREF_LOW_BATTERY_THRESHOLD, it).apply()
                        }
                        SwitchRow(
                            title = stringResource(R.string.low_battery_speak_title),
                            description = stringResource(R.string.low_battery_speak_desc),
                            checked = batterySpeak
                        ) {
                            batterySpeak = it
                            prefs.edit().putBoolean(WellbeingCoordinator.PREF_LOW_BATTERY_SPEAK, it).apply()
                        }
                    }

                    // --- Automation
                    SectionLabel(stringResource(R.string.smart_section_automation))
                    SwitchRow(
                        title = stringResource(R.string.external_control_title),
                        description = stringResource(R.string.external_control_desc),
                        checked = externalControl
                    ) {
                        haptic.performHapticFeedback(HapticFeedbackType.TextHandleMove)
                        externalControl = it
                        prefs.edit().putBoolean(ExternalControlReceiver.PREF_ENABLED, it).apply()
                    }
                    if (externalControl) {
                        TextButton(onClick = { showTaskerHelp = true }) {
                            Text(stringResource(R.string.external_control_help))
                        }
                    }
                }
            }
        }
    }

    if (showWizard) {
        SoundProfileWizard(onDismiss = {
            showWizard = false
            autoEqMode = prefs.getString(PersonalSoundProfile.PREF_MODE, PersonalSoundProfile.MODE_CURVES) ?: PersonalSoundProfile.MODE_CURVES
        })
    }
    if (showGenreDialog) {
        GenreMappingDialog(
            supportsCustom = budsController.effectiveModel.collectAsState().value.supportsCustomEqualizer,
            onDismiss = { showGenreDialog = false }
        )
    }
    if (showTaskerHelp) {
        AlertDialog(
            onDismissRequest = { showTaskerHelp = false },
            confirmButton = { TextButton(onClick = { showTaskerHelp = false }) { Text(stringResource(android.R.string.ok)) } },
            title = { Text(stringResource(R.string.external_control_title)) },
            text = {
                Column(Modifier.verticalScroll(rememberScrollState())) {
                    Text(stringResource(R.string.external_control_help_intro))
                    Spacer(Modifier.height(8.dp))
                    Text(
                        text = TASKER_HELP,
                        fontFamily = FontFamily.Monospace,
                        style = MaterialTheme.typography.bodySmall
                    )
                }
            }
        )
    }
}

private val TASKER_HELP = """
Package: com.benegedeniz.budsdynamiceq
Target: Broadcast Receiver

${ExternalControlReceiver.ACTION_SET_NC}
  mode = OFF | NOISE_CANCELLATION | TRANSPARENT | ADAPTIVE

${ExternalControlReceiver.ACTION_TOGGLE_NC}

${ExternalControlReceiver.ACTION_SET_EQ}
  preset = NORMAL | BASS_BOOST | SOFT | DYNAMIC | CLEAR | TREBLE_BOOST | CUSTOM_1 | CUSTOM_2 | CUSTOM_3

${ExternalControlReceiver.ACTION_SET_AUTO_EQ}
  enabled = true | false

${ExternalControlReceiver.ACTION_CONNECT}
""".trimIndent()

private fun openBatterySettings(context: Context) {
    val intents = listOf(
        Intent(Settings.ACTION_IGNORE_BATTERY_OPTIMIZATION_SETTINGS),
        Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS, Uri.parse("package:${context.packageName}"))
    )
    for (intent in intents) {
        try {
            context.startActivity(intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK))
            Toast.makeText(context, context.getString(R.string.battery_opt_hint), Toast.LENGTH_LONG).show()
            return
        } catch (_: Exception) { }
    }
}

@Composable
private fun GenreMappingDialog(supportsCustom: Boolean, onDismiss: () -> Unit) {
    val context = LocalContext.current
    val prefs = remember(context) { context.getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE) }
    val presets = remember(supportsCustom) {
        EqPreset.entries.filter {
            it != EqPreset.DEFAULT && it != EqPreset.IGNORE && (supportsCustom || !it.isCustom)
        }
    }
    val categories = GenreEqMapper.Category.entries
    val mapping = remember {
        mutableStateMapOf<GenreEqMapper.Category, EqPreset>().apply {
            categories.forEach { put(it, GenreEqMapper.presetFor(prefs, it)) }
        }
    }

    AlertDialog(
        onDismissRequest = onDismiss,
        confirmButton = { TextButton(onClick = onDismiss) { Text(stringResource(android.R.string.ok)) } },
        title = { Text(stringResource(R.string.auto_eq_configure)) },
        text = {
            Column(Modifier.verticalScroll(rememberScrollState()), verticalArrangement = Arrangement.spacedBy(4.dp)) {
                Text(
                    stringResource(R.string.auto_eq_configure_desc),
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                Spacer(Modifier.height(8.dp))
                categories.forEach { category ->
                    var menuOpen by remember { mutableStateOf(false) }
                    Row(
                        Modifier.fillMaxWidth().padding(vertical = 2.dp),
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        Text(stringResource(category.labelRes), modifier = Modifier.weight(1f))
                        Box {
                            OutlinedButton(onClick = { menuOpen = true }, shape = RoundedCornerShape(12.dp)) {
                                Text(stringResource(mapping[category]!!.displayNameRes))
                            }
                            DropdownMenu(expanded = menuOpen, onDismissRequest = { menuOpen = false }) {
                                presets.forEach { preset ->
                                    DropdownMenuItem(
                                        text = { Text(stringResource(preset.displayNameRes)) },
                                        onClick = {
                                            mapping[category] = preset
                                            GenreEqMapper.setPresetFor(prefs, category, preset)
                                            ServiceLocator.notifyAutoEqSettingsChanged()
                                            menuOpen = false
                                        }
                                    )
                                }
                            }
                        }
                    }
                }
            }
        }
    )
}

@Composable
private fun NcModePicker(
    label: String,
    selected: NoiseControlMode,
    modes: List<NoiseControlMode>,
    onSelect: (NoiseControlMode) -> Unit
) {
    var open by remember { mutableStateOf(false) }
    Row(Modifier.fillMaxWidth(), verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.SpaceBetween) {
        Text(label, style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.onSurface, modifier = Modifier.weight(1f))
        Box {
            OutlinedButton(onClick = { open = true }, shape = RoundedCornerShape(12.dp)) {
                Text(stringResource(selected.displayNameRes))
            }
            DropdownMenu(expanded = open, onDismissRequest = { open = false }) {
                modes.forEach { mode ->
                    DropdownMenuItem(
                        text = { Text(stringResource(mode.displayNameRes)) },
                        onClick = {
                            onSelect(mode)
                            open = false
                        }
                    )
                }
            }
        }
    }
}

@Composable
private fun SectionLabel(text: String) {
    Text(
        text = text.uppercase(),
        style = MaterialTheme.typography.labelMedium,
        fontWeight = FontWeight.Bold,
        color = MaterialTheme.colorScheme.primary,
        modifier = Modifier.padding(top = 4.dp)
    )
}

@Composable
private fun HintTextNeutral(text: String) {
    Text(text, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
}

@Composable
private fun HintText(text: String) {
    Text(text, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.error)
}

@Composable
private fun TitleAndDescription(title: String, description: String, modifier: Modifier) {
    Column(modifier = modifier) {
        Text(title, style = MaterialTheme.typography.bodyLarge, fontWeight = FontWeight.Bold, color = MaterialTheme.colorScheme.onSurface)
        Spacer(Modifier.height(4.dp))
        Text(description, style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.onSurfaceVariant)
    }
}

@Composable
private fun SwitchRow(title: String, description: String, checked: Boolean, onChange: (Boolean) -> Unit) {
    Row(Modifier.fillMaxWidth(), verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.SpaceBetween) {
        TitleAndDescription(title, description, Modifier.weight(1f).padding(end = 16.dp))
        Switch(checked = checked, onCheckedChange = onChange)
    }
}

@Composable
private fun StatusActionRow(
    title: String,
    description: String,
    done: Boolean,
    doneLabel: String,
    actionLabel: String,
    actionEnabled: Boolean,
    onAction: () -> Unit
) {
    Row(Modifier.fillMaxWidth(), verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.SpaceBetween) {
        TitleAndDescription(title, description, Modifier.weight(1f).padding(end = 12.dp))
        if (done) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Icon(Icons.Default.Check, null, tint = StatusActiveGreen, modifier = Modifier.size(18.dp))
                Spacer(Modifier.width(4.dp))
                Text(doneLabel, color = StatusActiveGreen, fontWeight = FontWeight.Bold, style = MaterialTheme.typography.bodyMedium)
            }
        } else {
            Button(onClick = onAction, enabled = actionEnabled, shape = RoundedCornerShape(14.dp)) {
                Text(actionLabel)
            }
        }
    }
}

@Composable
private fun ChoiceChips(options: List<Int>, selected: Int, label: @Composable (Int) -> String, onSelect: (Int) -> Unit) {
    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
        options.forEach { value ->
            FilterChip(
                selected = value == selected,
                onClick = { onSelect(value) },
                label = { Text(label(value)) }
            )
        }
    }
}
