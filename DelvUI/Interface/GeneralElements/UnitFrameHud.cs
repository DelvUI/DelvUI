using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Enums;

namespace DelvUI.Interface.GeneralElements
{
    public unsafe class UnitFrameHud : DraggableHudElement, IHudElementWithActor, IHudElementWithMouseOver, IHudElementWithPreview
    {
        public UnitFrameConfig Config => (UnitFrameConfig)_config;

        private readonly OpenContextMenuFromTarget _openContextMenuFromTarget;

        private SmoothHPHelper _smoothHPHelper = new SmoothHPHelper();

        public GameObject? Actor { get; set; }

        private bool _wasHovering = false;

        public UnitFrameHud(UnitFrameConfig config, string displayName) : base(config, displayName)
        {
            // interaction stuff

            /*
             Part of openContextMenuFromTarget disassembly signature
            .text:00007FF648523940                   Client__UI__Agent__AgentHUD_OpenContextMenuFromTarget proc near
            .text:00007FF648523940
            .text:00007FF648523940                   arg_0= qword ptr  8
            .text:00007FF648523940                   arg_8= qword ptr  10h
            .text:00007FF648523940
            .text:00007FF648523940 48 85 D2          test    rdx, rdx
            .text:00007FF648523943 74 7F             jz      short locret_7FF6485239C4
            */
            _openContextMenuFromTarget =
                Marshal.GetDelegateForFunctionPointer<OpenContextMenuFromTarget>(Plugin.SigScanner.ScanText("48 85 D2 74 7F 48 89 5C 24"));
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public void StopPreview()
        {
            Config.MouseoverAreaConfig.Preview = false;
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
            if (ImGui.IsMouseHoveringRect(areaStart, areaEnd) && !DraggingEnabled)
            {
                InputsHelper.Instance.SetTarget(Actor);
                _wasHovering = true;

                if (InputsHelper.Instance.LeftButtonClicked)
                {
                    Plugin.TargetManager.SetTarget(Actor);
                }
                else if (InputsHelper.Instance.RightButtonClicked)
                {
                    var agentHud = new IntPtr(Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalID(4));
                    _openContextMenuFromTarget(agentHud, Actor.Address);
                }
            }
            else if (_wasHovering)
            {
                InputsHelper.Instance.ClearTarget();
                _wasHovering = false;
            }
        }

        protected virtual void DrawExtras(Vector2 origin, GameObject? actor)
        {
            // override
        }

        private void DrawCharacter(Vector2 pos, Character character)
        {
            uint currentHp = character.CurrentHp;
            uint maxHp = character.MaxHp;

            if (Config.SmoothHealthConfig.Enabled)
            {
                currentHp = _smoothHPHelper.GetNextHp((int)currentHp, (int)maxHp, Config.SmoothHealthConfig.Velocity);
            }

            PluginConfigColor fillColor = GetColor(character, currentHp, maxHp);
            Rect background = new Rect(Config.Position, Config.Size, BackgroundColor(character));
            if (Config.RangeConfig.Enabled || Config.EnemyRangeConfig.Enabled)
            {
                fillColor = GetDistanceColor(character, fillColor);
                background.Color = GetDistanceColor(character, background.Color);
            }

            Rect healthFill = BarUtilities.GetFillRect(Config.Position, Config.Size, Config.FillDirection, fillColor, currentHp, maxHp);

            BarHud bar = new BarHud(Config, character);
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
                    ? GlobalColors.Instance.SafeColorForJobId(character!.ClassJob.Id)
                    : Config.UseRoleColorAsMissingHealthColor && character is BattleChara
                        ? GlobalColors.Instance.SafeRoleColorForJobId(character!.ClassJob.Id)
                        : Config.HealthMissingColor;

                if (Config.UseDeathIndicatorBackgroundColor && character is BattleChara { CurrentHp: <= 0 })
                {
                    missingHealthColor = Config.DeathIndicatorBackgroundColor;
                }

                if (Config.UseCustomInvulnerabilityColor && character is BattleChara battleChara)
                {
                    Status? tankInvuln = Utils.GetTankInvulnerabilityID(battleChara);
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

            if (Config.ShieldConfig.Enabled)
            {
                float shield = Utils.ActorShieldValue(Actor);
                if (shield > 0f)
                {
                    bar.AddForegrounds(
                        BarUtilities.GetShieldForeground(
                            Config.ShieldConfig,
                            Config.Position,
                            Config.Size,
                            healthFill.Size,
                            Config.FillDirection,
                            shield,
                            character.CurrentHp,
                            character.MaxHp)
                    );
                }
            }

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
            if (Config.RoleIconConfig.Enabled && character is PlayerCharacter)
            {
                uint jobId = character.ClassJob.Id;
                uint iconId = Config.RoleIconConfig.UseRoleIcons ?
                        JobsHelper.RoleIconIDForJob(jobId, Config.RoleIconConfig.UseSpecificDPSRoleIcons) :
                        JobsHelper.IconIDForJob(jobId) + (uint)Config.RoleIconConfig.Style * 100;

                if (iconId > 0)
                {
                    var barPos = Utils.GetAnchoredPosition(pos, Config.Size, Config.Anchor);
                    var parentPos = Utils.GetAnchoredPosition(barPos + Config.Position, -Config.Size, Config.RoleIconConfig.FrameAnchor);
                    var iconPos = Utils.GetAnchoredPosition(parentPos + Config.RoleIconConfig.Position, Config.RoleIconConfig.Size, Config.RoleIconConfig.Anchor);

                    AddDrawAction(Config.RoleIconConfig.StrataLevel, () =>
                    {
                        DrawHelper.DrawInWindow(ID + "_jobIcon", iconPos, Config.RoleIconConfig.Size, false, false, (drawList) =>
                        {
                            DrawHelper.DrawIcon(iconId, iconPos, Config.RoleIconConfig.Size, false, drawList);
                        });
                    });
                }
            }
        }

        private LabelConfig[] GetLabels(uint maxHp)
        {
            List<LabelConfig> labels = new List<LabelConfig>();

            if (Config.HideHealthIfPossible)
            {
                bool isHealthLabel = IsHealthLabel(Config.LeftLabelConfig);
                if (!isHealthLabel || maxHp > 0)
                {
                    labels.Add(Config.LeftLabelConfig);
                }

                isHealthLabel = IsHealthLabel(Config.RightLabelConfig);
                if (!isHealthLabel || maxHp > 0)
                {
                    labels.Add(Config.RightLabelConfig);
                }

                isHealthLabel = IsHealthLabel(Config.OptionalLabelConfig);
                if (!isHealthLabel || maxHp > 0)
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

        private bool IsHealthLabel(LabelConfig config)
        {
            return config.GetText().Contains("[health");
        }

        private PluginConfigColor GetColor(GameObject? actor, uint currentHp = 0, uint maxHp = 0)
        {
            Character? character = actor as Character;

            if (Config.UseJobColor && character != null)
            {
                return Utils.ColorForActor(character);
            }
            else if (Config.UseRoleColor)
            {
                return character is PlayerCharacter ?
                    GlobalColors.Instance.SafeRoleColorForJobId(character.ClassJob.Id) :
                    Utils.ColorForActor(character);
            }
            else if (Config.ColorByHealth.Enabled && character != null)
            {
                var scale = (float)currentHp / Math.Max(1, maxHp);
                if (Config.ColorByHealth.UseJobColorAsMaxHealth)
                {
                    return Utils.GetColorByScale(scale, Config.ColorByHealth.LowHealthColorThreshold / 100f, Config.ColorByHealth.FullHealthColorThreshold / 100f, Config.ColorByHealth.LowHealthColor, Config.ColorByHealth.FullHealthColor, Utils.ColorForActor(character), Config.ColorByHealth.UseMaxHealthColor, Config.ColorByHealth.BlendMode);
                }
                else if (Config.ColorByHealth.UseRoleColorAsMaxHealth)
                {
                    return Utils.GetColorByScale(scale, Config.ColorByHealth.LowHealthColorThreshold / 100f, Config.ColorByHealth.FullHealthColorThreshold / 100f, Config.ColorByHealth.LowHealthColor, Config.ColorByHealth.FullHealthColor, character is PlayerCharacter ? GlobalColors.Instance.SafeRoleColorForJobId(character.ClassJob.Id) : Utils.ColorForActor(character), Config.ColorByHealth.UseMaxHealthColor, Config.ColorByHealth.BlendMode);
                }
                return Utils.GetColorByScale(scale, Config.ColorByHealth);
            }
            return Config.FillColor;
        }

        private PluginConfigColor GetDistanceColor(Character? character, PluginConfigColor color)
        {
            byte distance = character != null ? character.YalmDistanceX : byte.MaxValue;
            float currentAlpha = color.Vector.W * 100f;
            float alpha = Config.RangeConfig.AlphaForDistance(distance, currentAlpha) / 100f;

            if (character is BattleNpc { BattleNpcKind: BattleNpcSubKind.Enemy } && Config.EnemyRangeConfig.Enabled)
            {
                alpha = Config.EnemyRangeConfig.AlphaForDistance(distance, currentAlpha) / 100f;
            }

            return new PluginConfigColor(color.Vector.WithNewAlpha(alpha));
        }



        private void DrawFriendlyNPC(Vector2 pos, GameObject? actor)
        {
            var bar = new BarHud(Config, actor);
            bar.AddForegrounds(new Rect(Config.Position, Config.Size, GetColor(actor)));
            bar.AddLabels(GetLabels(0));
            bar.Draw(pos);
        }

        private PluginConfigColor BackgroundColor(Character? chara)
        {
            if (Config.ShowTankInvulnerability &&
                !Config.UseMissingHealthBar &&
                chara is BattleChara battleChara)
            {
                Status? tankInvuln = Utils.GetTankInvulnerabilityID(battleChara);

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
                        color = new PluginConfigColor(GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.Id).Vector.AdjustColor(-.8f));
                    }

                    return color;
                }
            }

            if (chara is BattleChara)
            {
                if (Config.UseJobColorAsBackgroundColor)
                {
                    return GlobalColors.Instance.SafeColorForJobId(chara.ClassJob.Id);
                }
                else if (Config.UseRoleColorAsBackgroundColor)
                {
                    return GlobalColors.Instance.SafeRoleColorForJobId(chara.ClassJob.Id);
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

        private delegate void OpenContextMenuFromTarget(IntPtr agentHud, IntPtr gameObject);
    }

    public class PlayerUnitFrameHud : UnitFrameHud
    {
        public new PlayerUnitFrameConfig Config => (PlayerUnitFrameConfig)_config;

        public PlayerUnitFrameHud(PlayerUnitFrameConfig config, string displayName) : base(config, displayName)
        {

        }

        protected override void DrawExtras(Vector2 origin, GameObject? actor)
        {
            TankStanceIndicatorConfig config = Config.TankStanceIndicatorConfig;

            if (!config.Enabled || actor is not PlayerCharacter chara) { return; }

            uint jobId = chara.ClassJob.Id;
            if (JobsHelper.RoleForJob(jobId) != JobRoles.Tank) { return; }

            var tankStanceBuff = chara.StatusList.Where(o =>
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
                DrawHelper.DrawInWindow(ID + "_TankStance", startPos, endPos - startPos, false, false, (drawList) =>
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