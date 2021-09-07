using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Numerics;

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

        public ConfigurationWindow(PluginConfiguration pluginConfiguration, DalamudPluginInterface pluginInterface)
        {
            //TODO ADD PRIMARYRESOURCEBAR TO CONFIGMAP jobs general

            _pluginConfiguration = pluginConfiguration;
            _pluginInterface = pluginInterface;
            _configMap.Add("General", new[] { "General" });

            _configMap.Add("Individual Unitframes", new[] { "General", "Player", "Focus", "Target", "Target of Target" });

            //configMap.Add("Group Unitframes", new [] {"General", "Party", "8man", "24man", "Enemies"});
            _configMap.Add("Castbars", new[] { "Player", "Target" });

            _configMap.Add("Buffs and Debuffs", new[] { "Player Buffs", "Player Debuffs", "Target Buffs", "Target Debuffs", "Raid/Job Buffs" });

            _configMap.Add("Job Specific Bars", new[] { "General", "Tank", "Healer", "Melee", "Ranged" });
            _configMap.Add("Import/Export", new[] { "General" });
        }

        public void ToggleHud()
        {
            _pluginConfiguration.HideHud = !_pluginConfiguration.HideHud;
            _changed = true;
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

            if (!ImGui.Begin(
                "titlebar",
                ref IsVisible,
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse
            ))
            {
                return;
            }

            _xOffsetLimit = _viewportWidth / 2;
            _yOffsetLimit = _viewportHeight / 2;
            _changed = false;
            Vector2 pos = ImGui.GetCursorPos();

            ImGui.BeginGroup();

            {
                ImGui.BeginGroup(); // Left

                {
                    TextureWrap delvUiBanner = _pluginConfiguration.BannerImage;
                    ImGui.Image(delvUiBanner.ImGuiHandle, new Vector2(delvUiBanner.Width, delvUiBanner.Height));
                    ImGui.BeginChild("left pane", new Vector2(150, -ImGui.GetFrameHeightWithSpacing()), true);

                    foreach (var config in _configMap.Keys)
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

                    ImGui.BeginChild("item view", new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us

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

            ImGui.EndGroup();
            ImGui.Separator();

            ImGui.BeginGroup();

            if (ImGui.Button("Lock HUD"))
            {
                _changed |= ImGui.Checkbox("Lock HUD", ref _pluginConfiguration.LockHud);
            }

            ImGui.SameLine();

            if (ImGui.Button(_pluginConfiguration.HideHud ? "Show HUD" : "Hide HUD"))
            {
                ToggleHud();
            }

            ImGui.SameLine();

            if (ImGui.Button("Reset HUD"))
            {
                _pluginConfiguration.TransferConfig(PluginConfiguration.ReadConfig("default", _pluginInterface));
                _changed = true;
            }

            ImGui.SameLine();

            pos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 60, ImGui.GetCursorPos().Y));

            if (ImGui.Button("Donate!"))
            { }

            ImGui.SetCursorPos(pos);
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

                case "Job Specific Bars":
                    switch (subConfig)
                    {
                        case "General":
                            DrawJobsGeneralConfig();

                            break;

                        case "Tank":
                            DrawJobsTankConfig();

                            break;

                        case "Healer":
                            DrawJobsHealerConfig();

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

        private void DrawIndividualUnitFramesGeneralConfig()
        {
            ImGui.Text("Colors");
            ImGui.BeginGroup();

            {
                ImGui.BeginGroup(); // Left

                {
                    ImGui.BeginChild("leftpane", new Vector2(150, ImGui.GetWindowHeight() / 4), true);

                    foreach (var colorType in _configColorMap)
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
                    var shieldHeight = _pluginConfiguration.ShieldHeight;
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
            var disabled = true;
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
                        var healthBarHeight = _pluginConfiguration.HealthBarHeight;

                        if (ImGui.DragInt("##HealthBarHeight", ref healthBarHeight, .1f, 1, 1000))
                        {
                            _pluginConfiguration.HealthBarHeight = healthBarHeight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("X Offset");
                        var healthBarXOffset = _pluginConfiguration.HealthBarXOffset;

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
                        var healthBarWidth = _pluginConfiguration.HealthBarWidth;

                        if (ImGui.DragInt("##HealthBarWidth", ref healthBarWidth, .1f, 1, 1000))
                        {
                            _pluginConfiguration.HealthBarWidth = healthBarWidth;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Y Offset");
                        var healthBarYOffset = _pluginConfiguration.HealthBarYOffset;

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
                        var healthBarTextLeft = _pluginConfiguration.HealthBarTextLeft;

                        if (ImGui.InputText("##HealthBarTextLeft", ref healthBarTextLeft, 999))
                        {
                            _pluginConfiguration.HealthBarTextLeft = healthBarTextLeft;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Left Text X Offset");
                        var healthBarTextLeftXOffset = _pluginConfiguration.HealthBarTextLeftXOffset;

                        if (ImGui.DragInt("##HealthBarTextLeftXOffset", ref healthBarTextLeftXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.HealthBarTextLeftXOffset = healthBarTextLeftXOffset;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Left Text Y Offset");
                        var healthBarTextLeftYOffset = _pluginConfiguration.HealthBarTextLeftYOffset;

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
                        var healthBarTextRight = _pluginConfiguration.HealthBarTextRight;

                        if (ImGui.InputText("##HealthBarTextRight", ref healthBarTextRight, 999))
                        {
                            _pluginConfiguration.HealthBarTextRight = healthBarTextRight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Right Text X Offset");
                        var healthBarTextRightXOffset = _pluginConfiguration.HealthBarTextRightXOffset;

                        if (ImGui.DragInt("##HealthBarTextRightXOffset", ref healthBarTextRightXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.HealthBarTextRightXOffset = healthBarTextRightXOffset;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Right Text Y Offset");
                        var healthBarTextRightYOffset = _pluginConfiguration.HealthBarTextRightYOffset;

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
            var disabled = true;
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
                        var targetBarHeight = _pluginConfiguration.TargetBarHeight;

                        if (ImGui.DragInt("##TargetBarHeight", ref targetBarHeight, .1f, 1, 1000))
                        {
                            _pluginConfiguration.TargetBarHeight = targetBarHeight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("X Offset");
                        var targetBarXOffset = _pluginConfiguration.TargetBarXOffset;

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
                        var targetBarWidth = _pluginConfiguration.TargetBarWidth;

                        if (ImGui.DragInt("##TargetBarWidth", ref targetBarWidth, .1f, 1, 1000))
                        {
                            _pluginConfiguration.TargetBarWidth = targetBarWidth;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Y Offset");
                        var targetBarYOffset = _pluginConfiguration.TargetBarYOffset;

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
                        var targetBarTextLeft = _pluginConfiguration.TargetBarTextLeft;

                        if (ImGui.InputText("##TargetBarTextLeft", ref targetBarTextLeft, 999))
                        {
                            _pluginConfiguration.TargetBarTextLeft = targetBarTextLeft;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Left Text X Offset");
                        var targetBarTextLeftXOffset = _pluginConfiguration.TargetBarTextLeftXOffset;

                        if (ImGui.DragInt("##TargetBarTextLeftXOffset", ref targetBarTextLeftXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.TargetBarTextLeftXOffset = targetBarTextLeftXOffset;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Left Text Y Offset");
                        var targetBarTextLeftYOffset = _pluginConfiguration.TargetBarTextLeftYOffset;

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
                        var targetBarTextRight = _pluginConfiguration.TargetBarTextRight;

                        if (ImGui.InputText("##TargetBarTextRight", ref targetBarTextRight, 999))
                        {
                            _pluginConfiguration.TargetBarTextRight = targetBarTextRight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Right Text X Offset");
                        var targetBarTextRightXOffset = _pluginConfiguration.TargetBarTextRightXOffset;

                        if (ImGui.DragInt("##TargetBarTextRightXOffset", ref targetBarTextRightXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.TargetBarTextRightXOffset = targetBarTextRightXOffset;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Right Text Y Offset");
                        var targetBarTextRightYOffset = _pluginConfiguration.TargetBarTextRightYOffset;

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
            var disabled = true;
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
                        var toTBarHeight = _pluginConfiguration.ToTBarHeight;

                        if (ImGui.DragInt("##ToTBarHeight", ref toTBarHeight, .1f, 1, 1000))
                        {
                            _pluginConfiguration.ToTBarHeight = toTBarHeight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("X Offset");
                        var toTBarXOffset = _pluginConfiguration.ToTBarXOffset;

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
                        var toTBarWidth = _pluginConfiguration.ToTBarWidth;

                        if (ImGui.DragInt("##ToTBarWidth", ref toTBarWidth, .1f, 1, 1000))
                        {
                            _pluginConfiguration.ToTBarWidth = toTBarWidth;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Y Offset");
                        var toTBarYOffset = _pluginConfiguration.ToTBarYOffset;

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
                    var toTBarText = _pluginConfiguration.ToTBarText;

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
                        var toTBarTextXOffset = _pluginConfiguration.ToTBarTextXOffset;

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
                        var toTBarTextYOffset = _pluginConfiguration.ToTBarTextYOffset;

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
            var disabled = true;
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
                        var focusBarHeight = _pluginConfiguration.FocusBarHeight;

                        if (ImGui.DragInt("##FocusBarHeight", ref focusBarHeight, .1f, 1, 1000))
                        {
                            _pluginConfiguration.FocusBarHeight = focusBarHeight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("X Offset");
                        var focusBarXOffset = _pluginConfiguration.FocusBarXOffset;

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
                        var focusBarWidth = _pluginConfiguration.FocusBarWidth;

                        if (ImGui.DragInt("##FocusBarWidth", ref focusBarWidth, .1f, 1, 1000))
                        {
                            _pluginConfiguration.FocusBarWidth = focusBarWidth;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Y Offset");
                        var focusBarYOffset = _pluginConfiguration.FocusBarYOffset;

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
                    var focusBarText = _pluginConfiguration.FocusBarText;

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
                        var focusBarTextXOffset = _pluginConfiguration.FocusBarTextXOffset;

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
                        var focusBarTextYOffset = _pluginConfiguration.FocusBarTextYOffset;

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

        private void DrawIndividualUnitFramesFocusConfigss()
        {
            var disabled = true;
            ImGui.Checkbox("Enabled", ref disabled); //TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0, ImGui.GetWindowHeight() / 2), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("hpsizepane", new Vector2(0, ImGui.GetWindowHeight() / 2), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text("Height");
                        var focusBarHeight = _pluginConfiguration.FocusBarHeight;

                        if (ImGui.DragInt("##FocusBarHeight", ref focusBarHeight, .1f, 1, 1000))
                        {
                            _pluginConfiguration.FocusBarHeight = focusBarHeight;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text("Width");
                        var focusBarWidth = _pluginConfiguration.FocusBarWidth;

                        if (ImGui.DragInt("##FocusBarWidth", ref focusBarWidth, .1f, 1, 1000))
                        {
                            _pluginConfiguration.FocusBarWidth = focusBarWidth;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();
                ImGui.Separator();
                ImGui.BeginChild("hpoffsetpane", new Vector2(0, ImGui.GetWindowHeight() / 2), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hpxpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text("X Offset");
                        var focusBarXOffset = _pluginConfiguration.FocusBarXOffset;

                        if (ImGui.DragInt("##FocusBarXOffset", ref focusBarXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.FocusBarXOffset = focusBarXOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hpypane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text("Y Offset");
                        var focusBarYOffset = _pluginConfiguration.FocusBarYOffset;

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

            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left

            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hptxtpane", new Vector2(0, ImGui.GetWindowHeight() / 2), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                ImGui.BeginChild("hptxtformatpane", new Vector2(0, ImGui.GetWindowHeight() / 2), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text("Text Format");
                        var focusBarText = _pluginConfiguration.FocusBarText;

                        if (ImGui.InputText("##FocusBarText", ref focusBarText, 999))
                        {
                            _pluginConfiguration.FocusBarText = focusBarText;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();
                }

                ImGui.EndChild();
                ImGui.Separator();
                ImGui.BeginChild("hptxtoffsetpane", new Vector2(0, ImGui.GetWindowHeight() / 2), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                {
                    ImGui.BeginChild("hptxtleftxpane", new Vector2(ImGui.GetWindowWidth() / 2, 0), false, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);

                    {
                        ImGui.Text("Text X Offset");
                        var focusBarTextXOffset = _pluginConfiguration.FocusBarTextXOffset;

                        if (ImGui.DragInt("##FocusBarTextXOffset", ref focusBarTextXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                        {
                            _pluginConfiguration.FocusBarTextXOffset = focusBarTextXOffset;
                            _pluginConfiguration.Save();
                        }
                    }

                    ImGui.EndChild();

                    ImGui.SameLine();

                    ImGui.BeginChild("hptxtleftypane", new Vector2(ImGui.GetWindowWidth() / 2, 0));

                    {
                        ImGui.Text("Text Y Offset");
                        var focusBarTextYOffset = _pluginConfiguration.FocusBarTextYOffset;

                        if (ImGui.DragInt("##FocusBarTextYOffset", ref focusBarTextYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                        {
                            _pluginConfiguration.ToTBarTextYOffset = focusBarTextYOffset;
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
            _changed |= ImGui.Checkbox("Enabled", ref _pluginConfiguration.ShowCastBar);

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
                        var castBarHeight = _pluginConfiguration.CastBarHeight;

                        if (ImGui.DragInt("##CastBarHeight", ref castBarHeight, .1f, 1, 1000))
                        {
                            _pluginConfiguration.CastBarHeight = castBarHeight;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("X Offset");
                        var castBarXOffset = _pluginConfiguration.CastBarXOffset;

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
                        var castBarWidth = _pluginConfiguration.CastBarWidth;

                        if (ImGui.DragInt("##CastBarWidth", ref castBarWidth, .1f, 1, 1000))
                        {
                            _pluginConfiguration.CastBarWidth = castBarWidth;
                            _pluginConfiguration.Save();
                        }

                        ImGui.Text(""); //SPACING

                        ImGui.Text("Y Offset");
                        var castBarYOffset = _pluginConfiguration.CastBarYOffset;

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
                        var slideCastTime = _pluginConfiguration.SlideCastTime;

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
            _changed |= ImGui.Checkbox("Show Target Cast Bar", ref _pluginConfiguration.ShowTargetCastBar);

            var targetCastBarHeight = _pluginConfiguration.TargetCastBarHeight;

            if (ImGui.DragInt("Target Castbar Height", ref targetCastBarHeight, .1f, 1, 1000))
            {
                _pluginConfiguration.TargetCastBarHeight = targetCastBarHeight;
                _pluginConfiguration.Save();
            }

            var targetCastBarWidth = _pluginConfiguration.TargetCastBarWidth;

            if (ImGui.DragInt("Target Castbar Width", ref targetCastBarWidth, .1f, 1, 1000))
            {
                _pluginConfiguration.TargetCastBarWidth = targetCastBarWidth;
                _pluginConfiguration.Save();
            }

            var targetCastBarXOffset = _pluginConfiguration.TargetCastBarXOffset;

            if (ImGui.DragInt("Target Castbar X Offset", ref targetCastBarXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
            {
                _pluginConfiguration.TargetCastBarXOffset = targetCastBarXOffset;
                _pluginConfiguration.Save();
            }

            var targetCastBarYOffset = _pluginConfiguration.TargetCastBarYOffset;

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

        private void DrawJobsGeneralConfig()
        {
            var primaryResourceHeight = _pluginConfiguration.PrimaryResourceBarHeight;

            if (ImGui.DragInt("Primary Resource Height", ref primaryResourceHeight, .1f, 1, 1000))
            {
                _pluginConfiguration.PrimaryResourceBarHeight = primaryResourceHeight;
                _pluginConfiguration.Save();
            }

            var primaryResourceWidth = _pluginConfiguration.PrimaryResourceBarWidth;

            if (ImGui.DragInt("Primary Resource Width", ref primaryResourceWidth, .1f, 1, 1000))
            {
                _pluginConfiguration.PrimaryResourceBarWidth = primaryResourceWidth;
                _pluginConfiguration.Save();
            }

            var primaryResourceBarXOffset = _pluginConfiguration.PrimaryResourceBarXOffset;

            if (ImGui.DragInt("Primary Resource X Offset", ref primaryResourceBarXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
            {
                _pluginConfiguration.PrimaryResourceBarXOffset = primaryResourceBarXOffset;
                _pluginConfiguration.Save();
            }

            var primaryResourceBarYOffset = _pluginConfiguration.PrimaryResourceBarYOffset;

            if (ImGui.DragInt("Primary Resource Y Offset", ref primaryResourceBarYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
            {
                _pluginConfiguration.PrimaryResourceBarYOffset = primaryResourceBarYOffset;
                _pluginConfiguration.Save();
            }

            _changed |= ImGui.Checkbox("Show Primary Resource Value", ref _pluginConfiguration.ShowPrimaryResourceBarValue);

            if (_pluginConfiguration.ShowPrimaryResourceBarValue)
            {
                var primaryResourceBarTextXOffset = _pluginConfiguration.PrimaryResourceBarTextXOffset;

                if (ImGui.DragInt("Primary Resource Text X Offset", ref primaryResourceBarTextXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                {
                    _pluginConfiguration.PrimaryResourceBarTextXOffset = primaryResourceBarTextXOffset;
                    _pluginConfiguration.Save();
                }

                var primaryResourceBarTextYOffset = _pluginConfiguration.PrimaryResourceBarTextYOffset;

                if (ImGui.DragInt("Primary Resource Text Y Offset", ref primaryResourceBarTextYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                {
                    _pluginConfiguration.PrimaryResourceBarTextYOffset = primaryResourceBarTextYOffset;
                    _pluginConfiguration.Save();
                }
            }

            _changed |= ImGui.Checkbox("Show Primary Resource Threshold Marker", ref _pluginConfiguration.ShowPrimaryResourceBarThresholdMarker);

            if (_pluginConfiguration.ShowPrimaryResourceBarThresholdMarker)
            {
                var primaryResourceBarThresholdValue = _pluginConfiguration.PrimaryResourceBarThresholdValue;

                if (ImGui.DragInt("Primary Resource Bar Threshold Marker Value", ref primaryResourceBarThresholdValue, 1f, 1, 10000))
                {
                    _pluginConfiguration.PrimaryResourceBarThresholdValue = primaryResourceBarThresholdValue;
                    _pluginConfiguration.Save();
                }
            }

            _changed |= ImGui.ColorEdit4("Bar Background Color", ref _pluginConfiguration.EmptyColor);
            _changed |= ImGui.ColorEdit4("Bar Partial Fill Color", ref _pluginConfiguration.PartialFillColor);
        }

        private void DrawJobsTankConfig()
        {
            if (ImGui.BeginTabBar("##tanks-tabs"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    _changed |= ImGui.Checkbox("Tank Stance Indicator Enabled", ref _pluginConfiguration.TankStanceIndicatorEnabled);

                    var tankStanceIndicatorWidth = _pluginConfiguration.TankStanceIndicatorWidth;

                    if (ImGui.DragInt("Tank Stance Indicator Width", ref tankStanceIndicatorWidth, .1f, 1, 6))
                    {
                        _pluginConfiguration.TankStanceIndicatorWidth = tankStanceIndicatorWidth;
                        _pluginConfiguration.Save();
                    }

                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        private void DrawJobsHealerConfig()
        {
            if (ImGui.BeginTabBar("##HEALERS-tabs"))
            {
                if (ImGui.BeginTabItem("Scholar"))
                {
                    var schBaseXOffset = _pluginConfiguration.SCHBaseXOffset;

                    if (ImGui.DragInt("Base X Offset", ref schBaseXOffset, .1f, -_xOffsetLimit, _xOffsetLimit))
                    {
                        _pluginConfiguration.SCHBaseXOffset = schBaseXOffset;
                        _pluginConfiguration.Save();
                    }

                    var schBaseYOffset = _pluginConfiguration.SCHBaseYOffset;

                    if (ImGui.DragInt("Base Y Offset", ref schBaseYOffset, .1f, -_yOffsetLimit, _yOffsetLimit))
                    {
                        _pluginConfiguration.SCHBaseYOffset = schBaseYOffset;
                        _pluginConfiguration.Save();
                    }

                    var fairyBarHeight = _pluginConfiguration.FairyBarHeight;

                    if (ImGui.DragInt("Fairy Gauge Height", ref fairyBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.FairyBarHeight = fairyBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var fairyBarWidth = _pluginConfiguration.FairyBarWidth;

                    if (ImGui.DragInt("Fairy Gauge Width", ref fairyBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.FairyBarWidth = fairyBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var fairyBarX = _pluginConfiguration.FairyBarX;

                    if (ImGui.DragInt("Fairy Gauge X Offset", ref fairyBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.FairyBarX = fairyBarX;
                        _pluginConfiguration.Save();
                    }

                    var fairyBarY = _pluginConfiguration.FairyBarY;

                    if (ImGui.DragInt("Fairy Gauge Y Offset", ref fairyBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.FairyBarY = fairyBarY;
                        _pluginConfiguration.Save();
                    }

                    var schAetherBarHeight = _pluginConfiguration.SchAetherBarHeight;

                    if (ImGui.DragInt("Aether Gauge Height", ref schAetherBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SchAetherBarHeight = schAetherBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var schAetherBarWidth = _pluginConfiguration.SchAetherBarWidth;

                    if (ImGui.DragInt("Aether Gauge Width", ref schAetherBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SchAetherBarWidth = schAetherBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var schAetherBarX = _pluginConfiguration.SchAetherBarX;

                    if (ImGui.DragInt("Aether Gauge X Offset", ref schAetherBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SchAetherBarX = schAetherBarX;
                        _pluginConfiguration.Save();
                    }

                    var schAetherBarY = _pluginConfiguration.SchAetherBarY;

                    if (ImGui.DragInt("Aether Gauge Y Offset", ref schAetherBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SchAetherBarY = schAetherBarY;
                        _pluginConfiguration.Save();
                    }

                    var schAetherBarPad = _pluginConfiguration.SchAetherBarPad;

                    if (ImGui.DragInt("Aether Padding", ref schAetherBarPad, .1f, -100, 1000))
                    {
                        _pluginConfiguration.SchAetherBarPad = schAetherBarPad;
                        _pluginConfiguration.Save();
                    }

                    var schBioBarHeight = _pluginConfiguration.SCHBioBarHeight;

                    if (ImGui.DragInt("Bio Bar Height", ref schBioBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SCHBioBarHeight = schBioBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var schBioBarWidth = _pluginConfiguration.SCHBioBarWidth;

                    if (ImGui.DragInt("Bio Bar Width", ref schBioBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SCHBioBarWidth = schBioBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var schBioBarX = _pluginConfiguration.SCHBioBarX;

                    if (ImGui.DragInt("Bio Bar X Offset", ref schBioBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SCHBioBarX = schBioBarX;
                        _pluginConfiguration.Save();
                    }

                    var schBioBarY = _pluginConfiguration.SCHBioBarY;

                    if (ImGui.DragInt("Bio Bar Y Offset", ref schBioBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SCHBioBarY = schBioBarY;
                        _pluginConfiguration.Save();
                    }

                    _changed |= ImGui.Checkbox("Show Aether Bar", ref _pluginConfiguration.SCHShowAetherBar);
                    _changed |= ImGui.Checkbox("Show Fairy Bar", ref _pluginConfiguration.SCHShowFairyBar);
                    _changed |= ImGui.Checkbox("Show Bio Bar", ref _pluginConfiguration.SCHShowBioBar);

                    _changed |= ImGui.Checkbox("Show Primary Resource Bar", ref _pluginConfiguration.SCHShowPrimaryResourceBar);

                    _changed |= ImGui.ColorEdit4("Fairy Bar Color", ref _pluginConfiguration.SchFairyColor);
                    _changed |= ImGui.ColorEdit4("Aether Bar Color", ref _pluginConfiguration.SchAetherColor);

                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }
    }
}
