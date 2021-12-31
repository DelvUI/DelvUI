using Dalamud.Logging;
using DelvUI.Config.Attributes;
using DelvUI.Config.Profiles;
using DelvUI.Helpers;
using ImGuiNET;
using Newtonsoft.Json;
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
        private List<ConfigNode>? _drawList = null;
        private Dictionary<string, ConfigPageNode> _nestedConfigPageNodes = null!;

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
            if (!AllowShare())
            {
                return null;
            }

            return ImportExportHelper.GenerateExportString(ConfigObject);
        }

        protected override bool AllowExport()
        {
            return ConfigObject.Exportable;
        }

        protected override bool AllowShare()
        {
            return ConfigObject.Shareable;
        }

        protected override bool AllowReset()
        {
            return ConfigObject.Resettable;
        }

        public override bool Draw(ref bool changed) { return DrawWithID(ref changed); }

        private bool DrawWithID(ref bool changed, string? ID = null)
        {
            bool didReset = false;

            // Only do this stuff the first time the config page is loaded
            if (_drawList is null)
            {
                _drawList = GenerateDrawList();
            }

            if (_drawList is not null)
            {
                foreach (var fieldNode in _drawList)
                {
                    didReset |= fieldNode.Draw(ref changed);
                }
            }

            didReset |= DrawPortableSection();

            ImGui.NewLine(); // fixes some long pages getting cut off

            return didReset;
        }

        private List<ConfigNode> GenerateDrawList(string? ID = null)
        {
            Dictionary<string, ConfigNode> fieldMap = new Dictionary<string, ConfigNode>();

            FieldInfo[] fields = ConfigObject.GetType().GetFields();
            foreach (var field in fields)
            {
                if (ConfigObject.DisableParentSettings != null && ConfigObject.DisableParentSettings.Contains(field.Name))
                {
                    continue;
                }

                foreach (object attribute in field.GetCustomAttributes(true))
                {
                    if (attribute is NestedConfigAttribute nestedConfigAttribute && _nestedConfigPageNodes.TryGetValue(field.Name, out ConfigPageNode? node))
                    {
                        var newNodes = node.GenerateDrawList(node.Name);
                        foreach (var newNode in newNodes)
                        {
                            newNode.Position = nestedConfigAttribute.pos;
                            newNode.Separator = nestedConfigAttribute.separator;
                            newNode.Spacing = nestedConfigAttribute.spacing;
                            newNode.ParentName = nestedConfigAttribute.collapseWith;
                            newNode.Nest = nestedConfigAttribute.nest;
                            newNode.CollapsingHeader = nestedConfigAttribute.collapsingHeader;
                            fieldMap.Add($"{node.Name}_{newNode.Name}", newNode);
                        }
                    }
                    else if (attribute is OrderAttribute orderAttribute)
                    {
                        var fieldNode = new FieldNode(field, ConfigObject, ID);
                        fieldNode.Position = orderAttribute.pos;
                        fieldNode.ParentName = orderAttribute.collapseWith;
                        fieldMap.Add(field.Name, fieldNode);
                    }
                }
            }

            var manualDrawMethods = ConfigObject.GetType().GetMethods().Where(m => Attribute.IsDefined(m, typeof(ManualDrawAttribute), false));
            foreach (var method in manualDrawMethods)
            {
                string id = $"ManualDraw##{method.GetHashCode()}";
                fieldMap.Add(id, new ManualDrawNode(method, ConfigObject, id));
            }

            foreach (var configNode in fieldMap.Values)
            {
                if (configNode.ParentName is not null &&
                    fieldMap.TryGetValue(configNode.ParentName, out ConfigNode? parentNode))
                {
                    if (!ConfigObject.Disableable &&
                        parentNode.Name.Equals("Enabled") &&
                        parentNode.ID is null)
                    {
                        continue;
                    }

                    if (parentNode is FieldNode parentFieldNode)
                    {
                        parentFieldNode.CollapseControl = true;
                        parentFieldNode.AddChild(configNode.Position, configNode);
                    }
                }
            }

            var fieldNodes = fieldMap.Values.ToList();
            fieldNodes.RemoveAll(f => f.IsChild);
            fieldNodes.Sort((x, y) => x.Position - y.Position);
            return fieldNodes;
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

            ImGui.NewLine();
            ImGui.EndGroup();

            return DrawResetModal();
        }

        public override void Save(string path)
        {
            string[] splits = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);
            string directory = path.Replace(splits.Last(), "");
            Directory.CreateDirectory(directory);

            string finalPath = path + ".json";

            try
            {
                File.WriteAllText(
                    finalPath,
                    JsonConvert.SerializeObject(
                        ConfigObject,
                        Formatting.Indented,
                        new JsonSerializerSettings { TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple, TypeNameHandling = TypeNameHandling.Objects }
                    )
                );
            }
            catch (Exception e)
            {
                PluginLog.Error("Error when saving config object: " + e.Message);
            }
        }

        public override void Load(string path, string currentVersion, string? previousVersion = null)
        {
            if (ConfigObject is not PluginConfigObject) { return; }

            FileInfo finalPath = new(path + ".json");

            // Use reflection to call the LoadForType method, this allows us to specify a type at runtime.
            // While in general use this is important as the conversion from the superclass 'PluginConfigObject' to a specific subclass (e.g. 'BlackMageHudConfig') would
            // be handled by Json.NET, when the plugin is reloaded with a different assembly (as is the case when using LivePluginLoader, or updating the plugin in-game)
            // it fails. In order to fix this we need to specify the specific subclass, in order to do this during runtime we must use reflection to set the generic.
            MethodInfo? methodInfo = ConfigObject.GetType().GetMethod("Load");
            MethodInfo? function = methodInfo?.MakeGenericMethod(ConfigObject.GetType());

            object?[] args = new object?[] { finalPath, currentVersion, previousVersion };
            PluginConfigObject? config = (PluginConfigObject?)function?.Invoke(ConfigObject, args);

            ConfigObject = config ?? ConfigObject;
        }

        public override void Reset()
        {
            ConfigObject = ConfigurationManager.GetDefaultConfigObjectForType(ConfigObject.GetType());
        }

        public override ConfigPageNode? GetOrAddConfig<T>() => this;
    }
}