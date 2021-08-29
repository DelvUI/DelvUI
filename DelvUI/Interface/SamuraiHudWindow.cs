using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class SamuraiHudWindow : HudWindow
    {

        public override uint JobId => 34;

        private bool GaugeEnabled => PluginConfiguration.SAMGaugeEnabled;
        protected int GaugeHeight => PluginConfiguration.SAMGaugeHeight;
        protected int GaugeWidth => PluginConfiguration.SAMGaugeWidth;
        protected int GaugeXOffset => PluginConfiguration.SAMGaugeXOffset;
        protected int GaugeYOffset => PluginConfiguration.SAMGaugeYOffset;

        private bool SenEnabled => PluginConfiguration.SAMSenEnabled;
        protected int SenPadding => PluginConfiguration.SAMSenPadding;
        protected int SenHeight => PluginConfiguration.SAMSenHeight;
        protected int SenWidth => PluginConfiguration.SAMSenWidth;
        protected int SenXOffset => PluginConfiguration.SAMSenXOffset;
        protected int SenYOffset => PluginConfiguration.SAMSenYOffset;

        private bool MeditationEnabled => PluginConfiguration.SAMMeditationEnabled;
        protected int MeditationPadding => PluginConfiguration.SAMMeditationPadding;
        protected int MeditationHeight => PluginConfiguration.SAMMeditationHeight;
        protected int MeditationWidth => PluginConfiguration.SAMMeditationWidth;
        protected int MeditationXOffset => PluginConfiguration.SAMMeditationXOffset;
        protected int MeditationYOffset => PluginConfiguration.SAMMeditationYOffset;



        public SamuraiHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            if (GaugeEnabled)
                DrawPrimaryResourceBar();
            if (SenEnabled)
                DrawSenResourceBar();
            if (MeditationEnabled)
                DrawMeditationResourceBar();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        protected override void DrawPrimaryResourceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();

            var xPos = CenterX - XOffset + GaugeXOffset;
            var yPos = CenterY + YOffset + GaugeHeight + GaugeYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            const int chunkSize = 100;
            var barSize = new Vector2(GaugeWidth, GaugeHeight);

            // Kenki Gauge
            var kenki = Math.Min((int)gauge.Kenki, chunkSize);
            var scale = (float)kenki / chunkSize;
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(GaugeWidth * scale, GaugeHeight),
                0xFF5252FF, 0xFF9C9CFF, 0xFF9C9CFF, 0xFF5252FF
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            var textSize = ImGui.CalcTextSize(gauge.Kenki.ToString());
            DrawOutlinedText(gauge.Kenki.ToString(), new Vector2(cursorPos.X + GaugeWidth / 2f - textSize.X / 2f, cursorPos.Y + (barSize.Y / 2) - 12));

        }

        private void DrawSenResourceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();

            const int numChunks = 3;

            var SenBarWidth = (SenWidth - SenPadding * (numChunks - 1)) / numChunks;
            var SenBarSize = new Vector2(SenBarWidth, SenHeight);
            var xPos = CenterX - SenXOffset;
            var yPos = CenterY + SenYOffset;
            var cursorPos = new Vector2(xPos - SenPadding - SenBarWidth, yPos);

            var drawList = ImGui.GetWindowDrawList();

            // Setsu Bar
            cursorPos = new Vector2(cursorPos.X + SenPadding + SenBarWidth, cursorPos.Y);
            if (gauge.HasSetsu()) drawList.AddRectFilled(cursorPos, cursorPos + SenBarSize, 0xFFF7EA59);
            else drawList.AddRectFilled(cursorPos, cursorPos + SenBarSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + SenBarSize, 0xFF000000);

            // Getsu Bar
            cursorPos = new Vector2(cursorPos.X + SenPadding + SenBarWidth, cursorPos.Y);
            if (gauge.HasGetsu()) drawList.AddRectFilled(cursorPos, cursorPos + SenBarSize, 0xFFF77E59);
            else drawList.AddRectFilled(cursorPos, cursorPos + SenBarSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + SenBarSize, 0xFF000000);

            // Ka Bar
            cursorPos = new Vector2(cursorPos.X + SenPadding + SenBarWidth, cursorPos.Y);
            if (gauge.HasKa()) drawList.AddRectFilled(cursorPos, cursorPos + SenBarSize, 0XFF5959F7);
            else drawList.AddRectFilled(cursorPos, cursorPos + SenBarSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + SenBarSize, 0xFF000000);
        }


        private void DrawMeditationResourceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<SAMGauge>();

            const int numChunks = 3;

            var MeditationBarWidth = (MeditationWidth - MeditationPadding * (numChunks - 1)) / numChunks;
            var MeditationBarSize = new Vector2(MeditationBarWidth, MeditationHeight);
            var xPos = CenterX - MeditationXOffset;
            var yPos = CenterY + MeditationYOffset + MeditationHeight;
            var cursorPos = new Vector2(xPos - MeditationPadding - MeditationBarWidth, yPos);

            var drawList = ImGui.GetWindowDrawList();

            // Meditation Stacks
            for (var i = 1; i < 4; i++)
            {
                cursorPos = new Vector2(cursorPos.X + MeditationPadding + MeditationBarWidth, cursorPos.Y);

                if (gauge.MeditationStacks >= i)
                {
                    drawList.AddRectFilled(
                        cursorPos, cursorPos + MeditationBarSize,
                        ImGui.ColorConvertFloat4ToU32(new Vector4(247 / 255f, 163 / 255f, 89 / 255f, 255f))
                    );
                }
                else
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + MeditationBarSize, 0x88000000);
                }

                drawList.AddRect(cursorPos, cursorPos + MeditationBarSize, 0xFF000000);
            }
        }
    }
}