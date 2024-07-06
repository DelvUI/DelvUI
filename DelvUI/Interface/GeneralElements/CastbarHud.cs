﻿using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures.TextureWraps;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.EnemyList;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;
using StructsBattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;

namespace DelvUI.Interface.GeneralElements
{
    public class CastbarHud : ParentAnchoredDraggableHudElement, IHudElementWithActor, IHudElementWithAnchorableParent, IHudElementWithPreview
    {
        private CastbarConfig Config => (CastbarConfig)_config;
        private readonly LabelHud _castNameLabel;
        private readonly LabelHud _castTimeLabel;

        protected LastUsedCast? LastUsedCast;

        public IGameObject? Actor { get; set; }

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
                (Actor == null || Actor is not ICharacter || Actor.ObjectKind != ObjectKind.Player && Actor.ObjectKind != ObjectKind.BattleNpc))
            {
                return;
            }

            UpdateCurrentCast(out float currentCastTime, out float totalCastTime);
            if (totalCastTime == 0 || currentCastTime >= totalCastTime)
            {
                return;
            }

            if (!ShouldShow() && !Config.Preview)
            {
                return;
            }

            Vector2 size = GetSize();
            IDalamudTextureWrap? iconTexture = LastUsedCast?.GetIconTexture();
            bool validIcon = Config.Preview ? true : iconTexture is not null;
            Vector2 iconSize = Config.ShowIcon && validIcon && !Config.SeparateIcon ? new Vector2(size.Y, size.Y) : Vector2.Zero;

            PluginConfigColor fillColor = GetColor();
            Rect background = new(Config.Position, size, Config.BackgroundColor);
            Rect progress = BarUtilities.GetFillRect(Config.Position, size, Config.FillDirection, fillColor, currentCastTime, totalCastTime);

            BarHud bar = new(Config, Actor);
            bar.SetBackground(background);

            if (Config.UseReverseFill)
            {
                Vector2 reverseFillSize = size - BarUtilities.GetFillDirectionOffset(progress.Size, Config.FillDirection);
                Vector2 reverseFillPos = Config.FillDirection.IsInverted()
                    ? Config.Position
                    : Config.Position + BarUtilities.GetFillDirectionOffset(progress.Size, Config.FillDirection);

                PluginConfigColor reverseFillColor = Config.ReverseFillColor;
                bar.AddForegrounds(new Rect(reverseFillPos, reverseFillSize, reverseFillColor));
            }

            AddExtras(bar, totalCastTime, iconTexture);

            bar.AddForegrounds(progress);

            Vector2 pos = origin + ParentPos();
            AddDrawActions(bar.GetDrawActions(pos, Config.StrataLevel));

            // icon
            Vector2 startPos = Config.Position + Utils.GetAnchoredPosition(pos, size, Config.Anchor);
            if (Config.ShowIcon && validIcon)
            {
                Vector2 finalIconPos = Config.SeparateIcon ? startPos + Config.CustomIconPosition : startPos;
                Vector2 finalIconSize = Config.SeparateIcon ? Config.CustomIconSize : iconSize;

                AddDrawAction(Config.StrataLevel, () =>
                {
                    DrawHelper.DrawInWindow(ID + "_icon", finalIconPos, finalIconSize, false, (drawList) =>
                    {
                        ImGui.SetCursorPos(finalIconPos);

                        IDalamudTextureWrap? texture = Config.Preview ? TexturesHelper.GetTexture<LuminaAction>(3577) : iconTexture;
                        if (texture != null)
                        {
                            ImGui.Image(texture.ImGuiHandle, finalIconSize);
                        }

                        if (Config.DrawBorder)
                        {
                            drawList.AddRect(finalIconPos, finalIconPos + finalIconSize, Config.BorderColor.Base, 0, ImDrawFlags.None, Config.BorderThickness);
                        }
                    });
                });
            }

            // cast name
            bool isNameLeftAnchored = Config.CastNameLabel.TextAnchor is DrawAnchor.Left or DrawAnchor.TopLeft or DrawAnchor.BottomLeft;
            Vector2 namePos = Config.ShowIcon && isNameLeftAnchored ? startPos + new Vector2(iconSize.X, 0) : startPos;

