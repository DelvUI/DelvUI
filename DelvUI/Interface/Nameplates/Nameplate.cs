using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using System;
using System.Collections.Generic;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Action = System.Action;

namespace DelvUI.Interface.Nameplates
{
    public class Nameplate
    {
        protected NameplateConfig _config;

        protected LabelHud _nameLabelHud;
        protected LabelHud _titleLabelHud;

        public Nameplate(NameplateConfig config)
        {
            _config = config;

            _nameLabelHud = new LabelHud(config.NameLabelConfig);
            _titleLabelHud = new LabelHud(config.TitleLabelConfig);
        }

        protected bool IsVisible(GameObject? actor)
        {
            if (!_config.Enabled || actor == null) { return false; }
            if (_config.OnlyShowWhenTargeted && actor.Address != Plugin.TargetManager.Target?.Address) { return false; }

            return true;
        }

        public virtual List<(StrataLevel, Action)> GetElementsDrawActions(NameplateData data, Vector2? parentPos = null, Vector2? parentSize = null)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();
            if (!IsVisible(data.GameObject)) { return drawActions; }

            Vector2 origin = parentPos ?? data.Position;

            Vector2 swapOffset = Vector2.Zero;
            if (_config.SwapLabelsWhenNeeded && NameplatesManager.Instance?.IsTitleInFront(data.Title) == true)
            {
                swapOffset = _config.TitleLabelConfig.Position - _config.NameLabelConfig.Position;
            }

            // name
            drawActions.Add((_config.NameLabelConfig.StrataLevel, () =>
            {
                _nameLabelHud.Draw(origin + swapOffset, parentSize, null, data.Name, isPlayerName: data.Kind == ObjectKind.Player);
            }
            ));

            // title
            if (data.Title.Length > 0)
            {
                drawActions.Add((_config.TitleLabelConfig.StrataLevel, () =>
                {
                    _titleLabelHud.Draw(origin - swapOffset, parentSize, title: data.Title);
                }
                ));
            }

