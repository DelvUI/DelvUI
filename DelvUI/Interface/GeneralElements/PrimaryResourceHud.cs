﻿using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using System;

namespace DelvUI.Interface.GeneralElements
{
    public class PrimaryResourceHud : DraggableHudElement, IHudElementWithActor
    {
        private new PrimaryResourceConfig Config => (PrimaryResourceConfig)base.Config;
        private readonly LabelHud _valueLabel;
        public GameObject? Actor { get; set; } = null;
        public PrimaryResourceTypes ResourceType = PrimaryResourceTypes.MP;

        public PrimaryResourceHud(string iD, PrimaryResourceConfig config, string displayName) : base(iD, config, displayName)
        {
            _valueLabel = new LabelHud(iD + "_valueLabel", config.ValueLabelConfig);
        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>() { Config.Position }, new List<Vector2>() { Config.Size });
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (!Config.Enabled || ResourceType == PrimaryResourceTypes.None || Actor == null || Actor is not Character)
            {
                return;
            }

            var chara = (Character)Actor;
            int current = 0;
            int max = 0;
            int percent = 0;

            GetResources(ref current, ref max, ref percent, chara);
            if (Config.HidePrimaryResourceWhenFull && current == max) { return; }

            float scale = (float)current / max;

            Vector2 startPos = Utils.GetAnchoredPosition(origin + Config.Position, Config.Size, Config.Anchor);

            // bar
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(startPos, startPos + Config.Size, 0x88000000);

            PluginConfigColor? color = Config.ShowThresholdMarker && percent < Config.ThresholdMarkerValue / 100 ? Config.BelowThresholdColor : Color(Actor);

            DrawHelper.DrawGradientFilledRect(startPos, new Vector2(Config.Size.X * scale, Config.Size.Y), color, drawList);

            drawList.AddRect(startPos, startPos + Config.Size, 0xFF000000);

            // threshold
            if (Config.ShowThresholdMarker)
            {
                var position = new Vector2(startPos.X + Config.ThresholdMarkerValue / 10000f * Config.Size.X - 2, startPos.Y);
                var size = new Vector2(2, Config.Size.Y);
                drawList.AddRect(position, position + size, 0xFF000000);
            }

            // label
            if (Config.ValueLabelConfig.Enabled)
            {
                Config.ValueLabelConfig.SetText($"{current,0}");
                _valueLabel.Draw(startPos, Config.Size, Actor);
            }

        }

        private void GetResources(ref int current, ref int max, ref int percent, Character actor)
        {
            switch (ResourceType)
            {
                case PrimaryResourceTypes.MP:
                    {
                        current = (int)actor.CurrentMp;
                        max = (int)actor.MaxMp;
                        if (max != 0) { percent = (int)Math.Round((double)(100 * current / max)); }
                    }

                    break;

                case PrimaryResourceTypes.CP:
                    {
                        current = (int)actor.CurrentCp;
                        max = (int)actor.MaxCp;
                        if (max != 0) { percent = (int)Math.Round((double)(100 * current / max)); }
                    }

                    break;

                case PrimaryResourceTypes.GP:
                    {
                        current = (int)actor.CurrentGp;
                        max = (int)actor.MaxGp;
                        if (max != 0) { percent = (int)Math.Round((double)(100 * current / max)); }
                    }

                    break;
            }
        }

        public virtual PluginConfigColor Color(GameObject? actor = null)
        {
            if (!Config.UseJobColor)
            {
                return Config.Color;
            }

            return actor is not Character character ? GlobalColors.Instance.NPCFriendlyColor : Utils.ColorForActor(character);
        }
    }
}