            string original = LastUsedCast?.ActionText ?? "";
            string? castName = EncryptedStringsHelper.GetString(original).CheckForUpperCase();
            Config.CastNameLabel.SetText(Config.Preview ? "Cast Name" : castName ?? "");

            AddDrawAction(Config.CastNameLabel.StrataLevel, () =>
            {
                _castNameLabel.Draw(namePos, size, Actor);
            });

            // cast time
            bool isTimeLeftAnchored = Config.CastTimeLabel.TextAnchor is DrawAnchor.Left or DrawAnchor.TopLeft or DrawAnchor.BottomLeft;
            Vector2 timePos = Config.ShowIcon && isTimeLeftAnchored ? startPos + new Vector2(iconSize.X, 0) : startPos;
            float value = Config.Preview ? 0.5f : totalCastTime - currentCastTime;

            if (Config.ShowMaxCastTime)
            {
                string format = Config.CastTimeLabel.NumberFormat.ToString();
                Config.CastTimeLabel.SetText(
                    value.ToString("N" + format, ConfigurationManager.Instance.ActiveCultreInfo) +
                    " / " +
                    totalCastTime.ToString("N" + format, ConfigurationManager.Instance.ActiveCultreInfo)
                );
            }
            else
            {
                Config.CastTimeLabel.SetValue(value);
            }

