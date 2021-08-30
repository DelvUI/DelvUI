﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface {
    public class RedMageHudWindow : HudWindow {
        public override uint JobId => Jobs.RDM;

        private float OriginY => CenterY + YOffset + PluginConfiguration.RDMVerticalOffset;
        private float OriginX => CenterX + PluginConfiguration.RDMHorizontalOffset;
        private int HorizontalSpaceBetweenBars => PluginConfiguration.RDMHorizontalSpaceBetweenBars;
        private int ManaBarWidth => PluginConfiguration.RDMManaBarWidth;
        private int ManaBarHeight => PluginConfiguration.RDMManaBarHeight;
        private int ManaBarXOffset => PluginConfiguration.RDMManaBarXOffset;
        private int ManaBarYOffset => PluginConfiguration.RDMManaBarYOffset;
        private int WhiteManaBarHeight => PluginConfiguration.RDMWhiteManaBarHeight;
        private int WhiteManaBarWidth => PluginConfiguration.RDMWhiteManaBarWidth;
        private int WhiteManaBarXOffset => PluginConfiguration.RDMWhiteManaBarXOffset;
        private int WhiteManaBarYOffset => PluginConfiguration.RDMWhiteManaBarYOffset;
        private bool WhiteManaBarInversed => PluginConfiguration.RDMWhiteManaBarInversed;
        private bool ShowWhiteManaValue => PluginConfiguration.RDMShowWhiteManaValue;
        private int BlackManaBarHeight => PluginConfiguration.RDMBlackManaBarHeight;
        private int BlackManaBarWidth => PluginConfiguration.RDMBlackManaBarWidth;
        private int BlackManaBarXOffset => PluginConfiguration.RDMBlackManaBarXOffset;
        private int BlackManaBarYOffset => PluginConfiguration.RDMBlackManaBarYOffset;
        private bool BlackManaBarInversed => PluginConfiguration.RDMBlackManaBarInversed;
        private bool ShowBlackManaValue => PluginConfiguration.RDMShowBlackManaValue;
        private int AccelBarHeight => PluginConfiguration.RDMAccelerationBarHeight;
        private int AccelBarWidth => PluginConfiguration.RDMAccelerationBarWidth;
        private int AccelerationBarXOffset => PluginConfiguration.RDMAccelerationBarXOffset;
        private int AccelerationBarYOffset => PluginConfiguration.RDMAccelerationBarYOffset;
        private int BalanceBarHeight => PluginConfiguration.RDMBalanceBarHeight;
        private int BalanceBarWidth => PluginConfiguration.RDMBalanceBarWidth;
        private int BalanceBarXOffset => PluginConfiguration.RDMBalanceBarXOffset;
        private int BalanceBarYOffset => PluginConfiguration.RDMBalanceBarYOffset;
        private bool ShowManaValue => PluginConfiguration.RDMShowManaValue;
        private bool ShowManaThresholdMarker => PluginConfiguration.RDMShowManaThresholdMarker;
        private int ManaThresholdValue => PluginConfiguration.RDMManaThresholdValue;
        private bool ShowDualCast => PluginConfiguration.RDMShowDualCast;
        private int DualCastHeight => PluginConfiguration.RDMDualCastHeight;
        private int DualCastWidth => PluginConfiguration.RDMDualCastWidth;
        private int DualCastXOffset => PluginConfiguration.RDMDualCastXOffset;
        private int DualCastYOffset => PluginConfiguration.RDMDualCastYOffset;
        private bool ShowVerfireProcs => PluginConfiguration.RDMShowVerfireProcs;
        private bool ShowVerstoneProcs => PluginConfiguration.RDMShowVerstoneProcs;
        private int ProcsHeight => PluginConfiguration.RDMProcsHeight;

        private Dictionary<string, uint> ManaBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000];
        private Dictionary<string, uint> ManaBarBelowThresholdColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 1];
        private Dictionary<string, uint> WhiteManaBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 2];
        private Dictionary<string, uint> BlackManaBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 3];
        private Dictionary<string, uint> BalanceColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 4];
        private Dictionary<string, uint> AccelBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 5];
        private Dictionary<string, uint> DualcastBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 6];
        private Dictionary<string, uint> VerstoneBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 7];
        private Dictionary<string, uint> VerfireBarColor => PluginConfiguration.JobColorMap[Jobs.RDM * 1000 + 8];
        

        public RedMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _) 
        {
            DrawBalanceBar();
            DrawWhiteManaBar();
            DrawBlackManaBar();
            DrawAccelerationBar();

            if (ShowDualCast)
            {
                DrawDualCastBar();
            }

            if (ShowVerstoneProcs)
            {
                DrawVerstoneProc();
            }

            if (ShowVerfireProcs)
            {
                DrawVerfireProc();
            }
        }

        protected override void DrawPrimaryResourceBar() 
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float) actor.CurrentMp / actor.MaxMp;
            var barSize = new Vector2(ManaBarWidth, ManaBarHeight);
            var cursorPos = new Vector2(OriginX - barSize.X / 2 + ManaBarXOffset, OriginY - barSize.Y + ManaBarYOffset);
            var color = ShowManaThresholdMarker && actor.CurrentMp < ManaThresholdValue ? ManaBarBelowThresholdColor : ManaBarColor;

            // bar
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, color["background"]);

            if (scale > 0)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(Math.Max(1, barSize.X * scale), barSize.Y),
                    color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
                );
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            // threshold
            if (ShowManaThresholdMarker)
            {
                var position = new Vector2(cursorPos.X + (ManaThresholdValue / 10000f) * barSize.X - 3, cursorPos.Y);
                var size = new Vector2(2, barSize.Y);
                drawList.AddRect(position, position + size, 0xFF000000);
            }

            // text
            if (!ShowManaValue) return;
            var mana = PluginInterface.ClientState.LocalPlayer.CurrentMp;
            var text = $"{mana,0}";
            var textSize = ImGui.CalcTextSize(text);
            DrawOutlinedText(text, new Vector2(cursorPos.X + 2, OriginY - barSize.Y / 2f + ManaBarYOffset - textSize.Y / 2f));
        }

        private void DrawBalanceBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<RDMGauge>();
            var whiteGauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().WhiteGauge;
            var blackGauge = (float)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().BlackGauge;
            var scale = gauge.WhiteGauge - gauge.BlackGauge;
            var barSize = new Vector2(BalanceBarWidth, BalanceBarHeight);
            var cursorPos = new Vector2(
                OriginX - barSize.X /2f + BalanceBarXOffset, 
                OriginY + BalanceBarYOffset
            );

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);

            Dictionary<string, uint> color = null;
            if (whiteGauge >= 80 && blackGauge >= 80)
                color = BalanceColor;
            else if (scale >= 30)
                color = WhiteManaBarColor;
            else if (scale <= -30)
                color = BlackManaBarColor;

            if (color != null)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + barSize,
                    color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
                );
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }

        private void DrawWhiteManaBar()
        {
            var gauge = (int)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().WhiteGauge;
            var scale = gauge / 100f;
            var size = new Vector2(WhiteManaBarWidth, WhiteManaBarHeight);
            var position = new Vector2(
                OriginX + WhiteManaBarXOffset,
                OriginY + WhiteManaBarYOffset
            );

            DrawManaBar(position, size, WhiteManaBarColor, gauge, scale, WhiteManaBarInversed, ShowWhiteManaValue);
        }

        private void DrawBlackManaBar()
        {
            var gauge = (int)PluginInterface.ClientState.JobGauges.Get<RDMGauge>().BlackGauge;
            var scale = gauge / 100f;
            var size = new Vector2(BlackManaBarWidth, BlackManaBarHeight);
            var position = new Vector2(
                OriginX + BlackManaBarXOffset,
                OriginY + BlackManaBarYOffset
            );

            DrawManaBar(position, size, BlackManaBarColor, gauge, scale, BlackManaBarInversed, ShowBlackManaValue);
        }

        private void DrawManaBar(Vector2 position, Vector2 size, Dictionary<string, uint> color, int value, float scale, bool inversed, bool showText)
        {
            var origin = inversed ? new Vector2(position.X - size.X, position.Y) : position;

            // bar
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(origin, origin + size, color["background"]);

            // fill
            if (scale > 0)
            {
                var barStartPos = inversed ? new Vector2(origin.X + size.X * (1 - scale), origin.Y) : origin;
                drawList.AddRectFilledMultiColor(
                    barStartPos, barStartPos + new Vector2(Math.Max(1, size.X * scale), size.Y),
                    color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
                );
            }

            // border
            drawList.AddRect(origin, origin + size, 0xFF000000);

            // threshold
            var thresholdRatio = inversed ? 0.2f : 0.8f;
            var thresholdPos = new Vector2(origin.X + size.X * thresholdRatio, origin.Y);
            drawList.AddRect(thresholdPos, thresholdPos + new Vector2(2, size.Y), 0xFF000000);

            // text
            if (!showText) return;
            var text = $"{value}";
            var textSize = ImGui.CalcTextSize(text);
            var textPos = inversed ? new Vector2(origin.X + size.X - 10 - textSize.X, origin.Y - 2) : new Vector2(origin.X + 10, origin.Y - 2);
            DrawOutlinedText(text, textPos);
        }
        
        private void DrawAccelerationBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var barSize = new Vector2(AccelBarWidth, AccelBarHeight);
            var totalWidth = barSize.X * 3 + HorizontalSpaceBetweenBars * 2;
            var cursorPos = new Vector2(
                OriginX - totalWidth / 2 + AccelerationBarXOffset, 
                OriginY + AccelerationBarYOffset
            );
            var accelBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1238);
            
            var drawList = ImGui.GetWindowDrawList();
            for (int i = 1; i <= 3; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, AccelBarColor["background"]);
                if (accelBuff.StackCount >= i)
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
        
        private void DrawDualCastBar() 
        {
            var barSize = new Vector2(DualCastWidth, DualCastHeight);
            var cursorPos = new Vector2(
                OriginX - DualCastWidth / 2f + DualCastXOffset, 
                OriginY + DualCastYOffset
            );
            
            var dualCastBuff = Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1249).Duration);
            
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, DualcastBarColor["background"]);
   
            if (dualCastBuff > 0)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + barSize,
                    DualcastBarColor["gradientLeft"], DualcastBarColor["gradientRight"], DualcastBarColor["gradientRight"], DualcastBarColor["gradientLeft"]
                );
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }  

        private void DrawVerstoneProc()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var duration = Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1235).Duration);
            if (duration == 0)
            {
                return;
            }

            var position = new Vector2(
                OriginX - HorizontalSpaceBetweenBars - DualCastWidth,
                OriginY + DualCastYOffset + DualCastHeight / 2f + ProcsHeight / 2f
            );

            var scale = duration / 30f;
            DrawTimerBar(position, scale, ProcsHeight, VerstoneBarColor, true);
        }

        private void DrawVerfireProc()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");

            var duration = Math.Abs(PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1234).Duration);
            if (duration == 0)
            {
                return;
            }

            var position = new Vector2(
                OriginX + HorizontalSpaceBetweenBars + DualCastWidth,
                OriginY + DualCastYOffset + DualCastHeight / 2f - ProcsHeight / 2f
            );

            var scale = duration / 30f;
            DrawTimerBar(position, scale, ProcsHeight, VerfireBarColor, false);
        }

        private void DrawTimerBar(Vector2 position, float scale, float height, Dictionary<string, uint> colorMap, bool inverted)
        {
            var drawList = ImGui.GetWindowDrawList();
            var size = new Vector2((ManaBarWidth / 2f - DualCastWidth - HorizontalSpaceBetweenBars * 2f) * scale, height);
            size.X = Math.Max(1, size.X);

            var startPoint = inverted ? position - size : position;
            var leftColor = inverted ? colorMap["gradientRight"] : colorMap["gradientLeft"];
            var rightColor = inverted ? colorMap["gradientLeft"] : colorMap["gradientRight"];

            drawList.AddRectFilledMultiColor(startPoint, startPoint + size,
                leftColor, rightColor, rightColor, leftColor
            );
        }
    }
}