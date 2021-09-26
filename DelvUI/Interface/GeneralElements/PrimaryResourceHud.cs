using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;

namespace DelvUI.Interface.GeneralElements
{
    public class PrimaryResourceHud : DraggableHudElement, IHudElementWithActor
    {
        private PrimaryResourceConfig Config => (PrimaryResourceConfig)_config;
        private LabelHud _valueLabel;
        public GameObject? Actor { get; set; } = null;
        public PrimaryResourceTypes ResourceType = PrimaryResourceTypes.MP;

        public PrimaryResourceHud(string ID, PrimaryResourceConfig config, string displayName) : base(ID, config, displayName)
        {
            _valueLabel = new LabelHud(ID + "_valueLabel", config.ValueLabelConfig);
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

            GetResources(ref current, ref max, chara);

            var scale = (float)current / max;
            var startPos = Utils.GetAnchoredPosition(origin + Config.Position, Config.Size, Config.Anchor);

            // bar
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(startPos, startPos + Config.Size, 0x88000000);
            
            var color = Color(Actor);
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
                _valueLabel.Draw(origin + Config.Position, Config.Size, Actor);
            }

        }

        private void GetResources(ref int current, ref int max, Character actor)
        {
            switch (ResourceType)
            {
                case PrimaryResourceTypes.MP:
                    {
                        current = (int)actor.CurrentMp;
                        max = (int)actor.MaxMp;
                    }

                    break;

                case PrimaryResourceTypes.CP:
                    {
                        current = (int)actor.CurrentCp;
                        max = (int)actor.MaxCp;
                    }

                    break;

                case PrimaryResourceTypes.GP:
                    {
                        current = (int)actor.CurrentGp;
                        max = (int)actor.MaxGp;
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
