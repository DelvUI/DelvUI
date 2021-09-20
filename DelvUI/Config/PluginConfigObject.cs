﻿using DelvUI.Config.Attributes;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;

namespace DelvUI.Config
{
    public abstract class PluginConfigObject : IOnChangeEventArgs
    {
        [Checkbox("Enabled", separator = true)]
        [Order(0)]
        public bool Enabled = true;

        [JsonIgnore]
        public bool Portable
        {
            get
            {
                PortableAttribute attribute = (PortableAttribute)GetType().GetCustomAttribute(typeof(PortableAttribute), false);
                return attribute == null || attribute.portable;
            }
        }

        [JsonIgnore]
        public bool Disableable
        {
            get
            {
                DisableableAttribute attribute = (DisableableAttribute)GetType().GetCustomAttribute(typeof(DisableableAttribute), false);
                return attribute == null || attribute.disableable;
            }
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

        public static PluginConfigObject DefaultConfig()
        {
            Debug.Assert(false, "Static method 'DefaultConfig' not found !!!");
            return null;
        }

        #region IOnChangeEventArgs

        // sending event outside of the config
        public event EventHandler<OnChangeBaseArgs> onValueChanged;

        // received events from the node
        public void onValueChangedRegisterEvent(OnChangeBaseArgs e)
        {
            onValueChanged?.Invoke(this, e);
        }

        #endregion
    }

    public abstract class MovablePluginConfigObject : PluginConfigObject
    {
        [DragInt2("Position", min = -4000, max = 4000)]
        [Order(5)]
        public Vector2 Position = Vector2.Zero;
    }

    public class PluginConfigColor
    {
        [JsonIgnore] private float[] _colorMapRatios = { -.8f, -.3f, .1f };

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

        [JsonIgnore] public uint TopGradient { get; private set; }

        [JsonIgnore] public uint BottomGradient { get; private set; }

        private void Update()
        {
            Base = ImGui.ColorConvertFloat4ToU32(_vector);
            Background = ImGui.ColorConvertFloat4ToU32(_vector.AdjustColor(_colorMapRatios[0]));
            TopGradient = ImGui.ColorConvertFloat4ToU32(_vector.AdjustColor(_colorMapRatios[1]));
            BottomGradient = ImGui.ColorConvertFloat4ToU32(_vector.AdjustColor(_colorMapRatios[2]));
        }
    }
}
