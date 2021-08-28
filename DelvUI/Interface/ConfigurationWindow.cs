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
            configMap.Add("Unitframes", new [] {"General", "Self", "Target", "Target of Target"});
            configMap.Add("Castbar", new [] {"General", "Enemy Castbar"});
            configMap.Add("Jobs", new [] {"General", "Tank", "Healer", "Melee","Ranged", "Caster"
            });

        }


        public void Draw()
        {
            if (!IsVisible) {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(1400, 900), ImGuiCond.Appearing);

            if (!ImGui.Begin("DelvUI configuration", ref IsVisible, ImGuiWindowFlags.MenuBar)) {
                return;
            }
            var changed = false;

            var viewportWidth = (int) ImGui.GetMainViewport().Size.X;
            var viewportHeight = (int) ImGui.GetMainViewport().Size.Y;
            var xOffsetLimit = viewportWidth / 2;
            var yOffsetLimit = viewportHeight / 2;
            if (ImGui.BeginTabBar("##settings-tabs"))
            {
                ImGui.Button("IMAGE GOES HERE");

                 // Left
                 {
                     ImGui.BeginChild("left pane", new Vector2(150, 0), true);
                     
                     foreach (var config in configMap.Keys)
                     {
                         if (ImGui.Selectable( config, selected == config))
                             selected = config;
                     }
                     ImGui.EndChild();
                 }
                 ImGui.SameLine();

                 // Right
                 {
                     var subConfigs = configMap[selected];

                     ImGui.BeginGroup();
                     ImGui.BeginChild("item view", new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us
                     if (ImGui.BeginTabBar("##Tabs", ImGuiTabBarFlags.None))
                     {
                         foreach (string job in subConfigs)
                         {
                            if (!ImGui.BeginTabItem(job)) continue;
                             ImGui.TextWrapped(job);
                             ImGui.EndTabItem();
                         }

                         ImGui.EndTabBar();
                     }
                     ImGui.EndChild();

                 }
                

            }
            ImGui.EndTabBar();
            if (ImGui.Button("Revert")) {}
            ImGui.SameLine();
            if (ImGui.Button("Save")) {}

            if (changed)
            {
                _pluginConfiguration.BuildColorMap();
                _pluginConfiguration.Save();
            }
            
            ImGui.End();
        }
    }
}
