using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public abstract class HudWindow {
        public bool IsVisible = true;
        protected readonly DalamudPluginInterface PluginInterface;
        protected readonly PluginConfiguration PluginConfiguration;

        protected HudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) {
            PluginInterface = pluginInterface;
            PluginConfiguration = pluginConfiguration;
        }

        // Move the positioning offsets and such into config :and clean up these Draw methods
        protected virtual void DrawHealthBar() {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentHp / actor.MaxHp;

            var barWidth = PluginConfiguration.HealthBarBackgroundImage.Width;
            var barHeight = PluginConfiguration.HealthBarBackgroundImage.Height * 2 + 5;
            const int xOffset = 180;
            const int yOffset = 490;
            
            var cursorPos = new Vector2(
                ImGui.GetMainViewport().Size.X / 2f - barWidth - xOffset, 
                ImGui.GetMainViewport().Size.Y / 2f + yOffset
            );

            ImGui.SetCursorPos(cursorPos);

            ImGui.Image(
                PluginConfiguration.HealthBarBackgroundImage.ImGuiHandle,
                new Vector2(barWidth, barHeight),
                Vector2.One,
                Vector2.Zero
            );

            ImGui.SetCursorPos(cursorPos);

            ImGui.Image(
                PluginConfiguration.HealthBarImage.ImGuiHandle,
                new Vector2(barWidth * scale, barHeight),
                new Vector2(scale, 1f),
                Vector2.Zero
            );
            
            const int indent = 5;
            ImGui.SetCursorPos(new Vector2(cursorPos.X + indent, cursorPos.Y + barHeight - 26));
            ImGui.TextColored(Vector4.One, $"{actor.Name.Abbreviate().Truncate(16)}");
            
            var hp = $"{actor.MaxHp.KiloFormat(),6} | ";
            var hpSize = ImGui.CalcTextSize(hp);
            var percentageSize = ImGui.CalcTextSize("100");
            ImGui.SetCursorPos(new Vector2(cursorPos.X + barWidth - hpSize.X - percentageSize.X - indent, cursorPos.Y + barHeight - 26));
            ImGui.TextColored(Vector4.One, hp);
            ImGui.SetCursorPos(new Vector2(cursorPos.X + barWidth - percentageSize.X - indent, cursorPos.Y + barHeight - 26));
            ImGui.TextColored(Vector4.One, $"{(int)(scale * 100),3}");
        }

        protected virtual void DrawTargetBar() {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (!(target is Chara actor)) {
                return;
            }

            var scale = (float) actor.CurrentHp / actor.MaxHp;
                
            var barWidth = PluginConfiguration.TargetBarBackgroundImage.Width;
            var barHeight = PluginConfiguration.TargetBarBackgroundImage.Height * 2 + 5;
            const int xOffset = 65;
            const int yOffset = 490;
                
            var cursorPos = new Vector2(
                ImGui.GetMainViewport().Size.X / 2f + barWidth / 2f + xOffset, 
                ImGui.GetMainViewport().Size.Y / 2f + yOffset
            );
            
            ImGui.SetCursorPos(cursorPos);

            ImGui.Image(
                PluginConfiguration.TargetBarBackgroundImage.ImGuiHandle,
                new Vector2(barWidth, barHeight),
                Vector2.One,
                Vector2.Zero
            );

            ImGui.SetCursorPos(cursorPos);
            ImGui.Image(
                PluginConfiguration.TargetBarImage.ImGuiHandle,
                new Vector2(barWidth * scale, barHeight),
                new Vector2(scale, 1f),
                Vector2.Zero
            );
            
            var indent = 5;
            var percentage = $"{(int) (scale * 100)}";
            var percentageSize = ImGui.CalcTextSize(percentage);
            var maxPercentageSize = ImGui.CalcTextSize("100");
            ImGui.SetCursorPos(new Vector2(cursorPos.X + indent + maxPercentageSize.X - percentageSize.X, cursorPos.Y + barHeight - 26));
            ImGui.TextColored(Vector4.One, percentage);
            
            ImGui.SetCursorPos(new Vector2(cursorPos.X + indent + maxPercentageSize.X, cursorPos.Y + barHeight - 26));
            ImGui.TextColored(Vector4.One, $" | {actor.MaxHp.KiloFormat(),-6}");

            var name = $"{actor.Name.Abbreviate().Truncate(16)}";
            var nameSize = ImGui.CalcTextSize(name);
            ImGui.SetCursorPos(new Vector2(cursorPos.X + barWidth - nameSize.X - indent, cursorPos.Y + barHeight - 26));
            ImGui.TextColored(Vector4.One, name);
        }
        
        public abstract void Draw();
        
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