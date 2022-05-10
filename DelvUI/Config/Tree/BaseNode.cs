using Dalamud.Interface;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Config.Tree
{
    public delegate void ConfigObjectResetEventHandler(BaseNode sender);

    public class BaseNode : Node
    {
        public event ConfigObjectResetEventHandler? ConfigObjectResetEvent;

        private Dictionary<Type, ConfigPageNode> _configPageNodesMap;

        public bool NeedsSave = false;
        public string? SelectedOptionName = null;

        private List<Node> _extraNodes = new List<Node>();
        private List<Node>? _nodes = null;

        public BaseNode()
        {
            _configPageNodesMap = new Dictionary<Type, ConfigPageNode>();
        }

        public void AddExtraSectionNode(SectionNode node)
        {
            _extraNodes.Add(node);
            _nodes = null;
        }

        public T? GetConfigObject<T>() where T : PluginConfigObject
        {
            var pageNode = GetConfigPageNode<T>();

            return pageNode != null ? (T)pageNode.ConfigObject : null;
        }

        public void RemoveConfigObject<T>() where T : PluginConfigObject
        {
            if (_configPageNodesMap.ContainsKey(typeof(T)))
            {
                _configPageNodesMap.Remove(typeof(T));
            }
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

        public bool SetConfigObject(PluginConfigObject configObject)
        {
            if (_configPageNodesMap.TryGetValue(configObject.GetType(), out ConfigPageNode? configPageNode))
            {
                configPageNode.ConfigObject = configObject;
                return true;
            }

            return false;
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

        private void CreateNodesIfNeeded()
        {
            if (_nodes != null)
            {
                return;
            }

            _nodes = new List<Node>();
            _nodes.AddRange(_children);
            _nodes.AddRange(_extraNodes);
        }

        public void RefreshSelectedNode()
        {
            if (_nodes == null)
            {
                return;
            }

            foreach (SectionNode node in _nodes.FindAll(x => x is SectionNode))
            {
                node.Selected = node.Name == SelectedOptionName;
            }
        }

        public void Draw(float alpha)
        {
            CreateNodesIfNeeded();
            if (_nodes == null)
            {
                return;
            }

            bool changed = false;
            bool didReset = false;

            PushStyles();

            ImGui.BeginGroup(); // Middle section
            {
                ImGui.BeginGroup(); // Left
                {
                    // banner
                    TextureWrap? delvUiBanner = ConfigurationManager.Instance.BannerImage;
                    if (delvUiBanner != null)
                    {
                        ImGui.Image(delvUiBanner.ImGuiHandle, new Vector2(delvUiBanner.Width, delvUiBanner.Height));
                    }

                    // version
                    ImGui.SetCursorPos(new Vector2(60, 35));
                    ImGui.Text($"v{Plugin.Version}");

                    // section list
                    ImGui.BeginChild("left pane", new Vector2(150, -ImGui.GetFrameHeightWithSpacing() - 15), true);

                    // if no section is selected, select the first
                    if (_nodes.Any() && _nodes.Find(o => o is SectionNode sectionNode && sectionNode.Selected) == null)
                    {
                        SectionNode? selectedSection = (SectionNode?)_nodes.Find(o => o is SectionNode sectionNode && sectionNode.Name == SelectedOptionName);
                        if (selectedSection != null)
                        {
                            selectedSection.Selected = true;
                            SelectedOptionName = selectedSection.Name;
                        }
                        else if (_nodes.Count > 0)
                        {
                            SectionNode node = (SectionNode)_nodes[0];
                            node.Selected = true;
                            SelectedOptionName = node.Name;
                        }
                    }

                    foreach (SectionNode selectionNode in _nodes)
                    {
                        if (ImGui.Selectable(selectionNode.Name, selectionNode.Selected))
                        {
                            selectionNode.Selected = true;
                            SelectedOptionName = selectionNode.Name;

                            foreach (SectionNode otherNode in _nodes.FindAll(x => x != selectionNode))
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
                    foreach (SectionNode selectionNode in _nodes)
                    {
                        didReset |= selectionNode.Draw(ref changed, alpha);
                    }
                }

                ImGui.EndGroup(); // Right
            }

            ImGui.EndGroup(); // Middle section

            ImGui.BeginGroup();
            {
                ImGui.SetCursorPosX(0);

                ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);
                ImGui.BeginChild("DelvUI_Settings_Buttons", new Vector2(1200, 0), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                ImGui.PopStyleColor();

                const float buttonWidth = 150;

                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(45f / 255f, 45f / 255f, 45f / 255f, alpha));
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
                if (ImGui.Button((ConfigurationManager.Instance.ShowHUD ? "Hide" : "Show") + " HUD", new Vector2(buttonWidth, 0)))
                {
                    ConfigurationManager.Instance.ShowHUD = !ConfigurationManager.Instance.ShowHUD;
                }

                ImGui.SameLine();
                if (ImGui.Button((ConfigurationManager.Instance.LockHUD ? "Unlock" : "Lock") + " HUD", new Vector2(buttonWidth, 0)))
                {
                    ConfigurationManager.Instance.LockHUD = !ConfigurationManager.Instance.LockHUD;
                }

                ImGui.PopStyleVar();

                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 93);
                if (ImGui.Button($"Changelog", new Vector2(buttonWidth, 0)))
                {
                    ConfigurationManager.Instance.OpenChangelogWindow();
                }

                ImGui.SameLine();

                if (ImGui.Button("Browse Presets", new Vector2(buttonWidth, 0)))
                {
                    Utils.OpenUrl("https://wago.io/delvui");
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Find premade profiles on Wago.io!");
                }

                ImGui.PopStyleColor();

                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(114f / 255f, 137f / 255f, 218f / 255f, alpha));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(114f / 255f, 137f / 255f, 218f / 255f, alpha * .85f));

                if (ImGui.Button("Discord", new Vector2(buttonWidth, 0)))
                {
                    Utils.OpenUrl("https://discord.gg/xzde5qQayh");
                }

                ImGui.PopStyleColor(2);

                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(255f / 255f, 94f / 255f, 91f / 255f, alpha));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(255f / 255f, 94f / 255f, 91f / 255f, alpha * .85f));

                if (ImGui.Button("Donate!", new Vector2(buttonWidth, 0)))
                {
                    Utils.OpenUrl("https://ko-fi.com/DelvUI");
                }

                ImGui.PopStyleColor(2);
                ImGui.EndChild();
            }
            ImGui.EndGroup();

            // close button
            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 28, 5));
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(229f / 255f, 57f / 255f, 57f / 255f, alpha));
            if (ImGui.Button(FontAwesomeIcon.Times.ToIconString(), new Vector2(22, 22)))
            {
                ConfigurationManager.Instance.CloseConfigWindow();
            }
            ImGui.PopStyleColor();
            ImGui.PopFont();

            PopStyles();

            if (didReset)
            {
                ConfigObjectResetEvent?.Invoke(this);
            }

            NeedsSave |= changed | didReset;
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
                    newNode.ForceAllowExport = sectionAttribute.ForceAllowExport;
                    _children.Add(newNode);

                    return newNode.GetOrAddConfig<T>();
                }
            }

            Type type = typeof(T);
            throw new ArgumentException("The provided configuration object does not specify a section: " + type.Name);
        }
    }
}
