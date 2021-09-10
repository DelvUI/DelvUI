using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Enums;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class CastbarHud : HudElement, IHudElementWithActor
    {
        private CastbarConfig Config => (CastbarConfig)_config;
        private LabelHud _castNameLabel;
        private LabelHud _castTimeLabel;

        protected LastUsedCast _lastUsedCast = null;

        public Actor Actor { get; set; } = null;

        public CastbarHud(string id, CastbarConfig config) : base(id, config)
        {
            _castNameLabel = new LabelHud(id + "_castNameLabel", config.CastNameConfig);
            _castTimeLabel = new LabelHud(id + "_castTimeLabel", config.CastTimeConfig);
        }

        public override unsafe void Draw(Vector2 origin)
        {
            if (!Config.Enabled || Actor == null || Actor is not Chara)
            {
                return;
            }

            var battleChara = (BattleChara*)Actor.Address;
            var castInfo = battleChara->SpellCastInfo;
            var isCasting = castInfo.IsCasting > 0;

            if (castInfo.IsCasting <= 0)
            {
                return;
            }

            var currentCastId = castInfo.ActionID;
            var currentCastType = castInfo.ActionType;
            var currentCastTime = castInfo.CurrentCastTime;
            var totalCastTime = castInfo.TotalCastTime;

            if (_lastUsedCast == null || _lastUsedCast.CastId != currentCastId || _lastUsedCast.ActionType != currentCastType)
            {
                _lastUsedCast = new LastUsedCast(currentCastId, currentCastType, castInfo);
            }

            var castPercent = 100f / totalCastTime * currentCastTime;
            var castScale = castPercent / 100f;
            var startPos = origin + Config.Position - Config.Size / 2f;
            var endPos = startPos + Config.Size;

            var drawList = ImGui.GetWindowDrawList();

            // bg
            drawList.AddRectFilled(startPos, endPos, 0x88000000);

            // extras
            DrawExtras(origin + Config.Position, totalCastTime);

            // cast bar
            var color = Color();
            drawList.AddRectFilledMultiColor(
                startPos,
                startPos + new Vector2(Config.Size.X * castScale, Config.Size.Y),
                color["gradientLeft"],
                color["gradientRight"],
                color["gradientRight"],
                color["gradientLeft"]
            );

            // border
            drawList.AddRect(startPos, endPos, 0xFF000000);

            // icon
            var iconSize = Vector2.Zero;
            if (Config.ShowIcon && _lastUsedCast.IconTexture != null)
            {
                ImGui.SetCursorPos(startPos);
                iconSize = new Vector2(Config.Size.Y, Config.Size.Y);
                ImGui.Image(_lastUsedCast.IconTexture.ImGuiHandle, iconSize);
                drawList.AddRect(startPos, startPos + iconSize, 0xFF000000);
            }

            // cast name
            Config.CastNameConfig.SetText(_lastUsedCast.ActionText);
            _castNameLabel.Draw(origin + Config.Position);

            // cast time
            Config.CastTimeConfig.SetText(Math.Round(totalCastTime - totalCastTime * castScale, 1).ToString(CultureInfo.InvariantCulture));
            _castTimeLabel.Draw(origin + Config.Position);
        }

        public virtual void DrawExtras(Vector2 origin, float totalCastTime)
        {
            // override
        }

        public virtual Dictionary<string, uint> Color()
        {
            return Config.Color.Map;
        }
    }

    public class PlayerCastbarHud : CastbarHud
    {
        private PlayerCastbarConfig Config => (PlayerCastbarConfig)_config;

        public PlayerCastbarHud(string id, PlayerCastbarConfig config) : base(id, config)
        {

        }

        public override void DrawExtras(Vector2 origin, float totalCastTime)
        {
            if (!Config.ShowSlideCast || Config.SlideCastTime <= 0)
            {
                return;
            }

            var drawList = ImGui.GetWindowDrawList();

            var slideCastWidth = Math.Min(Config.Size.X, (Config.SlideCastTime / 1000f) * Config.Size.X / totalCastTime);
            var startPos = new Vector2(origin.X + Config.Size.X / 2f - slideCastWidth, origin.Y - Config.Size.Y / 2f);
            var endPos = startPos + new Vector2(slideCastWidth, Config.Size.Y);
            var color = Config.SlideCastColor.Map;

            drawList.AddRectFilledMultiColor(
                startPos,
                endPos,
                color["gradientLeft"],
                color["gradientRight"],
                color["gradientRight"],
                color["gradientLeft"]
            );
        }

        public override Dictionary<string, uint> Color()
        {
            if (!Config.UseJobColor || Actor is not Chara)
            {
                return Config.Color.Map;
            }

            var chara = (Chara)Actor;
            var color = GlobalColors.Instance.ColorForJobId(chara.ClassJob.Id);
            return color != null ? color.Map : Config.Color.Map;
        }
    }

    public class TargetCastbarHud : CastbarHud
    {
        private TargetCastbarConfig Config => (TargetCastbarConfig)_config;

        public TargetCastbarHud(string id, TargetCastbarConfig config) : base(id, config)
        {

        }

        public override Dictionary<string, uint> Color()
        {
            if (Config.ShowInterruptableColor && _lastUsedCast.Interruptable)
            {
                return Config.InterruptableColor.Map;
            }

            if (!Config.UseColorForDamageTypes)
            {
                return Config.Color.Map;
            }

            switch (_lastUsedCast.DamageType)
            {
                case DamageType.Physical:
                case DamageType.Blunt:
                case DamageType.Slashing:
                case DamageType.Piercing:
                    return Config.PhysicalDamageColor.Map;

                case DamageType.Magic:
                    return Config.MagicalDamageColor.Map;

                case DamageType.Darkness:
                    return Config.DarknessDamageColor.Map;
            }

            return Config.Color.Map;
        }
    }
}
