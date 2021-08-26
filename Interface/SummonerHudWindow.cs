using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface
{
    public class SummonerHudWindow : HudWindow
    {
        public override uint JobId => 27;

        private static int BarHeight => 20;
        private static int SmallBarHeight => 10;
        private static int BarWidth => 254;
        private new static int XOffset => 127;
        private new static int YOffset => 466;

        public SummonerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawRuinBar();
            DrawActiveDots();
            DrawAetherBar();
            DrawTargetBar();
            DrawCastBar();
        }

        private void DrawActiveDots()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (!(target is Chara))
            {
                return;
            }

            var expiryColor = 0xFF2E2EC7;
            var xPadding = 2;
            var barWidth = (BarWidth / 2) - 1;
            var miasma = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1215 || o.EffectId == 180);
            var bio = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1214 || o.EffectId == 179 || o.EffectId == 189);

            var miasmaDuration = miasma.Duration;
            var bioDuration = bio.Duration;

            var miasmaColor = miasmaDuration > 5 ? 0xFFFAFFA4 : expiryColor;
            var bioColor = bioDuration > 5 ? 0xFF005239 : expiryColor;

            var xOffset = CenterX - 127;
            var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 46);
            var barSize = new Vector2(barWidth, SmallBarHeight);
            var drawList = ImGui.GetWindowDrawList();

            var dotStart = new Vector2(xOffset + barWidth - (barSize.X / 30) * miasmaDuration, CenterY + YOffset - 46);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilled(dotStart, cursorPos + new Vector2(barSize.X, barSize.Y), miasmaColor);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2((barSize.X / 30) * bioDuration, barSize.Y), bioColor);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

        }
        private void DrawAetherBar()
        {
            var aetherFlowBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 304);
            var xPadding = 2;
            var xOffset = CenterX - 127;
            var barWidth = (BarWidth / 2) - 1;
            var cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 22);
            var barSize = new Vector2(barWidth, BarHeight);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            cursorPos = new Vector2(CenterX - 127, CenterY + YOffset - 22);

            switch (aetherFlowBuff.StackCount)
            {
                case 1:
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFFFFFF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                    break;
                case 2:
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFFFFFF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0xFFFFFF00);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    break;

            }

        }
        private void DrawRuinBar()
        {
            var ruinBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1212);
            var ruinStacks = ruinBuff.StackCount;

            const int xPadding = 2;
            var barWidth = (BarWidth - xPadding * 3) / 4;
            var barSize = new Vector2(barWidth, SmallBarHeight);
            var xPos = CenterX - XOffset;
            var yPos = CenterY + YOffset - 34;
            var cursorPos = new Vector2(xPos, yPos);
            var barColor = 0xFFFFFF00;

            var drawList = ImGui.GetWindowDrawList();
            for (var i = 0; i <= 4 - 1; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (ruinStacks > i)
                {
                    drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y), barColor);
                }
                else
                {

                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            }

        }
        private void DrawTranceBar()
        {
            //Need to figure this out, trances dont give a visible buff, Api has a buff listing as 808 but unsure if this is correct

        }
        private void DrawEgiAssaultsBar()
        {
            // Does dalamud give ability tracking? This may still need to be a action bar the user creates.
        }
    }
}