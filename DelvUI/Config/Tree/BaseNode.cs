using Dalamud.Logging;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace DelvUI.Config.Tree
{
    public delegate void ConfigObjectResetEventHandler(BaseNode sender);

    public class BaseNode : Node
    {
        public event ConfigObjectResetEventHandler? ConfigObjectResetEvent;

        private Dictionary<Type, ConfigPageNode> _configPageNodesMap;

        public BaseNode()
        {
            _configPageNodesMap = new Dictionary<Type, ConfigPageNode>();
        }

        public T? GetConfigObject<T>() where T : PluginConfigObject
        {
            var pageNode = GetConfigPageNode<T>();

            return pageNode != null ? (T)pageNode.ConfigObject : null;
        }

        public ConfigPageNode? GetConfigPageNode<T>() where T : PluginConfigObject
        {
            if (_configPageNodesMap.TryGetValue(typeof(T), out var node))
            {
                return node;
            }

            var configPageNode = GetOrAddConfig<T>();

            if (configPageNode != null && configPageNode.ConfigObject != null)
            {
                _configPageNodesMap.Add(typeof(T), configPageNode);

                return configPageNode;
            }

            return null;
        }

        public void SetConfigPageNode(ConfigPageNode configPageNode)
        {
            if (configPageNode.ConfigObject == null)
            {
                return;
            }

            _configPageNodesMap[configPageNode.ConfigObject.GetType()] = configPageNode;
        }

        public void SetConfigObject(PluginConfigObject configObject)
        {
            if (_configPageNodesMap.TryGetValue(configObject.GetType(), out ConfigPageNode? configPageNode))
            {
                configPageNode.ConfigObject = configObject;
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
            bool didReset = false;

            ImGui.SetNextWindowSize(new Vector2(1050, 750), ImGuiCond.Appearing);
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.BorderShadow, new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 1f));
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(20f / 255f, 21f / 255f, 20f / 255f, 1f));

            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 1);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);

            if (!ImGui.Begin("DelvUI_settings", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse))
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
                    if (_children.Any() && _children.Find(o => o is SectionNode sectionNode && sectionNode.Selected) == null)
                    {
                        SectionNode node = (SectionNode)_children[0];
                        node.Selected = true;
                    }

                    foreach (SectionNode selectionNode in _children)
                    {
                        if (ImGui.Selectable(selectionNode.Name, selectionNode.Selected))
                        {
                            selectionNode.Selected = true;

                            foreach (SectionNode otherNode in _children.FindAll(x => x != selectionNode))
                            {
                                otherNode.Selected = false;
                            }
                        }

                        DrawExportResetContextMenu(selectionNode, selectionNode.Name);
                    }

                    ImGui.EndChild();
                }

                ImGui.EndGroup(); // Left

                didReset |= DrawResetModal();

                ImGui.SameLine();

                ImGui.BeginGroup(); // Right

                {
                    foreach (SectionNode selectionNode in _children)
                    {
                        didReset |= selectionNode.Draw(ref changed);
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

            if (didReset)
            {
                ConfigObjectResetEvent?.Invoke(this);
            }

            if (changed || didReset)
            {
                ConfigurationManager.Instance.SaveConfigurations();
            }
        }

        public ConfigPageNode? GetOrAddConfig<T>() where T : PluginConfigObject
        {
            object[] attributes = typeof(T).GetCustomAttributes(true);

            foreach (object attribute in attributes)
            {
                if (attribute is SectionAttribute sectionAttribute)
                {
                    foreach (SectionNode sectionNode in _children)
                    {
                        if (sectionNode.Name == sectionAttribute.SectionName)
                        {
                            return sectionNode.GetOrAddConfig<T>();
                        }
                    }

                    SectionNode newNode = new();
                    newNode.Name = sectionAttribute.SectionName;
                    _children.Add(newNode);

                    return newNode.GetOrAddConfig<T>();
                }
            }

            throw new ArgumentException("The provided configuration object does not specify a section");
        }
    }
}
