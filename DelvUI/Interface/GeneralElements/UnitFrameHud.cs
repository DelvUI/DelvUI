using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using BattleChara = Dalamud.Game.ClientState.Objects.Types.IBattleChara;
using BattleNpcSubKind = Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind;
using Character = Dalamud.Game.ClientState.Objects.Types.ICharacter;

namespace DelvUI.Interface.GeneralElements
{
    public unsafe class UnitFrameHud(UnitFrameConfig config, string displayName)
        : DraggableHudElement(config, displayName), IHudElementWithActor, IHudElementWithMouseOver, IHudElementWithPreview, IHudElementWithVisibilityConfig
    {
        public UnitFrameConfig Config => (UnitFrameConfig)_config;
        public VisibilityConfig VisibilityConfig => Config.VisibilityConfig;

        private SmoothHPHelper _smoothHPHelper = new SmoothHPHelper();

        public IGameObject? Actor { get; set; }

        private bool _wasHovering = false;

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public void StopPreview()
        {
            Config.MouseoverAreaConfig.Preview = false;
            Config.SignIconConfig.Preview = false;
        }

        public void StopMouseover()
        {
            if (_wasHovering)
            {
                InputsHelper.Instance.ClearTarget();
                _wasHovering = false;
            }
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || Actor == null)
            {
                StopMouseover();
                return;
            }

            DrawExtras(origin, Actor);

            if (Actor is Character character)
            {
                DrawCharacter(origin, character);
            }
            else
            {
                DrawFriendlyNPC(origin, Actor);
            }

            // Check if mouse is hovering over the box properly
            var startPos = Utils.GetAnchoredPosition(origin + Config.Position, Config.Size, Config.Anchor);
            var (areaStart, areaEnd) = Config.MouseoverAreaConfig.GetArea(startPos, Config.Size);
            bool isHovering = ImGui.IsMouseHoveringRect(areaStart, areaEnd);
            bool ignoreMouseover = Config.MouseoverAreaConfig.Enabled && Config.MouseoverAreaConfig.Ignore;

            if (isHovering && !DraggingEnabled)
            {
                _wasHovering = true;
                InputsHelper.Instance.SetTarget(Actor, ignoreMouseover);

                if (InputsHelper.Instance.LeftButtonClicked)
                {
                    Plugin.TargetManager.Target = Actor;
                }
                else if (InputsHelper.Instance.RightButtonClicked)
                {
                    AgentModule.Instance()->GetAgentHUD()->OpenContextMenuFromTarget((GameObject*)Actor.Address);
                }
            }
            else if (_wasHovering)
            {
                InputsHelper.Instance.ClearTarget();
                _wasHovering = false;
            }
        }

        protected virtual void DrawExtras(Vector2 origin, IGameObject? actor)
        {
            // override
        }

        private void DrawCharacter(Vector2 pos, Character character)
        {
            uint currentHp = character.CurrentHp;
            uint maxHp = character.MaxHp;

            // fixes weird bug with npcs
            if (maxHp == 1)
            {
                currentHp = 1;
            }
            else if (Config.SmoothHealthConfig.Enabled)
            {
                currentHp = _smoothHPHelper.GetNextHp((int)currentHp, (int)maxHp, Config.SmoothHealthConfig.Velocity);
            }

            PluginConfigColor fillColor = ColorUtils.ColorForCharacter(
                character,
                currentHp,
                maxHp,
                Config.UseJobColor,
                Config.UseRoleColor,
                Config.ColorByHealth
            ) ?? Config.FillColor;

            Rect background = new Rect(Config.Position, Config.Size, BackgroundColor(character));
            if (Config.RangeConfig.Enabled || Config.EnemyRangeConfig.Enabled)
            {
                fillColor = GetDistanceColor(character, fillColor);
                background.Color = GetDistanceColor(character, background.Color);
            }

            Rect healthFill = BarUtilities.GetFillRect(Config.Position, Config.Size, Config.FillDirection, fillColor, currentHp, maxHp);

            BarHud bar = new BarHud(Config, character);
            bar.NeedsInputs = true;
            bar.SetBackground(background);
            bar.AddForegrounds(healthFill);
            bar.AddLabels(GetLabels(maxHp));

            if (Config.UseMissingHealthBar)
            {
                Vector2 healthMissingSize = Config.Size - BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);
                Vector2 healthMissingPos = Config.FillDirection.IsInverted()
                    ? Config.Position
                    : Config.Position + BarUtilities.GetFillDirectionOffset(healthFill.Size, Config.FillDirection);

                PluginConfigColor missingHealthColor = Config.UseJobColorAsMissingHealthColor && character is BattleChara
                    ? GlobalColors.Instance.SafeColorForJobId(character!.ClassJob.RowId)
                    : Config.UseRoleColorAsMissingHealthColor && character is BattleChara
                        ? GlobalColors.Instance.SafeRoleColorForJobId(character!.ClassJob.RowId)
                        : Config.HealthMissingColor;

                if (Config.UseDeathIndicatorBackgroundColor && character is BattleChara { CurrentHp: <= 0 })
                {
                    missingHealthColor = Config.DeathIndicatorBackgroundColor;
                }

                if (Config.UseCustomInvulnerabilityColor && character is BattleChara battleChara)
                {
                    IStatus? tankInvuln = Utils.GetTankInvulnerabilityID(battleChara);
                    if (tankInvuln is not null)
                    {
                        missingHealthColor = Config.CustomInvulnerabilityColor;
                    }
                }

                if (Config.RangeConfig.Enabled || Config.EnemyRangeConfig.Enabled)
                {
                    missingHealthColor = GetDistanceColor(character, missingHealthColor);
                }

                bar.AddForegrounds(new Rect(healthMissingPos, healthMissingSize, missingHealthColor));
            }

