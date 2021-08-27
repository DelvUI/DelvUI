using System;
using System.Linq;
using System.Collections.Generic;
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


        protected int DemolishHeight => PluginConfiguration.MNKDemolishHeight;
        protected int DemolishWidth => PluginConfiguration.MNKDemolishWidth;
        protected new int DemolishXOffset => PluginConfiguration.MNKDemolishXOffset;
        protected new int DemolishYOffset => PluginConfiguration.MNKDemolishYOffset;
        protected int ChakraHeight => PluginConfiguration.MNKChakraHeight;
        protected int ChakraWidth => PluginConfiguration.MNKChakraWidth;
        protected new int ChakraXOffset => PluginConfiguration.MNKChakraXOffset;
        protected new int ChakraYOffset => PluginConfiguration.MNKChakraYOffset;
        protected int BuffHeight => PluginConfiguration.MNKBuffHeight;
        protected int BuffWidth => PluginConfiguration.MNKBuffWidth;
        protected new int BuffXOffset => PluginConfiguration.MNKBuffXOffset;
        protected new int BuffYOffset => PluginConfiguration.MNKBuffYOffset;
        protected new int TimeTwinXOffset => PluginConfiguration.MNKTimeTwinXOffset;
        protected new int TimeTwinYOffset => PluginConfiguration.MNKTimeTwinYOffset;
        protected new int TimeLeadenXOffset => PluginConfiguration.MNKTimeLeadenXOffset;
        protected new int TimeLeadenYOffset => PluginConfiguration.MNKTimeLeadenYOffset;
        protected new int TimeDemoXOffset => PluginConfiguration.MNKTimeDemoXOffset;
        protected new int TimeDemoYOffset => PluginConfiguration.MNKTimeDemoYOffset;

        protected Dictionary<string, uint> DemolishColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000];
        protected Dictionary<string, uint> ChakraColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 1];
        protected Dictionary<string, uint> LeadenFistColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 2];
        protected Dictionary<string, uint> TwinSnakesColor => PluginConfiguration.JobColorMap[Jobs.MNK * 1000 + 3];

        public MonkHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawTargetBar();
            ChakraBar();
            Demolish();
            ActiveBuffs();
            DrawCastBar();
        }

        private void ActiveBuffs()
        {
            var target = PluginInterface.ClientState.LocalPlayer;

            if (!(target is Chara))
            {
                return;
            }

            var expiryColor = 0xFF2E2EC7;
            var xPadding = 1;
            var barWidth = (BuffWidth / 2) - 1;
            var twinSnakes = target.StatusEffects.FirstOrDefault(o => o.EffectId == 101);
            var leadenFist = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1861);

            var twinSnakesDuration = twinSnakes.Duration;
            var leadenFistDuration = leadenFist.Duration;

            var xOffset = CenterX - BuffXOffset;
            var cursorPos = new Vector2(CenterX - BuffXOffset, CenterY + BuffYOffset - 8);
            var barSize = new Vector2(barWidth, BuffHeight);
            var drawList = ImGui.GetWindowDrawList();
            var twinXOffset = TimeTwinXOffset;
            var twinYOffset = TimeTwinYOffset;

            var buffStart = new Vector2(xOffset + barWidth - (barSize.X / 15) * twinSnakesDuration, CenterY + BuffYOffset - 8);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                    buffStart, cursorPos + new Vector2(barSize.X, barSize.Y),
                    TwinSnakesColor["gradientLeft"], TwinSnakesColor["gradientRight"], TwinSnakesColor["gradientRight"], TwinSnakesColor["gradientLeft"]
                );



            if (!PluginConfiguration.ShowBuffTime)
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((barSize.X / 30) * leadenFistDuration, barSize.Y),
                    leadenFistDuration > 0 ? LeadenFistColor["gradientLeft"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientRight"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientRight"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientLeft"] : 0x00202E3
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            }
            else
            {
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                DrawOutlinedText(Math.Round(twinSnakesDuration).ToString(), new Vector2(twinXOffset, twinYOffset));

                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
                var leadenXOffset = TimeLeadenXOffset;
                var leadenYOffset = TimeLeadenYOffset;

                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((barSize.X / 30) * leadenFistDuration, barSize.Y),
                    leadenFistDuration > 0 ? LeadenFistColor["gradientLeft"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientRight"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientRight"] : 0x00202E3,
                    leadenFistDuration > 0 ? LeadenFistColor["gradientLeft"] : 0x00202E3
                );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                if (leadenFistDuration <= 0)
                    DrawOutlinedText("0", new Vector2(leadenXOffset, leadenYOffset));
                else
                    DrawOutlinedText(Math.Round(leadenFistDuration).ToString(), new Vector2(leadenXOffset, leadenYOffset));
            }
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
            var barWidth = (DemolishWidth) - 1;
            var demolish = target.StatusEffects.FirstOrDefault(o => o.EffectId == 246 || o.EffectId == 1309);

            var demolishDuration = demolish.Duration;
            var demolishColor = DemolishColor;

            var xOffset = CenterX - DemolishXOffset;
            var cursorPos = new Vector2(CenterX - DemolishXOffset - 255, CenterY + DemolishYOffset - 52);
            var barSize = new Vector2(barWidth, DemolishHeight);
            var drawList = ImGui.GetWindowDrawList();

            var demoXOffset = TimeDemoXOffset;
            var demoYOffset = TimeDemoYOffset;

            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2((barSize.X / 18) * demolishDuration, barSize.Y),
                    demolishColor["gradientLeft"], demolishColor["gradientRight"], demolishColor["gradientRight"], demolishColor["gradientLeft"]
                );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            if (!PluginConfiguration.ShowDemolishTime)
                return;
            else
                DrawOutlinedText(Math.Round(demolishDuration).ToString(), new Vector2(demoXOffset, demoYOffset));

        }

        private void ChakraBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MNKGauge>();

            const int xPadding = 2;
            var barWidth = (ChakraWidth - xPadding * 3) / 5;
            var barSize = new Vector2(barWidth, ChakraHeight);
            var xPos = CenterX - ChakraXOffset;
            var yPos = CenterY + ChakraYOffset - 30;
            var cursorPos = new Vector2(xPos, yPos);

            var drawList = ImGui.GetWindowDrawList();
            for (var i = 0; i <= 5 - 1; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (gauge.NumChakra > i)
                {
                    drawList.AddRectFilledMultiColor(
                            cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                            ChakraColor["gradientLeft"], ChakraColor["gradientRight"], ChakraColor["gradientRight"], ChakraColor["gradientLeft"]
                        );
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