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
//test commit
            _plugin = plugin;
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginConfiguration;
            configMap.Add("General", new [] {"General"});
            configMap.Add("Individual Unitframes", new []
            {
                "General",
                "Player", "Focus", "Target", "Target of Target"
            });
            //configMap.Add("Group Unitframes", new [] {"General", "Party", "8man", "24man", "Enemies"});
            configMap.Add("Castbars", new [] {
                //"General", 
                "Player"
                , "Enemy"
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
                    /*
                    var imagePath = Path.Combine(Path.GetDirectoryName(_plugin.AssemblyLocation) ?? "", "Media", "Images", "banner_short_x150.png");
                    var delvuiBanner = _pluginInterface.UiBuilder.LoadImage(imagePath);
                    ImGui.Image(delvuiBanner.ImGuiHandle, new Vector2(delvuiBanner.Width, delvuiBanner.Height));
*/
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
            changed |= ImGui.Checkbox("Show MP Ticker", ref _pluginConfiguration.MPTickerEnabled);

                    var mpTickerHeight = _pluginConfiguration.MPTickerHeight;
                    if (ImGui.DragInt("MP Ticker Height", ref mpTickerHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MPTickerHeight = mpTickerHeight;
                        _pluginConfiguration.Save();
                    }

                    var mpTickerWidth = _pluginConfiguration.MPTickerWidth;
                    if (ImGui.DragInt("MP Ticker Width", ref mpTickerWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MPTickerWidth = mpTickerWidth;
                        _pluginConfiguration.Save();
                    }

                    var mpTickerXOffset = _pluginConfiguration.MPTickerXOffset;
                    if (ImGui.DragInt("MP Ticker X Offset", ref mpTickerXOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.MPTickerXOffset = mpTickerXOffset;
                        _pluginConfiguration.Save();
                    }

                    var mpTickerYOffset = _pluginConfiguration.MPTickerYOffset;
                    if (ImGui.DragInt("MP Ticker Y Offset", ref mpTickerYOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.MPTickerYOffset = mpTickerYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show MP Ticker border", ref _pluginConfiguration.MPTickerShowBorder);
                    changed |= ImGui.Checkbox("Hide MP Ticker on full mp", ref _pluginConfiguration.MPTickerHideOnFullMp);
                    changed |= ImGui.ColorEdit4("MP Ticker Color", ref _pluginConfiguration.MPTickerColor);

                    // gcd indicator
                    changed |= ImGui.Checkbox("Show GCD Indicator", ref _pluginConfiguration.GCDIndicatorEnabled);

                    var GCDIndicatorHeight = _pluginConfiguration.GCDIndicatorHeight;
                    if (ImGui.DragInt("GCD Indicator Height", ref GCDIndicatorHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.GCDIndicatorHeight = GCDIndicatorHeight;
                        _pluginConfiguration.Save();
                    }

                    var GCDIndicatorWidth = _pluginConfiguration.GCDIndicatorWidth;
                    if (ImGui.DragInt("GCD Indicator Width", ref GCDIndicatorWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.GCDIndicatorWidth = GCDIndicatorWidth;
                        _pluginConfiguration.Save();
                    }

                    var GCDIndicatorXOffset = _pluginConfiguration.GCDIndicatorXOffset;
                    if (ImGui.DragInt("GCD Indicator X Offset", ref GCDIndicatorXOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.GCDIndicatorXOffset = GCDIndicatorXOffset;
                        _pluginConfiguration.Save();
                    }

                    var GCDIndicatorYOffset = _pluginConfiguration.GCDIndicatorYOffset;
                    if (ImGui.DragInt("GCD Indicator Y Offset", ref GCDIndicatorYOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.GCDIndicatorYOffset = GCDIndicatorYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show GCD Indicator border", ref _pluginConfiguration.GCDIndicatorShowBorder);
                    changed |= ImGui.ColorEdit4("GCD Indicator Color", ref _pluginConfiguration.GCDIndicatorColor);


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
                ImGui.BeginChild("hppane", new Vector2(0,ImGui.GetWindowHeight()/2), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                    ImGui.BeginChild("hpsizepane", new Vector2(0,ImGui.GetWindowHeight()/2),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        ImGui.BeginChild("hpheightpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Height");
                            var HealthBarHeight = _pluginConfiguration.HealthBarHeight;
                            if (ImGui.DragInt("##HealthBarHeight", ref HealthBarHeight, .1f, 1, 1000))
                            {
                                _pluginConfiguration.HealthBarHeight = HealthBarHeight;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("X Offset");
                            var HealthBarXOffset = _pluginConfiguration.HealthBarXOffset;
                            if (ImGui.DragInt("##HealthBarXOffset", ref HealthBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarXOffset = HealthBarXOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                        ImGui.SameLine();
                        
                        ImGui.BeginChild("hpwidthpane", new Vector2(ImGui.GetWindowWidth()/2, 0),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                        {
                            ImGui.Text("");//SPACING

                            ImGui.Text("Width");
                            var HealthBarWidth = _pluginConfiguration.HealthBarWidth;
                            if (ImGui.DragInt("##HealthBarWidth", ref HealthBarWidth, .1f, 1, 1000))
                            {
                                _pluginConfiguration.HealthBarWidth = HealthBarWidth;
                                _pluginConfiguration.Save();
                            }
                            ImGui.Text("");//SPACING

                            ImGui.Text("Y Offset");
                            var HealthBarYOffset = _pluginConfiguration.HealthBarYOffset;
                            if (ImGui.DragInt("##HealthBarYOffset", ref HealthBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                            {
                                _pluginConfiguration.HealthBarYOffset = HealthBarYOffset;
                                _pluginConfiguration.Save();
                            }
                        }
                        ImGui.EndChild();
                        
                    }
                    ImGui.EndChild();
                    ImGui.Text("");//SPACING

                    ImGui.BeginChild("castbarcolor", new Vector2(0,ImGui.GetWindowHeight()/3),false,ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
                    {
                        changed |= ImGui.Checkbox("Custom Color", ref _pluginConfiguration.CustomHealthBarColorEnabled);
                        changed |= ImGui.ColorEdit4("##CustomHealthbarColor", ref _pluginConfiguration.CustomHealthBarColor);
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
            changed |= ImGui.Checkbox("Show Target Cast Bar", ref _pluginConfiguration.ShowTargetCastBar);

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
                    if (ImGui.DragInt("Target Castbar X Offset", ref targetCastBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                    {
                        _pluginConfiguration.TargetCastBarXOffset = targetCastBarXOffset;
                        _pluginConfiguration.Save();
                    }

                    var targetCastBarYOffset = _pluginConfiguration.TargetCastBarYOffset;
                    if (ImGui.DragInt("Target Castbar Y Offset", ref targetCastBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                    {
                        _pluginConfiguration.TargetCastBarYOffset = targetCastBarYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Color Target CastBar by Damage Type", ref _pluginConfiguration.ColorCastBarByDamageType);
                    
                    changed |= ImGui.ColorEdit4("Target Castbar Color", ref _pluginConfiguration.TargetCastBarColor);
                    changed |= ImGui.ColorEdit4("Interruptable Cast Color", ref _pluginConfiguration.TargetCastBarInterruptColor);
                    changed |= ImGui.ColorEdit4("Physical Cast Color", ref _pluginConfiguration.TargetCastBarPhysicalColor);
                    changed |= ImGui.ColorEdit4("Magical Cast Color", ref _pluginConfiguration.TargetCastBarMagicalColor);
                    changed |= ImGui.ColorEdit4("Darkness Cast Color", ref _pluginConfiguration.TargetCastBarDarknessColor);

                    changed |= ImGui.Checkbox("Show Target Action Icon", ref _pluginConfiguration.ShowTargetActionIcon);
                    changed |= ImGui.Checkbox("Show Target Action Name", ref _pluginConfiguration.ShowTargetActionName);
                    changed |= ImGui.Checkbox("Show Target Cast Time", ref _pluginConfiguration.ShowTargetCastTime);
                    changed |= ImGui.Checkbox("Show Interruptable Casts", ref _pluginConfiguration.ShowTargetInterrupt);

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

        private void DrawJobsTankConfig()
        {

            if (ImGui.BeginTabBar("##tanks-tabs"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    changed |= ImGui.Checkbox("Tank Stance Indicator Enabled", ref _pluginConfiguration.TankStanceIndicatorEnabled);

                    var tankStanceIndicatorWidth = _pluginConfiguration.TankStanceIndicatorWidth;
                    if (ImGui.DragInt("Tank Stance Indicator Width", ref tankStanceIndicatorWidth, .1f, 1, 6))
                    {
                        _pluginConfiguration.TankStanceIndicatorWidth = tankStanceIndicatorWidth;
                        _pluginConfiguration.Save();
                    }
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Warrior"))
                {
                    var stormsEyeHeight = _pluginConfiguration.WARStormsEyeHeight;
                    if (ImGui.DragInt("Storm's Eye Height", ref stormsEyeHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.WARStormsEyeHeight = stormsEyeHeight;
                        _pluginConfiguration.Save();
                    }

                    var stormsEyeWidth = _pluginConfiguration.WARStormsEyeWidth;
                    if (ImGui.DragInt("Storm's Eye Width", ref stormsEyeWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.WARStormsEyeWidth = stormsEyeWidth;
                        _pluginConfiguration.Save();
                    }

                    var warBaseXOffset = _pluginConfiguration.WARBaseXOffset;
                    if (ImGui.DragInt("Base X Offset", ref warBaseXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.WARBaseXOffset = warBaseXOffset;
                        _pluginConfiguration.Save();
                    }

                    var warBaseYOffset = _pluginConfiguration.WARBaseYOffset;
                    if (ImGui.DragInt("Base Y Offset", ref warBaseYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.WARBaseYOffset = warBaseYOffset;
                        _pluginConfiguration.Save();
                    }

                    var beastGaugeHeight = _pluginConfiguration.WARBeastGaugeHeight;
                    if (ImGui.DragInt("Beast Gauge Height", ref beastGaugeHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.WARBeastGaugeHeight = beastGaugeHeight;
                        _pluginConfiguration.Save();
                    }

                    var beastGaugeWidth = _pluginConfiguration.WARBeastGaugeWidth;
                    if (ImGui.DragInt("Beast Gauge Width", ref beastGaugeWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.WARBeastGaugeWidth = beastGaugeWidth;
                        _pluginConfiguration.Save();
                    }

                    var beastGaugePadding = _pluginConfiguration.WARBeastGaugePadding;
                    if (ImGui.DragInt("Beast Gauge Padding", ref beastGaugePadding, .1f, 1, 1000))
                    {
                        _pluginConfiguration.WARBeastGaugePadding = beastGaugePadding;
                        _pluginConfiguration.Save();
                    }

                    var warBeastGaugeXOffset = _pluginConfiguration.WARBeastGaugeXOffset;
                    if (ImGui.DragInt("Beast Gauge X Offset", ref warBeastGaugeXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.WARBeastGaugeXOffset = warBeastGaugeXOffset;
                        _pluginConfiguration.Save();
                    }

                    var warBeastGaugeYOffset = _pluginConfiguration.WARBeastGaugeYOffset;
                    if (ImGui.DragInt("Beast Gauge Y Offset", ref warBeastGaugeYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.WARBeastGaugeYOffset = warBeastGaugeYOffset;
                        _pluginConfiguration.Save();
                    }

                    var warInterBarOffset = _pluginConfiguration.WARInterBarOffset;
                    if (ImGui.DragInt("Space Between Bars", ref warInterBarOffset, .1f, 1, 1000))
                    {
                        _pluginConfiguration.WARInterBarOffset = warInterBarOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Inner Release Color", ref _pluginConfiguration.WARInnerReleaseColor);
                    changed |= ImGui.ColorEdit4("Storm's Eye Color", ref _pluginConfiguration.WARStormsEyeColor);
                    changed |= ImGui.ColorEdit4("Beast Gauge Full Color", ref _pluginConfiguration.WARFellCleaveColor);
                    changed |= ImGui.ColorEdit4("Nascent Chaos Ready Color",
                        ref _pluginConfiguration.WARNascentChaosColor);
                    changed |= ImGui.ColorEdit4("Bar Not Full Color", ref _pluginConfiguration.WAREmptyColor);

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Paladin"))
                {
                    int pldManaHeight = _pluginConfiguration.PLDManaHeight;
                    if (ImGui.DragInt("Mana Height", ref pldManaHeight, 0.1f, 1, 1000))
                    {
                        _pluginConfiguration.PLDManaHeight = pldManaHeight;
                        _pluginConfiguration.Save();
                    }

                    int pldManaWidth = _pluginConfiguration.PLDManaWidth;
                    if (ImGui.DragInt("Mana Width", ref pldManaWidth, 0.1f, 1, 1000))
                    {
                        _pluginConfiguration.PLDManaWidth = pldManaWidth;
                        _pluginConfiguration.Save();
                    }

                    int pldManaPadding = _pluginConfiguration.PLDManaPadding;
                    if (ImGui.DragInt("Mana Padding", ref pldManaPadding, 0.1f, 1, 1000))
                    {
                        _pluginConfiguration.PLDManaPadding = pldManaPadding;
                        _pluginConfiguration.Save();
                    }

                    int pldBaseXoffset = _pluginConfiguration.PLDBaseXOffset;
                    if (ImGui.DragInt("Base X Offset", ref pldBaseXoffset, 0.1f, -2000, 2000))
                    {
                        _pluginConfiguration.PLDBaseXOffset = pldBaseXoffset;
                        _pluginConfiguration.Save();
                    }

                    int pldBaseYoffset = _pluginConfiguration.PLDBaseYOffset;
                    if (ImGui.DragInt("Base Y Offset", ref pldBaseYoffset, 0.1f, -2000, 2000))
                    {
                        _pluginConfiguration.PLDBaseYOffset = pldBaseYoffset;
                        _pluginConfiguration.Save();
                    }

                    int pldOathGaugeHeight = _pluginConfiguration.PLDOathGaugeHeight;
                    if (ImGui.DragInt("Oath Gauge Height", ref pldOathGaugeHeight, 0.1f, 1, 1000))
                    {
                        _pluginConfiguration.PLDOathGaugeHeight = pldOathGaugeHeight;
                        _pluginConfiguration.Save();
                    }

                    int pldOathGaugeWidth = _pluginConfiguration.PLDOathGaugeWidth;
                    if (ImGui.DragInt("Oath Gauge Width", ref pldOathGaugeWidth, 0.1f, 1, 1000))
                    {
                        _pluginConfiguration.PLDOathGaugeWidth = pldOathGaugeWidth;
                        _pluginConfiguration.Save();
                    }

                    int oathGaugePadding = _pluginConfiguration.PLDOathGaugePadding;
                    if (ImGui.DragInt("Oath Gauge Padding", ref oathGaugePadding, 0.1f, 1, 1000))
                    {
                        _pluginConfiguration.PLDOathGaugePadding = oathGaugePadding;
                        _pluginConfiguration.Save();
                    }

                    int oathGaugeXoffset = _pluginConfiguration.PLDOathGaugeXOffset;
                    if (ImGui.DragInt("Oath Gauge X Offset", ref oathGaugeXoffset, 0.1f, -2000, 2000))
                    {
                        _pluginConfiguration.PLDOathGaugeXOffset = oathGaugeXoffset;
                        _pluginConfiguration.Save();
                    }

                    int oathGaugeYoffset = _pluginConfiguration.PLDOathGaugeYOffset;
                    if (ImGui.DragInt("Oath Gauge Y Offset", ref oathGaugeYoffset, 0.1f, -2000, 2000))
                    {
                        _pluginConfiguration.PLDOathGaugeYOffset = oathGaugeYoffset;
                        _pluginConfiguration.Save();
                    }

                    bool oathGaugeText = _pluginConfiguration.PLDOathGaugeText;
                    if (ImGui.Checkbox("Oath Gauge Text", ref oathGaugeText))
                    {
                        _pluginConfiguration.PLDOathGaugeText = oathGaugeText;
                        _pluginConfiguration.Save();
                    }

                    int pldBuffBarHeight = _pluginConfiguration.PLDBuffBarHeight;
                    if (ImGui.DragInt("Buff Bar Height", ref pldBuffBarHeight, 0.1f, 1, 1000))
                    {
                        _pluginConfiguration.PLDBuffBarHeight = pldBuffBarHeight;
                        _pluginConfiguration.Save();
                    }

                    int pldBuffBarWidth = _pluginConfiguration.PLDBuffBarWidth;
                    if (ImGui.DragInt("Buff Bar Width", ref pldBuffBarWidth, 0.1f, 1, 1000))
                    {
                        _pluginConfiguration.PLDBuffBarWidth = pldBuffBarWidth;
                        _pluginConfiguration.Save();
                    }

                    int pldBuffBarXoffset = _pluginConfiguration.PLDBuffBarXOffset;
                    if (ImGui.DragInt("Buff Bar X Offset", ref pldBuffBarXoffset, 0.1f, -2000, 2000))
                    {
                        _pluginConfiguration.PLDBuffBarXOffset = pldBuffBarXoffset;
                        _pluginConfiguration.Save();
                    }

                    int pldBuffBarYoffset = _pluginConfiguration.PLDBuffBarYOffset;
                    if (ImGui.DragInt("Buff Bar Y Offset", ref pldBuffBarYoffset, 0.1f, -2000, 2000))
                    {
                        _pluginConfiguration.PLDBuffBarYOffset = pldBuffBarYoffset;
                        _pluginConfiguration.Save();
                    }
                    int pldAtonementBarHeight = _pluginConfiguration.PLDAtonementBarHeight;
                    if (ImGui.DragInt("Atonement Bar Height", ref pldAtonementBarHeight, 0.1f, 1, 1000))
                    {
                        _pluginConfiguration.PLDAtonementBarHeight = pldAtonementBarHeight;
                        _pluginConfiguration.Save();
                    }

                    int pldAtonementBarWidth = _pluginConfiguration.PLDAtonementBarWidth;
                    if (ImGui.DragInt("Atonement Bar Width", ref pldAtonementBarWidth, 0.1f, 1, 1000))
                    {
                        _pluginConfiguration.PLDAtonementBarWidth = pldAtonementBarWidth;
                        _pluginConfiguration.Save();
                    }

                    int pldAtonementBarXoffset = _pluginConfiguration.PLDAtonementBarXOffset;
                    if (ImGui.DragInt("Atonement Bar X Offset", ref pldAtonementBarXoffset, 0.1f, -2000, 2000))
                    {
                        _pluginConfiguration.PLDAtonementBarXOffset = pldAtonementBarXoffset;
                        _pluginConfiguration.Save();
                    }

                    int pldAtonementBarYoffset = _pluginConfiguration.PLDAtonementBarYOffset;
                    if (ImGui.DragInt("Atonement Bar Y Offset", ref pldAtonementBarYoffset, 0.1f, -2000, 2000))
                    {
                        _pluginConfiguration.PLDAtonementBarYOffset = pldAtonementBarYoffset;
                        _pluginConfiguration.Save();
                    }
                    int pldAtonementBarPadding = _pluginConfiguration.PLDAtonementBarPadding;
                    if (ImGui.DragInt("Atonement Bar Padding", ref pldAtonementBarPadding, 0.1f, 1, 2000))
                    {
                        _pluginConfiguration.PLDAtonementBarPadding = pldAtonementBarPadding;
                        _pluginConfiguration.Save();
                    }

                    int pldInterBarOffset = _pluginConfiguration.PLDInterBarOffset;
                    if (ImGui.DragInt("Space Between Bars", ref pldInterBarOffset, 0.1f, -2000, 2000))
                    {
                        _pluginConfiguration.PLDInterBarOffset = pldInterBarOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Mana Bar Color", ref _pluginConfiguration.PLDManaColor);
                    changed |= ImGui.ColorEdit4("Oath Gauge Color", ref _pluginConfiguration.PLDOathGaugeColor);
                    changed |= ImGui.ColorEdit4("Fight or Flight Color",
                        ref _pluginConfiguration.PLDFightOrFlightColor);
                    changed |= ImGui.ColorEdit4("Requiescat Color", ref _pluginConfiguration.PLDRequiescatColor);
                    changed |= ImGui.ColorEdit4("Atonement Color", ref _pluginConfiguration.PLDAtonementColor);
                    changed |= ImGui.ColorEdit4("Bar Not Full Color", ref _pluginConfiguration.PLDEmptyColor);

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Dark Knight")) {
                    var drkBaseXOffset = _pluginConfiguration.DRKBaseXOffset;
                    if (ImGui.DragInt("Base X Offset", ref drkBaseXOffset, .1f, -xOffsetLimit, xOffsetLimit)) {
                        _pluginConfiguration.DRKBaseXOffset = drkBaseXOffset;
                        _pluginConfiguration.Save();
                    }

                    var drkBaseYOffset = _pluginConfiguration.DRKBaseYOffset;
                    if (ImGui.DragInt("Base Y Offset", ref drkBaseYOffset, .1f, -yOffsetLimit, yOffsetLimit)) {
                        _pluginConfiguration.DRKBaseYOffset = drkBaseYOffset;
                        _pluginConfiguration.Save();
                    }

                    var drkManaBarEnabled = _pluginConfiguration.DRKManaBarEnabled;
                    if (ImGui.Checkbox("Mana Bar Enabled", ref drkManaBarEnabled))
                    {
                        _pluginConfiguration.DRKManaBarEnabled = drkManaBarEnabled;
                        _pluginConfiguration.Save();
                    }

                    if (drkManaBarEnabled) {
                        var drkManaBarOverflowEnabled = _pluginConfiguration.DRKManaBarOverflowEnabled;
                        if (ImGui.Checkbox("Mana Bar Overflow Enabled", ref drkManaBarOverflowEnabled))
                        {
                            _pluginConfiguration.DRKManaBarOverflowEnabled = drkManaBarOverflowEnabled;
                            _pluginConfiguration.Save();
                        }

                        var drkManaBarHeight = _pluginConfiguration.DRKManaBarHeight;
                        if (ImGui.DragInt("Mana Bar Height", ref drkManaBarHeight, .1f, 1, 1000)) {
                            _pluginConfiguration.DRKManaBarHeight = drkManaBarHeight;
                            _pluginConfiguration.Save();
                        }

                        var drkManaBarWidth = _pluginConfiguration.DRKManaBarWidth;
                        if (ImGui.DragInt("Mana Bar Width", ref drkManaBarWidth, .1f, 1, 1000)) {
                            _pluginConfiguration.DRKManaBarWidth = drkManaBarWidth;
                            _pluginConfiguration.Save();
                        }

                        var drkManaBarPadding = _pluginConfiguration.DRKManaBarPadding;
                        if (ImGui.DragInt("Mana Bar Padding", ref drkManaBarPadding, .1f, 1, 1000)) {
                            _pluginConfiguration.DRKManaBarPadding = drkManaBarPadding;
                            _pluginConfiguration.Save();
                        }

                        var drkManaBarXOffset = _pluginConfiguration.DRKManaBarXOffset;
                        if (ImGui.DragInt("Mana Bar X Offset", ref drkManaBarXOffset, .1f, -xOffsetLimit, xOffsetLimit)) {
                            _pluginConfiguration.DRKManaBarXOffset = drkManaBarXOffset;
                            _pluginConfiguration.Save();
                        }

                        var drkManaBarYOffset = _pluginConfiguration.DRKManaBarYOffset;
                        if (ImGui.DragInt("Mana Bar Y Offset", ref drkManaBarYOffset, .1f, -yOffsetLimit, yOffsetLimit)) {
                            _pluginConfiguration.DRKManaBarYOffset = drkManaBarYOffset;
                            _pluginConfiguration.Save();
                        }

                        changed |= ImGui.ColorEdit4("Mana Color", ref _pluginConfiguration.DRKManaColor);
                        changed |= ImGui.ColorEdit4("Dark Arts Proc. Color", ref _pluginConfiguration.DRKDarkArtsColor);
                    }

                    var drkBloodGaugeEnabled = _pluginConfiguration.DRKBloodGaugeEnabled;
                    if (ImGui.Checkbox("Blood Gauge Enabled", ref drkBloodGaugeEnabled))
                    {
                        _pluginConfiguration.DRKBloodGaugeEnabled = drkBloodGaugeEnabled;
                        _pluginConfiguration.Save();
                    }

                    if (drkBloodGaugeEnabled) {
                        var drkBloodGaugeSplit = _pluginConfiguration.DRKBloodGaugeSplit;
                        if (ImGui.Checkbox("Split Blood Gauge", ref drkBloodGaugeSplit))
                        {
                            _pluginConfiguration.DRKBloodGaugeSplit = drkBloodGaugeSplit;
                            _pluginConfiguration.Save();
                        }

                        if (! drkBloodGaugeSplit) {
                            var drkBloodGaugeThreshold = _pluginConfiguration.DRKBloodGaugeThreshold;
                            if (ImGui.Checkbox("Draw Blood Gauge Threshold", ref drkBloodGaugeThreshold))
                            {
                                _pluginConfiguration.DRKBloodGaugeThreshold = drkBloodGaugeThreshold;
                                _pluginConfiguration.Save();
                            }
                        }

                        var drkBloodGaugeHeight = _pluginConfiguration.DRKBloodGaugeHeight;
                        if (ImGui.DragInt("Blood Gauge Height", ref drkBloodGaugeHeight, .1f, 1, 1000)) {
                            _pluginConfiguration.DRKBloodGaugeHeight = drkBloodGaugeHeight;
                            _pluginConfiguration.Save();
                        }

                        var drkBloodGaugeWidth = _pluginConfiguration.DRKBloodGaugeWidth;
                        if (ImGui.DragInt("Blood Gauge Width", ref drkBloodGaugeWidth, .1f, 1, 1000)) {
                            _pluginConfiguration.DRKBloodGaugeWidth = drkBloodGaugeWidth;
                            _pluginConfiguration.Save();
                        }

                        var drkBloodGaugePadding = _pluginConfiguration.DRKBloodGaugePadding;
                        if (drkBloodGaugeSplit) {
                            if (ImGui.DragInt("Blood Gauge Padding", ref drkBloodGaugePadding, .1f, 1, 1000)) {
                                _pluginConfiguration.DRKBloodGaugePadding = drkBloodGaugePadding;
                                _pluginConfiguration.Save();
                            }
                        }

                        var drkBloodGaugeXOffset = _pluginConfiguration.DRKBloodGaugeXOffset;
                        if (ImGui.DragInt( "Blood Gauge X Offset", ref drkBloodGaugeXOffset, .1f, -xOffsetLimit, xOffsetLimit)) {
                            _pluginConfiguration.DRKBloodGaugeXOffset = drkBloodGaugeXOffset;
                            _pluginConfiguration.Save();
                        }

                        var drkBloodGaugeYOffset = _pluginConfiguration.DRKBloodGaugeYOffset;
                        if (ImGui.DragInt( "Blood Gauge Y Offset", ref drkBloodGaugeYOffset, .1f, -yOffsetLimit, yOffsetLimit)) {
                            _pluginConfiguration.DRKBloodGaugeYOffset = drkBloodGaugeYOffset;
                            _pluginConfiguration.Save();
                        }

                        changed |= ImGui.ColorEdit4("Blood Color Left", ref _pluginConfiguration.DRKBloodColorLeft);
                        changed |= ImGui.ColorEdit4("Blood Color Right", ref _pluginConfiguration.DRKBloodColorRight);
                        changed |= ImGui.ColorEdit4("Bar Not Full Color", ref _pluginConfiguration.DRKEmptyColor);
                    }

                    var drkBuffBarEnabled = _pluginConfiguration.DRKBuffBarEnabled;
                    if (ImGui.Checkbox("Buff Bar Enabled", ref drkBuffBarEnabled))
                    {
                        _pluginConfiguration.DRKBuffBarEnabled = drkBuffBarEnabled;
                        _pluginConfiguration.Save();
                    }

                    if (drkBuffBarEnabled) {
                        var drkBuffBarHeight = _pluginConfiguration.DRKBuffBarHeight;
                        if (ImGui.DragInt("Buff Bar Height", ref drkBuffBarHeight, .1f, 1, 1000)) {
                            _pluginConfiguration.DRKBuffBarHeight = drkBuffBarHeight;
                            _pluginConfiguration.Save();
                        }

                        var drkBuffBarWidth = _pluginConfiguration.DRKBuffBarWidth;
                        if (ImGui.DragInt("Buff Bar Width", ref drkBuffBarWidth, .1f, 1, 1000)) {
                            _pluginConfiguration.DRKBuffBarWidth = drkBuffBarWidth;
                            _pluginConfiguration.Save();
                        }

                        var drkBuffBarPadding = _pluginConfiguration.DRKBuffBarPadding;
                        if (ImGui.DragInt("Buff Bar Padding", ref drkBuffBarPadding, .1f, 1, 1000)) {
                            _pluginConfiguration.DRKBuffBarPadding = drkBuffBarPadding;
                            _pluginConfiguration.Save();
                        }

                        var drkBuffBarXOffset = _pluginConfiguration.DRKBuffBarXOffset;
                        if (ImGui.DragInt("Buff Bar X Offset", ref drkBuffBarXOffset, .1f, -xOffsetLimit, xOffsetLimit)) {
                            _pluginConfiguration.DRKBuffBarXOffset = drkBuffBarXOffset;
                            _pluginConfiguration.Save();
                        }

                        var drkBuffBarYOffset = _pluginConfiguration.DRKBuffBarYOffset;
                        if (ImGui.DragInt("Buff Bar Y Offset", ref drkBuffBarYOffset, .1f, -yOffsetLimit, yOffsetLimit)) {
                            _pluginConfiguration.DRKBuffBarYOffset = drkBuffBarYOffset;
                            _pluginConfiguration.Save();
                        }

                        changed |= ImGui.ColorEdit4("Blood Weapon Color", ref _pluginConfiguration.DRKBloodWeaponColor);
                        changed |= ImGui.ColorEdit4("Delirium Color", ref _pluginConfiguration.DRKDeliriumColor);
                    }

                    var drkLivingShadowBarEnabled = _pluginConfiguration.DRKLivingShadowBarEnabled;
                    if (ImGui.Checkbox("Living Shadow Bar Enabled", ref drkLivingShadowBarEnabled))
                    {
                        _pluginConfiguration.DRKLivingShadowBarEnabled = drkLivingShadowBarEnabled;
                        _pluginConfiguration.Save();
                    }

                    if (drkLivingShadowBarEnabled) {
                        var drkLivingShadowBarHeight = _pluginConfiguration.DRKLivingShadowBarHeight;
                        if (ImGui.DragInt("Living Shadow Bar Height", ref drkLivingShadowBarHeight, .1f, 1, 1000)) {
                            _pluginConfiguration.DRKLivingShadowBarHeight = drkLivingShadowBarHeight;
                            _pluginConfiguration.Save();
                        }

                        var drkLivingShadowBarWidth = _pluginConfiguration.DRKLivingShadowBarWidth;
                        if (ImGui.DragInt("Living Shadow Bar Width", ref drkLivingShadowBarWidth, .1f, 1, 1000)) {
                            _pluginConfiguration.DRKLivingShadowBarWidth = drkLivingShadowBarWidth;
                            _pluginConfiguration.Save();
                        }

                        var drkLivingShadowBarPadding = _pluginConfiguration.DRKLivingShadowBarPadding;
                        if (ImGui.DragInt("Living Shadow Bar Padding", ref drkLivingShadowBarPadding, .1f, 1, 1000)) {
                            _pluginConfiguration.DRKLivingShadowBarPadding = drkLivingShadowBarPadding;
                            _pluginConfiguration.Save();
                        }

                        var drkLivingShadowBarXOffset = _pluginConfiguration.DRKLivingShadowBarXOffset;
                        if (ImGui.DragInt("Living Shadow Bar X Offset", ref drkLivingShadowBarXOffset, .1f, -xOffsetLimit, xOffsetLimit)) {
                            _pluginConfiguration.DRKLivingShadowBarXOffset = drkLivingShadowBarXOffset;
                            _pluginConfiguration.Save();
                        }

                        var drkLivingShadowBarYOffset = _pluginConfiguration.DRKLivingShadowBarYOffset;
                        if (ImGui.DragInt("Living Shadow Bar Y Offset", ref drkLivingShadowBarYOffset, .1f, -yOffsetLimit, yOffsetLimit)) {
                            _pluginConfiguration.DRKLivingShadowBarYOffset = drkLivingShadowBarYOffset;
                            _pluginConfiguration.Save();
                        }

                        changed |= ImGui.ColorEdit4("Living Shadow Color", ref _pluginConfiguration.DRKLivingShadowColor);
                    }

                    var drkInterBarOffset = _pluginConfiguration.DRKInterBarOffset;
                    if (ImGui.DragInt("Space Between Bars", ref drkInterBarOffset, .1f, 1, 1000)) {
                        _pluginConfiguration.DRKInterBarOffset = drkInterBarOffset;
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

                    changed |= ImGui.Checkbox("Show Aether Bar", ref _pluginConfiguration.SCHShowAetherBar);
                    changed |= ImGui.Checkbox("Show Fairy Bar", ref _pluginConfiguration.SCHShowFairyBar);
                    changed |= ImGui.Checkbox("Show Bio Bar", ref _pluginConfiguration.SCHShowBioBar);
                    changed |= ImGui.Checkbox("Show Primary Resource Bar",
                        ref _pluginConfiguration.SCHShowPrimaryResourceBar);

                    changed |= ImGui.ColorEdit4("Fairy Bar Color", ref _pluginConfiguration.SchFairyColor);
                    changed |= ImGui.ColorEdit4("Aether Bar Color", ref _pluginConfiguration.SchAetherColor);
                    changed |= ImGui.ColorEdit4("Bar Not Full Color", ref _pluginConfiguration.SchEmptyColor);

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("White Mage"))
                {

                    var lillyBarHeight = _pluginConfiguration.LillyBarHeight;
                    if (ImGui.DragInt("Lilly Gauge Height", ref lillyBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.LillyBarHeight = lillyBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var lillyBarWidth = _pluginConfiguration.LillyBarWidth;
                    if (ImGui.DragInt("Lilly Gauge Width", ref lillyBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.LillyBarWidth = lillyBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var lillyBarX = _pluginConfiguration.LillyBarX;
                    if (ImGui.DragInt("Lilly Gauge X Offset", ref lillyBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.LillyBarX = lillyBarX;
                        _pluginConfiguration.Save();
                    }

                    var lillyBarY = _pluginConfiguration.LillyBarY;
                    if (ImGui.DragInt("Lilly Gauge Y Offset", ref lillyBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.LillyBarY = lillyBarY;
                        _pluginConfiguration.Save();
                    }

                    var lillyBarPad = _pluginConfiguration.LillyBarPad;
                    if (ImGui.DragInt("Lilly Gauge Padding", ref lillyBarPad, .1f, -100, 1000))
                    {
                        _pluginConfiguration.LillyBarPad = lillyBarPad;
                        _pluginConfiguration.Save();
                    }

                    var bloodLillyBarHeight = _pluginConfiguration.BloodLillyBarHeight;
                    if (ImGui.DragInt("Blood Lilly Gauge Height", ref bloodLillyBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BloodLillyBarHeight = bloodLillyBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var bloodLillyBarWidth = _pluginConfiguration.BloodLillyBarWidth;
                    if (ImGui.DragInt("Blood Lilly Gauge Width", ref bloodLillyBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BloodLillyBarWidth = bloodLillyBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var bloodLillyBarX = _pluginConfiguration.BloodLillyBarX;
                    if (ImGui.DragInt("Blood Lilly Gauge X Offset", ref bloodLillyBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.BloodLillyBarX = bloodLillyBarX;
                        _pluginConfiguration.Save();
                    }

                    var bloodLillyBarY = _pluginConfiguration.BloodLillyBarY;
                    if (ImGui.DragInt("Blood Lilly Gauge Y Offset", ref bloodLillyBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.BloodLillyBarY = bloodLillyBarY;
                        _pluginConfiguration.Save();
                    }

                    var bloodLillyBarPad = _pluginConfiguration.BloodLillyBarPad;
                    if (ImGui.DragInt("Blood Lilly Gauge Padding", ref bloodLillyBarPad, .1f, -100, 1000))
                    {
                        _pluginConfiguration.BloodLillyBarPad = bloodLillyBarPad;
                        _pluginConfiguration.Save();
                    }

                    var diaBarHeight = _pluginConfiguration.DiaBarHeight;
                    if (ImGui.DragInt("Dia Bar Height", ref diaBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.DiaBarHeight = diaBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var diaBarWidth = _pluginConfiguration.DiaBarWidth;
                    if (ImGui.DragInt("Dia Bar Width", ref diaBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.DiaBarWidth = diaBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var diaBarX = _pluginConfiguration.DiaBarX;
                    if (ImGui.DragInt("Dia Bar X Offset", ref diaBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.DiaBarX = diaBarX;
                        _pluginConfiguration.Save();
                    }

                    var diaBarY = _pluginConfiguration.DiaBarY;
                    if (ImGui.DragInt("Dia Bar Y Offset", ref diaBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.DiaBarY = diaBarY;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Lilly Bar", ref _pluginConfiguration.WHMShowLillyBar);
                    changed |= ImGui.Checkbox("Show Dia Bar", ref _pluginConfiguration.WHMShowDiaBar);
                    changed |= ImGui.Checkbox("Show Primary Resource Bar",
                        ref _pluginConfiguration.WHMShowPrimaryResourceBar);

                    changed |= ImGui.ColorEdit4("Lilly Bar Color", ref _pluginConfiguration.WhmLillyColor);
                    changed |= ImGui.ColorEdit4("Lilly Charging Bar Color",
                        ref _pluginConfiguration.WhmLillyChargingColor);
                    changed |= ImGui.ColorEdit4("Blood Lilly Bar Color", ref _pluginConfiguration.WhmBloodLillyColor);
                    changed |= ImGui.ColorEdit4("Dia Bar Color", ref _pluginConfiguration.WhmDiaColor);
                    changed |= ImGui.ColorEdit4("Bar Not Full Color", ref _pluginConfiguration.WhmEmptyColor);

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Astrologian"))
                {

                    var ASTDrawBarHeight = _pluginConfiguration.ASTDrawBarHeight;
                    if (ImGui.DragInt("Draw Gauge Height", ref ASTDrawBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ASTDrawBarHeight = ASTDrawBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var ASTDrawBarWidth = _pluginConfiguration.ASTDrawBarWidth;
                    if (ImGui.DragInt("Draw Gauge Width", ref ASTDrawBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ASTDrawBarWidth = ASTDrawBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var ASTDrawBarX = _pluginConfiguration.ASTDrawBarX;
                    if (ImGui.DragInt("Draw Gauge X Offset", ref ASTDrawBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.ASTDrawBarX = ASTDrawBarX;
                        _pluginConfiguration.Save();
                    }

                    var ASTDrawBarY = _pluginConfiguration.ASTDrawBarY;
                    if (ImGui.DragInt("Draw Gauge Y Offset", ref ASTDrawBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.ASTDrawBarY = ASTDrawBarY;
                        _pluginConfiguration.Save();
                    }

                    var ASTDivinationHeight = _pluginConfiguration.ASTDivinationHeight;
                    if (ImGui.DragInt("Divination Gauge Height", ref ASTDivinationHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ASTDivinationHeight = ASTDivinationHeight;
                        _pluginConfiguration.Save();
                    }

                    var ASTDivinationWidth = _pluginConfiguration.ASTDivinationWidth;
                    if (ImGui.DragInt("Divination Gauge Width", ref ASTDivinationWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ASTDivinationWidth = ASTDivinationWidth;
                        _pluginConfiguration.Save();
                    }

                    var ASTDivinationBarX = _pluginConfiguration.ASTDivinationBarX;
                    if (ImGui.DragInt("Divination Gauge X Offset", ref ASTDivinationBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.ASTDivinationBarX = ASTDivinationBarX;
                        _pluginConfiguration.Save();
                    }

                    var ASTDivinationBarY = _pluginConfiguration.ASTDivinationBarY;
                    if (ImGui.DragInt("Divination Gauge Y Offset", ref ASTDivinationBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.ASTDivinationBarY = ASTDivinationBarY;
                        _pluginConfiguration.Save();
                    }

                    var ASTDivinationBarPad = _pluginConfiguration.ASTDivinationBarPad;
                    if (ImGui.DragInt("Divination Padding", ref ASTDivinationBarPad, .1f, -100, 1000))
                    {
                        _pluginConfiguration.ASTDivinationBarPad = ASTDivinationBarPad;
                        _pluginConfiguration.Save();
                    }

                    var ASTDotBarHeight = _pluginConfiguration.ASTDotBarHeight;
                    if (ImGui.DragInt("Dot Gauge Height", ref ASTDotBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ASTDotBarHeight = ASTDotBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var ASTDotBarWidth = _pluginConfiguration.ASTDotBarWidth;
                    if (ImGui.DragInt("Dot Gauge Width", ref ASTDotBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ASTDotBarWidth = ASTDotBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var ASTDotBarX = _pluginConfiguration.ASTDotBarX;
                    if (ImGui.DragInt("Dot Gauge X Offset", ref ASTDotBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.ASTDotBarX = ASTDotBarX;
                        _pluginConfiguration.Save();
                    }

                    var ASTDotBarY = _pluginConfiguration.ASTDotBarY;
                    if (ImGui.DragInt("Dot Gauge Y Offset", ref ASTDotBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.ASTDotBarY = ASTDotBarY;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Divination Bar", ref _pluginConfiguration.ASTShowDivinationBar);
                    changed |= ImGui.Checkbox("Show Draw Bar", ref _pluginConfiguration.ASTShowDrawBar);
                    changed |= ImGui.Checkbox("Show Dot Bar", ref _pluginConfiguration.ASTShowDotBar);
                    changed |= ImGui.Checkbox("Show Primary Resource Bar",
                        ref _pluginConfiguration.ASTShowPrimaryResourceBar);

                    changed |= ImGui.ColorEdit4("Seal Sun Color", ref _pluginConfiguration.ASTSealSunColor);
                    changed |= ImGui.ColorEdit4("Seal Lunar Color", ref _pluginConfiguration.ASTSealLunarColor);
                    changed |= ImGui.ColorEdit4("Seal Celestial Color", ref _pluginConfiguration.ASTSealCelestialColor);
                    changed |= ImGui.ColorEdit4("Dot Color", ref _pluginConfiguration.ASTDotColor);
                    changed |= ImGui.ColorEdit4("Bar Empty Color", ref _pluginConfiguration.ASTEmptyColor);

                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }

        private void DrawJobsMeleeConfig()
        {
            if (ImGui.BeginTabBar("##melee-tabs"))
            {
            if (ImGui.BeginTabItem("Samurai"))
                {

                    var samHiganbanaBarX = _pluginConfiguration.SamHiganbanaBarX;
                    if (ImGui.DragInt("Higanbana X Offset", ref samHiganbanaBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SamHiganbanaBarX = samHiganbanaBarX;
                        _pluginConfiguration.Save();
                    }

                    var samHiganbanaBarY = _pluginConfiguration.SamHiganbanaBarY;
                    if (ImGui.DragInt("Higanbana Y Offset", ref samHiganbanaBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SamHiganbanaBarY = samHiganbanaBarY;
                        _pluginConfiguration.Save();
                    }

                    var samHiganbanaBarHeight = _pluginConfiguration.SamHiganbanaBarHeight;
                    if (ImGui.DragInt("Higanbana Height", ref samHiganbanaBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SamHiganbanaBarHeight = samHiganbanaBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var samHiganbanaBarWidth = _pluginConfiguration.SamHiganbanaBarWidth;
                    if (ImGui.DragInt("Higanbana Width", ref samHiganbanaBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SamHiganbanaBarWidth = samHiganbanaBarWidth;
                        _pluginConfiguration.Save();
                    }
                    var samBuffsBarX = _pluginConfiguration.SamBuffsBarX;
                    if (ImGui.DragInt("Shifu/Jinpu X Offset", ref samBuffsBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SamBuffsBarX = samBuffsBarX;
                        _pluginConfiguration.Save();
                    }

                    var samBuffsBarY = _pluginConfiguration.SamBuffsBarY;
                    if (ImGui.DragInt("Shifu/Jinpu Y Offset", ref samBuffsBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SamBuffsBarY = samBuffsBarY;
                        _pluginConfiguration.Save();
                    }

                    var samBuffsBarHeight = _pluginConfiguration.SamBuffsBarHeight;
                    if (ImGui.DragInt("Shifu/Jinpu Height", ref samBuffsBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SamBuffsBarHeight = samBuffsBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var samBuffsBarWidth = _pluginConfiguration.SamBuffsBarWidth;
                    if (ImGui.DragInt("Shifu/Jinpu Width", ref samBuffsBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SamBuffsBarWidth = samBuffsBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var samKenkiBarX = _pluginConfiguration.SamKenkiBarX;
                    if (ImGui.DragInt("Kenki X Offset", ref samKenkiBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SamKenkiBarX = samKenkiBarX;
                        _pluginConfiguration.Save();
                    }

                    var samKenkiBarY = _pluginConfiguration.SamKenkiBarY;
                    if (ImGui.DragInt("Kenki Y Offset", ref samKenkiBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SamKenkiBarY = samKenkiBarY;
                        _pluginConfiguration.Save();
                    }

                    var samKenkiBarHeight = _pluginConfiguration.SamKenkiBarHeight;
                    if (ImGui.DragInt("Kenki Height", ref samKenkiBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SamKenkiBarHeight = samKenkiBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var samKenkiBarWidth = _pluginConfiguration.SamKenkiBarWidth;
                    if (ImGui.DragInt("Kenki Width", ref samKenkiBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SamKenkiBarWidth = samKenkiBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var samSenBarX = _pluginConfiguration.SamSenBarX;
                    if (ImGui.DragInt("Sen X Offset", ref samSenBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SamSenBarX = samSenBarX;
                        _pluginConfiguration.Save();
                    }

                    var samSenBarY = _pluginConfiguration.SamSenBarY;
                    if (ImGui.DragInt("Sen Y Offset", ref samSenBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SamSenBarY = samSenBarY;
                        _pluginConfiguration.Save();
                    }

                    var samSenBarHeight = _pluginConfiguration.SamSenBarHeight;
                    if (ImGui.DragInt("Sen Height", ref samSenBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SamSenBarHeight = samSenBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var samSenBarWidth = _pluginConfiguration.SamSenBarWidth;
                    if (ImGui.DragInt("Sen Width", ref samSenBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SamSenBarWidth = samSenBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var SAMSenPadding = _pluginConfiguration.SAMSenPadding;
                    if (ImGui.DragInt("Sen Bar Padding", ref SAMSenPadding, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SAMSenPadding = SAMSenPadding;
                        _pluginConfiguration.Save();
                    }

                    var samMeditationBarX = _pluginConfiguration.SamMeditationBarX;
                    if (ImGui.DragInt("Meditation X Offset", ref samMeditationBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SamMeditationBarX = samMeditationBarX;
                        _pluginConfiguration.Save();
                    }

                    var samMeditationBarY = _pluginConfiguration.SamMeditationBarY;
                    if (ImGui.DragInt("Meditation Y Offset", ref samMeditationBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SamMeditationBarY = samMeditationBarY;
                        _pluginConfiguration.Save();
                    }

                    var samMeditationBarHeight = _pluginConfiguration.SamMeditationBarHeight;
                    if (ImGui.DragInt("Meditation Height", ref samMeditationBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SamMeditationBarHeight = samMeditationBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var samMeditationBarWidth = _pluginConfiguration.SamMeditationBarWidth;
                    if (ImGui.DragInt("Meditation Width", ref samMeditationBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SamMeditationBarWidth = samMeditationBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var SAMMeditationPadding = _pluginConfiguration.SAMMeditationPadding;
                    if (ImGui.DragInt("Meditation Bar Padding", ref SAMMeditationPadding, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SAMMeditationPadding = SAMMeditationPadding;
                        _pluginConfiguration.Save();
                    }

                    var SAMBuffsPadding = _pluginConfiguration.SAMBuffsPadding;
                    if (ImGui.DragInt("Jinpu/Shifu Bar Padding", ref SAMBuffsPadding, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SAMBuffsPadding = SAMBuffsPadding;
                        _pluginConfiguration.Save();
                    }

                    var SAMGaugeEnabled = _pluginConfiguration.SAMGaugeEnabled;
                    if (ImGui.Checkbox("Kenki Enabled", ref SAMGaugeEnabled))
                    {
                        _pluginConfiguration.SAMGaugeEnabled = SAMGaugeEnabled;
                        _pluginConfiguration.Save();
                    }

                    var SAMSenEnabled = _pluginConfiguration.SAMSenEnabled;
                    if (ImGui.Checkbox("Sen Enabled", ref SAMSenEnabled))
                    {
                        _pluginConfiguration.SAMSenEnabled = SAMSenEnabled;
                        _pluginConfiguration.Save();
                    }

                    var SAMMeditationEnabled = _pluginConfiguration.SAMMeditationEnabled;
                    if (ImGui.Checkbox("Meditation Enabled", ref SAMMeditationEnabled))
                    {
                        _pluginConfiguration.SAMMeditationEnabled = SAMMeditationEnabled;
                        _pluginConfiguration.Save();
                    }

                    var SAMHiganbanaEnabled = _pluginConfiguration.SAMHiganbanaEnabled;
                    if (ImGui.Checkbox("Higanbana Enabled", ref SAMHiganbanaEnabled))
                    {
                        _pluginConfiguration.SAMHiganbanaEnabled = SAMHiganbanaEnabled;
                        _pluginConfiguration.Save();
                    }

                    var SAMBuffsEnabled = _pluginConfiguration.SAMBuffsEnabled;
                    if (ImGui.Checkbox("Jinpu/Shifu Enabled", ref SAMBuffsEnabled))
                    {
                        _pluginConfiguration.SAMBuffsEnabled = SAMBuffsEnabled;
                        _pluginConfiguration.Save();
                    }

                    var SAMKenkiText = _pluginConfiguration.SAMKenkiText;
                    if (ImGui.Checkbox("Show Current Kenki", ref SAMKenkiText))
                    {
                        _pluginConfiguration.SAMKenkiText = SAMKenkiText;
                        _pluginConfiguration.Save();
                    }

                    var SAMHiganbanaText = _pluginConfiguration.SAMHiganbanaText;
                    if (ImGui.Checkbox("Show Higanbana Duration Text", ref SAMHiganbanaText))
                    {
                        _pluginConfiguration.SAMHiganbanaText = SAMHiganbanaText;
                        _pluginConfiguration.Save();
                    }

                    var SAMBuffText = _pluginConfiguration.SAMBuffText;
                    if (ImGui.Checkbox("Show Jinpu/Shifu Duration Text", ref SAMBuffText))
                    {
                        _pluginConfiguration.SAMBuffText = SAMBuffText;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Higanbana Bar Color", ref _pluginConfiguration.SamHiganbanaColor);
                    changed |= ImGui.ColorEdit4("Shifu Bar Color", ref _pluginConfiguration.SamShifuColor);
                    changed |= ImGui.ColorEdit4("Jinpu Bar Color", ref _pluginConfiguration.SamJinpuColor);
                    changed |= ImGui.ColorEdit4("Setsu Color", ref _pluginConfiguration.SamSetsuColor);
                    changed |= ImGui.ColorEdit4("Getsu Color", ref _pluginConfiguration.SamGetsuColor);
                    changed |= ImGui.ColorEdit4("Ka Color", ref _pluginConfiguration.SamKaColor);
                    changed |= ImGui.ColorEdit4("Meditation Color", ref _pluginConfiguration.SamMeditationColor);
                    changed |= ImGui.ColorEdit4("Kenki Color", ref _pluginConfiguration.SamKenkiColor);
                    changed |= ImGui.ColorEdit4("Expiry Color", ref _pluginConfiguration.SamExpiryColor);
                    changed |= ImGui.ColorEdit4("Empty Color", ref _pluginConfiguration.SamEmptyColor);

                    ImGui.EndTabItem();
                }
            if (ImGui.BeginTabItem("Ninja"))
                {
                    var NINBaseXOffset = _pluginConfiguration.NINBaseXOffset;
                    if (ImGui.DragInt("NIN Base X Offset", ref NINBaseXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.NINBaseXOffset = NINBaseXOffset;
                        _pluginConfiguration.Save();
                    }

                    var NINBaseYOffset = _pluginConfiguration.NINBaseYOffset;
                    if (ImGui.DragInt("NIN Base Y Offset", ref NINBaseYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.NINBaseYOffset = NINBaseYOffset;
                        _pluginConfiguration.Save();
                    }

                    var NINHutonGaugeHeight = _pluginConfiguration.NINHutonGaugeHeight;
                    if (ImGui.DragInt("Huton Gauge Height", ref NINHutonGaugeHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.NINHutonGaugeHeight = NINHutonGaugeHeight;
                        _pluginConfiguration.Save();
                    }

                    var NINHutonGaugeWidth = _pluginConfiguration.NINHutonGaugeWidth;
                    if (ImGui.DragInt("Huton Gauge Width", ref NINHutonGaugeWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.NINHutonGaugeWidth = NINHutonGaugeWidth;
                        _pluginConfiguration.Save();
                    }

                    var NINNinkiGaugeHeight = _pluginConfiguration.NINNinkiGaugeHeight;
                    if (ImGui.DragInt("Ninki Gauge Height", ref NINNinkiGaugeHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.NINNinkiGaugeHeight = NINNinkiGaugeHeight;
                        _pluginConfiguration.Save();
                    }

                    var NINNinkiGaugeWidth = _pluginConfiguration.NINNinkiGaugeWidth;
                    if (ImGui.DragInt("Ninki Gauge Width", ref NINNinkiGaugeWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.NINNinkiGaugeWidth = NINNinkiGaugeWidth;
                        _pluginConfiguration.Save();
                    }

                    var NINNinkiGaugePadding = _pluginConfiguration.NINNinkiGaugePadding;
                    if (ImGui.DragInt("Ninki Gauge Padding", ref NINNinkiGaugePadding, .1f, 1, 1000))
                    {
                        _pluginConfiguration.NINNinkiGaugePadding = NINNinkiGaugePadding;
                        _pluginConfiguration.Save();
                    }

                    var NINNinkiGaugeXOffset = _pluginConfiguration.NINNinkiGaugeXOffset;
                    if (ImGui.DragInt("Ninki Gauge X Offset", ref NINNinkiGaugeXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.NINNinkiGaugeXOffset = NINNinkiGaugeXOffset;
                        _pluginConfiguration.Save();
                    }

                    var NINNinkiGaugeYOffset = _pluginConfiguration.NINNinkiGaugeYOffset;
                    if (ImGui.DragInt("Ninki Gauge Y Offset", ref NINNinkiGaugeYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.NINNinkiGaugeYOffset = NINNinkiGaugeYOffset;
                        _pluginConfiguration.Save();
                    }


                    var NINInterBarOffset = _pluginConfiguration.NINInterBarOffset;
                    if (ImGui.DragInt("Space Between Bars", ref NINInterBarOffset, .1f, 1, 1000))
                    {
                        _pluginConfiguration.NINInterBarOffset = NINInterBarOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Empty Color", ref _pluginConfiguration.NINEmptyColor);
                    changed |= ImGui.ColorEdit4("Huton Bar Color", ref _pluginConfiguration.NINHutonColor);
                    changed |= ImGui.ColorEdit4("Ninki Bar Color", ref _pluginConfiguration.NINNinkiColor);

                    ImGui.EndTabItem();
                }
                            if (ImGui.BeginTabItem("Monk"))
                {
                    var MNKDemolishHeight = _pluginConfiguration.MNKDemolishHeight;
                    if (ImGui.DragInt("Demolish Height", ref MNKDemolishHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MNKDemolishHeight = MNKDemolishHeight;
                        _pluginConfiguration.Save();
                    }
                    
                    var MNKDemolishWidth = _pluginConfiguration.MNKDemolishWidth;
                    if (ImGui.DragInt("Demolish Width", ref MNKDemolishWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MNKDemolishWidth = MNKDemolishWidth;
                        _pluginConfiguration.Save();
                    }
                    
                    var MNKDemolishXOffset = _pluginConfiguration.MNKDemolishXOffset;
                    if (ImGui.DragInt("Demolish X Offset", ref MNKDemolishXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKDemolishXOffset = MNKDemolishXOffset;
                        _pluginConfiguration.Save();
                    }
                    
                    var MNKDemolishYOffset = _pluginConfiguration.MNKDemolishYOffset;
                    if (ImGui.DragInt("Demolish Y Offset", ref MNKDemolishYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKDemolishYOffset = MNKDemolishYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Demolish Color", ref _pluginConfiguration.MNKDemolishColor);

                    var MNKTimeDemoXOffset = _pluginConfiguration.MNKTimeDemoXOffset;
                    if (ImGui.DragInt("Demolish Timer X Offset", ref MNKTimeDemoXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKTimeDemoXOffset = MNKTimeDemoXOffset;
                        _pluginConfiguration.Save();
                    }

                    var MNKTimeDemoYOffset = _pluginConfiguration.MNKTimeDemoYOffset;
                    if (ImGui.DragInt("Demolish Timer Y Offset", ref MNKTimeDemoYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKTimeDemoYOffset = MNKTimeDemoYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Demolish Timer", ref _pluginConfiguration.ShowDemolishTime);

                    var MNKChakraHeight = _pluginConfiguration.MNKChakraHeight;
                    if (ImGui.DragInt("Chakra Height", ref MNKChakraHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MNKChakraHeight = MNKChakraHeight;
                        _pluginConfiguration.Save();
                    }
                    
                    var MNKChakraWidth = _pluginConfiguration.MNKChakraWidth;
                    if (ImGui.DragInt("Chakra Width", ref MNKChakraWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MNKChakraWidth = MNKChakraWidth;
                        _pluginConfiguration.Save();
                    }
                    
                    var MNKChakraXOffset = _pluginConfiguration.MNKChakraXOffset;
                    if (ImGui.DragInt("Chakra X Offset", ref MNKChakraXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKChakraXOffset = MNKChakraXOffset;
                        _pluginConfiguration.Save();
                    }
                    
                    var MNKChakraYOffset = _pluginConfiguration.MNKChakraYOffset;
                    if (ImGui.DragInt("Chakra Y Offset", ref MNKChakraYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKChakraYOffset = MNKChakraYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Chakra Color", ref _pluginConfiguration.MNKChakraColor);

                    var MNKBuffHeight = _pluginConfiguration.MNKBuffHeight;
                    if (ImGui.DragInt("Buff Height", ref MNKBuffHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MNKBuffHeight = MNKBuffHeight;
                        _pluginConfiguration.Save();
                    }
                    
                    var MNKBuffWidth = _pluginConfiguration.MNKBuffWidth;
                    if (ImGui.DragInt("Buff Width", ref MNKBuffWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MNKBuffWidth = MNKBuffWidth;
                        _pluginConfiguration.Save();
                    }
                    
                    var MNKBuffXOffset = _pluginConfiguration.MNKBuffXOffset;
                    if (ImGui.DragInt("Buff X Offset", ref MNKBuffXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKBuffXOffset = MNKBuffXOffset;
                        _pluginConfiguration.Save();
                    }
                    
                    var MNKBuffYOffset = _pluginConfiguration.MNKBuffYOffset;
                    if (ImGui.DragInt("Buff Y Offset", ref MNKBuffYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKBuffYOffset = MNKBuffYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Leaden Fist Color", ref _pluginConfiguration.MNKLeadenFistColor);
                    changed |= ImGui.ColorEdit4("Twin Snakes Color", ref _pluginConfiguration.MNKTwinSnakesColor);

                    var MNKTimeTwinXOffset = _pluginConfiguration.MNKTimeTwinXOffset;
                    if (ImGui.DragInt("Twin Timer X Offset", ref MNKTimeTwinXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKTimeTwinXOffset = MNKTimeTwinXOffset;
                        _pluginConfiguration.Save();
                    }

                    var MNKTimeTwinYOffset = _pluginConfiguration.MNKTimeTwinYOffset;
                    if (ImGui.DragInt("Twin Timer Y Offset", ref MNKTimeTwinYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKTimeTwinYOffset = MNKTimeTwinYOffset;
                        _pluginConfiguration.Save();
                    }

                    var MNKTimeLeadenXOffset = _pluginConfiguration.MNKTimeLeadenXOffset;
                    if (ImGui.DragInt("Leaden Timer X Offset", ref MNKTimeLeadenXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKTimeLeadenXOffset = MNKTimeLeadenXOffset;
                        _pluginConfiguration.Save();
                    }

                    var MNKTimeLeadenYOffset = _pluginConfiguration.MNKTimeLeadenYOffset;
                    if (ImGui.DragInt("Leaden Timer Y Offset", ref MNKTimeLeadenYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MNKTimeLeadenYOffset = MNKTimeLeadenYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Buff Timers", ref _pluginConfiguration.ShowBuffTime);

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Dragoon"))
                {
                    var eyeOfTheDragonHeight = _pluginConfiguration.DRGEyeOfTheDragonHeight;
                    if (ImGui.DragInt("Eye of the Dragon Bar Height", ref eyeOfTheDragonHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.DRGEyeOfTheDragonHeight = eyeOfTheDragonHeight;
                        _pluginConfiguration.Save();
                    }

                    var eyeOfTheDragonBarWidth = _pluginConfiguration.DRGEyeOfTheDragonBarWidth;
                    if (ImGui.DragInt("Eye of the Dragon Bar Width", ref eyeOfTheDragonBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.DRGEyeOfTheDragonBarWidth = eyeOfTheDragonBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var eyeOfTheDragonPadding = _pluginConfiguration.DRGEyeOfTheDragonPadding;
                    if (ImGui.DragInt("Space between Eye of the Dragon Bars", ref eyeOfTheDragonPadding, .1f, 0, 1000))
                    {
                        _pluginConfiguration.DRGEyeOfTheDragonPadding = eyeOfTheDragonPadding;
                        _pluginConfiguration.Save();
                    }

                    var drgBaseXOffset = _pluginConfiguration.DRGBaseXOffset;
                    if (ImGui.DragInt("Base X Offset", ref drgBaseXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.DRGBaseXOffset = drgBaseXOffset;
                        _pluginConfiguration.Save();
                    }

                    var drgBaseYOffset = _pluginConfiguration.DRGBaseYOffset;
                    if (ImGui.DragInt("Base Y Offset", ref drgBaseYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.DRGBaseYOffset = drgBaseYOffset;
                        _pluginConfiguration.Save();
                    }

                    var drgBloodBarHeight = _pluginConfiguration.DRGBloodBarHeight;
                    if (ImGui.DragInt("Height of Blood/Life Bar", ref drgBloodBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.DRGBloodBarHeight = drgBloodBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var drgDisembowelBarHeight = _pluginConfiguration.DRGDisembowelBarHeight;
                    if (ImGui.DragInt("Height of Disembowel Bar", ref drgDisembowelBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.DRGDisembowelBarHeight = drgDisembowelBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var drgChaosThrustBarHeight = _pluginConfiguration.DRGChaosThrustBarHeight;
                    if (ImGui.DragInt("Height of Chaos Thrust Bar", ref drgChaosThrustBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.DRGChaosThrustBarHeight = drgChaosThrustBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var drgInterBarOffset = _pluginConfiguration.DRGInterBarOffset;
                    if (ImGui.DragInt("Space Between Bars", ref drgInterBarOffset, .1f, 0, 1000))
                    {
                        _pluginConfiguration.DRGInterBarOffset = drgInterBarOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Chaos Thrust Timer", ref _pluginConfiguration.DRGShowChaosThrustTimer);
                    changed |= ImGui.Checkbox("Show Disembowel Timer", ref _pluginConfiguration.DRGShowDisembowelBuffTimer);
                    changed |= ImGui.Checkbox("Show Chaos Thrust Text", ref _pluginConfiguration.DRGShowChaosThrustText);
                    changed |= ImGui.Checkbox("Show Blood/Life of the Dragon Text", ref _pluginConfiguration.DRGShowBloodText);
                    changed |= ImGui.Checkbox("Show Disembowel Text", ref _pluginConfiguration.DRGShowDisembowelText);

                    changed |= ImGui.ColorEdit4("Eye of the Dragon Color", ref _pluginConfiguration.DRGEyeOfTheDragonColor);
                    changed |= ImGui.ColorEdit4("Blood of the Dragon Color", ref _pluginConfiguration.DRGBloodOfTheDragonColor);
                    changed |= ImGui.ColorEdit4("Life of the Dragon Color", ref _pluginConfiguration.DRGLifeOfTheDragonColor);
                    changed |= ImGui.ColorEdit4("Disembowel Color", ref _pluginConfiguration.DRGDisembowelColor);
                    changed |= ImGui.ColorEdit4("Chaos Thrust Color", ref _pluginConfiguration.DRGChaosThrustColor);
                    changed |= ImGui.ColorEdit4("Bar Not Full Color", ref _pluginConfiguration.DRGEmptyColor);
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }

        private void DrawJobsRangedConfig()
        {
            if (ImGui.BeginTabBar("##ranged-tabs"))
            {
                if (ImGui.BeginTabItem("Machinist"))
                {
                    var overheatHeight = _pluginConfiguration.MCHOverheatHeight;
                    if (ImGui.DragInt("Overheat Height", ref overheatHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MCHOverheatHeight = overheatHeight;
                        _pluginConfiguration.Save();
                    }

                    var overheatWidth = _pluginConfiguration.MCHOverheatWidth;
                    if (ImGui.DragInt("Overheat Width", ref overheatWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MCHOverheatWidth = overheatWidth;
                        _pluginConfiguration.Save();
                    }

                    var mchBaseXOffset = _pluginConfiguration.MCHBaseXOffset;
                    if (ImGui.DragInt("MCH Base X Offset", ref mchBaseXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MCHBaseXOffset = mchBaseXOffset;
                        _pluginConfiguration.Save();
                    }

                    var mchBaseYOffset = _pluginConfiguration.MCHBaseYOffset;
                    if (ImGui.DragInt("MCH Base Y Offset", ref mchBaseYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MCHBaseYOffset = mchBaseYOffset;
                        _pluginConfiguration.Save();
                    }

                    var heatGaugeHeight = _pluginConfiguration.MCHHeatGaugeHeight;
                    if (ImGui.DragInt("Heat Gauge Height", ref heatGaugeHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MCHHeatGaugeHeight = heatGaugeHeight;
                        _pluginConfiguration.Save();
                    }

                    var heatGaugeWidth = _pluginConfiguration.MCHHeatGaugeWidth;
                    if (ImGui.DragInt("Heat Gauge Width", ref heatGaugeWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MCHHeatGaugeWidth = heatGaugeWidth;
                        _pluginConfiguration.Save();
                    }

                    var heatGaugePadding = _pluginConfiguration.MCHHeatGaugePadding;
                    if (ImGui.DragInt("Heat Gauge Padding", ref heatGaugePadding, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MCHHeatGaugePadding = heatGaugePadding;
                        _pluginConfiguration.Save();
                    }

                    var heatGaugeXOffset = _pluginConfiguration.MCHHeatGaugeXOffset;
                    if (ImGui.DragInt("Heat Gauge X Offset", ref heatGaugeXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MCHHeatGaugeXOffset = heatGaugeXOffset;
                        _pluginConfiguration.Save();
                    }

                    var heatGaugeYOffset = _pluginConfiguration.MCHHeatGaugeYOffset;
                    if (ImGui.DragInt("Heat Gauge Y Offset", ref heatGaugeYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MCHHeatGaugeYOffset = heatGaugeYOffset;
                        _pluginConfiguration.Save();
                    }

                    var batteryGaugeHeight = _pluginConfiguration.MCHBatteryGaugeHeight;
                    if (ImGui.DragInt("Battery Gauge Height", ref batteryGaugeHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MCHBatteryGaugeHeight = batteryGaugeHeight;
                        _pluginConfiguration.Save();
                    }

                    var batteryGaugeWidth = _pluginConfiguration.MCHBatteryGaugeWidth;
                    if (ImGui.DragInt("Battery Gauge Width", ref batteryGaugeWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MCHBatteryGaugeWidth = batteryGaugeWidth;
                        _pluginConfiguration.Save();
                    }

                    var batteryGaugePadding = _pluginConfiguration.MCHBatteryGaugePadding;
                    if (ImGui.DragInt("Battery Gauge Padding", ref batteryGaugePadding, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MCHBatteryGaugePadding = batteryGaugePadding;
                        _pluginConfiguration.Save();
                    }

                    var batteryGaugeXOffset = _pluginConfiguration.MCHBatteryGaugeXOffset;
                    if (ImGui.DragInt("Battery Gauge X Offset", ref batteryGaugeXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MCHBatteryGaugeXOffset = batteryGaugeXOffset;
                        _pluginConfiguration.Save();
                    }

                    var batteryGaugeYOffset = _pluginConfiguration.MCHBatteryGaugeYOffset;
                    if (ImGui.DragInt("Battery Gauge Y Offset", ref batteryGaugeYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MCHBatteryGaugeYOffset = batteryGaugeYOffset;
                        _pluginConfiguration.Save();
                    }

                    var wildfireEnabled = _pluginConfiguration.MCHWildfireEnabled;
                    if (ImGui.Checkbox("Wildfire Bar Enabled", ref wildfireEnabled))
                    {
                        _pluginConfiguration.MCHWildfireEnabled = wildfireEnabled;
                        _pluginConfiguration.Save();
                    }

                    var wildfireHeight = _pluginConfiguration.MCHWildfireHeight;
                    if (ImGui.DragInt("Wildfire Bar Height", ref wildfireHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MCHWildfireHeight = wildfireHeight;
                        _pluginConfiguration.Save();
                    }

                    var wildfireWidth = _pluginConfiguration.MCHWildfireWidth;
                    if (ImGui.DragInt("Wildfire Bar Width", ref wildfireWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MCHWildfireWidth = wildfireWidth;
                        _pluginConfiguration.Save();
                    }

                    var wildfireXOffset = _pluginConfiguration.MCHWildfireXOffset;
                    if (ImGui.DragInt("Wildfire Bar X Offset", ref wildfireXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MCHWildfireXOffset = wildfireXOffset;
                        _pluginConfiguration.Save();
                    }

                    var wildfireYOffset = _pluginConfiguration.MCHWildfireYOffset;
                    if (ImGui.DragInt("Wildfire Bar Y Offset", ref wildfireYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.MCHWildfireYOffset = wildfireYOffset;
                        _pluginConfiguration.Save();
                    }

                    var mchInterBarOffset = _pluginConfiguration.MCHInterBarOffset;
                    if (ImGui.DragInt("Space Between Bars", ref mchInterBarOffset, .1f, 1, 1000))
                    {
                        _pluginConfiguration.MCHInterBarOffset = mchInterBarOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Heat Bar Color", ref _pluginConfiguration.MCHHeatColor);
                    changed |= ImGui.ColorEdit4("Battery Bar Color", ref _pluginConfiguration.MCHBatteryColor);
                    changed |= ImGui.ColorEdit4("Robot Summon Bar Color", ref _pluginConfiguration.MCHRobotColor);
                    changed |= ImGui.ColorEdit4("Overheat Bar Color", ref _pluginConfiguration.MCHOverheatColor);
                    changed |= ImGui.ColorEdit4("Wildfire Bar Color", ref _pluginConfiguration.MCHWildfireColor);
                    changed |= ImGui.ColorEdit4("Bar Not Full Color", ref _pluginConfiguration.MCHEmptyColor);

                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }

        private void DrawJobsCasterConfig()
        {
            if (ImGui.BeginTabBar("##caster-tabs"))
            {
                                if (ImGui.BeginTabItem("Summoner"))
                {

                    var smnRuinBarX = _pluginConfiguration.SmnRuinBarX;
                    if (ImGui.DragInt("Ruin Bar X Offset", ref smnRuinBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SmnRuinBarX = smnRuinBarX;
                        _pluginConfiguration.Save();
                    }

                    var smnRuinBarY = _pluginConfiguration.SmnRuinBarY;
                    if (ImGui.DragInt("Ruin Bar Y Offset", ref smnRuinBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SmnRuinBarY = smnRuinBarY;
                        _pluginConfiguration.Save();
                    }

                    var smnRuinBarHeight = _pluginConfiguration.SmnRuinBarHeight;
                    if (ImGui.DragInt("Ruin Bar Height", ref smnRuinBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SmnRuinBarHeight = smnRuinBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var smnRuinBarWidth = _pluginConfiguration.SmnRuinBarWidth;
                    if (ImGui.DragInt("Ruin Bar Width", ref smnRuinBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SmnRuinBarWidth = smnRuinBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var smnDotBarX = _pluginConfiguration.SmnDotBarX;
                    if (ImGui.DragInt("Dot Bar X Offset", ref smnDotBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SmnDotBarX = smnDotBarX;
                        _pluginConfiguration.Save();
                    }

                    var smnDotBarY = _pluginConfiguration.SmnDotBarY;
                    if (ImGui.DragInt("Dot Bar Y Offset", ref smnDotBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SmnDotBarY = smnDotBarY;
                        _pluginConfiguration.Save();
                    }

                    var smnDotBarHeight = _pluginConfiguration.SmnDotBarHeight;
                    if (ImGui.DragInt("Dot Bar Height", ref smnDotBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SmnDotBarHeight = smnDotBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var smnDotBarWidth = _pluginConfiguration.SmnDotBarWidth;
                    if (ImGui.DragInt("Dot Bar Width", ref smnDotBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SmnDotBarWidth = smnDotBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var smnAetherBarX = _pluginConfiguration.SmnAetherBarX;
                    if (ImGui.DragInt("Aether Bar X Offset", ref smnAetherBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SmnAetherBarX = smnAetherBarX;
                        _pluginConfiguration.Save();
                    }

                    var smnAetherBarY = _pluginConfiguration.SmnAetherBarY;
                    if (ImGui.DragInt("Aether Bar Y Offset", ref smnAetherBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.SmnAetherBarY = smnAetherBarY;
                        _pluginConfiguration.Save();
                    }

                    var smnAetherBarHeight = _pluginConfiguration.SmnAetherBarHeight;
                    if (ImGui.DragInt("Aether Bar Height", ref smnAetherBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SmnAetherBarHeight = smnAetherBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var smnAetherBarWidth = _pluginConfiguration.SmnAetherBarWidth;
                    if (ImGui.DragInt("Aether Bar Width", ref smnAetherBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.SmnAetherBarWidth = smnAetherBarWidth;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Aether Bar Color", ref _pluginConfiguration.SmnAetherColor);
                    changed |= ImGui.ColorEdit4("Ruin Bar Color", ref _pluginConfiguration.SmnRuinColor);
                    changed |= ImGui.ColorEdit4("Empty Bar Color", ref _pluginConfiguration.SmnEmptyColor);
                    changed |= ImGui.ColorEdit4("Miasma Color", ref _pluginConfiguration.SmnMiasmaColor);
                    changed |= ImGui.ColorEdit4("Bio Color", ref _pluginConfiguration.SmnBioColor);
                    changed |= ImGui.ColorEdit4("Expiry Color", ref _pluginConfiguration.SmnExpiryColor);

                    ImGui.EndTabItem();
                }
                                                if (ImGui.BeginTabItem("Red Mage"))
                {
                    var RDMVerticalOffset = _pluginConfiguration.RDMVerticalOffset;
                    if (ImGui.DragInt("Vertical Offset", ref RDMVerticalOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMVerticalOffset = RDMVerticalOffset;
                        _pluginConfiguration.Save();
                    }                    
                    
                    var RDMVHorizontalOffset = _pluginConfiguration.RDMHorizontalOffset;
                    if (ImGui.DragInt("Horizontal Offset", ref RDMVHorizontalOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMHorizontalOffset = RDMVHorizontalOffset;
                        _pluginConfiguration.Save();
                    }

                    var RDMHorizontalSpaceBetweenBars = _pluginConfiguration.RDMHorizontalSpaceBetweenBars;
                    if (ImGui.DragInt("Horizontal Padding", ref RDMHorizontalSpaceBetweenBars, .1f, 1, 1000))
                    {
                        _pluginConfiguration.RDMHorizontalSpaceBetweenBars = RDMHorizontalSpaceBetweenBars;
                        _pluginConfiguration.Save();
                    }

                    var RDMManaBarHeight = _pluginConfiguration.RDMManaBarHeight;
                    if (ImGui.DragInt("Mana Bar Height", ref RDMManaBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.RDMManaBarHeight = RDMManaBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var RDMManaBarWidth = _pluginConfiguration.RDMManaBarWidth;
                    if (ImGui.DragInt("Mana Bar Width", ref RDMManaBarWidth, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMManaBarWidth = RDMManaBarWidth;
                        _pluginConfiguration.Save();
                    }                    
                    
                    var RDMManaBarXOffset = _pluginConfiguration.RDMManaBarXOffset;
                    if (ImGui.DragInt("Mana Bar Horizontal Offset", ref RDMManaBarXOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMManaBarXOffset = RDMManaBarXOffset;
                        _pluginConfiguration.Save();
                    }                    
                    
                    var RDMManaBarYOffset = _pluginConfiguration.RDMManaBarYOffset;
                    if (ImGui.DragInt("Mana Bar Vertical Offset", ref RDMManaBarYOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMManaBarYOffset = RDMManaBarYOffset;
                        _pluginConfiguration.Save();
                    }

                    var RDMWhiteManaBarHeight = _pluginConfiguration.RDMWhiteManaBarHeight;
                    if (ImGui.DragInt("White Mana Height", ref RDMWhiteManaBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.RDMWhiteManaBarHeight = RDMWhiteManaBarHeight;
                        _pluginConfiguration.Save();
                    }
                    var RDMWhiteManaBarWidth = _pluginConfiguration.RDMWhiteManaBarWidth;
                    if (ImGui.DragInt("White Mana Width", ref RDMWhiteManaBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.RDMWhiteManaBarWidth = RDMWhiteManaBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var RDMWhiteManaBarXOffset = _pluginConfiguration.RDMWhiteManaBarXOffset;
                    if (ImGui.DragInt("White Mana Horizontal Offset", ref RDMWhiteManaBarXOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMWhiteManaBarXOffset = RDMWhiteManaBarXOffset;
                        _pluginConfiguration.Save();
                    }

                    var RDMWhiteManaBarYOffset = _pluginConfiguration.RDMWhiteManaBarYOffset;
                    if (ImGui.DragInt("White Mana Vertical Offset", ref RDMWhiteManaBarYOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMWhiteManaBarYOffset = RDMWhiteManaBarYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Invert White Mana Bar", ref _pluginConfiguration.RDMWhiteManaBarInversed);
                    changed |= ImGui.Checkbox("Show White Mana Value", ref _pluginConfiguration.RDMShowWhiteManaValue);

                    var RDMBlackManaBarHeight = _pluginConfiguration.RDMBlackManaBarHeight;
                    if (ImGui.DragInt("Black Mana Height", ref RDMBlackManaBarHeight, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMBlackManaBarHeight = RDMBlackManaBarHeight;
                        _pluginConfiguration.Save();
                    }
                    
                    var RDMBlackManaBarWidth = _pluginConfiguration.RDMBlackManaBarWidth;
                    if (ImGui.DragInt("Black Mana Width", ref RDMBlackManaBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.RDMBlackManaBarWidth = RDMBlackManaBarWidth;
                        _pluginConfiguration.Save();
                    }
                    
                    var RDMBlackManaBarXOffset = _pluginConfiguration.RDMBlackManaBarXOffset;
                    if (ImGui.DragInt("Black Mana Horizontal Offset", ref RDMBlackManaBarXOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMBlackManaBarXOffset = RDMBlackManaBarXOffset;
                        _pluginConfiguration.Save();
                    }                    
                    
                    var RDMBlackManaBarYOffset = _pluginConfiguration.RDMBlackManaBarYOffset;
                    if (ImGui.DragInt("Black Mana Vertical Offset", ref RDMBlackManaBarYOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMBlackManaBarYOffset = RDMBlackManaBarYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Invert Black Mana Bar", ref _pluginConfiguration.RDMBlackManaBarInversed);
                    changed |= ImGui.Checkbox("Show Black Mana Value", ref _pluginConfiguration.RDMShowBlackManaValue);

                    var RDMBalanceBarHeight = _pluginConfiguration.RDMBalanceBarHeight;
                    if (ImGui.DragInt("Balance Height", ref RDMBalanceBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.RDMBalanceBarHeight = RDMBalanceBarHeight;
                        _pluginConfiguration.Save();
                    }                    
                    var RDMBalanceBarWidth = _pluginConfiguration.RDMBalanceBarWidth;
                    if (ImGui.DragInt("Balance Width", ref RDMBalanceBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.RDMBalanceBarWidth = RDMBalanceBarWidth;
                        _pluginConfiguration.Save();
                    }        
                    
                    var RDMBalanceBarXOffset = _pluginConfiguration.RDMBalanceBarXOffset;
                    if (ImGui.DragInt("Balance Horizontal Offset", ref RDMBalanceBarXOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMBalanceBarXOffset = RDMBalanceBarXOffset;
                        _pluginConfiguration.Save();
                    }                    
                    
                    var RDMBalanceBarYOffset = _pluginConfiguration.RDMBalanceBarYOffset;
                    if (ImGui.DragInt("Balance Vertical Offset", ref RDMBalanceBarYOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMBalanceBarYOffset = RDMBalanceBarYOffset;
                        _pluginConfiguration.Save();
                    }
                    
                    var RDMAccelerationBarHeight = _pluginConfiguration.RDMAccelerationBarHeight;
                    if (ImGui.DragInt("Acceleration Stacks Height", ref RDMAccelerationBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.RDMAccelerationBarHeight = RDMAccelerationBarHeight;
                        _pluginConfiguration.Save();
                    }                    
                    var RDMAccelerationBarWidth = _pluginConfiguration.RDMAccelerationBarWidth;
                    if (ImGui.DragInt("Acceleration Stacks Width", ref RDMAccelerationBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.RDMAccelerationBarWidth = RDMAccelerationBarWidth;
                        _pluginConfiguration.Save();
                    }
                    var RDMAccelerationBarXOffset = _pluginConfiguration.RDMAccelerationBarXOffset;
                    if (ImGui.DragInt("Acceleration X Offset", ref RDMAccelerationBarXOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMAccelerationBarXOffset = RDMAccelerationBarXOffset;
                        _pluginConfiguration.Save();
                    }
                    var RDMAccelerationBarYOffset = _pluginConfiguration.RDMAccelerationBarYOffset;
                    if (ImGui.DragInt("Acceleration Y Offset", ref RDMAccelerationBarYOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMAccelerationBarYOffset = RDMAccelerationBarYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Mana Value", ref _pluginConfiguration.RDMShowManaValue);
                    changed |= ImGui.Checkbox("Show Mana Threshold Marker",
                        ref _pluginConfiguration.RDMShowManaThresholdMarker);
                    
                    var RDMManaThresholdValue = _pluginConfiguration.RDMManaThresholdValue;
                    if (ImGui.DragInt("Mana Threshold Marker Value", ref RDMManaThresholdValue, 1f, 1, 10000))
                    {
                        _pluginConfiguration.RDMManaThresholdValue = RDMManaThresholdValue;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Dualcast", ref _pluginConfiguration.RDMShowDualCast);

                    var RDMDualCastHeight = _pluginConfiguration.RDMDualCastHeight;
                    if (ImGui.DragInt("Dualcast Bar Height", ref RDMDualCastHeight, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMDualCastHeight = RDMDualCastHeight;
                        _pluginConfiguration.Save();
                    }                    
                    var RDMDualCastWidth = _pluginConfiguration.RDMDualCastWidth;
                    if (ImGui.DragInt("Dualcast Bar Width", ref RDMDualCastWidth, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMDualCastWidth = RDMDualCastWidth;
                        _pluginConfiguration.Save();
                    }
                    var RDMDualCastXOffset = _pluginConfiguration.RDMDualCastXOffset;
                    if (ImGui.DragInt("Dualcast X Offset", ref RDMDualCastXOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMDualCastXOffset = RDMDualCastXOffset;
                        _pluginConfiguration.Save();
                    }
                    var RDMDualCastYOffset = _pluginConfiguration.RDMDualCastYOffset;
                    if (ImGui.DragInt("Dualcast Y Offset", ref RDMDualCastYOffset, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMDualCastYOffset = RDMDualCastYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Verstone Procs",
                        ref _pluginConfiguration.RDMShowVerstoneProcs);
                    changed |= ImGui.Checkbox("Show Verfire Procs",
                        ref _pluginConfiguration.RDMShowVerfireProcs);

                    var RDMProcsHeight = _pluginConfiguration.RDMProcsHeight;
                    if (ImGui.DragInt("Procs Height", ref RDMProcsHeight, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.RDMProcsHeight = RDMProcsHeight;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Mana Bar Color", ref _pluginConfiguration.RDMManaBarColor);
                    changed |= ImGui.ColorEdit4("Mana Bar Below Threshold Color", ref _pluginConfiguration.RDMManaBarBelowThresholdColor);
                    changed |= ImGui.ColorEdit4("White Mana Bar Color", ref _pluginConfiguration.RDMWhiteManaBarColor);
                    changed |= ImGui.ColorEdit4("Black Mana Bar Color", ref _pluginConfiguration.RDMBlackManaBarColor);
                    changed |= ImGui.ColorEdit4("Balance Color", ref _pluginConfiguration.RDMBalanceBarColor);
                    changed |= ImGui.ColorEdit4("Acceleration Color", ref _pluginConfiguration.RDMAccelerationBarColor);
                    changed |= ImGui.ColorEdit4("Dualcast Color", ref _pluginConfiguration.RDMDualcastBarColor);
                    changed |= ImGui.ColorEdit4("Verstone Ready Proc Color", ref _pluginConfiguration.RDMVerstoneBarColor);
                    changed |= ImGui.ColorEdit4("Verfire Ready Proc Color", ref _pluginConfiguration.RDMVerfireBarColor);

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Black Mage"))
                {
                    var BLMVerticalOffset = _pluginConfiguration.BLMVerticalOffset;
                    if (ImGui.DragInt("Vertical Offset", ref BLMVerticalOffset, 1f, -1000, 1000))
                    {
                        _pluginConfiguration.BLMVerticalOffset = BLMVerticalOffset;
                        _pluginConfiguration.Save();
                    }                    
                    
                    var BLMHorizontalOffset = _pluginConfiguration.BLMHorizontalOffset;
                    if (ImGui.DragInt("Horizontal Offset", ref BLMHorizontalOffset, 1f, -1000, 1000))
                    {
                        _pluginConfiguration.BLMHorizontalOffset = BLMHorizontalOffset;
                        _pluginConfiguration.Save();
                    }

                    var BLMVerticalSpaceBetweenBars = _pluginConfiguration.BLMVerticalSpaceBetweenBars;
                    if (ImGui.DragInt("Vertical Padding", ref BLMVerticalSpaceBetweenBars, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BLMVerticalSpaceBetweenBars = BLMVerticalSpaceBetweenBars;
                        _pluginConfiguration.Save();
                    }

                    var BLMHorizontalSpaceBetweenBars = _pluginConfiguration.BLMHorizontalSpaceBetweenBars;
                    if (ImGui.DragInt("Horizontal Padding", ref BLMHorizontalSpaceBetweenBars, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BLMHorizontalSpaceBetweenBars = BLMHorizontalSpaceBetweenBars;
                        _pluginConfiguration.Save();
                    }

                    var BLMManaBarHeight = _pluginConfiguration.BLMManaBarHeight;
                    if (ImGui.DragInt("Mana Bar Height", ref BLMManaBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BLMManaBarHeight = BLMManaBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var BLMManaBarWidth = _pluginConfiguration.BLMManaBarWidth;
                    if (ImGui.DragInt("Mana Bar Width", ref BLMManaBarWidth, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.BLMManaBarWidth = BLMManaBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var BLMUmbralHeartHeight = _pluginConfiguration.BLMUmbralHeartHeight;
                    if (ImGui.DragInt("Umbral Heart Height", ref BLMUmbralHeartHeight, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.BLMUmbralHeartHeight = BLMUmbralHeartHeight;
                        _pluginConfiguration.Save();
                    }                    
                    
                    var BLMUmbralHeartWidth = _pluginConfiguration.BLMUmbralHeartWidth;
                    if (ImGui.DragInt("Umbral Heart Width", ref BLMUmbralHeartWidth, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.BLMUmbralHeartWidth = BLMUmbralHeartWidth;
                        _pluginConfiguration.Save();
                    }

                    var BLMPolyglotHeight = _pluginConfiguration.BLMPolyglotHeight;
                    if (ImGui.DragInt("Polyglot Height", ref BLMPolyglotHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BLMPolyglotHeight = BLMPolyglotHeight;
                        _pluginConfiguration.Save();
                    }

                    var BLMPolyglotWidth = _pluginConfiguration.BLMPolyglotWidth;
                    if (ImGui.DragInt("Polyglot Width", ref BLMPolyglotWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BLMPolyglotWidth = BLMPolyglotWidth;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Mana Value", ref _pluginConfiguration.BLMShowManaValue);
                    changed |= ImGui.Checkbox("Show Mana Threshold Marker During Astral Fire",
                        ref _pluginConfiguration.BLMShowManaThresholdMarker);

                    var BLMManaThresholdValue = _pluginConfiguration.BLMManaThresholdValue;
                    if (ImGui.DragInt("Mana Threshold Marker Value", ref BLMManaThresholdValue, 1f, 1, 10000))
                    {
                        _pluginConfiguration.BLMManaThresholdValue = BLMManaThresholdValue;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Triplecast", ref _pluginConfiguration.BLMShowTripleCast);

                    var BLMTripleCastHeight = _pluginConfiguration.BLMTripleCastHeight;
                    if (ImGui.DragInt("Triplecast Bar Height", ref BLMTripleCastHeight, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.BLMTripleCastHeight = BLMTripleCastHeight;
                        _pluginConfiguration.Save();
                    }   
                    
                    var BLMTripleCastWidth = _pluginConfiguration.BLMTripleCastWidth;
                    if (ImGui.DragInt("Triplecast Bar Width", ref BLMTripleCastWidth, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.BLMTripleCastWidth = BLMTripleCastWidth;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Firestarter Procs",
                        ref _pluginConfiguration.BLMShowFirestarterProcs);
                    changed |= ImGui.Checkbox("Show Thundercloud Procs",
                        ref _pluginConfiguration.BLMShowThundercloudProcs);

                    var BLMProcsHeight = _pluginConfiguration.BLMProcsHeight;
                    if (ImGui.DragInt("Procs Height", ref BLMProcsHeight, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.BLMProcsHeight = BLMProcsHeight;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show DoT Timer", ref _pluginConfiguration.BLMShowDotTimer);

                    var BLMDotTimerHeight = _pluginConfiguration.BLMDotTimerHeight;
                    if (ImGui.DragInt("DoT Timer Height", ref BLMDotTimerHeight, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.BLMDotTimerHeight = BLMDotTimerHeight;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Mana Bar Color",
                        ref _pluginConfiguration.BLMManaBarNoElementColor);
                    changed |= ImGui.ColorEdit4("Mana Bar Ice Color", ref _pluginConfiguration.BLMManaBarIceColor);
                    changed |= ImGui.ColorEdit4("Mana Bar Fire Color",
                        ref _pluginConfiguration.BLMManaBarFireColor);
                    changed |= ImGui.ColorEdit4("Umbral Heart Color", ref _pluginConfiguration.BLMUmbralHeartColor);
                    changed |= ImGui.ColorEdit4("Polyglot Color", ref _pluginConfiguration.BLMPolyglotColor);
                    changed |= ImGui.ColorEdit4("Triplecast Color", ref _pluginConfiguration.BLMTriplecastColor);
                    changed |= ImGui.ColorEdit4("Firestarter Proc Color",
                        ref _pluginConfiguration.BLMFirestarterColor);
                    changed |= ImGui.ColorEdit4("Thundercloud Proc Color",
                        ref _pluginConfiguration.BLMThundercloudColor);
                    changed |= ImGui.ColorEdit4("DoT Timer Color", ref _pluginConfiguration.BLMDotColor);
                    
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }

    }
}
