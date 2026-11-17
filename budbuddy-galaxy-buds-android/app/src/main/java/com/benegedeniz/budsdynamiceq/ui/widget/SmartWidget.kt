package com.benegedeniz.budsdynamiceq.ui.widget

import android.content.Context
import android.content.Intent
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.glance.ColorFilter
import androidx.glance.GlanceId
import androidx.glance.GlanceModifier
import androidx.glance.Image
import androidx.glance.ImageProvider
import androidx.glance.LocalContext
import androidx.glance.action.ActionParameters
import androidx.glance.action.actionStartActivity
import androidx.glance.action.clickable
import androidx.glance.appwidget.GlanceAppWidget
import androidx.glance.appwidget.GlanceAppWidgetReceiver
import androidx.glance.appwidget.action.ActionCallback
import androidx.glance.appwidget.action.actionRunCallback
import androidx.glance.appwidget.provideContent
import androidx.glance.background
import androidx.glance.currentState
import androidx.glance.layout.Alignment
import androidx.glance.layout.Box
import androidx.glance.layout.Column
import androidx.glance.layout.Row
import androidx.glance.layout.fillMaxHeight
import androidx.glance.layout.fillMaxSize
import androidx.glance.layout.height
import androidx.glance.layout.padding
import androidx.glance.layout.size
import androidx.glance.layout.width
import androidx.glance.text.FontWeight
import androidx.glance.text.Text
import androidx.glance.text.TextStyle
import com.benegedeniz.budsdynamiceq.MainActivity
import com.benegedeniz.budsdynamiceq.R
import com.benegedeniz.budsdynamiceq.di.ServiceLocator
import com.benegedeniz.budsdynamiceq.rules.GenreEqMapper

/**
 * Home-screen widget: per-earbud and case battery, a Gemini shortcut and an Auto-EQ toggle.
 * Refreshed by WidgetCoordinator while the service runs.
 */
class SmartWidget : GlanceAppWidget() {
    override suspend fun provideGlance(context: Context, id: GlanceId) {
        val controller = ServiceLocator.provideBudsController(context)
        provideContent {
            // Reading this key makes Glance recompose when WidgetCoordinator flips it.
            currentState(key = androidx.datastore.preferences.core.booleanPreferencesKey("force_update"))
            val prefs = context.getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE)
            SmartWidgetContent(
                isConnected = controller.isConnected.value,
                batteryL = controller.batteryL.value,
                batteryR = controller.batteryR.value,
                batteryCase = controller.batteryCase.value,
                chargingL = controller.chargingL.value,
                chargingR = controller.chargingR.value,
                chargingCase = controller.chargingCase.value,
                autoEqEnabled = prefs.getBoolean(GenreEqMapper.PREF_ENABLED, false)
            )
        }
    }
}

class SmartWidgetReceiver : GlanceAppWidgetReceiver() {
    override val glanceAppWidget: GlanceAppWidget = SmartWidget()
}

class ToggleAutoEqCallback : ActionCallback {
    override suspend fun onAction(context: Context, glanceId: GlanceId, parameters: ActionParameters) {
        val prefs = context.getSharedPreferences("BudsPrefs", Context.MODE_PRIVATE)
        val enabled = !prefs.getBoolean(GenreEqMapper.PREF_ENABLED, false)
        prefs.edit().putBoolean(GenreEqMapper.PREF_ENABLED, enabled).apply()
        SmartWidget().update(context, glanceId)
    }
}

@Composable
private fun SmartWidgetContent(
    isConnected: Boolean,
    batteryL: Int,
    batteryR: Int,
    batteryCase: Int,
    chargingL: Boolean,
    chargingR: Boolean,
    chargingCase: Boolean,
    autoEqEnabled: Boolean
) {
    val ctx = LocalContext.current
    Row(
        modifier = GlanceModifier
            .fillMaxSize()
            .background(ImageProvider(R.drawable.widget_surface_bg))
            .padding(8.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Row(
            modifier = GlanceModifier
                .defaultWeight()
                .fillMaxHeight()
                .background(ImageProvider(R.drawable.widget_surface_variant_bg))
                .clickable(actionStartActivity<MainActivity>()),
            verticalAlignment = Alignment.CenterVertically
        ) {
            BatteryCell(ctx.getString(R.string.buds_left), batteryL, chargingL, isConnected)
            BatteryCell(ctx.getString(R.string.buds_right), batteryR, chargingR, isConnected)
            BatteryCell(ctx.getString(R.string.widget_case_short), batteryCase, chargingCase, isConnected)
        }

        Box(modifier = GlanceModifier.width(8.dp)) {}

        // Gemini: a tap from the home screen is a user action, so starting the assistant is allowed.
        ShortcutButton(
            iconRes = R.drawable.ic_widget_mic,
            label = ctx.getString(R.string.widget_gemini),
            highlighted = false,
            modifier = GlanceModifier.clickable(
                androidx.glance.appwidget.action.actionStartActivity(
                    Intent(Intent.ACTION_VOICE_COMMAND).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
                )
            )
        )

        Box(modifier = GlanceModifier.width(8.dp)) {}

        ShortcutButton(
            iconRes = R.drawable.ic_widget_auto_eq,
            label = ctx.getString(R.string.widget_auto_eq),
            highlighted = autoEqEnabled,
            modifier = GlanceModifier.clickable(actionRunCallback<ToggleAutoEqCallback>())
        )
    }
}

@Composable
private fun androidx.glance.layout.RowScope.BatteryCell(label: String, level: Int, charging: Boolean, connected: Boolean) {
    Box(modifier = GlanceModifier.defaultWeight(), contentAlignment = Alignment.Center) {
        Column(horizontalAlignment = Alignment.CenterHorizontally) {
            Text(
                text = label,
                style = TextStyle(color = OnSurfaceVariantColor, fontSize = 11.sp, fontWeight = FontWeight.Bold)
            )
            val value = when {
                !connected || level < 0 -> "--"
                charging -> "⚡$level%"
                else -> "$level%"
            }
            val color = if (connected && level in 0..15 && !charging) {
                androidx.glance.color.ColorProvider(day = Color(0xFFD32F2F), night = Color(0xFFFF6B6B))
            } else OnSurfaceColor
            Text(text = value, style = TextStyle(color = color, fontSize = 15.sp, fontWeight = FontWeight.Bold))
        }
    }
}

@Composable
private fun ShortcutButton(iconRes: Int, label: String, highlighted: Boolean, modifier: GlanceModifier) {
    Box(
        modifier = modifier
            .size(56.dp)
            .background(ImageProvider(if (highlighted) R.drawable.widget_primary_bg else R.drawable.widget_surface_variant_bg)),
        contentAlignment = Alignment.Center
    ) {
        Column(horizontalAlignment = Alignment.CenterHorizontally) {
            Image(
                provider = ImageProvider(iconRes),
                contentDescription = label,
                colorFilter = ColorFilter.tint(if (highlighted) SurfaceColor else OnSurfaceVariantColor),
                modifier = GlanceModifier.size(22.dp)
            )
            Box(modifier = GlanceModifier.height(2.dp)) {}
            Text(
                text = label,
                style = TextStyle(
                    color = if (highlighted) SurfaceColor else OnSurfaceVariantColor,
                    fontSize = 9.sp,
                    fontWeight = FontWeight.Bold
                ),
                maxLines = 1
            )
        }
    }
}