            AddDrawAction(Config.CastTimeLabel.StrataLevel, () =>
            {
                _castTimeLabel.Draw(timePos, size, Actor);
            });
        }

        private unsafe void UpdateCurrentCast(out float currentCastTime, out float totalCastTime)
        {
            if (Config.Preview || Actor is not IBattleChara battleChara)
            {
                currentCastTime = Config.Preview ? 0.5f : 0f;
                totalCastTime = 1f;
                return;
            }

            float current = 0;
            float total = 0;

            try
            {
                current = battleChara.CurrentCastTime;
                StructsBattleChara* chara = (StructsBattleChara*)battleChara.Address;
                CastInfo* castInfo = chara->GetCastInfo();

                if (castInfo != null)
                {
                    total = castInfo->TotalCastTime;
                }
            }
            catch
            {
                currentCastTime = 0;
                totalCastTime = 0;
                return;
            }

            if (!Utils.IsActorCasting(battleChara) && current <= 0)
            {
                currentCastTime = 0;
                totalCastTime = 0;
                return;
            }

            currentCastTime = current;
            totalCastTime = total;

            uint currentCastId = battleChara.CastActionId;
            ActionType currentCastType = (ActionType)battleChara.CastActionType;

            if (LastUsedCast == null || LastUsedCast.CastId != currentCastId || LastUsedCast.ActionType != currentCastType)
            {
                LastUsedCast = new LastUsedCast(currentCastId, currentCastType, battleChara.IsCastInterruptible);
            }
        }

        public virtual void AddExtras(BarHud bar, float totalCastTime, IDalamudTextureWrap? iconTexture)
        {
            // override
        }

        public virtual PluginConfigColor GetColor() => Config.FillColor;
        public virtual Vector2 GetSize() => Config.Size;

        public virtual bool ShouldShow() => true;
    }

    public class PlayerCastbarHud : CastbarHud
    {
        private PlayerCastbarConfig Config => (PlayerCastbarConfig)_config;

        public PlayerCastbarHud(PlayerCastbarConfig config, string displayName) : base(config, displayName)
        {

        }

        public override void AddExtras(BarHud bar, float totalCastTime, IDalamudTextureWrap? iconTexture)
        {
            if (!Config.ShowSlideCast || Config.SlideCastTime <= 0 || Config.Preview)
            {
                return;
            }

            Rect slideCast;

            if (Config.FillDirection.IsHorizontal())
            {
                float slideCastWidth = Math.Min(Config.Size.X, Config.SlideCastTime / 1000f * Config.Size.X / totalCastTime);
                Vector2 size = new(slideCastWidth, Config.Size.Y);
                slideCast = new(Config.Position + Config.Size - size, size, Config.SlideCastColor);

                if (Config.FillDirection is BarDirection.Left)
                {
                    bool validIcon = iconTexture is not null;
                    Vector2 iconSize = Config.ShowIcon && validIcon ? new Vector2(Config.Size.Y, Config.Size.Y) : Vector2.Zero;
                    slideCast = Config.ShowIcon ? new Rect(Config.Position, size + new Vector2(iconSize.X, 0), Config.SlideCastColor) : new Rect(Config.Position, size, Config.SlideCastColor);
                }
            }
            else
            {
                float slideCastHeight = Math.Min(Config.Size.Y, Config.SlideCastTime / 1000f * Config.Size.Y / totalCastTime);
                Vector2 size = new(Config.Size.X, slideCastHeight);
                slideCast = new(Config.Position + Config.Size - size, size, Config.SlideCastColor);

                if (Config.FillDirection is BarDirection.Up)
                {
                    slideCast = new(Config.Position, size, Config.SlideCastColor);
                }
            }

            bar.AddForegrounds(slideCast);
        }

        public override PluginConfigColor GetColor()
        {
            if (!Config.UseJobColor || Actor is not ICharacter)
            {
                return Config.FillColor;
            }

            ICharacter? chara = (ICharacter)Actor;
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

        public override unsafe bool ShouldShow()
        {
            bool? targetCasting = Utils.IsTargetCasting();
            if (targetCasting.HasValue)
            {
                return targetCasting.Value;
            }

            return true;
        }
    }

    public class FocusTargetCastbarHud : TargetCastbarHud
    {
        private TargetCastbarConfig Config => (TargetCastbarConfig)_config;

        public FocusTargetCastbarHud(TargetCastbarConfig config, string? displayName = null) : base(config, displayName)
        {

        }

        public override unsafe bool ShouldShow()
        {
            bool? focusTargetCasting = Utils.IsFocusTargetCasting();
            if (focusTargetCasting.HasValue)
            {
                return focusTargetCasting.Value;
            }

            return true;
        }
    }

    public class TargetOfTargetCastbarHud : TargetCastbarHud
    {
        private TargetCastbarConfig Config => (TargetCastbarConfig)_config;

        public TargetOfTargetCastbarHud(TargetCastbarConfig config, string? displayName = null) : base(config, displayName)
        {

        }

        public override unsafe bool ShouldShow()
        {
            IGameObject? target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            if (Actor == target)
            {
                bool? targetCasting = Utils.IsTargetCasting();
                if (targetCasting.HasValue)
                {
                    return targetCasting.Value;
                }
            }

            IGameObject? focusTarget = Plugin.TargetManager.FocusTarget;
            if (Actor == focusTarget)
            {
                bool? focusTargetCasting = Utils.IsFocusTargetCasting();
                if (focusTargetCasting.HasValue)
                {
                    return focusTargetCasting.Value;
                }
            }

            return true;
        }
    }

    public class EnemyListCastbarHud : TargetCastbarHud
    {
        private EnemyListCastbarConfig Config => (EnemyListCastbarConfig)_config;

        public int EnemyListIndex = 0;

        public EnemyListCastbarHud(EnemyListCastbarConfig config, string? displayName = null) : base(config, displayName)
        {

        }

        public override unsafe bool ShouldShow()
        {
            bool? casting = Utils.IsEnemyInListCasting(EnemyListIndex);
            if (casting.HasValue)
            {
                return casting.Value;
            }

            return true;
        }
    }

    public class NameplateCastbarHud : TargetOfTargetCastbarHud
    {
        private NameplateCastbarConfig Config => (NameplateCastbarConfig)_config;

        private Vector2 _customSize = new Vector2(0);
        public Vector2 ParentSize { get; set; } = new Vector2(0);

        public NameplateCastbarHud(NameplateCastbarConfig config, string? displayName = null) : base(config, displayName)
        {
            _customSize = Config.Size;
        }

        public override void DrawChildren(Vector2 origin)
        {
            // calculate size
            float x = Config.MatchWidth ? ParentSize.X : Config.Size.X;
            float y = Config.MatchHeight ? ParentSize.Y : Config.Size.Y;
            _customSize = new Vector2(x, y);

            // draw
            base.DrawChildren(origin);
        }

        public override Vector2 GetSize() => _customSize;
    }
}
