using DelvUI.Enums;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using BattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;

namespace DelvUI.Interface.GeneralElements
{
    public class CastbarHud : DraggableHudElement, IHudElementWithActor
    {
        private CastbarConfig Config => (CastbarConfig)_config;
        private LabelHud _castNameLabel;
        private LabelHud _castTimeLabel;

        protected LastUsedCast? LastUsedCast;

        public GameObject? Actor { get; set; }

        public CastbarHud(string id, CastbarConfig config, string displayName) : base(id, config, displayName)
        {
            _castNameLabel = new LabelHud(id + "_castNameLabel", config.CastNameConfig);
            _castTimeLabel = new LabelHud(id + "_castTimeLabel", config.CastTimeConfig);
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override unsafe void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || Actor == null || Actor is not Character)
            {
                return;
            }

            if (Actor.ObjectKind != ObjectKind.Player && Actor.ObjectKind != ObjectKind.BattleNpc)
            {
                return;
            }

            var battleChara = (BattleChara*)Actor.Address;
            var castInfo = battleChara->SpellCastInfo;
            var isCasting = castInfo.IsCasting > 0;

            if (castInfo.IsCasting <= 0 && !Config.Preview)
            {
                return;
            }

            var currentCastId = castInfo.ActionID;
            var currentCastType = castInfo.ActionType;
            var currentCastTime = castInfo.CurrentCastTime;
            var totalCastTime = castInfo.TotalCastTime;

            if (LastUsedCast == null || LastUsedCast.CastId != currentCastId || LastUsedCast.ActionType != currentCastType)
            {
                LastUsedCast = new LastUsedCast(currentCastId, currentCastType, castInfo);
            }

            var castPercent = 100f / totalCastTime * currentCastTime;
            var castScale = castPercent / 100f;
            var startPos = Utils.GetAnchoredPosition(origin + Config.Position, Config.Size, Config.Anchor);
            var endPos = startPos + Config.Size;

            var drawList = ImGui.GetWindowDrawList();

            // bg
            drawList.AddRectFilled(startPos, endPos, 0x88000000);

            // extras
            DrawExtras(startPos, totalCastTime);

            // cast bar
            var color = Color();
            DrawHelper.DrawGradientFilledRect(startPos, new Vector2(Config.Size.X * castScale, Config.Size.Y), color, drawList);

            // border
            drawList.AddRect(startPos, endPos, 0xFF000000);

            // icon
            var iconSize = Vector2.Zero;
            if (Config.ShowIcon)
            {
                if (LastUsedCast.IconTexture != null)
                {
                    ImGui.SetCursorPos(startPos);
                    iconSize = new Vector2(Config.Size.Y, Config.Size.Y);
                    ImGui.Image(LastUsedCast.IconTexture.ImGuiHandle, iconSize);
                    drawList.AddRect(startPos, startPos + iconSize, 0xFF000000);
                }
                else if (Config.Preview)
                {
                    drawList.AddRect(startPos, startPos + new Vector2(Config.Size.Y, Config.Size.Y), 0xFF000000);
                }
            }

            // cast name
            Config.CastNameConfig.SetText(Config.Preview ? "Name" : LastUsedCast.ActionText);
            _castNameLabel.Draw(startPos + new Vector2(iconSize.X, 0), Config.Size, Actor);

            // cast time
            var text = Config.Preview ? "Time" : Math.Round(totalCastTime - totalCastTime * castScale, 1).ToString(CultureInfo.InvariantCulture);
            Config.CastTimeConfig.SetText(text);
            _castTimeLabel.Draw(startPos, Config.Size, Actor);
        }

        public virtual void DrawExtras(Vector2 castbarPos, float totalCastTime)
        {
            // override
        }

        public virtual PluginConfigColor Color()
        {
            return Config.Color;
        }
    }

    public class PlayerCastbarHud : CastbarHud
    {
        private PlayerCastbarConfig Config => (PlayerCastbarConfig)_config;

        public PlayerCastbarHud(string id, PlayerCastbarConfig config, string displayName) : base(id, config, displayName)
        {

        }

        public override void DrawExtras(Vector2 castbarPos, float totalCastTime)
        {
            if (!Config.ShowSlideCast || Config.SlideCastTime <= 0 || Config.Preview)
            {
                return;
            }

            var drawList = ImGui.GetWindowDrawList();

            var slideCastWidth = Math.Min(Config.Size.X, (Config.SlideCastTime / 1000f) * Config.Size.X / totalCastTime);
            var startPos = new Vector2(castbarPos.X + Config.Size.X - slideCastWidth, castbarPos.Y);
            var endPos = startPos + new Vector2(slideCastWidth, Config.Size.Y);
            var color = Config.SlideCastColor;

            DrawHelper.DrawGradientFilledRect(startPos, new Vector2(slideCastWidth, Config.Size.Y), color, drawList);
        }

        public override PluginConfigColor Color()
        {
            if (!Config.UseJobColor || Actor is not Character)
            {
                return Config.Color;
            }

            var chara = (Character)Actor;
            var color = GlobalColors.Instance.ColorForJobId(chara.ClassJob.Id);
            return color != null ? color : Config.Color;
        }
    }

    public class TargetCastbarHud : CastbarHud
    {
        private TargetCastbarConfig Config => (TargetCastbarConfig)_config;

        public TargetCastbarHud(string id, TargetCastbarConfig config, string displayName) : base(id, config, displayName)
        {

        }

        public override PluginConfigColor Color()
        {
            if (Config.ShowInterruptableColor && LastUsedCast.Interruptable)
            {
                return Config.InterruptableColor;
            }

            if (!Config.UseColorForDamageTypes)
            {
                return Config.Color;
            }

            switch (LastUsedCast.DamageType)
            {
                case DamageType.Physical:
                case DamageType.Blunt:
                case DamageType.Slashing:
                case DamageType.Piercing:
                    return Config.PhysicalDamageColor;

                case DamageType.Magic:
                    return Config.MagicalDamageColor;

                case DamageType.Darkness:
                    return Config.DarknessDamageColor;
            }

            return Config.Color;
        }
    }
}
