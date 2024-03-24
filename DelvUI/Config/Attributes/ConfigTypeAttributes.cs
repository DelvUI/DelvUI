using Dalamud.Interface;
using Dalamud.Interface.Utility;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace DelvUI.Config.Attributes
{
    #region class attributes
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportableAttribute : Attribute
    {
        public bool exportable;

        public ExportableAttribute(bool exportable)
        {
            this.exportable = exportable;
        }
    }

    public class ShareableAttribute : Attribute
    {
        public bool shareable;

        public ShareableAttribute(bool shareable)
        {
            this.shareable = shareable;
        }
    }

    public class ResettableAttribute : Attribute
    {
        public bool resettable;

        public ResettableAttribute(bool resettable)
        {
            this.resettable = resettable;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DisableableAttribute : Attribute
    {
        public bool disableable;

        public DisableableAttribute(bool disableable)
        {
            this.disableable = disableable;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DisableParentSettingsAttribute : Attribute
    {
        public readonly string[] DisabledFields;

        public DisableParentSettingsAttribute(params string[] fields)
        {
            this.DisabledFields = fields;
        }
    }
    #endregion

    #region method attributes
    [AttributeUsage(AttributeTargets.Method)]
    public class ManualDrawAttribute : Attribute
    {
    }
    #endregion

    #region field attributes
    [AttributeUsage(AttributeTargets.Field)]
    public abstract class ConfigAttribute : Attribute
    {
        public string friendlyName;
        public bool isMonitored = false;
        public bool separator = false;
        public bool spacing = false;
        public string? help = null;

        public ConfigAttribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
        }

        public bool Draw(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader = false)
        {
            bool result = DrawField(field, config, ID, collapsingHeader);

            if (help != null && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(help);
            }

            return result;
        }

        public abstract bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader = false);

        protected string IDText(string? ID) => ID != null ? " ##" + ID : "";

        protected void TriggerChangeEvent<T>(PluginConfigObject config, string fieldName, object value, ChangeType type = ChangeType.None)
        {
            if (!isMonitored || config is not IOnChangeEventArgs eventObject)
            {
                return;
            }

            eventObject.OnValueChanged(new OnChangeEventArgs<T>(fieldName, (T)value, type));
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CheckboxAttribute : ConfigAttribute
    {
        public CheckboxAttribute(string friendlyName) : base(friendlyName) { }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            var disableable = config.Disableable;

            if (!disableable && friendlyName == "Enabled")
            {
                if (ID != null)
                {
                    ImGui.Text(ID);
                }
                return false;
            }

            bool? fieldVal = (bool?)field.GetValue(config);
            bool boolVal = fieldVal.HasValue ? fieldVal.Value : false;

            if (ImGui.Checkbox(ID != null && friendlyName == "Enabled" && !collapsingHeader ? ID : friendlyName + IDText(ID), ref boolVal))
            {
                field.SetValue(config, boolVal);

                TriggerChangeEvent<bool>(config, field.Name, boolVal);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class RadioSelector : ConfigAttribute
    {
        private string[] _options;

        public RadioSelector(params string[] options) : base(string.Join("_", options))
        {
            _options = options;
        }

        public RadioSelector(Type enumType) : this(enumType.IsEnum ? Enum.GetNames(enumType) : Array.Empty<string>()) { }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            bool changed = false;
            object? fieldVal = field.GetValue(config);

            int intVal = 0;
            if (fieldVal != null)
            {
                intVal = (int)fieldVal;
            }

            for (int i = 0; i < _options.Length; i++)
            {
                changed |= ImGui.RadioButton(_options[i], ref intVal, i);
                if (i < _options.Length - 1)
                {
                    ImGui.SameLine();
                }
            }

            if (changed)
            {
                field.SetValue(config, intVal);
                TriggerChangeEvent<int>(config, field.Name, intVal);
            }

            return changed;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragFloatAttribute : ConfigAttribute
    {
        public float min;
        public float max;
        public float velocity;

        public DragFloatAttribute(string friendlyName) : base(friendlyName)
        {
            min = 1f;
            max = 1000f;
            velocity = 1f;
        }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            float? fieldVal = (float?)field.GetValue(config);
            float floatVal = fieldVal.HasValue ? fieldVal.Value : 0;

            if (ImGui.DragFloat(friendlyName + IDText(ID), ref floatVal, velocity, min, max))
            {
                field.SetValue(config, floatVal);

                TriggerChangeEvent<float>(config, field.Name, floatVal);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragIntAttribute : ConfigAttribute
    {
        public int min;
        public int max;
        public float velocity;

        public DragIntAttribute(string friendlyName) : base(friendlyName)
        {
            min = 1;
            max = 1000;
            velocity = 1;
        }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            int? fieldVal = (int?)field.GetValue(config);
            int intVal = fieldVal.HasValue ? fieldVal.Value : 0;

            if (ImGui.DragInt(friendlyName + IDText(ID), ref intVal, velocity, min, max))
            {
                field.SetValue(config, intVal);

                TriggerChangeEvent<int>(config, field.Name, intVal);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragFloat2Attribute : ConfigAttribute
    {
        public float min;
        public float max;
        public float velocity;

        public DragFloat2Attribute(string friendlyName) : base(friendlyName)
        {
            min = 1f;
            max = 1000f;
            velocity = 1f;
        }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            Vector2? fieldVal = (Vector2?)field.GetValue(config);
            Vector2 vectorVal = fieldVal.HasValue ? fieldVal.Value : Vector2.Zero;

            if (ImGui.DragFloat2(friendlyName + IDText(ID), ref vectorVal, velocity, min, max))
            {
                field.SetValue(config, vectorVal);

                TriggerChangeEvent<Vector2>(config, field.Name, vectorVal);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragInt2Attribute : ConfigAttribute
    {
        public int min;
        public int max;
        public int velocity;

        public DragInt2Attribute(string friendlyName) : base(friendlyName)
        {
            min = 1;
            max = 1000;
            velocity = 1;
        }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            Vector2? fieldVal = (Vector2?)field.GetValue(config);
            Vector2 vectorVal = fieldVal.HasValue ? fieldVal.Value : Vector2.Zero;

            if (ImGui.DragFloat2(friendlyName + IDText(ID), ref vectorVal, velocity, min, max))
            {
                field.SetValue(config, vectorVal);

                TriggerChangeEvent<Vector2>(config, field.Name, vectorVal);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InputTextAttribute : ConfigAttribute
    {
        public uint maxLength;
        public bool formattable = true;

        private string _searchText = "";

        public InputTextAttribute(string friendlyName) : base(friendlyName)
        {
            this.friendlyName = friendlyName;
            maxLength = 999;
        }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            string? fieldVal = (string?)field.GetValue(config);
            string stringVal = fieldVal ?? "";
            string? finalValue = null;

            string popupId = ID != null ? "DelvUI_TextTagsList " + ID : "DelvUI_TextTagsList ##" + friendlyName;
           
            if (!formattable)
            {
                if (ImGui.InputText(friendlyName + IDText(ID), ref stringVal, maxLength))
                {
                    finalValue = stringVal;
                }
            }
            else
            {
                float scale = ImGuiHelpers.GlobalScale;
                float width = ImGui.CalcItemWidth();
                float height = Math.Max(24 * scale, ImGui.CalcTextSize(stringVal, false, width).Y + 6 * scale);
                Vector2 size = new Vector2(width, height);

                if (ImGui.InputTextMultiline(friendlyName + IDText(ID), ref stringVal, maxLength, size, ImGuiInputTextFlags.AllowTabInput))
                {
                    finalValue = stringVal;
                }

                // text tags
                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Pen.ToIconString() + "##" + ID))
                {
                    ImGui.OpenPopup(popupId);
                }
                ImGui.PopFont();

                ImGuiHelper.SetTooltip("Text Tags");
            }

            var selectedTag = ImGuiHelper.DrawTextTagsList(popupId, ref _searchText);
            if (selectedTag != null)
            {
                finalValue = stringVal + selectedTag;
            }

            if (finalValue != null)
            {
                field.SetValue(config, finalValue);
                TriggerChangeEvent<string>(config, field.Name, finalValue);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ColorEdit4Attribute : ConfigAttribute
    {
        public ColorEdit4Attribute(string friendlyName) : base(friendlyName) { }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            PluginConfigColor? colorVal = (PluginConfigColor?)field.GetValue(config);
            Vector4 vector = (colorVal != null ? colorVal.Vector : Vector4.Zero);

            if (ImGui.ColorEdit4(friendlyName + IDText(ID), ref vector, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
            {
                if (colorVal is null)
                {
                    return false;
                }

                colorVal.Vector = vector;
                field.SetValue(config, colorVal);

                TriggerChangeEvent<PluginConfigColor>(config, field.Name, colorVal);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ComboAttribute : ConfigAttribute
    {
        public string[] options;

        public ComboAttribute(string friendlyName, params string[] options) : base(friendlyName)
        {
            this.options = options;
        }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            object? fieldVal = field.GetValue(config);

            int intVal = 0;
            if (fieldVal != null)
            {
                intVal = (int)fieldVal;
            }

            if (ImGui.Combo(friendlyName + IDText(ID), ref intVal, options, options.Length, 4))
            {
                field.SetValue(config, intVal);

                TriggerChangeEvent<int>(config, field.Name, intVal);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DragDropHorizontalAttribute : ConfigAttribute
    {
        public string[] names;

        public DragDropHorizontalAttribute(string friendlyName, params string[] names) : base(friendlyName)
        {
            this.names = names;
        }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            ImGui.Text(friendlyName);
            int[]? fieldVal = (int[]?)field.GetValue(config);
            int[] order = fieldVal ?? Array.Empty<int>();

            for (int i = 0; i < order.Length; i++)
            {
                ImGui.SameLine();
                ImGui.Button(names[order[i]], new Vector2(100, 25));

                if (ImGui.IsItemActive())
                {
                    float drag_dx = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left).X;

                    if ((drag_dx > 80.0f && i < order.Length - 1))
                    {
                        var _curri = order[i];
                        order[i] = order[i + 1];
                        order[i + 1] = _curri;
                        field.SetValue(config, order);
                        ImGui.ResetMouseDragDelta();
                    }
                    else if ((drag_dx < -80.0f && i > 0))
                    {
                        var _curri = order[i];
                        order[i] = order[i - 1];
                        order[i - 1] = _curri;
                        field.SetValue(config, order);
                        ImGui.ResetMouseDragDelta();
                    }

                    TriggerChangeEvent<int[]>(config, field.Name, order);

                    return true;
                }
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class FontAttribute : ConfigAttribute
    {
        public FontAttribute(string friendlyName = "Font and Size") : base(friendlyName) { }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            var fontsConfig = ConfigurationManager.Instance.GetConfigObject<FontsConfig>();
            if (fontsConfig == null)
            {
                return false;
            }

            string? stringVal = (string?)field.GetValue(config);

            int index = stringVal == null || stringVal.Length == 0 || !fontsConfig.Fonts.ContainsKey(stringVal) ? -1 :
                fontsConfig.Fonts.IndexOfKey(stringVal);

            if (index == -1)
            {
                if (fontsConfig.Fonts.ContainsKey(FontsConfig.DefaultBigFontKey))
                {
                    index = fontsConfig.Fonts.IndexOfKey(FontsConfig.DefaultBigFontKey);
                }
                else
                {
                    index = 0;
                }
            }

            var options = fontsConfig.Fonts.Values.Select(fontData => fontData.Name + "  " + fontData.Size.ToString()).ToArray();

            if (ImGui.Combo(friendlyName + IDText(ID), ref index, options, options.Length, 4))
            {
                stringVal = fontsConfig.Fonts.Keys[index];
                field.SetValue(config, stringVal);

                TriggerChangeEvent<string>(config, field.Name, stringVal);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class BarTextureAttribute : ConfigAttribute
    {
        public BarTextureAttribute(string friendlyName = "Bar Texture") : base(friendlyName) { }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            if (BarTexturesManager.Instance == null)
            {
                return false;
            }

            List<string> textures = BarTexturesManager.Instance.BarTextureNames.ToList();
            string? stringVal = (string?)field.GetValue(config);

            int index = 0; 
            if (stringVal != null && stringVal.Length > 0 && textures.Contains(stringVal))
            {
                index = textures.IndexOf(stringVal);
            }

            string[] options = textures.ToArray();

            if (ImGui.Combo(friendlyName + IDText(ID), ref index, options, options.Length, 10))
            {
                stringVal = options[index];
                field.SetValue(config, stringVal);

                TriggerChangeEvent<string>(config, field.Name, stringVal);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class AnchorAttribute : ComboAttribute
    {
        public AnchorAttribute(string friendlyName)
            : base(friendlyName, new string[] { "Center", "Left", "Right", "Top", "TopLeft", "TopRight", "Bottom", "BottomLeft", "BottomRight" })
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class BarTextureDrawModeAttribute : ComboAttribute
    {
        public BarTextureDrawModeAttribute(string friendlyName)
            : base(friendlyName, new string[] { "Stretch", "Repeat Horizontal", "Repeat Vertical", "Repeat" })
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class StrataLevelAttribute : ConfigAttribute
    {
        private string[] options = { "Lowest", "Low", "Mid-Low", "Mid", "Mid-High", "High", "Highest" };

        public StrataLevelAttribute(string friendlyName) : base(friendlyName)
        {
        }

        public override bool DrawField(FieldInfo field, PluginConfigObject config, string? ID, bool collapsingHeader)
        {
            object? fieldVal = field.GetValue(config);

            int intVal = 0;
            if (fieldVal != null)
            {
                intVal = (int)fieldVal;
            }

            if (ImGui.Combo(friendlyName + IDText(ID), ref intVal, options, options.Length, 4))
            {
                field.SetValue(config, (StrataLevel?)intVal);

                TriggerChangeEvent<int>(config, field.Name, intVal);
                ConfigurationManager.Instance?.OnStrataLevelChanged(config);

                return true;
            }

            return false;
        }
    }
    #endregion

    #region field ordering attributes
    [AttributeUsage(AttributeTargets.Field)]
    public class OrderAttribute : Attribute
    {
        public int pos;
        public string? collapseWith = "Enabled";

        public OrderAttribute(int pos)
        {
            this.pos = pos;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NestedConfigAttribute : OrderAttribute
    {
        public string friendlyName;
        public bool separator = false;
        public bool spacing = true;
        public bool nest = true;
        public bool collapsingHeader = true;

        public NestedConfigAttribute(string friendlyName, int pos) : base(pos)
        {
            this.friendlyName = friendlyName;

        }
    }

    #endregion
}
