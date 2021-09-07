using DelvUI.Config.Attributes;
using ImGuiNET;
using ImGuiScene;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;

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
    }

    public class BaseNode : Node
    {
        public new List<SectionNode> children;

        public BaseNode() { children = new List<SectionNode>(); }

        private void ToggleJobPacks()
        {
            ConfigurationManager.GetInstance().ConfigurationWindow.IsVisible = !ConfigurationManager.GetInstance().ConfigurationWindow.IsVisible; 
            ConfigurationManager.GetInstance().DrawConfigWindow = !ConfigurationManager.GetInstance().DrawConfigWindow;
        }
        
        public void Draw()
        {
            var changed = false;

            ImGui.SetNextWindowSize(new Vector2(1050, 750), ImGuiCond.Appearing);

            if (!ImGui.Begin("titlebarnew", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse))
            {
                return;
            }

            ImGui.BeginGroup(); // Middle section

            {
                ImGui.BeginGroup(); // Left

                {
                    TextureWrap delvUiBanner = ConfigurationManager.GetInstance().BannerImage;

                    if (delvUiBanner != null)
                    {
                        ImGui.Image(delvUiBanner.ImGuiHandle, new Vector2(delvUiBanner.Width, delvUiBanner.Height));
                    }

                    ImGui.BeginChild("left pane", new Vector2(150, -ImGui.GetFrameHeightWithSpacing()), true);

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

            ImGui.Separator();

            ImGui.BeginGroup(); // Bottom Bar

            {
                if (ImGui.Button("Job Packs"))
                {
                    ToggleJobPacks();
                }
                
                ImGui.SameLine();
                
                if (ImGui.Button("Lock HUD")) // TODO: Functioning buttons
                { }

                ImGui.SameLine();

                if (ImGui.Button("Hide/Show HUD"))
                { }

                ImGui.SameLine();

                if (ImGui.Button("Reset HUD"))
                { }

                ImGui.SameLine();

                var pos = ImGui.GetCursorPos();
                ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 60, ImGui.GetCursorPos().Y));
                if (ImGui.Button("Donate!"))
                {
                    Process.Start("https://ko-fi.com/DelvUI");
                }
                ImGui.SetCursorPos(pos);

                // show current version
                ImGui.Text($"v{Plugin.Version}");
            }

            ImGui.EndGroup(); // Bottom Bar

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
            var attributes = configObject.GetType().GetCustomAttributes(false);

            foreach (var attribute in attributes)
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

        public void Draw(ref bool changed)
        {
            if (!Selected)
            {
                return;
            }

            ImGui.BeginChild("item view", new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us

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

                    // TODO: Close Button - Maybe better suited elsewhere
                    /* Current close button code
                    Vector2 pos = ImGui.GetCursorPos();
                    ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 20, 0));
                    ImGui.PushFont(UiBuilder.IconFont);

                    if (ImGui.Button(FontAwesomeIcon.Times.ToIconString())) {
                        //hide config window
                    }

                    ImGui.PopFont();
                    ImGui.SetCursorPos(pos);
                    */
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
            var attributes = configObject.GetType().GetCustomAttributes(false);

            foreach (var attribute in attributes)
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
            var attributes = configObject.GetType().GetCustomAttributes(false);

            foreach (var attribute in attributes)
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

        public override void Draw(ref bool changed)
        {
            var fields = ConfigObject.GetType().GetFields();
            var drawList = new List<KeyValuePair<int, CategoryField>>();
            var collapseWithList = new List<FieldInfo>();

            foreach (FieldInfo field in fields)
            {
                var hasOrderAttribute = false;
                foreach (var attribute in field.GetCustomAttributes(true))
                {
                    if (attribute is OrderAttribute orderAttribute)
                    {
                        drawList.Add(new KeyValuePair<int, CategoryField>(orderAttribute.pos, new CategoryField(field, ConfigObject)));
                        hasOrderAttribute = true;
                    }
                    else if (attribute is CollapseControlAttribute collapseControlAtrribute)
                    {
                        CategoryField categoryField = new CategoryField(field, ConfigObject);
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

            foreach (var field in collapseWithList)
            {
                foreach (var attribute in field.GetCustomAttributes(true))
                {
                    if (attribute is CollapseWithAttribute collapseWithAttribute)
                    {
                        foreach (var categoryField in drawList)
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
            foreach (var pair in drawList)
            {
                pair.Value.Draw(ref changed);
            }
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
            var finalPath = new FileInfo(path + ".json");

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
                var methodInfo = GetType().GetMethod("LoadForType");
                var function = methodInfo.MakeGenericMethod(ConfigObject.GetType());
                ConfigObject = (PluginConfigObject)function.Invoke(this, new object[] { finalPath.FullName });
            }
        }

        public T LoadForType<T>(string path) where T : PluginConfigObject
        {
            var file = new FileInfo(path);

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

        public void AddChild(int position, FieldInfo field)
        {
            Children.Add(position, field);
        }

        public void Draw(ref bool changed)
        {
            Draw(ref changed, MainField, 0);
            if (CategoryId != -1 && (bool)MainField.GetValue(ConfigObject))
            {
                ImGui.BeginGroup();
                ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(0, 2));
                foreach (var child in Children.Values)
                {
                    Draw(ref changed, child, 4);
                }
                ImGui.EndGroup();
                ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin() + new Vector2(0, -2), ImGui.GetItemRectMax() + new Vector2(ImGui.GetContentRegionAvail().X - ImGui.GetItemRectMax().X + ImGui.GetItemRectMin().X - 4, 4), 0xFF4A4141);
                ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(0, 2));
            }
        }

        public void Draw(ref bool changed, FieldInfo field, int xOffset)
        {
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(xOffset, 0));
            object fieldVal = field.GetValue(ConfigObject);

            foreach (var attribute in field.GetCustomAttributes(true))
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
                    var vector = colorVal.Vector;

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
