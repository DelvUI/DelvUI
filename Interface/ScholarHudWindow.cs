using System;
using System.Collections.Generic;
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
        private Vector2 _barsize;
        private Vector2 _barcoords;

        protected int FairyBarHeight => PluginConfiguration.FairyBarHeight;
        protected int FairyBarWidth => PluginConfiguration.FairyBarWidth;
        protected int FairyBarX => PluginConfiguration.FairyBarX;
        protected int FairyBarY => PluginConfiguration.FairyBarY;
        protected int SchAetherBarHeight => PluginConfiguration.SchAetherBarHeight;
        protected int SchAetherBarWidth => PluginConfiguration.SchAetherBarWidth;
        protected int SchAetherBarX => PluginConfiguration.SchAetherBarX;
        protected int SchAetherBarY => PluginConfiguration.SchAetherBarY;
        protected int SchAetherBarPad => PluginConfiguration.SchAetherBarPad;

        protected Dictionary<string, uint> SchAetherColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000];
        protected Dictionary<string, uint> SchFairyColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000 + 1];
        protected Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000 + 2];

        protected Vector2 BarSize => _barsize;
        protected Vector2 BarCoords => _barcoords;

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

        private void DrawFairyBar()
        {
            var gauge = (float)PluginInterface.ClientState.JobGauges.Get<SCHGauge>().FairyGaugeAmount;
            _barsize = new Vector2(FairyBarWidth, FairyBarHeight);
            _barcoords = new Vector2(FairyBarX, FairyBarY);
            var cursorPos = new Vector2(CenterX - BarCoords.X, CenterY + BarCoords.Y - 49);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(BarSize.X * gauge / 100, BarSize.Y),
                SchFairyColor["gradientLeft"], SchFairyColor["gradientRight"], SchFairyColor["gradientRight"], SchFairyColor["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            DrawOutlinedText(gauge.ToString(), new Vector2(cursorPos.X+BarSize.X * gauge/100-(gauge==100?30:gauge>3?20:0), cursorPos.Y + (BarSize.Y / 2) - 12));

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
            var barWidth = (SchAetherBarWidth / 3);
            _barsize = new Vector2(barWidth, SchAetherBarHeight);
            _barcoords = new Vector2(SchAetherBarX, SchAetherBarY);
            var cursorPos = new Vector2(CenterX + BarCoords.X, CenterY + BarCoords.Y - 71);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X - barWidth*2 - SchAetherBarPad * 2, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

            switch (aetherFlowBuff.StackCount)
            {
                case 1:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

                    break;
                case 2:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    break;
                case 3:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    break;
            }

        }
    }
}