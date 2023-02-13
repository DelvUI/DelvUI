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

        protected List<(StrataLevel, Action)> GetMainLabelDrawActions(NameplateData data, NameplateAnchor? barAnchor = null)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();
            Vector2 origin = barAnchor?.Position ?? data.ScreenPosition;

            Vector2 swapOffset = Vector2.Zero;
            if (_config.SwapLabelsWhenNeeded && (data.IsTitlePrefix || data.Title.Length == 0))
            {
                swapOffset = _config.TitleLabelConfig.Position - _config.NameLabelConfig.Position;
            }

            // name
            drawActions.Add((_config.NameLabelConfig.StrataLevel, () =>
            {
                _nameLabelHud.Draw(data.ScreenPosition + swapOffset, barAnchor?.Size, data.GameObject, data.Name, isPlayerName: data.Kind == ObjectKind.Player);
            }
            ));

            // title
            if (data.Title.Length > 0)
            {
                drawActions.Add((_config.TitleLabelConfig.StrataLevel, () =>
                {
                    _titleLabelHud.Draw(data.ScreenPosition - swapOffset, barAnchor?.Size, data.GameObject, title: data.Title);
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

            NameplateAnchor? barAnchor = GetBarAnchor(data);
            drawActions.AddRange(GetMainLabelDrawActions(data, barAnchor));

            return drawActions;
        }

        protected virtual NameplateAnchor? GetBarAnchor(NameplateData data)
        {
            if (data.GameObject is Character chara &&
                BarConfig.IsVisible(chara.CurrentHp, chara.MaxHp))
            {
                Vector2 pos = Utils.GetAnchoredPosition(data.ScreenPosition + BarConfig.Position, BarConfig.Size, BarConfig.Anchor);
                Vector2 size = BarConfig.Size;

                return new NameplateAnchor(pos, size);
            }

            return null;
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

            NameplateAnchor? barAnchor = GetBarAnchor(data);
            Vector2 origin = barAnchor?.Position ?? data.ScreenPosition;

            Vector2 swapOffset = Vector2.Zero;
            if (_config.SwapLabelsWhenNeeded && (data.IsTitlePrefix || data.Title.Length == 0))
            {
                swapOffset = _config.TitleLabelConfig.Position - _config.NameLabelConfig.Position;
            }

            // name
            var (nameText, namePos, nameSize, nameColor) = _nameLabelHud.PreCalculate(origin + swapOffset, barAnchor?.Size, data.GameObject, data.Name, isPlayerName: data.Kind == ObjectKind.Player);
            drawActions.Add((_config.NameLabelConfig.StrataLevel, () =>
            {
                _nameLabelHud.DrawLabel(nameText, namePos, nameSize, nameColor);
            }
            ));

            // title
            var (titleText, titlePos, titleSize, titleColor) = _titleLabelHud.PreCalculate(origin - swapOffset, barAnchor?.Size, data.GameObject, title: data.Title);
            if (data.Title.Length > 0)
            {
                drawActions.Add((_config.TitleLabelConfig.StrataLevel, () =>
                {
                    _titleLabelHud.DrawLabel(titleText, titlePos, titleSize, titleColor);
                }
                ));
            }

            // extras anchor
            NameplateExtrasAnchors extrasAnchors = new NameplateExtrasAnchors(
                barAnchor,
                _config.NameLabelConfig.Enabled ? new NameplateAnchor(namePos, nameSize) : null,
                _config.TitleLabelConfig.Enabled && data.Title.Length > 0 ? new NameplateAnchor(titlePos, titleSize) : null
            );

            drawActions.AddRange(GetExtrasDrawActions(data, extrasAnchors));

            return drawActions;
        }

        protected virtual List<(StrataLevel, Action)> GetExtrasDrawActions(NameplateData data, NameplateExtrasAnchors anchors)
        {
            // override
            return new List<(StrataLevel, Action)>();
        }
    }

    public class NameplateWithPlayerBar : NameplateWithBarAndExtras
    {
        private NameplateWithPlayerBarConfig Config => (NameplateWithPlayerBarConfig)_config;

        public NameplateWithPlayerBar(NameplateWithPlayerBarConfig config) : base(config)
        {
        }

        protected override List<(StrataLevel, Action)> GetExtrasDrawActions(NameplateData data, NameplateExtrasAnchors anchors)
        {
            List<(StrataLevel, Action)> drawActions = new List<(StrataLevel, Action)>();
            if (data.GameObject is not Character character) { return drawActions; }

            // role/job icon
            if (Config.RoleIconConfig.Enabled && character is PlayerCharacter)
            {
                NameplateAnchor? anchor = anchors.GetAnchor(Config.RoleIconConfig.NameplateLabelAnchor, Config.RoleIconConfig.PrioritizeHealthBarAnchor);
                anchor = anchor ?? new NameplateAnchor(data.ScreenPosition, Vector2.Zero);

                uint jobId = character.ClassJob.Id;
                uint iconId = Config.RoleIconConfig.UseRoleIcons ?
                        JobsHelper.RoleIconIDForJob(jobId, Config.RoleIconConfig.UseSpecificDPSRoleIcons) :
                        JobsHelper.IconIDForJob(jobId, (uint)Config.RoleIconConfig.Style);

                if (iconId > 0)
                {
                    var pos = Utils.GetAnchoredPosition(anchor.Value.Position, -anchor.Value.Size, Config.RoleIconConfig.FrameAnchor);
                    var iconPos = Utils.GetAnchoredPosition(pos + Config.RoleIconConfig.Position, Config.RoleIconConfig.Size, Config.RoleIconConfig.Anchor);

                    drawActions.Add((Config.RoleIconConfig.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(_config.ID + "_jobIcon", iconPos, Config.RoleIconConfig.Size, false, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon(iconId, iconPos, Config.RoleIconConfig.Size, false, drawList);
                        });
                    }
                    ));
                }
            }

            // state icon
            if (Config.StateIconConfig.Enabled && character is PlayerCharacter)
            {
                NameplateAnchor? anchor = anchors.GetAnchor(Config.StateIconConfig.NameplateLabelAnchor, Config.StateIconConfig.PrioritizeHealthBarAnchor);
                anchor = anchor ?? new NameplateAnchor(data.ScreenPosition, Vector2.Zero);

                if (data.NamePlateIconId > 0)
                {
                    var pos = Utils.GetAnchoredPosition(anchor.Value.Position, -anchor.Value.Size, Config.StateIconConfig.FrameAnchor);
                    var iconPos = Utils.GetAnchoredPosition(pos + Config.StateIconConfig.Position, Config.StateIconConfig.Size, Config.StateIconConfig.Anchor);

                    drawActions.Add((Config.StateIconConfig.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(_config.ID + "_stateIcon", iconPos, Config.StateIconConfig.Size, false, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon((uint)data.NamePlateIconId, iconPos, Config.StateIconConfig.Size, false, drawList);
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

    #region utils
    public struct NameplateAnchor
    {
        public Vector2 Position;
        public Vector2 Size;

        internal NameplateAnchor(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }
    }

    public struct NameplateExtrasAnchors
    {
        public NameplateAnchor? BarAnchor;
        public NameplateAnchor? NameLabelAnchor;
        public NameplateAnchor? TitleLabelAnchor;
        public NameplateAnchor? HighestLabelAnchor;
        public NameplateAnchor? LowestLabelAnchor;
        private NameplateAnchor? DefaultLabelAnchor;

        internal NameplateExtrasAnchors(NameplateAnchor? barAnchor, NameplateAnchor? nameLabelAnchor, NameplateAnchor? titleLabelAnchor)
        {
            BarAnchor = barAnchor;
            NameLabelAnchor = nameLabelAnchor;
            TitleLabelAnchor = titleLabelAnchor;
            DefaultLabelAnchor = nameLabelAnchor;

            float nameY = -1;
            if (nameLabelAnchor.HasValue)
            {
                nameY = nameLabelAnchor.Value.Position.Y;
            }

            float titleY = -1;
            if (titleLabelAnchor.HasValue)
            {
                titleY = titleLabelAnchor.Value.Position.Y;
            }

            if (nameY == -1)
            {
                DefaultLabelAnchor = titleLabelAnchor;
            }            
            else if (nameY < titleY)
            {
                HighestLabelAnchor = nameLabelAnchor;
                LowestLabelAnchor = titleLabelAnchor;
            }
            else if (nameY > titleY)
            {
                HighestLabelAnchor = titleLabelAnchor;
                LowestLabelAnchor = nameLabelAnchor;
            }
        }

        internal NameplateAnchor? GetAnchor(NameplateLabelAnchor label, bool prioritizeHealthBar)
        {
            if (prioritizeHealthBar && BarAnchor != null) { return BarAnchor; }

            NameplateAnchor? labelAnchor = null;

            switch(label)
            {
                case NameplateLabelAnchor.Name: labelAnchor = NameLabelAnchor; break;
                case NameplateLabelAnchor.Title: labelAnchor = TitleLabelAnchor; break;
                case NameplateLabelAnchor.Highest: labelAnchor = HighestLabelAnchor; break;
                case NameplateLabelAnchor.Lowest: labelAnchor = LowestLabelAnchor; break;
            }

            return labelAnchor ?? DefaultLabelAnchor;
        }
        #endregion
    }
}
