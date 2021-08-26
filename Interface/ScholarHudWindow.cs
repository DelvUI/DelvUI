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
        private Vector2 _barsize;
        private Vector2 _barcoords;

        private new int BarHeight => 20;
        private new int BarWidth => 250;
        private new int XOffset => 127;
        private new int YOffset => 461;
       // protected int ManaBarHeight => PluginConfiguration.ManaBarHeight;
        //protected int ManaBarWidth => PluginConfiguration.ManaBarWidth;

        protected int FairyBarHeight => PluginConfiguration.FairyBarHeight;
        protected int FairyBarWidth => PluginConfiguration.FairyBarWidth;
        protected int FairyBarX => PluginConfiguration.FairyBarX;
        protected int FairyBarY => PluginConfiguration.FairyBarY;
        protected int SchAetherBarHeight => PluginConfiguration.SchAetherBarHeight;
        protected int SchAetherBarWidth => PluginConfiguration.SchAetherBarWidth;
        protected int SchAetherBarX => PluginConfiguration.SchAetherBarX;
        protected int SchAetherBarY => PluginConfiguration.SchAetherBarY;
        protected int SchAetherBarPad => PluginConfiguration.SchAetherBarPad;

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

        /**protected override void DrawPrimaryResourceBar()
        {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float)actor.CurrentMp / actor.MaxMp;
            _barsize = new Vector2(ManaBarWidth, ManaBarHeight);
            //var barSize = new Vector2(250, 13);
            var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 27);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + _barsize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(_barsize.X * scale, _barsize.Y),
                0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00
            );
            drawList.AddRect(cursorPos, cursorPos + _barsize, 0xFF000000);
        }**/

        private void DrawFairyBar()
        {
            var gauge = (float)PluginInterface.ClientState.JobGauges.Get<SCHGauge>().FairyGaugeAmount;
            //var barSize = new Vector2(BarWidth, BarHeight);
            _barsize = new Vector2(FairyBarWidth, FairyBarHeight);
            _barcoords = new Vector2(FairyBarX, FairyBarY);
            var cursorPos = new Vector2(CenterX - BarCoords.X, CenterY + BarCoords.Y - 49);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(BarSize.X * gauge / 100, BarSize.Y),
                0x7097CE0D, 0xFF5EFB09, 0xFF5EFB09, 0x7097CE0D
            );

            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            DrawOutlinedText(gauge.ToString(), new Vector2(cursorPos.X+BarSize.X * gauge/100-(gauge==100?30:gauge>3?20:0), cursorPos.Y + (BarSize.Y / 2) - 12));
        }

        private void DrawAetherBar()
        {
            var aetherFlowBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 304);
            //var xPadding = 2;
            var barWidth = (SchAetherBarWidth / 3);
            _barsize = new Vector2(barWidth, SchAetherBarHeight);
            _barcoords = new Vector2(SchAetherBarX, SchAetherBarY);
            //var cursorPos = new Vector2(CenterX - 43 + BarCoords.X, CenterY + BarCoords.Y - 71);
            var cursorPos = new Vector2(CenterX + BarCoords.X, CenterY + BarCoords.Y - 71);
            //var barSize = new Vector2(barWidth, BarHeight);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X - barWidth*2 - SchAetherBarPad * 2, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

            switch (aetherFlowBuff.StackCount)
            {
                case 1:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

                    break;
                case 2:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    break;
                case 3:
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + BarSize, 0xFF08FF00);
                    drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                    break;
            }

        }
    }
}