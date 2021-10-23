using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class CastbarHud : ParentAnchoredDraggableHudElement, IHudElementWithActor, IHudElementWithAnchorableParent, IHudElementWithPreview
    {
        private CastbarConfig Config => (CastbarConfig)_config;
        private LabelHud _castNameLabel;
        private LabelHud _castTimeLabel;

        protected LastUsedCast? LastUsedCast;

        public GameObject? Actor { get; set; }

        protected override bool AnchorToParent => Config is UnitFrameCastbarConfig { AnchorToUnitFrame: true };
        protected override DrawAnchor ParentAnchor => Config is UnitFrameCastbarConfig config ? config.UnitFrameAnchor : DrawAnchor.Center;

        public CastbarHud(CastbarConfig config, string displayName) : base(config, displayName)
        {
            _castNameLabel = new LabelHud(config.CastNameConfig);
            _castTimeLabel = new LabelHud(config.CastTimeConfig);
        }

        public void StopPreview()
        {
            Config.Preview = false;
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override unsafe void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled)
            {
                return;
            }

            if (!Config.Preview &&
                (Actor == null || Actor is not Character || Actor.ObjectKind != ObjectKind.Player && Actor.ObjectKind != ObjectKind.BattleNpc))
            {
                return;
            }

            UpdateCurrentCast(out var currentCastTime, out var totalCastTime);
            if (totalCastTime == 0)
            {
                return;
            }

            float castPercent = 100f / totalCastTime * currentCastTime;
            float castScale = castPercent / 100f;

            Vector2 startPos = origin + GetAnchoredPosition(Config.Position, Config.Size, Config.Anchor);
            Vector2 endPos = startPos + Config.Size;

            bool validIcon = LastUsedCast != null && LastUsedCast.IconTexture != null;
            Vector2 iconSize = Config.ShowIcon && validIcon ? new Vector2(Config.Size.Y, Config.Size.Y) : Vector2.Zero;

            DrawHelper.DrawInWindow(ID, startPos, Config.Size, false, false, (drawList) =>
            {
                // bg
                drawList.AddRectFilled(startPos, endPos, Config.BackgroundColor.Base);


                // extras
                DrawExtras(startPos, totalCastTime);

                // cast bar
                PluginConfigColor? color = Color();
                Vector2 fillStartPos = startPos + new Vector2(iconSize.X, 0);
                Vector2 fillMaxSize = new Vector2(Config.Size.X - iconSize.X, Config.Size.Y);
                DrawHelper.DrawGradientFilledRect(fillStartPos, new Vector2(fillMaxSize.X * castScale, fillMaxSize.Y), color, drawList);

                // border
                drawList.AddRect(startPos, endPos, 0xFF000000);

                // icon
                if (Config.ShowIcon)
                {
                    if (validIcon)
                    {
                        ImGui.SetCursorPos(startPos);
                        ImGui.Image(LastUsedCast!.IconTexture!.ImGuiHandle, iconSize);
                        drawList.AddRect(startPos, startPos + iconSize, 0xFF000000);
                    }
                    else if (Config.Preview)
                    {
                        drawList.AddRect(startPos, startPos + iconSize, 0xFF000000);
                    }
                }
            });

            // cast name
            float iconSizeX = Config.ShowIcon ? iconSize.X : 0;
            Vector2 castNamePos = startPos + new Vector2(iconSizeX, 0);
            string? castName = LastUsedCast?.ActionText.CheckForUpperCase();

            Config.CastNameConfig.SetText(Config.Preview ? "Cast Name" : castName ?? "");
            _castNameLabel.Draw(startPos + new Vector2(iconSizeX, 0), Config.Size, Actor);

            // cast time
            string? text = Config.Preview ? "Cast Time" : Math.Round(totalCastTime - totalCastTime * castScale, 1).ToString(CultureInfo.InvariantCulture);
            Config.CastTimeConfig.SetText(text);
            _castTimeLabel.Draw(startPos, Config.Size, Actor);
        }

        private unsafe void UpdateCurrentCast(out float currentCastTime, out float totalCastTime)
        {
            currentCastTime = Config.Preview ? 0.5f : 0f;
            totalCastTime = 1f;

            if (Config.Preview || Actor is not BattleChara battleChara)
            {
                return;
            }

            totalCastTime = 0;
            if (!battleChara.IsCasting)
            {
                return;
            }

            var currentCastId = battleChara.CastActionId;
            var currentCastType = (ActionType)battleChara.CastActionType;
            currentCastTime = battleChara.CurrentCastTime;
            totalCastTime = battleChara.TotalCastTime;

            if (LastUsedCast == null || LastUsedCast.CastId != currentCastId || LastUsedCast.ActionType != currentCastType)
            {
                LastUsedCast = new LastUsedCast(currentCastId, currentCastType, battleChara.IsCastInterruptible);
            }
        }

        public virtual void DrawExtras(Vector2 castbarPos, float totalCastTime)
        {
            // override
        }

        public virtual PluginConfigColor Color() => Config.Color;
    }

    public class PlayerCastbarHud : CastbarHud
    {
        private PlayerCastbarConfig Config => (PlayerCastbarConfig)_config;

        public PlayerCastbarHud(PlayerCastbarConfig config, string displayName) : base(config, displayName)
        {

        }

        public override void DrawExtras(Vector2 castbarPos, float totalCastTime)
        {
            if (!Config.ShowSlideCast || Config.SlideCastTime <= 0 || Config.Preview)
            {
                return;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            float slideCastWidth = Math.Min(Config.Size.X, (Config.SlideCastTime / 1000f) * Config.Size.X / totalCastTime);
            Vector2 startPos = new(castbarPos.X + Config.Size.X - slideCastWidth, castbarPos.Y);
            Vector2 endPos = startPos + new Vector2(slideCastWidth, Config.Size.Y);
            PluginConfigColor? color = Config.SlideCastColor;

            DrawHelper.DrawGradientFilledRect(startPos, new Vector2(slideCastWidth, Config.Size.Y), color, drawList);
        }

        public override PluginConfigColor Color()
        {
            if (!Config.UseJobColor || Actor is not Character)
            {
                return Config.Color;
            }

            Character? chara = (Character)Actor;
            PluginConfigColor? color = GlobalColors.Instance.ColorForJobId(chara.ClassJob.Id);
            return color ?? Config.Color;
        }
    }

    public class TargetCastbarHud : CastbarHud
    {
        private TargetCastbarConfig Config => (TargetCastbarConfig)_config;

        public TargetCastbarHud(TargetCastbarConfig config, string displayName) : base(config, displayName)
        {

        }

        public override PluginConfigColor Color()
        {
            if (Config.ShowInterruptableColor && LastUsedCast?.Interruptible == true)
            {
                return Config.InterruptableColor;
            }

            if (!Config.UseColorForDamageTypes)
            {
                return Config.Color;
            }

            if (LastUsedCast != null)
            {
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
            }

            return Config.Color;
        }
    }
}