            // shield
            BarUtilities.AddShield(bar, Config, Config.ShieldConfig, character, healthFill.Size);

            // draw action
            AddDrawActions(bar.GetDrawActions(pos, Config.StrataLevel));

            // mouseover area
            BarHud? mouseoverAreaBar = Config.MouseoverAreaConfig.GetBar(
                Config.Position,
                Config.Size,
                Config.ID + "_mouseoverArea",
                Config.Anchor
            );

            if (mouseoverAreaBar != null)
            {
                AddDrawActions(mouseoverAreaBar.GetDrawActions(pos, StrataLevel.HIGHEST));
            }

            // role/job icon
            if (Config.RoleIconConfig.Enabled && character is IPlayerCharacter)
            {
                uint jobId = character.ClassJob.RowId;
                uint iconId = Config.RoleIconConfig.UseRoleIcons ?
                        JobsHelper.RoleIconIDForJob(jobId, Config.RoleIconConfig.UseSpecificDPSRoleIcons) :
                        JobsHelper.IconIDForJob(jobId, (uint)Config.RoleIconConfig.Style);

                if (iconId > 0)
                {
                    var barPos = Utils.GetAnchoredPosition(pos, Config.Size, Config.Anchor);
                    var parentPos = Utils.GetAnchoredPosition(barPos + Config.Position, -Config.Size, Config.RoleIconConfig.FrameAnchor);
                    var iconPos = Utils.GetAnchoredPosition(parentPos + Config.RoleIconConfig.Position, Config.RoleIconConfig.Size, Config.RoleIconConfig.Anchor);

                    AddDrawAction(Config.RoleIconConfig.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(ID + "_jobIcon", iconPos, Config.RoleIconConfig.Size, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon(iconId, iconPos, Config.RoleIconConfig.Size, false, drawList);
                        });
                    });
                }
            }

            // sign icon
            if (Config.SignIconConfig.Enabled)
            {
                uint? iconId = Config.SignIconConfig.IconID(character);
                if (iconId.HasValue)
                {
                    var barPos = Utils.GetAnchoredPosition(pos, Config.Size, Config.Anchor);
                    var parentPos = Utils.GetAnchoredPosition(barPos + Config.Position, -Config.Size, Config.SignIconConfig.FrameAnchor);
                    var iconPos = Utils.GetAnchoredPosition(parentPos + Config.SignIconConfig.Position, Config.SignIconConfig.Size, Config.SignIconConfig.Anchor);

                    AddDrawAction(Config.SignIconConfig.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(ID + "_signIcon", iconPos, Config.SignIconConfig.Size, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon(iconId.Value, iconPos, Config.SignIconConfig.Size, false, drawList);
                        });
                    });
                }
            }
        }

        private LabelConfig[] GetLabels(uint maxHp)
        {
            List<LabelConfig> labels = new List<LabelConfig>();

            if (Config.HideHealthIfPossible && maxHp <= 1)
            {
                if (!Utils.IsHealthLabel(Config.LeftLabelConfig))
                {
                    labels.Add(Config.LeftLabelConfig);
                }

                if (!Utils.IsHealthLabel(Config.RightLabelConfig))
                {
                    labels.Add(Config.RightLabelConfig);
                }

                if (!Utils.IsHealthLabel(Config.OptionalLabelConfig))
                {
                    labels.Add(Config.OptionalLabelConfig);
                }
            }
            else
            {
                labels.Add(Config.LeftLabelConfig);
                labels.Add(Config.RightLabelConfig);
                labels.Add(Config.OptionalLabelConfig);
            }

            return labels.ToArray();
        }



        private PluginConfigColor GetDistanceColor(Character? character, PluginConfigColor color)
        {
            byte distance = character != null ? character.YalmDistanceX : byte.MaxValue;
            float currentAlpha = color.Vector.W * 100f;
            float alpha = Config.RangeConfig.AlphaForDistance(distance, currentAlpha) / 100f;

            if (character is IBattleNpc { BattleNpcKind: BattleNpcSubKind.Enemy or BattleNpcSubKind.BattleNpcPart } && Config.EnemyRangeConfig.Enabled)
            {
                alpha = Config.EnemyRangeConfig.AlphaForDistance(distance, currentAlpha) / 100f;
            }

            return color.WithAlpha(alpha);
        }

        private unsafe void GetNPCHpValues(IGameObject? actor, out uint currentHp, out uint maxHp)
        {
            currentHp = 0;
            maxHp = 0;

            var player = Plugin.ObjectTable.LocalPlayer;
            if (player == null || actor == null || player.TargetObject == null || actor.GameObjectId != player.TargetObject.GameObjectId)
            {
                return;
            }

            AtkUnitBase* TargetWidget = (AtkUnitBase*)Plugin.GameGui.GetAddonByName("_TargetInfoMainTarget", 1).Address;
            if (TargetWidget != null)
            {
                AtkTextNode* textNode = TargetWidget->GetTextNodeById(11);
                string integrityText = textNode->NodeText.ToString();

                // not a gathering node or node at 100%, nothing to do
                if (!integrityText.Contains("%"))
                {
                    return;
                }

                try
                {
                    currentHp = Convert.ToUInt32((integrityText.Replace("%", "")));
                    maxHp = 100;
                }
                catch { }
            }
        }

        private void DrawFriendlyNPC(Vector2 pos, IGameObject? actor)
        {
            GetNPCHpValues(actor, out uint currentHp, out uint maxHp);

            BarHud bar = new BarHud(Config, actor);
            bar.AddLabels(GetLabels(0));

            if (maxHp == 0)
            {
                bar.AddForegrounds(new Rect(Config.Position, Config.Size, ColorUtils.ColorForActor(actor)));
            }
            else
            {
                if (Config.SmoothHealthConfig.Enabled)
                {
                    currentHp = _smoothHPHelper.GetNextHp((int)currentHp, (int)maxHp, Config.SmoothHealthConfig.Velocity);
                }

                PluginConfigColor fillColor = ColorUtils.ColorForCharacter(
                    actor,
                    currentHp,
                    maxHp,
                    colorByHealthConfig: Config.ColorByHealth
                ) ?? Config.FillColor;

                Rect background = new Rect(Config.Position, Config.Size, Config.BackgroundColor);
                Rect healthFill = BarUtilities.GetFillRect(Config.Position, Config.Size, Config.FillDirection, fillColor, currentHp, maxHp);

                bar.NeedsInputs = true;
                bar.SetBackground(background);
                bar.AddForegrounds(healthFill);
            }

            AddDrawActions(bar.GetDrawActions(pos, Config.StrataLevel));
        }

        private PluginConfigColor BackgroundColor(Character? chara)
        {
            if (Config.ShowTankInvulnerability &&
                !Config.UseMissingHealthBar &&
                chara is BattleChara battleChara)
            {
                IStatus? tankInvuln = Utils.GetTankInvulnerabilityID(battleChara);

                if (tankInvuln != null)
                {
                    PluginConfigColor color;
                    if (Config.UseCustomInvulnerabilityColor)
                    {
                        color = Config.CustomInvulnerabilityColor;
                    }
                    else if (tankInvuln.StatusId == 811 && Config.UseCustomWalkingDeadColor)
                    {
                        color = Config.CustomWalkingDeadColor;
                    }
                    else
                    {
                        color = new PluginConfigColor(GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.RowId).Vector.AdjustColor(-.8f));
                    }

                    return color;
                }
            }

            if (chara is BattleChara)
            {
                if (Config.UseJobColorAsBackgroundColor)
                {
                    return GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.RowId);
                }
                else if (Config.UseRoleColorAsBackgroundColor)
                {
                    return GlobalColors.Instance.SafeRoleColorForJobId(chara.ClassJob.RowId);
                }
                else if (Config.UseDeathIndicatorBackgroundColor && chara.CurrentHp <= 0)
                {
                    return Config.DeathIndicatorBackgroundColor;
                }
                else
                {
                    return Config.BackgroundColor;
                }
            }

            return GlobalColors.Instance.EmptyUnitFrameColor;
        }
    }

    public class PlayerUnitFrameHud : UnitFrameHud
    {
        public new PlayerUnitFrameConfig Config => (PlayerUnitFrameConfig)_config;

        public PlayerUnitFrameHud(PlayerUnitFrameConfig config, string displayName) : base(config, displayName)
        {

        }

        protected override void DrawExtras(Vector2 origin, IGameObject? actor)
        {
            TankStanceIndicatorConfig config = Config.TankStanceIndicatorConfig;

            if (!config.Enabled || actor is not IPlayerCharacter chara) { return; }

            uint jobId = chara.ClassJob.RowId;
            if (JobsHelper.RoleForJob(jobId) != JobRoles.Tank) { return; }

            var tankStanceBuff = Utils.StatusListForBattleChara(chara).Where(o =>
                o.StatusId == 79 || // IRON WILL
                o.StatusId == 91 || // DEFIANCE
                o.StatusId == 392 || // ROYAL GUARD
                o.StatusId == 393 || // IRON WILL
                o.StatusId == 743 || // GRIT
                o.StatusId == 1396 || // DEFIANCE
                o.StatusId == 1397 || // GRIT
                o.StatusId == 1833    // ROYAL GUARD
            );

            PluginConfigColor color = tankStanceBuff.Any() ? config.ActiveColor : config.InactiveColor;

            Vector2 pos = GetTankStanceCornerOrigin(origin);
            var (verticalDir, horizontalDir) = GetTankStanceLinesDirections();

            pos = new Vector2(pos.X + config.Thickess * -horizontalDir, pos.Y + config.Thickess * -verticalDir);
            Vector2 vSize = new Vector2(config.Thickess * horizontalDir, (config.Size.Y + config.Thickess) * verticalDir);
            Vector2 vEndPos = pos + vSize;
            Vector2 hSize = new Vector2((config.Size.X + config.Thickess) * horizontalDir, config.Thickess * verticalDir);
            Vector2 hEndPos = pos + hSize;

            Vector2 startPos = new Vector2(Math.Min(pos.X, hEndPos.X), Math.Min(pos.Y, hEndPos.Y));
            Vector2 endPos = new Vector2(Math.Max(pos.X, hEndPos.X), Math.Max(pos.Y, hEndPos.Y)); ;

            AddDrawAction(StrataLevel.LOWEST, () =>
            {
                DrawHelper.DrawInWindow(ID + "_TankStance", startPos, endPos - startPos, false, (drawList) =>
                {
                    // TODO: clean up hacky math
                    // there's some 1px errors prob due to negative sizes
                    // couldn't figure it out so I did the hacky fixes

                    // vertical

                    drawList.AddRectFilled(pos, vEndPos, color.Base);

                    if (config.Corner == TankStanceCorner.TopRight)
                    {
                        drawList.AddLine(pos, pos + new Vector2(0, vSize.Y + 1), 0xFF000000);
                    }
                    else
                    {
                        drawList.AddLine(pos, pos + new Vector2(0, vSize.Y), 0xFF000000);
                    }

                    drawList.AddLine(pos + vSize, pos + vSize + new Vector2(-vSize.X, 0), 0xFF000000);

                    // horizontal
                    drawList.AddRectFilled(pos, hEndPos, color.Base);

                    if (config.Corner == TankStanceCorner.BottomLeft)
                    {
                        drawList.AddLine(pos, pos + new Vector2(hSize.X + 1, 0), 0xFF000000);
                    }
                    else
                    {
                        drawList.AddLine(pos, pos + new Vector2(hSize.X, 0), 0xFF000000);
                    }

                    if (config.Corner == TankStanceCorner.BottomRight)
                    {
                        drawList.AddLine(pos + new Vector2(0, 1), pos + new Vector2(0, hSize.Y), 0xFF000000);
                    }
                    else
                    {
                        drawList.AddLine(pos, pos + new Vector2(0, hSize.Y), 0xFF000000);
                    }

                    drawList.AddLine(pos + hSize, pos + hSize + new Vector2(0, -hSize.Y), 0xFF000000);
                });
            });
        }

        private Vector2 GetTankStanceCornerOrigin(Vector2 origin)
        {
            var topLeft = Utils.GetAnchoredPosition(origin + Config.Position, Config.Size, Config.Anchor);

            return Config.TankStanceIndicatorConfig.Corner switch
            {
                TankStanceCorner.TopRight => topLeft + new Vector2(Config.Size.X - 1, 0),
                TankStanceCorner.BottomLeft => topLeft + new Vector2(0, Config.Size.Y - 1),
                TankStanceCorner.BottomRight => topLeft + Config.Size - Vector2.One,
                _ => topLeft
            };
        }

        private (int, int) GetTankStanceLinesDirections()
        {
            return Config.TankStanceIndicatorConfig.Corner switch
            {
                TankStanceCorner.TopLeft => (1, 1),
                TankStanceCorner.TopRight => (1, -1),
                TankStanceCorner.BottomLeft => (-1, 1),
                _ => (-1, -1)
            };
        }
    }
}