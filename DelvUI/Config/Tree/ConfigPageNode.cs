using Dalamud.Logging;
using DelvUI.Config.Attributes;
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

                    _nestedConfigPageNodes.Add(field.Name, configPageNode);
                }
            }
        }

        private string _importString = "";
        private string _exportString = "";

        public override string GetBase64String()
        {
            return ConfigObject.Portable ? ConfigurationManager.GenerateExportString(ConfigObject) : "";
        }

        public override void LoadBase64String(string[] importStrings)
        {
            // go through and check types
            // if type matches, load it
            foreach (string importString in importStrings)
            {
                Type? importedType = null;

                try
                {
                    // get type from json
                    string jsonString = ConfigurationManager.Base64DecodeAndDecompress(importString);

                    var typeString = (string?)JObject.Parse(jsonString)["$type"];
                    if (typeString != null)
                    {
                        importedType = Type.GetType(typeString);
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.Log($"Error parsing import string!\n{ex.StackTrace}");
                }

                // abort import if the import string is for the wrong type
                if (importedType != null && ConfigObject.GetType().FullName == importedType.FullName)
                {
                    // see comments on ConfigPageNode's Loadd
                    MethodInfo? methodInfo = typeof(ConfigurationManager)?.GetMethod("LoadImportString");
                    MethodInfo? function = methodInfo?.MakeGenericMethod(ConfigObject.GetType());
                    PluginConfigObject? importedConfigObject = (PluginConfigObject?)function?.Invoke(ConfigurationManager.Instance, new object[] { importString });

                    if (importedConfigObject != null)
                    {
                        ConfigObject = importedConfigObject;
                    }
                    else
                    {
                        PluginLog.Log($"Could not load from import string (of type {ConfigObject.GetType()})");
                    }
                }
            }
        }

        public override void Draw(ref bool changed) { DrawWithID(ref changed); }

        private void DrawWithID(ref bool changed, string? ID = null)
        {
            // Only do this stuff the first time the config page is loaded
            if (_drawList is null)
            {
                GenerateDrawList();
            }

            foreach (KeyValuePair<int, object> pair in _drawList!)
            {
                if (pair.Value is FieldNode categoryField)
                {
                    categoryField.Draw(ref changed);
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
                        DrawHelper.DrawImGuiSeparator(1, 1);
                    }
                    if (node._hasSpacing)
                    {
                        DrawHelper.DrawImGuiSpacing(1);
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

                // TODO allow the manual draw methods to take parameters
                bool? result = (bool?)method.Invoke(ConfigObject, null);
                changed |= (result.HasValue && result.Value);
            }

            // if the config object is not marked with [Portable(false)], or is marked with [Portable(true)],
            // draw the import/export UI
            if (ConfigObject.Portable)
            {
                DrawImportExportGeneralConfig();
            }
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

        private void DrawImportExportGeneralConfig()
        {
            DrawHelper.DrawImGuiSeparator(2, 1);

            uint maxLength = 40000;
            ImGui.BeginChild("importpane", new Vector2(0, ImGui.GetWindowHeight() / 6), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            {
                ImGui.Text("Import string:");
                ImGui.InputText("", ref _importString, maxLength);

                if (ImGui.Button("Import configuration"))
                {
                    // get type from json 
                    Type? importedType = null;

                    try
                    {
                        string jsonString = ConfigurationManager.Base64DecodeAndDecompress(_importString);

                        var typeString = (string?)JObject.Parse(jsonString)["$type"];
                        if (typeString != null)
                        {
                            importedType = Type.GetType(typeString);
                        }
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Log($"Error parsing import string!\n{ex.StackTrace}");
                    }

                    // abort import if the import string is for the wrong type
                    if (importedType != null && ConfigObject.GetType().FullName == importedType.FullName)
                    {
                        ConfigurationManager.LoadImportedConfiguration(_importString, this);
                    }
                    else
                    {
                        PluginLog.Log($"Could not convert {(importedType == null ? "null" : importedType)} to {ConfigObject.GetType()}! Aborting import.");
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button("Paste from clipboard"))
                {
                    try
                    {
                        _importString = ImGui.GetClipboardText();
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Log("Could not get clipboard text:\n" + ex.StackTrace);
                    }
                }
            }

            ImGui.EndChild();

            ImGui.BeginChild("exportpane", new Vector2(0, ImGui.GetWindowHeight() / 6), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            {
                ImGui.Text("Export string:");
                ImGui.InputText("", ref _exportString, maxLength, ImGuiInputTextFlags.ReadOnly);

                if (ImGui.Button("Export configuration"))
                {
                    _exportString = ConfigurationManager.GenerateExportString(ConfigObject);
                    PluginLog.Log($"Exported type {ConfigObject.GetType()}");
                }

                ImGui.SameLine();

                if (ImGui.Button("Copy to clipboard") && _exportString != "")
                {
                    try
                    {
                        ImGui.SetClipboardText(_exportString);
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Log("Could not set clipboard text:\n" + ex.StackTrace);
                    }
                }
            }

            ImGui.EndChild();
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

        public override ConfigPageNode? GetOrAddConfig<T>() => this;
    }
}
