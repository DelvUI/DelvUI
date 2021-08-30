using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Data.LuminaExtensions;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.GameStructs;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;
using DelvUI.Helpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Action = System.Action;

namespace DelvUI.Interface {
    public abstract class HudWindow {
        public bool IsVisible = true;
        protected readonly DalamudPluginInterface PluginInterface;
        protected readonly PluginConfiguration PluginConfiguration;
        private Vector2 _barSize;

        public abstract uint JobId { get; }

        protected static float CenterX => ImGui.GetMainViewport().Size.X / 2f;
        protected static float CenterY => ImGui.GetMainViewport().Size.Y / 2f;
        protected static int XOffset => 160;
        protected static int YOffset => 460;

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

        protected int CastBarWidth => PluginConfiguration.CastBarWidth;
        protected int CastBarHeight => PluginConfiguration.CastBarHeight;
        protected int CastBarXOffset => PluginConfiguration.CastBarXOffset;
        protected int CastBarYOffset => PluginConfiguration.CastBarYOffset;
        
        protected int TargetCastBarWidth => PluginConfiguration.TargetCastBarWidth;
        protected int TargetCastBarHeight => PluginConfiguration.TargetCastBarHeight;
        protected int TargetCastBarXOffset => PluginConfiguration.TargetCastBarXOffset;
        protected int TargetCastBarYOffset => PluginConfiguration.TargetCastBarYOffset;

        protected Vector2 BarSize => _barSize;

        private LastUsedCast _lastPlayerUsedCast;
        private LastUsedCast _lastTargetUsedCast;

        private delegate void OpenContextMenuFromTarget(IntPtr agentHud, IntPtr gameObject);
        private OpenContextMenuFromTarget openContextMenuFromTarget;

        private MpTickHelper mpTickHelper = null;

        protected HudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) {
            PluginInterface = pluginInterface;
            PluginConfiguration = pluginConfiguration;

            openContextMenuFromTarget = Marshal.GetDelegateForFunctionPointer<OpenContextMenuFromTarget>(PluginInterface.TargetModuleScanner.ScanText("48 85 D2 74 7F 48 89 5C 24"));

            PluginConfiguration.ConfigChangedEvent += OnConfigChanged;
        }

        protected void OnConfigChanged(object sender, EventArgs args)
        {
            if (!PluginConfiguration.MPTickerEnabled)
            {
                mpTickHelper = null;
            } 
        }

        protected virtual void DrawHealthBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            _barSize = new Vector2(HealthBarWidth, HealthBarHeight);
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentHp / actor.MaxHp;

            if (PluginConfiguration.TankStanceIndicatorEnabled && (actor.ClassJob.Id is 19 or 32 or 21 or 37)) {
                DrawTankStanceIndicator();
            }

            var cursorPos = new Vector2(CenterX - HealthBarWidth - HealthBarXOffset, CenterY + HealthBarYOffset);
            ImGui.SetCursorPos(cursorPos);

            PluginConfiguration.JobColorMap.TryGetValue(PluginInterface.ClientState.LocalPlayer.ClassJob.Id, out var colors);
            colors ??= PluginConfiguration.NPCColorMap["friendly"];

            var drawList = ImGui.GetWindowDrawList();

            // Basically make an invisible box for BeginChild to work properly.
            ImGuiWindowFlags windowFlags = 0;
            windowFlags |= ImGuiWindowFlags.NoBackground;
            windowFlags |= ImGuiWindowFlags.NoTitleBar;
            windowFlags |= ImGuiWindowFlags.NoMove;
            windowFlags |= ImGuiWindowFlags.NoDecoration;

            ImGui.SetNextWindowPos(cursorPos);
            ImGui.SetNextWindowSize(_barSize);

