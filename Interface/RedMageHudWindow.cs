using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public class RedMageHudWindow : HudWindow {
        public override uint JobId => 35;
        
        private new int BarHeight => 20;
        private new int BarWidth => 254;
        private new int XOffset => 127;
        private new int YOffset => 440;
        
        public RedMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawWhiteManaBar();
            DrawBlackManaBar();
            DrawAccelBar();
            DrawDualCastBar();
            DrawCrystalBar();
            DrawTargetBar();
        }
        protected override void DrawPrimaryResourceBar() {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentMp / actor.MaxMp;
            var barSize = new Vector2(BarWidth, BarHeight);
            var cursorPos = new Vector2(CenterX - XOffset, CenterY + YOffset);
            
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            if (actor.CurrentMp >= 2600)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y), 
                    0xFFE6CD00, 0xFFD8Df3C, 0xFFD8Df3C, 0xFFE6CD00
                ); 
            }
            else
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y), 
                    0xFF2e2ec7, 0xFF2e2ec7, 0xFF2e2ec7, 0xFF2e2ec7
                );
            }
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X*0.26f, barSize.Y), 0xFF000000);
            DrawOutlinedText(actor.CurrentMp.ToString(), new Vector2(CenterX-20, cursorPos.Y-2));



        }
        
        private void DrawWhiteManaBar() {
            var gauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().WhiteGauge;
            var barSize = new Vector2(BarWidth, BarHeight);
            var cursorPos = new Vector2(CenterX - XOffset, CenterY + YOffset - 22);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * gauge/100, barSize.Y), 
                0xFFEEECF6, 0xFFEEECF6, 0xFFEEECF6, 0xFFEEECF6
            ); 
            
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X*0.8f, barSize.Y), 0xFF000000);
            DrawOutlinedText(gauge.ToString(), new Vector2(cursorPos.X+barSize.X * gauge/100-(gauge==100?30:gauge>10?20:0), cursorPos.Y+-2));


        }        
        
        private void DrawBlackManaBar() {
            var gauge = (float) PluginInterface.ClientState.JobGauges.Get<RDMGauge>().BlackGauge;
            var barSize = new Vector2(BarWidth, BarHeight);
            var cursorPos = new Vector2(CenterX - XOffset, CenterY + YOffset - 44);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * gauge/100, barSize.Y), 
                0xFFCE503C, 0xFFCE503C, 0xFFCE503C, 0xFFCE503C
            ); 
            
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            drawList.AddRect(cursorPos, new Vector2(cursorPos.X + barSize.X*0.8f, cursorPos.Y + barSize.Y), 0xFF000000);
            DrawOutlinedText(gauge.ToString(), new Vector2(cursorPos.X+barSize.X * gauge/100-(gauge==100?30:gauge>10?20:0), cursorPos.Y+-2));

        }  
        
        private void DrawAccelBar() {
            var barSize = new Vector2(BarWidth/3-1, BarHeight/2);
            var cursorPos = new Vector2(CenterX - XOffset, CenterY + YOffset - 55);
            var drawList = ImGui.GetWindowDrawList();
            var accelBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1238);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0x88000000);
            
            drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barSize.X + 2, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barSize.X + 3, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0xFF000000);
            if (accelBuff.Count() != 1) return;
            cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 55);
            switch (accelBuff.First().StackCount)
            {
                case 1:
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 
                        0xFF2000FC, 0xFF2000FC, 0xFF2000FC, 0xFF2000FC
                    );
                    drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0xFF000000);
                    break;
                case 2:
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 
                        0xFF2000FC, 0xFF2000FC, 0xFF2000FC, 0xFF2000FC
                    );
                    drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barSize.X + 2, cursorPos.Y);
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 
                        0xFF2000FC, 0xFF2000FC, 0xFF2000FC, 0xFF2000FC
                    );
                    drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0xFF000000);
                    break;
                case 3:
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 
                        0xFF2000FC, 0xFF2000FC, 0xFF2000FC, 0xFF2000FC
                    );
                    drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barSize.X + 2, cursorPos.Y);
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 
                        0xFF2000FC, 0xFF2000FC, 0xFF2000FC, 0xFF2000FC
                    );
                    drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barSize.X + 3, cursorPos.Y);
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 
                        0xFF2000FC, 0xFF2000FC, 0xFF2000FC, 0xFF2000FC
                    );
                    drawList.AddRect(cursorPos, cursorPos + new Vector2(barSize.X , barSize.Y), 0xFF000000);
                    break;
            }

        }    
        
        private void DrawDualCastBar() {
            var barSize = new Vector2(BarWidth, BarHeight);
            var cursorPos = new Vector2(CenterX - XOffset, CenterY + YOffset - 22);
            var drawList = ImGui.GetWindowDrawList();
            var dualCastBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1249);

            drawList.AddRectFilled(new Vector2(cursorPos.X-2,cursorPos.Y + barSize.Y-53), 
                new Vector2(cursorPos.X-8,cursorPos.Y + barSize.Y*2+2), 0x88000000);
            if (dualCastBuff.Count() == 1)
            {
                drawList.AddRectFilledMultiColor(
                    new Vector2(cursorPos.X-2,cursorPos.Y + barSize.Y-53), 
                    new Vector2(cursorPos.X-8,cursorPos.Y + barSize.Y*2+2), 
                    0xFFF473AE, 0xFFF473AE, 0xFFF473AE, 0xFFF473AE
                );
            }
            drawList.AddRect(new Vector2(cursorPos.X-2,cursorPos.Y + barSize.Y-53), 
                             new Vector2(cursorPos.X-8,cursorPos.Y + barSize.Y*2+2), 0xFF000000);
                
                
        }  
        
        private void DrawCrystalBar() {
            var whiteGauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().WhiteGauge;
            var blackGauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().BlackGauge;
            var gaugeDiff = whiteGauge - blackGauge;
            var barSize = new Vector2(BarWidth, BarHeight);
            var cursorPos = new Vector2(CenterX - XOffset, CenterY + YOffset - 22);
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(new Vector2(cursorPos.X + barSize.X+2,cursorPos.Y + barSize.Y-53), 
                new Vector2(cursorPos.X + barSize.X+10,cursorPos.Y + barSize.Y*2+2), 0x88000000);
            if (gaugeDiff >= 30)
            {
                drawList.AddRectFilledMultiColor(
                    new Vector2(cursorPos.X + barSize.X+2,cursorPos.Y + barSize.Y-53), 
                    new Vector2(cursorPos.X + barSize.X+10,cursorPos.Y + barSize.Y*2+2), 
                    0xFFEEECF6, 0xFFEEECF6, 0xFFEEECF6, 0xFFEEECF6
                ); 
            }else if (gaugeDiff <= -30)
            {
                drawList.AddRectFilledMultiColor(
                    new Vector2(cursorPos.X + barSize.X+2,cursorPos.Y + barSize.Y-53), 
                    new Vector2(cursorPos.X + barSize.X+10,cursorPos.Y + barSize.Y*2+2), 
                    0xFFCE503C, 0xFFCE503C, 0xFFCE503C, 0xFFCE503C
                ); 
            }else if (whiteGauge >= 80 && blackGauge >= 80)
            {                
                drawList.AddRectFilledMultiColor(
                    new Vector2(cursorPos.X + barSize.X+2,cursorPos.Y + barSize.Y-53), 
                    new Vector2(cursorPos.X + barSize.X+10,cursorPos.Y + barSize.Y*2+2), 
                    0xFF2e2ec7, 0xFF2e2ec7, 0xFF2e2ec7, 0xFF2e2ec7
                ); 
                
            }

            
            drawList.AddRect(new Vector2(cursorPos.X + barSize.X+2,cursorPos.Y + barSize.Y-53), 
                new Vector2(cursorPos.X + barSize.X+10,cursorPos.Y + barSize.Y*2+2), 0xFF000000);
        }
    }
}