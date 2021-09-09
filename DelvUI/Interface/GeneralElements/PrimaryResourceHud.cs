using Dalamud.Game.ClientState.Actors.Types;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public enum PrimaryResourceType
    {
        MP,
        CP,
        GP,
        None
    }

    public class PrimaryResourceHud : HudElement, IHudElementWithActor
    {
        private PrimaryResourceConfig Config => (PrimaryResourceConfig)_config;
        private LabelHud _valueLabel;
        public Actor Actor { get; set; } = null;
        private PrimaryResourceType _resourceType = PrimaryResourceType.MP;

        public PrimaryResourceHud(string ID, PrimaryResourceConfig config) : base(ID, config)
        {
            _valueLabel = new LabelHud(ID + "_valueLabel", config.ValueLabelConfig);
        }

        public override void Draw(Vector2 origin)
        {
            if (!Config.Enabled || _resourceType == PrimaryResourceType.None || Actor == null || Actor is not Chara)
            {
                return;
            }

            var chara = (Chara)Actor;
            int current = 0;
            int max = 0;

            GetResources(ref current, ref max, chara);

            var scale = (float)current / max;
            var startPos = origin + Config.Position - Config.Size / 2f;

            // bar
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(startPos, startPos + Config.Size, 0x88000000);

            drawList.AddRectFilledMultiColor(
                startPos,
                startPos + new Vector2(Math.Max(1, Config.Size.X * scale), Config.Size.Y),
                Config.Color.LeftGradient,
                Config.Color.RightGradient,
                Config.Color.RightGradient,
                Config.Color.LeftGradient
            );

            drawList.AddRect(startPos, startPos + Config.Size, 0xFF000000);

            // threshold
            if (Config.ShowThresholdMarker)
            {
                var position = new Vector2(startPos.X + Config.ThresholdMarkerValue / 10000f * Config.Size.X - 2, Config.Size.Y);
                var size = new Vector2(2, Config.Size.Y);
                drawList.AddRect(position, position + size, 0xFF000000);
            }

            // text
            if (Config.ShowValue)
            {
                Config.ValueLabelConfig.Text = $"{current,0}";
                _valueLabel.Draw(origin + Config.Position);
            }
        }

        private void GetResources(ref int current, ref int max, Chara actor)
        {
            switch (_resourceType)
            {
                case PrimaryResourceType.MP:
                    {
                        current = actor.CurrentMp;
                        max = actor.MaxMp;
                    }

                    break;

                case PrimaryResourceType.CP:
                    {
                        current = actor.CurrentCp;
                        max = actor.MaxCp;
                    }

                    break;

                case PrimaryResourceType.GP:
                    {
                        current = actor.CurrentGp;
                        max = actor.MaxGp;
                    }

                    break;
            }
        }
    }
}
