using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;
using Dalamud.Game.Internal.Gui.Addon;
using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.StatusEffects;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public abstract class HudWindow
    {
        private readonly OpenContextMenuFromTarget _openContextMenuFromTarget;

        private readonly StatusEffectsList _playerBuffList;
        private readonly StatusEffectsList _playerDebuffList;
        private readonly StatusEffectsList _targetBuffList;
        private readonly StatusEffectsList _targetDebuffList;
        private readonly StatusEffectsList _raidJobsBuffList;
        protected readonly PluginConfiguration PluginConfiguration;
        protected readonly DalamudPluginInterface PluginInterface;

        private ImGuiWindowFlags _childFlags = 0;

        protected uint[] _raidWideBuffs = {
            // See https://external-preview.redd.it/bKacLk4PKav7vdP1ilT66gAtB1t7BTJjxsMrImRHr1k.png?auto=webp&s=cbe6880c34b45e2db20c247c8ab9eef543538e96
            // Left Eye
            1184,
            1454,
            // Battle Litany
            786,
            1414,
            // Brotherhood
            1185,
            2174,
            // Battle Voice
            141,
            // Devilment
            1825,
            // Technical Finish
            1822,
            2050,
            // Standard Finish
            1821,
            2024,
            2105,
            2113,
            // Embolden
            1239,
            1297,
            2282,
            // Devotion
            1213,
            // ------ AST Card Buffs -------
            // The Balance
            829,
            1338,
            1882,
            // The Bole
            830,
            1339,
            1883,
            // The Arrow
            831,
            1884,
            // The Spear
            832,
            1885,
            // The Ewer
            833,
            1340,
            1886,
            // The Spire
            834,
            1341,
            1887,
            // Lord of Crowns
            1451,
            1876,
            // Lady of Crowns
            1452,
            1877,
            // Divination
            1878,
            2034,
            // Chain Stratagem
            1221,
            1406,
        };
        protected List<uint> RaidWideBuffs;
        private List<uint> JobSpecificBuffs;
        protected bool ShowRaidWideBuffIcons => PluginConfiguration.ShowRaidWideBuffIcons;
        protected bool ShowJobSpecificBuffIcons => PluginConfiguration.ShowJobSpecificBuffIcons;

        private LastUsedCast _lastPlayerUsedCast;
        private LastUsedCast _lastTargetUsedCast;

        private MpTickHelper _mpTickHelper;
        public bool IsVisible = true;

        protected HudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            PluginInterface = pluginInterface;
            PluginConfiguration = pluginConfiguration;

            _childFlags |= ImGuiWindowFlags.NoTitleBar;
            _childFlags |= ImGuiWindowFlags.NoScrollbar;
            _childFlags |= ImGuiWindowFlags.AlwaysAutoResize;
            _childFlags |= ImGuiWindowFlags.NoBackground;
            _childFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus;

            _openContextMenuFromTarget =
                Marshal.GetDelegateForFunctionPointer<OpenContextMenuFromTarget>(PluginInterface.TargetModuleScanner.ScanText("48 85 D2 74 7F 48 89 5C 24"));

            RaidWideBuffs = new List<uint>(_raidWideBuffs);
            JobSpecificBuffs = GetJobSpecificBuffs();
            PluginConfiguration.ConfigChangedEvent += OnConfigChanged;

            var center = new Vector2(CenterX, CenterY);
            _playerBuffList = new StatusEffectsList(pluginInterface, pluginConfiguration.PlayerBuffListConfig) { Center = center };

            _playerDebuffList = new StatusEffectsList(pluginInterface, pluginConfiguration.PlayerDebuffListConfig) { Center = center };

            _targetBuffList = new StatusEffectsList(pluginInterface, pluginConfiguration.TargetDebuffListConfig) { Center = center };

            _targetDebuffList = new StatusEffectsList(pluginInterface, pluginConfiguration.TargetBuffListConfig) { Center = center };
            _raidJobsBuffList = new StatusEffectsList(pluginInterface, pluginConfiguration.RaidJobBuffListConfig) { Center = center };
        }

        public abstract uint JobId { get; }

        protected static float CenterX => ImGui.GetMainViewport().Size.X / 2f;
        protected static float CenterY => ImGui.GetMainViewport().Size.Y / 2f;
        protected static int XOffset => 160;
        protected static int YOffset => 460;

        protected Vector2 BarSize { get; private set; }

        private void OnConfigChanged(object sender, EventArgs args)
        {
            if (!PluginConfiguration.MPTickerEnabled)
            {
                _mpTickHelper = null;
            }
        }

        protected virtual void DrawHealthBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            BarSize = new Vector2(HealthBarWidth, HealthBarHeight);
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float)actor.CurrentHp / actor.MaxHp;

            if (PluginConfiguration.TankStanceIndicatorEnabled && actor.ClassJob.Id is 19 or 32 or 21 or 37)
            {
                DrawTankStanceIndicator();
            }

            var cursorPos = new Vector2(CenterX - HealthBarWidth - HealthBarXOffset, CenterY + HealthBarYOffset);
            ImGui.SetCursorPos(cursorPos);

            PluginConfiguration.JobColorMap.TryGetValue(PluginInterface.ClientState.LocalPlayer.ClassJob.Id, out var colors);
            colors ??= PluginConfiguration.NPCColorMap["friendly"];

            if (PluginConfiguration.CustomHealthBarColorEnabled)
            {
                colors = PluginConfiguration.MiscColorMap["customhealth"];
            }


            var drawList = ImGui.GetWindowDrawList();

            // Basically make an invisible box for BeginChild to work properly.
            ImGuiWindowFlags windowFlags = 0;
            windowFlags |= ImGuiWindowFlags.NoBackground;
            windowFlags |= ImGuiWindowFlags.NoTitleBar;
            windowFlags |= ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoDecoration;

            ImGui.SetNextWindowPos(cursorPos);
            ImGui.SetNextWindowSize(BarSize);

            ImGui.Begin("health_bar", windowFlags);

            if (ImGui.BeginChild("health_bar", BarSize))
            {
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, PlayerUnitFrameColor);

                if (HasTankInvuln(actor) == 1)
                {
                    var jobColors = PluginConfiguration.JobColorMap[PluginInterface.ClientState.LocalPlayer.ClassJob.Id];
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, jobColors["invuln"]);
                }

                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + new Vector2(HealthBarWidth * scale, HealthBarHeight),
                    colors["gradientLeft"],
                    colors["gradientRight"],
                    colors["gradientRight"],
                    colors["gradientLeft"]
                );

                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

                // Check if mouse is hovering over the box properly
                if (ImGui.GetIO().MouseClicked[0] && ImGui.IsMouseHoveringRect(cursorPos, cursorPos + BarSize))
                {
                    PluginInterface.ClientState.Targets.SetCurrentTarget(actor);
                }
            }

            ImGui.EndChild();
            ImGui.End();

            DrawTargetShield(actor, cursorPos, BarSize);

            DrawOutlinedText(
                $"{TextTags.GenerateFormattedTextFromTags(actor, PluginConfiguration.HealthBarTextLeft)}",
                new Vector2(cursorPos.X + 5 + HealthBarTextLeftXOffset, cursorPos.Y - 22 + HealthBarTextLeftYOffset)
            );

            var text = TextTags.GenerateFormattedTextFromTags(actor, PluginConfiguration.HealthBarTextRight);
            var textSize = ImGui.CalcTextSize(text);
            DrawOutlinedText(text, new Vector2(cursorPos.X + HealthBarWidth - textSize.X - 5 + HealthBarTextRightXOffset, cursorPos.Y - 22 + HealthBarTextRightYOffset));
        }

        protected virtual void DrawPrimaryResourceBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            BarSize = new Vector2(PrimaryResourceBarWidth, PrimaryResourceBarHeight);
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float)actor.CurrentMp / actor.MaxMp;
            var cursorPos = new Vector2(CenterX - PrimaryResourceBarXOffset + 33, CenterY + PrimaryResourceBarYOffset - 16);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0x88000000);
            drawList.AddRectFilledMultiColor(cursorPos, cursorPos + new Vector2(BarSize.X * scale, BarSize.Y), 0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

            if (ShowPrimaryResourceBarThresholdMarker)
            {
                // threshold
                var position = new Vector2(cursorPos.X + PrimaryResourceBarThresholdValue / 10000f * BarSize.X - 3, cursorPos.Y);
                var size = new Vector2(2, BarSize.Y);
                drawList.AddRect(position, position + size, 0xFF000000);
            }

            if (!ShowPrimaryResourceBarValue)
            {
                return;
            }

            // text
            var mana = PluginInterface.ClientState.LocalPlayer.CurrentMp;
            var text = $"{mana,0}";
            DrawOutlinedText(text, new Vector2(cursorPos.X + 2 + PrimaryResourceBarTextXOffset, cursorPos.Y - 3 + PrimaryResourceBarTextYOffset));
        }

        protected virtual void DrawTargetBar()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is null)
            {
                return;
            }

            BarSize = new Vector2(TargetBarWidth, TargetBarHeight);

            var cursorPos = new Vector2(CenterX + TargetBarXOffset, CenterY + TargetBarYOffset);
            ImGui.SetCursorPos(cursorPos);

            var drawList = ImGui.GetWindowDrawList();
            ImGuiWindowFlags windowFlags = 0;
            windowFlags |= ImGuiWindowFlags.NoBackground;
            windowFlags |= ImGuiWindowFlags.NoTitleBar;
            windowFlags |= ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoDecoration;
            windowFlags |= ImGuiWindowFlags.NoInputs;

            var addon = PluginInterface.Framework.Gui.GetAddonByName("ContextMenu", 1);

            ClipAround(
                addon,
                "target_bar",
                drawList,
                (drawListPtr, windowName) =>
                {
                    ImGui.SetNextWindowSize(BarSize);
                    ImGui.SetNextWindowPos(cursorPos);

                    ImGui.Begin(windowName, windowFlags);

                    if (addon is not { Visible: true })
                    {
                        _childFlags &= ~ImGuiWindowFlags.NoInputs;
                    }
                    else
                    {
                        if (ImGui.IsMouseHoveringRect(new Vector2(addon.X, addon.Y), new Vector2(addon.X + addon.Width, addon.Y + addon.Height)))
                        {
                            _childFlags |= ImGuiWindowFlags.NoInputs;
                        }
                        else
                        {
                            _childFlags &= ~ImGuiWindowFlags.NoInputs;
                        }
                    }

                    if (ImGui.BeginChild(windowName, BarSize, default, _childFlags))
                    {
                        if (target is not Chara actor)
                        {
                            var friendly = PluginConfiguration.NPCColorMap["friendly"];
                            drawListPtr.AddRectFilled(cursorPos, cursorPos + BarSize, ImGui.ColorConvertFloat4ToU32(PluginConfiguration.UnitFrameEmptyColor));

                            drawListPtr.AddRectFilledMultiColor(
                                cursorPos,
                                cursorPos + new Vector2(TargetBarWidth, TargetBarHeight),
                                friendly["gradientLeft"],
                                friendly["gradientRight"],
                                friendly["gradientRight"],
                                friendly["gradientLeft"]
                            );

                            drawListPtr.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                        }
                        else
                        {
                            var scale = actor.MaxHp > 0f ? (float)actor.CurrentHp / actor.MaxHp : 0f;
                            var colors = DetermineTargetPlateColors(actor);
                            drawListPtr.AddRectFilled(cursorPos, cursorPos + BarSize, UnitFrameEmptyColor);

                            if (HasTankInvuln(actor) == 1)
                            {
                                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, colors["invuln"]);
                            }

                            drawListPtr.AddRectFilledMultiColor(
                                cursorPos,
                                cursorPos + new Vector2(TargetBarWidth * scale, TargetBarHeight),
                                colors["gradientLeft"],
                                colors["gradientRight"],
                                colors["gradientRight"],
                                colors["gradientLeft"]
                            );

                            drawListPtr.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                        }

                        if (ImGui.GetIO().MouseDown[1] && ImGui.IsMouseHoveringRect(cursorPos, cursorPos + BarSize))
                        {
                            unsafe
                            {
                                var agentHud = new IntPtr(Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalID(4));
                                _openContextMenuFromTarget(agentHud, target.Address);
                            }
                        }
                    }

                    ImGui.EndChild();
                    ImGui.End();
                }
            );

            DrawTargetShield(target, cursorPos, BarSize);

            var textLeft = TextTags.GenerateFormattedTextFromTags(target, PluginConfiguration.TargetBarTextLeft);
            DrawOutlinedText(textLeft, new Vector2(cursorPos.X + 5 + TargetBarTextLeftXOffset, cursorPos.Y - 22 + TargetBarTextLeftYOffset));

            var textRight = TextTags.GenerateFormattedTextFromTags(target, PluginConfiguration.TargetBarTextRight);
            var textRightSize = ImGui.CalcTextSize(textRight);

            DrawOutlinedText(textRight, new Vector2(cursorPos.X + TargetBarWidth - textRightSize.X - 5 + TargetBarTextRightXOffset, cursorPos.Y - 22 + TargetBarTextRightYOffset));

            DrawTargetOfTargetBar(target.TargetActorID);
        }

        protected virtual void DrawFocusBar()
        {
            var focus = PluginInterface.ClientState.Targets.FocusTarget;

            if (focus is null)
            {
                return;
            }

            var barSize = new Vector2(FocusBarWidth, FocusBarHeight);

            var cursorPos = new Vector2(CenterX - FocusBarXOffset - HealthBarWidth - FocusBarWidth - 2, CenterY + FocusBarYOffset);
            ImGui.SetCursorPos(cursorPos);

            var drawList = ImGui.GetWindowDrawList();

            ImGuiWindowFlags windowFlags = 0;
            windowFlags |= ImGuiWindowFlags.NoBackground;
            windowFlags |= ImGuiWindowFlags.NoTitleBar;
            windowFlags |= ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoDecoration;

            ImGui.SetNextWindowPos(cursorPos);
            ImGui.SetNextWindowSize(barSize);

            ImGui.Begin("focus_bar", windowFlags);

            if (ImGui.BeginChild("focus_bar", BarSize))
            {
                if (focus is not Chara actor)
                {
                    var friendly = PluginConfiguration.NPCColorMap["friendly"];
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, UnitFrameEmptyColor);

                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + new Vector2(FocusBarWidth, FocusBarHeight),
                        friendly["gradientLeft"],
                        friendly["gradientRight"],
                        friendly["gradientRight"],
                        friendly["gradientLeft"]
                    );

                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    DrawTargetShield(focus, cursorPos, barSize);
                }
                else
                {
                    var colors = DetermineTargetPlateColors(actor);
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, UnitFrameEmptyColor);

                    if (HasTankInvuln(actor) == 1)
                    {
                        drawList.AddRectFilled(cursorPos, cursorPos + barSize, colors["invuln"]);
                    }

                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + new Vector2((float)FocusBarWidth * actor.CurrentHp / actor.MaxHp, FocusBarHeight),
                        colors["gradientLeft"],
                        colors["gradientRight"],
                        colors["gradientRight"],
                        colors["gradientLeft"]
                    );

                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                }

                if (ImGui.GetIO().MouseClicked[1] && ImGui.IsMouseHoveringRect(cursorPos, cursorPos + BarSize))
                {
                    unsafe
                    {
                        //PluginLog.Information();
                        var agentHud = new IntPtr(Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalID(4));

                        _openContextMenuFromTarget(agentHud, focus.Address);
                    }
                }
            }

            ImGui.EndChild();
            ImGui.End();

            DrawTargetShield(focus, cursorPos, barSize);

            var text = TextTags.GenerateFormattedTextFromTags(focus, PluginConfiguration.FocusBarText);
            var textSize = ImGui.CalcTextSize(text);
            DrawOutlinedText(text, new Vector2(cursorPos.X + FocusBarWidth / 2f - textSize.X / 2f + FocusBarTextXOffset, cursorPos.Y - 22 + FocusBarTextYOffset));
        }

        protected virtual void DrawTargetOfTargetBar(int targetActorId)
        {
            Actor target = null;

            if (targetActorId == 0 && PluginInterface.ClientState.LocalPlayer.TargetActorID == 0)
            {
                target = PluginInterface.ClientState.LocalPlayer;
            }
            else
            {
                for (var i = 0; i < 200; i += 2)
                {
                    if (PluginInterface.ClientState.Actors[i]?.ActorId == targetActorId)
                    {
                        target = PluginInterface.ClientState.Actors[i];
                    }
                }
            }

            if (target is not Chara actor)
            {
                return;
            }

            var barSize = new Vector2(ToTBarWidth, ToTBarHeight);
            var colors = DetermineTargetPlateColors(actor);
            var text = TextTags.GenerateFormattedTextFromTags(target, PluginConfiguration.ToTBarText);
            var textSize = ImGui.CalcTextSize(text);
            var cursorPos = new Vector2(CenterX + ToTBarXOffset + TargetBarWidth + 2, CenterY + ToTBarYOffset);

            ImGui.SetCursorPos(cursorPos);

            var drawList = ImGui.GetWindowDrawList();

            // Basically make an invisible box for BeginChild to work properly.
            ImGuiWindowFlags windowFlags = 0;
            windowFlags |= ImGuiWindowFlags.NoBackground;
            windowFlags |= ImGuiWindowFlags.NoTitleBar;
            windowFlags |= ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoDecoration;

            ImGui.SetNextWindowPos(cursorPos);
            ImGui.SetNextWindowSize(barSize);

            ImGui.Begin("target_of_target_bar", windowFlags);

            if (ImGui.BeginChild("target_of_target_bar", barSize))
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, UnitFrameEmptyColor);

                if (HasTankInvuln(actor) == 1)
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, colors["invuln"]);
                }

                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + new Vector2((float)ToTBarWidth * actor.CurrentHp / actor.MaxHp, ToTBarHeight),
                    colors["gradientLeft"],
                    colors["gradientRight"],
                    colors["gradientRight"],
                    colors["gradientLeft"]
                );

                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                if (ImGui.IsMouseHoveringRect(cursorPos, cursorPos + barSize))
                {
                    if (ImGui.GetIO().MouseClicked[0])
                    {
                        PluginInterface.ClientState.Targets.SetCurrentTarget(target);
                    }

                    if (ImGui.GetIO().MouseClicked[1])
                    {
                        unsafe
                        {
                            //PluginLog.Information();
                            var agentHud = new IntPtr(Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalID(4));
                            _openContextMenuFromTarget(agentHud, target.Address);
                        }
                    }
                }
            }

            ImGui.EndChild();
            ImGui.End();

            DrawTargetShield(target, cursorPos, barSize);
            DrawOutlinedText(text, new Vector2(cursorPos.X + ToTBarWidth / 2f - textSize.X / 2f + ToTBarTextXOffset, cursorPos.Y - 22 + ToTBarTextYOffset));
        }

        protected virtual unsafe void DrawCastBar()
        {
            if (!PluginConfiguration.ShowCastBar)
            {
                return;
            }

            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var actor = PluginInterface.ClientState.LocalPlayer;
            var battleChara = (BattleChara*)actor.Address;
            var castInfo = battleChara->SpellCastInfo;
            var isCasting = castInfo.IsCasting > 0;

            if (!isCasting)
            {
                return;
            }

            var currentCastId = castInfo.ActionID;
            var currentCastType = castInfo.ActionType;
            var currentCastTime = castInfo.CurrentCastTime;
            var totalCastTime = castInfo.TotalCastTime;

            if (_lastPlayerUsedCast != null)
            {
                if (!(_lastPlayerUsedCast.CastId == currentCastId && _lastPlayerUsedCast.ActionType == currentCastType))
                {
                    _lastPlayerUsedCast = new LastUsedCast(currentCastId, currentCastType, castInfo, PluginInterface);
                }
            }
            else
            {
                _lastPlayerUsedCast = new LastUsedCast(currentCastId, currentCastType, castInfo, PluginInterface);
            }

            var castText = _lastPlayerUsedCast.ActionText;

            var castPercent = 100f / totalCastTime * currentCastTime;
            var castScale = castPercent / 100f;

            var castTime = Math.Round(totalCastTime - totalCastTime * castScale, 1).ToString(CultureInfo.InvariantCulture);
            var barSize = new Vector2(CastBarWidth, CastBarHeight);
            var cursorPos = new Vector2(CenterX + CastBarXOffset - CastBarWidth / 2f, CenterY + CastBarYOffset);

            ImGui.SetCursorPos(cursorPos);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (PluginConfiguration.SlideCast)
            {
                var slideColor = PluginConfiguration.CastBarColorMap["slidecast"];
                var slideCastScale = PluginConfiguration.SlideCastTime / 10f / totalCastTime / 100f;

                drawList.AddRectFilledMultiColor(
                    cursorPos + barSize - new Vector2(barSize.X * slideCastScale, barSize.Y),
                    cursorPos + barSize,
                    slideColor["gradientLeft"],
                    slideColor["gradientRight"],
                    slideColor["gradientRight"],
                    slideColor["gradientLeft"]
                );
            }

            var castColor = PluginConfiguration.CastBarColorMap["castbar"];

            if (PluginConfiguration.ColorCastBarByJob)
            {
                PluginConfiguration.JobColorMap.TryGetValue(PluginInterface.ClientState.LocalPlayer.ClassJob.Id, out castColor);
                castColor ??= PluginConfiguration.CastBarColorMap["castbar"];
            }

            drawList.AddRectFilledMultiColor(
                cursorPos,
                cursorPos + new Vector2(barSize.X * castScale, barSize.Y),
                castColor["gradientLeft"],
                castColor["gradientRight"],
                castColor["gradientRight"],
                castColor["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            if (PluginConfiguration.ShowActionIcon && _lastPlayerUsedCast.IconTexture != null)
            {
                ImGui.Image(_lastPlayerUsedCast.IconTexture.ImGuiHandle, new Vector2(CastBarHeight, CastBarHeight));
                drawList.AddRect(cursorPos, cursorPos + new Vector2(CastBarHeight, CastBarHeight), 0xFF000000);
            }

            var castTextSize = ImGui.CalcTextSize(castText);
            var castTimeTextSize = ImGui.CalcTextSize(castTime);

            if (PluginConfiguration.ShowCastTime)
            {
                DrawOutlinedText(castTime, new Vector2(cursorPos.X + CastBarWidth - castTimeTextSize.X - 5, cursorPos.Y + CastBarHeight / 2f - castTimeTextSize.Y / 2f));
            }

            if (PluginConfiguration.ShowActionName)
            {
                DrawOutlinedText(
                    castText,
                    new Vector2(
                        cursorPos.X + (PluginConfiguration.ShowActionIcon && _lastPlayerUsedCast.IconTexture != null ? CastBarHeight : 0) + 5,
                        cursorPos.Y + CastBarHeight / 2f - castTextSize.Y / 2f
                    )
                );
            }
        }

        protected virtual unsafe void DrawTargetCastBar()
        {
            var actor = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (!PluginConfiguration.ShowTargetCastBar || actor is null)
            {
                return;
            }

            if (actor is not Chara || actor.ObjectKind == ObjectKind.Companion)
            {
                return;
            }

            var battleChara = (BattleChara*)actor.Address;
            var castInfo = battleChara->SpellCastInfo;

            var isCasting = castInfo.IsCasting > 0;

            if (!isCasting)
            {
                return;
            }

            var currentCastId = castInfo.ActionID;
            var currentCastType = castInfo.ActionType;
            var currentCastTime = castInfo.CurrentCastTime;
            var totalCastTime = castInfo.TotalCastTime;

            if (_lastTargetUsedCast != null)
            {
                if (!(_lastTargetUsedCast.CastId == currentCastId && _lastTargetUsedCast.ActionType == currentCastType))
                {
                    _lastTargetUsedCast = new LastUsedCast(currentCastId, currentCastType, castInfo, PluginInterface);
                }
            }
            else
            {
                _lastTargetUsedCast = new LastUsedCast(currentCastId, currentCastType, castInfo, PluginInterface);
            }

            var castText = _lastTargetUsedCast.ActionText;

            var castPercent = 100f / totalCastTime * currentCastTime;
            var castScale = castPercent / 100f;

            var castTime = Math.Round(totalCastTime - totalCastTime * castScale, 1).ToString(CultureInfo.InvariantCulture);
            var barSize = new Vector2(TargetCastBarWidth, TargetCastBarHeight);
            var cursorPos = new Vector2(CenterX + PluginConfiguration.TargetCastBarXOffset - TargetCastBarWidth / 2f, CenterY + PluginConfiguration.TargetCastBarYOffset);

            ImGui.SetCursorPos(cursorPos);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            var castColor = PluginConfiguration.CastBarColorMap["targetcastbar"];

            if (PluginConfiguration.ColorCastBarByDamageType)
            {
                switch (_lastTargetUsedCast.DamageType)
                {
                    case DamageType.Physical:
                    case DamageType.Blunt:
                    case DamageType.Slashing:
                    case DamageType.Piercing:
                        castColor = PluginConfiguration.CastBarColorMap["targetphysicalcastbar"];

                        break;

                    case DamageType.Magic:
                        castColor = PluginConfiguration.CastBarColorMap["targetmagicalcastbar"];

                        break;

                    case DamageType.Darkness:
                        castColor = PluginConfiguration.CastBarColorMap["targetdarknesscastbar"];

                        break;

                    case DamageType.Unknown:
                    case DamageType.LimitBreak:
                        castColor = PluginConfiguration.CastBarColorMap["targetcastbar"];

                        break;

                    default:
                        castColor = PluginConfiguration.CastBarColorMap["targetcastbar"];

                        break;
                }
            }

            if (PluginConfiguration.ShowTargetInterrupt && _lastTargetUsedCast.Interruptable)
            {
                castColor = PluginConfiguration.CastBarColorMap["targetinterruptcastbar"];
            }

            drawList.AddRectFilledMultiColor(
                cursorPos,
                cursorPos + new Vector2(barSize.X * castScale, barSize.Y),
                castColor["gradientLeft"],
                castColor["gradientRight"],
                castColor["gradientRight"],
                castColor["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            if (PluginConfiguration.ShowTargetActionIcon && _lastTargetUsedCast.IconTexture != null)
            {
                ImGui.Image(_lastTargetUsedCast.IconTexture.ImGuiHandle, new Vector2(TargetCastBarHeight, TargetCastBarHeight));
                drawList.AddRect(cursorPos, cursorPos + new Vector2(TargetCastBarHeight, TargetCastBarHeight), 0xFF000000);
            }

            var castTextSize = ImGui.CalcTextSize(castText);
            var castTimeTextSize = ImGui.CalcTextSize(castTime);

            if (PluginConfiguration.ShowTargetCastTime)
            {
                DrawOutlinedText(
                    castTime,
                    new Vector2(cursorPos.X + TargetCastBarWidth - castTimeTextSize.X - 5, cursorPos.Y + TargetCastBarHeight / 2f - castTimeTextSize.Y / 2f)
                );
            }

            if (PluginConfiguration.ShowTargetActionName)
            {
                DrawOutlinedText(
                    castText,
                    new Vector2(
                        cursorPos.X + (PluginConfiguration.ShowTargetActionIcon && _lastTargetUsedCast.IconTexture != null ? TargetCastBarHeight : 0) + 5,
                        cursorPos.Y + TargetCastBarHeight / 2f - castTextSize.Y / 2f
                    )
                );
            }
        }

        protected virtual void DrawTargetShield(Actor actor, Vector2 cursorPos, Vector2 targetBar)
        {
            if (!PluginConfiguration.ShieldEnabled)
            {
                return;
            }

            if (actor.ObjectKind is not ObjectKind.Player)
            {
                return;
            }

            var shieldColor = PluginConfiguration.MiscColorMap["shield"];
            var shield = Utils.ActorShieldValue(actor);
            
            // Account for border and draw shield inside the border of the HudElement
            cursorPos = new Vector2(cursorPos.X + 1, cursorPos.Y + 1);

            if (Math.Abs(shield) < 0)
            {
                return;
            }

            var drawList = ImGui.GetWindowDrawList();
            var y = PluginConfiguration.ShieldHeightPixels ? PluginConfiguration.ShieldHeight : targetBar.Y / 100 * PluginConfiguration.ShieldHeight;

            drawList.AddRectFilledMultiColor(
                cursorPos,
                cursorPos + new Vector2(targetBar.X * shield, y),
                shieldColor["gradientLeft"],
                shieldColor["gradientRight"],
                shieldColor["gradientRight"],
                shieldColor["gradientLeft"]
            );
        }

        protected virtual void DrawTankStanceIndicator()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var tankStanceBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(
                o => o.EffectId == 79
                  || // IRON WILL
                     o.EffectId == 91
                  || // DEFIANCE
                     o.EffectId == 392
                  || // ROYAL GUARD
                     o.EffectId == 393
                  || // IRON WILL
                     o.EffectId == 743
                  || // GRIT
                     o.EffectId == 1396
                  || // DEFIANCE
                     o.EffectId == 1397
||                                      // GRIT
                     o.EffectId == 1833 // ROYAL GUARD
            );

            var offset = PluginConfiguration.TankStanceIndicatorWidth + 1;

            if (tankStanceBuff.Count() != 1)
            {
                var barSize = new Vector2(HealthBarHeight > HealthBarWidth ? HealthBarWidth : HealthBarHeight, HealthBarHeight);
                var cursorPos = new Vector2(CenterX - HealthBarWidth - HealthBarXOffset - offset, CenterY + HealthBarYOffset + offset);
                ImGui.SetCursorPos(cursorPos);

                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(cursorPos, cursorPos + barSize, 0xFF2000FC, 0xFF2000FC, 0xFF2000FC, 0xFF2000FC);
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
            else
            {
                var barSize = new Vector2(HealthBarHeight > HealthBarWidth ? HealthBarWidth : HealthBarHeight, HealthBarHeight);
                var cursorPos = new Vector2(CenterX - HealthBarWidth - HealthBarXOffset - offset, CenterY + HealthBarYOffset + offset);
                ImGui.SetCursorPos(cursorPos);

                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(cursorPos, cursorPos + barSize, 0xFFE6CD00, 0xFFE6CD00, 0xFFE6CD00, 0xFFE6CD00);
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
        }

        protected virtual void DrawMPTicker()
        {
            if (!PluginConfiguration.MPTickerEnabled)
            {
                return;
            }

            if (MPTickerHideOnFullMp)
            {
                Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
                var actor = PluginInterface.ClientState.LocalPlayer;

                if (actor.CurrentMp >= actor.MaxMp)
                {
                    return;
                }
            }

            _mpTickHelper ??= new MpTickHelper(PluginInterface);

            var now = ImGui.GetTime();
            var scale = (float)((now - _mpTickHelper.LastTick) / MpTickHelper.ServerTickRate);

            if (scale <= 0)
            {
                return;
            }

            if (scale > 1)
            {
                scale = 1;
            }

            var fullSize = new Vector2(MPTickerWidth, MPTickerHeight);
            var barSize = new Vector2(Math.Max(1f, MPTickerWidth * scale), MPTickerHeight);
            var position = new Vector2(CenterX + MPTickerXOffset - MPTickerWidth / 2f, CenterY + MPTickerYOffset);
            var colors = PluginConfiguration.MiscColorMap["mpTicker"];

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(position, position + fullSize, 0x88000000);
            drawList.AddRectFilledMultiColor(position, position + barSize, colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]);

            if (MPTickerShowBorder)
            {
                drawList.AddRect(position, position + fullSize, 0xFF000000);
            }
        }

        protected virtual void DrawGCDIndicator()
        {
            if (!PluginConfiguration.GCDIndicatorEnabled || PluginInterface.ClientState.LocalPlayer is null)
            {
                return;
            }

            GCDHelper.GetGCDInfo(PluginInterface.ClientState.LocalPlayer, out var elapsed, out var total);

            if (total == 0 && !PluginConfiguration.GCDAlwaysShow)
            {
                return;
            }

            var scale = elapsed / total;

            if (scale <= 0)
            {
                return;
            }

            var (height, width) = GCDIndicatorVertical ? (-GCDIndicatorHeight, GCDIndicatorWidth) : (GCDIndicatorHeight, GCDIndicatorWidth);
            var position = new Vector2(CenterX + GCDIndicatorXOffset - GCDIndicatorWidth / 2f, CenterY + GCDIndicatorYOffset);
            var colors = PluginConfiguration.MiscColorMap["gcd"];

            var drawList = ImGui.GetWindowDrawList();
            var builder = BarBuilder.Create(position.X, position.Y, height, width);
            var gcdBar = builder.AddInnerBar(elapsed, total, colors).SetDrawBorder(GCDIndicatorShowBorder).SetVertical(GCDIndicatorVertical).Build();
            gcdBar.Draw(drawList, PluginConfiguration);
        }

        private void DrawPlayerStatusEffects()
        {
            _playerBuffList.Actor = PluginInterface.ClientState.LocalPlayer;
            _playerBuffList.Draw();
            _playerDebuffList.Actor = PluginInterface.ClientState.LocalPlayer;
            _playerDebuffList.Draw();
        }

        private void DrawTargetStatusEffects()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is null)
            {
                return;
            }

            if (target.ObjectKind != ObjectKind.Player && target.ObjectKind != ObjectKind.BattleNpc)
            {
                return;
            }

            _targetBuffList.Actor = target;
            _targetBuffList.Draw();
            _targetDebuffList.Actor = target;
            _targetDebuffList.Draw();
        }

        private int HasTankInvuln(Actor actor)
        {
            var tankInvulnBuff = actor.StatusEffects.Where(o => o.EffectId is 810 or 1302 or 409 or 1836);

            return tankInvulnBuff.Count();
        }

        protected virtual List<uint> GetJobSpecificBuffs()
        {
            return new List<uint>();
        }

        private void DrawRaidJobBuffs()
        {
            if (!(ShowRaidWideBuffIcons || ShowJobSpecificBuffIcons))
            {
                return;
            }
            List<uint> buffIds = new();
            if (ShowJobSpecificBuffIcons)
            {
                buffIds.AddRange(JobSpecificBuffs);
            }
            if (ShowRaidWideBuffIcons)
            {
                buffIds.AddRange(RaidWideBuffs);
            }
            _raidJobsBuffList.Actor = PluginInterface.ClientState.LocalPlayer;
            _raidJobsBuffList.Draw(buffIds);
        }

        protected Dictionary<string, uint> DetermineTargetPlateColors(Chara actor)
        {
            var colors = PluginConfiguration.NPCColorMap["neutral"];

            switch (actor.ObjectKind)
            {
                // Still need to figure out the "orange" state; aggroed but not yet attacked.
                case ObjectKind.Player:
                    PluginConfiguration.JobColorMap.TryGetValue(actor.ClassJob.Id, out colors);
                    colors ??= PluginConfiguration.NPCColorMap["neutral"];

                    break;

                case ObjectKind.BattleNpc when (actor.StatusFlags & StatusFlags.InCombat) == StatusFlags.InCombat:
                    colors = PluginConfiguration.NPCColorMap["hostile"];

                    break;

                case ObjectKind.BattleNpc:
                    if (!Utils.IsHostileMemory((BattleNpc)actor))
                    {
                        colors = PluginConfiguration.NPCColorMap["friendly"];
                    }

                    break;
            }

            return colors;
        }

        private void ClipAround(Addon addon, string windowName, ImDrawListPtr drawList, Action<ImDrawListPtr, string> drawAction)
        {
            if (addon is { Visible: true })
            {
                ClipAround(new Vector2(addon.X + 5, addon.Y + 5), new Vector2(addon.X + addon.Width - 5, addon.Y + addon.Height - 5), windowName, drawList, drawAction);
            }
            else
            {
                drawAction(drawList, windowName);
            }
        }

        private void ClipAround(Vector2 min, Vector2 max, string windowName, ImDrawListPtr drawList, Action<ImDrawListPtr, string> drawAction)
        {
            var maxX = ImGui.GetMainViewport().Size.X;
            var maxY = ImGui.GetMainViewport().Size.Y;
            var aboveMin = new Vector2(0, 0);
            var aboveMax = new Vector2(maxX, min.Y);
            var leftMin = new Vector2(0, min.Y);
            var leftMax = new Vector2(min.X, maxY);

            var rightMin = new Vector2(max.X, min.Y);
            var rightMax = new Vector2(maxX, max.Y);
            var belowMin = new Vector2(min.X, max.Y);
            var belowMax = new Vector2(maxX, maxY);

            for (var i = 0; i < 4; i++)
            {
                Vector2 clipMin;
                Vector2 clipMax;

                switch (i)
                {
                    default:
                        clipMin = aboveMin;
                        clipMax = aboveMax;

                        break;

                    case 1:
                        clipMin = leftMin;
                        clipMax = leftMax;

                        break;

                    case 2:
                        clipMin = rightMin;
                        clipMax = rightMax;

                        break;

                    case 3:
                        clipMin = belowMin;
                        clipMax = belowMax;

                        break;
                }

                ImGui.PushClipRect(clipMin, clipMax, false);
                drawAction(drawList, windowName + "_" + i);
                ImGui.PopClipRect();
            }
        }

        protected void DrawOutlinedText(string text, Vector2 pos) { DrawHelper.DrawOutlinedText(text, pos); }

        protected void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor) { DrawHelper.DrawOutlinedText(text, pos, color, outlineColor); }

        public void Draw()
        {
            if (!ShouldBeVisible() || PluginInterface.ClientState.LocalPlayer == null)
            {
                return;
            }

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);

            var begin = ImGui.Begin(
                "DelvUI",
                ImGuiWindowFlags.NoTitleBar
              | ImGuiWindowFlags.NoScrollbar
              | ImGuiWindowFlags.AlwaysAutoResize
              | ImGuiWindowFlags.NoBackground
              | ImGuiWindowFlags.NoInputs
              | ImGuiWindowFlags.NoBringToFrontOnFocus
            );

            if (!begin)
            {
                return;
            }

            DrawGenericElements();

            Draw(true);

            ImGui.End();
        }

        protected void DrawGenericElements()
        {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
            DrawTargetCastBar();
            DrawMPTicker();
            DrawGCDIndicator();

            DrawPlayerStatusEffects();
            DrawTargetStatusEffects();
            DrawRaidJobBuffs();
        }

        protected abstract void Draw(bool _);

        protected virtual unsafe bool ShouldBeVisible()
        {
            if (PluginConfiguration.HideHud)
            {
                return false;
            }

            if (IsVisible)
            {
                return true;
            }

            var parameterWidget = (AtkUnitBase*)PluginInterface.Framework.Gui.GetUiObjectByName("_ParameterWidget", 1);
            var fadeMiddleWidget = (AtkUnitBase*)PluginInterface.Framework.Gui.GetUiObjectByName("FadeMiddle", 1);

            // Display HUD only if parameter widget is visible and we're not in a fade event
            return PluginInterface.ClientState.LocalPlayer == null
                || parameterWidget == null
                || fadeMiddleWidget == null
                || !parameterWidget->IsVisible
                || fadeMiddleWidget->IsVisible;
        }

        private delegate void OpenContextMenuFromTarget(IntPtr agentHud, IntPtr gameObject);

        #region configs

        protected int HealthBarHeight => PluginConfiguration.HealthBarHeight;
        protected int HealthBarWidth => PluginConfiguration.HealthBarWidth;
        protected int HealthBarXOffset => PluginConfiguration.HealthBarXOffset;
        protected int HealthBarYOffset => PluginConfiguration.HealthBarYOffset;
        protected int HealthBarTextLeftXOffset => PluginConfiguration.HealthBarTextLeftXOffset;
        protected int HealthBarTextLeftYOffset => PluginConfiguration.HealthBarTextLeftYOffset;
        protected int HealthBarTextRightXOffset => PluginConfiguration.HealthBarTextRightXOffset;
        protected int HealthBarTextRightYOffset => PluginConfiguration.HealthBarTextRightYOffset;

        protected int PrimaryResourceBarHeight => PluginConfiguration.PrimaryResourceBarHeight;
        protected int PrimaryResourceBarWidth => PluginConfiguration.PrimaryResourceBarWidth;
        protected int PrimaryResourceBarXOffset => PluginConfiguration.PrimaryResourceBarXOffset;
        protected int PrimaryResourceBarYOffset => PluginConfiguration.PrimaryResourceBarYOffset;
        protected int PrimaryResourceBarTextXOffset => PluginConfiguration.PrimaryResourceBarTextXOffset;
        protected int PrimaryResourceBarTextYOffset => PluginConfiguration.PrimaryResourceBarTextYOffset;
        protected bool ShowPrimaryResourceBarValue => PluginConfiguration.ShowPrimaryResourceBarValue;
        protected bool ShowPrimaryResourceBarThresholdMarker => PluginConfiguration.ShowPrimaryResourceBarThresholdMarker;
        protected int PrimaryResourceBarThresholdValue => PluginConfiguration.PrimaryResourceBarThresholdValue;

        protected int TargetBarHeight => PluginConfiguration.TargetBarHeight;
        protected int TargetBarWidth => PluginConfiguration.TargetBarWidth;
        protected int TargetBarXOffset => PluginConfiguration.TargetBarXOffset;
        protected int TargetBarYOffset => PluginConfiguration.TargetBarYOffset;
        protected int TargetBarTextLeftXOffset => PluginConfiguration.TargetBarTextLeftXOffset;
        protected int TargetBarTextLeftYOffset => PluginConfiguration.TargetBarTextLeftYOffset;
        protected int TargetBarTextRightXOffset => PluginConfiguration.TargetBarTextRightXOffset;
        protected int TargetBarTextRightYOffset => PluginConfiguration.TargetBarTextRightYOffset;

        protected int ToTBarHeight => PluginConfiguration.ToTBarHeight;
        protected int ToTBarWidth => PluginConfiguration.ToTBarWidth;
        protected int ToTBarXOffset => PluginConfiguration.ToTBarXOffset;
        protected int ToTBarYOffset => PluginConfiguration.ToTBarYOffset;
        protected int ToTBarTextXOffset => PluginConfiguration.ToTBarTextXOffset;
        protected int ToTBarTextYOffset => PluginConfiguration.ToTBarTextYOffset;

        protected int FocusBarHeight => PluginConfiguration.FocusBarHeight;
        protected int FocusBarWidth => PluginConfiguration.FocusBarWidth;
        protected int FocusBarXOffset => PluginConfiguration.FocusBarXOffset;
        protected int FocusBarYOffset => PluginConfiguration.FocusBarYOffset;
        protected int FocusBarTextXOffset => PluginConfiguration.FocusBarTextXOffset;
        protected int FocusBarTextYOffset => PluginConfiguration.FocusBarTextYOffset;

        protected int MPTickerHeight => PluginConfiguration.MPTickerHeight;
        protected int MPTickerWidth => PluginConfiguration.MPTickerWidth;
        protected int MPTickerXOffset => PluginConfiguration.MPTickerXOffset;
        protected int MPTickerYOffset => PluginConfiguration.MPTickerYOffset;
        protected bool MPTickerShowBorder => PluginConfiguration.MPTickerShowBorder;
        protected bool MPTickerHideOnFullMp => PluginConfiguration.MPTickerHideOnFullMp;

        protected int GCDIndicatorHeight => PluginConfiguration.GCDIndicatorHeight;
        protected int GCDIndicatorWidth => PluginConfiguration.GCDIndicatorWidth;
        protected int GCDIndicatorXOffset => PluginConfiguration.GCDIndicatorXOffset;
        protected int GCDIndicatorYOffset => PluginConfiguration.GCDIndicatorYOffset;
        protected bool GCDIndicatorShowBorder => PluginConfiguration.GCDIndicatorShowBorder;
        protected bool GCDIndicatorVertical => PluginConfiguration.GCDIndicatorVertical;

        protected int CastBarWidth => PluginConfiguration.CastBarWidth;
        protected int CastBarHeight => PluginConfiguration.CastBarHeight;
        protected int CastBarXOffset => PluginConfiguration.CastBarXOffset;
        protected int CastBarYOffset => PluginConfiguration.CastBarYOffset;

        protected int TargetCastBarWidth => PluginConfiguration.TargetCastBarWidth;
        protected int TargetCastBarHeight => PluginConfiguration.TargetCastBarHeight;
        protected int TargetCastBarXOffset => PluginConfiguration.TargetCastBarXOffset;
        protected int TargetCastBarYOffset => PluginConfiguration.TargetCastBarYOffset;

        protected uint UnitFrameEmptyColor => ImGui.ColorConvertFloat4ToU32(PluginConfiguration.UnitFrameEmptyColor);

        protected uint PlayerUnitFrameColor => ImGui.ColorConvertFloat4ToU32(
            PluginConfiguration.CustomHealthBarBackgroundColorEnabled
                ? PluginConfiguration.CustomHealthBarBackgroundColor
                : PluginConfiguration.UnitFrameEmptyColor
        );

        #endregion
    }
}
