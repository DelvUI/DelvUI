using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Reflection;
using System.Text;
using Dalamud.Configuration;
using Dalamud.Plugin;
using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using ImGuiScene;
using Newtonsoft.Json;

namespace DelvUI.Config
{
    public class PluginConfiguration : IPluginConfiguration
    {
        public readonly StatusEffectsListConfig PlayerBuffListConfig = new(new Vector2(750, -480), true, false, true, GrowthDirections.Left | GrowthDirections.Down);

        public readonly StatusEffectsListConfig PlayerDebuffListConfig = new(new Vector2(750, -380), false, true, true, GrowthDirections.Left | GrowthDirections.Down);

        public readonly StatusEffectsListConfig TargetBuffListConfig = new(new Vector2(160, 415), true, false, true, GrowthDirections.Right | GrowthDirections.Up);

        public readonly StatusEffectsListConfig TargetDebuffListConfig = new(new Vector2(160, 315), false, true, true, GrowthDirections.Right | GrowthDirections.Up);

        public StatusEffectsListConfig RaidJobBuffListConfig = new(
            new Vector2(0, 300),
            true,
            false,
            false,
            GrowthDirections.Out | GrowthDirections.Right,
            new StatusEffectIconConfig(new Vector2(35, 35), true, true, false, false)
        );

        [JsonIgnore]
        private DalamudPluginInterface _pluginInterface;

        [JsonIgnore]
        public TextureWrap BannerImage = null;

        [JsonIgnore]
        public ImFontPtr BigNoodleTooFont = null;

        public Vector4 CastBarColor = new(255f / 255f, 158f / 255f, 208f / 255f, 100f / 100f);

        [JsonIgnore]
        public Dictionary<string, Dictionary<string, uint>> CastBarColorMap;

        public bool ColorCastBarByDamageType = false;
        public bool ColorCastBarByJob = false;
        public Vector4 CustomHealthBarBackgroundColor = new(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f);
        public bool CustomHealthBarBackgroundColorEnabled = false;

        public Vector4 CustomHealthBarColor = new(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f);
        public bool CustomHealthBarColorEnabled = false;
        public Vector4 EmptyColor = new(0f / 255f, 0f / 255f, 0f / 255f, 50f / 100f);
        public Vector4 PartialFillColor = new(180f / 255f, 180f / 255f, 180f / 255f, 100f / 100f);
        public string FocusBarText = "[name:abbreviate]";
        public bool GCDAlwaysShow = false;
        public Vector4 GCDIndicatorColor = new(255f / 255f, 255f / 255f, 255f / 255f, 70f / 100f);

        public bool GCDIndicatorEnabled = false;
        public bool GCDIndicatorShowBorder = false;
        public bool GCDIndicatorVertical = false;

        public string HealthBarTextLeft = "[name:abbreviate]";
        public string HealthBarTextRight = "[health:max-short] | [health:percent]";
        public bool HideHud = false;
        public Vector4 JobColorAST = new(121f / 255f, 85f / 255f, 72f / 255f, 100f / 100f);

        public Vector4 JobColorBLM = new(126f / 255f, 87f / 255f, 194f / 255f, 100f / 100f);
        public Vector4 JobColorBLU = new(0f / 255f, 185f / 255f, 247f / 255f, 100f / 100f);

        public Vector4 JobColorBRD = new(158f / 255f, 157f / 255f, 36f / 255f, 100f / 100f);
        public Vector4 JobColorDNC = new(244f / 255f, 143f / 255f, 177f / 255f, 100f / 100f);
        public Vector4 JobColorDRG = new(63f / 255f, 81f / 255f, 181f / 255f, 100f / 100f);
        public Vector4 JobColorDRK = new(136f / 255f, 14f / 255f, 79f / 255f, 100f / 100f);
        public Vector4 JobColorGNB = new(78f / 255f, 52f / 255f, 46f / 255f, 100f / 100f);

        [JsonIgnore]
        public Dictionary<uint, Dictionary<string, uint>> JobColorMap;

        public Vector4 JobColorMCH = new(0f / 255f, 151f / 255f, 167f / 255f, 100f / 100f);

        public Vector4 JobColorMNK = new(78f / 255f, 52f / 255f, 46f / 255f, 100f / 100f);
        public Vector4 JobColorNIN = new(211f / 255f, 47f / 255f, 47f / 255f, 100f / 100f);

        public Vector4 JobColorPLD = new(21f / 255f, 28f / 255f, 100f / 255f, 100f / 100f);
        public Vector4 JobColorRDM = new(233f / 255f, 30f / 255f, 99f / 255f, 100f / 100f);
        public Vector4 JobColorSAM = new(255f / 255f, 202f / 255f, 40f / 255f, 100f / 100f);
        public Vector4 JobColorSCH = new(121f / 255f, 134f / 255f, 203f / 255f, 100f / 100f);
        public Vector4 JobColorSMN = new(46f / 255f, 125f / 255f, 50f / 255f, 100f / 100f);
        public Vector4 JobColorWAR = new(153f / 255f, 23f / 255f, 23f / 255f, 100f / 100f);

        public Vector4 JobColorWHM = new(150f / 255f, 150f / 255f, 150f / 255f, 100f / 100f);
        public bool LockHud = true;

        [JsonIgnore]
        public Dictionary<string, Dictionary<string, uint>> MiscColorMap;

        public Vector4 MPTickerColor = new(255f / 255f, 255f / 255f, 255f / 255f, 70f / 100f);

        public bool MPTickerEnabled = false;
        public bool MPTickerHideOnFullMp = false;
        public bool MPTickerShowBorder = false;
        public Vector4 NPCColorFriendly = new(0f / 255f, 145f / 255f, 6f / 255f, 100f / 100f);

