using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DelvUI.Config
{
    [Serializable]
    public abstract class PluginConfigObject : INotifyPropertyChanged
    {
        public bool Enabled = true;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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

        protected bool Draw()
        {
            var changed = ImGui.Checkbox("Enabled", ref Enabled);
            ImGui.Spacing();

            return changed;
        }
    }

    [Serializable]
    public abstract class MovablePluginConfigObject : PluginConfigObject
    {
        [JsonIgnore] protected Vector2 _position = Vector2.Zero;
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position == value)
                {
                    return;
                }

                _position = value;
                NotifyPropertyChanged();
            }
        }

        [JsonIgnore] protected Vector2 _size = Vector2.Zero;
        public Vector2 Size
        {
            get => _size;
            set
            {
                if (_position == value)
                {
                    return;
                }

                _size = value;
                NotifyPropertyChanged();
            }
        }

        public MovablePluginConfigObject(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public MovablePluginConfigObject(Vector2 position)
        {
            Position = position;
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
