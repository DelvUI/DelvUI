using Dalamud.Interface;
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
    public class PortableAttribute : Attribute
    {
        public bool portable;

        public PortableAttribute(bool portable)
        {
            this.portable = portable;
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

        public ConfigAttribute(string friendlyName)
        {
            this.friendlyName = friendlyName;
        }

        public abstract bool Draw(FieldInfo field, PluginConfigObject config, string? iD);

        protected string IDText(string? iD) => iD != null ? " ##" + iD : "";

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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string? iD)
        {
            bool disableable = config.Disableable;

            if (!disableable && friendlyName == "Enabled")
            {
                if (iD != null)
                {
                    ImGui.Text(iD);
                }
                return false;
            }

            bool? fieldVal = (bool?)field.GetValue(config);
            bool boolVal = fieldVal ?? false;

            if (ImGui.Checkbox(iD != null && friendlyName == "Enabled" ? iD : friendlyName + IDText(iD), ref boolVal))
            {
                field.SetValue(config, boolVal);

                TriggerChangeEvent<bool>(config, field.Name, boolVal);

                return true;
            }

            return false;
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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string? iD)
        {
            float? fieldVal = (float?)field.GetValue(config);
            float floatVal = fieldVal ?? 0;

            if (ImGui.DragFloat(friendlyName + IDText(iD), ref floatVal, velocity, min, max))
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
        public int velocity;

        public DragIntAttribute(string friendlyName) : base(friendlyName)
        {
            min = 1;
            max = 1000;
            velocity = 1;
        }

        public override bool Draw(FieldInfo field, PluginConfigObject config, string? iD)
        {
            int? fieldVal = (int?)field.GetValue(config);
            int intVal = fieldVal ?? 0;

            if (ImGui.DragInt(friendlyName + IDText(iD), ref intVal, velocity, min, max))
            {
                field.SetValue(config, intVal);

                TriggerChangeEvent<float>(config, field.Name, intVal);

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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string? iD)
        {
            Vector2? fieldVal = (Vector2?)field.GetValue(config);
            Vector2 vectorVal = fieldVal ?? Vector2.Zero;

            if (ImGui.DragFloat2(friendlyName + IDText(iD), ref vectorVal, velocity, min, max))
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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string? iD)
        {
            Vector2? fieldVal = (Vector2?)field.GetValue(config);
            Vector2 vectorVal = fieldVal ?? Vector2.Zero;

            if (ImGui.DragFloat2(friendlyName + IDText(iD), ref vectorVal, velocity, min, max))
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

        public InputTextAttribute(string friendlyName) : base(friendlyName)
        {
            this.friendlyName = friendlyName;
            maxLength = 999;
        }

        public override bool Draw(FieldInfo field, PluginConfigObject config, string? iD)
        {
            string? fieldVal = (string?)field.GetValue(config);
            string stringVal = fieldVal ?? "";

            if (ImGui.InputText(friendlyName + IDText(iD), ref stringVal, maxLength))
            {
                field.SetValue(config, stringVal);

                TriggerChangeEvent<string>(config, field.Name, stringVal);

                return true;
            }

            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ColorEdit4Attribute : ConfigAttribute
    {
        public ColorEdit4Attribute(string friendlyName) : base(friendlyName) { }

        public override bool Draw(FieldInfo field, PluginConfigObject config, string? iD)
        {
            PluginConfigColor? colorVal = (PluginConfigColor?)field.GetValue(config);
            Vector4 vector = (colorVal != null ? colorVal.Vector : Vector4.Zero);

            if (ImGui.ColorEdit4(friendlyName + IDText(iD), ref vector))
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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string? iD)
        {
            object? fieldVal = field.GetValue(config);

            int intVal = 0;
            if (fieldVal != null)
            {
                intVal = (int)fieldVal;
            }

            if (ImGui.Combo(friendlyName + IDText(iD), ref intVal, options, options.Length, 4))
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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string? iD)
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
                        int _curri = order[i];
                        order[i] = order[i + 1];
                        order[i + 1] = _curri;
                        field.SetValue(config, order);
                        ImGui.ResetMouseDragDelta();
                    }
                    else if ((drag_dx < -80.0f && i > 0))
                    {
                        int _curri = order[i];
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
    public class DynamicListAttribute : ConfigAttribute
    {
        public string[] options;

        public DynamicListAttribute(string friendlyName, params string[] options) : base(friendlyName)
        {
            this.options = options;
        }

        public override bool Draw(FieldInfo field, PluginConfigObject config, string? iD)
        {
            bool changed = false;

            List<string>? fieldVal = (List<string>?)field.GetValue(config);
            List<string> opts = fieldVal ?? new List<string>();

            string? idText = IDText(iD);
            int indexToRemove = -1;

            ImGui.BeginChild(friendlyName, new Vector2(400, 230));

            List<string> addOptions = new(options);
            for (int i = 0; i < opts.Count; i++)
            {
                addOptions.Remove(opts[i]);
            }

            int intVal = 0;
            ImGui.Text("Add");
            if (ImGui.Combo("##Add" + idText + friendlyName, ref intVal, addOptions.ToArray(), addOptions.Count, 6))
            {
                changed = true;

                string? change = addOptions[intVal];
                opts.Add(change);
                field.SetValue(config, opts);

                TriggerChangeEvent<string>(config, field.Name, change, ChangeType.ListAdd);
            }

            ImGui.Text(friendlyName + ":");
            ImGuiTableFlags flags =
                ImGuiTableFlags.RowBg |
                ImGuiTableFlags.Borders |
                ImGuiTableFlags.BordersOuter |
                ImGuiTableFlags.BordersInner |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingFixedSame;

            if (ImGui.BeginTable("##myTable2" + friendlyName + idText, 2, flags, new Vector2(326, 150)))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0, 0);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 0, 1);

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                for (int i = 0; i < opts.Count(); i++)
                {
                    ImGui.PushID(i.ToString());
                    ImGui.TableNextRow(ImGuiTableRowFlags.None);

                    if (ImGui.TableSetColumnIndex(0))
                    {
                        ImGui.Text(opts[i]);
                    }

                    if (ImGui.TableSetColumnIndex(1))
                    {
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);

                        if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString()))
                        {
                            changed = true;
                            indexToRemove = i;
                        }

                        ImGui.PopFont();
                        ImGui.PopStyleColor(3);
                    }
                }

                ImGui.EndTable();
            }

            if (indexToRemove >= 0)
            {
                changed = true;

                string? change = opts[indexToRemove];
                opts.Remove(change);
                field.SetValue(config, opts);

                TriggerChangeEvent<string>(config, field.Name, change, ChangeType.ListRemove);
            }

            ImGui.EndChild();

            return changed;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class FontAttribute : ConfigAttribute
    {
        public FontAttribute() : base("Font and Size") { }

        public override bool Draw(FieldInfo field, PluginConfigObject config, string? iD)
        {
            FontsConfig? fontsConfig = ConfigurationManager.Instance.GetConfigObject<FontsConfig>();
            if (fontsConfig == null)
            {
                return false;
            }

            string? stringVal = (string?)field.GetValue(config);

            int index = stringVal == null || stringVal.Length == 0 || !fontsConfig.Fonts.ContainsKey(stringVal) ? -1 :
                fontsConfig.Fonts.IndexOfKey(stringVal);

            if (index == -1)
            {
                if (fontsConfig.Fonts.ContainsKey(fontsConfig.DefaultFontKey))
                {
                    index = fontsConfig.Fonts.IndexOfKey(fontsConfig.DefaultFontKey);
                }
                else
                {
                    index = 0;
                }
            }

            string[]? options = fontsConfig.Fonts.Values.Select(fontData => fontData.Name + "\u2002\u2002" + fontData.Size.ToString()).ToArray();

            if (ImGui.Combo(friendlyName + IDText(iD), ref index, options, options.Length, 4))
            {
                stringVal = fontsConfig.Fonts.Keys[index];
                field.SetValue(config, stringVal);

                TriggerChangeEvent<string>(config, field.Name, stringVal);

                return true;
            }

            return false;
        }
    }
    #endregion

    #region field ordering attributes
    [AttributeUsage(AttributeTargets.Field)]
    public class AnchorAttribute : ComboAttribute
    {
        public AnchorAttribute(string friendlyName)
            : base(friendlyName, new string[] { "Center", "Left", "Right", "Top", "TopLeft", "TopRight", "Bottom", "BottomLeft", "BottomRight" })
        {
        }
    }

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
    public class NestedConfigAttribute : Attribute
    {
        public string friendlyName;
        public int pos;
        public bool separator = true;
        public bool spacing = false;
        public string? collapseWith = "Enabled";

        public NestedConfigAttribute(string friendlyName, int pos)
        {
            this.friendlyName = friendlyName;
            this.pos = pos;

        }
    }

    #endregion
}