        public Vector4 NPCColorHostile = new(205f / 255f, 25f / 255f, 25f / 255f, 100f / 100f);

        [JsonIgnore]
        public Dictionary<string, Dictionary<string, uint>> NPCColorMap;

        public Vector4 NPCColorNeutral = new(214f / 255f, 145f / 255f, 64f / 255f, 100f / 100f);

        public Vector4 ShieldColor = new(255f / 255f, 255f / 255f, 0f / 255f, 100f / 100f);

        public bool ShieldEnabled = true;

        public bool ShieldHeightPixels = true;
        public bool ShowActionIcon = true;
        public bool ShowActionName = true;

        public bool ShowCastBar = true;
        public bool ShowCastTime = true;
        public bool ShowPrimaryResourceBarThresholdMarker = false;
        public bool ShowPrimaryResourceBarValue = false;
        public bool ShowTargetActionIcon = true;
        public bool ShowTargetActionName = true;

        public bool ShowTargetCastBar = true;
        public bool ShowTargetCastTime = true;
        public bool ShowTargetInterrupt = true;
        public bool SlideCast = false;
        public Vector4 SlideCastColor = new(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f);
        public float SlideCastTime = 500;
        public bool TankStanceIndicatorEnabled = true;

        public string TargetBarTextLeft = "[health:max-short] | [health:percent]";
        public string TargetBarTextRight = "[name:abbreviate]";

        public Vector4 TargetCastBarColor = new(255f / 255f, 158f / 255f, 208f / 255f, 100f / 100f);
        public Vector4 TargetCastBarDarknessColor = new(255f / 255f, 0 / 255f, 255f / 255f, 100f / 100f);
        public Vector4 TargetCastBarInterruptColor = new(255f / 255f, 0 / 255f, 255f / 255f, 100f / 100f);
        public Vector4 TargetCastBarMagicalColor = new(0f / 255f, 0 / 255f, 255f / 255f, 100f / 100f);
        public Vector4 TargetCastBarPhysicalColor = new(255f / 255f, 0 / 255f, 0f / 255f, 100f / 100f);

        public string ToTBarText = "[name:abbreviate]";
        public Vector4 UnitFrameEmptyColor = new(0f / 255f, 0f / 255f, 0f / 255f, 95f / 100f);
        public int HealthBarHeight { get; set; } = 50;
        public int HealthBarWidth { get; set; } = 270;
        public int HealthBarXOffset { get; set; } = 160;
        public int HealthBarYOffset { get; set; } = 460;
        public int PrimaryResourceBarHeight { get; set; } = 20;
        public int PrimaryResourceBarWidth { get; set; } = 254;
        public int PrimaryResourceBarXOffset { get; set; } = 160;
        public int PrimaryResourceBarYOffset { get; set; } = 455;
        public int PrimaryResourceBarTextXOffset { get; set; }
        public int PrimaryResourceBarTextYOffset { get; set; }
        public int PrimaryResourceBarThresholdValue { get; set; } = 7000;
        public int TargetBarHeight { get; set; } = 50;
        public int TargetBarWidth { get; set; } = 270;
        public int TargetBarXOffset { get; set; } = 160;
        public int TargetBarYOffset { get; set; } = 460;
        public int ToTBarHeight { get; set; } = 20;
        public int ToTBarWidth { get; set; } = 120;
        public int ToTBarXOffset { get; set; } = 164;
        public int ToTBarYOffset { get; set; } = 460;
        public int FocusBarHeight { get; set; } = 20;
        public int FocusBarWidth { get; set; } = 120;
        public int FocusBarXOffset { get; set; } = 164;
        public int FocusBarYOffset { get; set; } = 460;
        public int ShieldHeight { get; set; } = 10;

        public int TankStanceIndicatorWidth { get; set; } = 2;
        public int HealthBarTextLeftXOffset { get; set; }
        public int HealthBarTextLeftYOffset { get; set; }
        public int HealthBarTextRightXOffset { get; set; }
        public int HealthBarTextRightYOffset { get; set; }
        public int TargetBarTextLeftXOffset { get; set; }
        public int TargetBarTextLeftYOffset { get; set; }
        public int TargetBarTextRightXOffset { get; set; }
        public int TargetBarTextRightYOffset { get; set; }
        public int ToTBarTextXOffset { get; set; }
        public int ToTBarTextYOffset { get; set; }
        public int FocusBarTextXOffset { get; set; }
        public int FocusBarTextYOffset { get; set; }
        public int MPTickerHeight { get; set; } = 4;
        public int MPTickerWidth { get; set; } = 254;
        public int MPTickerXOffset { get; set; }
        public int MPTickerYOffset { get; set; } = 470;
        public int GCDIndicatorHeight { get; set; } = 4;
        public int GCDIndicatorWidth { get; set; } = 254;
        public int GCDIndicatorXOffset { get; set; }
        public int GCDIndicatorYOffset { get; set; } = 480;

        public int CastBarHeight { get; set; } = 25;
        public int CastBarWidth { get; set; } = 254;
        public int CastBarXOffset { get; set; }
        public int CastBarYOffset { get; set; } = 460;

        public int TargetCastBarHeight { get; set; } = 25;
        public int TargetCastBarWidth { get; set; } = 254;
        public int TargetCastBarXOffset { get; set; }
        public int TargetCastBarYOffset { get; set; } = 320;

        public bool ShowRaidWideBuffIcons = true;
        public bool ShowJobSpecificBuffIcons = true;

        public int Version { get; set; }

        public event EventHandler<EventArgs> ConfigChangedEvent;

        public void Init(DalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;
            BuildColorMap();
        }

