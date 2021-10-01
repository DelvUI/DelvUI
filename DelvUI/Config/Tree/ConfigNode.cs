using Dalamud.Interface;
using Dalamud.Logging;
using DelvUI.Config.Attributes;
using ImGuiNET;
using ImGuiScene;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace DelvUI.Config.Tree
{
    public abstract class Node
    {
        public List<Node> children = null!;

        public virtual void Save(string path)
        {
            foreach (Node child in children)
            {
                child.Save(ConfigurationManager.Instance.ConfigDirectory);
            }
        }

        public virtual void Load(string path)
        {
            foreach (Node child in children)
            {
                child.Load(ConfigurationManager.Instance.ConfigDirectory);
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

        public static void Separator(int topSpacing, int bottomSpacing)
        {
            Spacing(topSpacing);
            ImGui.Separator();
            Spacing(bottomSpacing);
        }

        public static void Spacing(int spacingSize)
        {
            for (int i = 0; i < spacingSize; i++)
            {
                ImGui.NewLine();
            }
        }
    }
    public class BaseNode : Node
    {
        public new List<SectionNode> children;
        public Dictionary<Type, ConfigPageNode> configPageNodesMap;

        public BaseNode()
        {
            children = new List<SectionNode>();
            configPageNodesMap = new Dictionary<Type, ConfigPageNode>();
        }

        public T? GetConfigObject<T>() where T : PluginConfigObject
        {
            var pageNode = GetConfigPageNode<T>();

            return pageNode != null ? (T)pageNode.ConfigObject : null;
        }

        public ConfigPageNode? GetConfigPageNode<T>() where T : PluginConfigObject
        {
            if (configPageNodesMap.TryGetValue(typeof(T), out var node))
            {
                return node;
            }

            var configPageNode = GetOrAddConfig<T>();

            if (configPageNode != null && configPageNode.ConfigObject != null)
            {
                configPageNodesMap.Add(typeof(T), configPageNode);

                return configPageNode;
            }

            return null;
        }

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

        private void PushStyles()
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(46f / 255f, 45f / 255f, 46f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, .2f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, .2f));

            ImGui.PushStyleColor(ImGuiCol.Separator, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, .4f));

            ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, new Vector4(20f / 255f, 21f / 255f, 20f / 255f, .7f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, .7f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, .7f));
            ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, .7f));

            ImGui.PushStyleColor(ImGuiCol.Tab, new Vector4(46f / 255f, 45f / 255f, 46f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.TabActive, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, .7f));
            ImGui.PushStyleColor(ImGuiCol.TabHovered, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, .2f));
            ImGui.PushStyleColor(ImGuiCol.TabUnfocused, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, .2f));

            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, 1f));

            ImGui.PushStyleColor(ImGuiCol.TableBorderStrong, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.TableBorderLight, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, .4f));
            ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, .2f));

            ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 1); //Scrollbar Radius
            ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 1); //Tabs Radius Radius
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 1); //Intractable Elements Radius
            ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 1); //Gradable Elements Radius
            ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 1); //Popup Radius
            ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 10); //Popup Radius
        }

        private void PopStyles()
        {
            ImGui.PopStyleColor(17);
            ImGui.PopStyleVar(6);
        }

        public void Draw()
        {
            bool changed = false;

            ImGui.SetNextWindowSize(new Vector2(1050, 750), ImGuiCond.Appearing);
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.BorderShadow, new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(20f / 255f, 21f / 255f, 20f / 255f, 1f));

            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 1);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);

            if (!ImGui.Begin("titlebarnew", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.End();
                return;
            }

            ImGui.PopStyleColor(3);
            ImGui.PopStyleVar(3);
            PushStyles();


            ImGui.BeginGroup(); // Middle section

            {
                ImGui.BeginGroup(); // Left

                {
                    TextureWrap? delvUiBanner = ConfigurationManager.Instance.BannerImage;

                    if (delvUiBanner != null)
                    {
                        ImGui.Image(delvUiBanner.ImGuiHandle, new Vector2(delvUiBanner.Width, delvUiBanner.Height));
                    }

                    ImGui.BeginChild("left pane", new Vector2(150, -ImGui.GetFrameHeightWithSpacing() - 15), true);

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
            ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);
            ImGui.BeginChild("buttons", new Vector2(0, 0), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
            ImGui.PopStyleColor();

            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
            if (ImGui.Button((ConfigurationManager.Instance.ShowHUD ? "Hide" : "Show") + " HUD", new Vector2(ImGui.GetWindowWidth() / 7, 0)))
            {
                ConfigurationManager.Instance.ShowHUD = !ConfigurationManager.Instance.ShowHUD;
            }
            ImGui.PopStyleVar();
            ImGui.SameLine();
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);

            if (ImGui.Button((ConfigurationManager.Instance.LockHUD ? "Unlock" : "Lock") + " HUD", new Vector2(ImGui.GetWindowWidth() / 7, 0)))
            {
                ConfigurationManager.Instance.LockHUD = !ConfigurationManager.Instance.LockHUD;
            }
            ImGui.PopStyleVar();

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);

            if (ImGui.Button($"v{Plugin.Version}", new Vector2(ImGui.GetWindowWidth() / 7 * 3 - 50, 0)))
            { }

            ImGui.PopStyleColor(3);

            ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(114f / 255f, 137f / 255f, 218f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(114f / 255f, 137f / 255f, 218f / 255f, .85f));

            if (ImGui.Button("Help!", new Vector2(ImGui.GetWindowWidth() / 7, 0)))
            {
                Process.Start("https://discord.gg/delvui");
            }

            ImGui.PopStyleColor(2);

            ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(255f / 255f, 94f / 255f, 91f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(255f / 255f, 94f / 255f, 91f / 255f, .85f));

            if (ImGui.Button("Donate!", new Vector2(ImGui.GetWindowWidth() / 7, 0)))
            {
                Process.Start("https://ko-fi.com/DelvUI");
            }

            ImGui.PopStyleColor(2);
            ImGui.EndChild();
            ImGui.EndGroup();
            PopStyles();
            ImGui.End();

            if (changed)
            {
                ConfigurationManager.Instance.SaveConfigurations();
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

        public ConfigPageNode? GetOrAddConfig<T>() where T : PluginConfigObject
        {
            object[] attributes = typeof(T).GetCustomAttributes(true);

            foreach (object attribute in attributes)
            {
                if (attribute is SectionAttribute sectionAttribute)
                {
                    foreach (SectionNode sectionNode in children)
                    {
                        if (sectionNode.Name == sectionAttribute.SectionName)
                        {
                            return sectionNode.GetOrAddConfig<T>();
                        }
                    }

                    SectionNode newNode = new();
                    newNode.Name = sectionAttribute.SectionName;
                    children.Add(newNode);

                    return newNode.GetOrAddConfig<T>();
                }
            }

            throw new ArgumentException("The provided configuration object does not specify a section");
        }
    }

    public class SectionNode : Node
    {
        public new List<SubSectionNode> children;

        public bool Selected;
        public string Name = null!;

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
                new Vector2(0, -ImGui.GetFrameHeightWithSpacing() - 15),
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
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, 1f));
                    if (ImGui.Button(FontAwesomeIcon.Times.ToIconString(), new Vector2(20, 20)))
                    {
                        ConfigurationManager.Instance.DrawConfigWindow = !ConfigurationManager.Instance.DrawConfigWindow;
                    }
                    ImGui.PopStyleColor();
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

        public ConfigPageNode? GetOrAddConfig<T>() where T : PluginConfigObject
        {
            object[] attributes = typeof(T).GetCustomAttributes(true);

            foreach (object attribute in attributes)
            {
                if (attribute is SubSectionAttribute subSectionAttribute)
                {
                    foreach (SubSectionNode subSectionNode in children)
                    {
                        if (subSectionNode.Name == subSectionAttribute.SubSectionName)
                        {
                            return subSectionNode.GetOrAddConfig<T>();
                        }
                    }

                    if (subSectionAttribute.Depth == 0)
                    {
                        NestedSubSectionNode newNode = new();
                        newNode.Name = subSectionAttribute.SubSectionName;
                        newNode.Depth = 0;
                        children.Add(newNode);

                        return newNode.GetOrAddConfig<T>();
                    }
                }
            }

            throw new ArgumentException("The provided configuration object does not specify a sub-section");
        }
    }

    public abstract class SubSectionNode : Node
    {
        public string Name = null!;
        public int Depth;

        public abstract void Draw(ref bool changed);

        public abstract ConfigPageNode? GetOrAddConfig<T>() where T : PluginConfigObject;
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
            if (children.Count > 1)
            {
                ImGui.BeginChild(
                    "item" + Depth + " view",
                    new Vector2(0, ImGui.GetWindowHeight() - 22),
                    false,
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse
                ); // Leave room for 1 line below us

                if (ImGui.BeginTabBar("##tabs" + Depth, ImGuiTabBarFlags.None))
                {
                    DrawSubConfig(ref changed);
                }

                ImGui.EndTabBar();

                ImGui.EndChild();
            }
            else
            {
                ImGui.BeginChild("item" + Depth + " view", new Vector2(0, ImGui.GetWindowHeight() - 20)); // Leave room for 1 line below us

                DrawSubConfig(ref changed);

                ImGui.EndChild();
            }
        }

        public void DrawSubConfig(ref bool changed)
        {
            foreach (SubSectionNode subSectionNode in children)
            {
                if (subSectionNode is NestedSubSectionNode)
                {
                    if (!ImGui.BeginTabItem(subSectionNode.Name))
                    {
                        continue;
                    }

                    ImGui.BeginChild("subconfig" + Depth + " value", new Vector2(0, ImGui.GetWindowHeight()));
                    subSectionNode.Draw(ref changed);
                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }
                else
                {
                    subSectionNode.Draw(ref changed);
                }
            }
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

        public override ConfigPageNode? GetOrAddConfig<T>()
        {
            var type = typeof(T);
            if (type == null)
            {
                return null;
            }

            object[] attributes = type.GetCustomAttributes(true);

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
                            return subSectionNode.GetOrAddConfig<T>();
                        }
                    }

                    NestedSubSectionNode nestedSubSectionNode = new();
                    nestedSubSectionNode.Name = subSectionAttribute.SubSectionName;
                    nestedSubSectionNode.Depth = Depth + 1;
                    children.Add(nestedSubSectionNode);

                    return nestedSubSectionNode.GetOrAddConfig<T>();
                }
            }

            foreach (SubSectionNode subSectionNode in children)
            {
                if (subSectionNode.Name == type.FullName && subSectionNode is ConfigPageNode node)
                {
                    return node;
                }
            }

            ConfigPageNode configPageNode = new();

            var method = type.GetMethod("DefaultConfig", BindingFlags.Public | BindingFlags.Static);
            configPageNode.ConfigObject = (PluginConfigObject)method?.Invoke(null, null)!;
            configPageNode.Name = type.FullName!;
            children.Add(configPageNode);

            return configPageNode;
        }
    }

    public class ConfigPageNode : SubSectionNode
    {
        private PluginConfigObject _configObject = null!;
        public bool HasSeparator = true;
        public bool HasSpacing = false;

        public FieldInfo? ParentCollapseField = null;
        public PluginConfigObject? ParentConfigObject = null;

        private List<KeyValuePair<int, object>>? DrawList = null;

        public PluginConfigObject ConfigObject
        {
            get => _configObject;
            set
            {
                _configObject = value;
                GenerateNestedConfigPageNodes();
            }
        }

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
                    // see comments on ConfigPageNode's Load
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
            if (DrawList is null)
            {
                DrawList = new List<KeyValuePair<int, object>>();
                FieldInfo[] fields = ConfigObject.GetType().GetFields();
                List<CategoryField> collapseWithList = new();

                foreach (FieldInfo field in fields)
                {
                    bool wasAdded = false;

                    foreach (object attribute in field.GetCustomAttributes(true))
                    {
                        if (attribute is OrderAttribute orderAttribute)
                        {
                            CategoryField categoryField = new CategoryField(field, ConfigObject, ID);

                            // By default the 'Enabled' checkbox (inherited from PluginConfigObject) is considered the parent of all config items,
                            // so if the ConfigObject is marked as undisableable, we bypass adding items as children *only* if their parent was the 'Enabled' field.
                            if (orderAttribute.collapseWith is null || (!ConfigObject.Disableable && orderAttribute.collapseWith.Equals("Enabled")))
                            {
                                DrawList.Add(new KeyValuePair<int, object>(orderAttribute.pos, categoryField));
                            }
                            else
                            {
                                collapseWithList.Add(categoryField);
                            }

                            wasAdded = true;
                        }
                        else if (attribute is NestedConfigAttribute nestedConfigAttribute && _nestedConfigPageNodes.TryGetValue(field.Name, out ConfigPageNode? node))
                        {
                            node.HasSeparator = nestedConfigAttribute.separator;
                            node.HasSpacing = nestedConfigAttribute.spacing;
                            if (nestedConfigAttribute.collapseWith is not null)
                            {
                                node.ParentCollapseField = fields.Where(f => f.Name.Equals(nestedConfigAttribute.collapseWith)).FirstOrDefault();
                                node.ParentConfigObject = ConfigObject;
                            }

                            DrawList.Add(new KeyValuePair<int, object>(nestedConfigAttribute.pos, node));
                            wasAdded = true;
                        }
                    }

                    if (!wasAdded && Attribute.IsDefined(field, typeof(ConfigAttribute), true))
                    {
                        DrawList.Add(new KeyValuePair<int, object>(int.MaxValue, new CategoryField(field, ConfigObject, ID)));
                    }
                }

                // Loop through nodes that should have a parent and build those relationships
                foreach (CategoryField categoryField in collapseWithList)
                {
                    OrderAttribute? orderAttribute = categoryField.MainField.GetCustomAttributes(false).Where(a => a is OrderAttribute).FirstOrDefault() as OrderAttribute;
                    if (orderAttribute is not null && orderAttribute.collapseWith is not null)
                    {
                        // The CategoryField for the parent could either be in the drawList (if it is the root) or the collapseWithList (if there is multi-layered nesting)
                        // Create a union of the CategoryFields that already exist in the drawList and the collapseWithList, then search for the parent field.
                        // This looks incredibly gross but it's probably the cleanest way to do it without an extremely heavy refactor of the code above.
                        CategoryField? parentCategoryField = DrawList
                            .Where(kvp => kvp.Value is CategoryField)
                            .Select(kvp => kvp.Value as CategoryField)
                            .Union(collapseWithList)
                            .Where(categoryField => categoryField is not null && orderAttribute.collapseWith.Equals(categoryField.MainField.Name))
                            .FirstOrDefault();

                        if (parentCategoryField is not null)
                        {
                            parentCategoryField.CollapseControl = true;
                            categoryField.Depth = parentCategoryField.Depth + 1;
                            parentCategoryField.AddChild(orderAttribute.pos, categoryField);
                        }
                    }
                }

                DrawList.Sort((x, y) => x.Key - y.Key);
            }

            foreach (KeyValuePair<int, object> pair in DrawList)
            {
                if (pair.Value is CategoryField categoryField)
                {
                    categoryField.Draw(ref changed);
                }
                else if (pair.Value is ConfigPageNode node)
                {
                    // If the parent checkbox of this nested config is disabled, don't draw this nestedconfig
                    if (node.ParentCollapseField is not null && node.ParentConfigObject is not null)
                    {
                        if (!(Attribute.IsDefined(node.ParentCollapseField, typeof(CheckboxAttribute)) && 
                            (node.ParentCollapseField.GetValue(node.ParentConfigObject) as bool? ?? false)))
                        {
                            continue;
                        }
                    }

                    ImGui.BeginGroup();
                    if (node.HasSeparator)
                    {
                        Separator(1, 1);
                    }
                    if (node.HasSpacing)
                    {
                        Spacing(1);
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
        private void DrawImportExportGeneralConfig()
        {

            Separator(2, 1);

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

    public class CategoryField : Node
    {
        public SortedDictionary<int, CategoryField> Children;
        public FieldInfo MainField;
        public PluginConfigObject ConfigObject;
        public bool CollapseControl;
        public string? ID;
        public bool HasSeparator = false;
        public bool HasSpacing = false;
        public int Depth = 0;
        public bool IsChild = false;
        public ConfigAttribute? ConfigAttribute;

        public CategoryField(FieldInfo mainField, PluginConfigObject configObject, string? id = null)
        {
            MainField = mainField;
            ConfigObject = configObject;
            Children = new SortedDictionary<int, CategoryField>();
            ID = id;
            CollapseControl = false;

            ConfigAttribute = GetConfigAttribute(mainField);
            if (ConfigAttribute is not null)
            {
                HasSeparator = ConfigAttribute.separator;
                HasSpacing = ConfigAttribute.spacing;
            }
        }

        public void AddChild(int position, CategoryField field) 
        {
            field.IsChild = true;
            Children.Add(position, field); 
        }

        public void Draw(ref bool changed, bool separatorDrawn = false)
        {
            if (!IsChild)
            {
                DrawSeparatorOrSpacing(MainField, ID);
            }

            // Draw the ConfigAttribute
            Draw(ref changed, MainField);

            // Draw children
            if (CollapseControl && Attribute.IsDefined(MainField, typeof(CheckboxAttribute)) && (MainField.GetValue(ConfigObject) as bool? ?? false))
            {
                ImGui.BeginGroup();

                foreach (CategoryField child in Children.Values)
                {
                    DrawSeparatorOrSpacing(child.MainField, ID);
                    separatorDrawn |= child.HasSeparator;

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
            if (ConfigAttribute is not null)
            {
                changed |= ConfigAttribute.Draw(field, ConfigObject, ID);
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
                    Separator(1, 1);
                }
                else if (attribute is ConfigAttribute { spacing: true })
                {
                    Spacing(1);
                }
            }
        }
    }
}
