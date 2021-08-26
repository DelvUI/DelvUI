using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface
{
    public class MonkHudWindow : HudWindow
    {
        public override uint JobId => 20;

        private new static int BarHeight => 20;
        private new static int BarWidth => 254;
        private new static int XOffset => 127;
        private new static int YOffset => 370;

        public MonkHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawTargetBar();
            ChakraBar();
            Demolish();
            ActiveBuffs();
        }

        private void ActiveBuffs()
        {
            var target = PluginInterface.ClientState.LocalPlayer;

            if (!(target is Chara))
            {
                return;
            }

            var expiryColor = 0xFF2E2EC7;
            var xPadding = 2;
            var barWidth = (BarWidth / 2) - 1;
            var twinSnakes = target.StatusEffects.FirstOrDefault(o => o.EffectId == 101);
            var leadenFist = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1861);

            var twinSnakesDuration = twinSnakes.Duration;
            var leadenFistDuration = leadenFist.Duration;

            var xOffset = CenterX - 127;
            var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 8);
            var barSize = new Vector2(barWidth, BarHeight);
            var drawList = ImGui.GetWindowDrawList();

            var buffStart = new Vector2(xOffset + barWidth - (barSize.X / 15) * twinSnakesDuration, CenterY + YOffset - 8);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilled(buffStart, cursorPos + new Vector2(barSize.X, barSize.Y), 0xFF02DCE3);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2((barSize.X / 30) * leadenFistDuration, barSize.Y), leadenFistDuration > 0 ? 0xFFA8107F : 0x00202E3);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

        }

        private void Demolish()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (!(target is Chara))
            {
                return;
            }

            var expiryColor = 0xFF2E2EC7;
            var xPadding = 2;
            var barWidth = (BarWidth) - 1;
            var demolish = target.StatusEffects.FirstOrDefault(o => o.EffectId == 246 || o.EffectId == 1309);

            var demolishDuration = demolish.Duration;

            var demolishColor = demolishDuration > 6 ? 0xFF572DB9 : expiryColor;

            var xOffset = CenterX;
            var cursorPos = new Vector2(CenterX - 382, CenterY + YOffset - 52);
            var barSize = new Vector2(barWidth, BarHeight);
            var drawList = ImGui.GetWindowDrawList();

            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2((barSize.X / 18) * demolishDuration, barSize.Y), demolishColor);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

        }

        private void ChakraBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MNKGauge>();

            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding * 3) / 5;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 30;
            var cursorPos = new Vector2(xPos, yPos);

            var drawList = ImGui.GetWindowDrawList();
            for (var i = 0; i <= 5 - 1; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (gauge.NumChakra > i)
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y), 0xFF00A2FF);
                }
                else
                {

                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            }
        }
    }
}
