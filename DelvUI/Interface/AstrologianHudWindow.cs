using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace DelvUI.Interface
{
    class AstrologianHudWindow : HudWindow
    {
        public override uint JobId => 33;

        private new int XOffset => PluginConfiguration.ASTBaseXOffset;

        private new int YOffset => PluginConfiguration.ASTBaseYOffset;

        private bool DivinationEnabled => PluginConfiguration.ASTDivinationEnabled;

        private int DivinationHeight => PluginConfiguration.ASTDivinationHeight;

        private int DivinationWidth => PluginConfiguration.ASTDivinationWidth;

        private int DivinationBarX => PluginConfiguration.ASTDivinationBarX;

        private int DivinationBarY => PluginConfiguration.ASTDivinationBarY;

        private int DivinationBarPad => PluginConfiguration.ASTDivinationBarPad;

        //private int InterBarOffset => PluginConfiguration.ASTInterBarOffset;

        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000];

        private new Vector2 BarSize { get; set; }

        private Vector2 BarCoords { get; set; }

        public AstrologianHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawPrimaryResourceBar();
            DrawDivinationBar();
            DrawTargetBar();
            DrawFocusBar();
            DrawCastBar();
        }

        private void DrawDivinationBar()
        {
            var gauge = FFXIVClientStructs.FFXIV.Client.Game.Gauge.

            var barWidth = (DivinationWidth / 3);
            BarSize = new Vector2(barWidth, DivinationHeight);
            BarCoords = new Vector2(DivinationBarX, DivinationBarY);
            var cursorPos = new Vector2(CenterX + BarCoords.X, CenterY + BarCoords.Y - 71);

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X + barWidth + DivinationBarPad, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);
            cursorPos = new Vector2(cursorPos.X - barWidth * 2 - DivinationBarPad * 2, cursorPos.Y);

            drawList.AddRectFilled(cursorPos, cursorPos + BarSize, EmptyColor["gradientRight"]);
            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);


            drawList.AddRect(cursorPos, cursorPos + BarSize, 0xFF000000);

            //NOTE: in api3, seals are private
            // private unsafe fixed byte seals[3]
            // so this a workaround until Net5 Dalamud
            // https://github.com/aers/FFXIVClientStructs/blob/dd3ec0485395e23592303311bb29d0447e03f06d/FFXIVClientStructs/FFXIV/Client/Game/Gauge/JobGauges.cs#L30
        }
    }
}
/*
    //     No seal.
    NONE = 0,

    //     Sun seal.
    SUN = 1,

    //     Moon seal.
    MOON = 2,

    //     Celestial seal.
    CELESTIAL = 3
*/