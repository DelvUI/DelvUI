using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class SummonerHudWindow : HudWindow
    {
        public override uint JobId => 27;

        private int SmnRuinBarX => PluginConfiguration.SmnRuinBarX;
        private int SmnRuinBarY => PluginConfiguration.SmnRuinBarY;
        private int SmnRuinBarHeight => PluginConfiguration.SmnRuinBarHeight;
        private int SmnRuinBarWidth => PluginConfiguration.SmnRuinBarWidth;
        private int SmnDotBarX => PluginConfiguration.SmnDotBarX;
        private int SmnDotBarY => PluginConfiguration.SmnDotBarY;
        private int SmnDotBarHeight => PluginConfiguration.SmnDotBarHeight;
        private int SmnDotBarWidth => PluginConfiguration.SmnDotBarWidth;
        private int SmnAetherBarHeight => PluginConfiguration.SmnAetherBarHeight;
        private int SmnAetherBarWidth => PluginConfiguration.SmnAetherBarWidth;
        private int SmnAetherBarX => PluginConfiguration.SmnAetherBarX;
        private int SmnAetherBarY => PluginConfiguration.SmnAetherBarY;

        private Dictionary<string, uint> SmnAetherColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000];
        private Dictionary<string, uint> SmnRuinColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 1];
        private Dictionary<string, uint> SmnEmptyColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 2];
        private Dictionary<string, uint> SmnMiasmaColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 3];
        private Dictionary<string, uint> SmnBioColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 4];
        private Dictionary<string, uint> SmnExpiryColor => PluginConfiguration.JobColorMap[Jobs.SMN * 1000 + 5];


        public SummonerHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawRuinBar();
            DrawActiveDots();
            DrawAetherBar();
        }

        protected override void DrawPrimaryResourceBar()
        {
        }

        private void DrawActiveDots()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;

            if (!(target is Chara))
            {
                return;
            }

            var xPadding = 2;
            var barWidth = (SmnDotBarWidth / 2) - 1;
            var miasma = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1215 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                                  o.EffectId == 180 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
            var bio = target.StatusEffects.FirstOrDefault(o => o.EffectId == 1214 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                               o.EffectId == 179 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                               o.EffectId == 189 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);

            var miasmaDuration = miasma.Duration;
            var bioDuration = bio.Duration;

            var miasmaColor = miasmaDuration > 5 ? SmnMiasmaColor["base"] : SmnExpiryColor["base"];
            var bioColor = bioDuration > 5 ? SmnBioColor["base"] : SmnExpiryColor["base"];

            var xOffset = CenterX - SmnDotBarX;
            var cursorPos = new Vector2(CenterX - SmnDotBarX, CenterY + SmnDotBarY - 46);
            var barSize = new Vector2(barWidth, SmnDotBarHeight);
            var drawList = ImGui.GetWindowDrawList();

            var dotStart = new Vector2(xOffset + barWidth - (barSize.X / 30) * miasmaDuration, CenterY + SmnDotBarY - 46);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnEmptyColor["base"]);
            drawList.AddRectFilled(dotStart, cursorPos + new Vector2(barSize.X, barSize.Y), miasmaColor);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnEmptyColor["base"]);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2((barSize.X / 30) * bioDuration, barSize.Y), bioColor);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
        }
        private void DrawAetherBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var aetherFlowBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 304);
            var xPadding = 2;
            var barWidth = (SmnAetherBarWidth / 2) - 1;
            var cursorPos = new Vector2(CenterX - 127, CenterY + SmnAetherBarY - 22);
            var barSize = new Vector2(barWidth, SmnAetherBarHeight);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnEmptyColor["base"]);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnEmptyColor["base"]);
            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
            cursorPos = new Vector2(CenterX - 127, CenterY + SmnAetherBarY - 22);

            switch (aetherFlowBuff.StackCount)
            {
                case 1:
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnAetherColor["base"]);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                    break;
                case 2:
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnAetherColor["base"]);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    cursorPos = new Vector2(cursorPos.X + barWidth + xPadding, cursorPos.Y);
                    drawList.AddRectFilled(cursorPos, cursorPos + barSize, SmnAetherColor["base"]);
                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                    break;

            }

        }
        private void DrawRuinBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var ruinBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1212);
            var ruinStacks = ruinBuff.StackCount;

            const int xPadding = 2;
            var barWidth = (SmnRuinBarWidth - xPadding * 3) / 4;
            var barSize = new Vector2(barWidth, SmnRuinBarHeight);
            var xPos = CenterX - SmnRuinBarX;
            var yPos = CenterY + SmnRuinBarY - 34;
            var cursorPos = new Vector2(xPos, yPos);
            var barColor = SmnRuinColor["base"];
            var emptyColor = SmnEmptyColor["base"];

            var drawList = ImGui.GetWindowDrawList();
            for (var i = 0; i <= 4 - 1; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, emptyColor);
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
    }
}