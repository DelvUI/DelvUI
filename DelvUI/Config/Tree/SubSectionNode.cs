using DelvUI.Config.Attributes;
using Dalamud.Bindings.ImGui;
using System.IO;
using System.Numerics;
using System.Reflection;

namespace DelvUI.Config.Tree
{
    public abstract class SubSectionNode : Node
    {
        public string Name = null!;
        public int Depth;
        public string? ForceSelectedTabName = null;

        public abstract bool Draw(ref bool changed);

        public abstract ConfigPageNode? GetOrAddConfig<T>() where T : PluginConfigObject;
    }

    public class NestedSubSectionNode : SubSectionNode
    {
        public NestedSubSectionNode() { }

        public override bool Draw(ref bool changed)
        {
            bool didReset = false;

            if (_children.Count > 1)
            {
                ImGui.BeginChild(
                    "DelvUI_Tabs_" + Depth,
                    new Vector2(0, ImGui.GetWindowHeight() - 22),
                    false,
                    ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse
                ); // Leave room for 1 line below us

                if (ImGui.BeginTabBar("##tabs" + Depth, ImGuiTabBarFlags.None))
                {
                    didReset |= DrawSubConfig(ref changed);
                }

                ImGui.EndTabBar();

                ImGui.EndChild();
            }
            else
            {
                ImGui.BeginChild("item" + Depth + " view", new Vector2(0, ImGui.GetWindowHeight() - 20)); // Leave room for 1 line below us

                didReset |= DrawSubConfig(ref changed);

                ImGui.EndChild();
            }

            return didReset;
        }

        public bool DrawSubConfig(ref bool changed)
        {
            bool didReset = false;

            foreach (SubSectionNode subSectionNode in _children)
            {
                if (subSectionNode is NestedSubSectionNode)
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

                    ImGui.BeginChild("subconfig" + Depth + " value", new Vector2(0, ImGui.GetWindowHeight()));
                    didReset |= subSectionNode.Draw(ref changed);
                    ImGui.EndChild();

                    ImGui.EndTabItem();
                }
                else
                {
                    didReset |= subSectionNode.Draw(ref changed);
                }
            }

            ForceSelectedTabName = null;

            didReset |= DrawResetModal();

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

                    foreach (SubSectionNode subSectionNode in _children)
                    {
                        if (subSectionNode.Name == subSectionAttribute.SubSectionName)
                        {
                            return subSectionNode.GetOrAddConfig<T>();
                        }
                    }

                    NestedSubSectionNode nestedSubSectionNode = new();
                    nestedSubSectionNode.Name = subSectionAttribute.SubSectionName;
                    nestedSubSectionNode.Depth = Depth + 1;
                    _children.Add(nestedSubSectionNode);

                    return nestedSubSectionNode.GetOrAddConfig<T>();
                }
            }

            foreach (SubSectionNode subSectionNode in _children)
            {
                if (subSectionNode.Name == type.FullName && subSectionNode is ConfigPageNode node)
                {
                    return node;
                }
            }

            ConfigPageNode configPageNode = new();
            configPageNode.ConfigObject = ConfigurationManager.GetDefaultConfigObjectForType(type);
            configPageNode.Name = type.FullName!;
            _children.Add(configPageNode);

            return configPageNode;
        }
    }
}
