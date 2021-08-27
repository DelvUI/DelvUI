using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class PaladinHudWindow : HudWindow
    {
        public override uint JobId => 19;

        protected int ManaBarHeight => PluginConfiguration.PLDManaHeight;
        protected int ManaBarWidth => PluginConfiguration.PLDManaWidth;
        protected int ManaBarPadding => PluginConfiguration.PLDManaPadding;
        protected new int XOffset => PluginConfiguration.PLDBaseXOffset;
        protected new int YOffset => PluginConfiguration.PLDBaseYOffset;
        protected int OathGaugeBarHeight => PluginConfiguration.PLDOathGaugeHeight;
        protected int OathGaugeBarWidth => PluginConfiguration.PLDOathGaugeWidth;
        protected int OathGaugeBarPadding => PluginConfiguration.PLDOathGaugePadding;
        protected int OathGaugeXOffset => PluginConfiguration.PLDOathGaugeXOffset;
        protected int OathGaugeYOffset => PluginConfiguration.PLDOathGaugeYOffset;
        protected bool OathGaugeText => PluginConfiguration.PLDOathGaugeText;
        protected int BuffBarHeight => PluginConfiguration.PLDBuffBarHeight;
        protected int BuffBarWidth => PluginConfiguration.PLDBuffBarWidth;
        protected int BuffBarXOffset => PluginConfiguration.PLDBuffBarXOffset;
        protected int BuffBarYOffset => PluginConfiguration.PLDBuffBarYOffset;
        protected int AtonementBarHeight => PluginConfiguration.PLDAtonementBarHeight;
        protected int AtonementBarWidth => PluginConfiguration.PLDAtonementBarWidth;
        protected int AtonementBarPadding => PluginConfiguration.PLDAtonementBarPadding;
        protected int AtonementBarXOffset => PluginConfiguration.PLDAtonementBarXOffset;
        protected int AtonementBarYOffset => PluginConfiguration.PLDAtonementBarYOffset;
        protected int InterBarOffset => PluginConfiguration.PLDInterBarOffset;
        protected Dictionary<string, uint> ManaColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000];
        protected Dictionary<string, uint> OathGaugeColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 1];
        protected Dictionary<string, uint> FightOrFlightColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 2];
        protected Dictionary<string, uint> RequiescatColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 3];
        protected Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 4];
        protected Dictionary<string, uint> AtonementColor => PluginConfiguration.JobColorMap[Jobs.PLD * 1000 + 5];

        public PaladinHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) {}

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            var nextHeight = DrawManaBar(0);
            nextHeight = DrawOathGauge(nextHeight);
            nextHeight = DrawBuffBar(nextHeight);
            nextHeight = DrawAtonementBar(nextHeight);
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        private int DrawManaBar(int initialHeight)
        {
            var actor = PluginInterface.ClientState.LocalPlayer;

            var barWidth = (ManaBarWidth - ManaBarPadding * 4f) / 5f;
            var posX = CenterX - XOffset;
            var posY = CenterY + YOffset;

            var cursorPos = new Vector2(posX + barWidth * 4 + ManaBarPadding * 4, posY);
            var barSize = new Vector2(barWidth, ManaBarHeight);

            var drawList = ImGui.GetWindowDrawList();

            for (var i = 5; i >= 1; --i)
            {
                var scale = Math.Max(Math.Min(actor.CurrentMp, 2000 * i) - 2000 * (i - 1), 0f) / 2000f;
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (scale >= 1.0)
                    drawList.AddRectFilledMultiColor(cursorPos,
                        cursorPos + new Vector2(barWidth * scale, ManaBarHeight),
                        ManaColor["gradientLeft"], ManaColor["gradientRight"], ManaColor["gradientRight"], ManaColor["gradientLeft"]
                    );
                else
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, ManaBarHeight),
                        EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]
                    );
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                cursorPos = new Vector2(cursorPos.X - barWidth - ManaBarPadding, cursorPos.Y);
            }

            return ManaBarHeight + initialHeight + InterBarOffset;
        }

        private int DrawOathGauge(int initialHeight)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<PLDGauge>();

            var barWidth = (OathGaugeBarWidth - OathGaugeBarPadding) / 2f;
            var xPos = CenterX - XOffset + OathGaugeXOffset;
            var yPos = CenterY + YOffset + initialHeight + OathGaugeYOffset;
            var cursorPos = new Vector2(xPos + barWidth + OathGaugeBarPadding, yPos);
            var barSize = new Vector2(barWidth, OathGaugeBarHeight);

            var drawList = ImGui.GetWindowDrawList();
            for (var i = 2; i >= 1; --i)
            {
                var scale = Math.Max(Math.Min(gauge.GaugeAmount, 50 * i) - 50 * (i - 1), 0) /
                            50f;
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (scale >= 1.0)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, OathGaugeBarHeight),
                        OathGaugeColor["gradientLeft"], OathGaugeColor["gradientRight"], OathGaugeColor["gradientRight"], OathGaugeColor["gradientLeft"]
                    );
                    
                    if (OathGaugeText)
                    {
                        var text = (scale * 50).ToString();
                        var textSize = ImGui.CalcTextSize(text);
                        DrawOutlinedText(text, new Vector2(cursorPos.X + barWidth / 2f - textSize.X / 2f, cursorPos.Y-2));                        
                    }
                }
                else
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth * scale, OathGaugeBarHeight),
                        EmptyColor["gradientLeft"], EmptyColor["gradientRight"], EmptyColor["gradientRight"], EmptyColor["gradientLeft"]
                    );
                    
                    if (OathGaugeText)
                    {
                        var text = (scale * 50).ToString();
                        var textSize = ImGui.CalcTextSize(text);
                        DrawOutlinedText(text, new Vector2(cursorPos.X + barWidth / 2f - textSize.X / 2f, cursorPos.Y-2));                        
                    }
                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos = new Vector2(cursorPos.X - barWidth - OathGaugeBarPadding, cursorPos.Y);
            }

            return OathGaugeBarHeight + initialHeight + InterBarOffset;
        }

        private int DrawBuffBar(int initialHeight)
        {
            var fightOrFlightBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 76);
            var requiescatBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1368);
            
            var buffBarBarWidth = BuffBarWidth;
            var xPos = CenterX - XOffset + BuffBarXOffset;
            var yPos = CenterY + YOffset + initialHeight + BuffBarYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var buffBarBarHeight = BuffBarHeight;
            var barSize = new Vector2(buffBarBarWidth, buffBarBarHeight);
            
            var drawList = ImGui.GetWindowDrawList();
            
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            if (fightOrFlightBuff.Any() && requiescatBuff.Any())
            {
                var innerBarHeight = buffBarBarHeight / 2;
                barSize = new Vector2(buffBarBarWidth, innerBarHeight);
                
                var fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().Duration);
                var requiescatDuration = Math.Abs(requiescatBuff.First().Duration);
                
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X / 25f * fightOrFlightDuration, barSize.Y), 
                    FightOrFlightColor["gradientLeft"], FightOrFlightColor["gradientRight"], FightOrFlightColor["gradientRight"], FightOrFlightColor["gradientLeft"]
                );
                drawList.AddRectFilledMultiColor(
                    cursorPos + new Vector2(0.0f, innerBarHeight), cursorPos + new Vector2(barSize.X / 12f * requiescatDuration, barSize.Y * 2f),
                    RequiescatColor["gradientLeft"], RequiescatColor["gradientRight"], RequiescatColor["gradientRight"], RequiescatColor["gradientLeft"]
                );
                
                var fightOrFlightDurationText = fightOrFlightDuration == 0 ? "" : Math.Round(fightOrFlightDuration).ToString();
                DrawOutlinedText(fightOrFlightDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), PluginConfiguration.PLDFightOrFlightColor, new Vector4(0f, 0f, 0f, 1f));
                
                var requiescatDurationText = requiescatDuration == 0 ? "" : Math.Round(requiescatDuration).ToString();
                DrawOutlinedText(requiescatDurationText, new Vector2(cursorPos.X + 27f, cursorPos.Y - 2f), PluginConfiguration.PLDRequiescatColor, new Vector4(0f, 0f, 0f, 1f));
                
                barSize = new Vector2(buffBarBarWidth, buffBarBarHeight);
            }
            else if (fightOrFlightBuff.Any())
            {
                var fightOrFlightDuration = Math.Abs(fightOrFlightBuff.First().Duration);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X / 25f * fightOrFlightDuration, barSize.Y), 
                    FightOrFlightColor["gradientLeft"], FightOrFlightColor["gradientRight"], FightOrFlightColor["gradientRight"], FightOrFlightColor["gradientLeft"]
                );
                
                var fightOrFlightDurationText = fightOrFlightDuration == 0 ? "" : Math.Round(fightOrFlightDuration).ToString();
                DrawOutlinedText(fightOrFlightDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), PluginConfiguration.PLDFightOrFlightColor, new Vector4(0f, 0f, 0f, 1f));
            }
            else if (requiescatBuff.Any())
            {
                var requiescatDuration = Math.Abs(requiescatBuff.First().Duration);
                drawList.AddRectFilledMultiColor(
                    cursorPos, cursorPos + new Vector2(barSize.X / 12f * requiescatDuration, barSize.Y), 
                    RequiescatColor["gradientLeft"], RequiescatColor["gradientRight"], RequiescatColor["gradientRight"], RequiescatColor["gradientLeft"]
                );
                
                var requiescatDurationText = requiescatDuration == 0 ? "" : Math.Round(requiescatDuration).ToString();
                DrawOutlinedText(requiescatDurationText, new Vector2(cursorPos.X + 5f, cursorPos.Y - 2f), PluginConfiguration.PLDRequiescatColor, new Vector4(0f, 0f, 0f, 1f));
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            return BuffBarHeight + initialHeight + InterBarOffset;
        }

        private int DrawAtonementBar(int initialHeight)
        {
            var atonementBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1902);
            var stackCount = atonementBuff.Any() ? atonementBuff.First().StackCount : 0;
            
            var barWidth = (AtonementBarWidth - AtonementBarPadding * 2) / 3f;
            var xPos = CenterX - XOffset + AtonementBarXOffset;
            var yPos = CenterY + YOffset + initialHeight + AtonementBarYOffset;
            var cursorPos = new Vector2(xPos, yPos);
            var barSize = new Vector2(barWidth, AtonementBarHeight);
            var drawList = ImGui.GetWindowDrawList();

            for (var i = 0; i <= 2; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (stackCount > 0)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barWidth, AtonementBarHeight),
                        AtonementColor["gradientLeft"], AtonementColor["gradientRight"], AtonementColor["gradientRight"], AtonementColor["gradientLeft"]
                    );
                    stackCount--;
                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos += new Vector2(barWidth + AtonementBarPadding, 0);
            }

            return AtonementBarHeight + initialHeight + InterBarOffset;
        }
    }
}