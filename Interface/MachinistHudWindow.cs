using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface
{
    public class MachinistHudWindow : HudWindow
    {
        public override uint JobId => 31;

        private new int BarHeight => 20;
        private new int BarWidth => 254;
        private new int XOffset => 127;
        private new int YOffset => 466;
        private new int InterBarOffset => 2;
        private new int JobInfoOffset => -33;
        // TODO: Rook auto-turret differences?
        private new int[] RobotDuration = {12450, 13950, 15450, 16950, 18450, 19950};
        
        public MachinistHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            DrawOverheatBar(0);
            DrawHeatGauge(1);
            DrawBatteryGauge(2);
            // DrawWildfireBar(0);
            DrawTargetBar();
        }

        private void DrawHeatGauge(short order)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();
            
            var heatColor = 0xFF0D0DC9;
            var emptyColor = 0xFF8E8D8F;
            
            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding) / 2;
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + BarHeight * order + InterBarOffset * order + JobInfoOffset;
            var cursorPos = new Vector2(xPos + barWidth + xPadding, yPos);
            const int chunkSize = 50;
            var barSize = new Vector2(barWidth, BarHeight);
            
            var drawList = ImGui.GetWindowDrawList();
            
            for (var i = 2; i >= 1; i--)
            {
                var heat = Math.Max(Math.Min(gauge.Heat, chunkSize * i) - chunkSize * (i - 1), 0);
                var scale = (float) heat / chunkSize;
            
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                
                if (scale >= 1.0f)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight),
                        heatColor, heatColor, heatColor, heatColor
                    );
                }
                else 
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, BarHeight), 
                        emptyColor, emptyColor, emptyColor, emptyColor
                    );
                }
        
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                var heatText = heat.ToString();
                var textSize = ImGui.CalcTextSize(heatText);
                DrawOutlinedText(heatText, new Vector2(cursorPos.X + BarWidth / 4f - textSize.X / 4f, cursorPos.Y-2));

                cursorPos = new Vector2(cursorPos.X - barWidth - xPadding, cursorPos.Y);
            }
        }

        private void DrawBatteryGauge(short order)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();
            var robotTimeLeft = gauge.RobotTimeRemaining;
            var robotPercentLeft = (float) robotTimeLeft / RobotDuration[gauge.LastRobotBatteryPower / 10 - 5];
            
            var batteryColor = 0xFFFFFF6A;
            var robotColor = 0xFFFF0099;
            var emptyColor = 0xFF8E8D8F;
            
            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding * 9f) / 10;
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + BarHeight * order + InterBarOffset * order + JobInfoOffset;
            var cursorPos = new Vector2(xPos + barWidth * 9 + xPadding * 9, yPos);
            const int chunkSizeEnd = 10;
            const int chunkSizeStart = 50;
            var barSize = new Vector2(barWidth, BarHeight);
            var batteryGaugeHeight = gauge.IsRobotActive() ? BarHeight / 2 : BarHeight;
            
            var drawList = ImGui.GetWindowDrawList();

            int battery = 0;
            float scale = 0;
            
            for (var i = 10; i >= 6; i--)
            {
                battery = Math.Max(Math.Min(gauge.Battery, chunkSizeEnd * i) - chunkSizeEnd * (i - 1), 0);
                scale = (float) battery / chunkSizeEnd;
                
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

                if (scale >= 1.0f)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, batteryGaugeHeight),
                        batteryColor, batteryColor, batteryColor, batteryColor
                    );
                }
                else 
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, batteryGaugeHeight), 
                        emptyColor, emptyColor, emptyColor, emptyColor
                    );
                }

                if (gauge.IsRobotActive())
                {
                    var robotScale = Math.Min(Math.Max((robotPercentLeft - (i - 1) / 10f) * 10f, 0), 1);
                    drawList.AddRectFilledMultiColor(
                        cursorPos + new Vector2(0, batteryGaugeHeight), cursorPos + new Vector2(barWidth * robotScale, batteryGaugeHeight * 2), 
                        robotColor, robotColor, robotColor, robotColor
                    );
                }
            
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos = new Vector2(cursorPos.X - barWidth - xPadding, cursorPos.Y);
            }

            battery = Math.Min((int)gauge.Battery, chunkSizeStart);
            scale = (float) battery / chunkSizeStart;
            cursorPos = new Vector2(xPos, yPos);
            barWidth = (BarWidth - xPadding) / 2;
            barSize = new Vector2(barWidth, BarHeight);
            
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            if (scale >= 1.0f) 
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, batteryGaugeHeight), 
                    batteryColor, batteryColor, batteryColor, batteryColor
                );
            }
            else 
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barWidth * scale, batteryGaugeHeight), 
                    emptyColor, emptyColor, emptyColor, emptyColor
                );
            }
            
            if (gauge.IsRobotActive())
            {
                var robotScale = Math.Max(Math.Min(robotPercentLeft, chunkSizeStart / 100f), 0);
                drawList.AddRectFilledMultiColor(
                    cursorPos + new Vector2(0, batteryGaugeHeight), cursorPos + new Vector2(barWidth * robotScale * 2, batteryGaugeHeight * 2), 
                    robotColor, robotColor, robotColor, robotColor
                );
                
                var durationText = Math.Round(gauge.RobotTimeRemaining / 1000f).ToString();
                DrawOutlinedText(durationText, new Vector2(cursorPos.X + 5f, cursorPos.Y-2));
            }
            
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private void DrawOverheatBar(short order)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<MCHGauge>();
            var displayOverheat = gauge.IsOverheated();
            
            var overheatColor = 0xFF0EEFFF;
            
            var barWidth = BarWidth;
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset + BarHeight * order + InterBarOffset * order + JobInfoOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var barSize = new Vector2(barWidth, BarHeight);
            
            var drawList = ImGui.GetWindowDrawList();
            
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            
            if (displayOverheat)
            {
                var duration = barWidth / 8000f * gauge.OverheatTimeRemaining;
                
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(duration, BarHeight),
                    overheatColor, overheatColor, overheatColor, overheatColor
                );
                
                var durationText = Math.Round(gauge.OverheatTimeRemaining / 1000f).ToString();
                var textSize = ImGui.CalcTextSize(durationText);
                DrawOutlinedText(durationText, new Vector2(cursorPos.X + BarWidth / 2f - textSize.X / 2f, cursorPos.Y-2));
            }
            
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        // TODO: Implement as optional
        // private void DrawWildfireBar(short order)
        // {
        //     var wildfireBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1946);
        //     
        //     var wildfireColor = 0xFF0000FF;
        //
        //     var barWidth = BarWidth;
        //     var xPos = CenterX - XOffset;
        //     var yPos = CenterY + YOffset + BarHeight * order + InterBarOffset * order + JobInfoOffset;
        //     var cursorPos = new Vector2(xPos, yPos);
        //     var barSize = new Vector2(barWidth, BarHeight);
        //
        //     var drawList = ImGui.GetWindowDrawList();
        //
        //     float duration = 0;
        //     drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
        //     if (wildfireBuff.Any())
        //     {
        //         duration = Math.Abs(wildfireBuff.First().Duration);
        //         drawList.AddRectFilledMultiColor(
        //             cursorPos, cursorPos + new Vector2(barSize.X / 10 * duration, barSize.Y),
        //             wildfireColor, wildfireColor, wildfireColor, wildfireColor
        //         );
        //     }
        //     
        //     drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        //     
        //     var durationText = duration != 0 ? Math.Round(duration).ToString() : "";
        //     var textSize = ImGui.CalcTextSize(durationText);
        //     DrawOutlinedText(durationText, new Vector2(cursorPos.X + BarWidth / 2f - textSize.X / 2f, cursorPos.Y-2));
        // }
    }
}