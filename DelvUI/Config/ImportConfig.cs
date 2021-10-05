using DelvUI.Config;
using DelvUI.Config.Attributes;
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
    [Section("Import")]
    [SubSection("General", 0)]
    public class ImportConfig : PluginConfigObject
    {
        private string _importString = "";
        private bool _importing = false;
        private string? _errorMessage = null;

        private List<ImportData>? _importDataList = null;
        private List<string>? _importMessages = null;

        public new static ImportConfig DefaultConfig() { return new ImportConfig(); }

        [ManualDraw]
        public bool Draw(ref bool changed)
        {
            ImGui.Text("Import string:");

            ImGui.InputText("", ref _importString, 999999);

            ImGui.NewLine();
            if (ImGui.Button("Import", new Vector2(560, 24)))
            {
                _importing = _importString.Length > 0;
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
            if (_importDataList != null && _importDataList.Count > 0 && _importMessages != null)
            {
                var (didConfirm, didClose) = ImGuiHelper.DrawConfirmationModal("Import", _importMessages);

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
                    _importMessages = null;
                    changed = true;
                }

                return didConfirm && _errorMessage == null;
            }

            return false;
        }

        private string? Import()
        {
            if (_importDataList == null)
            {
                return null;
            }

            List<PluginConfigObject> configObjects = new List<PluginConfigObject>(_importDataList.Count);

            foreach (ImportData importData in _importDataList)
            {
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

            const int maxLines = 6;
            _importMessages = new List<string>(Math.Max(importStrings.Length + 1, maxLines));
            _importMessages.Add("Are you sure you want to import?");

            foreach (var str in importStrings)
            {
                try
                {
                    ImportData importData = new ImportData(str);
                    _importDataList.Add(importData);
                    _importMessages.Add("  -" + importData.Name);
                }
                catch (Exception e)
                {
                    _importDataList = null;
                    _importMessages = null;

                    return e is ArgumentException ? e.Message : "Invalid import string!";
                }
            }

            if (_importMessages.Count == maxLines && importStrings.Length > maxLines)
            {
                _importMessages[maxLines - 1] = "  -...";
            }

            return null;
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
            JsonString = ConfigurationManager.Base64DecodeAndDecompress(base64String);

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
            MethodInfo? methodInfo = GetType().GetMethod("DeserializeObject", BindingFlags.Public | BindingFlags.Static);
            MethodInfo? function = methodInfo?.MakeGenericMethod(ConfigType);
            return (PluginConfigObject?)function?.Invoke(this, new object[] { JsonString })!;
        }

        public static T? DeserializeObject<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
