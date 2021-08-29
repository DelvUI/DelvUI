using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface
{
    public class ConfigurationWindow
    {
        public bool IsVisible;
        private readonly Plugin _plugin;
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly PluginConfiguration _pluginConfiguration;
        private string selected = "General";
        private Dictionary<string, Array> configMap = new Dictionary<string, Array>() ;
        private bool changed;
        private int viewportWidth = (int) ImGui.GetMainViewport().Size.X;
        private int viewportHeight = (int) ImGui.GetMainViewport().Size.Y;
        private int xOffsetLimit;
        private int yOffsetLimit;
        




        public ConfigurationWindow(Plugin plugin, DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            //TODO ADD PRIMARYRESOURCEBAR TO CONFIGMAP
            //TODO ADD SHIELD TO CONFIGMAP
            //TODO ADD SHIELD TO CONFIGMAP
            _plugin = plugin;
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginConfiguration;
            configMap.Add("General", new [] {"General","Color Map"});
            configMap.Add("Individual Unitframes", new [] {"General", "Player", "Target", "Target of Target", "Focus"});
            configMap.Add("Group Unitframes", new [] {"General", "Party", "8man", "24man", "Enemies"});
            configMap.Add("Castbars", new [] {"General", "Player", "Enemy"});
            configMap.Add("Jobs", new [] {"General", "Tank", "Healer", "Melee","Ranged", "Caster"});

        }   


        public void Draw()
        {
            if (!IsVisible) {
                return;
            }
            //Todo
            //ImGui.GetIO().FontGlobalScale;
            ImGui.SetNextWindowSize(new Vector2(900, 600), ImGuiCond.Appearing);


            if (!ImGui.Begin("titlebar", ref IsVisible, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize)) {
                return;
            }
            xOffsetLimit = viewportWidth / 2;
            yOffsetLimit = viewportHeight / 2;
            changed = false;
            var pos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth()-26, 0));
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(FontAwesomeIcon.Times.ToIconString()))
            {
                IsVisible = false;
            }
            ImGui.PopFont();
            ImGui.SetCursorPos(pos);
            
            ImGui.BeginGroup();
            {
                ImGui.BeginGroup(); // Left
                {
                    var imagePath = Path.Combine(Path.GetDirectoryName(_plugin.AssemblyLocation) ?? "", "Media", "Images", "banner_short_x150.png");
                    var delvuiBanner = _pluginInterface.UiBuilder.LoadImage(imagePath);
                    ImGui.Image(delvuiBanner.ImGuiHandle, new Vector2(delvuiBanner.Width, delvuiBanner.Height));

                    ImGui.BeginChild("left pane", new Vector2(150, -ImGui.GetFrameHeightWithSpacing()), true);

                    foreach (var config in configMap.Keys)
                    {
                        if (ImGui.Selectable(config, selected == config))
                            selected = config;
                    }

                    ImGui.EndChild();


                }
                ImGui.EndGroup();

                ImGui.SameLine();

                // Right
                ImGui.BeginGroup();
                {
                    var subConfigs = configMap[selected];
                        
                    ImGui.BeginChild("item view",new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us
                    {
                        if (ImGui.BeginTabBar("##Tabs", ImGuiTabBarFlags.None))
                        {
                            foreach (string subConfig in subConfigs)
                            {

                                if (!ImGui.BeginTabItem(subConfig)) continue;
                                ImGui.BeginChild("subconfig value", new Vector2(0, 0), true);
                                DrawSubConfig(selected, subConfig);
                                ImGui.EndChild();
                                ImGui.EndTabItem();
                            }

                            ImGui.EndTabBar();
                                
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
                changed |= ImGui.Checkbox("Lock HUD", ref _pluginConfiguration.LockHud);

            }
            ImGui.SameLine();
            if (ImGui.Button("Hide HUD"))
            {
                changed |= ImGui.Checkbox("Hide HUD", ref _pluginConfiguration.HideHud);

            }                
            ImGui.SameLine();
            if (ImGui.Button("Reset HUD")) {}
                 

            ImGui.EndGroup();
                

            if (changed)
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
                        case "Color Map":
                            DrawGeneralColorMapConfig();
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
                            DrawGroupUnitFrames8manConfig();
                            break;                        
                        case "24man":
                            DrawGroupUnitFrames24manConfig();
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
                        case "Enemy":
                            DrawCastbarsEnemyConfig();
                            break;
                    }
                    break;
                case "Jobs":
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
                        case "Melee":
                            DrawJobsMeleeConfig();
                            break;                        
                        case "Ranged":
                            DrawJobsRangedConfig();
                            break;                        
                        case "Caster":
                            DrawJobsCasterConfig();
                            break;
                    }
                    break;
            }
            
        }

        private void DrawGeneralGeneralConfig()
        {
            ImGui.Text("No Configs Yet");
        }

        private void DrawGeneralColorMapConfig()
        {
            if (ImGui.BeginTabBar("##ColorTabs", ImGuiTabBarFlags.None))
            {
                if (ImGui.BeginTabItem("Tanks"))
                {
                    changed |= ImGui.ColorEdit4("Paladin", ref _pluginConfiguration.JobColorPLD);
                    changed |= ImGui.ColorEdit4("Warrior", ref _pluginConfiguration.JobColorWAR);
                    changed |= ImGui.ColorEdit4("Dark Knight", ref _pluginConfiguration.JobColorDRK);
                    changed |= ImGui.ColorEdit4("Gunbreaker", ref _pluginConfiguration.JobColorGNB);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Healers"))
                {
                    changed |= ImGui.ColorEdit4("White Mage", ref _pluginConfiguration.JobColorWHM);
                    changed |= ImGui.ColorEdit4("Scholar", ref _pluginConfiguration.JobColorSCH);
                    changed |= ImGui.ColorEdit4("Astrologian", ref _pluginConfiguration.JobColorAST);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Melee"))
                {
                    changed |= ImGui.ColorEdit4("Monk", ref _pluginConfiguration.JobColorMNK);
                    changed |= ImGui.ColorEdit4("Dragoon", ref _pluginConfiguration.JobColorDRG);
                    changed |= ImGui.ColorEdit4("Ninja", ref _pluginConfiguration.JobColorNIN);
                    changed |= ImGui.ColorEdit4("Samurai", ref _pluginConfiguration.JobColorSAM);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Ranged"))
                {
                    changed |= ImGui.ColorEdit4("Bard", ref _pluginConfiguration.JobColorBRD);
                    changed |= ImGui.ColorEdit4("Machinist", ref _pluginConfiguration.JobColorMCH);
                    changed |= ImGui.ColorEdit4("Dancer", ref _pluginConfiguration.JobColorDNC);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Casters"))
                {
                    changed |= ImGui.ColorEdit4("Black Mage", ref _pluginConfiguration.JobColorBLM);
                    changed |= ImGui.ColorEdit4("Summoner", ref _pluginConfiguration.JobColorSMN);
                    changed |= ImGui.ColorEdit4("Red Mage", ref _pluginConfiguration.JobColorRDM);
                    changed |= ImGui.ColorEdit4("Blue Mage", ref _pluginConfiguration.JobColorBLU);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("NPC"))
                {
                    changed |= ImGui.ColorEdit4("Hostile", ref _pluginConfiguration.NPCColorHostile);
                    changed |= ImGui.ColorEdit4("Neutral", ref _pluginConfiguration.NPCColorNeutral);
                    changed |= ImGui.ColorEdit4("Friendly", ref _pluginConfiguration.NPCColorFriendly);
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
            
        }

        private void DrawIndividualUnitFramesGeneralConfig()
        {
            
        }

        private void DrawIndividualUnitFramesPlayerConfig()
        {
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled);
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()*0.3f), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()*0.5f),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth() / 2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Height");
                            var healthBarHeight = _pluginConfiguration.HealthBarHeight;
                            if (ImGui.DragInt("", ref healthBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.HealthBarHeight = healthBarHeight;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth() / 2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Width");
                            var healthBarWidth = _pluginConfiguration.HealthBarWidth;
                            if (ImGui.DragInt("", ref healthBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.HealthBarWidth = healthBarWidth;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();
                    ImGui.Separator();
                    ImGui.BeginChild("hpoffsetpane", new Vector2(0,ImGui.GetWindowHeight()*0.5f),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpxpane", new Vector2(ImGui.GetWindowWidth() / 2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("X Offset");
                            var healthBarXOffset = _pluginConfiguration.HealthBarXOffset;
                            if (ImGui.DragInt("", ref healthBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarXOffset = healthBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpypane", new Vector2(ImGui.GetWindowWidth() / 2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Y Offset");
                            var healthBarYOffset = _pluginConfiguration.HealthBarYOffset;
                            if (ImGui.DragInt("", ref healthBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
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
            
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hptxtpane", new Vector2(0,ImGui.GetWindowHeight()*0.7f), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    ImGui.BeginChild("hptxtformatpane", new Vector2(0,ImGui.GetWindowHeight()/3),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth() / 2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Left Text Format");
                            var healthBarTextLeft = _pluginConfiguration.HealthBarTextLeft;
                            if (ImGui.InputText("", ref healthBarTextLeft, 999))
                            {
                                _pluginConfiguration.HealthBarTextLeft = healthBarTextLeft;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hptxtformatrightpane", new Vector2(ImGui.GetWindowWidth() / 2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Right Text Format");
                            var healthBarTextRight = _pluginConfiguration.HealthBarTextRight;
                            if (ImGui.InputText("", ref healthBarTextRight, 999))
                            {
                                _pluginConfiguration.HealthBarTextRight = healthBarTextRight;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();
                    ImGui.Separator();
                    ImGui.BeginChild("hptxtoffsetpane", new Vector2(0,ImGui.GetWindowHeight()/3),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtleftxpane", new Vector2(ImGui.GetWindowWidth() / 2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Left Text X Offset");
                            var healthBarTextLeftXOffset = _pluginConfiguration.HealthBarTextLeftXOffset;
                            if (ImGui.DragInt("", ref healthBarTextLeftXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarTextLeftXOffset = healthBarTextLeftXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hptxtleftypane", new Vector2(ImGui.GetWindowWidth() / 2, 0));
                        {
                            ImGui.Text("Left Text Y Offset");
                            var healthBarTextLeftYOffset = _pluginConfiguration.HealthBarTextLeftYOffset;
                            if (ImGui.DragInt("", ref healthBarTextLeftYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarTextLeftYOffset = healthBarTextLeftYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();
                    ImGui.Separator();
                    ImGui.BeginChild("hpoffsetpane", new Vector2(0,ImGui.GetWindowHeight()/2));
                    {
                        ImGui.BeginChild("hpxleftpane", new Vector2(ImGui.GetWindowWidth() / 2, 0));
                        {
                            ImGui.Text("Right Text X Offset");
                            var healthBarTextRightXOffset = _pluginConfiguration.HealthBarTextRightXOffset;
                            if (ImGui.DragInt("", ref healthBarTextRightXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarTextRightXOffset = healthBarTextRightXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpypane", new Vector2(ImGui.GetWindowWidth() / 2, 0));
                        {
                            ImGui.Text("Right Text Y Offset");
                            var healthBarTextRightYOffset = _pluginConfiguration.HealthBarTextRightYOffset;
                            if (ImGui.DragInt("", ref healthBarTextRightYOffset, .1f, -yOffsetLimit, yOffsetLimit))
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
            if (ImGui.DragInt("Primary Resource X Offset", ref primaryResourceBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
            {
                _pluginConfiguration.PrimaryResourceBarXOffset = primaryResourceBarXOffset;
                _pluginConfiguration.Save();
            }

            var primaryResourceBarYOffset = _pluginConfiguration.PrimaryResourceBarYOffset;
            if (ImGui.DragInt("Primary Resource Y Offset", ref primaryResourceBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
            {
                _pluginConfiguration.PrimaryResourceBarYOffset = primaryResourceBarYOffset;
                _pluginConfiguration.Save();
            }
        }
        
        private void DrawIndividualUnitFramesToTConfig()
        {
            var totBarHeight = _pluginConfiguration.ToTBarHeight;
                    if (ImGui.DragInt("Target of Target Height", ref totBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ToTBarHeight = totBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var totBarWidth = _pluginConfiguration.ToTBarWidth;
                    if (ImGui.DragInt("Target of Target Width", ref totBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ToTBarWidth = totBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var totBarText = _pluginConfiguration.ToTBarText;
                    if (ImGui.InputText("Target of Target Text", ref totBarText, 999))
                    {
                        _pluginConfiguration.ToTBarText = totBarText;
                        _pluginConfiguration.Save();
                    }

                    var toTBarXOffset = _pluginConfiguration.ToTBarXOffset;
                    if (ImGui.DragInt("Target of Target X Offset", ref toTBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                    {
                        _pluginConfiguration.ToTBarXOffset = toTBarXOffset;
                        _pluginConfiguration.Save();
                    }

                    var toTBarYOffset = _pluginConfiguration.ToTBarYOffset;
                    if (ImGui.DragInt("Target of Target Y Offset", ref toTBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                    {
                        _pluginConfiguration.ToTBarYOffset = toTBarYOffset;
                        _pluginConfiguration.Save();
                    }
                    
                    var totBarTextXOffset = _pluginConfiguration.ToTBarTextXOffset;
                    if (ImGui.DragInt("Target of Target Text X Offset", ref totBarTextXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                    {
                        _pluginConfiguration.ToTBarTextXOffset = totBarTextXOffset;
                        _pluginConfiguration.Save();
                    }

                    var totBarTextYOffset = _pluginConfiguration.ToTBarTextYOffset;
                    if (ImGui.DragInt("Target of Target Text Y Offset", ref totBarTextYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                    {
                        _pluginConfiguration.ToTBarTextYOffset = totBarTextYOffset;
                        _pluginConfiguration.Save();
                    }
        }
        
        private void DrawIndividualUnitFramesFocusConfig()
        {
            var focusBarHeight = _pluginConfiguration.FocusBarHeight;
                    if (ImGui.DragInt("Focus Height", ref focusBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.FocusBarHeight = focusBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var focusBarWidth = _pluginConfiguration.FocusBarWidth;
                    if (ImGui.DragInt("Focus Width", ref focusBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.FocusBarWidth = focusBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var focusBarXOffset = _pluginConfiguration.FocusBarXOffset;
                    if (ImGui.DragInt("Focus X Offset", ref focusBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                    {
                        _pluginConfiguration.FocusBarXOffset = focusBarXOffset;
                        _pluginConfiguration.Save();
                    }

                    var focusBarYOffset = _pluginConfiguration.FocusBarYOffset;
                    if (ImGui.DragInt("Focus Y Offset", ref focusBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                    {
                        _pluginConfiguration.FocusBarYOffset = focusBarYOffset;
                        _pluginConfiguration.Save();
                    }

                    var focusBarText = _pluginConfiguration.FocusBarText;
                    if (ImGui.InputText("Focus Text", ref focusBarText, 999))
                    {
                        _pluginConfiguration.FocusBarText = focusBarText;
                        _pluginConfiguration.Save();
                    }
                    
                    var focusBarTextXOffset = _pluginConfiguration.FocusBarTextXOffset;
                    if (ImGui.DragInt("Focus Text X Offset", ref focusBarTextXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                    {
                        _pluginConfiguration.FocusBarTextXOffset = focusBarTextXOffset;
                        _pluginConfiguration.Save();
                    }

                    var focusBarTextYOffset = _pluginConfiguration.FocusBarTextYOffset;
                    if (ImGui.DragInt("Focus Text Y Offset", ref focusBarTextYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                    {
                        _pluginConfiguration.FocusBarTextYOffset = focusBarTextYOffset;
                        _pluginConfiguration.Save();
                    }
        }

        private void DrawGroupUnitFramesGeneralConfig()
        {
            
        }

        private void DrawGroupUnitFramesPartyConfig()
        {
            
        }

        private void DrawGroupUnitFrames8manConfig()
        {
            
        }

        private void DrawGroupUnitFrames24manConfig()
        {
            
        }

        private void DrawGroupUnitFramesEnemiesConfig()
        {
            
        }

        private void DrawCastbarsGeneralConfig()
        {
            
        }

        private void DrawCastbarsPlayerConfig()
        {
            
        }

        private void DrawCastbarsEnemyConfig()
        {
            
        }

        private void DrawJobsGeneralConfig()
        {
            
        }

        private void DrawJobsTankConfig()
        {
            
        }

        private void DrawJobsHealerConfig()
        {
            
        }

        private void DrawJobsMeleeConfig()
        {
            
        }

        private void DrawJobsRangedConfig()
        {
            
        }

        private void DrawJobsCasterConfig()
        {
            
        }

    }
}
