using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace DelvUI.Config.Tree
{
    public class FieldNode : Node
    {
        public FieldInfo MainField;
        public bool CollapseControl;
        public int Depth = 0;

        private SortedDictionary<int, FieldNode> _childrenFields;
        private PluginConfigObject _configObject;
        private string? _id;
        private bool _hasSeparator = false;
        private bool _isChild = false;
        private ConfigAttribute? _configAttribute;

        public FieldNode(FieldInfo mainField, PluginConfigObject configObject, string? id = null)
        {
            MainField = mainField;
            CollapseControl = false;

            _configObject = configObject;
            _childrenFields = new SortedDictionary<int, FieldNode>();
            _id = id;

            _configAttribute = GetConfigAttribute(mainField);
            if (_configAttribute is not null)
            {
                _hasSeparator = _configAttribute.separator;
            }
        }

        public void AddChild(int position, FieldNode field)
        {
            field._isChild = true;
            _childrenFields.Add(position, field);
        }

        public void Draw(ref bool changed, bool separatorDrawn = false)
        {
            if (!_isChild)
            {
                DrawSeparatorOrSpacing(MainField, _id);
            }

            // Draw the ConfigAttribute
            Draw(ref changed, MainField);

            // Draw children
            if (CollapseControl && Attribute.IsDefined(MainField, typeof(CheckboxAttribute)) && (MainField.GetValue(_configObject) as bool? ?? false))
            {
                ImGui.BeginGroup();

                foreach (FieldNode child in _childrenFields.Values)
                {
                    DrawSeparatorOrSpacing(child.MainField, _id);
                    separatorDrawn |= child._hasSeparator;

                    // Shift everything left if a separator was drawn
                    var depth = separatorDrawn ? child.Depth - 1 : child.Depth;

                    // This draws the L shaped symbols and padding to the left of config items collapsible under a checkbox.
                    if (depth > 0)
                    {
                        // Shift cursor to the right to pad for children with depth more than 1.
                        // 26 is an arbitrary value I found to be around half the width of a checkbox
                        ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(26, 0) * Math.Max((depth - 1), 0));
                        ImGui.TextColored(new Vector4(229f / 255f, 57f / 255f, 57f / 255f, 1f), "\u2002\u2514");
                        ImGui.SameLine();
                    }

                    child.Draw(ref changed, separatorDrawn);
                }

                ImGui.EndGroup();
            }
        }

        public void Draw(ref bool changed, FieldInfo field)
        {
            if (_configAttribute is not null)
            {
                changed |= _configAttribute.Draw(field, _configObject, _id);
            }
        }

        public ConfigAttribute? GetConfigAttribute(FieldInfo field)
        {
            return field.GetCustomAttributes(true).Where(a => a is ConfigAttribute).FirstOrDefault() as ConfigAttribute;
        }

        public void DrawSeparatorOrSpacing(FieldInfo field, string? ID)
        {
            foreach (object attribute in field.GetCustomAttributes(true))
            {
                if (attribute is ConfigAttribute { separator: true })
                {
                    if (attribute is CheckboxAttribute checkboxAttribute && (checkboxAttribute.friendlyName != "Enabled" || ID is null) && checkboxAttribute.friendlyName == "Enabled")
                    {
                        continue;
                    }

                    ImGuiHelper.DrawSeparator(1, 1);
                }
                else if (attribute is ConfigAttribute { spacing: true })
                {
                    ImGuiHelper.DrawSpacing(1);
                }
            }
        }
    }
}
