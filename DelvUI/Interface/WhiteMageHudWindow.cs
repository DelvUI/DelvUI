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
    public class WhiteMageHudWindow : HudWindow
    {
        public override uint JobId => 24;

        private int BaseXOffset => PluginConfiguration.WHMBaseXOffset;
        private int BaseYOffset => PluginConfiguration.WHMBaseYOffset;
		
        private int LillyBarHeight => PluginConfiguration.LillyBarHeight;
        private int LillyBarWidth => PluginConfiguration.LillyBarWidth;
        private int LillyBarX => PluginConfiguration.LillyBarX;
        private int LillyBarY => PluginConfiguration.LillyBarY;
        private int LillyBarPad => PluginConfiguration.LillyBarPad;

        private int BloodLillyBarHeight => PluginConfiguration.BloodLillyBarHeight;
        private int BloodLillyBarWidth => PluginConfiguration.BloodLillyBarWidth;
        private int BloodLillyBarX => PluginConfiguration.BloodLillyBarX;
        private int BloodLillyBarY => PluginConfiguration.BloodLillyBarY;
        private int BloodLillyBarPad => PluginConfiguration.BloodLillyBarPad;

        private int DiaBarHeight => PluginConfiguration.DiaBarHeight;
        private int DiaBarWidth => PluginConfiguration.DiaBarWidth;
        private int DiaBarX => PluginConfiguration.DiaBarX;
        private int DiaBarY => PluginConfiguration.DiaBarY;

        private bool ShowDiaBar => PluginConfiguration.WHMShowDiaBar;
        private bool ShowLillyBar => PluginConfiguration.WHMShowLillyBar;
        private bool ShowPrimaryResourceBar => PluginConfiguration.WHMShowPrimaryResourceBar;

        private Dictionary<string, uint> LillyColor => PluginConfiguration.JobColorMap[Jobs.WHM * 1000];
        private Dictionary<string, uint> LillyChargingColor => PluginConfiguration.JobColorMap[Jobs.WHM * 1000 + 3];
        private Dictionary<string, uint> BloodLillyColor => PluginConfiguration.JobColorMap[Jobs.WHM * 1000 + 1];
        private Dictionary<string, uint> WhmDiaColor => PluginConfiguration.JobColorMap[Jobs.WHM * 1000 + 4];
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.WHM * 1000 + 2];

        private new Vector2 BarSize { get; set; }
        private Vector2 BarCoords { get; set; }

        public WhiteMageHudWindow(
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
            if (ShowLillyBar) {
                DrawSecondaryResourceBar();
            }

            if (ShowDiaBar) {
                DrawDiaBar();
            }
        }

        protected override void DrawPrimaryResourceBar() {
            if (!ShowPrimaryResourceBar) {
                return;
            }

            base.DrawPrimaryResourceBar();
        }

        private void DrawDiaBar() {
            var actor = TargetManager.SoftTarget ?? TargetManager.Target;
            BarSize = new Vector2(DiaBarWidth, DiaBarHeight);
            BarCoords = new Vector2(DiaBarX + BaseXOffset, DiaBarY + BaseYOffset);
            var cursorPos = new Vector2(CenterX - BarCoords.X, CenterY + BarCoords.Y);

            var drawList = ImGui.GetWindowDrawList();
            //var barSize = new Vector2(barWidth, 20);

            if (actor is not BattleChara target)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["background"]);
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                return;
            }

            Debug.Assert(ClientState.LocalPlayer != null, "ClientState.LocalPlayer != null");
            var dia = target.StatusList.FirstOrDefault(o => o.StatusId == 1871 && o.SourceID == ClientState.LocalPlayer.ObjectId ||
                                                            o.StatusId == 144 && o.SourceID == ClientState.LocalPlayer.ObjectId ||
                                                            o.StatusId == 143 && o.SourceID == ClientState.LocalPlayer.ObjectId);
            var diaCooldown = dia?.StatusId == 1871 ? 30f : 18f;
            var diaDuration = dia?.RemainingTime ?? 0f;

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["background"]);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2((BarSize.X / diaCooldown) * diaDuration, BarSize.Y), WhmDiaColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            DrawOutlinedText(string.Format(CultureInfo.InvariantCulture, "{0,2:N0}", diaDuration), // keeps 10 -> 9 from jumping
                new Vector2(
                    // smooths transition of counter to the right of the emptying bar
                    cursorPos.X + BarSize.X * diaDuration / diaCooldown - (diaDuration == diaCooldown ? diaCooldown : diaDuration > 3 ? 20 : diaDuration * (20f / 3f)), 
                    cursorPos.Y + (BarSize.Y / 2) - 12
                )
            );
        }

        private void DrawSecondaryResourceBar() {
            var gauge = JobGauges.Get<WHMGauge>();

            BarSize = new Vector2(LillyBarWidth, LillyBarHeight);
            BarCoords = new Vector2(LillyBarX + BaseXOffset, LillyBarY + BaseYOffset);

            var xPadding = LillyBarPad;
            const int numChunks = 6;

            var barWidth = (BarSize.X - xPadding * (numChunks - 1)) / numChunks;
            var barSize = new Vector2(barWidth, BarSize.Y);
            var xPos = CenterX - BarCoords.X;
            var yPos = CenterY + BarCoords.Y - 20;

            const float lilyCooldown = 30000f;

            var cursorPos = new Vector2(xPos, yPos);
            var drawList = ImGui.GetWindowDrawList();

            var scale = gauge.Lily == 0 ? gauge.LilyTimer / lilyCooldown : 1;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            if (gauge.Lily >= 1) {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, LillyBarHeight),
                    LillyColor["gradientLeft"], LillyColor["gradientRight"], LillyColor["gradientRight"], LillyColor["gradientLeft"]
                );
            }
            else {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, LillyBarHeight),
                    LillyChargingColor["gradientLeft"], LillyChargingColor["gradientRight"], LillyChargingColor["gradientRight"], LillyChargingColor["gradientLeft"]
                );
            }

            if (scale < 1) {
                var timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                var size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                DrawOutlinedText(timer, new Vector2(cursorPos.X + barWidth / 2f - size.X / 2f, cursorPos.Y - 23));
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            if (gauge.Lily > 0) {
                scale = gauge.Lily == 1 ? gauge.LilyTimer / lilyCooldown : 1;

                if (gauge.Lily >= 2) {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, LillyBarHeight),
                        LillyColor["gradientLeft"], LillyColor["gradientRight"], LillyColor["gradientRight"], LillyColor["gradientLeft"]
                    );
                }
                else {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, LillyBarHeight),
                        LillyChargingColor["gradientLeft"], LillyChargingColor["gradientRight"], LillyChargingColor["gradientRight"], LillyChargingColor["gradientLeft"]
                    );
                }

                if (scale < 1) {
                    var timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                    var size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                    DrawOutlinedText(timer, new Vector2(cursorPos.X + barWidth / 2f - size.X / 2f, cursorPos.Y - 23));
                }
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            if (gauge.Lily > 1) {
                scale = gauge.Lily == 2 ? gauge.LilyTimer / lilyCooldown : 1;
                
                if (gauge.Lily == 3) {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, LillyBarHeight),
                        LillyColor["gradientLeft"], LillyColor["gradientRight"], LillyColor["gradientRight"], LillyColor["gradientLeft"]
                    );
                }
                else {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, LillyBarHeight),
                        LillyChargingColor["gradientLeft"], LillyChargingColor["gradientRight"], LillyChargingColor["gradientRight"], LillyChargingColor["gradientLeft"]
                    );
                }

                if (scale < 1) {
                    var timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                    var size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                    DrawOutlinedText(timer, new Vector2(cursorPos.X + barWidth / 2f - size.X / 2f, cursorPos.Y - 23));
                }
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            // Blood Lilies
            BarSize = new Vector2(BloodLillyBarWidth, BloodLillyBarHeight);
            BarCoords = new Vector2(BloodLillyBarX + BaseXOffset, BloodLillyBarY +  BaseYOffset);

            barWidth = (BarSize.X - xPadding * (numChunks - 1)) / numChunks;
            barSize = new Vector2(barWidth, BarSize.Y);
            xPos = CenterX - BarCoords.X;
            yPos = CenterY + BarCoords.Y - 20;
            xPadding = BloodLillyBarPad;

            cursorPos = new Vector2(xPos + xPadding + barWidth, yPos);
            scale = gauge.BloodLily > 0 ? 1 : 0;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                BloodLillyColor["gradientLeft"], BloodLillyColor["gradientRight"], BloodLillyColor["gradientRight"], BloodLillyColor["gradientLeft"]
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            scale = gauge.BloodLily > 1 ? 1 : 0;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                BloodLillyColor["gradientLeft"], BloodLillyColor["gradientRight"], BloodLillyColor["gradientRight"], BloodLillyColor["gradientLeft"]
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + xPadding + barWidth, cursorPos.Y);
            scale = gauge.BloodLily > 2 ? 1 : 0;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                BloodLillyColor["gradientLeft"], BloodLillyColor["gradientRight"], BloodLillyColor["gradientRight"], BloodLillyColor["gradientLeft"]
            );
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
    }
}