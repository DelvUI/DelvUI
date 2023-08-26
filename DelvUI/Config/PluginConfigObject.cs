using DelvUI.Config.Attributes;
using DelvUI.Enums;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

        [JsonIgnore]
        public string[]? DisableParentSettings
        {
            get
            {
                DisableParentSettingsAttribute? attribute = (DisableParentSettingsAttribute?)GetType().GetCustomAttribute(typeof(DisableParentSettingsAttribute), true);
                return attribute?.DisabledFields;
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

        public List<T> GetObjects<T>()
        {
            List<T> list = new List<T>();

            Type type = typeof(T);
            if (this is T obj)
            {
                list.Add(obj);
            }

            // iterate properties
            PropertyInfo[] properties = GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object? value = property.GetValue(this);

                if (value is T o)
                {
                    list.Add(o);
                }
                else if (value is PluginConfigObject p)
                {
                    list.AddRange(p.GetObjects<T>());
                }
            }

            // iterate fields
            FieldInfo[] fields = GetType().GetFields();
            foreach (FieldInfo field in fields)
            {
                object? value = field.GetValue(this);

                if (value is T o)
                {
                    list.Add(o);
                }
                else if (value is PluginConfigObject p)
                {
                    list.AddRange(p.GetObjects<T>());
                }
            }

            return list;
        }

        public T? Load<T>(FileInfo fileInfo) where T : PluginConfigObject
        {
            return LoadFromJson<T>(fileInfo.FullName);
        }

        public static T? LoadFromJson<T>(string path) where T : PluginConfigObject
        {
            if (!File.Exists(path)) { return null; }

            return LoadFromJsonString<T>(File.ReadAllText(path));
        }

        public static T? LoadFromJsonString<T>(string jsonString) where T : PluginConfigObject
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new PluginConfigObjectsContractResolver();

            return JsonConvert.DeserializeObject<T>(jsonString, settings);
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
        public string ID;

        [StrataLevel("Strata Level")]
        [Order(2)]
        public StrataLevel? Strata;

        public StrataLevel StrataLevel => Strata ?? StrataLevel.LOWEST;

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
        [DragInt2("Size", min = 1, max = 4000, isMonitored = true)]
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

        public static PluginConfigColor FromHex(uint hexColor)
        {
            // ARGB to ABGR
            uint r = (hexColor >> 16) & 0xFF;
            uint b = hexColor & 0xFF;
            hexColor = (hexColor & 0xFF00FF00) | (b << 16) | r;

            return new PluginConfigColor(ImGui.ColorConvertU32ToFloat4(hexColor));
        }

        public PluginConfigColor WithAlpha(float alpha)
        {
            if (alpha == Vector.W) { return this; }

            return new PluginConfigColor(Vector.WithNewAlpha(alpha));
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
