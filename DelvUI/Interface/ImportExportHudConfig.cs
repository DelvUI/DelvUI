using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using ImGuiNET;
using System;
using System.Numerics;

namespace DelvUI.Interface
{
    [Portable(false)]
    [Section("Import/Export")]
    [SubSection("General", 0)]
    public class ImportExportHudConfig : PluginConfigObject
    {
        private string _importString = "";
        private string _exportString = "";

        [ManualDraw]
        public void DrawFullImportExport()
        {
            uint maxLength = 40000;
            ImGui.BeginChild("importpane", new Vector2(0, ImGui.GetWindowHeight() / 6), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            {
                ImGui.Text("Import string:");
                ImGui.InputText("", ref _importString, maxLength);

                if (ImGui.Button("Import configuration") && _importString != "")
                {
                    string[] importStrings = _importString.Trim().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    ConfigurationManager.GetInstance().ConfigBaseNode.LoadBase64String(importStrings);
                }

                ImGui.SameLine();

                if (ImGui.Button("Paste from clipboard"))
                {
                    try
                    {
                        _importString = ImGui.GetClipboardText();
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Log("Could not get clipboard text:\n" + ex.StackTrace);
                    }

                }
            }

            ImGui.EndChild();

            ImGui.BeginChild("exportpane", new Vector2(0, ImGui.GetWindowHeight() / 6), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            {
                ImGui.Text("Export string:");
                ImGui.InputText("", ref _exportString, maxLength, ImGuiInputTextFlags.ReadOnly);

                if (ImGui.Button("Export configuration"))
                {
                    _exportString = ConfigurationManager.GetInstance().ConfigBaseNode.GetBase64String();
                }

                ImGui.SameLine();

                if (ImGui.Button("Copy to clipboard") && _exportString != "")
                {
                    try
                    {
                        ImGui.SetClipboardText(_exportString);
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Log("Could not set clipboard text:\n" + ex.StackTrace);
                    }

                }
            }

            ImGui.EndChild();
        }
    }
}
