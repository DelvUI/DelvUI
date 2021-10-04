using Dalamud.Interface;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.IO;
using System.Numerics;

namespace DelvUI.Config.Tree
{
    public class SectionNode : Node
    {
        public bool Selected;
        public string Name = null!;

        public SectionNode() { }

        public bool Draw(ref bool changed)
        {
            if (!Selected)
            {
                return false;
            }

            bool didReset = false;

            ImGui.BeginChild(
                "item view",
                new Vector2(0, -ImGui.GetFrameHeightWithSpacing() - 15),
                false,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse
            ); // Leave room for 1 line below us

            {
                if (ImGui.BeginTabBar("##Tabs", ImGuiTabBarFlags.None))
                {
                    foreach (SubSectionNode subSectionNode in _children)
                    {
                        if (!ImGui.BeginTabItem(subSectionNode.Name))
                        {
                            continue;
                        }

                        DrawExportResetContextMenu(subSectionNode, subSectionNode.Name);

                        ImGui.BeginChild("subconfig value", new Vector2(0, 0), true);
                        didReset |= subSectionNode.Draw(ref changed);
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

                didReset |= DrawResetModal();
            }

            ImGui.EndChild();

            return didReset;
        }

        public override void Save(string path)
        {
            foreach (SubSectionNode child in _children)
            {
                child.Save(Path.Combine(path, Name));
            }
        }

        public override void Load(string path)
        {
            foreach (SubSectionNode child in _children)
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
                    foreach (SubSectionNode subSectionNode in _children)
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
                        _children.Add(newNode);

                        return newNode.GetOrAddConfig<T>();
                    }
                }
            }

            throw new ArgumentException("The provided configuration object does not specify a sub-section");
        }
    }
}
