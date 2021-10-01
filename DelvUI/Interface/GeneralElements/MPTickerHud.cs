using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Interface.GeneralElements
{
    public class MPTickerHud : DraggableHudElement, IHudElementWithActor
    {
        private new MPTickerConfig Config => (MPTickerConfig)base.Config;

        private MPTickHelper _mpTickHelper = null!;
        public GameObject? Actor { get; set; } = null;

        public MPTickerHud(string iD, MPTickerConfig config, string displayName) : base(iD, config, displayName) { }

        protected override void InternalDispose()
        {
            _mpTickHelper.Dispose();
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || Actor == null || Actor is not Character)
            {
                return;
            }

            if (Config.HideOnFullMP)
            {
                var chara = (Character)Actor;
                if (chara.CurrentMp >= chara.MaxMp)
                {
                    return;
                }
            }

            _mpTickHelper ??= new MPTickHelper();

            double now = ImGui.GetTime();
            float scale = (float)((now - _mpTickHelper.LastTick) / MPTickHelper.ServerTickRate);

            if (scale <= 0)
            {
                return;
            }

            if (scale > 1)
            {
                scale = 1;
            }

            var barSize = new Vector2(Math.Max(1f, Config.Size.X * scale), Config.Size.Y);
            Vector2 startPos = Utils.GetAnchoredPosition(origin + Config.Position, Config.Size, Config.Anchor);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(startPos, startPos + Config.Size, 0x88000000);

            drawList.AddRectFilledMultiColor(
                startPos,
                startPos + barSize,
                Config.Color.TopGradient,
                Config.Color.TopGradient,
                Config.Color.BottomGradient,
                Config.Color.BottomGradient
            );

            if (Config.ShowBorder)
            {
                drawList.AddRect(startPos, startPos + Config.Size, 0xFF000000);
            }
        }
    }
}
