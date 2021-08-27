using System.Numerics;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class WhiteMageHudWindow : HudWindow
    {
        public override uint JobId => 24;

        private int BarHeight => PluginConfiguration.WHMLilyBarHeight;

        private int BarWidth => PluginConfiguration.WHMLilyBarWidth;

        private int LilyBarXPadding => PluginConfiguration.WHMLilyBarPad;

        private new int XOffset => PluginConfiguration.WHMLilyBarX;

        private new int YOffset => PluginConfiguration.WHMLilyBarY;

        private Dictionary<string, uint> LilyColor => PluginConfiguration.JobColorMap[Jobs.WHM * 1000];

        private Dictionary<string, uint> BloodLilyColor => PluginConfiguration.JobColorMap[Jobs.WHM * 1000 + 1];

        private Dictionary<string, uint> ChargingColor => PluginConfiguration.JobColorMap[Jobs.WHM * 1000 + 2];

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.WHM * 1000 + 3];

        public WhiteMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration)
        {
        }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawSecondaryResourceBar();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        private void DrawSecondaryResourceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<WHMGauge>();

            const int numChunks = 6;

            var barWidth = (BarWidth - LilyBarXPadding * (numChunks - 1)) / numChunks;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 20;

            const float lilyCooldown = 30000f;

            var cursorPos = new Vector2(xPos, yPos);
            var drawList = ImGui.GetWindowDrawList();

            var scale = gauge.NumLilies == 0 ? gauge.LilyTimer / lilyCooldown : 1;
            drawList.AddRectFilledMultiColor(cursorPos, cursorPos + barSize, 
                    EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]);
            if (gauge.NumLilies >= 1)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                    LilyColor["gradientLeft"], LilyColor["gradientRight"], LilyColor["gradientRight"], LilyColor["gradientLeft"]
                );
            }
            else
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                    ChargingColor["gradientLeft"], ChargingColor["gradientRight"], ChargingColor["gradientRight"], ChargingColor["gradientLeft"]
                );
            }

            if (scale < 1)
            {
                var timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                var size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                DrawOutlinedText(timer, new Vector2(cursorPos.X + barWidth / 2f - size.X / 2f, cursorPos.Y + BarHeight));
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + LilyBarXPadding + barWidth, cursorPos.Y);

            drawList.AddRectFilledMultiColor(cursorPos, cursorPos + barSize, 
                    EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]);

            if (gauge.NumLilies > 0)
            {
                scale = gauge.NumLilies == 1 ? gauge.LilyTimer / lilyCooldown : 1;

                if (gauge.NumLilies >= 2)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                        LilyColor["gradientLeft"], LilyColor["gradientRight"], LilyColor["gradientRight"], LilyColor["gradientLeft"]
                    );
                }
                else
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                        ChargingColor["gradientLeft"], ChargingColor["gradientRight"], ChargingColor["gradientRight"], ChargingColor["gradientLeft"]
                    );
                }

                if (scale < 1)
                {
                    var timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                    var size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                    DrawOutlinedText(timer, new Vector2(cursorPos.X + barWidth / 2f - size.X / 2f, cursorPos.Y + BarHeight));
                }
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + LilyBarXPadding + barWidth, cursorPos.Y);
            drawList.AddRectFilledMultiColor(cursorPos, cursorPos + barSize, 
                    EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]);

            if (gauge.NumLilies > 1)
            {
                scale = gauge.NumLilies == 2 ? gauge.LilyTimer / lilyCooldown : 1;

                if (gauge.NumLilies == 3)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                        LilyColor["gradientLeft"], LilyColor["gradientRight"], LilyColor["gradientRight"], LilyColor["gradientLeft"]
                    );
                }
                else
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                        ChargingColor["gradientLeft"], ChargingColor["gradientRight"], ChargingColor["gradientRight"], ChargingColor["gradientLeft"]
                    );
                }

                if (scale < 1)
                {
                    var timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                    var size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                    DrawOutlinedText(timer, new Vector2(cursorPos.X + barWidth / 2f - size.X / 2f, cursorPos.Y + BarHeight));
                }
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + LilyBarXPadding + barWidth, cursorPos.Y);
            scale = gauge.NumBloodLily > 0 ? 1 : 0;
            drawList.AddRectFilledMultiColor(cursorPos, cursorPos + barSize, 
                    EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                BloodLilyColor["gradientLeft"], BloodLilyColor["gradientRight"], BloodLilyColor["gradientRight"], BloodLilyColor["gradientLeft"]
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + LilyBarXPadding + barWidth, cursorPos.Y);
            scale = gauge.NumBloodLily > 1 ? 1 : 0;
            drawList.AddRectFilledMultiColor(cursorPos, cursorPos + barSize, 
                    EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                BloodLilyColor["gradientLeft"], BloodLilyColor["gradientRight"], BloodLilyColor["gradientRight"], BloodLilyColor["gradientLeft"]
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + LilyBarXPadding + barWidth, cursorPos.Y);
            scale = gauge.NumBloodLily > 2 ? 1 : 0;
            drawList.AddRectFilledMultiColor(cursorPos, cursorPos + barSize, 
                    EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                BloodLilyColor["gradientLeft"], BloodLilyColor["gradientRight"], BloodLilyColor["gradientRight"], BloodLilyColor["gradientLeft"]
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
    }
}