            return drawActions;
        }
    }

    public class NameplateWithPlayerBar : Nameplate, NameplateWithBar
    {
        private NameplateWithPlayerBarConfig Config => (NameplateWithPlayerBarConfig)_config;

        //private bool _wasHovering = false;

        public NameplateWithPlayerBar(NameplateConfig config) : base(config)
        {
        }

        public List<(StrataLevel, Action)> GetBarDrawActions(NameplateData data)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();
            if (!IsVisible(data.GameObject)) { return drawActions; }

            NameplatePlayerBarConfig config = Config.BarConfig;
            if (data.GameObject is not Character character) { return drawActions; }
            
            uint currentHp = character.CurrentHp;
            uint maxHp = character.MaxHp;

            if (!config.IsVisible(currentHp, maxHp)) { return drawActions; }
            
            PluginConfigColor fillColor = ColorUtils.ColorForCharacter(
                character,
                currentHp,
                maxHp,
                config.UseJobColor,
                config.UseRoleColor,
                config.ColorByHealth
            ) ?? config.FillColor;

            Rect background = new Rect(config.Position, config.Size, BackgroundColor(character));
            //if (Config.RangeConfig.Enabled || Config.EnemyRangeConfig.Enabled)
            //{
            //    fillColor = GetDistanceColor(character, fillColor);
            //    background.Color = GetDistanceColor(character, background.Color);
            //}

            Rect healthFill = BarUtilities.GetFillRect(config.Position, config.Size, config.FillDirection, fillColor, currentHp, maxHp);

            BarHud bar = new BarHud(config, character);
            bar.SetBackground(background);
            bar.AddForegrounds(healthFill);
            //bar.AddLabels(GetLabels(maxHp));

            //if (Config.UseMissingHealthBar)
            //{
            //    Vector2 healthMissingSize = Config.Size - BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
            //    Vector2 healthMissingPos = Config.FillDirection.IsInverted()
            //        ? Config.Position
            //        : Config.Position + BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);

            //    PluginConfigColor missingHealthColor = Config.UseJobColorAsMissingHealthColor && character is BattleChara
            //        ? GlobalColors.Instance.SafeColorForJobId(character!.ClassJob.Id)
            //        : Config.UseRoleColorAsMissingHealthColor && character is BattleChara
            //            ? GlobalColors.Instance.SafeRoleColorForJobId(character!.ClassJob.Id)
            //            : Config.HealthMissingColor;

            //    if (Config.UseDeathIndicatorBackgroundColor && character is BattleChara { CurrentHp: <= 0 })
            //    {
            //        missingHealthColor = Config.DeathIndicatorBackgroundColor;
            //    }

            //    if (Config.UseCustomInvulnerabilityColor && character is BattleChara battleChara)
            //    {
            //        Status? tankInvuln = Utils.GetTankInvulnerabilityID(battleChara);
            //        if (tankInvuln is not null)
            //        {
            //            missingHealthColor = Config.CustomInvulnerabilityColor;
            //        }
            //    }

            //    if (Config.RangeConfig.Enabled || Config.EnemyRangeConfig.Enabled)
            //    {
            //        missingHealthColor = GetDistanceColor(character, missingHealthColor);
            //    }

            //    bar.AddForegrounds(new Rect(healthMissingPos, healthMissingSize, missingHealthColor));
            //}

            if (config.ShieldConfig.Enabled)
            {
                float shield = Utils.ActorShieldValue(character);
                if (shield > 0f)
                {
                    bar.AddForegrounds(
                        BarUtilities.GetShieldForeground(
                            config.ShieldConfig,
                            config.Position,
                            config.Size,
                            healthFill.Size,
                            config.FillDirection,
                            shield,
                            character.CurrentHp,
                            character.MaxHp)
                    );
                }
            }

            drawActions.AddRange(bar.GetDrawActions(Config.Position + data.Position, Config.StrataLevel));

            return drawActions;
        }

        public override List<(StrataLevel, Action)> GetElementsDrawActions(NameplateData data, Vector2? parentPos = null, Vector2? parentSize = null)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();
            if (!_config.Enabled) { return drawActions; }

            NameplatePlayerBarConfig config = Config.BarConfig;
            Vector2? barPos = null;
            Vector2? barSize = null;

            if (data.GameObject is Character chara &&
                config.IsVisible(chara.CurrentHp, chara.MaxHp))
            {
                barPos = Utils.GetAnchoredPosition(data.Position + config.Position, config.Size, config.Anchor);
                barSize = config.Size;
            }

            drawActions.AddRange(base.GetElementsDrawActions(data, barPos, barSize));

            return drawActions;
        }

        private PluginConfigColor BackgroundColor(Character? chara)
        {
            if (Config.BarConfig.ShowTankInvulnerability &&
                !Config.BarConfig.UseMissingHealthBar &&
                chara is BattleChara battleChara)
            {
                Status? tankInvuln = Utils.GetTankInvulnerabilityID(battleChara);

                if (tankInvuln != null)
                {
                    PluginConfigColor color;
                    if (Config.BarConfig.UseCustomInvulnerabilityColor)
                    {
                        color = Config.BarConfig.CustomInvulnerabilityColor;
                    }
                    else if (tankInvuln.StatusId == 811 && Config.BarConfig.UseCustomWalkingDeadColor)
                    {
                        color = Config.BarConfig.CustomWalkingDeadColor;
                    }
                    else
                    {
                        color = new PluginConfigColor(GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.Id).Vector.AdjustColor(-.8f));
                    }

                    return color;
                }
            }

            if (chara is BattleChara)
            {
                if (Config.BarConfig.UseJobColorAsBackgroundColor)
                {
                    return GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.Id);
                }
                else if (Config.BarConfig.UseRoleColorAsBackgroundColor)
                {
                    return GlobalColors.Instance.SafeRoleColorForJobId(chara.ClassJob.Id);
                }
                else if (Config.BarConfig.UseDeathIndicatorBackgroundColor && chara.CurrentHp <= 0)
                {
                    return Config.BarConfig.DeathIndicatorBackgroundColor;
                }
                else
                {
                    return Config.BarConfig.BackgroundColor;
                }
            }

            return GlobalColors.Instance.EmptyUnitFrameColor;
        }
    }

    public interface NameplateWithBar
    {
        public List<(StrataLevel, Action)> GetBarDrawActions(NameplateData data);
    }
}
