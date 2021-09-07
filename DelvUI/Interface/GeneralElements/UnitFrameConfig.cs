using DelvUI.Config;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{

    public static class DefaultUnitFrames
    {
        internal static Vector2 DefaultBigSize = new Vector2(270, 50);
        internal static Vector2 DefaultSmallSize = new Vector2(120, 20);

        public static UnitFrameConfig PlayerUnitFrame()
        {
            var size = DefaultBigSize;
            var pos = new Vector2(-HUDConstants.UnitFramesOffsetX - size.X / 2f, HUDConstants.BaseHUDOffsetY);

            var leftLabelConfig = new EditableLabelConfig(new Vector2(-size.X / 2f + 5, -size.Y / 2f + 2), "[name:abbreviate]", LabelTextAnchor.BottomLeft);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(size.X / 2 + 10, -size.Y / 2f + 2), "[health:current-short] | [health:percent]%", LabelTextAnchor.BottomRight);

            var config = new UnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig, "Player Unit Frame");
            config.TankStanceIndicatorConfig = new TankStanceIndicatorConfig();

            return config;
        }

        public static UnitFrameConfig TargetUnitFrame()
        {
            var size = DefaultBigSize;
            var pos = new Vector2(HUDConstants.UnitFramesOffsetX + size.X / 2f, HUDConstants.BaseHUDOffsetY);

            var leftLabelConfig = new EditableLabelConfig(new Vector2(-size.X / 2f + 5, -size.Y / 2f + 2), "[health:current-short] | [health:percent]%", LabelTextAnchor.BottomLeft);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(size.X / 2 - 5, -size.Y / 2 + 2), "[name:abbreviate]", LabelTextAnchor.BottomRight);

            return new UnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig, "Target Unit Frame");
        }

        public static UnitFrameConfig TargetOfTargetUnitFrame()
        {
            var size = DefaultSmallSize;
            var pos = new Vector2(
                HUDConstants.UnitFramesOffsetX + DefaultBigSize.X + 6 + size.X / 2f,
                HUDConstants.BaseHUDOffsetY - 15
            );

            var leftLabelConfig = new EditableLabelConfig(new Vector2(0, -size.Y / 2f + 2), "[name:abbreviate]", LabelTextAnchor.Bottom);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(0, size.Y / 2f), "", LabelTextAnchor.Top);

            return new UnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig, "Target of Target Unit Frame");
        }

        public static UnitFrameConfig FocusTargetUnitFrame()
        {
            var size = DefaultSmallSize;
            var pos = new Vector2(
                -HUDConstants.UnitFramesOffsetX - DefaultBigSize.X - 6 - size.X / 2f,
                HUDConstants.BaseHUDOffsetY - 15
            );

            var leftLabelConfig = new EditableLabelConfig(new Vector2(0, -size.Y / 2f + 2), "[name:abbreviate]", LabelTextAnchor.Bottom);
            var rightLabelConfig = new EditableLabelConfig(new Vector2(0, size.Y / 2f), "", LabelTextAnchor.Top);

            return new UnitFrameConfig(pos, size, leftLabelConfig, rightLabelConfig, "Focus Target Unit Frame");
        }
    }

    [Serializable]
    public class UnitFrameConfig : MovablePluginConfigObject
    {
        public bool UseCustomColor = false;
        public PluginConfigColor CustomColor = new PluginConfigColor(new Vector4(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f));

        public bool UseCustomBackgroundColor = false;
        public PluginConfigColor CustomBackgroundColor = new PluginConfigColor(new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        public LabelConfig LeftLabelConfig;
        public LabelConfig RightLabelConfig;
        public ShieldConfig ShieldConfig = new ShieldConfig();
        public TankStanceIndicatorConfig TankStanceIndicatorConfig = null;
        public bool ShowTankInvulnerability = true;

        [JsonIgnore] public string Title;

        public UnitFrameConfig(Vector2 position, Vector2 size, LabelConfig leftLabelConfig, LabelConfig rightLabelConfig, string title = null)
            : base(position, size)
        {
            LeftLabelConfig = leftLabelConfig;
            LeftLabelConfig.Title = "Left Label Text Format";

            RightLabelConfig = rightLabelConfig;
            LeftLabelConfig.Title = "Right Label Text Format";
        }

        public new bool Draw()
        {
            var changed = false;

            ImGui.Text(Title);
            ImGui.BeginGroup();
            {
                changed |= base.Draw();

                changed |= ImGui.Checkbox("Use Custom Color", ref UseCustomColor);
                if (UseCustomColor)
                {
                    changed |= ColorEdit4("Custom Color", ref CustomColor);
                }

                changed |= ImGui.Checkbox("Use Custom Background Color", ref UseCustomBackgroundColor);
                if (UseCustomBackgroundColor)
                {
                    changed |= ColorEdit4("Custom Background Color", ref CustomBackgroundColor);
                }

                changed |= LeftLabelConfig.Draw();
                changed |= RightLabelConfig.Draw();
                changed |= ShieldConfig.Draw();

                if (TankStanceIndicatorConfig != null)
                {
                    changed |= TankStanceIndicatorConfig.Draw();
                }

                changed |= ImGui.Checkbox("Show Tank Invulnerability", ref ShowTankInvulnerability);
            }

            return changed;
        }
    }

    [Serializable]
    public class ShieldConfig : PluginConfigObject
    {
        public int Height = 10;
        public bool HeightInPixels = false;
        public bool FillHealthFirst = true;
        public PluginConfigColor Color = new PluginConfigColor(new Vector4(198f / 255f, 210f / 255f, 255f / 255f, 70f / 100f));

        public new bool Draw()
        {
            var changed = false;

            ImGui.Text("Shields");
            ImGui.BeginGroup();
            {
                changed |= base.Draw();
                changed |= ImGui.DragInt("Shield Size", ref Height, .1f, 1, 1000);
                changed |= ImGui.Checkbox("Size in pixels", ref HeightInPixels);
                changed |= ImGui.Checkbox("Fill Health First", ref FillHealthFirst);
                changed |= ColorEdit4("Shield Color", ref Color);
            }

            return changed;
        }
    }

    [Serializable]
    public class TankStanceIndicatorConfig : PluginConfigObject
    {
        public int Thickness = 2;
        public Vector4 ActiveColor = new Vector4(0f / 255f, 205f / 255f, 230f / 255f, 100f / 100f);
        public Vector4 UnactiveColor = new Vector4(255f / 255f, 0f / 255f, 32f / 255f, 100f / 100f);

        public new bool Draw()
        {
            var changed = false;

            ImGui.Text("Tank Stance Indicator");
            ImGui.BeginGroup();
            {
                changed |= base.Draw();
                changed |= ImGui.DragInt("Thickness", ref Thickness, .1f, 0, 100);
                changed |= ImGui.ColorEdit4("Active Color", ref ActiveColor);
                changed |= ImGui.ColorEdit4("Unactive Color", ref ActiveColor);
            }

            return changed;
        }
    }
}