        public static void WriteConfig(string filename, DalamudPluginInterface pluginInterface, PluginConfiguration config)
        {
            if (pluginInterface == null)
            {
                return;
            }

            var configDirectory = pluginInterface.GetPluginConfigDirectory();
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

        public static PluginConfiguration ReadConfig(string filename, DalamudPluginInterface pluginInterface)
        {
            if (pluginInterface == null)
            {
                return null;
            }

            var configDirectory = pluginInterface.GetPluginConfigDirectory();
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
            WriteConfig("DelvUI", _pluginInterface, this);

            // call event when the config changes
            ConfigChangedEvent?.Invoke(this, null);
        }

        public void BuildColorMap()
        {
            JobColorMap = new Dictionary<uint, Dictionary<string, uint>>
            {
                [Jobs.PLD] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD.AdjustColor(-10f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD.AdjustColor(.1f)),
                        ["invuln"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD.AdjustColor(-.8f))
                    },
                [Jobs.WAR] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR.AdjustColor(-10f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR.AdjustColor(.1f)),
                        ["invuln"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR.AdjustColor(-.8f))
                    },
                [Jobs.DRK] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorDRK),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorDRK.AdjustColor(-10f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorDRK.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorDRK.AdjustColor(.1f)),
                        ["invuln"] = ImGui.ColorConvertFloat4ToU32(JobColorDRK.AdjustColor(-.8f))
                    },
                [Jobs.GNB] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorGNB),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorGNB.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorGNB.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorGNB.AdjustColor(.1f)),
                    ["invuln"] = ImGui.ColorConvertFloat4ToU32(JobColorGNB.AdjustColor(-.8f))
                },
                [Jobs.WHM] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorWHM),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorWHM.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorWHM.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorWHM.AdjustColor(.1f))
                },
                [Jobs.SCH] = new() // Scholar job color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH.AdjustColor(.1f))
                },
                [Jobs.SCH * 1000] = new() // Scholar Aether Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SchAetherColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SchAetherColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SchAetherColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SchAetherColor.AdjustColor(.1f))
                },
                [Jobs.SCH * 1000 + 1] = new() // Scholar Fairy Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SchFairyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SchFairyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SchFairyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SchFairyColor.AdjustColor(.1f))
                },
                [Jobs.SCH * 1000 + 2] = new() // Scholar Empty Bar Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(EmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(.1f))
                },
                [Jobs.SCH * 1000 + 3] = new() // Scholar Biolysis Color
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SCHBioColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SCHBioColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SCHBioColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SCHBioColor.AdjustColor(.1f))
                },
                [Jobs.SMN] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN.AdjustColor(.1f))
                },
                [Jobs.SMN * 1000] = new() // Aether Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SmnAetherColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SmnAetherColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SmnAetherColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SmnAetherColor.AdjustColor(.1f))
                },
                [Jobs.SMN * 1000 + 1] = new() // Ruin Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SmnRuinColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SmnRuinColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SmnRuinColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SmnRuinColor.AdjustColor(.1f))
                },
                [Jobs.SMN * 1000 + 2] = new() // Empty Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(EmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(.1f))
                },
                [Jobs.SMN * 1000 + 3] = new() // Miasma Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SmnMiasmaColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SmnMiasmaColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SmnMiasmaColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SmnMiasmaColor.AdjustColor(.1f))
                },
                [Jobs.SMN * 1000 + 4] = new() // Bio Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SmnBioColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SmnBioColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SmnBioColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SmnBioColor.AdjustColor(.1f))
                },
                [Jobs.SMN * 1000 + 5] = new() // Dot Expiry
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SmnExpiryColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SmnExpiryColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SmnExpiryColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SmnExpiryColor.AdjustColor(.1f))
                },
                [Jobs.AST] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorAST),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorAST.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorAST.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorAST.AdjustColor(.1f))
                },
                [Jobs.MNK] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorMNK),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorMNK.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorMNK.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorMNK.AdjustColor(.1f))
                },
                [Jobs.MNK * 1000] = new() // Demolish
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKDemolishColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MNKDemolishColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MNKDemolishColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MNKDemolishColor.AdjustColor(.1f))
                },
                [Jobs.MNK * 1000 + 1] = new() // Chakra
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKChakraColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MNKChakraColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MNKChakraColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MNKChakraColor.AdjustColor(.1f))
                },
                [Jobs.MNK * 1000 + 2] = new() // Leaden Fist
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKLeadenFistColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MNKLeadenFistColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MNKLeadenFistColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MNKLeadenFistColor.AdjustColor(.1f))
                },
                [Jobs.MNK * 1000 + 3] = new() // Twin Snakes
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKTwinSnakesColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MNKTwinSnakesColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MNKTwinSnakesColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MNKTwinSnakesColor.AdjustColor(.1f))
                },
                [Jobs.MNK * 1000 + 4] = new() // Empty
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(EmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(.1f))
                },
                [Jobs.MNK * 1000 + 5] = new() // Riddle Of Earth
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKRiddleOfEarthColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(.1f))
                },
                [Jobs.MNK * 1000 + 6] = new() // Perfect Balance
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKRiddleOfEarthColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(.1f))
                },
                [Jobs.MNK * 1000 + 7] = new() // True North
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKTrueNorthColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(.1f))
                },
                [Jobs.MNK * 1000 + 8] = new() // Forms
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MNKFormsColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(.1f))
                },
                [Jobs.DRG] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorDRG),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorDRG.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorDRG.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorDRG.AdjustColor(.1f))
                },
                [Jobs.SAM] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorSAM),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorSAM.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorSAM.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorSAM.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 0] = new() // Higanbana Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamHiganbanaColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamHiganbanaColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamHiganbanaColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamHiganbanaColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 1] = new() // Shifu Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamShifuColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamShifuColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamShifuColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamShifuColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 2] = new() // Jinpu Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamJinpuColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamJinpuColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamJinpuColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamJinpuColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 3] = new() // Setsu Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamSetsuColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamSetsuColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamSetsuColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamSetsuColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 4] = new() // Getsu Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamGetsuColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamGetsuColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamGetsuColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamGetsuColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 5] = new() // Ka Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamKaColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamKaColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamKaColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamKaColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 6] = new() // Meditation Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamMeditationColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamMeditationColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamMeditationColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamMeditationColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 7] = new() // Kenki Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamKenkiColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamKenkiColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamKenkiColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamKenkiColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 8] = new() // Empty Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(EmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(.1f))
                },
                [Jobs.SAM * 1000 + 9] = new() // Dot Expiry Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SamExpiryColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SamExpiryColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SamExpiryColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SamExpiryColor.AdjustColor(.1f))
                },
                [Jobs.NIN] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN.AdjustColor(-10f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorNIN.AdjustColor(.1f))
                    },
                [Jobs.BRD] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorBRD),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorBRD.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorBRD.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorBRD.AdjustColor(.1f))
                },
                [Jobs.MCH] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorMCH),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorMCH.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorMCH.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorMCH.AdjustColor(.1f))
                },
                [Jobs.MCH * 1000] = new() // Heat gauge ready
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MCHHeatColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MCHHeatColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MCHHeatColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MCHHeatColor.AdjustColor(.1f))
                },
                [Jobs.MCH * 1000 + 1] = new() // Battery gauge ready
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MCHBatteryColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MCHBatteryColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MCHBatteryColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MCHBatteryColor.AdjustColor(.1f))
                },
                [Jobs.MCH * 1000 + 2] = new() // Robot summoned
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MCHRobotColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MCHRobotColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MCHRobotColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MCHRobotColor.AdjustColor(.1f))
                },
                [Jobs.MCH * 1000 + 3] = new() // Overheated
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MCHOverheatColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MCHOverheatColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MCHOverheatColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MCHOverheatColor.AdjustColor(.1f))
                },
                [Jobs.MCH * 1000 + 4] = new() // Bar not ready
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(EmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(.1f))
                },
                [Jobs.MCH * 1000 + 5] = new() // Wildfire Active
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(MCHWildfireColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(MCHWildfireColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MCHWildfireColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MCHWildfireColor.AdjustColor(.1f))
                },
                [Jobs.DNC] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorDNC),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorDNC.AdjustColor(-10f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorDNC.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorDNC.AdjustColor(.1f))
                    },
                [Jobs.BLM] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorBLM),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorBLM.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorBLM.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorBLM.AdjustColor(.1f))
                },
                [Jobs.RDM] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorRDM),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorRDM.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorRDM.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorRDM.AdjustColor(.1f))
                },
                [Jobs.RDM * 1000] = new() // Mana Bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarColor.AdjustColor(.1f))
                },
                [Jobs.RDM * 1000 + 1] = new() // Mana bar threshold
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarBelowThresholdColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarBelowThresholdColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarBelowThresholdColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMManaBarBelowThresholdColor.AdjustColor(.1f))
                },
                [Jobs.RDM * 1000 + 2] = new() // White mana bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMWhiteManaBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMWhiteManaBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMWhiteManaBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMWhiteManaBarColor.AdjustColor(.1f))
                },
                [Jobs.RDM * 1000 + 3] = new() // Black mana bar
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMBlackManaBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMBlackManaBarColor.AdjustColor(-.5f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMBlackManaBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMBlackManaBarColor.AdjustColor(.1f))
                },
                [Jobs.RDM * 1000 + 4] = new() // Balance
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMBalanceBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMBalanceBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMBalanceBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMBalanceBarColor.AdjustColor(.1f))
                },
                [Jobs.RDM * 1000 + 5] = new() // Acceleration
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMAccelerationBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMAccelerationBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMAccelerationBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMAccelerationBarColor.AdjustColor(.1f))
                },
                [Jobs.RDM * 1000 + 6] = new() // Dualcast
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMDualcastBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMDualcastBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMDualcastBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMDualcastBarColor.AdjustColor(.1f))
                },
                [Jobs.RDM * 1000 + 7] = new() // Verstone
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMVerstoneBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMVerstoneBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMVerstoneBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMVerstoneBarColor.AdjustColor(.1f))
                },
                [Jobs.RDM * 1000 + 8] = new() // Verfire
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(RDMVerfireBarColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(RDMVerfireBarColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(RDMVerfireBarColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(RDMVerfireBarColor.AdjustColor(.1f))
                },
                [Jobs.BLU] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorBLU),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorBLU.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorBLU.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorBLU.AdjustColor(.1f))
                }
            };

            JobColorMap.Add(Jobs.GLD, JobColorMap[Jobs.PLD]);
            JobColorMap.Add(Jobs.PGL, JobColorMap[Jobs.MNK]);
            JobColorMap.Add(Jobs.MRD, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.LNC, JobColorMap[Jobs.DRG]);
            JobColorMap.Add(Jobs.ROG, JobColorMap[Jobs.NIN]);
            JobColorMap.Add(Jobs.ARC, JobColorMap[Jobs.BRD]);
            JobColorMap.Add(Jobs.CNJ, JobColorMap[Jobs.WHM]);
            JobColorMap.Add(Jobs.THM, JobColorMap[Jobs.BLM]);
            JobColorMap.Add(Jobs.ACN, JobColorMap[Jobs.SMN]);

            JobColorMap.Add(Jobs.CRP, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.BSM, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.ARM, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.GSM, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.LTW, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.WVR, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.ALC, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.CUL, JobColorMap[Jobs.WAR]);

            JobColorMap.Add(Jobs.MIN, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.BOT, JobColorMap[Jobs.WAR]);
            JobColorMap.Add(Jobs.FSH, JobColorMap[Jobs.WAR]);

            NPCColorMap = new Dictionary<string, Dictionary<string, uint>>
            {
                ["hostile"] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(NPCColorHostile),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(NPCColorHostile.AdjustColor(-10f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(NPCColorHostile.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(NPCColorHostile.AdjustColor(.1f))
                    },
                ["neutral"] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(NPCColorNeutral),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(NPCColorNeutral.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(NPCColorNeutral.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(NPCColorNeutral.AdjustColor(.1f))
                },
                ["friendly"] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(NPCColorFriendly),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(NPCColorFriendly.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(NPCColorFriendly.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(NPCColorFriendly.AdjustColor(.1f))
                }
            };

            MiscColorMap = new Dictionary<string, Dictionary<string, uint>>
            {
                ["customhealth"] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(CustomHealthBarColor),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(CustomHealthBarColor.AdjustColor(-10f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(CustomHealthBarColor.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(CustomHealthBarColor.AdjustColor(.1f))
                    },
                ["shield"] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(ShieldColor),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(ShieldColor.AdjustColor(-.8f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(ShieldColor.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(ShieldColor.AdjustColor(.1f))
                    },
                ["mpTicker"] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor.AdjustColor(-.8f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(MPTickerColor.AdjustColor(.1f))
                    },
                ["gcd"] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(GCDIndicatorColor),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(GCDIndicatorColor.AdjustColor(-.8f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(GCDIndicatorColor.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(GCDIndicatorColor.AdjustColor(.1f))
                    },
                ["empty"] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(EmptyColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(EmptyColor.AdjustColor(.1f))
                },
                ["partial"] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(PartialFillColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(PartialFillColor),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(PartialFillColor),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(PartialFillColor)
                }
            };

            CastBarColorMap = new Dictionary<string, Dictionary<string, uint>>
            {
                ["castbar"] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(CastBarColor),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(CastBarColor.AdjustColor(-.8f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(CastBarColor.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(CastBarColor.AdjustColor(.1f))
                    },
                ["targetcastbar"] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarColor),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarColor.AdjustColor(-.8f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarColor.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarColor.AdjustColor(.1f))
                    },
                ["targetphysicalcastbar"] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarPhysicalColor),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarPhysicalColor.AdjustColor(-.8f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarPhysicalColor.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarPhysicalColor.AdjustColor(.1f))
                    },
                ["targetmagicalcastbar"] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarMagicalColor),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarMagicalColor.AdjustColor(-.8f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarMagicalColor.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarMagicalColor.AdjustColor(.1f))
                    },
                ["targetdarknesscastbar"] =
                    new()
                    {
                        ["base"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarDarknessColor),
                        ["background"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarDarknessColor.AdjustColor(-.8f)),
                        ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarDarknessColor.AdjustColor(-.1f)),
                        ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarDarknessColor.AdjustColor(.1f))
                    },
                ["targetinterruptcastbar"] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarInterruptColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarInterruptColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarInterruptColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(TargetCastBarInterruptColor.AdjustColor(.1f))
                },
                ["slidecast"] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(SlideCastColor),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(SlideCastColor.AdjustColor(-.8f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(SlideCastColor.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(SlideCastColor.AdjustColor(.1f))
                }
            };
        }

        #region SCH Configuration

        public int SCHBaseXOffset { get; set; }
        public int SCHBaseYOffset { get; set; }

        public int FairyBarHeight { get; set; } = 20;
        public int FairyBarWidth { get; set; } = 254;
        public int FairyBarX { get; set; } = 127;
        public int FairyBarY { get; set; } = 444;
        public int SchAetherBarHeight { get; set; } = 20;
        public int SchAetherBarWidth { get; set; } = 250;
        public int SchAetherBarX { get; set; } = -42;
        public int SchAetherBarY { get; set; } = 444;
        public int SchAetherBarPad { get; set; } = 2;
        public int SCHBioBarHeight { get; set; } = 20;
        public int SCHBioBarWidth { get; set; } = 254;
        public int SCHBioBarX { get; set; } = 127;
        public int SCHBioBarY { get; set; } = 417;

        public bool SCHShowBioBar = true;
        public bool SCHShowAetherBar = true;
        public bool SCHShowFairyBar = true;
        public bool SCHShowPrimaryResourceBar = true;

        public Vector4 SchAetherColor = new(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f);
        public Vector4 SchFairyColor = new(94f / 255f, 250f / 255f, 154f / 255f, 100f / 100f);
        public Vector4 SCHBioColor = new(50f / 255f, 93f / 255f, 37f / 255f, 1f);

        #endregion

        #region SMN Configuration

        public int SmnBaseXOffset { get; set; } = 127;
        public int SmnBaseYOffset { get; set; } = 395;
        public bool SmnMiasmaBarEnabled { get; set; } = true;
        public int SmnMiasmaBarWidth { get; set; } = 126;
        public int SmnMiasmaBarHeight { get; set; } = 20;
        public int SmnMiasmaBarXOffset { get; set; } = 0;
        public int SmnMiasmaBarYOffset { get; set; }
        public bool SmnMiasmaBarFlipped { get; set; }
        public bool SmnBioBarEnabled { get; set; } = true;
        public int SmnBioBarWidth { get; set; } = 126;
        public int SmnBioBarHeight { get; set; } = 20;
        public int SmnBioBarXOffset { get; set; } = 128;
        public int SmnBioBarYOffset { get; set; }
        public bool SmnBioBarFlipped { get; set; } = true;
        public int SmnInterBarOffset { get; set; } = 2;
        public bool SmnRuinBarEnabled { get; set; } = true;
        public int SmnRuinBarXOffset { get; set; }
        public int SmnRuinBarYOffset { get; set; } = 22;
        public int SmnRuinBarHeight { get; set; } = 20;
        public int SmnRuinBarWidth { get; set; } = 254;
        public int SmnRuinBarPadding { get; set; } = 2;
        public bool SmnAetherBarEnabled { get; set; } = true;
        public int SmnAetherBarHeight { get; set; } = 20;
        public int SmnAetherBarWidth { get; set; } = 254;
        public int SmnAetherBarPadding { get; set; } = 2;
        public int SmnAetherBarXOffset { get; set; }
        public int SmnAetherBarYOffset { get; set; } = 44;

        // public int SmnTranceBarHeight { get; set; } = 10;

        public Vector4 SmnAetherColor = new(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f);
        public Vector4 SmnRuinColor = new(94f / 255f, 250f / 255f, 154f / 255f, 100f / 100f);

        public Vector4 SmnMiasmaColor = new(106f / 255f, 237f / 255f, 241f / 255f, 100f / 100f);
        public Vector4 SmnBioColor = new(50f / 255f, 93f / 255f, 37f / 255f, 100f / 100f);
        public Vector4 SmnExpiryColor = new(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f);

        #endregion

        #region SAM Configuration

        public int SAMBaseXOffset { get; set; }
        public int SAMBaseYOffset { get; set; }

        public bool SAMGaugeEnabled { get; set; } = true;
        public bool SAMSenEnabled { get; set; } = true;
        public bool SAMMeditationEnabled { get; set; } = true;
        public bool SAMHiganbanaEnabled { get; set; } = true;
        public bool SAMBuffsEnabled { get; set; } = true;
        public bool ShowBuffTime { get; set; } = true;

        public bool SAMHiganbanaText { get; set; } = true;
        public bool SAMBuffText { get; set; } = true;
        public bool SAMKenkiText { get; set; } = true;
        public int SamHiganbanaBarX { get; set; } = 127;
        public int SamHiganbanaBarY { get; set; } = 370;
        public int SamHiganbanaBarHeight { get; set; } = 20;
        public int SamHiganbanaBarWidth { get; set; } = 254;
        public int SamBuffsBarX { get; set; } = 127;
        public int SamBuffsBarY { get; set; } = 392;
        public int SamBuffsBarHeight { get; set; } = 20;
        public int SamBuffsBarWidth { get; set; } = 254;
        public int SamTimeShifuXOffset { get; set; } = 63;
        public int SamTimeShifuYOffset { get; set; } = 390;
        public int SamTimeJinpuXOffset { get; set; } = -63;
        public int SamTimeJinpuYOffset { get; set; } = 390;
        public int SamKenkiBarX { get; set; } = 127;
        public int SamKenkiBarY { get; set; } = 414;
        public int SamKenkiBarHeight { get; set; } = 20;
        public int SamKenkiBarWidth { get; set; } = 254;
        public int SAMSenPadding { get; set; } = 2;
        public int SamSenBarX { get; set; } = 127;
        public int SamSenBarY { get; set; } = 436;
        public int SamSenBarHeight { get; set; } = 10;
        public int SamSenBarWidth { get; set; } = 254;
        public int SamMeditationBarX { get; set; } = 127;
        public int SamMeditationBarY { get; set; } = 448;
        public int SamMeditationBarHeight { get; set; } = 10;
        public int SamMeditationBarWidth { get; set; } = 254;
        public int SAMMeditationPadding { get; set; } = 2;
        public int SAMBuffsPadding { get; set; } = 2;

        public Vector4 SamHiganbanaColor = new(237f / 255f, 141f / 255f, 7f / 255f, 100f / 100f);
        public Vector4 SamShifuColor = new(219f / 255f, 211f / 255f, 136f / 255f, 100f / 100f);
        public Vector4 SamJinpuColor = new(136f / 255f, 146f / 255f, 219f / 255f, 100f / 100f);

        public Vector4 SamSetsuColor = new(89f / 255f, 234f / 255f, 247f / 255f, 100f / 100f);
        public Vector4 SamGetsuColor = new(89f / 255f, 126f / 255f, 247f / 255f, 100f / 100f);
        public Vector4 SamKaColor = new(247f / 255f, 89f / 255f, 89f / 255f, 100f / 100f);

        public Vector4 SamMeditationColor = new(247f / 255f, 163f / 255f, 89f / 255f, 100f / 100f);
        public Vector4 SamKenkiColor = new(255f / 255f, 82f / 255f, 82f / 255f, 53f / 100f);

        public Vector4 SamExpiryColor = new(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f);

        #endregion

        #region MCH Configuration

        public int MCHBaseXOffset { get; set; }
        public int MCHBaseYOffset { get; set; }

        public bool MCHOverheatEnable { get; set; } = true;
        public bool MCHOverheatText { get; set; } = true;
        public int MCHOverheatHeight { get; set; } = 20;
        public int MCHOverheatWidth { get; set; } = 254;
        public int MCHOverheatXOffset { get; set; } = 127;
        public int MCHOverheatYOffset { get; set; } = 395;
        public bool MCHHeatGaugeEnable { get; set; } = true;
        public bool MCHHeatGaugeText { get; set; } = true;
        public int MCHHeatGaugeHeight { get; set; } = 20;
        public int MCHHeatGaugeWidth { get; set; } = 254;
        public int MCHHeatGaugePadding { get; set; } = 2;
        public int MCHHeatGaugeXOffset { get; set; } = 127;
        public int MCHHeatGaugeYOffset { get; set; } = 417;
        public bool MCHBatteryGaugeEnable { get; set; } = true;
        public bool MCHBatteryGaugeShowBattery { get; set; } = true;
        public bool MCHBatteryGaugeBatteryText { get; set; }
        public bool MCHBatteryGaugeShowRobotDuration { get; set; } = true;
        public bool MCHBatteryGaugeRobotDurationText { get; set; } = true;
        public int MCHBatteryGaugeHeight { get; set; } = 20;
        public int MCHBatteryGaugeWidth { get; set; } = 254;
        public int MCHBatteryGaugePadding { get; set; } = 2;
        public int MCHBatteryGaugeXOffset { get; set; } = 127;
        public int MCHBatteryGaugeYOffset { get; set; } = 439;
        public bool MCHWildfireEnabled { get; set; }
        public bool MCHWildfireText { get; set; } = true;
        public int MCHWildfireHeight { get; set; } = 20;
        public int MCHWildfireWidth { get; set; } = 254;
        public int MCHWildfireXOffset { get; set; } = 127;
        public int MCHWildfireYOffset { get; set; } = 373;
        public Vector4 MCHHeatColor = new(201f / 255f, 13f / 255f, 13f / 255f, 100f / 100f);
        public Vector4 MCHBatteryColor = new(106f / 255f, 255f / 255f, 255f / 255f, 100f / 100f);
        public Vector4 MCHRobotColor = new(153f / 255f, 0f / 255f, 255f / 255f, 100f / 100f);
        public Vector4 MCHOverheatColor = new(255f / 255f, 239f / 255f, 14f / 255f, 100f / 100f);
        public Vector4 MCHWildfireColor = new(255f / 255f, 0f / 255f, 0f / 255f, 100f / 100f);

        #endregion

        #region PLD Configuration

        public bool PLDDoTBarText { get; set; }
        public Vector4 PLDManaColor = new(0f / 255f, 203f / 255f, 230f / 255f, 100f / 100f);
        public Vector4 PLDOathGaugeColor = new(24f / 255f, 80f / 255f, 175f / 255f, 100f / 100f);
        public Vector4 PLDFightOrFlightColor = new(240f / 255f, 50f / 255f, 0f / 255f, 100f / 100f);
        public Vector4 PLDRequiescatColor = new(61f / 255f, 61f / 255f, 255f / 255f, 100f / 100f);
        public Vector4 PLDAtonementColor = new(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f);
        public Vector4 PLDDoTColor = new(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f);

        #endregion

        #region MNK Configuration

        public int MNKBaseXOffset { get; set; } = 160;
        public int MNKBaseYOffset { get; set; } = 460;
        public bool TwinSnakesBarFlipped { get; set; } = true;
        public bool RiddleOfEarthBarFlipped { get; set; } = true;
        public bool PerfectBalanceBarFlipped { get; set; } = true;
        public bool DemolishEnabled { get; set; } = true;
        public bool ChakraEnabled { get; set; } = true;
        public bool LeadenFistEnabled { get; set; } = true;
        public bool TwinSnakesEnabled { get; set; } = true;
        public bool RiddleOfEarthEnabled { get; set; } = true;
        public bool PerfectBalanceEnabled { get; set; } = true;
        public bool TrueNorthEnabled { get; set; } = true;
        public bool FormsEnabled { get; set; }
        public int MNKDemolishHeight { get; set; } = 20;
        public int MNKDemolishWidth { get; set; } = 111;
        public int MNKDemolishXOffset { get; set; }
        public int MNKDemolishYOffset { get; set; }
        public int MNKChakraHeight { get; set; } = 20;
        public int MNKChakraWidth { get; set; } = 254;
        public int MNKChakraXOffset { get; set; }
        public int MNKChakraYOffset { get; set; }
        public int MNKLeadenFistHeight { get; set; } = 20;
        public int MNKLeadenFistWidth { get; set; } = 28;
        public int MNKLeadenFistXOffset { get; set; }
        public int MNKLeadenFistYOffset { get; set; }
        public int MNKTwinSnakesHeight { get; set; } = 20;
        public int MNKTwinSnakesWidth { get; set; } = 111;
        public int MNKTwinSnakesXOffset { get; set; }
        public int MNKTwinSnakesYOffset { get; set; }
        public int MNKRiddleOfEarthHeight { get; set; } = 20;
        public int MNKRiddleOfEarthWidth { get; set; } = 115;
        public int MNKRiddleOfEarthXOffset { get; set; }
        public int MNKRiddleOfEarthYOffset { get; set; }
        public int MNKPerfectBalanceHeight { get; set; } = 20;
        public int MNKPerfectBalanceWidth { get; set; } = 20;
        public int MNKPerfectBalanceXOffset { get; set; }
        public int MNKPerfectBalanceYOffset { get; set; }
        public int MNKTrueNorthHeight { get; set; } = 20;
        public int MNKTrueNorthWidth { get; set; } = 115;
        public int MNKTrueNorthXOffset { get; set; }
        public int MNKTrueNorthYOffset { get; set; }
        public int MNKFormsHeight { get; set; } = 20;
        public int MNKFormsWidth { get; set; } = 254;
        public int MNKFormsXOffset { get; set; }
        public int MNKFormsYOffset { get; set; }

        public Vector4 MNKDemolishColor = new(246f / 255f, 169f / 255f, 255f / 255f, 100f);
        public Vector4 MNKChakraColor = new(204f / 255f, 115f / 255f, 0f, 100f);
        public Vector4 MNKLeadenFistColor = new(255f / 255f, 0f, 0f, 100f);
        public Vector4 MNKTwinSnakesColor = new(227f / 255f, 255f, 64f / 255f, 100f);
        public Vector4 MNKRiddleOfEarthColor = new(157f / 255f, 59f / 255f, 255f, 100f);
        public Vector4 MNKPerfectBalanceColor = new(150f / 255f, 255f, 255f, 100f);
        public Vector4 MNKTrueNorthColor = new(255f, 225f / 255f, 189f / 255f, 100f);
        public Vector4 MNKFormsColor = new(36f / 255f, 131f / 255f, 255f, 100f);

        #endregion

        #region RDM Configuration

        public int RDMVerticalOffset { get; set; } = -2;
        public int RDMHorizontalOffset { get; set; }
        public int RDMHorizontalSpaceBetweenBars { get; set; } = 2;
        public int RDMManaBarHeight { get; set; } = 18;
        public int RDMManaBarWidth { get; set; } = 253;
        public int RDMManaBarXOffset { get; set; }
        public int RDMManaBarYOffset { get; set; }
        public int RDMWhiteManaBarHeight { get; set; } = 20;
        public int RDMWhiteManaBarWidth { get; set; } = 114;
        public int RDMWhiteManaBarXOffset { get; set; } = -13;
        public int RDMWhiteManaBarYOffset { get; set; } = -40;
        public bool RDMWhiteManaBarInversed = true;
        public bool RDMShowWhiteManaValue = true;
        public int RDMBlackManaBarHeight { get; set; } = 20;
        public int RDMBlackManaBarWidth { get; set; } = 114;
        public int RDMBlackManaBarXOffset { get; set; } = 13;
        public int RDMBlackManaBarYOffset { get; set; } = -40;
        public bool RDMBlackManaBarInversed = false;
        public bool RDMShowBlackManaValue = true;
        public int RDMBalanceBarHeight { get; set; } = 20;
        public int RDMBalanceBarWidth { get; set; } = 21;
        public int RDMBalanceBarXOffset { get; set; }
        public int RDMBalanceBarYOffset { get; set; } = -40;
        public int RDMAccelerationBarHeight { get; set; } = 12;
        public int RDMAccelerationBarWidth { get; set; } = 83;
        public int RDMAccelerationBarXOffset { get; set; }
        public int RDMAccelerationBarYOffset { get; set; } = -54;
        public bool RDMShowManaValue = true;
        public bool RDMShowManaThresholdMarker = true;
        public int RDMManaThresholdValue { get; set; } = 2600;
        public bool RDMShowDualCast = true;
        public int RDMDualCastHeight { get; set; } = 16;
        public int RDMDualCastWidth { get; set; } = 16;
        public int RDMDualCastXOffset { get; set; }
        public int RDMDualCastYOffset { get; set; } = -72;
        public bool RDMShowVerstoneProcs = true;
        public bool RDMShowVerfireProcs = true;
        public int RDMProcsHeight { get; set; } = 7;

        public Vector4 RDMManaBarColor = new(0f / 255f, 142f / 255f, 254f / 255f, 100f / 100f);
        public Vector4 RDMManaBarBelowThresholdColor = new(210f / 255f, 33f / 255f, 33f / 255f, 100f / 100f);
        public Vector4 RDMWhiteManaBarColor = new(221f / 255f, 212f / 255f, 212f / 255f, 100f / 100f);
        public Vector4 RDMBlackManaBarColor = new(60f / 255f, 81f / 255f, 197f / 255f, 100f / 100f);
        public Vector4 RDMBalanceBarColor = new(195f / 255f, 35f / 255f, 35f / 255f, 100f / 100f);
        public Vector4 RDMAccelerationBarColor = new(194f / 255f, 74f / 255f, 74f / 255f, 100f / 100f);
        public Vector4 RDMDualcastBarColor = new(204f / 255f, 17f / 255f, 255f / 95f, 100f / 100f);
        public Vector4 RDMVerstoneBarColor = new(228f / 255f, 188f / 255f, 145 / 255f, 90f / 100f);
        public Vector4 RDMVerfireBarColor = new(238f / 255f, 119f / 255f, 17 / 255f, 90f / 100f);

        #endregion
    }

    public static class Jobs
    {
        public const uint GLD = 1;
        public const uint MRD = 3;
        public const uint PLD = 19;
        public const uint WAR = 21;
        public const uint DRK = 32;
        public const uint GNB = 37;

        public const uint CNJ = 6;
        public const uint WHM = 24;
        public const uint SCH = 28;
        public const uint AST = 33;

        public const uint PGL = 2;
        public const uint LNC = 4;
        public const uint ROG = 29;
        public const uint MNK = 20;
        public const uint DRG = 22;
        public const uint NIN = 30;
        public const uint SAM = 34;

        public const uint ARC = 5;
        public const uint BRD = 23;
        public const uint MCH = 31;
        public const uint DNC = 38;

        public const uint THM = 7;
        public const uint ACN = 26;
        public const uint BLM = 25;
        public const uint SMN = 27;
        public const uint RDM = 35;
        public const uint BLU = 36;

        public const uint CRP = 8;
        public const uint BSM = 9;
        public const uint ARM = 10;
        public const uint GSM = 11;
        public const uint LTW = 12;
        public const uint WVR = 13;
        public const uint ALC = 14;
        public const uint CUL = 15;

        public const uint MIN = 16;
        public const uint BOT = 17;
        public const uint FSH = 18;
    }
}
