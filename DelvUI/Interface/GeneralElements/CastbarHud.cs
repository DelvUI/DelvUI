using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class CastbarHud : ParentAnchoredDraggableHudElement, IHudElementWithActor, IHudElementWithAnchorableParent, IHudElementWithPreview
    {
        private CastbarConfig Config => (CastbarConfig)_config;
        private readonly LabelHud _castNameLabel;
        private readonly LabelHud _castTimeLabel;

        protected LastUsedCast? LastUsedCast;

        public GameObject? Actor { get; set; }

        protected override bool AnchorToParent => Config is UnitFrameCastbarConfig { AnchorToUnitFrame: true };
        protected override DrawAnchor ParentAnchor => Config is UnitFrameCastbarConfig config ? config.UnitFrameAnchor : DrawAnchor.Center;

        public CastbarHud(CastbarConfig config, string? displayName = null) : base(config, displayName)
        {
            _castNameLabel = new LabelHud(config.CastNameLabel);
            _castTimeLabel = new LabelHud(config.CastTimeLabel);
        }

        public void StopPreview()
        {
            Config.Preview = false;
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes() => (new List<Vector2> { Config.Position }, new List<Vector2> { Config.Size });

        public override void DrawChildren(Vector2 origin)
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

            UpdateCurrentCast(out float currentCastTime, out float totalCastTime);
            if (totalCastTime == 0)
            {
                return;
            }

            bool validIcon = LastUsedCast?.IconTexture is not null;
            Vector2 iconSize = Config.ShowIcon && validIcon ? new Vector2(Config.Size.Y, Config.Size.Y) : Vector2.Zero;

            PluginConfigColor fillColor = GetColor();
            Rect background = new(Config.Position, Config.Size, Config.BackgroundColor);
            Rect progress = BarUtilities.GetFillRect(Config.Position, Config.Size, Config.FillDirection, fillColor, currentCastTime, totalCastTime);

            BarHud bar = new(Config, Actor);
            bar.SetBackground(background);

            if (Config.UseReverseFill)
            {
                Vector2 reverseFillSize = Config.Size - BarUtilities.GetFillDirectionOffset(progress.Size, Config.FillDirection);
                Vector2 reverseFillPos = Config.FillDirection.IsInverted()
                    ? Config.Position
                    : Config.Position + BarUtilities.GetFillDirectionOffset(progress.Size, Config.FillDirection);

                PluginConfigColor reverseFillColor = Config.ReverseFillColor;
                bar.AddForegrounds(new Rect(reverseFillPos, reverseFillSize, reverseFillColor));
            }

            AddExtras(bar, totalCastTime);

            bar.AddForegrounds(progress);

            Vector2 pos = origin + ParentPos();
            AddDrawActions(bar.GetDrawActions(pos, Config.StrataLevel));

            // icon
            Vector2 startPos = Config.Position + Utils.GetAnchoredPosition(pos, Config.Size, Config.Anchor);
            if (Config.ShowIcon)
            {
                AddDrawAction(Config.StrataLevel, () =>
                {
                    DrawHelper.DrawInWindow(ID + "_icon", startPos, Config.Size, false, false, (drawList) =>
                    {
                        if (validIcon)
                        {
                            ImGui.SetCursorPos(startPos);
                            ImGui.Image(LastUsedCast!.IconTexture!.ImGuiHandle, iconSize);
                        }

                        if (Config.DrawBorder)
                        {
                            drawList.AddRect(startPos, startPos + iconSize, Config.BorderColor.Base, 0, ImDrawFlags.None, Config.BorderThickness);
                        }
                    });
                });
            }

            // cast name
            bool isNameLeftAnchored = Config.CastNameLabel.TextAnchor is DrawAnchor.Left or DrawAnchor.TopLeft or DrawAnchor.BottomLeft;
            Vector2 namePos = Config.ShowIcon && isNameLeftAnchored ? startPos + new Vector2(iconSize.X, 0) : startPos;
            string? castName = LastUsedCast?.ActionText.CheckForUpperCase();
            Config.CastNameLabel.SetText(Config.Preview ? "Cast Name" : castName ?? "");

            AddDrawAction(Config.CastNameLabel.StrataLevel, () =>
            {
                _castNameLabel.Draw(namePos, Config.Size, Actor);
            });

            // cast time
            bool isTimeLeftAnchored = Config.CastTimeLabel.TextAnchor is DrawAnchor.Left or DrawAnchor.TopLeft or DrawAnchor.BottomLeft;
            Vector2 timePos = Config.ShowIcon && isTimeLeftAnchored ? startPos + new Vector2(iconSize.X, 0) : startPos;
            float value = Config.Preview ? 0.5f : totalCastTime - currentCastTime;
            Config.CastTimeLabel.SetValue(value);

            AddDrawAction(Config.CastTimeLabel.StrataLevel, () =>
            {
                _castTimeLabel.Draw(timePos, Config.Size, Actor);
            });
        }

        private void UpdateCurrentCast(out float currentCastTime, out float totalCastTime)
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

            uint currentCastId = battleChara.CastActionId;
            ActionType currentCastType = (ActionType)battleChara.CastActionType;
            currentCastTime = battleChara.CurrentCastTime;
            totalCastTime = battleChara.TotalCastTime;

            if (LastUsedCast == null || LastUsedCast.CastId != currentCastId || LastUsedCast.ActionType != currentCastType)
            {
                LastUsedCast = new LastUsedCast(currentCastId, currentCastType, battleChara.IsCastInterruptible);
            }
        }

        public virtual void AddExtras(BarHud bar, float totalCastTime)
        {
            // override
        }

        public virtual PluginConfigColor GetColor() => Config.FillColor;
    }

    public class PlayerCastbarHud : CastbarHud
    {
        private PlayerCastbarConfig Config => (PlayerCastbarConfig)_config;

        public PlayerCastbarHud(PlayerCastbarConfig config, string displayName) : base(config, displayName)
        {

        }

        public override void AddExtras(BarHud bar, float totalCastTime)
        {
            if (!Config.ShowSlideCast || Config.SlideCastTime <= 0 || Config.Preview)
            {
                return;
            }

            float slideCastWidth = Math.Min(Config.Size.X, Config.SlideCastTime / 1000f * Config.Size.X / totalCastTime);
            Vector2 size = new(slideCastWidth, Config.Size.Y);
            Rect slideCast = new(Config.Position + Config.Size - size, size, Config.SlideCastColor);

            if (Config.FillDirection is BarDirection.Left)
            {
                bool validIcon = LastUsedCast?.IconTexture is not null;
                Vector2 iconSize = Config.ShowIcon && validIcon ? new Vector2(Config.Size.Y, Config.Size.Y) : Vector2.Zero;
                slideCast = Config.ShowIcon ? new Rect(Config.Position, size + new Vector2(iconSize.X, 0), Config.SlideCastColor) : new Rect(Config.Position, size, Config.SlideCastColor);
            }

            bar.AddForegrounds(slideCast);
        }

        public override PluginConfigColor GetColor()
        {
            if (!Config.UseJobColor || Actor is not Character)
            {
                return Config.FillColor;
            }

            Character? chara = (Character)Actor;
            PluginConfigColor? color = GlobalColors.Instance.ColorForJobId(chara.ClassJob.Id);
            return color ?? Config.FillColor;
        }
    }

    public class TargetCastbarHud : CastbarHud
    {
        private TargetCastbarConfig Config => (TargetCastbarConfig)_config;

        public TargetCastbarHud(TargetCastbarConfig config, string? displayName = null) : base(config, displayName)
        {

        }

        public override PluginConfigColor GetColor()
        {
            if (Config.ShowInterruptableColor && LastUsedCast?.Interruptible == true)
            {
                return Config.InterruptableColor;
            }

            if (!Config.UseColorForDamageTypes)
            {
                return Config.FillColor;
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

            return Config.FillColor;
        }
    }
}
