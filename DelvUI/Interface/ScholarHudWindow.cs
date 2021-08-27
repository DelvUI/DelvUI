using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class ScholarHudWindow : HudWindow
    {
        public override uint JobId => 28;

        private int FairyBarHeight => PluginConfiguration.FairyBarHeight;

        private int FairyBarWidth => PluginConfiguration.FairyBarWidth;

        private int FairyBarX => PluginConfiguration.FairyBarX;

        private int FairyBarY => PluginConfiguration.FairyBarY;

        private int SchAetherBarHeight => PluginConfiguration.SchAetherBarHeight;

        private int SchAetherBarWidth => PluginConfiguration.SchAetherBarWidth;

        private int SchAetherBarX => PluginConfiguration.SchAetherBarX;

        private int SchAetherBarY => PluginConfiguration.SchAetherBarY;

        private int SchAetherBarPad => PluginConfiguration.SchAetherBarPad;

        private Dictionary<string, uint> SchAetherColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000];

        private Dictionary<string, uint> SchFairyColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000 + 1];

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000 + 2];

        private new Vector2 BarSize { get; set; }

        private Vector2 BarCoords { get; set; }

        public ScholarHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawFairyBar();
            DrawAetherBar();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        private void DrawFairyBar()
        {
            var gauge = (float)PluginInterface.ClientState.JobGauges.Get<SCHGauge>().FairyGaugeAmount;
            BarSize = new Vector2(FairyBarWidth, FairyBarHeight);
            BarCoords = new Vector2(FairyBarX, FairyBarY);
            var cursorPos = new Vector2(CenterX - BarCoords.X, CenterY + BarCoords.Y - 49);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(BarSize.X * gauge / 100, BarSize.Y),
                SchFairyColor["gradientLeft"], SchFairyColor["gradientRight"], SchFairyColor["gradientRight"], SchFairyColor["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            DrawOutlinedText(gauge.ToString(CultureInfo.InvariantCulture), new Vector2(cursorPos.X + BarSize.X * gauge / 100 - (gauge == 100 ? 30 : gauge > 3 ? 20 : 0), cursorPos.Y + (BarSize.Y / 2) - 12));
        }

        private void DrawAetherBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var aetherFlowBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 304);
            var barWidth = (SchAetherBarWidth / 3);
            BarSize = new Vector2(barWidth, SchAetherBarHeight);
            BarCoords = new Vector2(SchAetherBarX, SchAetherBarY);
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