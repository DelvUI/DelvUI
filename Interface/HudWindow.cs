using System;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Interface;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public abstract class HudWindow {
        public bool IsVisible = true;
        protected readonly DalamudPluginInterface PluginInterface;
        protected readonly PluginConfiguration PluginConfiguration;
        private Vector2 _barsize;

        public abstract uint JobId { get; }

        protected float CenterX => ImGui.GetMainViewport().Size.X / 2f;
        protected float CenterY => ImGui.GetMainViewport().Size.Y / 2f;
        protected int XOffset => 238;
        protected int YOffset => 490;
        protected int BarHeight => PluginConfiguration.BarBorder.Height;
        protected int BarWidth => PluginConfiguration.BarBorder.Width;
        protected Vector2 BarSize => _barsize;
        protected IntPtr ImageBorder => PluginConfiguration.BarBorder.ImGuiHandle;
        protected IntPtr ImageHealth => PluginConfiguration.HealthBarImage.ImGuiHandle;
        protected IntPtr ImageHealthBackground => PluginConfiguration.HealthBarBackgroundImage.ImGuiHandle;
        protected IntPtr ImageTargetBar => PluginConfiguration.TargetBarImage.ImGuiHandle;
        protected IntPtr ImageTargetBarBackground => PluginConfiguration.TargetBarBackgroundImage.ImGuiHandle;
        
        protected HudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) {
            PluginInterface = pluginInterface;
            PluginConfiguration = pluginConfiguration;
            _barsize = new Vector2(BarWidth, BarHeight);
        }

        protected virtual void DrawHealthBar() {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentHp / actor.MaxHp;
            
            var cursorPos = new Vector2(CenterX - BarWidth - XOffset, CenterY + YOffset);

            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageHealthBackground, BarSize, Vector2.One, Vector2.Zero);

            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageHealth, new Vector2(BarWidth * scale, BarHeight), new Vector2(scale, 1f), Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, BarSize, Vector2.One, Vector2.Zero);
            
            const int indent = 5;
            ImGui.SetCursorPos(new Vector2(cursorPos.X + indent, cursorPos.Y + BarHeight - 26));
            ImGui.TextColored(Vector4.One, $"{actor.Name.Abbreviate().Truncate(16)}");
            
            var hp = $"{actor.MaxHp.KiloFormat(),6} | ";
            var hpSize = ImGui.CalcTextSize(hp);
            var percentageSize = ImGui.CalcTextSize("100");
            ImGui.SetCursorPos(new Vector2(cursorPos.X + BarWidth - hpSize.X - percentageSize.X - indent, cursorPos.Y + BarHeight - 26));
            ImGui.TextColored(Vector4.One, hp);
            ImGui.SetCursorPos(new Vector2(cursorPos.X + BarWidth - percentageSize.X - indent, cursorPos.Y + BarHeight - 26));
            ImGui.TextColored(Vector4.One, $"{(int)(scale * 100),3}");
        }

        protected virtual void DrawPrimaryResourceBar() {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentMp / actor.MaxMp;
            var barSize = new Vector2(357, 26);
            var cursorPos = new Vector2(CenterX - 178, CenterY + 496);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(PluginConfiguration.PrimaryBarBackgroundImage.ImGuiHandle, barSize, Vector2.One, Vector2.Zero);

            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(PluginConfiguration.PrimaryBarImage.ImGuiHandle, new Vector2(357 * scale, 26), new Vector2(scale, 1f), Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);
        }
        
        protected virtual void DrawTargetBar() {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (!(target is Chara actor)) {
                return;
            }

            var scale = (float) actor.CurrentHp / actor.MaxHp;
                
            var cursorPos = new Vector2(CenterX + XOffset, CenterY + YOffset);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageTargetBarBackground, BarSize, Vector2.One, Vector2.Zero);

            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageTargetBar, new Vector2(BarWidth * scale, BarHeight), new Vector2(scale, 1f), Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, BarSize, Vector2.One, Vector2.Zero);
            
            var indent = 5;
            var percentage = $"{(int) (scale * 100)}";
            var percentageSize = ImGui.CalcTextSize(percentage);
            var maxPercentageSize = ImGui.CalcTextSize("100");
            ImGui.SetCursorPos(new Vector2(cursorPos.X + indent + maxPercentageSize.X - percentageSize.X, cursorPos.Y + BarHeight - 26));
            ImGui.TextColored(Vector4.One, percentage);
            
            ImGui.SetCursorPos(new Vector2(cursorPos.X + indent + maxPercentageSize.X, cursorPos.Y + BarHeight - 26));
            ImGui.TextColored(Vector4.One, $" | {actor.MaxHp.KiloFormat(),-6}");

            var name = $"{actor.Name.Abbreviate().Truncate(16)}";
            var nameSize = ImGui.CalcTextSize(name);
            ImGui.SetCursorPos(new Vector2(cursorPos.X + BarWidth - nameSize.X - indent, cursorPos.Y + BarHeight - 26));
            ImGui.TextColored(Vector4.One, name);

            DrawTargetOfTargetBar(target.TargetActorID);
        }
        
        protected virtual void DrawTargetOfTargetBar(int targetActorId) {
            Actor target = null;
            
            for (var i = 0; i < 200; i += 2) {
                if (PluginInterface.ClientState.Actors[i]?.ActorId == targetActorId) {
                    target = PluginInterface.ClientState.Actors[i];
                }
            }
            
            if (!(target is Chara actor)) {
                return;
            }

            var scale = (float) actor.CurrentHp / actor.MaxHp;

            const int barWidth = 120;
            const int barHeight = 20;
            var cursorPos = new Vector2(CenterX + XOffset + BarWidth + 2, CenterY + YOffset);
            var barSize = new Vector2(barWidth, barHeight);
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageHealthBackground, barSize, Vector2.One, Vector2.Zero);

            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageHealth, new Vector2(barWidth * scale, barHeight), new Vector2(scale, 1f), Vector2.Zero);
            
            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(ImageBorder, barSize, Vector2.One, Vector2.Zero);
            
            const int indent = 5;
            ImGui.SetWindowFontScale(0.66f);
            ImGui.SetCursorPos(new Vector2(cursorPos.X + indent, cursorPos.Y + 2));
            ImGui.TextColored(Vector4.One, $"{actor.Name.Abbreviate().Truncate(16)}");
            ImGui.SetWindowFontScale(1.0f);
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
            
            Draw(true);
            
            ImGui.End();
        }
        
        protected abstract void Draw(bool _);

        protected virtual unsafe bool ShouldBeVisible() {
            if (IsVisible) {
                return true;
            }
            
            if (PluginConfiguration.HideHud) {
                return false;
            }

            if (PluginConfiguration.HideCombat && PluginInterface.ClientState.Condition[ConditionFlag.InCombat]) {
                return false;
            }

            var parameterWidget = (AtkUnitBase*) PluginInterface.Framework.Gui.GetUiObjectByName("_ParameterWidget", 1);
            var fadeMiddleWidget = (AtkUnitBase*) PluginInterface.Framework.Gui.GetUiObjectByName("FadeMiddle", 1);
            
            // Display HUD only if parameter widget is visible and we're not in a fade event
            return PluginInterface.ClientState.LocalPlayer == null || parameterWidget == null || fadeMiddleWidget == null || !parameterWidget->IsVisible || fadeMiddleWidget->IsVisible;
        }
    }
}