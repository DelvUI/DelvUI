using DelvUI.Config.Attributes;
using ImGuiNET;
using System.IO;
using System.Numerics;
using System.Reflection;

namespace DelvUI.Config.Tree
{
    public abstract class SubSectionNode : Node
    {
        public string Name = null!;
        public int Depth;

        public abstract void Draw(ref bool changed);

        public abstract ConfigPageNode? GetOrAddConfig<T>() where T : PluginConfigObject;
    }

    public class NestedSubSectionNode : SubSectionNode
    {
        public NestedSubSectionNode() { }

        public override void Draw(ref bool changed)
        {
            if (_children.Count > 1)
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
            foreach (SubSectionNode subSectionNode in _children)
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

            var method = type.GetMethod("DefaultConfig", BindingFlags.Public | BindingFlags.Static);
            configPageNode.ConfigObject = (PluginConfigObject)method?.Invoke(null, null)!;
            configPageNode.Name = type.FullName!;
            _children.Add(configPageNode);

            return configPageNode;
        }
    }
}
