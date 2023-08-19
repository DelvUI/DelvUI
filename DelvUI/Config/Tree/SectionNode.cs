using DelvUI.Config.Attributes;
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
        public bool ForceAllowExport = false;
        public string? ForceSelectedTabName = null;

        public SectionNode() { }

        protected override bool AllowExport()
        {
            if (ForceAllowExport) { return true; }

            return base.AllowExport();
        }

        public bool Draw(ref bool changed, float alpha)
        {
            if (!Selected)
            {
                return false;
            }

            bool didReset = false;

            ImGui.NewLine();

            ImGui.BeginChild(
                "DelvU_Settings_Tab",
                new Vector2(0, -10),
                false,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse
            ); // Leave room for 1 line below us

            {
                if (ConfigurationManager.Instance.OverrideDalamudStyle)
                {
                    ImGui.PushStyleColor(ImGuiCol.Tab, new Vector4(45f / 255f, 45f / 255f, 45f / 255f, alpha));
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(45f / 255f, 45f / 255f, 45f / 255f, alpha));
                }

                if (ImGui.BeginTabBar("##Tabs", ImGuiTabBarFlags.None))
                {
                    foreach (SubSectionNode subSectionNode in _children)
                    {
                        if (ForceSelectedTabName != null)
                        {
                            bool a = subSectionNode.Name == ForceSelectedTabName; // no idea how this works
                            ImGuiTabItemFlags flag = subSectionNode.Name == ForceSelectedTabName ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;

                            if (!ImGui.BeginTabItem(subSectionNode.Name, ref a, flag))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!ImGui.BeginTabItem(subSectionNode.Name))
                            {
                                continue;
                            }
                        }

                        DrawExportResetContextMenu(subSectionNode, subSectionNode.Name);

                        ImGui.BeginChild("subconfig value", new Vector2(0, 0), true);
                        didReset |= subSectionNode.Draw(ref changed);
                        ImGui.EndChild();

                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                    ForceSelectedTabName = null;
                }

                if (ConfigurationManager.Instance.OverrideDalamudStyle)
                {
                    ImGui.PopStyleColor(2);
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

            Type type = typeof(T);
            throw new ArgumentException("The provided configuration object does not specify a sub-section: " + type.Name);
        }
    }
}
