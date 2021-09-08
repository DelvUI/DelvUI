﻿using Dalamud.Game.ClientState.Actors.Types;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class MPTickerHud : HudElement, IHudElementWithActor
    {
        private MPTickerConfig Config => (MPTickerConfig)_config;

        private MPTickHelper _mpTickHelper;
        public Actor Actor { get; set; } = null;

        public MPTickerHud(string ID, MPTickerConfig config) : base(ID, config)
        {
        }

        public override void Draw(Vector2 origin)
        {
            if (!Config.Enabled || Actor == null || Actor is not Chara)
            {
                return;
            }

            if (Config.HideOnFullMP)
            {
                var chara = (Chara)Actor;
                if (chara.CurrentMp >= chara.MaxMp)
                {
                    return;
                }
            }

            _mpTickHelper ??= new MPTickHelper();

            var now = ImGui.GetTime();
            var scale = (float)((now - _mpTickHelper.LastTick) / MPTickHelper.ServerTickRate);

            if (scale <= 0)
            {
                return;
            }

            if (scale > 1)
            {
                scale = 1;
            }

            var barSize = new Vector2(Math.Max(1f, Config.Size.X * scale), Config.Size.Y);
            var startPos = origin + Config.Position - Config.Size / 2f;

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(startPos, startPos + Config.Size, 0x88000000);

            drawList.AddRectFilledMultiColor(
                startPos,
                startPos + barSize,
                Config.Color.LeftGradient,
                Config.Color.RightGradient,
                Config.Color.RightGradient,
                Config.Color.LeftGradient
            );

            if (Config.ShowBorder)
            {
                drawList.AddRect(startPos, startPos + Config.Size, 0xFF000000);
            }
        }
    }
}