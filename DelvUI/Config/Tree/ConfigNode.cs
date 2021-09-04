﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using DelvUI.Config.Attributes;
using ImGuiNET;
using Newtonsoft.Json;

namespace DelvUI.Config.Tree
{
    public abstract class Node
    {
        public List<Node> children;

        public virtual void Save(string path)
        {
            foreach (var child in children) {
                child.Save(ConfigurationManager.GetInstance().ConfigDirectory);
            }
        }

        public virtual void Load(string path)
        {
            foreach (var child in children) {
                child.Load(ConfigurationManager.GetInstance().ConfigDirectory);
            }
        }
    }

    public class BaseNode : Node
    {
        public new List<SectionNode> children;

        public BaseNode()
        {
            children = new List<SectionNode>();
        }

        public void Draw()
        {
            var changed = false;
            
            ImGui.SetNextWindowSize(new Vector2(1050, 750), ImGuiCond.Appearing);


            if (!ImGui.Begin("titlebarnew", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse)) {
                return;
            }

            ImGui.BeginGroup(); // Middle section
            {
                ImGui.BeginGroup(); // Left
                {
                    var delvUiBanner = ConfigurationManager.GetInstance().BannerImage;
                    if (delvUiBanner != null) {
                        ImGui.Image(delvUiBanner.ImGuiHandle, new Vector2(delvUiBanner.Width, delvUiBanner.Height));
                    }

                    ImGui.BeginChild("left pane", new Vector2(150, -ImGui.GetFrameHeightWithSpacing()), true);
                    
                    foreach (var selectionNode in children)
                    {
                        if (ImGui.Selectable(selectionNode.Name, selectionNode.Selected))
                        {
                            selectionNode.Selected = true;
                            foreach (var otherNode in children.FindAll(x => x != selectionNode)) {
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
                    foreach (var selectionNode in children)
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
                if (ImGui.Button("Lock HUD")) // TODO: Functioning buttons
                {
                    
                }
                
                ImGui.SameLine();

                if (ImGui.Button("Hide/Show HUD"))
                {
                    
                }
                
                ImGui.SameLine();

                if (ImGui.Button("Reset HUD"))
                {
                    
                }
                
                ImGui.SameLine();

                if (ImGui.Button("Donate!"))
                {
                    
                }
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
            foreach (var child in children) {
                child.Load(path);
            }
        }

        public override void Save(string path)
        {
            foreach (var child in children) {
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
                    foreach (var sectionNode in children)
                    {
                        if (sectionNode.Name == sectionAttribute.SectionName)
                        {
                            return sectionNode.GetOrAddConfig(configObject);
                        }
                    }

                    SectionNode newNode = new SectionNode();
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

        public SectionNode()
        {
            children = new List<SubSectionNode>();
        }

        public void Draw(ref bool changed)
        {
            if (!Selected) {
                return;
            }

            ImGui.BeginChild("item view",new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us
            {
                if (ImGui.BeginTabBar("##Tabs", ImGuiTabBarFlags.None))
                {
                    foreach (var subSectionNode in children)
                    {
                        if (!ImGui.BeginTabItem(subSectionNode.Name)) {
                            continue;
                        }

                        ImGui.BeginChild("subconfig value", new Vector2(0, 0), true);
                        subSectionNode.Draw(ref changed);
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }
                    ImGui.EndTabBar(); 
                    
                    // TODO: Close Button - Maybe better suited elsewhere
                }
            }
            ImGui.EndChild();
        }

        public override void Save(string path)
        {
            foreach (var child in children)
            {
                child.Save(Path.Combine(path, Name));
            }
        }

        public override void Load(string path)
        {
            foreach (var child in children)
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
                    foreach (var subSectionNode in children)
                    {
                        if (subSectionNode.Name == subSectionAttribute.SubSectionName)
                        {
                            return subSectionNode.GetOrAddConfig(configObject);
                        }
                    }

                    if (subSectionAttribute.Depth == 0)
                    {
                        NestedSubSectionNode newNode = new NestedSubSectionNode();
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

        public NestedSubSectionNode()
        {
            children = new List<SubSectionNode>();
        }

        public override void Draw(ref bool changed)
        {
            ImGui.BeginChild("item" + Depth + " view",new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us
            {
                if (ImGui.BeginTabBar("##tabs" + Depth, ImGuiTabBarFlags.None))
                {
                    foreach (var subSectionNode in children)
                    {
                        if (subSectionNode is NestedSubSectionNode)
                        {
                            if (!ImGui.BeginTabItem(subSectionNode.Name)) {
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
            foreach (var child in children)
            {
                child.Save(Path.Combine(path, Name));
            }
        }

        public override void Load(string path)
        {
            foreach (var child in children)
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
                    if (subSectionAttribute.Depth != Depth + 1) {
                        continue;
                    }

                    foreach (var subSectionNode in children)
                    {
                        if (subSectionNode.Name == subSectionAttribute.SubSectionName)
                        {
                            return subSectionNode.GetOrAddConfig(configObject);
                        }
                    }

                    NestedSubSectionNode nestedSubSectionNode = new NestedSubSectionNode();
                    nestedSubSectionNode.Name = subSectionAttribute.SubSectionName;
                    nestedSubSectionNode.Depth = Depth + 1;
                    children.Add(nestedSubSectionNode);
                    return nestedSubSectionNode.GetOrAddConfig(configObject);
                }
            }
            
            foreach (var subSectionNode in children)
            {
                if (subSectionNode.Name == configObject.GetType().FullName && subSectionNode is ConfigPageNode node) {
                    return node;
                }
            }
            ConfigPageNode configPageNode = new ConfigPageNode();
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
            foreach (var field in fields)
            {
                object fieldVal = field.GetValue(ConfigObject);
                foreach (var attribute in field.GetCustomAttributes(true))
                {
                    if (attribute is CheckboxAttribute checkboxAttribute)
                    {
                        bool boolVal = (bool) fieldVal;
                        if (ImGui.Checkbox(checkboxAttribute.friendlyName, ref boolVal))
                        {
                            field.SetValue(ConfigObject, boolVal);
                            changed = true;
                        }
                    }
                    else if (attribute is DragFloatAttribute dragFloatAttribute)
                    {
                        float floatVal = (float) fieldVal;
                        if (ImGui.DragFloat(dragFloatAttribute.friendlyName, ref floatVal, dragFloatAttribute.velocity, dragFloatAttribute.min, dragFloatAttribute.max))
                        {
                            field.SetValue(ConfigObject, floatVal);
                            changed = true;
                        }
                    }
                    else if (attribute is DragIntAttribute dragIntAttribute)
                    {
                        int intVal = (int) fieldVal;
                        if (ImGui.DragInt(dragIntAttribute.friendlyName, ref intVal, dragIntAttribute.velocity, dragIntAttribute.min, dragIntAttribute.max))
                        {
                            field.SetValue(ConfigObject, intVal);
                            changed = true;
                        }
                    }
                    else if (attribute is DragFloat2Attribute dragFloat2Attribute)
                    {
                        Vector2 floatVal = (Vector2) fieldVal;
                        if (ImGui.DragFloat2(dragFloat2Attribute.friendlyName, ref floatVal, dragFloat2Attribute.velocity, dragFloat2Attribute.min, dragFloat2Attribute.max))
                        {
                            field.SetValue(ConfigObject, floatVal);
                            changed = true;
                        }
                    }
                    else if (attribute is DragInt2Attribute dragInt2Attribute)
                    {
                        Vector2 intVal = (Vector2) fieldVal;
                        if (ImGui.DragFloat2(dragInt2Attribute.friendlyName, ref intVal, dragInt2Attribute.velocity, dragInt2Attribute.min, dragInt2Attribute.max))
                        {
                            field.SetValue(ConfigObject, intVal);
                        }
                    }
                    else if (attribute is InputTextAttribute inputTextAttribute)
                    {
                        string stringVal = (string) fieldVal;
                        if (ImGui.InputText(inputTextAttribute.friendlyName, ref stringVal, inputTextAttribute.maxLength))
                        {
                            field.SetValue(ConfigObject, stringVal);
                        }
                    }
                    else if (attribute is ColorEdit4Attribute colorEdit4Attribute)
                    {
                        PluginConfigColor colorVal = (PluginConfigColor) fieldVal;
                        var vector = colorVal.Vector;
                        if (ImGui.ColorEdit4(colorEdit4Attribute.friendlyName, ref vector))
                        {
                            colorVal.Vector = vector;
                            field.SetValue(ConfigObject, colorVal);
                        }
                    }
                }
            }
        }

        public override void Save(string path)
        {
            Directory.CreateDirectory(path);
            string finalPath = path + ".json";
            File.WriteAllText(finalPath, JsonConvert.SerializeObject(ConfigObject, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects,
            }));
        }

        public override void Load(string path)
        {
            var finalPath = new FileInfo(path + ".json");
            var configObject =  !finalPath.Exists ? ConfigObject : JsonConvert.DeserializeObject<PluginConfigObject>(File.ReadAllText(finalPath.FullName), new JsonSerializerSettings
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects,
            });
            ConfigObject = configObject;
        }

        public override ConfigPageNode GetOrAddConfig(PluginConfigObject configObject)
        {
            return this;
        }
    }
}