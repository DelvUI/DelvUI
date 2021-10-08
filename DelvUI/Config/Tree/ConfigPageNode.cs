using Dalamud.Logging;
using DelvUI.Config.Attributes;
using DelvUI.Config.Profiles;
using DelvUI.Helpers;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace DelvUI.Config.Tree
{
    public class ConfigPageNode : SubSectionNode
    {
        private PluginConfigObject _configObject = null!;
        public PluginConfigObject ConfigObject
        {
            get => _configObject;
            set
            {
                _configObject = value;
                GenerateNestedConfigPageNodes();
                _drawList = null;
            }
        }

        private bool _hasSeparator = true;
        private bool _hasSpacing = false;

        private FieldInfo? _parentCollapseField = null;
        private PluginConfigObject? _parentConfigObject = null;

        private List<KeyValuePair<int, object>>? _drawList = null;

        private Dictionary<string, ConfigPageNode> _nestedConfigPageNodes = null!;

        private void GenerateNestedConfigPageNodes()
        {
            _nestedConfigPageNodes = new Dictionary<string, ConfigPageNode>();

            FieldInfo[] fields = _configObject.GetType().GetFields();

            foreach (var field in fields)
            {
                foreach (var attribute in field.GetCustomAttributes(true))
                {
                    if (attribute is not NestedConfigAttribute nestedConfigAttribute)
                    {
                        continue;
                    }

                    var value = field.GetValue(_configObject);

                    if (value is not PluginConfigObject nestedConfig)
                    {
                        continue;
                    }
                    ConfigPageNode configPageNode = new();
                    configPageNode.ConfigObject = nestedConfig;
                    configPageNode.Name = nestedConfigAttribute.friendlyName;

                    if (nestedConfig.Disableable)
                    {
                        configPageNode.Name += "##" + nestedConfig.GetHashCode();
                    }

                    _nestedConfigPageNodes.Add(field.Name, configPageNode);
                }
            }
        }

        public override string? GetBase64String()
        {
            return ImportExportHelper.GenerateExportString(ConfigObject);
        }

        protected override bool AllowExport()
        {
            return ConfigObject.Exportable;
        }

        public override bool Draw(ref bool changed) { return DrawWithID(ref changed); }

        private bool DrawWithID(ref bool changed, string? ID = null)
        {
            bool didReset = false;

            // Only do this stuff the first time the config page is loaded
            if (_drawList is null)
            {
                GenerateDrawList(ID);
            }

            foreach (KeyValuePair<int, object> pair in _drawList!)
            {
                if (pair.Value is FieldNode fieldNode)
                {
                    fieldNode.Draw(ref changed);
                }
                else if (pair.Value is ConfigPageNode node)
                {
                    // If the parent checkbox of this nested config is disabled, don't draw this nestedconfig
                    if (node._parentCollapseField is not null && node._parentConfigObject is not null)
                    {
                        if (!(Attribute.IsDefined(node._parentCollapseField, typeof(CheckboxAttribute)) &&
                            (node._parentCollapseField.GetValue(node._parentConfigObject) as bool? ?? false)))
                        {
                            continue;
                        }
                    }

                    ImGui.BeginGroup();
                    if (node._hasSeparator)
                    {
                        ImGuiHelper.DrawSeparator(1, 1);
                    }
                    if (node._hasSpacing)
                    {
                        ImGuiHelper.DrawSpacing(1);
                    }

                    node.DrawWithID(ref changed, node.Name);
                    ImGui.EndGroup();
                }
            }

            // if the ConfigPageNode requires any manual drawing (i.e. not dictated by attributes), draw it now
            foreach (MethodInfo method in ConfigObject.GetType().GetMethods())
            {
                if (!method.GetCustomAttributes(typeof(ManualDrawAttribute), false).Any())
                {
                    continue;
                }

                object[] args = new object[] { false };
                bool? result = (bool?)method.Invoke(ConfigObject, args);

                bool arg = (bool)args[0];
                changed |= arg;
                didReset |= (result.HasValue && result.Value);
            }

            didReset |= DrawPortableSection();

            return didReset;
        }

        private void GenerateDrawList(string? ID = null)
        {
            _drawList = new List<KeyValuePair<int, object>>();
            FieldInfo[] fields = ConfigObject.GetType().GetFields();
            List<FieldNode> collapseWithList = new();

            foreach (FieldInfo field in fields)
            {
                bool wasAdded = false;

                foreach (object attribute in field.GetCustomAttributes(true))
                {
                    if (attribute is OrderAttribute orderAttribute)
                    {
                        FieldNode fieldNode = new FieldNode(field, ConfigObject, ID);

                        // By default the 'Enabled' checkbox (inherited from PluginConfigObject) is considered the parent of all config items,
                        // so if the ConfigObject is marked as undisableable, we bypass adding items as children *only* if their parent was the 'Enabled' field.
                        if (orderAttribute.collapseWith is null || (!ConfigObject.Disableable && orderAttribute.collapseWith.Equals("Enabled")))
                        {
                            _drawList.Add(new KeyValuePair<int, object>(orderAttribute.pos, fieldNode));
                        }
                        else
                        {
                            collapseWithList.Add(fieldNode);
                        }

                        wasAdded = true;
                    }
                    else if (attribute is NestedConfigAttribute nestedConfigAttribute && _nestedConfigPageNodes.TryGetValue(field.Name, out ConfigPageNode? node))
                    {
                        node._hasSeparator = nestedConfigAttribute.separator;
                        node._hasSpacing = nestedConfigAttribute.spacing;
                        if (nestedConfigAttribute.collapseWith is not null)
                        {
                            node._parentCollapseField = fields.Where(f => f.Name.Equals(nestedConfigAttribute.collapseWith)).FirstOrDefault();
                            node._parentConfigObject = ConfigObject;
                        }

                        _drawList.Add(new KeyValuePair<int, object>(nestedConfigAttribute.pos, node));
                        wasAdded = true;
                    }
                }

                if (!wasAdded && Attribute.IsDefined(field, typeof(ConfigAttribute), true))
                {
                    _drawList.Add(new KeyValuePair<int, object>(int.MaxValue, new FieldNode(field, ConfigObject, ID)));
                }
            }

            // Loop through nodes that should have a parent and build those relationships
            foreach (FieldNode fieldNode in collapseWithList)
            {
                OrderAttribute? orderAttribute = fieldNode.MainField.GetCustomAttributes(false).Where(a => a is OrderAttribute).FirstOrDefault() as OrderAttribute;
                if (orderAttribute is not null && orderAttribute.collapseWith is not null)
                {
                    // The CategoryField for the parent could either be in the drawList (if it is the root) or the collapseWithList (if there is multi-layered nesting)
                    // Create a union of the CategoryFields that already exist in the drawList and the collapseWithList, then search for the parent field.
                    // This looks incredibly gross but it's probably the cleanest way to do it without an extremely heavy refactor of the code above.
                    FieldNode? parentFieldNode = _drawList
                        .Where(kvp => kvp.Value is FieldNode)
                        .Select(kvp => kvp.Value as FieldNode)
                        .Union(collapseWithList)
                        .Where(categoryField => categoryField is not null && orderAttribute.collapseWith.Equals(categoryField.MainField.Name))
                        .FirstOrDefault();

                    if (parentFieldNode is not null)
                    {
                        parentFieldNode.CollapseControl = true;
                        fieldNode.Depth = parentFieldNode.Depth + 1;
                        parentFieldNode.AddChild(orderAttribute.pos, fieldNode);
                    }
                }
            }

            _drawList.Sort((x, y) => x.Key - y.Key);
        }

        private bool DrawPortableSection()
        {
            if (!AllowExport())
            {
                return false;
            }

            ImGuiHelper.DrawSeparator(2, 1);

            const float buttonWidth = 120;

            ImGui.BeginGroup();

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowContentRegionWidth() / 2f - buttonWidth - 5, ImGui.GetCursorPosY()));

            if (ImGui.Button("Export", new Vector2(120, 24)))
            {
                var exportString = ImportExportHelper.GenerateExportString(ConfigObject);
                ImGui.SetClipboardText(exportString);
            }

            ImGui.SameLine();

            if (ImGui.Button("Reset", new Vector2(120, 24)))
            {
                _nodeToReset = this;
                _nodeToResetName = Utils.UserFriendlyConfigName(ConfigObject.GetType().Name);
            }

            ImGui.EndGroup();

            return DrawResetModal();
        }

        public override void Save(string path)
        {
            string[] splits = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);
            string directory = path.Replace(splits.Last(), "");
            Directory.CreateDirectory(directory);

            string finalPath = path + ".json";

            File.WriteAllText(
                finalPath,
                JsonConvert.SerializeObject(
                    ConfigObject,
                    Formatting.Indented,
                    new JsonSerializerSettings { TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple, TypeNameHandling = TypeNameHandling.Objects }
                )
            );
        }

        public override void Load(string path)
        {

            FileInfo finalPath = new(path + ".json");

            if (!finalPath.Exists)
            {
                return;
            }

            // Use reflection to call the LoadForType method, this allows us to specify a type at runtime.
            // While in general use this is important as the conversion from the superclass 'PluginConfigObject' to a specific subclass (e.g. 'BlackMageHudConfig') would
            // be handled by Json.NET, when the plugin is reloaded with a different assembly (as is the case when using LivePluginLoader, or updating the plugin in-game)
            // it fails. In order to fix this we need to specify the specific subclass, in order to do this during runtime we must use reflection to set the generic.
            if (ConfigObject is PluginConfigObject)
            {
                MethodInfo? methodInfo = GetType().GetMethod("LoadForType");
                MethodInfo? function = methodInfo?.MakeGenericMethod(ConfigObject.GetType());
                ConfigObject = (PluginConfigObject)function?.Invoke(this, new object[] { finalPath.FullName })!;
            }
        }

        public T? LoadForType<T>(string path) where T : PluginConfigObject
        {
            FileInfo file = new(path);

            return JsonConvert.DeserializeObject<T>(File.ReadAllText(file.FullName));
        }

        public override void Reset()
        {
            ConfigObject = ConfigurationManager.GetDefaultConfigObjectForType(ConfigObject.GetType());
        }

        public override ConfigPageNode? GetOrAddConfig<T>() => this;
    }
}
