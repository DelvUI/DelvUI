using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using DelvUI.Helpers;

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
        [JsonIgnore] private Vector4 _vector;
        public Vector4 Vector
        {
            get { return _vector; }
            set
            {
                if (_vector == value) return;
                _vector = value;

                Update();
            }
        }

        [JsonIgnore] private uint _base;
        [JsonIgnore] public uint Base { get { return _base; } }

        [JsonIgnore] private uint _background;
        [JsonIgnore] public uint Background { get { return _background; } }

        [JsonIgnore] private uint _leftGradient;
        [JsonIgnore] public uint LeftGradient { get { return _leftGradient; } }

        [JsonIgnore] private uint _rightGradient;
        [JsonIgnore] public uint RightGradient { get { return _rightGradient; } }

        [JsonIgnore] private Dictionary<string, uint> _map;
        [JsonIgnore] public Dictionary<string, uint> Map { get { return _map; } }

        [JsonIgnore] private float[] _colorMapRatios = new float[] { -.8f, -.1f, .1f };


        public PluginConfigColor(Vector4 vector, float[] colorMapRatios = null)
        {
            _vector = vector;

            if (colorMapRatios != null && colorMapRatios.Length >= 3)
            {
                _colorMapRatios = colorMapRatios;
            }

            Update();
        }

        private void Update()
        {
            _base = ImGui.ColorConvertFloat4ToU32(_vector);
            _background = ImGui.ColorConvertFloat4ToU32(_vector.AdjustColor(_colorMapRatios[0]));
            _leftGradient = ImGui.ColorConvertFloat4ToU32(_vector.AdjustColor(_colorMapRatios[1]));
            _rightGradient = ImGui.ColorConvertFloat4ToU32(_vector.AdjustColor(_colorMapRatios[2]));
            _map = new Dictionary<string, uint>()
            {
                ["base"] = _base,
                ["background"] = _background,
                ["gradientLeft"] = _leftGradient,
                ["gradientRight"] = _rightGradient
            };
        }
    }
}
