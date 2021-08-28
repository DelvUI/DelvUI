using System;
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
    class AstrologianHudWindow : HudWindow
    {
        public override uint JobId => 33;

        private new int XOffset => PluginConfiguration.ASTBaseXOffset;

        private new int YOffset => PluginConfiguration.ASTBaseYOffset;

        private bool DivinationEnabled => PluginConfiguration.ASTDivinationEnabled;

        private int DivinationHeight => PluginConfiguration.ASTDivinationHeight;

        private int DivinationWidth => PluginConfiguration.ASTDivinationWidth;


        private Dictionary<string, uint> EmptyColor => PluginConfiguration.JobColorMap[Jobs.AST * 1000 + 4];

    }

    protected override void Draw(bool _)
    {
        DrawHealthBar();
        DrawTargetBar();
        DrawFocusBar();
        DrawCastBar();
    }
}
