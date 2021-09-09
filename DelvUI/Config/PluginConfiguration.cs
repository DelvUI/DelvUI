using Dalamud.Configuration;
using Dalamud.Plugin;
using DelvUI.Helpers;
using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using ImGuiScene;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace DelvUI.Config
{
    public class PluginConfiguration : IPluginConfiguration
    {
        [JsonIgnore]
        public TextureWrap BannerImage = null;

        [JsonIgnore]
        public ImFontPtr BigNoodleTooFont = null;

        public int Version { get; set; }

        public void Init()
        {
        }

        public static void WriteConfig(string filename, PluginConfiguration config)
        {
            if (Plugin.GetPluginInterface() == null)
            {
                return;
            }

            var configDirectory = Plugin.GetPluginInterface().GetPluginConfigDirectory();
            var configFile = Path.Combine(configDirectory, filename + ".json");

            try
            {
                var jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFile, jsonString);
            }
            catch (Exception ex)
            {
                PluginLog.Log($"Failed to write configuration {configFile} to JSON");
                PluginLog.Log(ex.StackTrace);
            }
        }

        public static string GenerateExportString(PluginConfiguration config)
        {
            var jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);

            return CompressAndBase64Encode(jsonString);
        }

        public static PluginConfiguration LoadImportString(string importString)
        {
            try
            {
                var jsonString = Base64DecodeAndDecompress(importString);

                return JsonConvert.DeserializeObject<PluginConfiguration>(jsonString);
            }
            catch (Exception ex)
            {
                PluginLog.Log(ex.StackTrace);

                return null;
            }
        }

        public static PluginConfiguration ReadConfig(string filename)
        {
            if (Plugin.GetPluginInterface() == null)
            {
                return null;
            }

            var configDirectory = Plugin.GetPluginInterface().GetPluginConfigDirectory();
            var configFile = Path.Combine(configDirectory, filename + ".json");

            try
            {
                if (File.Exists(configFile))
                {
                    var jsonString = File.ReadAllText(configFile);

                    return JsonConvert.DeserializeObject<PluginConfiguration>(jsonString);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Log($"Failed to load configuration file: {configFile}");
                PluginLog.Log(ex.StackTrace);
            }

            return null;
        }

        public static string CompressAndBase64Encode(string jsonString)
        {
            using MemoryStream output = new();

            using (DeflateStream gzip = new(output, CompressionLevel.Fastest))
            {
                using StreamWriter writer = new(gzip, Encoding.UTF8);
                writer.Write(jsonString);
            }

            return Convert.ToBase64String(output.ToArray());
        }

        public static string Base64DecodeAndDecompress(string base64String)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64String);

            using MemoryStream inputStream = new(base64EncodedBytes);
            using DeflateStream gzip = new(inputStream, CompressionMode.Decompress);
            using StreamReader reader = new(gzip, Encoding.UTF8);
            var decodedString = reader.ReadToEnd();

            return decodedString;
        }

        public void TransferConfig(PluginConfiguration fromOtherConfig)
        {
            // write fields
            foreach (FieldInfo item in typeof(PluginConfiguration).GetFields())
            {
                if (item.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0)
                {
                    continue;
                }

                item.SetValue(this, item.GetValue(fromOtherConfig));
            }

            // write properties
            foreach (PropertyInfo item in typeof(PluginConfiguration).GetProperties())
            {
                if (item.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0)
                {
                    continue;
                }

                item.SetValue(this, item.GetValue(fromOtherConfig));
            }
        }

        public void Save()
        {
            // TODO should not use the name explicitly here
            WriteConfig("DelvUI", this);
        }
    }
}