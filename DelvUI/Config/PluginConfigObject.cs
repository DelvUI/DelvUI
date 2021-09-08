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
    }

    [Serializable]
    public class PluginConfigColor
    {
        [JsonIgnore] private float[] _colorMapRatios = { -.8f, -.1f, .1f };

        [JsonIgnore] private Vector4 _vector;

        public PluginConfigColor(Vector4 vector, float[] colorMapRatios = null)
        {
            _vector = vector;

            if (colorMapRatios is { Length: >= 3 })
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
