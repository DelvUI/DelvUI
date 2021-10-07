﻿using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class ProgressBarConfig : BarConfig
    {
        [NestedConfig("Threshold", 45)]
        public ThresholdConfig ThresholdConfig = new ThresholdConfig();

        [NestedConfig("Bar Text", 1000, separator = false, spacing = true)]
        public LabelConfig Label;

        public ProgressBarConfig(
            Vector2 position,
            Vector2 size,
            PluginConfigColor fillColor,
            PluginConfigColor? threshHoldColor = null,
            float threshold = 0f) : base(position, size, fillColor)
        {
            Label = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
            ThresholdConfig.Color = threshHoldColor ?? ThresholdConfig.Color;
            ThresholdConfig.Value = threshold;
        }
    }

    public class ThresholdConfig : PluginConfigObject
    {
        [DragFloat("Threshold Value", min = 0f, max = 10000f)]
        [Order(10)]
        public float Value = 0f;

        [Combo("Activate Above/Below Threshold", "Above", "Below")]
        [Order(15)]
        public ThresholdType ThresholdType = ThresholdType.Below;

        [ColorEdit4("Color")]
        [Order(20)]
        public PluginConfigColor Color = new PluginConfigColor(new(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));

        [Checkbox("Show Threshold Marker")]
        [Order(25)]
        public bool ShowMarker = false;

        [DragInt("Threshold Marker Size", min = 0, max = 10000)]
        [Order(30, collapseWith = nameof(ShowMarker))]
        public int MarkerSize = 2;

        [ColorEdit4("Threshold Marker Color")]
        [Order(35, collapseWith = nameof(ShowMarker))]
        public PluginConfigColor MarkerColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        public bool IsActive(float current)
        {
            return Enabled && (ThresholdType == ThresholdType.Below && current < Value ||
                               ThresholdType == ThresholdType.Above && current > Value);
        }

        public ThresholdConfig()
        {
            Enabled = false;
        }
    }

    public enum ThresholdType
    {
        Above,
        Below
    }
}
