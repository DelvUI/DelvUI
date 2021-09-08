using Dalamud.Configuration;
using Dalamud.Plugin;
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
            new StatusEffectIconConfig(new Vector2(35, 35), true, true, false, false),
            new Vector2(1000, 0)
        );

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
        public bool ShowTestCastBar = false;
        public bool ShowTargetActionIcon = true;
        public bool ShowTargetActionName = true;

        public bool ShowTargetCastBar = true;
        public bool ShowTargetCastTime = true;
        public bool ShowTargetTestCastBar = false;
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

        public void Init()
        {
            BuildColorMap();
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

            // call event when the config changes
            ConfigChangedEvent?.Invoke(this, null);
        }

        public void BuildColorMap()
        {
            JobColorMap = new Dictionary<uint, Dictionary<string, uint>>
            {
                [Jobs.PLD] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD.AdjustColor(.1f)),
                    ["invuln"] = ImGui.ColorConvertFloat4ToU32(JobColorPLD.AdjustColor(-.8f))
                },
                [Jobs.WAR] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR.AdjustColor(.1f)),
                    ["invuln"] = ImGui.ColorConvertFloat4ToU32(JobColorWAR.AdjustColor(-.8f))
                },
                [Jobs.DRK] = new()
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
                [Jobs.SCH] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorSCH.AdjustColor(.1f))
                },
                [Jobs.SMN] = new()
                {
                    ["base"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN),
                    ["background"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN.AdjustColor(-10f)),
                    ["gradientLeft"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN.AdjustColor(-.1f)),
                    ["gradientRight"] = ImGui.ColorConvertFloat4ToU32(JobColorSMN.AdjustColor(.1f))
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
                [Jobs.NIN] = new()
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
                [Jobs.DNC] = new()
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

        #region PLD Configuration

        public bool PLDDoTBarText { get; set; }
        public Vector4 PLDManaColor = new(0f / 255f, 203f / 255f, 230f / 255f, 100f / 100f);
        public Vector4 PLDOathGaugeColor = new(24f / 255f, 80f / 255f, 175f / 255f, 100f / 100f);
        public Vector4 PLDFightOrFlightColor = new(240f / 255f, 50f / 255f, 0f / 255f, 100f / 100f);
        public Vector4 PLDRequiescatColor = new(61f / 255f, 61f / 255f, 255f / 255f, 100f / 100f);
        public Vector4 PLDAtonementColor = new(240f / 255f, 176f / 255f, 0f / 255f, 100f / 100f);
        public Vector4 PLDDoTColor = new(255f / 255f, 128f / 255f, 0f / 255f, 100f / 100f);

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