            ImGui.Begin("health_bar", windowFlags);
            if (ImGui.BeginChild("health_bar", _barSize)) {
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, colors["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(HealthBarWidth * scale, HealthBarHeight),
                    colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

                // Check if mouse is hovering over the box properly
                if (ImGui.GetIO().MouseClicked[0] && ImGui.IsMouseHoveringRect(cursorPos, cursorPos + BarSize)) {
                    PluginInterface.ClientState.Targets.SetCurrentTarget(actor);
                }
            }

            ImGui.EndChild();
            ImGui.End();

            DrawTargetShield(actor, cursorPos, _barSize, true);

            DrawOutlinedText(
                $"{Helpers.TextTags.GenerateFormattedTextFromTags(actor, PluginConfiguration.HealthBarTextLeft)}",
                new Vector2(cursorPos.X + 5 + HealthBarTextLeftXOffset, cursorPos.Y - 22 + HealthBarTextLeftYOffset)
            );

            var text = Helpers.TextTags.GenerateFormattedTextFromTags(actor, PluginConfiguration.HealthBarTextRight);
            var textSize = ImGui.CalcTextSize(text);

            DrawOutlinedText(text,
                new Vector2(cursorPos.X + HealthBarWidth - textSize.X - 5 + HealthBarTextRightXOffset,
                    cursorPos.Y - 22 + HealthBarTextRightYOffset
                )
            );
        }

