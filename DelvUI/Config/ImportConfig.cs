using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Config.Profiles;
using DelvUI.Helpers;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DelvUI.Interface
{
    [Disableable(false)]
    [Exportable(false)]
    [Shareable(false)]
    [Resettable(false)]
    [Section("Import")]
    [SubSection("General", 0)]
    public class ImportConfig : PluginConfigObject
    {
        private string _importString = "";
        private bool _importing = false;
        private string? _errorMessage = null;

        private List<ImportData>? _importDataList = null;
        private List<bool>? _importDataEnabled = null;

        public new static ImportConfig DefaultConfig() { return new ImportConfig(); }

        [ManualDraw]
        public bool Draw(ref bool changed)
        {
            ImGui.Text("Import string:");

            ImGui.InputText("", ref _importString, 999999);

            ImGui.Text("Here you can import specific parts of a profile.\nIf the string contains more than one part you will be able to select which parts you wish to import.");

            if (ImGui.Button("Import", new Vector2(560, 24)))
            {
                _importing = _importString.Length > 0;
            }

            ImGuiHelper.DrawSeparator(1, 1);
            ImGui.Text("To browse presets made by users of the DelvUI community, click the button below.");

            if (ImGui.Button("DelvUI on wago.io", new Vector2(560, 24)))
            {
                Utils.OpenUrl("https://wago.io/delvui");
            }

            // error modal
            if (_errorMessage != null)
            {
                if (ImGuiHelper.DrawErrorModal(_errorMessage))
                {
                    _importing = false;
                    _errorMessage = null;
                }

                return false;
            }

            // parse import string
            if (_importing && _importDataList == null)
            {
                _errorMessage = Parse();
            }

            // confirmation modal
            if (_importDataList != null && _importDataList.Count > 0)
            {
                var (didConfirm, didClose) = DrawImportConfirmationModal();

                if (didConfirm)
                {
                    _errorMessage = Import();

                    if (_errorMessage == null)
                    {
                        _importString = "";
                    }
                }

                if (didConfirm || didClose)
                {
                    _importing = false;
                    _importDataList = null;
                    _importDataEnabled = null;
                    changed = true;
                }

                return didConfirm && _errorMessage == null;
            }

            return false;
        }

        private string? Import()
        {
            if (_importDataList == null || _importDataEnabled == null)
            {
                return null;
            }

            List<PluginConfigObject> configObjects = new List<PluginConfigObject>(_importDataList.Count);

            for (int i = 0; i < _importDataList.Count; i++)
            {
                if (i >= _importDataEnabled.Count || _importDataEnabled[i] == false)
                {
                    continue;
                }

                ImportData importData = _importDataList[i];
                PluginConfigObject? config = importData.GetObject();
                if (config == null)
                {
                    return "Couldn't import \"" + importData.Name + "\"";
                }

                configObjects.Add(config);
            }

            foreach (PluginConfigObject config in configObjects)
            {
                ConfigurationManager.Instance.SetConfigObject(config);
            }

            return null;
        }

        private string? Parse()
        {
            string[] importStrings = _importString.Trim().Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            if (importStrings.Length == 0)
            {
                return null;
            }

            _importDataList = new List<ImportData>(importStrings.Length);
            _importDataEnabled = new List<bool>(importStrings.Length);

            foreach (var str in importStrings)
            {
                try
                {
                    ImportData importData = new ImportData(str);
                    _importDataList.Add(importData);
                    _importDataEnabled.Add(true);
                }
                catch (Exception e)
                {
                    _importDataList = null;
                    _importDataEnabled = null;

                    return e is ArgumentException ? e.Message : "Invalid import string!";
                }
            }

            return null;
        }

        public (bool, bool) DrawImportConfirmationModal()
        {
            if (_importDataList == null || _importDataEnabled == null)
            {
                return (false, true);
            }

            ConfigurationManager.Instance.ShowingModalWindow = true;

            bool didConfirm = false;
            bool didClose = false;

            ImGui.OpenPopup("Import ##DelvUI");

            Vector2 center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

            bool p_open = true; // i've no idea what this is used for

            if (ImGui.BeginPopupModal("Import ##DelvUI", ref p_open, ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
            {
                float width = 300;

                ImGui.Text("Select which parts to import:");

                ImGui.NewLine();
                if (ImGui.Button("Select All", new Vector2(width / 2f - 5, 24)))
                {
                    for (int i = 0; i < _importDataEnabled.Count; i++)
                    {
                        _importDataEnabled[i] = true;
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button("Deselect All", new Vector2(width / 2f - 5, 24)))
                {
                    for (int i = 0; i < _importDataEnabled.Count; i++)
                    {
                        _importDataEnabled[i] = false;
                    }
                }

                ImGui.NewLine();
                float height = Math.Min(30 * _importDataList.Count, 400);

                ImGui.BeginChild("import checkboxes", new Vector2(width, height), false);

                for (int i = 0; i < _importDataList.Count; i++)
                {
                    bool value = _importDataEnabled[i];
                    if (ImGui.Checkbox(_importDataList[i].Name, ref value))
                    {
                        _importDataEnabled[i] = value;
                    }
                }

                ImGui.EndChild();

                ImGui.NewLine();
                if (ImGui.Button("OK", new Vector2(width / 2f - 5, 24)))
                {
                    ImGui.CloseCurrentPopup();
                    didConfirm = true;
                    didClose = true;
                }

                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Cancel", new Vector2(width / 2f - 5, 24)))
                {
                    ImGui.CloseCurrentPopup();
                    didClose = true;
                }

                ImGui.EndPopup();
            }
            // close button on nav
            else
            {
                didClose = true;
            }

            if (didClose)
            {
                ConfigurationManager.Instance.ShowingModalWindow = false;
            }

            return (didConfirm, didClose);
        }
    }

    public struct ImportData
    {
        public readonly Type ConfigType;
        public readonly string Name;

        public readonly string ImportString;
        public readonly string JsonString;

        public ImportData(string base64String)
        {
            ImportString = base64String;
            JsonString = ImportExportHelper.Base64DecodeAndDecompress(base64String);

            var typeString = (string?)JObject.Parse(JsonString)["$type"];
            if (typeString == null)
            {
                throw new ArgumentException("Invalid type");
            }

            Type? type = Type.GetType(typeString);
            if (type == null)
            {
                throw new ArgumentException("Invalid type: \"" + typeString + "\"");
            }

            ConfigType = type;
            Name = Utils.UserFriendlyConfigName(type.Name);
        }

        public PluginConfigObject? GetObject()
        {
            MethodInfo? methodInfo = typeof(PluginConfigObject).GetMethod("LoadFromJsonString", BindingFlags.Public | BindingFlags.Static);
            MethodInfo? function = methodInfo?.MakeGenericMethod(ConfigType);
            return (PluginConfigObject?)function?.Invoke(this, new object[] { JsonString })!;
        }
    }
}
