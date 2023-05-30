using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Memory.Exceptions;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using ImGuiScene;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    [Disableable(false)]
    [Section("Customization")]
    [SubSection("Bar Textures", 0)]
    public class BarTexturesConfig : PluginConfigObject
    {
        public new static BarTexturesConfig DefaultConfig() { return new BarTexturesConfig(); }

        public string BarTexturesPath = "C:\\";

        [JsonIgnore] public string ValidatedBarTexturesPath => ValidatePath(BarTexturesPath);

        [JsonIgnore] private int _inputBarTexture = 0;
        [JsonIgnore] private int _drawModeIndex = 0;
        [JsonIgnore] private Vector4 _color = new Vector4(229 / 255f, 57 / 255f, 57 / 255f, 1);
        [JsonIgnore] private PluginConfigColor _pluginConfigColor = PluginConfigColor.FromHex(0xFFE53939);
        [JsonIgnore] private FileDialogManager _fileDialogManager = new FileDialogManager();
        [JsonIgnore] private bool _applying = false;

        private string ValidatePath(string path)
        {
            if (path.EndsWith("\\") || path.EndsWith("/"))
            {
                return path;
            }

            return path + "\\";
        }

        private void SelectFolder()
        {
            Action<bool, string> callback = (finished, path) =>
            {
                if (finished && path.Length > 0)
                {
                    BarTexturesPath = path;
                    BarTexturesManager.Instance?.ReloadTextures();
                }
            };

            _fileDialogManager.OpenFolderDialog("Select Bar Textures Folder", callback);
        }

        [ManualDraw]
        public bool Draw(ref bool changed)
        {
            if (BarTexturesManager.Instance == null) { return false; }

            string[] textureNames = BarTexturesManager.Instance.BarTextureNames.ToArray();
            string[] drawModes = new string[] { "Stretch", "Repeat Horizontal", "Repeat Vertical", "Repeat" };

            if (ImGui.BeginChild("Bar Textures", new Vector2(800, 400), false, ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGuiHelper.NewLineAndTab();
                ImGui.Text("Custom Bar Textures path");

                ImGuiHelper.Tab();
                if (ImGui.InputText("", ref BarTexturesPath, 200, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    changed = true;
                    BarTexturesManager.Instance?.ReloadTextures();
                }

                ImGui.SameLine();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button(FontAwesomeIcon.Folder.ToIconString(), new Vector2(0, 0)))
                {
                    SelectFolder();
                }
                ImGui.PopFont();

                ImGuiHelper.NewLineAndTab();
                ImGui.Text("Preview");
                ImGuiHelper.Tab();
                ImGui.Combo("Bar Texture ##bar texture", ref _inputBarTexture, textureNames, textureNames.Length, 10);

                ImGuiHelper.Tab();
                ImGui.Combo("Draw Mode", ref _drawModeIndex, drawModes, drawModes.Length, 4);

                ImGuiHelper.Tab();
                if (ImGui.ColorEdit4("Color", ref _color))
                {
                    _pluginConfigColor = new PluginConfigColor(_color);
                }

                if (textureNames.Length > _inputBarTexture)
                {
                    // draw preview
                    ImGui.NewLine();
                    ImGuiHelper.NewLineAndTab();
                    Vector2 pos = ImGui.GetWindowPos() + ImGui.GetCursorPos();
                    Vector2 size = new Vector2(512, 64);
                    ImDrawListPtr drawList = ImGui.GetWindowDrawList();

                    TextureWrap? texture = BarTexturesManager.Instance!.GetBarTexture(textureNames[_inputBarTexture]);
                    DrawHelper.DrawBarTexture(
                        pos,
                        size,
                        _pluginConfigColor,
                        textureNames[_inputBarTexture],
                        (BarTextureDrawMode)_drawModeIndex,
                        drawList
                    );


                    ImGuiHelper.DrawSpacing(3);
                    ImGuiHelper.NewLineAndTab();
                    if (ImGui.Button("Apply to all bars", new Vector2(200, 30)))
                    {
                        _applying = true;
                    }
                }
            }

            ImGui.EndChild();

            _fileDialogManager.Draw();

            if (_applying)
            {
                string[] lines = new string[] { "This will replace the Bar Texture", "and Draw Mode for ALL bars!", "THIS CAN'T BE UNDONE!", "Are you sure?" };
                var (didConfirm, didClose) = ImGuiHelper.DrawConfirmationModal("Apply to ALL bars?", lines);

                if (didConfirm)
                {
                    List<BarConfig> barConfigs = ConfigurationManager.Instance.GetObjects<BarConfig>();
                    foreach (BarConfig barConfig in barConfigs)
                    {
                        barConfig.BarTextureName = textureNames[_inputBarTexture];
                        barConfig.BarTextureDrawMode = (BarTextureDrawMode)_drawModeIndex;
                    }

                    changed = true;
                }

                if (didConfirm || didClose)
                {
                    _applying = false;
                }
            }

            return false;
        }
    }
}
