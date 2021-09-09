using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface;
using Dalamud.Plugin;
using DelvUI.Config.Attributes;
using ImGuiNET;
using ImGuiScene;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DelvUI.Config.Tree
{
    public abstract class Node
    {
        public List<Node> children;

        public virtual void Save(string path)
        {
            foreach (Node child in children)
            {
                child.Save(ConfigurationManager.GetInstance().ConfigDirectory);
            }
        }

        public virtual void Load(string path)
        {
            foreach (Node child in children)
            {
                child.Load(ConfigurationManager.GetInstance().ConfigDirectory);
            }
        }

        public virtual string GetBase64String()
        {
            if (children == null)
            {
                return "";
            }

            string base64String = "";

            foreach (Node child in children)
            {
                string childString = child.GetBase64String();

                if (childString != "")
                {
                    base64String += "|" + childString;
                }
            }

            return base64String;
        }

        public virtual void LoadBase64String(string[] importStrings)
        {
            if (children == null)
            {
                return;
            }

            foreach (Node child in children)
            {
                child.LoadBase64String(importStrings);
            }
        }
    }

    public class BaseNode : Node
    {
        public new List<SectionNode> children;

        public BaseNode() { children = new List<SectionNode>(); }

        public override string GetBase64String()
        {
            if (children == null)
            {
                return "";
            }

            string base64String = "";

            foreach (Node child in children)
            {
                string childString = child.GetBase64String();

                if (childString != "")
                {
                    base64String += "|" + childString;
                }
            }

            return base64String;
        }

        public override void LoadBase64String(string[] importStrings)
        {
            if (children == null)
            {
                return;
            }

            foreach (Node child in children)
            {
                child.LoadBase64String(importStrings);
            }
        }

        private void ToggleJobPacks()
        {
            ConfigurationManager.GetInstance().ConfigurationWindow.IsVisible = !ConfigurationManager.GetInstance().ConfigurationWindow.IsVisible;
            ConfigurationManager.GetInstance().DrawConfigWindow = !ConfigurationManager.GetInstance().DrawConfigWindow;
        }

        public void Draw()
        {
            bool changed = false;

            ImGui.SetNextWindowSize(new Vector2(1050, 750), ImGuiCond.Appearing);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(10f / 255f, 10f / 255f, 10f / 255f, 0.95f));

            if (!ImGui.Begin("titlebarnew", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse))
            {
                return;
            }

            ImGui.PopStyleColor();
            ImGui.BeginGroup(); // Middle section

            {
                ImGui.BeginGroup(); // Left

                {
                    TextureWrap delvUiBanner = ConfigurationManager.GetInstance().BannerImage;

                    if (delvUiBanner != null)
                    {
                        ImGui.Image(delvUiBanner.ImGuiHandle, new Vector2(delvUiBanner.Width, delvUiBanner.Height));
                    }

                    ImGui.BeginChild("left pane", new Vector2(150, -ImGui.GetFrameHeightWithSpacing() - 32), true);

                    // if no section is selected, select the first
                    if (children.Any() && children.All(o => !o.Selected))
                    {
                        children[0].Selected = true;
                    }

                    foreach (SectionNode selectionNode in children)
                    {
                        if (ImGui.Selectable(selectionNode.Name, selectionNode.Selected))
                        {
                            selectionNode.Selected = true;

                            foreach (SectionNode otherNode in children.FindAll(x => x != selectionNode))
                            {
                                otherNode.Selected = false;
                            }
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndGroup(); // Left

                ImGui.SameLine();

                ImGui.BeginGroup(); // Right

                {
                    foreach (SectionNode selectionNode in children)
                    {
                        selectionNode.Draw(ref changed);
                    }
                }

                ImGui.EndGroup(); // Right
            }

            ImGui.EndGroup(); // Middle section

            ImGui.BeginGroup();

            if (ImGui.Button("Switch to General Configuration", new Vector2(ImGui.GetWindowContentRegionWidth(), 0)))
            {
                ToggleJobPacks();
            }

            ImGui.Separator();

            ImGui.EndGroup();

            ImGui.BeginGroup();

            if (ImGui.Button(ConfigurationManager.GetInstance().ConfigurationWindow.GetToggleHudState() ? "Show HUD" : "Hide HUD", new Vector2(ImGui.GetWindowWidth() / 7, 0)))
            {
                ConfigurationManager.GetInstance().ConfigurationWindow.ToggleHud();
            }

            ImGui.SameLine();

            if (ImGui.Button("Reset to Default", new Vector2(ImGui.GetWindowWidth() / 7, 0)))

            {
                // save the old configuration window for use in the new ConfigurationManager
                ConfigurationWindow configurationWindow = ConfigurationManager.GetInstance().ConfigurationWindow;
                // make a new configuration from defaults
                ConfigurationManager.Initialize(true);
                ConfigurationManager.GetInstance().ConfigurationWindow = configurationWindow;
                // save the defaults to file
                ConfigurationManager.GetInstance().SaveConfigurations();
                // prevent the config window from closing
                ConfigurationManager.GetInstance().DrawConfigWindow = true;
            }

            ImGui.SameLine();

            ImGui.BeginChild("versionleft", new Vector2(ImGui.GetWindowWidth() / 7 + 10, 0));
            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("versioncenter", new Vector2(ImGui.GetWindowWidth() / 7 + 85, 0));
            ImGui.Text($"v{Plugin.Version}");
            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(114f / 255f, 137f / 255f, 218f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(124f / 255f, 147f / 255f, 228f / 255f, 1f));

            if (ImGui.Button("Help!", new Vector2(ImGui.GetWindowWidth() / 7, 0)))
            {
                Process.Start("https://discord.gg/delvui");
            }

            ImGui.PopStyleColor();
            ImGui.PopStyleColor();

            ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(255f / 255f, 94f / 255f, 91f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(255f / 255f, 104f / 255f, 101f / 255f, 1f));

            if (ImGui.Button("Donate!", new Vector2(ImGui.GetWindowWidth() / 7, 0)))
            {
                Process.Start("https://ko-fi.com/DelvUI");
            }

            ImGui.PopStyleColor();
            ImGui.PopStyleColor();

            ImGui.EndGroup();

            ImGui.End();

            if (changed)
            {
                ConfigurationManager.GetInstance().SaveConfigurations();
            }
        }

        public override void Load(string path)
        {
            foreach (SectionNode child in children)
            {
                child.Load(path);
            }
        }

        public override void Save(string path)
        {
            foreach (SectionNode child in children)
            {
                child.Save(path);
            }
        }

        public ConfigPageNode GetOrAddConfig(PluginConfigObject configObject)
        {
            object[] attributes = configObject.GetType().GetCustomAttributes(false);

            foreach (object attribute in attributes)
            {
                if (attribute is SectionAttribute sectionAttribute)
                {
                    foreach (SectionNode sectionNode in children)
                    {
                        if (sectionNode.Name == sectionAttribute.SectionName)
                        {
                            return sectionNode.GetOrAddConfig(configObject);
                        }
                    }

                    SectionNode newNode = new();
                    newNode.Name = sectionAttribute.SectionName;
                    children.Add(newNode);

                    return newNode.GetOrAddConfig(configObject);
                }
            }

            throw new ArgumentException("The provided configuration object does not specify a section");
        }
    }

    public class SectionNode : Node
    {
        public new List<SubSectionNode> children;

        public bool Selected;
        public string Name;

        public SectionNode() { children = new List<SubSectionNode>(); }

        public override string GetBase64String()
        {
            if (children == null)
            {
                return "";
            }

            string base64String = "";

            foreach (Node child in children)
            {
                string childString = child.GetBase64String();

                if (childString != "")
                {
                    base64String += "|" + childString;
                }
            }

            return base64String;
        }

        public override void LoadBase64String(string[] importStrings)
        {
            if (children == null)
            {
                return;
            }

            foreach (Node child in children)
            {
                child.LoadBase64String(importStrings);
            }
        }

        public void Draw(ref bool changed)
        {
            if (!Selected)
            {
                return;
            }

            ImGui.BeginChild(
                "item view",
                new Vector2(0, -ImGui.GetFrameHeightWithSpacing() - 32),
                false,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse
            ); // Leave room for 1 line below us

            {
                if (ImGui.BeginTabBar("##Tabs", ImGuiTabBarFlags.None))
                {
                    foreach (SubSectionNode subSectionNode in children)
                    {
                        if (!ImGui.BeginTabItem(subSectionNode.Name))
                        {
                            continue;
                        }

                        ImGui.BeginChild("subconfig value", new Vector2(0, 0), true);
                        subSectionNode.Draw(ref changed);
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                    // close button
                    Vector2 pos = ImGui.GetCursorPos();
                    ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 20, 0));
                    ImGui.PushFont(UiBuilder.IconFont);

                    if (ImGui.Button(FontAwesomeIcon.Times.ToIconString()))
                    {
                        ConfigurationManager.GetInstance().DrawConfigWindow = !ConfigurationManager.GetInstance().DrawConfigWindow;
                    }

                    ImGui.PopFont();
                    ImGui.SetCursorPos(pos);
                }
            }

            ImGui.EndChild();
        }

        public override void Save(string path)
        {
            foreach (SubSectionNode child in children)
            {
                child.Save(Path.Combine(path, Name));
            }
        }

        public override void Load(string path)
        {
            foreach (SubSectionNode child in children)
            {
                child.Load(Path.Combine(path, Name));
            }
        }

        public ConfigPageNode GetOrAddConfig(PluginConfigObject configObject)
        {
            object[] attributes = configObject.GetType().GetCustomAttributes(false);

            foreach (object attribute in attributes)
            {
                if (attribute is SubSectionAttribute subSectionAttribute)
                {
                    foreach (SubSectionNode subSectionNode in children)
                    {
                        if (subSectionNode.Name == subSectionAttribute.SubSectionName)
                        {
                            return subSectionNode.GetOrAddConfig(configObject);
                        }
                    }

                    if (subSectionAttribute.Depth == 0)
                    {
                        NestedSubSectionNode newNode = new();
                        newNode.Name = subSectionAttribute.SubSectionName;
                        newNode.Depth = 0;
                        children.Add(newNode);

                        return newNode.GetOrAddConfig(configObject);
                    }
                }
            }

            throw new ArgumentException("The provided configuration object does not specify a sub-section");
        }
    }

    public abstract class SubSectionNode : Node
    {
        public string Name;
        public int Depth;

        public abstract void Draw(ref bool changed);

        public abstract ConfigPageNode GetOrAddConfig(PluginConfigObject configObject);
    }

    public class NestedSubSectionNode : SubSectionNode
    {
        public new List<SubSectionNode> children;

        public NestedSubSectionNode() { children = new List<SubSectionNode>(); }

        public override string GetBase64String()
        {
            if (children == null)
            {
                return "";
            }

            string base64String = "";

            foreach (Node child in children)
            {
                string childString = child.GetBase64String();

                if (childString != "")
                {
                    base64String += "|" + childString;
                }
            }

            return base64String;
        }

        public override void LoadBase64String(string[] importStrings)
        {
            if (children == null)
            {
                return;
            }

            foreach (Node child in children)
            {
                child.LoadBase64String(importStrings);
            }
        }

        public override void Draw(ref bool changed)
        {
            ImGui.BeginChild("item" + Depth + " view", new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us

            {
                if (ImGui.BeginTabBar("##tabs" + Depth, ImGuiTabBarFlags.None))
                {
                    foreach (SubSectionNode subSectionNode in children)
                    {
                        if (subSectionNode is NestedSubSectionNode)
                        {
                            if (!ImGui.BeginTabItem(subSectionNode.Name))
                            {
                                continue;
                            }

                            ImGui.BeginChild("subconfig" + Depth + " value", new Vector2(0, 0), true);
                            subSectionNode.Draw(ref changed);
                            ImGui.EndChild();
                            ImGui.EndTabItem();
                        }
                        else
                        {
                            subSectionNode.Draw(ref changed);
                        }
                    }

                    ImGui.EndTabBar();
                }
            }

            ImGui.EndChild();
        }

        public override void Save(string path)
        {
            foreach (SubSectionNode child in children)
            {
                child.Save(Path.Combine(path, Name));
            }
        }

        public override void Load(string path)
        {
            foreach (SubSectionNode child in children)
            {
                child.Load(Path.Combine(path, Name));
            }
        }

        public override ConfigPageNode GetOrAddConfig(PluginConfigObject configObject)
        {
            object[] attributes = configObject.GetType().GetCustomAttributes(false);

            foreach (object attribute in attributes)
            {
                if (attribute is SubSectionAttribute subSectionAttribute)
                {
                    if (subSectionAttribute.Depth != Depth + 1)
                    {
                        continue;
                    }

                    foreach (SubSectionNode subSectionNode in children)
                    {
                        if (subSectionNode.Name == subSectionAttribute.SubSectionName)
                        {
                            return subSectionNode.GetOrAddConfig(configObject);
                        }
                    }

                    NestedSubSectionNode nestedSubSectionNode = new();
                    nestedSubSectionNode.Name = subSectionAttribute.SubSectionName;
                    nestedSubSectionNode.Depth = Depth + 1;
                    children.Add(nestedSubSectionNode);

                    return nestedSubSectionNode.GetOrAddConfig(configObject);
                }
            }

            foreach (SubSectionNode subSectionNode in children)
            {
                if (subSectionNode.Name == configObject.GetType().FullName && subSectionNode is ConfigPageNode node)
                {
                    return node;
                }
            }

            ConfigPageNode configPageNode = new();
            configPageNode.ConfigObject = configObject;
            configPageNode.Name = configObject.GetType().FullName;
            children.Add(configPageNode);

            return configPageNode;
        }
    }

    public class ConfigPageNode : SubSectionNode
    {
        public PluginConfigObject ConfigObject;
        private string _importString = "";
        private string _exportString = "";

        public override string GetBase64String()
        {
            PortableAttribute portableAttribute = (PortableAttribute)ConfigObject.GetType().GetCustomAttribute(typeof(PortableAttribute), false);

            return portableAttribute == null || portableAttribute.portable ? ConfigurationManager.GenerateExportString(ConfigObject) : "";
        }

        public override void LoadBase64String(string[] importStrings)
        {
            // go through and check types
            // if type matches, load it
            foreach (string importString in importStrings)
            {
                Type importedType = null;
                try
                {
                    // get type from json
                    string jsonString = ConfigurationManager.Base64DecodeAndDecompress(importString);
                    importedType = Type.GetType((string)JObject.Parse(jsonString)["$type"]);
                }
                catch (Exception ex)
                {
                    PluginLog.Log($"Error parsing import string!\n{ex.StackTrace}");
                }
                // abort import if the import string is for the wrong type
                if (importedType != null && ConfigObject.GetType().FullName == importedType.FullName)
                {
                    // see comments on ConfigPageNode's Load
                    MethodInfo methodInfo = typeof(ConfigurationManager).GetMethod("LoadImportString");
                    MethodInfo function = methodInfo.MakeGenericMethod(ConfigObject.GetType());
                    PluginConfigObject importedConfigObject = (PluginConfigObject)function.Invoke(ConfigurationManager.GetInstance(), new object[] { importString });

                    if (importedConfigObject != null)
                    {
                        PluginLog.Log($"Importing {importedConfigObject.GetType()}");
                        ConfigObject = importedConfigObject;
                        ConfigurationManager.GetInstance().SaveConfigurations();
                    }
                    else
                    {
                        PluginLog.Log($"Could not load from import string (of type {importedConfigObject.GetType()})");
                    }
                }
            }
        }

        public override void Draw(ref bool changed)
        {
            FieldInfo[] fields = ConfigObject.GetType().GetFields();
            List<KeyValuePair<int, CategoryField>> drawList = new();
            List<FieldInfo> collapseWithList = new();

            foreach (FieldInfo field in fields)
            {
                bool hasOrderAttribute = false;

                foreach (object attribute in field.GetCustomAttributes(true))
                {
                    if (attribute is OrderAttribute orderAttribute)
                    {
                        drawList.Add(new KeyValuePair<int, CategoryField>(orderAttribute.pos, new CategoryField(field, ConfigObject)));
                        hasOrderAttribute = true;
                    }
                    else if (attribute is CollapseControlAttribute collapseControlAtrribute)
                    {
                        CategoryField categoryField = new(field, ConfigObject);
                        categoryField.CategoryId = collapseControlAtrribute.id;
                        drawList.Add(new KeyValuePair<int, CategoryField>(collapseControlAtrribute.pos, categoryField));
                        hasOrderAttribute = true;
                    }
                    else if (attribute is CollapseWithAttribute collapseWithAttribute)
                    {
                        collapseWithList.Add(field);
                        hasOrderAttribute = true;
                    }
                }

                if (!hasOrderAttribute)
                {
                    drawList.Add(new KeyValuePair<int, CategoryField>(int.MaxValue, new CategoryField(field, ConfigObject)));
                }
            }

            foreach (FieldInfo field in collapseWithList)
            {
                foreach (object attribute in field.GetCustomAttributes(true))
                {
                    if (attribute is CollapseWithAttribute collapseWithAttribute)
                    {
                        foreach (KeyValuePair<int, CategoryField> categoryField in drawList)
                        {
                            if (categoryField.Value.CategoryId == collapseWithAttribute.id)
                            {
                                categoryField.Value.AddChild(collapseWithAttribute.pos, field);

                                break;
                            }
                        }

                        break;
                    }
                }
            }

            drawList.Sort((x, y) => x.Key - y.Key);

            foreach (KeyValuePair<int, CategoryField> pair in drawList)
            {
                pair.Value.Draw(ref changed);
            }

            // if the ConfigPageNode requires any manual drawing (i.e. not dictated by attributes), draw it now
            foreach (MethodInfo method in ConfigObject.GetType().GetMethods())
            {
                if (!method.GetCustomAttributes(typeof(ManualDrawAttribute), false).Any())
                {
                    continue;
                }

                // TODO allow the manual draw methods to take parameters
                method.Invoke(ConfigObject, null);
            }

            // if the config object is not marked with [Portable(false)], or is marked with [Portable(true)],
            // draw the import/export UI
            PortableAttribute portableAttribute = (PortableAttribute)ConfigObject.GetType().GetCustomAttribute(typeof(PortableAttribute), false);

            if (portableAttribute == null || portableAttribute.portable)
            {
                DrawImportExportGeneralConfig();
            }
        }

        private void DrawImportExportGeneralConfig()
        {
            uint maxLength = 40000;
            ImGui.BeginChild("importpane", new Vector2(0, ImGui.GetWindowHeight() / 6), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            {
                ImGui.Text("Import string:");
                ImGui.InputText("", ref _importString, maxLength);

                if (ImGui.Button("Import configuration"))
                {
                    // get type from json
                    Type importedType = null;
                    try
                    {
                        string jsonString = ConfigurationManager.Base64DecodeAndDecompress(_importString);
                        importedType = Type.GetType((string)JObject.Parse(jsonString)["$type"]);
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Log($"Error parsing import string!\n{ex.StackTrace}");
                    }

                    // abort import if the import string is for the wrong type
                    if (importedType != null && ConfigObject.GetType().FullName == importedType.FullName)
                    {
                        // see comments on ConfigPageNode's Load
                        MethodInfo methodInfo = typeof(ConfigurationManager).GetMethod("LoadImportString");
                        MethodInfo function = methodInfo.MakeGenericMethod(ConfigObject.GetType());
                        PluginConfigObject importedConfigObject = (PluginConfigObject)function.Invoke(ConfigurationManager.GetInstance(), new object[] { _importString });

                        if (importedConfigObject != null)
                        {
                            PluginLog.Log($"Importing {importedConfigObject.GetType()}");
                            ConfigObject = importedConfigObject;
                            ConfigurationManager.GetInstance().SaveConfigurations();
                        }
                        else
                        {
                            PluginLog.Log($"Could not load from import string (of type {importedConfigObject.GetType()})");
                        }
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
            Directory.CreateDirectory(path);
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
            if (ConfigObject.GetType().BaseType == typeof(PluginConfigObject))
            {
                MethodInfo methodInfo = GetType().GetMethod("LoadForType");
                MethodInfo function = methodInfo.MakeGenericMethod(ConfigObject.GetType());
                ConfigObject = (PluginConfigObject)function.Invoke(this, new object[] { finalPath.FullName });
            }
        }

        public T LoadForType<T>(string path) where T : PluginConfigObject
        {
            FileInfo file = new(path);

            return JsonConvert.DeserializeObject<T>(File.ReadAllText(file.FullName));
        }

        public override ConfigPageNode GetOrAddConfig(PluginConfigObject configObject) => this;
    }

    public class CategoryField
    {
        public SortedDictionary<int, FieldInfo> Children;
        public FieldInfo MainField;
        public PluginConfigObject ConfigObject;
        public int CategoryId;

        public CategoryField(FieldInfo mainField, PluginConfigObject configObject)
        {
            MainField = mainField;
            ConfigObject = configObject;
            CategoryId = -1;
            Children = new SortedDictionary<int, FieldInfo>();
        }

        public void AddChild(int position, FieldInfo field) { Children.Add(position, field); }

        public void Draw(ref bool changed)
        {
            Draw(ref changed, MainField, 0);

            if (CategoryId != -1 && (bool)MainField.GetValue(ConfigObject))
            {
                ImGui.BeginGroup();
                ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(0, 2));

                foreach (FieldInfo child in Children.Values)
                {
                    Draw(ref changed, child, 4);
                }

                ImGui.EndGroup();

                ImGui.GetWindowDrawList()
                     .AddRect(
                         ImGui.GetItemRectMin() + new Vector2(0, -2),
                         ImGui.GetItemRectMax() + new Vector2(ImGui.GetContentRegionAvail().X - ImGui.GetItemRectMax().X + ImGui.GetItemRectMin().X - 4, 4),
                         0xFF4A4141
                     );

                ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(0, 2));
            }
        }

        public void Draw(ref bool changed, FieldInfo field, int xOffset)
        {
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(xOffset, 0));
            object fieldVal = field.GetValue(ConfigObject);

            foreach (object attribute in field.GetCustomAttributes(true))
            {
                if (attribute is CheckboxAttribute checkboxAttribute)
                {
                    bool boolVal = (bool)fieldVal;

                    if (ImGui.Checkbox(checkboxAttribute.friendlyName, ref boolVal))
                    {
                        field.SetValue(ConfigObject, boolVal);
                        changed = true;
                    }
                }
                else if (attribute is DragFloatAttribute dragFloatAttribute)
                {
                    float floatVal = (float)fieldVal;

                    if (ImGui.DragFloat(dragFloatAttribute.friendlyName, ref floatVal, dragFloatAttribute.velocity, dragFloatAttribute.min, dragFloatAttribute.max))
                    {
                        field.SetValue(ConfigObject, floatVal);
                        changed = true;
                    }
                }
                else if (attribute is DragIntAttribute dragIntAttribute)
                {
                    int intVal = (int)fieldVal;

                    if (ImGui.DragInt(dragIntAttribute.friendlyName, ref intVal, dragIntAttribute.velocity, dragIntAttribute.min, dragIntAttribute.max))
                    {
                        field.SetValue(ConfigObject, intVal);
                        changed = true;
                    }
                }
                else if (attribute is DragFloat2Attribute dragFloat2Attribute)
                {
                    Vector2 floatVal = (Vector2)fieldVal;

                    if (ImGui.DragFloat2(dragFloat2Attribute.friendlyName, ref floatVal, dragFloat2Attribute.velocity, dragFloat2Attribute.min, dragFloat2Attribute.max))
                    {
                        field.SetValue(ConfigObject, floatVal);
                        changed = true;
                    }
                }
                else if (attribute is DragInt2Attribute dragInt2Attribute)
                {
                    Vector2 intVal = (Vector2)fieldVal;

                    if (ImGui.DragFloat2(dragInt2Attribute.friendlyName, ref intVal, dragInt2Attribute.velocity, dragInt2Attribute.min, dragInt2Attribute.max))
                    {
                        field.SetValue(ConfigObject, intVal);
                        changed = true;
                    }
                }
                else if (attribute is InputTextAttribute inputTextAttribute)
                {
                    string stringVal = (string)fieldVal;

                    if (ImGui.InputText(inputTextAttribute.friendlyName, ref stringVal, inputTextAttribute.maxLength))
                    {
                        field.SetValue(ConfigObject, stringVal);
                        changed = true;
                    }
                }
                else if (attribute is ColorEdit4Attribute colorEdit4Attribute)
                {
                    PluginConfigColor colorVal = (PluginConfigColor)fieldVal;
                    Vector4 vector = colorVal.Vector;

                    if (ImGui.ColorEdit4(colorEdit4Attribute.friendlyName, ref vector))
                    {
                        colorVal.Vector = vector;
                        field.SetValue(ConfigObject, colorVal);
                        changed = true;
                    }
                }
            }
        }
    }
}
