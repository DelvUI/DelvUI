using Dalamud.Game.ClientState.Actors.Types;
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

        protected int BioBarHeight => PluginConfiguration.SCHBioBarHeight;
        protected int BioBarWidth => PluginConfiguration.SCHBioBarWidth;
        protected int BioBarX => PluginConfiguration.SCHBioBarX;
        protected int BioBarY => PluginConfiguration.SCHBioBarY;

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

        public ScholarHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            if (ShowFairyBar)
            {
                DrawFairyBar();
            }
            if (ShowBioBar)
            {
                DrawBioBar();
            }
            if (ShowAetherBar)
            {
                DrawAetherBar();
            }
        }

        protected override void DrawPrimaryResourceBar()
        {
            if (!ShowPrimaryResourceBar)
            {
                return;
            }

            base.DrawPrimaryResourceBar();
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

            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barWidth+1, SchAetherBarHeight), EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + new Vector2(barWidth + 1, SchAetherBarHeight), 0xFF000000);
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
                    drawList.AddRectFilled(cursorPos, cursorPos + new Vector2(barWidth + 1, SchAetherBarHeight), SchAetherColor["gradientRight"]);
                    drawList.AddRect(cursorPos, cursorPos + new Vector2(barWidth + 1, SchAetherBarHeight), 0xFF000000);
                    break;
            }

        }

        private void DrawBioBar()
        {
            var target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            BarSize = new Vector2(BioBarWidth, BioBarHeight);
            BarCoords = new Vector2(BioBarX, BioBarY);
            var cursorPos = new Vector2(CenterX - BarCoords.X, CenterY + BarCoords.Y);

            var drawList = ImGui.GetWindowDrawList();;

            if (!(target is Chara))
            {
                drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
                drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
                return;
            }
            var bio = target.StatusEffects.FirstOrDefault(o => o.EffectId == 179 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                               o.EffectId == 189 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId ||
                                                               o.EffectId == 1895 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId);
            var bioDuration = (int)bio.Duration;
            var xOffset = CenterX;

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRectFilled(cursorPos, cursorPos + new Vector2((BarSize.X / 30) * bioDuration, BarSize.Y), SCHBioColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            DrawOutlinedText(bioDuration.ToString(CultureInfo.InvariantCulture), new Vector2(cursorPos.X + BarSize.X * bioDuration / 30 - (bioDuration == 30 ? 30 : bioDuration > 3 ? 20 : 0), cursorPos.Y + (BarSize.Y / 2) - 12));

        }

    }
}