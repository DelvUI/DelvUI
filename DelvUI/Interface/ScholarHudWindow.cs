using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using DelvUI.Config;

namespace DelvUI.Interface
{
    public class ScholarHudWindow : HudWindow
    {
        public override uint JobId => 28;

        private int BaseXOffset => PluginConfiguration.SCHBaseXOffset;
        private int BaseYOffset => PluginConfiguration.SCHBaseYOffset;

        private int FairyBarHeight => PluginConfiguration.FairyBarHeight;
        private int FairyBarWidth => PluginConfiguration.FairyBarWidth;
        private int FairyBarX => PluginConfiguration.FairyBarX;
        private int FairyBarY => PluginConfiguration.FairyBarY;

        private int SchAetherBarHeight => PluginConfiguration.SchAetherBarHeight;
        private int SchAetherBarWidth => PluginConfiguration.SchAetherBarWidth;
        private int SchAetherBarX => PluginConfiguration.SchAetherBarX;
        private int SchAetherBarY => PluginConfiguration.SchAetherBarY;
        private int SchAetherBarPad => PluginConfiguration.SchAetherBarPad;

        private int BioBarHeight => PluginConfiguration.SCHBioBarHeight;
        private int BioBarWidth => PluginConfiguration.SCHBioBarWidth;
        private int BioBarX => PluginConfiguration.SCHBioBarX;
        private int BioBarY => PluginConfiguration.SCHBioBarY;

        private bool ShowBioBar => PluginConfiguration.SCHShowBioBar;
        private bool ShowFairyBar => PluginConfiguration.SCHShowFairyBar;
        private bool ShowAetherBar => PluginConfiguration.SCHShowAetherBar;
        private bool ShowPrimaryResourceBar => PluginConfiguration.SCHShowPrimaryResourceBar;

        private Dictionary<string, uint> SchAetherColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000];

        private Dictionary<string, uint> SchFairyColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000 + 1];

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000 + 2];
        private Dictionary<string, uint> SCHBioColor => PluginConfiguration.JobColorMap[Jobs.SCH * 1000 + 3];

        private new Vector2 BarSize { get; set; }
        private Vector2 BarCoords { get; set; }

        public ScholarHudWindow(
            ClientState clientState,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable, 
            PluginConfiguration pluginConfiguration,
            SigScanner sigScanner,
            TargetManager targetManager,
            UiBuilder uiBuilder
        ) : base(
            clientState,
            pluginInterface,
            dataManager,
            framework,
            gameGui,
            jobGauges,
            objectTable,
            pluginConfiguration,
            sigScanner,
            targetManager,
            uiBuilder
        ) { }

        protected override void Draw(bool _) {
            if (ShowFairyBar) {
                DrawFairyBar();
            }
            
            if (ShowBioBar) {
                DrawBioBar();
            }
            
            if (ShowAetherBar) {
                DrawAetherBar();
            }
        }

        protected override void DrawPrimaryResourceBar() {
            if (!ShowPrimaryResourceBar) {
                return;
            }

            base.DrawPrimaryResourceBar();
        }

        private void DrawFairyBar() {
            var gauge = (float)JobGauges.Get<SCHGauge>().FairyGauge;
            BarSize = new Vector2(FairyBarWidth, FairyBarHeight);
            BarCoords = new Vector2(FairyBarX - BaseXOffset, FairyBarY + BaseYOffset);
            var cursorPos = new Vector2(CenterX - BarCoords.X, CenterY + BarCoords.Y - 49);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["background"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(BarSize.X * gauge / 100, BarSize.Y),
                SchFairyColor["gradientLeft"], SchFairyColor["gradientRight"], SchFairyColor["gradientRight"], SchFairyColor["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            DrawOutlinedText(gauge.ToString(CultureInfo.InvariantCulture), new Vector2(cursorPos.X + BarSize.X * gauge / 100 - (gauge == 100 ? 30 : gauge > 3 ? 20 : 0), cursorPos.Y + (BarSize.Y / 2) - 12));
        }

        private void DrawAetherBar() {
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var aetherFlowBuff = ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 304);
            var barWidth = (SchAetherBarWidth / 3);
            BarSize = new Vector2(barWidth, SchAetherBarHeight);
            BarCoords = new Vector2(SchAetherBarX + BaseXOffset, SchAetherBarY + BaseYOffset);
            var cursorPos = new Vector2(CenterX + BarCoords.X, CenterY + BarCoords.Y - 71);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["background"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barWidth + SchAetherBarPad, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barWidth+1, SchAetherBarHeight), EmptyColor["background"]);
            drawList.AddRect(cursorPos, cursorPos + new Vector2(barWidth + 1, SchAetherBarHeight), 0xFF000000);
            cursorPos = new Vector2(cursorPos.X - barWidth*2 - SchAetherBarPad * 2, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["background"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

            var stackCount = aetherFlowBuff?.StackCount ?? 0;
            switch (stackCount) {
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
                    drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barWidth + 1, SchAetherBarHeight), SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + new Vector2(barWidth + 1, SchAetherBarHeight), 0xFF000000);
                    break;
            }
        }

        private void DrawBioBar() {
            var actor = TargetManager.SoftTarget ?? TargetManager.Target;
            BarSize = new Vector2(BioBarWidth, BioBarHeight);
            BarCoords = new Vector2(BioBarX, BioBarY);
            var xOffset = CenterX + BaseXOffset - BarCoords.X;
            var yOffset = CenterY + BaseYOffset + BarCoords.Y;
            var cursorPos = new Vector2(xOffset, yOffset);

            var drawList = ImGui.GetWindowDrawList();;

            if (actor is not BattleChara target)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["background"]);
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                return;
            }
            
            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var bio = target.StatusList.FirstOrDefault(o => o.StatusId == 179 && o.SourceID == ClientState.LocalPlayer.ObjectId ||
                                                               o.StatusId == 189 && o.SourceID == ClientState.LocalPlayer.ObjectId ||
                                                               o.StatusId == 1895 && o.SourceID == ClientState.LocalPlayer.ObjectId);
            var bioDuration = bio?.RemainingTime ?? 0f;

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["background"]);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2((BarSize.X / 30) * bioDuration, BarSize.Y), SCHBioColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            DrawOutlinedText(bioDuration.ToString(CultureInfo.InvariantCulture), new Vector2(cursorPos.X + BarSize.X * bioDuration / 30 - (bioDuration == 30 ? 30 : bioDuration > 3 ? 20 : 0), cursorPos.Y + (BarSize.Y / 2) - 12));
        }
    }
}