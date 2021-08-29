using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface {
    public class RedMageHudWindow : HudWindow {
        public override uint JobId => Jobs.BLM;

        private float OriginY => CenterY + YOffset + PluginConfiguration.RDMVerticalOffset;
        private float OriginX => CenterY + XOffset + PluginConfiguration.RDMHorizontalOffset;
        private int VerticalSpaceBetweenBars => PluginConfiguration.RDMVerticalSpaceBetweenBars;
        private int HorizontalSpaceBetweenBars => PluginConfiguration.RDMHorizontalSpaceBetweenBars;
        private int ManaBarWidth => PluginConfiguration.RDMManaBarWidth;
        private int ManaBarHeight => PluginConfiguration.RDMManaBarHeight;
        private int BlackManaBarHeight => PluginConfiguration.RDMBlackManaBarHeight;
        private int BlackManaBarWidth => PluginConfiguration.RDMBlackManaBarWidth;
        private int WhiteManaBarHeight => PluginConfiguration.RDMWhiteManaBarHeight;
        private int WhiteManaBarWidth => PluginConfiguration.RDMWhiteManaBarWidth;        
        private int AccelBarHeight => PluginConfiguration.RDMAccelerationBarHeight;
        private int AccelBarWidth => PluginConfiguration.RDMAccelerationBarWidth;
        private int BalanceBarHeight => PluginConfiguration.RDMBalanceBarHeight;
        private int BalanceBarWidth => PluginConfiguration.RDMBalanceBarWidth;
        private bool ShowManaValue => PluginConfiguration.RDMShowManaValue;
        private bool ShowManaThresholdMarker => PluginConfiguration.RDMShowManaThresholdMarker;
        private int ManaThresholdValue => PluginConfiguration.RDMManaThresholdValue;
        private bool ShowDualCast => PluginConfiguration.RDMShowDualCast;
        private int DualCastHeight => PluginConfiguration.RDMDualCastHeight;
        private int DualCastWidth => PluginConfiguration.RDMDualCastWidth;
        private bool ShowVerfireProcs => PluginConfiguration.RDMShowVerfireProcs;
        private bool ShowVerstoneProcs => PluginConfiguration.RDMShowVerstoneProcs;
        private int ProcsHeight => PluginConfiguration.RDMProcsHeight;
        private bool ShowDotTimer => PluginConfiguration.RDMShowDotTimer;

        private Dictionary<string, uint> ManaBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000];
        private Dictionary<string, uint> ManaBarBelowThresholdColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 1];
        private Dictionary<string, uint> WhiteManaBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 2];
        private Dictionary<string, uint> BlackManaBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 3];
        private Dictionary<string, uint> AccelBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 4];
        private Dictionary<string, uint> DualcastBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 5];
        private Dictionary<string, uint> VerfireBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 6];
        private Dictionary<string, uint> VerthunderBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 7];
        private Dictionary<string, uint> DotColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 8];
        private Dictionary<string, uint> BalanceColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 9];

        public RedMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawWhiteManaBar();
            DrawBlackManaBar();
            DrawWhiteManaBar();
            DrawBlackManaBar();
            DrawAccelBar();
            DrawBalanceBar();

            if (ShowDotTimer)
            {
                DrawDotTimer();
            }

            if (ShowVerfireProcs)
            {
                //DrawVerfireRdyBar();

            }

            if (ShowVerstoneProcs)
            {
                //DrawVerstoneRdyBar();
            }

            if (ShowDualCast)
            {
                DrawDualCastBar();
            }
        }

        protected override void DrawPrimaryResourceBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentMp / actor.MaxMp;
            var barSize = new Vector2(ManaBarWidth, ManaBarHeight);
            var cursorPos = new Vector2(OriginX - barSize.X / 2, OriginY - barSize.Y);
            var color = ManaBarColor;

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, color["background"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            if (ShowManaThresholdMarker)
            {
                var position = new Vector2(OriginX - barSize.X / 2 + (ManaThresholdValue / 10000f) * barSize.X, cursorPos.Y + barSize.Y);
                var size = new Vector2(3, barSize.Y);
                drawList.AddRect(cursorPos, position - size, 0xFF000000);
                
                if(actor.CurrentMp >= ManaThresholdValue)
                {
                    color = ManaBarBelowThresholdColor;
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                        color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
                    );
                }
            }

            if (!ShowManaValue) return;
            var mana = PluginInterface.ClientState.LocalPlayer.CurrentMp;
            var text = $"{mana,0}";
            var textSize = ImGui.CalcTextSize(text);
            DrawOutlinedText(text, new Vector2(OriginX - barSize.X / 2f + 2, OriginY - ManaBarHeight / 2f - textSize.Y / 2f));
            
        }
        
        private void DrawBlackManaBar() {
            var gauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().BlackGauge;
            var scale = gauge / 100;
            var barSize = new Vector2(BlackManaBarWidth, BlackManaBarHeight);
            var cursorPos = new Vector2(OriginX - BlackManaBarWidth / 2, 
                OriginY - ManaBarHeight - VerticalSpaceBetweenBars - BlackManaBarHeight);

            var color = BlackManaBarColor;

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, color["background"]);
            
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            
            drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X*0.8f, barSize.Y), 0xFF000000);
            DrawOutlinedText(gauge.ToString(CultureInfo.InvariantCulture), new Vector2(cursorPos.X+barSize.X * scale-(gauge==100?30:gauge>3?20:0), cursorPos.Y+-2));

        }     
        
        private void DrawBalanceBar() {
            var whiteGauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().WhiteGauge;
            var blackGauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().BlackGauge;
            var scale = whiteGauge - blackGauge;
            var barSize = new Vector2(BalanceBarWidth, BalanceBarHeight);
            var cursorPos = new Vector2(OriginX - HorizontalSpaceBetweenBars - BlackManaBarWidth/2
                                        - HorizontalSpaceBetweenBars - BalanceBarWidth/2,
                OriginY - ManaBarHeight - VerticalSpaceBetweenBars - BalanceBarHeight);

            
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFF000000);

            if (scale >= 30)
            {
                var color = WhiteManaBarColor;
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + barSize,
                    color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
                );
            }else if (scale <= -30)
            {
                var color = BlackManaBarColor;
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + barSize,
                    color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
                ); 
            }else if (whiteGauge >= 80 && blackGauge >= 80)
            {     
                var color = BalanceColor;
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + barSize,
                    color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
                );
                
            }
            
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

        }
        
        private void DrawWhiteManaBar() {
            var gauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().WhiteGauge;
            var scale = gauge / 100;
            var barSize = new Vector2(WhiteManaBarWidth, WhiteManaBarHeight);
            var cursorPos = new Vector2(OriginX - HorizontalSpaceBetweenBars - BlackManaBarWidth/2
                                        - HorizontalSpaceBetweenBars - BalanceBarWidth/2
                                        - HorizontalSpaceBetweenBars - WhiteManaBarWidth / 2, 
                OriginY - ManaBarHeight - VerticalSpaceBetweenBars - WhiteManaBarHeight);
            var color = WhiteManaBarColor;

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, color["background"]);
            
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            
            drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X*0.8f, barSize.Y), 0xFF000000);
            DrawOutlinedText(gauge.ToString(CultureInfo.InvariantCulture), new Vector2(cursorPos.X+barSize.X * scale-(gauge==100?30:gauge>3?20:0), cursorPos.Y+-2));
            
        }        
        
        private void DrawAccelBar() {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var barSize = new Vector2(AccelBarWidth, AccelBarHeight);
            var totalWidth = barSize.X * 3 + HorizontalSpaceBetweenBars * 2;
            var cursorPos = new Vector2(OriginX - totalWidth / 2, OriginY - ManaBarHeight - 
                                                                  VerticalSpaceBetweenBars - BlackManaBarHeight - 
                                                                  VerticalSpaceBetweenBars - AccelBarHeight);
            var accelBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1238);
            if (accelBuff.Count() != 1) return;
            
            var drawList = ImGui.GetWindowDrawList();
            for (int i = 1; i <= 3; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, AccelBarColor["background"]);
                if (accelBuff.First().StackCount >= i)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                        AccelBarColor["gradientLeft"], AccelBarColor["gradientRight"], AccelBarColor["gradientRight"], AccelBarColor["gradientLeft"]
                    );
                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos.X = cursorPos.X + barSize.X + HorizontalSpaceBetweenBars;
            }
            
        }    
        
        private void DrawDualCastBar() {
            var barSize = new Vector2(BalanceBarWidth, BalanceBarHeight);
            var cursorPos = new Vector2(OriginX - DualCastWidth/2 , OriginY - ManaBarHeight - 
                                                                  VerticalSpaceBetweenBars - BlackManaBarHeight - 
                                                                  VerticalSpaceBetweenBars - AccelBarHeight -
                                                                  VerticalSpaceBetweenBars - DualCastHeight);
            var dualCastBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1249);
            if (dualCastBuff.Count() != 1) return;
            
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFF000000);
            if (dualCastBuff.Count() == 1)
            {
                
                var color = DualcastBarColor;
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + barSize,
                    color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
                );
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

        }  
        

        private void DrawVerstoneRdyBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            //var barSize = new Vector2(BarWidth, BarHeight);
            var cursorPos = new Vector2(CenterX - XOffset, CenterY + YOffset - 22);
            var drawList = ImGui.GetWindowDrawList();
            var verstoneBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1235);
            if (verstoneBuff.Count() == 1)
            {
                
            }

        }

        private void DrawVerfireRdyBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
           // var barSize = new Vector2(verfi, BarHeight);
            var cursorPos = new Vector2(CenterX - XOffset, CenterY + YOffset - 22);
            var drawList = ImGui.GetWindowDrawList();
            var verfireBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1234);
            if (verfireBuff.Count() == 1)
            {
                
            }

        }

        private void DrawDotTimer()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

        }
    }
}