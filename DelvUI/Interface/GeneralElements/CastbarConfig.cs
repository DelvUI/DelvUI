using DelvUI.Config;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Serializable]
    public class PlayerCastbarConfig : CastbarConfig
    {
        public bool UseJobColor = false;

        public bool ShowSlideCast = true;
        public int SlideCastTime = 200;
        public PluginConfigColor SlideCastColor = new PluginConfigColor(new(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        public static Vector2 DefaultSize => new Vector2(254, 25);
        public static PlayerCastbarConfig DefaultCastbar()
        {
            var size = DefaultSize;
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY - size.Y / 2f);

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", LabelTextAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(size.X / 2f - 5, 0), "", LabelTextAnchor.Right);

            return new PlayerCastbarConfig(pos, size, castNameConfig, castTimeConfig, "Player Castbar");
        }

        public PlayerCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig, string title = "")
            : base(position, size, castNameConfig, castTimeConfig, title)
        {

        }

        protected override bool DrawExtraSettings()
        {
            var changed = false;

            changed |= ImGui.Checkbox("Use Job Color", ref UseJobColor);
            changed |= ImGui.Checkbox("Show Slide Cast", ref UseJobColor);
            changed |= ImGui.DragInt("Slide Cast Time (milliseconds)", ref SlideCastTime, 1, 1, 2000);
            changed |= ColorEdit4("Slide Cast Color", ref SlideCastColor);

            return changed;
        }
    }

    [Serializable]
    public class TargetCastbarConfig : CastbarConfig
    {
        public bool ShowInterruptableColor = true;
        public PluginConfigColor InterruptableColor = new PluginConfigColor(new(255f / 255f, 0 / 255f, 255f / 255f, 100f / 100f));

        public bool UseColorForDamageTypes = true;
        public PluginConfigColor PhysicalDamangeColor = new PluginConfigColor(new(255f / 255f, 0 / 255f, 0f / 255f, 100f / 100f));
        public PluginConfigColor MagicalDamageColor = new PluginConfigColor(new(0f / 255f, 0 / 255f, 255f / 255f, 100f / 100f));
        public PluginConfigColor DarknessDamageColor = new PluginConfigColor(new(255f / 255f, 0 / 255f, 255f / 255f, 100f / 100f));

        public static Vector2 DefaultSize => new Vector2(254, 25);
        public static TargetCastbarConfig DefaultCastbar()
        {
            var size = DefaultSize;
            var pos = new Vector2(0, HUDConstants.BaseHUDOffsetY / 2f - size.Y / 2);

            var castNameConfig = new LabelConfig(new Vector2(5, 0), "", LabelTextAnchor.Left);
            var castTimeConfig = new LabelConfig(new Vector2(size.X / 2f - 5, 0), "", LabelTextAnchor.Right);

            return new TargetCastbarConfig(pos, size, castNameConfig, castTimeConfig, "Target Castbar");
        }

        public TargetCastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig, string title = "")
            : base(position, size, castNameConfig, castTimeConfig, title)
        {

        }

        protected override bool DrawExtraSettings()
        {
            var changed = false;

            changed |= ImGui.Checkbox("Show Interruptable Color", ref ShowInterruptableColor);
            changed |= ColorEdit4("Interruptable Color", ref InterruptableColor);

            changed |= ImGui.Checkbox("Show Damage Type Color", ref UseColorForDamageTypes);
            changed |= ColorEdit4("Physical Damage Color", ref PhysicalDamangeColor);
            changed |= ColorEdit4("Magical Color", ref MagicalDamageColor);
            changed |= ColorEdit4("Darkness Color", ref DarknessDamageColor);

            return changed;
        }
    }

    [Serializable]
    public class CastbarConfig : MovablePluginConfigObject
    {
        public bool ShowCastName = true;
        public LabelConfig CastNameConfig;

        public bool ShowCastTime = true;
        public LabelConfig CastTimeConfig;

        public bool ShowIcon = true;
        public PluginConfigColor Color = new PluginConfigColor(new(255f / 255f, 158f / 255f, 208f / 255f, 100f / 100f));

        [JsonIgnore] public string Title;

        public CastbarConfig(Vector2 position, Vector2 size, LabelConfig castNameConfig, LabelConfig castTimeConfig, string title = "")
            : base(position, size)
        {
            CastNameConfig = castNameConfig;
            CastTimeConfig = castTimeConfig;
        }

        public new bool Draw()
        {
            var changed = false;

            ImGui.Text(Title);
            ImGui.BeginGroup();
            {
                changed |= base.Draw();
                changed |= ImGui.Checkbox("Show Cast Name", ref ShowCastName);
                changed |= CastNameConfig.Draw();
                changed |= ImGui.Checkbox("Show Cast Time", ref ShowCastTime);
                changed |= CastTimeConfig.Draw();
                changed |= ImGui.Checkbox("Show Icon", ref ShowIcon);
                changed |= ColorEdit4("Color", ref Color);

                changed |= DrawExtraSettings();
            }

            return changed;
        }

        protected virtual bool DrawExtraSettings()
        {
            // override
            return false;
        }
    }
}
