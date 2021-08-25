using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface
{
    public class ScholarHudWindow : HudWindow
    {
        public override uint JobId => 28;

        private new int BarHeight => 20;
        private new int BarWidth => 250;
        private new int XOffset => 127;
        private new int YOffset => 461;

        public ScholarHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawFairyBar();
            DrawAetherBar();
            DrawTargetBar();
            DrawFocusBar();
        }

        protected override void DrawPrimaryResourceBar()
        {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float)actor.CurrentMp / actor.MaxMp;
            var barSize = new Vector2(250, 13);
            var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 27);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private void DrawFairyBar()
        {
            var gauge = (float)PluginInterface.ClientState.JobGauges.Get<SCHGauge>().FairyGaugeAmount;
            var barSize = new Vector2(BarWidth, BarHeight);
            var cursorPos = new Vector2(CenterX - XOffset, CenterY + YOffset - 49);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * gauge / 100, barSize.Y),
                0x7097CE0D, 0xFF5EFB09, 0xFF5EFB09, 0x7097CE0D
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            DrawOutlinedText(gauge.ToString(), new Vector2(cursorPos.X+barSize.X * gauge/100-(gauge==100?30:gauge>5?20:0), cursorPos.Y+-2));
        }

        private void DrawAetherBar()
        {
            var aetherFlowBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 304);
            var xPadding = 2;
            var barWidth = (BarWidth / 3) - 1;
            var cursorPos = new Vector2(CenterX - 43, CenterY + YOffset - 71);
            var barSize = new Vector2(barWidth, BarHeight);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X - barWidth*2 - xPadding*2, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            switch (aetherFlowBuff.StackCount)
            {
                case 1:
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                    break;
                case 2:
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    break;
                case 3:
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    break;
            }

        }
    }
}