using DelvUI.Config.Attributes;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Config
{
    [Serializable]
    public abstract class PluginConfigObject
    {
        [Checkbox("Enabled")]
        [Order(0)]
        public bool Enabled = true;

        protected bool ColorEdit4(string label, ref PluginConfigColor color)
        {
            var vector = color.Vector;

            if (ImGui.ColorEdit4(label, ref vector))
            {
                color.Vector = vector;

                return true;
            }

            return false;
        }

        public static PluginConfigObject DefaultConfig()
        {
            return null;
        }
    }

    [Serializable]
    public abstract class MovablePluginConfigObject : PluginConfigObject
    {
        [DragInt2("Position", min = -4000, max = 4000)]
        [Order(5)]
        public Vector2 Position = Vector2.Zero;

        [DragInt2("Size", min = 1, max = 4000)]
        [Order(10)]
        public Vector2 Size = Vector2.Zero;
    }

    [Serializable]
    public class PluginConfigColor
    {
        [JsonIgnore] private float[] _colorMapRatios = { -.8f, -.1f, .1f };

        [JsonIgnore] private Vector4 _vector;

        public PluginConfigColor(Vector4 vector, float[] colorMapRatios = null)
        {
            _vector = vector;

            if (colorMapRatios != null && colorMapRatios.Length >= 3)
            {
                _colorMapRatios = colorMapRatios;
            }

            Update();
        }

        public Vector4 Vector
        {
            get => _vector;
            set
            {
                if (_vector == value)
                {
                    return;
                }

                _vector = value;

                Update();
            }
        }

        [JsonIgnore] public uint Base { get; private set; }

        [JsonIgnore] public uint Background { get; private set; }

        [JsonIgnore] public uint LeftGradient { get; private set; }

        [JsonIgnore] public uint RightGradient { get; private set; }

        [JsonIgnore] public Dictionary<string, uint> Map { get; private set; }

        private void Update()
        {
            Base = ImGui.ColorConvertFloat4ToU32(_vector);
            Background = ImGui.ColorConvertFloat4ToU32(_vector.AdjustColor(_colorMapRatios[0]));
            LeftGradient = ImGui.ColorConvertFloat4ToU32(_vector.AdjustColor(_colorMapRatios[1]));
            RightGradient = ImGui.ColorConvertFloat4ToU32(_vector.AdjustColor(_colorMapRatios[2]));
            Map = new Dictionary<string, uint> { ["base"] = Base, ["background"] = Background, ["gradientLeft"] = LeftGradient, ["gradientRight"] = RightGradient };
        }
    }
}
