using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface
{
    public class WhiteMageHudWindow : HudWindow
    {
        public override uint JobId => 24;

        private int BarHeight => 13;
        private int BarWidth => 254;
        private new int XOffset => 127;
        private new int YOffset => 466;

        public WhiteMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

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

            const int xPadding = 4;
            const int numChunks = 6;

            var barWidth = (BarWidth - xPadding * (numChunks - 1)) / numChunks;
            var barSize = new Vector2(barWidth, BarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 20;

            const float lilyCooldown = 30000f;

            var cursorPos = new Vector2(xPos, yPos);
            var drawList = ImGui.GetWindowDrawList();

            var scale = gauge.NumLilies == 0 ? gauge.LilyTimer / lilyCooldown : 1;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (gauge.NumLilies >= 1)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                    0xFFD8D8D8, 0xFFFEFEFE, 0xFFFEFEFE, 0xFFD8D8D8
                );
            }
            else
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                    0xFF90827C, 0xFF8E8D8F, 0xFF8E8D8F, 0xFF90827C
                );
            }

            if (scale < 1)
            {
                var timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                var size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                DrawOutlinedText(timer, new Vector2(cursorPos.X + barWidth / 2f - size.X / 2f, cursorPos.Y + BarHeight));
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (gauge.NumLilies > 0)
            {
                scale = gauge.NumLilies == 1 ? gauge.LilyTimer / lilyCooldown : 1;

                if (gauge.NumLilies >= 2)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                        0xFFD8D8D8, 0xFFFEFEFE, 0xFFFEFEFE, 0xFFD8D8D8
                    );
                }
                else
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                        0xFF90827C, 0xFF8E8D8F, 0xFF8E8D8F, 0xFF90827C
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

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (gauge.NumLilies > 1)
            {
                scale = gauge.NumLilies == 2 ? gauge.LilyTimer / lilyCooldown : 1;

                if (gauge.NumLilies == 3)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                        0xFFD8D8D8, 0xFFFEFEFE, 0xFFFEFEFE, 0xFFD8D8D8
                    );
                }
                else
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                        0xFF90827C, 0xFF8E8D8F, 0xFF8E8D8F, 0xFF90827C
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

            // Blood Lilies
            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            scale = gauge.NumBloodLily > 0 ? 1 : 0;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                0xFF3D009B, 0xFF4D25DD, 0xFF4D25DD, 0xFF3D009B
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            scale = gauge.NumBloodLily > 1 ? 1 : 0;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                0xFF3D009B, 0xFF4D25DD, 0xFF4D25DD, 0xFF3D009B
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            scale = gauge.NumBloodLily > 2 ? 1 : 0;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                0xFF3D009B, 0xFF4D25DD, 0xFF4D25DD, 0xFF3D009B
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
    }
}

