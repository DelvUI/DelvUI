using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Interface.GeneralElements;
using System;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    [Portable(false)]
    public class ThresholdBarConfig : BarConfigBase
    {
        [Checkbox("Threshold")]
        [Order(45)]
        public bool Threshold = false;

        [DragFloat("Threshold Value", min = 0f, max = 10000f)]
        [Order(50, collapseWith = nameof(Threshold))]
        public float ThresholdValue = 0f;

        [Combo("Activate Above/Below Threshold", "Above", "Below")]
        [Order(60, collapseWith = nameof(Threshold))]
        public ThresholdType ThresholdType = ThresholdType.Below;

        [ColorEdit4("Threshold Color")]
        [Order(65, collapseWith = nameof(Threshold))]
        public PluginConfigColor ThresholdColor = new PluginConfigColor(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));

        [NestedConfig("Bar Text", 80, separator = false, spacing = true)]
        public LabelConfig LabelConfig;

        public ThresholdBarConfig(
            Vector2 position, 
            Vector2 size,
            PluginConfigColor fillColor, 
            PluginConfigColor? threshHoldColor = null,
            float threshold = 0f) : base(position, size, fillColor)
        {
            LabelConfig = new LabelConfig(Vector2.Zero, "", DrawAnchor.Center, DrawAnchor.Center);
            ThresholdColor = threshHoldColor ?? fillColor;
            ThresholdValue = threshold;
        }

        public override BarHud[] GetBars(float current, float max, float min = 0f, GameObject? actor = null)
        {
            if (HideWhenInactive && current <= min)
            {
                return Array.Empty<BarHud>();
            }

            Rect background = new Rect(Position, Size, BackgroundColor);
            PluginConfigColor fillColor = IsThresholdActive(current) ? ThresholdColor : FillColor;
            Rect foreground = Rect.GetFillRect(Position, Size, FillDirection, fillColor, current, max, min);
            return new BarHud[] { new BarHud(background, new[] { foreground }, DrawBorder, Anchor, new[] { LabelConfig }, actor) };
        }

        public override bool IsActive(float current, float max, float min)
        {
            return current > min;
        }

        public bool IsThresholdActive(float current)
        {
            return Threshold && (ThresholdType == ThresholdType.Below && current < ThresholdValue ||
                                 ThresholdType == ThresholdType.Above && current > ThresholdValue);
        }
    }

    public enum ThresholdType
    {
        Above,
        Below
    }
}
