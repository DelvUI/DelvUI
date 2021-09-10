using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;

namespace DelvUI.Config
{
    public class ConfigurationWindow
    {
        private readonly string[] _configColorMap = { "Tanks", "Healers", "Melee", "Ranged", "Casters", "NPC" };
        private readonly Dictionary<string, Array> _configMap = new();
        private readonly PluginConfiguration _pluginConfiguration;
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly int _viewportHeight = (int)ImGui.GetMainViewport().Size.Y;
        private readonly int _viewportWidth = (int)ImGui.GetMainViewport().Size.X;
        private bool _changed;
        private string _exportString = "";
        private string _importString = "";
        private string _selected = "General";
        private string _selectedColorType = "Tanks";
        private int _xOffsetLimit;
        private int _yOffsetLimit;
        public bool IsVisible;

        public ConfigurationWindow(PluginConfiguration pluginConfiguration)
        {
            _pluginConfiguration = pluginConfiguration;
            _pluginInterface = Plugin.GetPluginInterface();
            _configMap.Add("General", new[] { "General" });

            _configMap.Add("Individual Unitframes", new[] { "General", "Player", "Focus", "Target", "Target of Target" });

            //configMap.Add("Group Unitframes", new [] {"General", "Party", "8man", "24man", "Enemies"});
            _configMap.Add("Castbars", new[] { "Player", "Target" });

            // _configMap.Add("Buffs and Debuffs", new[] { "Player Buffs", "Player Debuffs", "Target Buffs", "Target Debuffs", "Raid/Job Buffs" });

            //_configMap.Add("Job Specific Bars", new[] { "General" });
            _configMap.Add("Import/Export", new[] { "General" });
        }

        public void ToggleHud()
        {
            _pluginConfiguration.HideHud = !_pluginConfiguration.HideHud;
            _changed = true;
        }

        public bool GetToggleHudState() { return _pluginConfiguration.HideHud; }

        public void ToggleJobPacks()
        {
            IsVisible = !IsVisible;
            ConfigurationManager.GetInstance().DrawConfigWindow = !ConfigurationManager.GetInstance().DrawConfigWindow;
        }

        public void ShowHud()
        {
            _pluginConfiguration.HideHud = false;
            _changed = true;
        }

        public void HideHud()
        {
            _pluginConfiguration.HideHud = true;
            _changed = true;
        }

        public void Draw()
        {
            if (!IsVisible)
            {
                return;
            }

            //Todo future reference dalamud native ui scaling
            //ImGui.GetIO().FontGlobalScale;
            ImGui.SetNextWindowSize(new Vector2(1050, 750), ImGuiCond.Appearing);

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(10f / 255f, 10f / 255f, 10f / 255f, 0.95f));

            if (!ImGui.Begin(
                "titlebar",
                ref IsVisible,
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse
            ))
            {
                return;
            }

            ImGui.PopStyleColor();
            _xOffsetLimit = _viewportWidth / 2;
            _yOffsetLimit = _viewportHeight / 2;
            _changed = false;
            Vector2 pos = ImGui.GetCursorPos();

            ImGui.BeginGroup(); //Main Group

