using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using System.Collections.Generic;
using System.Numerics;
using Action = System.Action;
using Character = Dalamud.Game.ClientState.Objects.Types.Character;

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

        public virtual List<(StrataLevel, Action)> GetElementsDrawActions(NameplateData data)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();
            if (!IsVisible(data.GameObject)) { return drawActions; }

            drawActions.AddRange(GetMainLabelDrawActions(data));

            return drawActions;
        }

        protected List<(StrataLevel, Action)> GetMainLabelDrawActions(NameplateData data, Vector2? parentPos = null, Vector2? parentSize = null)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();
            Vector2 origin = parentPos ?? data.ScreenPosition;

            Vector2 swapOffset = Vector2.Zero;
            if (_config.SwapLabelsWhenNeeded && (data.IsTitlePrefix || data.Title.Length == 0))
            {
                swapOffset = _config.TitleLabelConfig.Position - _config.NameLabelConfig.Position;
            }

            // name
            drawActions.Add((_config.NameLabelConfig.StrataLevel, () =>
            {
                _nameLabelHud.Draw(data.ScreenPosition + swapOffset, parentSize, data.GameObject, data.Name, isPlayerName: data.Kind == ObjectKind.Player);
            }
            ));

            // title
            if (data.Title.Length > 0)
            {
                drawActions.Add((_config.TitleLabelConfig.StrataLevel, () =>
                {
                    _titleLabelHud.Draw(data.ScreenPosition - swapOffset, parentSize, data.GameObject, title: data.Title);
                }
                ));
            }

            return drawActions;
        }
    }

    public class NameplateWithBar : Nameplate
    {
        protected NameplateBarConfig BarConfig => ((NameplateWithBarConfig)_config).GetBarConfig();

        //private bool _wasHovering = false;

        public NameplateWithBar(NameplateConfig config) : base(config)
        {
        }

        public List<(StrataLevel, Action)> GetBarDrawActions(NameplateData data)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();
            if (!IsVisible(data.GameObject)) { return drawActions; }

            if (data.GameObject is not Character character) { return drawActions; }

            uint currentHp = character.CurrentHp;
            uint maxHp = character.MaxHp;

            if (!BarConfig.IsVisible(currentHp, maxHp)) { return drawActions; }

            // colors
            PluginConfigColor fillColor = GetFillColor(character, currentHp, maxHp);
            PluginConfigColor bgColor = GetFillColor(character, currentHp, maxHp);

            //if (Config.RangeConfig.Enabled || Config.EnemyRangeConfig.Enabled)
            //{
            //    fillColor = GetDistanceColor(character, fillColor);
            //    background.Color = GetDistanceColor(character, background.Color);
            //}


            // bar
            Rect background = new Rect(BarConfig.Position, BarConfig.Size, bgColor);
            Rect healthFill = BarUtilities.GetFillRect(BarConfig.Position, BarConfig.Size, BarConfig.FillDirection, fillColor, currentHp, maxHp);

            BarHud bar = new BarHud(BarConfig, character);
            bar.SetBackground(background);
            bar.AddForegrounds(healthFill);
            bar.AddLabels(GetLabels(maxHp));

            // shield
            BarUtilities.AddShield(bar, BarConfig, BarConfig.ShieldConfig, character, healthFill.Size);

            Vector2 origin = _config.Position + data.ScreenPosition;
            drawActions.AddRange(bar.GetDrawActions(origin, _config.StrataLevel));

            // mouseover area
            BarHud? mouseoverAreaBar = BarConfig.MouseoverAreaConfig.GetBar(
                BarConfig.Position,
                BarConfig.Size,
                BarConfig.ID + "_mouseoverArea",
                BarConfig.Anchor
            );

            if (mouseoverAreaBar != null)
            {
                drawActions.AddRange(mouseoverAreaBar.GetDrawActions(origin, StrataLevel.HIGHEST));
            }

            return drawActions;
        }

        public override List<(StrataLevel, Action)> GetElementsDrawActions(NameplateData data)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();
            if (!IsVisible(data.GameObject)) { return drawActions; }

            var (barPos, barSize) = GetBarAnchorFrame(data);
            drawActions.AddRange(GetMainLabelDrawActions(data, barPos, barSize));

            return drawActions;
        }

        protected virtual (Vector2?, Vector2?) GetBarAnchorFrame(NameplateData data)
        {
            Vector2? pos = null;
            Vector2? size = null;

            if (data.GameObject is Character chara &&
                BarConfig.IsVisible(chara.CurrentHp, chara.MaxHp))
            {
                pos = Utils.GetAnchoredPosition(data.ScreenPosition + BarConfig.Position, BarConfig.Size, BarConfig.Anchor);
                size = BarConfig.Size;
            }

            return (pos, size);
        }

        private LabelConfig[] GetLabels(uint maxHp)
        {
            List<LabelConfig> labels = new List<LabelConfig>();

            if (BarConfig.HideHealthIfPossible && maxHp <= 0)
            {
                if (!Utils.IsHealthLabel(BarConfig.LeftLabelConfig))
                {
                    labels.Add(BarConfig.LeftLabelConfig);
                }

                if (!Utils.IsHealthLabel(BarConfig.RightLabelConfig))
                {
                    labels.Add(BarConfig.RightLabelConfig);
                }

                if (!Utils.IsHealthLabel(BarConfig.OptionalLabelConfig))
                {
                    labels.Add(BarConfig.OptionalLabelConfig);
                }
            }
            else
            {
                labels.Add(BarConfig.LeftLabelConfig);
                labels.Add(BarConfig.RightLabelConfig);
                labels.Add(BarConfig.OptionalLabelConfig);
            }

            return labels.ToArray();
        }

        protected virtual PluginConfigColor GetFillColor(Character character, uint currentHp, uint maxHp)
        {
            return ColorUtils.ColorForCharacter(
                character,
                currentHp,
                maxHp,
                false,
                false,
                BarConfig.ColorByHealth
            ) ?? BarConfig.FillColor;
        }

        protected virtual PluginConfigColor GetBackgroundColor(Character character)
        {
            return BarConfig.BackgroundColor;
        }
    }

    public class NameplateWithBarAndExtras : NameplateWithBar
    {
        public NameplateWithBarAndExtras(NameplateConfig config) : base(config)
        {
        }


        public override List<(StrataLevel, Action)> GetElementsDrawActions(NameplateData data)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();
            if (!IsVisible(data.GameObject)) { return drawActions; }

            var (barPos, barSize) = GetBarAnchorFrame(data);
            Vector2 origin = barPos ?? data.ScreenPosition;

            Vector2 swapOffset = Vector2.Zero;
            bool swapped = false;
            if (_config.SwapLabelsWhenNeeded && (data.IsTitlePrefix || data.Title.Length == 0))
            {
                swapOffset = _config.TitleLabelConfig.Position - _config.NameLabelConfig.Position;
                swapped = data.Title.Length > 0;
            }

            // name
            var (nameText, namePos, nameSize, nameColor) = _nameLabelHud.PreCalculate(origin + swapOffset, barSize, data.GameObject, data.Name, isPlayerName: data.Kind == ObjectKind.Player);
            drawActions.Add((_config.NameLabelConfig.StrataLevel, () =>
            {
                _nameLabelHud.DrawLabel(nameText, namePos, nameSize, nameColor);
            }
            ));

            // title
            var (titleText, titlePos, titleSize, titleColor) = _titleLabelHud.PreCalculate(origin - swapOffset, barSize, data.GameObject, title: data.Title);
            if (data.Title.Length > 0)
            {
                drawActions.Add((_config.TitleLabelConfig.StrataLevel, () =>
                {
                    _titleLabelHud.DrawLabel(titleText, titlePos, titleSize, titleColor);
                }
                ));
            }

            // extras
            Vector2 parentPos = barPos ?? (swapped ? titlePos : namePos);
            Vector2 parentSize = barSize ?? (swapped ? titleSize : nameSize);
            drawActions.AddRange(GetExtrasDrawActions(data, parentPos, parentSize));

            return drawActions;
        }

        protected virtual List<(StrataLevel, Action)> GetExtrasDrawActions(NameplateData data, Vector2 parentPos, Vector2 parentSize)
        {
            // override
            return new List<(StrataLevel, Action)>();
        }
    }

    public class NameplateWithPlayerBar : NameplateWithBarAndExtras
    {
        public NameplateWithPlayerBar(NameplateWithPlayerBarConfig config) : base(config)
        {
        }

        protected override List<(StrataLevel, Action)> GetExtrasDrawActions(NameplateData data, Vector2 parentPos, Vector2 parentSize)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();
            if (data.GameObject is not Character character) { return drawActions; }

            NameplatePlayerBarConfig config = (NameplatePlayerBarConfig)BarConfig;

            // role/job icon
            if (config.RoleIconConfig.Enabled && character is PlayerCharacter)
            {
                uint jobId = character.ClassJob.Id;
                uint iconId = config.RoleIconConfig.UseRoleIcons ?
                        JobsHelper.RoleIconIDForJob(jobId, config.RoleIconConfig.UseSpecificDPSRoleIcons) :
                        JobsHelper.IconIDForJob(jobId) + (uint)config.RoleIconConfig.Style * 100;

                if (iconId > 0)
                {
                    var pos = Utils.GetAnchoredPosition(parentPos, -parentSize, config.RoleIconConfig.FrameAnchor);
                    var iconPos = Utils.GetAnchoredPosition(pos + config.RoleIconConfig.Position, config.RoleIconConfig.Size, config.RoleIconConfig.Anchor);

                    drawActions.Add((config.RoleIconConfig.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(_config.ID + "_jobIcon", iconPos, config.RoleIconConfig.Size, false, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon(iconId, iconPos, config.RoleIconConfig.Size, false, drawList);
                        });
                    }
                    ));
                }
            }

            // state icon
            if (config.StateIconConfig.Enabled && character is PlayerCharacter)
            {
                if (data.NamePlateIconId > 0)
                {
                    var pos = Utils.GetAnchoredPosition(parentPos, -parentSize, config.StateIconConfig.FrameAnchor);
                    var iconPos = Utils.GetAnchoredPosition(pos + config.StateIconConfig.Position, config.StateIconConfig.Size, config.StateIconConfig.Anchor);

                    drawActions.Add((config.StateIconConfig.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(_config.ID + "_stateIcon", iconPos, config.StateIconConfig.Size, false, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon((uint)data.NamePlateIconId, iconPos, config.StateIconConfig.Size, false, drawList);
                        });
                    }
                    ));
                }
            }

            return drawActions;
        }

        protected override PluginConfigColor GetFillColor(Character character, uint currentHp, uint maxHp)
        {
            NameplatePlayerBarConfig config = (NameplatePlayerBarConfig)BarConfig;

            return ColorUtils.ColorForCharacter(
                character,
                currentHp,
                maxHp,
                config.UseJobColor,
                config.UseRoleColor,
                config.ColorByHealth
            ) ?? config.FillColor;
        }

        protected override PluginConfigColor GetBackgroundColor(Character character)
        {
            NameplatePlayerBarConfig config = (NameplatePlayerBarConfig)BarConfig;

            if (config.UseJobColorAsBackgroundColor)
            {
                return GlobalColors.Instance.SafeColorForJobId(character.ClassJob.Id);
            }
            else if (config.UseRoleColorAsBackgroundColor)
            {
                return GlobalColors.Instance.SafeRoleColorForJobId(character.ClassJob.Id);
            }

            return config.BackgroundColor;
        }
    }
}
