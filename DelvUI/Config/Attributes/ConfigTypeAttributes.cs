using ImGuiNET;
using System;
using System.Collections.Generic;
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

        public abstract bool Draw(FieldInfo field, PluginConfigObject config, string ID);

        protected string IDText(string ID) => ID != null ? " ##" + ID : "";

        protected void TriggerChangeEvent<T>(PluginConfigObject config, string fieldName, object value)
        {
            if (!isMonitored || config is not IOnChangeEventArgs eventObject)
            {
                return;
            }

            eventObject.onValueChangedRegisterEvent(new OnChangeEventArgs<T>(fieldName, (T)value));
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CheckboxAttribute : ConfigAttribute
    {
        public CheckboxAttribute(string friendlyName) : base(friendlyName) { }

        public override bool Draw(FieldInfo field, PluginConfigObject config, string ID)
        {
            object fieldVal = field.GetValue(config);
            var disableable = config.Disableable;

            if (!disableable && friendlyName == "Enabled")
            {
                if (ID != null)
                {
                    ImGui.Text(ID);
                }
                return false;
            }

            bool boolVal = (bool)fieldVal;

            if (ImGui.Checkbox(ID != null && friendlyName == "Enabled" ? ID : friendlyName + IDText(ID), ref boolVal))
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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string ID)
        {
            float floatVal = (float)field.GetValue(config);

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
        public int velocity;

        public DragIntAttribute(string friendlyName) : base(friendlyName)
        {
            min = 1;
            max = 1000;
            velocity = 1;
        }

        public override bool Draw(FieldInfo field, PluginConfigObject config, string ID)
        {
            int intVal = (int)field.GetValue(config);

            if (ImGui.DragInt(friendlyName + IDText(ID), ref intVal, velocity, min, max))
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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string ID)
        {
            Vector2 vectorVal = (Vector2)field.GetValue(config);

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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string ID)
        {
            Vector2 vectorVal = (Vector2)field.GetValue(config);

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

        public InputTextAttribute(string friendlyName) : base(friendlyName)
        {
            this.friendlyName = friendlyName;
            maxLength = 999;
        }

        public override bool Draw(FieldInfo field, PluginConfigObject config, string ID)
        {
            string stringVal = (string)field.GetValue(config);

            if (ImGui.InputText(friendlyName + IDText(ID), ref stringVal, maxLength))
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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string ID)
        {
            PluginConfigColor colorVal = (PluginConfigColor)field.GetValue(config);
            Vector4 vector = colorVal.Vector;

            if (ImGui.ColorEdit4(friendlyName + IDText(ID), ref vector))
            {
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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string ID)
        {
            int intVal = (int)field.GetValue(config);

            if (ImGui.Combo(friendlyName + IDText(ID), ref intVal, options, options.Length, 4))
            {
                field.SetValue(config, intVal);

                TriggerChangeEvent<float>(config, field.Name, intVal);

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

        public override bool Draw(FieldInfo field, PluginConfigObject config, string ID)
        {
            ImGui.Text(friendlyName);
            int[] order = (int[])field.GetValue(config);

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
    public class DynamicListAttribute : ConfigAttribute
    {
        public string[] options;

        public DynamicListAttribute(string friendlyName, params string[] options) : base(friendlyName)
        {
            this.options = options;
        }

        public override bool Draw(FieldInfo field, PluginConfigObject config, string ID)
        {
            List<string> opts = (List<string>)field.GetValue(config);
            var idText = IDText(ID);

            ImGui.BeginGroup();

            if (ImGui.BeginTable("##myTable2" + friendlyName + idText, 2))
            {
                ImGui.TableNextColumn();

                List<string> addOptions = new(options);
                for (int i = 0; i < opts.Count; i++)
                {
                    addOptions.Remove(opts[i]);
                }

                int intVal = 0;
                ImGui.Text("Add");
                if (ImGui.Combo("##Add" + idText + friendlyName, ref intVal, addOptions.ToArray(), addOptions.Count, 6))
                {
                    var change = addOptions[intVal];
                    opts.Add(change);
                    field.SetValue(config, opts);

                    if (isMonitored && config is IOnChangeEventArgs eventObject)
                    {
                        eventObject.onValueChangedRegisterEvent(
                            new OnChangeEventArgs<string>(field.Name, change, ChangeType.ListAdd)
                        );
                    }

                    return true;
                }

                ImGui.TableNextColumn();

                var removeOpts = opts;

                int removeVal = 0;
                ImGui.Text("Remove");
                if (ImGui.Combo("##Remove" + idText + friendlyName, ref removeVal, removeOpts.ToArray(), removeOpts.Count, 6))
                {
                    var change = removeOpts[removeVal];
                    opts.Remove(change);
                    field.SetValue(config, opts);

                    if (isMonitored && config is IOnChangeEventArgs eventObject)
                    {
                        eventObject.onValueChangedRegisterEvent(
                            new OnChangeEventArgs<string>(field.Name, change, ChangeType.ListRemove)
                        );
                    }

                    return true;
                }

                ImGui.EndTable();
            }

            ImGui.Text(friendlyName + ":");

            if (opts.Count > 0 && ImGui.BeginTable("##myTable" + friendlyName, 5))
            {
                var length = opts.Count;
                for (int i = 0; i < length; i++)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text(opts[i]);
                }
                ImGui.EndTable();
            }

            ImGui.EndGroup();

            return false;
        }
    }

    //[AttributeUsage(AttributeTargets.Field)]
    //public class FontAttribute : ConfigAttribute
    //{
    //    public FontAttribute() : base("") { }

    //    public override bool Draw(FieldInfo field, PluginConfigObject config, string ID)
    //    {

    //    }
    //}
    #endregion

    #region field ordering attributes
    [AttributeUsage(AttributeTargets.Field)]
    public class OrderAttribute : Attribute
    {
        public int pos;

        public OrderAttribute(int pos)
        {
            this.pos = pos;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CollapseControlAttribute : Attribute
    {
        public int pos;
        public int id;


        public CollapseControlAttribute(int pos, int id)
        {
            this.pos = pos;
            this.id = id;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CollapseWithAttribute : Attribute
    {
        public int pos;
        public int id;

        public CollapseWithAttribute(int pos, int id)
        {
            this.pos = pos;
            this.id = id;

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NestedConfigAttribute : Attribute
    {
        public string friendlyName;
        public int pos;

        public NestedConfigAttribute(string friendlyName, int pos)
        {
            this.friendlyName = friendlyName;
            this.pos = pos;

        }
    }

    #endregion
}
