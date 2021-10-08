using DelvUI.Config.Attributes;
using DelvUI.Enums;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Reflection;

namespace DelvUI.Config
{
    public abstract class PluginConfigObject : IOnChangeEventArgs
    {
        public string Version => Plugin.Version;

        [Checkbox("Enabled")]
        [Order(0, collapseWith = null)]
        public bool Enabled = true;

        #region convenience properties
        [JsonIgnore]
        public bool Exportable
        {
            get
            {
                ExportableAttribute? attribute = (ExportableAttribute?)GetType().GetCustomAttribute(typeof(ExportableAttribute), false);
                return attribute == null || attribute.exportable;
            }
        }

        [JsonIgnore]
        public bool Shareable
        {
            get
            {
                ShareableAttribute? attribute = (ShareableAttribute?)GetType().GetCustomAttribute(typeof(ShareableAttribute), false);
                return attribute == null || attribute.shareable;
            }
        }

        [JsonIgnore]
        public bool Resettable
        {
            get
            {
                ResettableAttribute? attribute = (ResettableAttribute?)GetType().GetCustomAttribute(typeof(ResettableAttribute), false);
                return attribute == null || attribute.resettable;
            }
        }

        [JsonIgnore]
        public bool Disableable
        {
            get
            {
                DisableableAttribute? attribute = (DisableableAttribute?)GetType().GetCustomAttribute(typeof(DisableableAttribute), false);
                return attribute == null || attribute.disableable;
            }
        }
        #endregion

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
            return null!;
        }

        #region IOnChangeEventArgs

        // sending event outside of the config
        public event ConfigValueChangeEventHandler? ValueChangeEvent;

        // received events from the node
        public void OnValueChanged(OnChangeBaseArgs e)
        {
            ValueChangeEvent?.Invoke(this, e);
        }

        #endregion
    }

    public abstract class MovablePluginConfigObject : PluginConfigObject
    {
        [JsonIgnore]
        public readonly string ID;

        [DragInt2("Position", min = -4000, max = 4000)]
        [Order(5)]
        public Vector2 Position = Vector2.Zero;

        public MovablePluginConfigObject()
        {
            ID = $"DelvUI_{GetType().Name}_{Guid.NewGuid()}";
        }
    }

    public abstract class AnchorablePluginConfigObject : MovablePluginConfigObject
    {
        [DragInt2("Size", min = 1, max = 4000)]
        [Order(10)]
        public Vector2 Size;

        [Anchor("Anchor")]
        [Order(15)]
        public DrawAnchor Anchor = DrawAnchor.Center;
    }

    public class PluginConfigColor
    {
        [JsonIgnore] private float[] _colorMapRatios = { -.8f, -.3f, .1f };

        [JsonIgnore] private Vector4 _vector;

        public PluginConfigColor(Vector4 vector, float[]? colorMapRatios = null)
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