            {
                ImGui.BeginGroup(); // Left

                {
                    TextureWrap delvUiBanner = _pluginConfiguration.BannerImage;
                    ImGui.Image(delvUiBanner.ImGuiHandle, new Vector2(delvUiBanner.Width, delvUiBanner.Height));
                    ImGui.BeginChild("left pane", new Vector2(150, -ImGui.GetFrameHeightWithSpacing() - 32), true);

                    foreach (string config in _configMap.Keys)
                    {
                        if (ImGui.Selectable(config, _selected == config))
                        {
                            _selected = config;
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndGroup();

                ImGui.SameLine();

                // Right
                ImGui.BeginGroup();

                {
                    Array subConfigs = _configMap[_selected];

                    ImGui.BeginChild("item view", new Vector2(0, -ImGui.GetFrameHeightWithSpacing() - 32)); // Leave room for 1 line below us

                    {
                        if (ImGui.BeginTabBar("##Tabs", ImGuiTabBarFlags.None))
                        {
                            foreach (string subConfig in subConfigs)
                            {
                                if (!ImGui.BeginTabItem(subConfig))
                                {
                                    continue;
                                }

                                ImGui.BeginChild("subconfig value", new Vector2(0, 0), true);
                                DrawSubConfig(_selected, subConfig);
                                ImGui.EndChild();
                                ImGui.EndTabItem();
                            }

                            ImGui.EndTabBar();
                            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 20, 0));
                            ImGui.PushFont(UiBuilder.IconFont);

                            if (ImGui.Button(FontAwesomeIcon.Times.ToIconString()))
                            {
                                IsVisible = false;
                            }

                            ImGui.PopFont();
                            ImGui.SetCursorPos(pos);
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndGroup();
            }

            ImGui.EndGroup(); // Main Group

            ImGui.BeginGroup();

            if (ImGui.Button("Switch to Job Packs Configuration", new Vector2(ImGui.GetWindowContentRegionWidth(), 0)))
            {
                ToggleJobPacks();
            }

            ImGui.Separator();

            ImGui.EndGroup();

            ImGui.BeginGroup();

            if (ImGui.Button(GetToggleHudState() ? "Show HUD" : "Hide HUD", new Vector2(ImGui.GetWindowWidth() / 7, 0)))
            {
                ToggleHud();
            }

            ImGui.SameLine();

            if (ImGui.Button("Reset to Default", new Vector2(ImGui.GetWindowWidth() / 7, 0)))

            {
                _pluginConfiguration.TransferConfig(PluginConfiguration.ReadConfig("default"));
                _changed = true;
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

            if (_changed)
            {
                _pluginConfiguration.BuildColorMap();
                _pluginConfiguration.Save();
            }

            ImGui.End();
        }

        private void DrawSubConfig(string config, string subConfig)
        {
            switch (config)
            {
                case "General":
                    switch (subConfig)
                    {
                        case "General":
                            DrawGeneralGeneralConfig();

                            break;
                    }

                    break;

                case "Individual Unitframes":
                    switch (subConfig)
                    {
                        case "General":
                            DrawIndividualUnitFramesGeneralConfig();

                            break;

                        case "Player":
                            DrawIndividualUnitFramesPlayerConfig();

                            break;

                        case "Target":
                            DrawIndividualUnitFramesTargetConfig();

                            break;

                        case "Target of Target":
                            DrawIndividualUnitFramesToTConfig();

                            break;

                        case "Focus":
                            DrawIndividualUnitFramesFocusConfig();

                            break;
                    }

                    break;

                case "Group Unitframes":
                    switch (subConfig)
                    {
                        case "General":
                            DrawGroupUnitFramesGeneralConfig();

                            break;

                        case "Party":
                            DrawGroupUnitFramesPartyConfig();

                            break;

                        case "8man":
                            DrawGroupUnitFrames8ManConfig();

                            break;

                        case "24man":
                            DrawGroupUnitFrames24ManConfig();

                            break;

                        case "Enemies":
                            DrawGroupUnitFramesEnemiesConfig();

                            break;
                    }

                    break;

                case "Castbars":
                    switch (subConfig)
                    {
                        case "General":
                            DrawCastbarsGeneralConfig();

                            break;

                        case "Player":
                            DrawCastbarsPlayerConfig();

                            break;

                        case "Target":
                            DrawCastbarsTargetConfig();

                            break;
                    }

                    break;

                case "Buffs and Debuffs":
                    switch (subConfig)
                    {
                        case "Player Buffs":
                            _changed |= _pluginConfiguration.PlayerBuffListConfig.Draw();

                            break;

                        case "Player Debuffs":
                            _changed |= _pluginConfiguration.PlayerDebuffListConfig.Draw();

                            break;

                        case "Target Buffs":
                            _changed |= _pluginConfiguration.TargetBuffListConfig.Draw();

                            break;

                        case "Target Debuffs":
                            _changed |= _pluginConfiguration.TargetDebuffListConfig.Draw();

                            break;

                        case "Raid/Job Buffs":
                            DrawRaidJobBuffsConfig();

                            break;
                    }

                    break;

                case "Import/Export":
                    switch (subConfig)
                    {
                        case "General":
                            DrawImportExportGeneralConfig();

                            break;
                    }

                    break;
            }
        }

        private void DrawGeneralGeneralConfig()
        {
            _changed |= ImGui.Checkbox("Show MP Ticker", ref _pluginConfiguration.MPTickerEnabled);

            int mpTickerHeight = _pluginConfiguration.MPTickerHeight;

            if (ImGui.DragInt("MP Ticker Height", ref mpTickerHeight, .1f, 1, 1000))
            {
                _pluginConfiguration.MPTickerHeight = mpTickerHeight;
                _pluginConfiguration.Save();
            }

            int mpTickerWidth = _pluginConfiguration.MPTickerWidth;

            if (ImGui.DragInt("MP Ticker Width", ref mpTickerWidth, .1f, 1, 1000))
            {
                _pluginConfiguration.MPTickerWidth = mpTickerWidth;
                _pluginConfiguration.Save();
            }

            int mpTickerXOffset = _pluginConfiguration.MPTickerXOffset;

            if (ImGui.DragInt("MP Ticker X Offset", ref mpTickerXOffset, 1f, -2000, 2000))
            {
                _pluginConfiguration.MPTickerXOffset = mpTickerXOffset;
                _pluginConfiguration.Save();
            }

            int mpTickerYOffset = _pluginConfiguration.MPTickerYOffset;

            if (ImGui.DragInt("MP Ticker Y Offset", ref mpTickerYOffset, 1f, -2000, 2000))
            {
                _pluginConfiguration.MPTickerYOffset = mpTickerYOffset;
                _pluginConfiguration.Save();
            }

            _changed |= ImGui.Checkbox("Show MP Ticker border", ref _pluginConfiguration.MPTickerShowBorder);
            _changed |= ImGui.Checkbox("Hide MP Ticker on full mp", ref _pluginConfiguration.MPTickerHideOnFullMp);
            _changed |= ImGui.ColorEdit4("MP Ticker Color", ref _pluginConfiguration.MPTickerColor);

            // gcd indicator
            _changed |= ImGui.Checkbox("Show GCD Indicator", ref _pluginConfiguration.GCDIndicatorEnabled);
            _changed |= ImGui.Checkbox("Always Show GCD Indicator", ref _pluginConfiguration.GCDAlwaysShow);

            if (ImGui.Checkbox("Vertical GCD Indicator", ref _pluginConfiguration.GCDIndicatorVertical))
            {
                int __temp = _pluginConfiguration.GCDIndicatorWidth;
                _pluginConfiguration.GCDIndicatorWidth = _pluginConfiguration.GCDIndicatorHeight;
                _pluginConfiguration.GCDIndicatorHeight = __temp;
            }

            int gcdIndicatorHeight = _pluginConfiguration.GCDIndicatorHeight;

            if (ImGui.DragInt("GCD Indicator Height", ref gcdIndicatorHeight, .1f, 1, 1000))
            {
                _pluginConfiguration.GCDIndicatorHeight = gcdIndicatorHeight;
                _pluginConfiguration.Save();
            }

            int gcdIndicatorWidth = _pluginConfiguration.GCDIndicatorWidth;

            if (ImGui.DragInt("GCD Indicator Width", ref gcdIndicatorWidth, .1f, 1, 1000))
            {
                _pluginConfiguration.GCDIndicatorWidth = gcdIndicatorWidth;
                _pluginConfiguration.Save();
            }

            int gcdIndicatorXOffset = _pluginConfiguration.GCDIndicatorXOffset;

            if (ImGui.DragInt("GCD Indicator X Offset", ref gcdIndicatorXOffset, 1f, -2000, 2000))
            {
                _pluginConfiguration.GCDIndicatorXOffset = gcdIndicatorXOffset;
                _pluginConfiguration.Save();
            }

            int gcdIndicatorYOffset = _pluginConfiguration.GCDIndicatorYOffset;

            if (ImGui.DragInt("GCD Indicator Y Offset", ref gcdIndicatorYOffset, 1f, -2000, 2000))
            {
                _pluginConfiguration.GCDIndicatorYOffset = gcdIndicatorYOffset;
                _pluginConfiguration.Save();
            }

            _changed |= ImGui.Checkbox("Show GCD Indicator border", ref _pluginConfiguration.GCDIndicatorShowBorder);
            _changed |= ImGui.ColorEdit4("GCD Indicator Color", ref _pluginConfiguration.GCDIndicatorColor);
        }

        private void DrawIndividualUnitFramesGeneralConfig()
        {
            ImGui.Text("Colors");
            ImGui.BeginGroup();

            {
                ImGui.BeginGroup(); // Left

                {
                    ImGui.BeginChild("leftpane", new Vector2(150, ImGui.GetWindowHeight() / 4), true);

                    foreach (string colorType in _configColorMap)
                    {
                        if (ImGui.Selectable(colorType, _selectedColorType == colorType))
                        {
                            _selectedColorType = colorType;
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndGroup();

                ImGui.SameLine();

                // Right
                ImGui.BeginGroup();

                {
                    ImGui.BeginChild("itemview", new Vector2(0, ImGui.GetWindowHeight() / 4)); // Leave room for 1 line below us

                    {
                        ImGui.BeginChild("subconfig value", new Vector2(0, 0), true);
                        ImGui.BeginChild("leftpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false);

                        switch (_selectedColorType)
                        {
                            case "Tanks":
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Paladin");
                                _changed |= ImGui.ColorEdit4("##JobColorPLD", ref _pluginConfiguration.JobColorPLD);

                                ImGui.Text(""); //SPACING
                                ImGui.Text("Warrior");
                                _changed |= ImGui.ColorEdit4("##JobColorWAR", ref _pluginConfiguration.JobColorWAR);

                                ImGui.EndChild();
                                ImGui.SameLine();
                                ImGui.BeginChild("leftp3ane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false);

                                ImGui.Text(""); //SPACING
                                ImGui.Text("Dark Knight");
                                _changed |= ImGui.ColorEdit4("##JobColorDRK", ref _pluginConfiguration.JobColorDRK);

                                ImGui.Text(""); //SPACING
                                ImGui.Text("Gunbreaker");
                                _changed |= ImGui.ColorEdit4("##JobColorGNB", ref _pluginConfiguration.JobColorGNB);

                                break;

                            case "Healers":

                                ImGui.Text(""); //SPACING
                                ImGui.Text("Scholar");
                                _changed |= ImGui.ColorEdit4("##JobColorSCH", ref _pluginConfiguration.JobColorSCH);

                                ImGui.Text(""); //SPACING
                                ImGui.Text("White Mage");
                                _changed |= ImGui.ColorEdit4("##JobColorWHM", ref _pluginConfiguration.JobColorWHM);

                                ImGui.EndChild();
                                ImGui.SameLine();
                                ImGui.BeginChild("leftpa2ne", new Vector2(ImGui.GetWindowWidth() / 2, 0), false);

                                ImGui.Text(""); //SPACING
                                ImGui.Text("Astrologian");
                                _changed |= ImGui.ColorEdit4("##JobColorAST", ref _pluginConfiguration.JobColorAST);

                                break;

                            case "Melee":
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Monk");
                                _changed |= ImGui.ColorEdit4("##JobColorMNK", ref _pluginConfiguration.JobColorMNK);
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Dragoon");
                                _changed |= ImGui.ColorEdit4("##JobColorDRG", ref _pluginConfiguration.JobColorDRG);
                                ImGui.EndChild();
                                ImGui.SameLine();
                                ImGui.BeginChild("leftpa2ne", new Vector2(ImGui.GetWindowWidth() / 2, 0), false);
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Ninja");
                                _changed |= ImGui.ColorEdit4("##JobColorNIN", ref _pluginConfiguration.JobColorNIN);
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Samurai");
                                _changed |= ImGui.ColorEdit4("##JobColorSAM", ref _pluginConfiguration.JobColorSAM);

                                break;

                            case "Ranged":
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Machinist");
                                _changed |= ImGui.ColorEdit4("##JobColorMCH", ref _pluginConfiguration.JobColorMCH);

                                ImGui.Text(""); //SPACING
                                ImGui.Text("Bard");
                                _changed |= ImGui.ColorEdit4("##JobColorBRD", ref _pluginConfiguration.JobColorBRD);

                                ImGui.EndChild();
                                ImGui.SameLine();
                                ImGui.BeginChild("leftpa2ne", new Vector2(ImGui.GetWindowWidth() / 2, 0), false);
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Dancer");
                                _changed |= ImGui.ColorEdit4("##JobColorDNC", ref _pluginConfiguration.JobColorDNC);

                                break;

                            case "Casters":
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Black Mage");
                                _changed |= ImGui.ColorEdit4("##JobColorBLM", ref _pluginConfiguration.JobColorBLM);
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Summoner");
                                _changed |= ImGui.ColorEdit4("##JobColorSMN", ref _pluginConfiguration.JobColorSMN);
                                ImGui.EndChild();
                                ImGui.SameLine();
                                ImGui.BeginChild("leftpa2ne", new Vector2(ImGui.GetWindowWidth() / 2, 0), false);
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Red Mage");
                                _changed |= ImGui.ColorEdit4("##JobColorRDM", ref _pluginConfiguration.JobColorRDM);
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Blue Mage");
                                _changed |= ImGui.ColorEdit4("##JobColorBLU", ref _pluginConfiguration.JobColorBLU);

                                break;

                            case "NPC":

                                ImGui.Text(""); //SPACING
                                ImGui.Text("Hostile");
                                _changed |= ImGui.ColorEdit4("##NPCColorHostile", ref _pluginConfiguration.NPCColorHostile);
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Neutral");
                                _changed |= ImGui.ColorEdit4("##NPCColorNeutral", ref _pluginConfiguration.NPCColorNeutral);
                                ImGui.EndChild();
                                ImGui.SameLine();
                                ImGui.BeginChild("leftpa2ne", new Vector2(ImGui.GetWindowWidth() / 2, 0), false);
                                ImGui.Text(""); //SPACING
                                ImGui.Text("Friendly");
                                _changed |= ImGui.ColorEdit4("##NPCColorFriendly", ref _pluginConfiguration.NPCColorFriendly);

                                break;
                        }

                        ImGui.EndChild();
                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }

                ImGui.EndGroup();
            }

            ImGui.EndGroup();
            ImGui.Text(""); //SPACING
            _changed |= ImGui.Checkbox("Shields", ref _pluginConfiguration.ShieldEnabled);
            ImGui.BeginGroup();
            ImGui.BeginChild("itemview2", new Vector2(0, ImGui.GetWindowHeight() / 5), true);

            {
                ImGui.BeginChild("itemvi2213ew2", new Vector2(ImGui.GetWindowWidth() / 2, 0));

                {
                    int shieldHeight = _pluginConfiguration.ShieldHeight;
                    ImGui.Text(""); //SPACING
                    ImGui.Text("Height");

                    if (ImGui.DragInt("##ShieldHeight", ref shieldHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ShieldHeight = shieldHeight;
                        _pluginConfiguration.Save();
                    }

                    ImGui.SameLine();
                    _changed |= ImGui.Checkbox("in pixels", ref _pluginConfiguration.ShieldHeightPixels);
                }

                ImGui.EndChild();
                ImGui.SameLine();
                ImGui.BeginChild("ite4123mview2", new Vector2(ImGui.GetWindowWidth() / 2, 0));

                {
                    ImGui.Text(""); //SPACING
                    ImGui.Text("Color");
                    _changed |= ImGui.ColorEdit4("##ShieldColor", ref _pluginConfiguration.ShieldColor);
                }

                ImGui.EndChild();
            }

            ImGui.EndChild();

            ImGui.EndGroup();
        }

        private void DrawIndividualUnitFramesPlayerConfig()
        {
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled); //TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0, ImGui.GetWindowHeight() / 4), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("hpsizepane", new Vector2(0, ImGui.GetWindowHeight()), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Height");
                        int healthBarHeight = _pluginConfiguration.HealthBarHeight;

                        if (ImGui.DragInt("##HealthBarHeight", ref healthBarHeight, .1f, 1, 1000))
                        {
                            _pluginConfiguration.HealthBarHeight = healthBarHeight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("X Offset");
                        int healthBarXOffset = _pluginConfiguration.HealthBarXOffset;

                        if (ImGui.DragInt("##HealthBarXOffset", ref healthBarXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.HealthBarXOffset = healthBarXOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Width");
                        int healthBarWidth = _pluginConfiguration.HealthBarWidth;

                        if (ImGui.DragInt("##HealthBarWidth", ref healthBarWidth, .1f, 1, 1000))
                        {
                            _pluginConfiguration.HealthBarWidth = healthBarWidth;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Y Offset");
                        int healthBarYOffset = _pluginConfiguration.HealthBarYOffset;

                        if (ImGui.DragInt("##HealthBarYOffset", ref healthBarYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.HealthBarYOffset = healthBarYOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();
                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();

            ImGui.Text(""); //SPACING

            ImGui.BeginGroup();
            ImGui.BeginGroup();

            {
                ImGui.Text("Colors");
                ImGui.BeginChild("hppane2", new Vector2(0, ImGui.GetWindowHeight() / 4), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("hpcolorpane", new Vector2(0, ImGui.GetWindowHeight()), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("healthbarcolors", new Vector2(0, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        _changed |= ImGui.Checkbox("Custom Foreground Color", ref _pluginConfiguration.CustomHealthBarColorEnabled);
                        _changed |= ImGui.ColorEdit4("##CustomHealthBarColor", ref _pluginConfiguration.CustomHealthBarColor);

                        ImGui.Text(""); //SPACING

                        _changed |= ImGui.Checkbox("Custom Background Color", ref _pluginConfiguration.CustomHealthBarBackgroundColorEnabled);
                        _changed |= ImGui.ColorEdit4("##CustomHealthBarBackgroundColor", ref _pluginConfiguration.CustomHealthBarBackgroundColor);
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();
                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();

            ImGui.Text(""); //SPACING

            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hptxtpane", new Vector2(0, ImGui.GetWindowHeight() / 2), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                ImGui.BeginChild("hptxtformatpane", new Vector2(0, ImGui.GetWindowHeight()), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Left Text Format");
                        string healthBarTextLeft = _pluginConfiguration.HealthBarTextLeft;

                        if (ImGui.InputText("##HealthBarTextLeft", ref healthBarTextLeft, 999))
                        {
                            _pluginConfiguration.HealthBarTextLeft = healthBarTextLeft;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Left Text X Offset");
                        int healthBarTextLeftXOffset = _pluginConfiguration.HealthBarTextLeftXOffset;

                        if (ImGui.DragInt("##HealthBarTextLeftXOffset", ref healthBarTextLeftXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.HealthBarTextLeftXOffset = healthBarTextLeftXOffset;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Left Text Y Offset");
                        int healthBarTextLeftYOffset = _pluginConfiguration.HealthBarTextLeftYOffset;

                        if (ImGui.DragInt("##HealthBarTextLeftYOffset", ref healthBarTextLeftYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.HealthBarTextLeftYOffset = healthBarTextLeftYOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hptxtformatrightpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Right Text Format");
                        string healthBarTextRight = _pluginConfiguration.HealthBarTextRight;

                        if (ImGui.InputText("##HealthBarTextRight", ref healthBarTextRight, 999))
                        {
                            _pluginConfiguration.HealthBarTextRight = healthBarTextRight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Right Text X Offset");
                        int healthBarTextRightXOffset = _pluginConfiguration.HealthBarTextRightXOffset;

                        if (ImGui.DragInt("##HealthBarTextRightXOffset", ref healthBarTextRightXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.HealthBarTextRightXOffset = healthBarTextRightXOffset;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Right Text Y Offset");
                        int healthBarTextRightYOffset = _pluginConfiguration.HealthBarTextRightYOffset;

                        if (ImGui.DragInt("##HealthBarTextRightYOffset", ref healthBarTextRightYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.HealthBarTextRightYOffset = healthBarTextRightYOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();
                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();
        }

        private void DrawIndividualUnitFramesTargetConfig()
        {
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled); //TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0, ImGui.GetWindowHeight() / 3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("hpsizepane", new Vector2(0, ImGui.GetWindowHeight()), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Height");
                        int targetBarHeight = _pluginConfiguration.TargetBarHeight;

                        if (ImGui.DragInt("##TargetBarHeight", ref targetBarHeight, .1f, 1, 1000))
                        {
                            _pluginConfiguration.TargetBarHeight = targetBarHeight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("X Offset");
                        int targetBarXOffset = _pluginConfiguration.TargetBarXOffset;

                        if (ImGui.DragInt("##TargetBarXOffset", ref targetBarXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.TargetBarXOffset = targetBarXOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Width");
                        int targetBarWidth = _pluginConfiguration.TargetBarWidth;

                        if (ImGui.DragInt("##TargetBarWidth", ref targetBarWidth, .1f, 1, 1000))
                        {
                            _pluginConfiguration.TargetBarWidth = targetBarWidth;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Y Offset");
                        int targetBarYOffset = _pluginConfiguration.TargetBarYOffset;

                        if (ImGui.DragInt("##TargetBarYOffset", ref targetBarYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.TargetBarYOffset = targetBarYOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();

                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();
            ImGui.Text(""); //SPACING

            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hptxtpane", new Vector2(0, ImGui.GetWindowHeight() / 2), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                ImGui.BeginChild("hptxtformatpane", new Vector2(0, ImGui.GetWindowHeight()), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Left Text Format");
                        string targetBarTextLeft = _pluginConfiguration.TargetBarTextLeft;

                        if (ImGui.InputText("##TargetBarTextLeft", ref targetBarTextLeft, 999))
                        {
                            _pluginConfiguration.TargetBarTextLeft = targetBarTextLeft;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Left Text X Offset");
                        int targetBarTextLeftXOffset = _pluginConfiguration.TargetBarTextLeftXOffset;

                        if (ImGui.DragInt("##TargetBarTextLeftXOffset", ref targetBarTextLeftXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.TargetBarTextLeftXOffset = targetBarTextLeftXOffset;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Left Text Y Offset");
                        int targetBarTextLeftYOffset = _pluginConfiguration.TargetBarTextLeftYOffset;

                        if (ImGui.DragInt("##TargetBarTextLeftYOffset", ref targetBarTextLeftYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.TargetBarTextLeftYOffset = targetBarTextLeftYOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hptxtformatrightpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Right Text Format");
                        string targetBarTextRight = _pluginConfiguration.TargetBarTextRight;

                        if (ImGui.InputText("##TargetBarTextRight", ref targetBarTextRight, 999))
                        {
                            _pluginConfiguration.TargetBarTextRight = targetBarTextRight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Right Text X Offset");
                        int targetBarTextRightXOffset = _pluginConfiguration.TargetBarTextRightXOffset;

                        if (ImGui.DragInt("##TargetBarTextRightXOffset", ref targetBarTextRightXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.TargetBarTextRightXOffset = targetBarTextRightXOffset;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Right Text Y Offset");
                        int targetBarTextRightYOffset = _pluginConfiguration.TargetBarTextRightYOffset;

                        if (ImGui.DragInt("##TargetBarTextRightYOffset", ref targetBarTextRightYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.TargetBarTextRightYOffset = targetBarTextRightYOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();

                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();
        }

        private void DrawIndividualUnitFramesToTConfig()
        {
            _changed |= ImGui.Checkbox("Enabled", ref _pluginConfiguration.ShowTargetOfTargetBar);
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0, ImGui.GetWindowHeight() / 3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("hpsizepane", new Vector2(0, ImGui.GetWindowHeight()), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Height");
                        int toTBarHeight = _pluginConfiguration.ToTBarHeight;

                        if (ImGui.DragInt("##ToTBarHeight", ref toTBarHeight, .1f, 1, 1000))
                        {
                            _pluginConfiguration.ToTBarHeight = toTBarHeight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("X Offset");
                        int toTBarXOffset = _pluginConfiguration.ToTBarXOffset;

                        if (ImGui.DragInt("##ToTBarXOffset", ref toTBarXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.ToTBarXOffset = toTBarXOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Width");
                        int toTBarWidth = _pluginConfiguration.ToTBarWidth;

                        if (ImGui.DragInt("##ToTBarWidth", ref toTBarWidth, .1f, 1, 1000))
                        {
                            _pluginConfiguration.ToTBarWidth = toTBarWidth;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Y Offset");
                        int toTBarYOffset = _pluginConfiguration.ToTBarYOffset;

                        if (ImGui.DragInt("##ToTBarYOffset", ref toTBarYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.ToTBarYOffset = toTBarYOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();

                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();
            ImGui.Text(""); //SPACING
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hpp2ane", new Vector2(0, ImGui.GetWindowHeight() / 3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("castbar2color", new Vector2(0, ImGui.GetWindowHeight() / 3), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.Text(""); //SPACING
                    ImGui.Text("Text Format");
                    string toTBarText = _pluginConfiguration.ToTBarText;

                    if (ImGui.InputText("##ToTBarText", ref toTBarText, 999))
                    {
                        _pluginConfiguration.ToTBarText = toTBarText;
                        _pluginConfiguration.Save();
                    }
                }

                ImGui.EndChild();
                ImGui.Text(""); //SPACING

                ImGui.BeginChild("hpsize2pane", new Vector2(0, ImGui.GetWindowHeight() / 2), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hpheight2pane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING
                        ImGui.Text("X Offset");
                        int toTBarTextXOffset = _pluginConfiguration.ToTBarTextXOffset;

                        if (ImGui.DragInt("##ToTBarTextXOffset", ref toTBarTextXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.ToTBarTextXOffset = toTBarTextXOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hpwidt2hpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Text Y Offset");
                        int toTBarTextYOffset = _pluginConfiguration.ToTBarTextYOffset;

                        if (ImGui.DragInt("##ToTBarTextYOffset", ref toTBarTextYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.ToTBarTextYOffset = toTBarTextYOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();

                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();
        }

        private void DrawIndividualUnitFramesFocusConfig()
        {
            _changed |= ImGui.Checkbox("Enabled", ref _pluginConfiguration.ShowFocusBar);
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0, ImGui.GetWindowHeight() / 3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("hpsizepane", new Vector2(0, ImGui.GetWindowHeight()), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Height");
                        int focusBarHeight = _pluginConfiguration.FocusBarHeight;

                        if (ImGui.DragInt("##FocusBarHeight", ref focusBarHeight, .1f, 1, 1000))
                        {
                            _pluginConfiguration.FocusBarHeight = focusBarHeight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("X Offset");
                        int focusBarXOffset = _pluginConfiguration.FocusBarXOffset;

                        if (ImGui.DragInt("##FocusBarXOffset", ref focusBarXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.FocusBarXOffset = focusBarXOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Width");
                        int focusBarWidth = _pluginConfiguration.FocusBarWidth;

                        if (ImGui.DragInt("##FocusBarWidth", ref focusBarWidth, .1f, 1, 1000))
                        {
                            _pluginConfiguration.FocusBarWidth = focusBarWidth;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Y Offset");
                        int focusBarYOffset = _pluginConfiguration.FocusBarYOffset;

                        if (ImGui.DragInt("##FocusBarYOffset", ref focusBarYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.FocusBarYOffset = focusBarYOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();

                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();
            ImGui.Text(""); //SPACING
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hpp2ane", new Vector2(0, ImGui.GetWindowHeight() / 3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("castbar2color", new Vector2(0, ImGui.GetWindowHeight() / 3), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.Text(""); //SPACING
                    ImGui.Text("Text Format");
                    string focusBarText = _pluginConfiguration.FocusBarText;

                    if (ImGui.InputText("##FocusBarText", ref focusBarText, 999))
                    {
                        _pluginConfiguration.FocusBarText = focusBarText;
                        _pluginConfiguration.Save();
                    }
                }

                ImGui.EndChild();
                ImGui.Text(""); //SPACING

                ImGui.BeginChild("hpsize2pane", new Vector2(0, ImGui.GetWindowHeight() / 2), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hpheight2pane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING
                        ImGui.Text("X Offset");
                        int focusBarTextXOffset = _pluginConfiguration.FocusBarTextXOffset;

                        if (ImGui.DragInt("##FocusBarTextXOffset", ref focusBarTextXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.FocusBarTextXOffset = focusBarTextXOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hpwidt2hpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Text Y Offset");
                        int focusBarTextYOffset = _pluginConfiguration.FocusBarTextYOffset;

                        if (ImGui.DragInt("##FocusBarTextYOffset", ref focusBarTextYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.FocusBarTextYOffset = focusBarTextYOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();

                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();
        }

        private void DrawCastbarsPlayerConfig()
        {
            _changed |= ImGui.Checkbox("Enable Player Castbar", ref _pluginConfiguration.ShowCastBar);
            _changed |= ImGui.Checkbox("Sample Player Castbar", ref _pluginConfiguration.ShowTestCastBar);

            if (!_pluginConfiguration.ShowCastBar)
            {
                _pluginConfiguration.ShowTestCastBar = false;
            }

            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0, ImGui.GetWindowHeight() / 2), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("hpsizepane", new Vector2(0, ImGui.GetWindowHeight() / 2), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Height");
                        int castBarHeight = _pluginConfiguration.CastBarHeight;

                        if (ImGui.DragInt("##CastBarHeight", ref castBarHeight, .1f, 1, 1000))
                        {
                            _pluginConfiguration.CastBarHeight = castBarHeight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("X Offset");
                        int castBarXOffset = _pluginConfiguration.CastBarXOffset;

                        if (ImGui.DragInt("##CastBarXOffset", ref castBarXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.CastBarXOffset = castBarXOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING

                        ImGui.Text("Width");
                        int castBarWidth = _pluginConfiguration.CastBarWidth;

                        if (ImGui.DragInt("##CastBarWidth", ref castBarWidth, .1f, 1, 1000))
                        {
                            _pluginConfiguration.CastBarWidth = castBarWidth;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Y Offset");
                        int castBarYOffset = _pluginConfiguration.CastBarYOffset;

                        if (ImGui.DragInt("##CastBarYOffset", ref castBarYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.CastBarYOffset = castBarYOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();
                ImGui.Text(""); //SPACING

                ImGui.BeginChild("castbarcolor", new Vector2(0, ImGui.GetWindowHeight() / 3), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.Text("Color");
                    _changed |= ImGui.ColorEdit4("##CastBarColor", ref _pluginConfiguration.CastBarColor);
                }

                ImGui.EndChild();
                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();
            ImGui.Text(""); //SPACING
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Other Options");
                ImGui.BeginChild("otheroptions", new Vector2(0, ImGui.GetWindowHeight() / 4), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("otheroptions1", new Vector2(0, ImGui.GetWindowHeight()), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("otheroptions2", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING
                        _changed |= ImGui.Checkbox("Show Action Icon", ref _pluginConfiguration.ShowActionIcon);
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("otheroptions3", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING
                        _changed |= ImGui.Checkbox("Show Action Name", ref _pluginConfiguration.ShowActionName);
                        ImGui.Text(""); //SPACING
                        _changed |= ImGui.Checkbox("Show Cast Time", ref _pluginConfiguration.ShowCastTime);
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();

                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();
            ImGui.Text(""); //SPACING
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                _changed |= ImGui.Checkbox("SlideCast", ref _pluginConfiguration.SlideCast);
                ImGui.BeginChild("hptxtpane", new Vector2(0, ImGui.GetWindowHeight() / 3), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                ImGui.BeginChild("hptxtformatpane", new Vector2(0, ImGui.GetWindowHeight()), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth(), 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text(""); //SPACING
                        ImGui.Text("Offset");
                        float slideCastTime = _pluginConfiguration.SlideCastTime;

                        if (ImGui.DragFloat("##SlideCastTime", ref slideCastTime, 1, 1, 1000))
                        {
                            _pluginConfiguration.SlideCastTime = slideCastTime;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING
                        ImGui.Text("Color");
                        _changed |= ImGui.ColorEdit4("##SlideCastColor", ref _pluginConfiguration.SlideCastColor);
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();
                ImGui.EndChild();
            }

            ImGui.EndGroup();
            ImGui.EndGroup();
        }

        private void DrawImportExportGeneralConfig()
        {
            ImGui.BeginGroup();

            {
                uint maxLength = 40000;
                ImGui.BeginChild("importpane", new Vector2(0, ImGui.GetWindowHeight() / 4), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                {
                    ImGui.Text("Import string:");
                    ImGui.InputText("", ref _importString, maxLength);

                    if (ImGui.Button("Import configuration"))
                    {
                        PluginConfiguration importedConfig = PluginConfiguration.LoadImportString(_importString.Trim());

                        if (importedConfig != null)
                        {
                            _pluginConfiguration.TransferConfig(importedConfig);
                            _changed = true;
                        }
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Paste from clipboard"))
                    {
                        _importString = ImGui.GetClipboardText();
                    }
                }

                ImGui.EndChild();

                ImGui.BeginChild("exportpane", new Vector2(0, ImGui.GetWindowHeight() / 4), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                {
                    ImGui.Text("Export string:");
                    ImGui.InputText("", ref _exportString, maxLength, ImGuiInputTextFlags.ReadOnly);

                    if (ImGui.Button("Export configuration"))
                    {
                        _exportString = PluginConfiguration.GenerateExportString(_pluginConfiguration);
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Copy to clipboard") && _exportString != "")
                    {
                        ImGui.SetClipboardText(_exportString);
                    }
                }

                ImGui.EndChild();
            }

            ImGui.EndGroup();
        }

        private void DrawGroupUnitFramesGeneralConfig() { }

        private void DrawGroupUnitFramesPartyConfig() { }

        private void DrawGroupUnitFrames8ManConfig() { }

        private void DrawGroupUnitFrames24ManConfig() { }

        private void DrawGroupUnitFramesEnemiesConfig() { }

        private void DrawCastbarsGeneralConfig() { }

        private void DrawCastbarsTargetConfig()
        {
            _changed |= ImGui.Checkbox("Enable Target Castbar", ref _pluginConfiguration.ShowTargetCastBar);
            _changed |= ImGui.Checkbox("Sample Target Castbar", ref _pluginConfiguration.ShowTargetTestCastBar);

            if (!_pluginConfiguration.ShowTargetTestCastBar)
            {
                _pluginConfiguration.ShowTargetTestCastBar = false;
            }

            int targetCastBarHeight = _pluginConfiguration.TargetCastBarHeight;

            if (ImGui.DragInt("Target Castbar Height", ref targetCastBarHeight, .1f, 1, 1000))
            {
                _pluginConfiguration.TargetCastBarHeight = targetCastBarHeight;
                _pluginConfiguration.Save();
            }

            int targetCastBarWidth = _pluginConfiguration.TargetCastBarWidth;

            if (ImGui.DragInt("Target Castbar Width", ref targetCastBarWidth, .1f, 1, 1000))
            {
                _pluginConfiguration.TargetCastBarWidth = targetCastBarWidth;
                _pluginConfiguration.Save();
            }

            int targetCastBarXOffset = _pluginConfiguration.TargetCastBarXOffset;

            if (ImGui.DragInt("Target Castbar X Offset", ref targetCastBarXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
            {
                _pluginConfiguration.TargetCastBarXOffset = targetCastBarXOffset;
                _pluginConfiguration.Save();
            }

            int targetCastBarYOffset = _pluginConfiguration.TargetCastBarYOffset;

            if (ImGui.DragInt("Target Castbar Y Offset", ref targetCastBarYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
            {
                _pluginConfiguration.TargetCastBarYOffset = targetCastBarYOffset;
                _pluginConfiguration.Save();
            }

            _changed |= ImGui.Checkbox("Color Target CastBar by Damage Type", ref _pluginConfiguration.ColorCastBarByDamageType);

            _changed |= ImGui.ColorEdit4("Target Castbar Color", ref _pluginConfiguration.TargetCastBarColor);
            _changed |= ImGui.ColorEdit4("Interruptable Cast Color", ref _pluginConfiguration.TargetCastBarInterruptColor);
            _changed |= ImGui.ColorEdit4("Physical Cast Color", ref _pluginConfiguration.TargetCastBarPhysicalColor);
            _changed |= ImGui.ColorEdit4("Magical Cast Color", ref _pluginConfiguration.TargetCastBarMagicalColor);
            _changed |= ImGui.ColorEdit4("Darkness Cast Color", ref _pluginConfiguration.TargetCastBarDarknessColor);

            _changed |= ImGui.Checkbox("Show Target Action Icon", ref _pluginConfiguration.ShowTargetActionIcon);
            _changed |= ImGui.Checkbox("Show Target Action Name", ref _pluginConfiguration.ShowTargetActionName);
            _changed |= ImGui.Checkbox("Show Target Cast Time", ref _pluginConfiguration.ShowTargetCastTime);
            _changed |= ImGui.Checkbox("Show Interruptable Casts", ref _pluginConfiguration.ShowTargetInterrupt);
        }

        private void DrawRaidJobBuffsConfig()
        {
            _changed |= ImGui.Checkbox("Show Raid wide buff icons", ref _pluginConfiguration.ShowRaidWideBuffIcons);
            _changed |= ImGui.Checkbox("Show Job specific buff icons", ref _pluginConfiguration.ShowJobSpecificBuffIcons);
            _changed |= _pluginConfiguration.RaidJobBuffListConfig.Draw();
        }
    }
}
