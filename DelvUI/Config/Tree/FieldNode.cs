using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using ImGuiNET;

namespace DelvUI.Config.Tree
{
    public abstract class ConfigNode
    {
        public bool CollapseControl { get; set; }

        public bool IsChild { get; set; }

        public string Name { get; private set; }

        public bool Nest { get; set; } = true;

        public string? ParentName { get; set; }

        public int Position { get; set; } = Int32.MaxValue;

        public bool Separator { get; set; }

        public bool Spacing { get; set; }

        public bool CollapsingHeader { get; set; }

        public string? ID { get; private set; }

        protected PluginConfigObject ConfigObject { get; set; }

        public ConfigNode(PluginConfigObject configObject, string? id, string name)
        {
            ConfigObject = configObject;
            ID = id;
            Name = name;
        }

        public abstract bool Draw(ref bool changed, int depth = 0);

        protected void DrawSeparatorOrSpacing()
        {
            if (Separator)
            {
                ImGuiHelper.DrawSeparator(1, 1);
            }

            if (Spacing)
            {
                ImGuiHelper.DrawSpacing(1);
            }
        }

        protected static void DrawNestIndicator(int depth)
        {
            // This draws the L shaped symbols and padding to the left of config items collapsible under a checkbox.
            // Shift cursor to the right to pad for children with depth more than 1.
            // 26 is an arbitrary value I found to be around half the width of a checkbox
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(26, 0) * Math.Max((depth - 1), 0));
            ImGui.TextColored(new Vector4(229f / 255f, 57f / 255f, 57f / 255f, 1f), "\u2002\u2514");
            ImGui.SameLine();
        }

        protected static ConfigAttribute? GetConfigAttribute(FieldInfo field)
        {
            return field.GetCustomAttributes(true).Where(a => a is ConfigAttribute).FirstOrDefault() as ConfigAttribute;
        }
    }

    public class FieldNode : ConfigNode
    {
        private SortedDictionary<int, ConfigNode> _childNodes;
        private ConfigAttribute? _configAttribute;
        private FieldInfo _mainField;

        public FieldNode(FieldInfo mainField, PluginConfigObject configObject, string? id) : base(configObject, id, mainField.Name)
        {
            _mainField = mainField;
            _childNodes = new SortedDictionary<int, ConfigNode>();

            _configAttribute = GetConfigAttribute(mainField);
            if (_configAttribute is not null)
            {
                Separator = _configAttribute.separator;
                Spacing = _configAttribute.spacing;
            }
        }

        public void AddChild(int position, ConfigNode field)
        {
            field.IsChild = true;

            while (_childNodes.ContainsKey(position))
            {
                position++;
            }

            _childNodes.Add(position, field);
        }

        public override bool Draw(ref bool changed, int depth = 0)
        {
            bool reset = false;
            DrawSeparatorOrSpacing();

            if (!Nest)
            {
                depth = 0;
            }

            if (depth > 0)
            {
                DrawNestIndicator(depth);
            }

            bool collapsing = CollapsingHeader && ConfigObject.Disableable;

            // Draw the ConfigAttribute
            if (!collapsing)
            {
                DrawConfigAttribute(ref changed, _mainField);
            }

            bool enabled = _mainField.GetValue(ConfigObject) as bool? ?? false;

            // Draw children
            if (CollapseControl && Attribute.IsDefined(_mainField, typeof(CheckboxAttribute)))
            {
                if (collapsing && ImGui.CollapsingHeader(ID + "##CollapsingHeader"))
                {
                    DrawNestIndicator(depth + 1);
                    DrawConfigAttribute(ref changed, _mainField);

                    if (enabled)
                    {
                        reset |= DrawChildren(ref changed, depth + 1);
                    }
                }
                else if (!collapsing && enabled)
                {
                    ImGui.BeginGroup();
                    reset |= DrawChildren(ref changed, depth);
                    ImGui.EndGroup();
                }
            }

            return reset;
        }

        private bool DrawChildren(ref bool changed, int depth)
        {
            bool reset = false;

            int childDepth = depth + 1;
            foreach (ConfigNode child in _childNodes.Values)
            {
                if (child.Separator)
                {
                    childDepth = 0;
                }

                reset |= child.Draw(ref changed, childDepth);
            }

            return reset;
        }

        private void DrawConfigAttribute(ref bool changed, FieldInfo field)
        {
            if (_configAttribute is not null)
            {
                changed |= _configAttribute.Draw(field, ConfigObject, ID, CollapsingHeader);
            }
        }
    }

    public class ManualDrawNode : ConfigNode
    {
        private MethodInfo _drawMethod;

        public ManualDrawNode(MethodInfo method, PluginConfigObject configObject, string? id) : base(configObject, id, id ?? "")
        {
            _drawMethod = method;
        }

        public override bool Draw(ref bool changed, int depth = 0)
        {
            object[] args = new object[] { false };
            bool? result = (bool?)_drawMethod.Invoke(ConfigObject, args);

            bool arg = (bool)args[0];
            changed |= arg;
            return result ?? false;
        }
    }
}