using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
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
        




        public ConfigurationWindow(Plugin plugin, DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration)
        {
            _plugin = plugin;
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginConfiguration;
            configMap.Add("General", new [] {"General","Color Map"});
            configMap.Add("Individual Unitframes", new [] {"General", "Player", "Target", "Target of Target", "Focus"});
            configMap.Add("Group Unitframes", new [] {"General", "Party", "8man", "24man", "Enemies"});
            configMap.Add("Castbars", new [] {"General", "Player Castbar", "Enemy Castbar"});
            configMap.Add("Jobs", new [] {"General", "Tank", "Healer", "Melee","Ranged", "Caster"});

        }


        public void Draw()
        {
            if (!IsVisible) {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(900, 600), ImGuiCond.Appearing);

            if (!ImGui.Begin("DelvUI configuration", ref IsVisible, ImGuiWindowFlags.MenuBar)) {
                return;
            }
            var changed = false;



                ImGui.BeginGroup();
                {
                    ImGui.BeginGroup(); // Left
                    {
                        ImGui.Button("IMAGE GOES HERE");
                        ImGui.Button("IMAGE GOES HERE");

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


                        ImGui.BeginChild("item view",
                            new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us
                        if (ImGui.BeginTabBar("##Tabs", ImGuiTabBarFlags.None))
                        {
                            foreach (string sunConfig in subConfigs)
                            {

                                if (!ImGui.BeginTabItem(sunConfig)) continue;
                                ImGui.BeginChild("subconfig value", new Vector2(0, 0), true);
                                ImGui.TextWrapped(sunConfig);
                                ImGui.EndChild();
                                ImGui.EndTabItem();
                            }

                            ImGui.EndTabBar();

                        }

                        ImGui.EndChild();

                    }
                    ImGui.EndGroup();
                }
                ImGui.EndGroup();
                
                ImGui.BeginGroup();

                if (ImGui.Button("Lock HUD")) {}
                ImGui.SameLine();
                if (ImGui.Button("Hide HUD")) {}                
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
    }
}