        protected virtual void DrawPrimaryResourceBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            _barSize = new Vector2(PrimaryResourceBarWidth, PrimaryResourceBarHeight);
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentMp / actor.MaxMp;
            var cursorPos = new Vector2(CenterX - PrimaryResourceBarXOffset + 33, CenterY + PrimaryResourceBarYOffset - 16);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(BarSize.X * scale, BarSize.Y),
                0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00
            );
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
        }

        protected virtual void DrawTargetBar() {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (target is null) {
                return;
            }

            _barSize = new Vector2(TargetBarWidth, TargetBarHeight);

            var cursorPos = new Vector2(CenterX + TargetBarXOffset, CenterY + TargetBarYOffset);
            ImGui.SetCursorPos(cursorPos);

            var drawList = ImGui.GetWindowDrawList();
            if (target is not Chara actor) {
                var friendly = PluginConfiguration.NPCColorMap["friendly"];
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, friendly["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(TargetBarWidth, TargetBarHeight),
                    friendly["gradientLeft"], friendly["gradientRight"],
                    friendly["gradientRight"], friendly["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            }
            else {
                var scale = actor.MaxHp > 0f ? (float) actor.CurrentHp / actor.MaxHp : 0f;
                var colors = DetermineTargetPlateColors(actor);
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, colors["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(TargetBarWidth * scale, TargetBarHeight),
                    colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                
                DrawTargetShield(target, cursorPos, BarSize, true);

                var text = Helpers.TextTags.GenerateFormattedTextFromTags(target, PluginConfiguration.TargetBarTextLeft);
                DrawOutlinedText(text, new Vector2(cursorPos.X + 5 + TargetBarTextLeftXOffset, cursorPos.Y - 22 + TargetBarTextLeftYOffset));
            }
            
            var textLeft = Helpers.TextTags.GenerateFormattedTextFromTags(target, PluginConfiguration.TargetBarTextLeft);
            DrawOutlinedText(textLeft,
                new Vector2(cursorPos.X + 5 + TargetBarTextLeftXOffset,
                    cursorPos.Y - 22 + TargetBarTextLeftYOffset));

            var textRight = Helpers.TextTags.GenerateFormattedTextFromTags(target, PluginConfiguration.TargetBarTextRight);
            var textRightSize = ImGui.CalcTextSize(textRight);

            DrawOutlinedText(textRight,
                new Vector2(cursorPos.X + TargetBarWidth - textRightSize.X - 5 + TargetBarTextRightXOffset,
                    cursorPos.Y - 22 + TargetBarTextRightYOffset));

            DrawTargetOfTargetBar(target.TargetActorID);
        }

        protected virtual void DrawFocusBar() {
            var focus = PluginInterface.ClientState.Targets.FocusTarget;
            if (focus is null) {
                return;
            }

            var barSize = new Vector2(FocusBarWidth, FocusBarHeight);

            var cursorPos = new Vector2(CenterX - FocusBarXOffset - HealthBarWidth - FocusBarWidth - 2, CenterY + FocusBarYOffset);
            ImGui.SetCursorPos(cursorPos);

            var drawList = ImGui.GetWindowDrawList();
            if (focus is not Chara actor) {
                var friendly = PluginConfiguration.NPCColorMap["friendly"];
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, friendly["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(FocusBarWidth, FocusBarHeight),
                    friendly["gradientLeft"], friendly["gradientRight"], friendly["gradientRight"], friendly["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                DrawTargetShield(focus, cursorPos, barSize, true);
            }
            else {
                var colors = DetermineTargetPlateColors(actor);
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, colors["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((float) FocusBarWidth * actor.CurrentHp / actor.MaxHp, FocusBarHeight),
                    colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                DrawTargetShield(focus, cursorPos, barSize, true);
            }

            var text = Helpers.TextTags.GenerateFormattedTextFromTags(focus, PluginConfiguration.FocusBarText);
            var textSize = ImGui.CalcTextSize(text);
            DrawOutlinedText(text,
                new Vector2(
                    cursorPos.X + FocusBarWidth / 2f - textSize.X / 2f + FocusBarTextXOffset,
                    cursorPos.Y - 22 + FocusBarTextYOffset
                )
            );
        }

        protected virtual void DrawTargetOfTargetBar(int targetActorId) {
            Actor target = null;
            if (targetActorId == 0 && PluginInterface.ClientState.LocalPlayer.TargetActorID == 0) {
                target = PluginInterface.ClientState.LocalPlayer;
            }
            else {
                for (var i = 0; i < 200; i += 2) {
                    if (PluginInterface.ClientState.Actors[i]?.ActorId == targetActorId) {
                        target = PluginInterface.ClientState.Actors[i];
                    }
                }
            }

            if (target is not Chara actor) {
                return;
            }

            var barSize = new Vector2(ToTBarWidth, ToTBarHeight);
            var colors = DetermineTargetPlateColors(actor);
            var text = Helpers.TextTags.GenerateFormattedTextFromTags(target, PluginConfiguration.ToTBarText);
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
            if (ImGui.BeginChild("target_of_target_bar", barSize)) {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, colors["background"]);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((float) ToTBarWidth * actor.CurrentHp / actor.MaxHp, ToTBarHeight),
                    colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                if (ImGui.GetIO().MouseClicked[0] && ImGui.IsMouseHoveringRect(cursorPos, cursorPos + barSize)) {
                    PluginInterface.ClientState.Targets.SetCurrentTarget(target);
                }
            }
            ImGui.EndChild();
            ImGui.End();

            DrawTargetShield(target, cursorPos, barSize, true);

            DrawOutlinedText(text,
                new Vector2(
                    cursorPos.X + ToTBarWidth / 2f - textSize.X / 2f + ToTBarTextXOffset,
                    cursorPos.Y - 22 + ToTBarTextYOffset
                )
            );
        }

        protected virtual unsafe void DrawCastBar() {
            if (!PluginConfiguration.ShowCastBar) {
                return;
            }

            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var actor = PluginInterface.ClientState.LocalPlayer;
            var battleChara = (BattleChara*) actor.Address;
            var castInfo = battleChara->SpellCastInfo;
            var isCasting = castInfo.IsCasting > 0;
            if (!isCasting) return;
            
            var currentCastId = castInfo.ActionID;
            var currentCastType = castInfo.ActionType;
            var currentCastTime = castInfo.CurrentCastTime;
            var totalCastTime = castInfo.TotalCastTime;

            _lastPlayerUsedCast = new LastUsedCast(currentCastId, currentCastType, PluginInterface);
            var iconTexFile = _lastPlayerUsedCast.Icon;
            var castText = _lastPlayerUsedCast.ActionText;

            var castPercent = 100f / totalCastTime * currentCastTime;
            var castScale = castPercent / 100f;

            var castTime = Math.Round((totalCastTime - totalCastTime * castScale), 1).ToString(CultureInfo.InvariantCulture);
            var barSize = new Vector2(CastBarWidth, CastBarHeight);
            var cursorPos = new Vector2(
                CenterX + CastBarXOffset - CastBarWidth / 2f,
                CenterY + CastBarYOffset
            );

            ImGui.SetCursorPos(cursorPos);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (PluginConfiguration.SlideCast) {
                var slideColor = PluginConfiguration.CastBarColorMap["slidecast"];
                var slideCastScale = PluginConfiguration.SlideCastTime / 10f / totalCastTime / 100f;

                drawList.AddRectFilledMultiColor(
                    cursorPos + barSize - new Vector2(barSize.X * slideCastScale, barSize.Y), cursorPos + barSize,
                    slideColor["gradientLeft"], slideColor["gradientRight"], slideColor["gradientRight"], slideColor["gradientLeft"]
                );
            }

            var castColor = PluginConfiguration.CastBarColorMap["castbar"];
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * castScale, barSize.Y),
                castColor["gradientLeft"], castColor["gradientRight"], castColor["gradientRight"], castColor["gradientLeft"]
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            var emptyIconPath = "ui/icon/000000/000000.tex";
            if (PluginConfiguration.ShowActionIcon && iconTexFile?.FilePath.Path != emptyIconPath) {
                var texture = PluginInterface.UiBuilder.LoadImageRaw(iconTexFile.GetRgbaImageData(), iconTexFile.Header.Width, iconTexFile.Header.Height, 4);

                ImGui.Image(texture.ImGuiHandle, new Vector2(CastBarHeight, CastBarHeight));
                drawList.AddRect(cursorPos, cursorPos + new Vector2(CastBarHeight, CastBarHeight), 0xFF000000);
            }

            var castTextSize = ImGui.CalcTextSize(castText);
            var castTimeTextSize = ImGui.CalcTextSize(castTime);

            if (PluginConfiguration.ShowCastTime) {
                DrawOutlinedText(
                    castTime,
                    new Vector2(cursorPos.X + CastBarWidth - castTimeTextSize.X - 5, cursorPos.Y + CastBarHeight / 2f - castTimeTextSize.Y / 2f)
                );
            }

            if (PluginConfiguration.ShowActionName) {
                DrawOutlinedText(
                    castText,
                    new Vector2(
                        cursorPos.X + (PluginConfiguration.ShowActionIcon && iconTexFile.FilePath.Path != emptyIconPath ? CastBarHeight : 0) + 5,
                        cursorPos.Y + CastBarHeight / 2f - castTextSize.Y / 2f
                    )
                );
            }
        }
        
        protected virtual unsafe void DrawTargetCastBar() {
            var actor = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            if (!PluginConfiguration.ShowTargetCastBar || actor is null) {
                return;
            }

            if (actor is not Chara) return;

            // GameObject.CurrentCastId (for 6.0)
            var battleChara = (BattleChara*) actor.Address;
            var castInfo = battleChara->SpellCastInfo;

            var isCasting = castInfo.IsCasting > 0;
            if (!isCasting) return;
            var currentCastId = castInfo.ActionID;
            var currentCastType = castInfo.ActionType;
            var currentCastTime = castInfo.CurrentCastTime;
            var totalCastTime = castInfo.TotalCastTime;

            _lastTargetUsedCast = new LastUsedCast(currentCastId, currentCastType, PluginInterface);
            var iconTexFile = _lastTargetUsedCast.Icon;
            var castText = _lastTargetUsedCast.ActionText;

            var castPercent = 100f / totalCastTime * currentCastTime;
            var castScale = castPercent / 100f;

            var castTime = Math.Round((totalCastTime - totalCastTime * castScale), 1).ToString(CultureInfo.InvariantCulture);
            var barSize = new Vector2(TargetCastBarWidth, TargetCastBarHeight);
            var cursorPos = new Vector2(
                CenterX + PluginConfiguration.TargetCastBarXOffset - TargetCastBarWidth / 2f,
                CenterY + PluginConfiguration.TargetCastBarYOffset
            );

            ImGui.SetCursorPos(cursorPos);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            var castColor = PluginConfiguration.CastBarColorMap["targetcastbar"];
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * castScale, barSize.Y),
                castColor["gradientLeft"], castColor["gradientRight"], castColor["gradientRight"], castColor["gradientLeft"]
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            var emptyIconPath = "ui/icon/000000/000000.tex";
            if (PluginConfiguration.ShowTargetActionIcon && iconTexFile?.FilePath.Path != emptyIconPath) {
                var texture = PluginInterface.UiBuilder.LoadImageRaw(iconTexFile.GetRgbaImageData(), iconTexFile.Header.Width, iconTexFile.Header.Height, 4);

                ImGui.Image(texture.ImGuiHandle, new Vector2(TargetCastBarHeight, TargetCastBarHeight));
                drawList.AddRect(cursorPos, cursorPos + new Vector2(TargetCastBarHeight, TargetCastBarHeight), 0xFF000000);
            }

            var castTextSize = ImGui.CalcTextSize(castText);
            var castTimeTextSize = ImGui.CalcTextSize(castTime);

            if (PluginConfiguration.ShowTargetCastTime) {
                DrawOutlinedText(
                    castTime,
                    new Vector2(cursorPos.X + TargetCastBarWidth - castTimeTextSize.X - 5, cursorPos.Y + TargetCastBarHeight / 2f - castTimeTextSize.Y / 2f)
                );
            }

            if (PluginConfiguration.ShowTargetActionName) {
                DrawOutlinedText(
                    castText,
                    new Vector2(
                        cursorPos.X + (PluginConfiguration.ShowTargetActionIcon && iconTexFile.FilePath.Path != emptyIconPath ? TargetCastBarHeight : 0) + 5,
                        cursorPos.Y + TargetCastBarHeight / 2f - castTextSize.Y / 2f
                    )
                );
            }
        }

        protected virtual void DrawTargetShield(Actor actor, Vector2 cursorPos, Vector2 targetBar, bool leftToRight) {
            if (!PluginConfiguration.ShieldEnabled) {
                return;
            }

            var shieldColor = PluginConfiguration.ShieldColorMap["shield"];
            var shield = ActorShieldValue(actor);
            if (Math.Abs(shield) < 0) {
                return;
            }

            var drawList = ImGui.GetWindowDrawList();
            var y = PluginConfiguration.ShieldHeightPixels
                ? PluginConfiguration.ShieldHeight
                : targetBar.Y / 100 * PluginConfiguration.ShieldHeight;
            
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(targetBar.X * shield, y),
                shieldColor["gradientLeft"], shieldColor["gradientRight"], shieldColor["gradientRight"], shieldColor["gradientLeft"]
            );
            drawList.AddRect(cursorPos, cursorPos + targetBar, 0xFF000000);
        }

        protected virtual void DrawTankStanceIndicator() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var tankStanceBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o =>
                    o.EffectId == 79   || // IRON WILL
                    o.EffectId == 91   || // DEFIANCE
                    o.EffectId == 392  || // ROYAL GUARD
                    o.EffectId == 393  || // IRON WILL
                    o.EffectId == 743  || // GRIT
                    o.EffectId == 1396 || // DEFIANCE
                    o.EffectId == 1397 || // GRIT
                    o.EffectId == 1833    // ROYAL GUARD
            );

            var offset = PluginConfiguration.TankStanceIndicatorWidth + 1;
            if (tankStanceBuff.Count() != 1)
            {
                var barSize = new Vector2(HealthBarHeight > HealthBarWidth ? HealthBarWidth : HealthBarHeight, HealthBarHeight);
                var cursorPos = new Vector2(CenterX - HealthBarWidth - HealthBarXOffset - offset, CenterY + HealthBarYOffset + offset);
                ImGui.SetCursorPos(cursorPos);

                var drawList = ImGui.GetWindowDrawList();

                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + barSize,
                    0xFF2000FC, 0xFF2000FC, 0xFF2000FC, 0xFF2000FC
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
            else
            {
                var barSize = new Vector2(HealthBarHeight > HealthBarWidth ? HealthBarWidth : HealthBarHeight, HealthBarHeight);
                var cursorPos = new Vector2(CenterX - HealthBarWidth - HealthBarXOffset - offset, CenterY + HealthBarYOffset + offset);
                ImGui.SetCursorPos(cursorPos);

                var drawList = ImGui.GetWindowDrawList();

                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + barSize,
                    0xFFE6CD00, 0xFFE6CD00, 0xFFE6CD00, 0xFFE6CD00
                );
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

            if (mpTickHelper == null)
            {
                mpTickHelper = new MpTickHelper(PluginInterface);
            }

            var now = ImGui.GetTime();
            var scale = (float)((now - mpTickHelper.lastTick) / MpTickHelper.serverTickRate);
            if (scale <= 0)
            {
                return;
            } 
            else if (scale > 1)
            {
                scale = 1;
            }

            var fullSize = new Vector2(MPTickerWidth, MPTickerHeight);
            var barSize = new Vector2(Math.Max(1f, MPTickerWidth * scale), MPTickerHeight);
            var position = new Vector2(CenterX + MPTickerXOffset - MPTickerWidth / 2f, CenterY + MPTickerYOffset);
            var colors = PluginConfiguration.MPTickerColorMap["mpTicker"];

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(position, position + fullSize, 0x88000000);
            drawList.AddRectFilledMultiColor(position, position + barSize,
                colors["gradientLeft"], colors["gradientRight"], colors["gradientRight"], colors["gradientLeft"]
            );

            if (MPTickerShowBorder)
            {
                drawList.AddRect(position, position + fullSize, 0xFF000000);
            }
        }

        protected unsafe virtual float ActorShieldValue(Actor actor) {
            return Math.Min(*(int*) (actor.Address + 0x1997), 100) / 100f;
        }

        protected Dictionary<string, uint> DetermineTargetPlateColors(Chara actor) {
            var colors = PluginConfiguration.NPCColorMap["neutral"];

            // Still need to figure out the "orange" state; aggroed but not yet attacked.
            switch (actor.ObjectKind) {
                case ObjectKind.Player:
                    PluginConfiguration.JobColorMap.TryGetValue(actor.ClassJob.Id, out colors);
                    colors ??= PluginConfiguration.NPCColorMap["neutral"];
                    break;

                case ObjectKind.BattleNpc when (actor.StatusFlags & StatusFlags.InCombat) == StatusFlags.InCombat:
                    colors = PluginConfiguration.NPCColorMap["hostile"];
                    break;

                case ObjectKind.BattleNpc:
                {
                    if (!IsHostileMemory((BattleNpc) actor)) {
                        colors = PluginConfiguration.NPCColorMap["friendly"];
                    }

                    break;
                }
            }

            return colors;
        }

        protected void DrawOutlinedText(string text, Vector2 pos) {
            DrawHelper.DrawOutlinedText(text, pos);
        }

        protected void DrawOutlinedText(string text, Vector2 pos, Vector4 color, Vector4 outlineColor) {
            DrawHelper.DrawOutlinedText(text, pos, color, outlineColor);
        }

        public void Draw() {
            if (!ShouldBeVisible() || PluginInterface.ClientState.LocalPlayer == null) {
                return;
            }

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);

            var begin = ImGui.Begin(
                "DelvUI",
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize |
                ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBringToFrontOnFocus
            );

            if (!begin) {
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
        }

        protected abstract void Draw(bool _);

        protected virtual void HandleProperties() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var actor = PluginInterface.ClientState.LocalPlayer;
        }

        protected virtual unsafe bool ShouldBeVisible() {
            if (PluginConfiguration.HideHud) {
                return false;
            }

            if (IsVisible) {
                return true;
            }

            var parameterWidget = (AtkUnitBase*) PluginInterface.Framework.Gui.GetUiObjectByName("_ParameterWidget", 1);
            var fadeMiddleWidget = (AtkUnitBase*) PluginInterface.Framework.Gui.GetUiObjectByName("FadeMiddle", 1);

            // Display HUD only if parameter widget is visible and we're not in a fade event
            return PluginInterface.ClientState.LocalPlayer == null || parameterWidget == null || fadeMiddleWidget == null || !parameterWidget->IsVisible || fadeMiddleWidget->IsVisible;
        }

        private static unsafe bool IsHostileMemory(BattleNpc npc) {
            return (npc.BattleNpcKind == BattleNpcSubKind.Enemy || (int) npc.BattleNpcKind == 1)
                   && *(byte*) (npc.Address + 0x1980) != 0
                   && *(byte*) (npc.Address + 0x193C) != 1;
        }
    }
}