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
        private string selected = "Individual Unitframes";
        private string selectedColorType = "Tanks";
        private Dictionary<string, Array> configMap = new Dictionary<string, Array>() ;
        private string[] configColorMap = new[] {"Tanks", "Healers", "Melee", "Ranged", "Casters", "NPC"};
        private bool changed;
        private int viewportWidth = (int) ImGui.GetMainViewport().Size.X;
        private int viewportHeight = (int) ImGui.GetMainViewport().Size.Y;
        private int xOffsetLimit;
        private int yOffsetLimit;

        public ConfigurationWindow(Plugin plugin, DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            //TODO ADD PRIMARYRESOURCEBAR TO CONFIGMAP jobs general

            _plugin = plugin;
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginConfiguration;
            //configMap.Add("General", new [] {"General"});
            configMap.Add("Individual Unitframes", new []
            {
                "General",
                //"Colors", "Shields", 
                "Player", "Focus", "Target", "Target of Target"
            });
            //configMap.Add("Group Unitframes", new [] {"General", "Party", "8man", "24man", "Enemies"});
            configMap.Add("Castbars", new [] {
                //"General", 
                "Player"
                //, "Enemy"
                });
            configMap.Add("Jobs", new [] {"General", "Tank", "Healer", "Melee","Ranged", "Caster"});

        }   


        public void Draw()
        {
            if (!IsVisible) {
                return;
            }
            //Todo future reference dalamud native ui scaling
            //ImGui.GetIO().FontGlobalScale;
            ImGui.SetNextWindowSize(new Vector2(1050, 750), ImGuiCond.Appearing);


            if (!ImGui.Begin("titlebar", ref IsVisible, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse)) {
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
            ImGui.SameLine();
            
            pos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth()-60, ImGui.GetCursorPos().Y));
            if (ImGui.Button("Donate!"))
            {
            }
            ImGui.SetCursorPos(pos);
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

                    }
                    break;
                case "Individual Unitframes":
                    switch (subConfig)
                    {
                        //TODO NEST COLOR MAP AND SHIELDS ON GENERAL
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
            ImGui.Text("this has no configs yet");

        }

        private void DrawIndividualUnitFramesGeneralConfig()
        {
            ImGui.Text("Colors");
            ImGui.BeginGroup();
            {
                ImGui.BeginGroup(); // Left
                {
                  

                    ImGui.BeginChild("leftpane", new Vector2(150, ImGui.GetWindowHeight()/4), true);

                    foreach (var colorType in configColorMap)
                    {
                        if (ImGui.Selectable(colorType, selectedColorType == colorType))
                            selectedColorType = colorType;
                    }

                    ImGui.EndChild();


                }
                ImGui.EndGroup();

                ImGui.SameLine();

                // Right
                ImGui.BeginGroup();
                {

                        
                    ImGui.BeginChild("itemview",new Vector2(0, ImGui.GetWindowHeight()/4)); // Leave room for 1 line below us
                    {
                        
                                ImGui.BeginChild("subconfig value", new Vector2(0, 0), true);
                                ImGui.BeginChild("leftpane", new Vector2(ImGui.GetWindowWidth()/2, 0), false);
                                switch (selectedColorType)
                                {
                                    case "Tanks":
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Paladin");
                                        changed |= ImGui.ColorEdit4("##JobColorPLD", ref _pluginConfiguration.JobColorPLD);
                    
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Warrior");
                                        changed |= ImGui.ColorEdit4("##JobColorWAR", ref _pluginConfiguration.JobColorWAR);
                                        
                                        ImGui.EndChild();
                                        ImGui.SameLine();
                                        ImGui.BeginChild("leftp3ane", new Vector2(ImGui.GetWindowWidth()/2, 0), false);
                                        
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Dark Knight");
                                        changed |= ImGui.ColorEdit4("##JobColorDRK", ref _pluginConfiguration.JobColorDRK);
                    
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Gunbreaker");
                                        changed |= ImGui.ColorEdit4("##JobColorGNB", ref _pluginConfiguration.JobColorGNB);
                                        break;
                                    case "Healers":
                                       
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("White Mage");
                                        changed |= ImGui.ColorEdit4("##JobColorWHM", ref _pluginConfiguration.JobColorWHM);

                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Scholar");
                                        changed |= ImGui.ColorEdit4("##JobColorSCH", ref _pluginConfiguration.JobColorSCH);
                                        
                                        ImGui.EndChild();
                                        ImGui.SameLine();
                                        ImGui.BeginChild("leftpa2ne", new Vector2(ImGui.GetWindowWidth()/2, 0), false);
                                        
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Astrologian");
                                        changed |= ImGui.ColorEdit4("##JobColorAST", ref _pluginConfiguration.JobColorAST);

                                        
                                        break;
                                    case "Melee":
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Monk");
                                        changed |= ImGui.ColorEdit4("##JobColorMNK", ref _pluginConfiguration.JobColorMNK);
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Dragoon");
                                        changed |= ImGui.ColorEdit4("##JobColorDRG", ref _pluginConfiguration.JobColorDRG);
                                        ImGui.EndChild();
                                        ImGui.SameLine();
                                        ImGui.BeginChild("leftpa2ne", new Vector2(ImGui.GetWindowWidth()/2, 0), false);
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Ninja");
                                        changed |= ImGui.ColorEdit4("##JobColorNIN", ref _pluginConfiguration.JobColorNIN);
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Samurai");
                                        changed |= ImGui.ColorEdit4("##JobColorSAM", ref _pluginConfiguration.JobColorSAM);


                                        break;
                                    case "Ranged":
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Bard");
                                        changed |= ImGui.ColorEdit4("##JobColorBRD", ref _pluginConfiguration.JobColorBRD);
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Machinist");
                                        changed |= ImGui.ColorEdit4("##JobColorMCH", ref _pluginConfiguration.JobColorMCH);
                                        ImGui.EndChild();
                                        ImGui.SameLine();
                                        ImGui.BeginChild("leftpa2ne", new Vector2(ImGui.GetWindowWidth()/2, 0), false);
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Dancer");
                                        changed |= ImGui.ColorEdit4("##JobColorDNC", ref _pluginConfiguration.JobColorDNC);


                                        break;
                                    case "Casters":
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Black Mage");
                                        changed |= ImGui.ColorEdit4("##JobColorBLM", ref _pluginConfiguration.JobColorBLM);
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Summoner");
                                        changed |= ImGui.ColorEdit4("##JobColorSMN", ref _pluginConfiguration.JobColorSMN);
                                        ImGui.EndChild();
                                        ImGui.SameLine();
                                        ImGui.BeginChild("leftpa2ne", new Vector2(ImGui.GetWindowWidth()/2, 0), false);
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Red Mage");
                                        changed |= ImGui.ColorEdit4("##JobColorRDM", ref _pluginConfiguration.JobColorRDM);
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Blue Mage");
                                        changed |= ImGui.ColorEdit4("##JobColorBLU", ref _pluginConfiguration.JobColorBLU);
 

                                        break;
                                    case "NPC":
                                        
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Hostile");
                                        changed |= ImGui.ColorEdit4("##NPCColorHostile", ref _pluginConfiguration.NPCColorHostile);
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Neutral");
                                        changed |= ImGui.ColorEdit4("##NPCColorNeutral", ref _pluginConfiguration.NPCColorNeutral);
                                        ImGui.EndChild();
                                        ImGui.SameLine();
                                        ImGui.BeginChild("leftpa2ne", new Vector2(ImGui.GetWindowWidth()/2, 0), false);
                                        ImGui.Text("");//SPACING
                                        ImGui.Text("Friendly");
                                        changed |= ImGui.ColorEdit4("##NPCColorFriendly", ref _pluginConfiguration.NPCColorFriendly);
                                        

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
            ImGui.Text("");//SPACING
            changed |= ImGui.Checkbox("Shields", ref _pluginConfiguration.ShieldEnabled);
            ImGui.BeginGroup();
            ImGui.BeginChild("itemview2",new Vector2(0, ImGui.GetWindowHeight()/5), true); 
            {
                ImGui.BeginChild("itemvi2213ew2",new Vector2(ImGui.GetWindowWidth()/2, 0) );
                {
                    var shieldHeight = _pluginConfiguration.ShieldHeight;
                    ImGui.Text("");//SPACING
                    ImGui.Text("Height");
                   
                    if (ImGui.DragInt("##ShieldHeight", ref shieldHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ShieldHeight = shieldHeight;
                        _pluginConfiguration.Save();
                    }
                    ImGui.SameLine();
                    changed |= ImGui.Checkbox("in pixels", ref _pluginConfiguration.ShieldHeightPixels);
                    
                }
                ImGui.EndChild();
                ImGui.SameLine();
                ImGui.BeginChild("ite4123mview2",new Vector2(ImGui.GetWindowWidth()/2, 0));
                {
                    ImGui.Text("");//SPACING
                    ImGui.Text("Color");
                    changed |= ImGui.ColorEdit4("##ShieldColor", ref _pluginConfiguration.ShieldColor);
                }
                ImGui.EndChild();

            }
            ImGui.EndChild();


            ImGui.EndGroup();
        }

        private void DrawIndividualUnitFramesPlayerConfig(){
                
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled); //TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Height");
                            var healthBarHeight = _pluginConfiguration.HealthBarHeight;
                            if (ImGui.DragInt("##HealthBarHeight", ref healthBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.HealthBarHeight = healthBarHeight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("X Offset");
                            var healthBarXOffset = _pluginConfiguration.HealthBarXOffset;
                            if (ImGui.DragInt("##HealthBarXOffset", ref healthBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarXOffset = healthBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Width");
                            var healthBarWidth = _pluginConfiguration.HealthBarWidth;
                            if (ImGui.DragInt("##HealthBarWidth", ref healthBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.HealthBarWidth = healthBarWidth;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Y Offset");
                            var healthBarYOffset = _pluginConfiguration.HealthBarYOffset;
                            if (ImGui.DragInt("##HealthBarYOffset", ref healthBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
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
            ImGui.Text("");//SPACING

            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hptxtpane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    ImGui.BeginChild("hptxtformatpane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text Format");
                            var healthBarTextLeft = _pluginConfiguration.HealthBarTextLeft;
                            if (ImGui.InputText("##HealthBarTextLeft", ref healthBarTextLeft, 999))
                            {
                                _pluginConfiguration.HealthBarTextLeft = healthBarTextLeft;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text X Offset");
                            var healthBarTextLeftXOffset = _pluginConfiguration.HealthBarTextLeftXOffset;
                            if (ImGui.DragInt("##HealthBarTextLeftXOffset", ref healthBarTextLeftXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarTextLeftXOffset = healthBarTextLeftXOffset;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text Y Offset");
                            var healthBarTextLeftYOffset = _pluginConfiguration.HealthBarTextLeftYOffset;
                            if (ImGui.DragInt("##HealthBarTextLeftYOffset", ref healthBarTextLeftYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarTextLeftYOffset = healthBarTextLeftYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hptxtformatrightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text Format");
                            var healthBarTextRight = _pluginConfiguration.HealthBarTextRight;
                            if (ImGui.InputText("##HealthBarTextRight", ref healthBarTextRight, 999))
                            {
                                _pluginConfiguration.HealthBarTextRight = healthBarTextRight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text X Offset");
                            var healthBarTextRightXOffset = _pluginConfiguration.HealthBarTextRightXOffset;
                            if (ImGui.DragInt("##HealthBarTextRightXOffset", ref healthBarTextRightXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarTextRightXOffset = healthBarTextRightXOffset;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text Y Offset");
                            var healthBarTextRightYOffset = _pluginConfiguration.HealthBarTextRightYOffset;
                            if (ImGui.DragInt("##HealthBarTextRightYOffset", ref healthBarTextRightYOffset, .1f, -yOffsetLimit, yOffsetLimit))
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
        private void DrawIndividualUnitFramesTargetConfig(){
                
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled); //TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Height");
                            var TargetBarHeight = _pluginConfiguration.TargetBarHeight;
                            if (ImGui.DragInt("##TargetBarHeight", ref TargetBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.TargetBarHeight = TargetBarHeight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("X Offset");
                            var TargetBarXOffset = _pluginConfiguration.TargetBarXOffset;
                            if (ImGui.DragInt("##TargetBarXOffset", ref TargetBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarXOffset = TargetBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Width");
                            var TargetBarWidth = _pluginConfiguration.TargetBarWidth;
                            if (ImGui.DragInt("##TargetBarWidth", ref TargetBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.TargetBarWidth = TargetBarWidth;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Y Offset");
                            var TargetBarYOffset = _pluginConfiguration.TargetBarYOffset;
                            if (ImGui.DragInt("##TargetBarYOffset", ref TargetBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarYOffset = TargetBarYOffset;
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
            ImGui.Text("");//SPACING

            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hptxtpane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    ImGui.BeginChild("hptxtformatpane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text Format");
                            var TargetBarTextLeft = _pluginConfiguration.TargetBarTextLeft;
                            if (ImGui.InputText("##TargetBarTextLeft", ref TargetBarTextLeft, 999))
                            {
                                _pluginConfiguration.TargetBarTextLeft = TargetBarTextLeft;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text X Offset");
                            var TargetBarTextLeftXOffset = _pluginConfiguration.TargetBarTextLeftXOffset;
                            if (ImGui.DragInt("##TargetBarTextLeftXOffset", ref TargetBarTextLeftXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarTextLeftXOffset = TargetBarTextLeftXOffset;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Left Text Y Offset");
                            var TargetBarTextLeftYOffset = _pluginConfiguration.TargetBarTextLeftYOffset;
                            if (ImGui.DragInt("##TargetBarTextLeftYOffset", ref TargetBarTextLeftYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarTextLeftYOffset = TargetBarTextLeftYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hptxtformatrightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text Format");
                            var TargetBarTextRight = _pluginConfiguration.TargetBarTextRight;
                            if (ImGui.InputText("##TargetBarTextRight", ref TargetBarTextRight, 999))
                            {
                                _pluginConfiguration.TargetBarTextRight = TargetBarTextRight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text X Offset");
                            var TargetBarTextRightXOffset = _pluginConfiguration.TargetBarTextRightXOffset;
                            if (ImGui.DragInt("##TargetBarTextRightXOffset", ref TargetBarTextRightXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarTextRightXOffset = TargetBarTextRightXOffset;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Right Text Y Offset");
                            var TargetBarTextRightYOffset = _pluginConfiguration.TargetBarTextRightYOffset;
                            if (ImGui.DragInt("##TargetBarTextRightYOffset", ref TargetBarTextRightYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.TargetBarTextRightYOffset = TargetBarTextRightYOffset;
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
        private void DrawIndividualUnitFramesToTConfig(){
                
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled); //TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {

                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Height");
                            var ToTBarHeight = _pluginConfiguration.ToTBarHeight;
                            if (ImGui.DragInt("##ToTBarHeight", ref ToTBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.ToTBarHeight = ToTBarHeight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("X Offset");
                            var ToTBarXOffset = _pluginConfiguration.ToTBarXOffset;
                            if (ImGui.DragInt("##ToTBarXOffset", ref ToTBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarXOffset = ToTBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Width");
                            var ToTBarWidth = _pluginConfiguration.ToTBarWidth;
                            if (ImGui.DragInt("##ToTBarWidth", ref ToTBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.ToTBarWidth = ToTBarWidth;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Y Offset");
                            var ToTBarYOffset = _pluginConfiguration.ToTBarYOffset;
                            if (ImGui.DragInt("##ToTBarYOffset", ref ToTBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarYOffset = ToTBarYOffset;
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
            ImGui.Text("");//SPACING
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hpp2ane", new Vector2(0,ImGui.GetWindowHeight()/3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("castbar2color", new Vector2(0,ImGui.GetWindowHeight()/3),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                {
                    ImGui.Text("");//SPACING
                    ImGui.Text("Text Format");
                    var ToTBarText = _pluginConfiguration.ToTBarText;
                    if (ImGui.InputText("##ToTBarText", ref ToTBarText, 999))
                    {
                        _pluginConfiguration.ToTBarText = ToTBarText;
                        _pluginConfiguration.Save();
                    }

                        
                }
                ImGui.EndChild();
                ImGui.Text("");//SPACING

                ImGui.BeginChild("hpsize2pane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                {
                    ImGui.BeginChild("hpheight2pane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.Text("");//SPACING
                        ImGui.Text("X Offset");
                        var ToTBarTextXOffset = _pluginConfiguration.ToTBarTextXOffset;
                        if (ImGui.DragInt("##ToTBarTextXOffset", ref ToTBarTextXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                        {
                            _pluginConfiguration.ToTBarTextXOffset = ToTBarTextXOffset;
                            _pluginConfiguration.Save();
                        }
                    }
                    ImGui.EndChild();
                        
                    ImGui.SameLine();
                        
                    ImGui.BeginChild("hpwidt2hpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.Text("");//SPACING

                        ImGui.Text("Text Y Offset");
                        var ToTBarTextYOffset = _pluginConfiguration.ToTBarTextYOffset;
                        if (ImGui.DragInt("##ToTBarTextYOffset", ref ToTBarTextYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                        {
                            _pluginConfiguration.ToTBarTextYOffset = ToTBarTextYOffset;
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
        private void DrawIndividualUnitFramesFocusConfig(){
                
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled); //TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {

                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Height");
                            var FocusBarHeight = _pluginConfiguration.FocusBarHeight;
                            if (ImGui.DragInt("##FocusBarHeight", ref FocusBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.FocusBarHeight = FocusBarHeight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("X Offset");
                            var FocusBarXOffset = _pluginConfiguration.FocusBarXOffset;
                            if (ImGui.DragInt("##FocusBarXOffset", ref FocusBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.FocusBarXOffset = FocusBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Width");
                            var FocusBarWidth = _pluginConfiguration.FocusBarWidth;
                            if (ImGui.DragInt("##FocusBarWidth", ref FocusBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.FocusBarWidth = FocusBarWidth;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Y Offset");
                            var FocusBarYOffset = _pluginConfiguration.FocusBarYOffset;
                            if (ImGui.DragInt("##FocusBarYOffset", ref FocusBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.FocusBarYOffset = FocusBarYOffset;
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
            ImGui.Text("");//SPACING
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Text Format");
                ImGui.BeginChild("hpp2ane", new Vector2(0,ImGui.GetWindowHeight()/3), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.BeginChild("castbar2color", new Vector2(0,ImGui.GetWindowHeight()/3),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                {
                    ImGui.Text("");//SPACING
                    ImGui.Text("Text Format");
                    var FocusBarText = _pluginConfiguration.FocusBarText;
                    if (ImGui.InputText("##FocusBarText", ref FocusBarText, 999))
                    {
                        _pluginConfiguration.FocusBarText = FocusBarText;
                        _pluginConfiguration.Save();
                    }

                        
                }
                ImGui.EndChild();
                ImGui.Text("");//SPACING

                ImGui.BeginChild("hpsize2pane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                {
                    ImGui.BeginChild("hpheight2pane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.Text("");//SPACING
                        ImGui.Text("X Offset");
                        var FocusBarTextXOffset = _pluginConfiguration.FocusBarTextXOffset;
                        if (ImGui.DragInt("##FocusBarTextXOffset", ref FocusBarTextXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                        {
                            _pluginConfiguration.FocusBarTextXOffset = FocusBarTextXOffset;
                            _pluginConfiguration.Save();
                        }
                    }
                    ImGui.EndChild();
                        
                    ImGui.SameLine();
                        
                    ImGui.BeginChild("hpwidt2hpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.Text("");//SPACING

                        ImGui.Text("Text Y Offset");
                        var FocusBarTextYOffset = _pluginConfiguration.FocusBarTextYOffset;
                        if (ImGui.DragInt("##FocusBarTextYOffset", ref FocusBarTextYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                        {
                            _pluginConfiguration.FocusBarTextYOffset = FocusBarTextYOffset;
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
        
        private void DrawIndividualUnitFramesFocusConfigss(){
            bool disabled = true;
            ImGui.Checkbox("Enabled", ref disabled);//TODO CODE THIS
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Height");
                            var FocusBarHeight = _pluginConfiguration.FocusBarHeight;
                            if (ImGui.DragInt("##FocusBarHeight", ref FocusBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.FocusBarHeight = FocusBarHeight;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Width");
                            var FocusBarWidth = _pluginConfiguration.FocusBarWidth;
                            if (ImGui.DragInt("##FocusBarWidth", ref FocusBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.FocusBarWidth = FocusBarWidth;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();
                    ImGui.Separator();
                    ImGui.BeginChild("hpoffsetpane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpxpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("X Offset");
                            var FocusBarXOffset = _pluginConfiguration.FocusBarXOffset;
                            if (ImGui.DragInt("##FocusBarXOffset", ref FocusBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.FocusBarXOffset = FocusBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpypane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Y Offset");
                            var FocusBarYOffset = _pluginConfiguration.FocusBarYOffset;
                            if (ImGui.DragInt("##FocusBarYOffset", ref FocusBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.FocusBarYOffset = FocusBarYOffset;
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
                ImGui.BeginChild("hptxtpane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    ImGui.BeginChild("hptxtformatpane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Text Format");
                            var FocusBarText = _pluginConfiguration.FocusBarText;
                            if (ImGui.InputText("##FocusBarText", ref FocusBarText, 999))
                            {
                                _pluginConfiguration.FocusBarText = FocusBarText;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        

                        
                    }
                    ImGui.EndChild();
                    ImGui.Separator();
                    ImGui.BeginChild("hptxtoffsetpane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtleftxpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("Text X Offset");
                            var FocusBarTextXOffset = _pluginConfiguration.FocusBarTextXOffset;
                            if (ImGui.DragInt("##FocusBarTextXOffset", ref FocusBarTextXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.FocusBarTextXOffset = FocusBarTextXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hptxtleftypane", new Vector2(ImGui.GetWindowWidth()/2, 0));
                        {
                            ImGui.Text("Text Y Offset");
                            var FocusBarTextYOffset = _pluginConfiguration.FocusBarTextYOffset;
                            if (ImGui.DragInt("##FocusBarTextYOffset", ref FocusBarTextYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.ToTBarTextYOffset = FocusBarTextYOffset;
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
        private void DrawCastbarsPlayerConfig(){
            bool disabled = true;
            changed |= ImGui.Checkbox("Enabled", ref _pluginConfiguration.ShowCastBar);

            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Bar Size & Position");
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Height");
                            var CastBarHeight = _pluginConfiguration.CastBarHeight;
                            if (ImGui.DragInt("##CastBarHeight", ref CastBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.CastBarHeight = CastBarHeight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("X Offset");
                            var CastBarXOffset = _pluginConfiguration.CastBarXOffset;
                            if (ImGui.DragInt("##CastBarXOffset", ref CastBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.CastBarXOffset = CastBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Width");
                            var CastBarWidth = _pluginConfiguration.CastBarWidth;
                            if (ImGui.DragInt("##CastBarWidth", ref CastBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.CastBarWidth = CastBarWidth;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Y Offset");
                            var CastBarYOffset = _pluginConfiguration.CastBarYOffset;
                            if (ImGui.DragInt("##CastBarYOffset", ref CastBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.CastBarYOffset = CastBarYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();
                    ImGui.Text("");//SPACING

                    ImGui.BeginChild("castbarcolor", new Vector2(0,ImGui.GetWindowHeight()/3),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.Text("Color");
                        changed |= ImGui.ColorEdit4("##CastBarColor", ref _pluginConfiguration.CastBarColor);

                        
                    }
                    ImGui.EndChild();
                ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();   
            ImGui.Text("");//SPACING
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {
                ImGui.Text("Other Options");
                ImGui.BeginChild("otheroptions", new Vector2(0,ImGui.GetWindowHeight()/4), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("otheroptions1", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("otheroptions2", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING
                            changed |= ImGui.Checkbox("Show Interrupted", ref _pluginConfiguration.InterruptCheck);
                            ImGui.Text("");//SPACING
                            changed |= ImGui.Checkbox("Show Action Icon", ref _pluginConfiguration.ShowActionIcon);
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("otheroptions3", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING
                            changed |= ImGui.Checkbox("Show Action Name", ref _pluginConfiguration.ShowActionName);
                            ImGui.Text("");//SPACING
                            changed |= ImGui.Checkbox("Show Cast Time", ref _pluginConfiguration.ShowCastTime);
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();

                    ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
            ImGui.Text("");//SPACING
            ImGui.BeginGroup();
            ImGui.BeginGroup(); // Left
            {                        
                changed |= ImGui.Checkbox("SlideCast", ref _pluginConfiguration.SlideCast);
                ImGui.BeginChild("hptxtpane", new Vector2(0,ImGui.GetWindowHeight()/3), true, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    ImGui.BeginChild("hptxtformatpane", new Vector2(0,ImGui.GetWindowHeight()),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hptxtformatleftpane", new Vector2(ImGui.GetWindowWidth(), 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING
                            ImGui.Text("Offset");
                            var SlideCastTime = _pluginConfiguration.SlideCastTime;
                            if (ImGui.DragFloat("##SlideCastTime", ref SlideCastTime, 1, 1, 1000))
                            {
                                _pluginConfiguration.SlideCastTime = SlideCastTime;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING
                            ImGui.Text("Color");
                            changed |= ImGui.ColorEdit4("##SlideCastColor", ref _pluginConfiguration.SlideCastColor);

                        }
                        ImGui.EndChild();
                        

                        
                    }
                    ImGui.EndChild();
                    ImGui.EndChild();

            }
            ImGui.EndGroup();
            ImGui.EndGroup();
                
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
