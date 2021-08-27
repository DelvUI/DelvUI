using System.Numerics;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface {
    public class ConfigurationWindow {
        public bool IsVisible;
        private readonly Plugin _plugin;
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly PluginConfiguration _pluginConfiguration;

        public ConfigurationWindow(Plugin plugin, DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) {
            _plugin = plugin;
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginConfiguration;
        }

        public void Draw() {
            if (!IsVisible) {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(0, 0), ImGuiCond.Always);

            if (!ImGui.Begin("DelvUI configuration", ref IsVisible, ImGuiWindowFlags.NoCollapse)) {
                return;
            }

            var changed = false;

            var viewportWidth = (int) ImGui.GetMainViewport().Size.X;
            var viewportHeight = (int) ImGui.GetMainViewport().Size.Y;
            var xOffsetLimit = viewportWidth / 2;
            var yOffsetLimit = viewportHeight / 2;

            if (ImGui.BeginTabBar("##settings-tabs")) {
                if (ImGui.BeginTabItem("General"))
                {
                    var healthBarHeight = _pluginConfiguration.HealthBarHeight;
                    if (ImGui.DragInt("Health Height", ref healthBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.HealthBarHeight = healthBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var healthBarWidth = _pluginConfiguration.HealthBarWidth;
                    if (ImGui.DragInt("Health Width", ref healthBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.HealthBarWidth = healthBarWidth;
                        _pluginConfiguration.Save();
                    }

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

                    var targetBarHeight = _pluginConfiguration.TargetBarHeight;
                    if (ImGui.DragInt("Target Height", ref targetBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.TargetBarHeight = targetBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var targetBarWidth = _pluginConfiguration.TargetBarWidth;
                    if (ImGui.DragInt("Target Width", ref targetBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.TargetBarWidth = targetBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var totBarHeight = _pluginConfiguration.ToTBarHeight;
                    if (ImGui.DragInt("Target of Target Height", ref totBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ToTBarHeight = totBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var totBarWidth = _pluginConfiguration.ToTBarWidth;
                    if (ImGui.DragInt("Target of Target Width", ref totBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ToTBarWidth = totBarWidth;
                        _pluginConfiguration.Save();
                    }
                    var focusBarHeight = _pluginConfiguration.FocusBarHeight;
                    if (ImGui.DragInt("Focus Height", ref focusBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.FocusBarHeight = focusBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var focusBarWidth = _pluginConfiguration.FocusBarWidth;
                    if (ImGui.DragInt("Focus Width", ref focusBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.FocusBarWidth = focusBarWidth;
                        _pluginConfiguration.Save();
                    }
                    
                    changed |= ImGui.Checkbox("Shield Enabled", ref _pluginConfiguration.ShieldEnabled);
                    
                    var shieldHeight = _pluginConfiguration.ShieldHeight;
                    if (ImGui.DragInt("Shield Height", ref shieldHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.ShieldHeight = shieldHeight;
                        _pluginConfiguration.Save();
                    }
                    
                    changed |= ImGui.Checkbox("Shield Height in px", ref _pluginConfiguration.ShieldHeightPixels);
                    
                    changed |= ImGui.ColorEdit4("Shield Color", ref _pluginConfiguration.ShieldColor);

                    changed |= ImGui.Checkbox("Hide HUD", ref _pluginConfiguration.HideHud);

                    changed |= ImGui.Checkbox("Lock HUD", ref _pluginConfiguration.LockHud);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Castbar"))
                {
                    changed |= ImGui.Checkbox("Show Cast Bar", ref _pluginConfiguration.ShowCastBar);

                    var castBarHeight = _pluginConfiguration.CastBarHeight;
                    if (ImGui.DragInt("Castbar Height", ref castBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.CastBarHeight = castBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var castBarWidth = _pluginConfiguration.CastBarWidth;
                    if (ImGui.DragInt("Castbar Width", ref castBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.CastBarWidth = castBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var castBarXOffset = _pluginConfiguration.CastBarXOffset;
                    if (ImGui.DragInt("Castbar X Offset", ref castBarXOffset, .1f, -xOffsetLimit, xOffsetLimit))
                    {
                        _pluginConfiguration.CastBarXOffset = castBarXOffset;
                        _pluginConfiguration.Save();
                    }

                    var castBarYOffset = _pluginConfiguration.CastBarYOffset;
                    if (ImGui.DragInt("Castbar Y Offset", ref castBarYOffset, .1f, -yOffsetLimit, yOffsetLimit))
                    {
                        _pluginConfiguration.CastBarYOffset = castBarYOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Castbar Color", ref _pluginConfiguration.CastBarColor);

                    changed |= ImGui.Checkbox("Show Action Icon", ref _pluginConfiguration.ShowActionIcon);
                    changed |= ImGui.Checkbox("Show Action Name", ref _pluginConfiguration.ShowActionName);
                    changed |= ImGui.Checkbox("Show Cast Time", ref _pluginConfiguration.ShowCastTime);
                    changed |= ImGui.Checkbox("Slide Cast Enabled", ref _pluginConfiguration.SlideCast);

                    var slideCastTime = _pluginConfiguration.SlideCastTime;
                    if (ImGui.DragFloat("Slide Cast Offset", ref slideCastTime, 1, 1, 1000))
                    {
                        _pluginConfiguration.SlideCastTime = slideCastTime;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Slide Cast Color", ref _pluginConfiguration.SlideCastColor);

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Color Map")) {
                    changed |= ImGui.ColorEdit4("Job Color PLD", ref _pluginConfiguration.JobColorPLD);
                    changed |= ImGui.ColorEdit4("Job Color WAR", ref _pluginConfiguration.JobColorWAR);
                    changed |= ImGui.ColorEdit4("Job Color DRK", ref _pluginConfiguration.JobColorDRK);
                    changed |= ImGui.ColorEdit4("Job Color GNB", ref _pluginConfiguration.JobColorGNB);
                    changed |= ImGui.ColorEdit4("Job Color WHM", ref _pluginConfiguration.JobColorWHM);
                    changed |= ImGui.ColorEdit4("Job Color AST", ref _pluginConfiguration.JobColorAST);
                    changed |= ImGui.ColorEdit4("Job Color SCH", ref _pluginConfiguration.JobColorSCH);
                    changed |= ImGui.ColorEdit4("Job Color MNK", ref _pluginConfiguration.JobColorMNK);
                    changed |= ImGui.ColorEdit4("Job Color DRG", ref _pluginConfiguration.JobColorDRG);
                    changed |= ImGui.ColorEdit4("Job Color NIN", ref _pluginConfiguration.JobColorNIN);
                    changed |= ImGui.ColorEdit4("Job Color SAM", ref _pluginConfiguration.JobColorSAM);
                    changed |= ImGui.ColorEdit4("Job Color BRD", ref _pluginConfiguration.JobColorBRD);
                    changed |= ImGui.ColorEdit4("Job Color MCH", ref _pluginConfiguration.JobColorMCH);
                    changed |= ImGui.ColorEdit4("Job Color DNC", ref _pluginConfiguration.JobColorDNC);
                    changed |= ImGui.ColorEdit4("Job Color BLM", ref _pluginConfiguration.JobColorBLM);
                    changed |= ImGui.ColorEdit4("Job Color SMN", ref _pluginConfiguration.JobColorSMN);
                    changed |= ImGui.ColorEdit4("Job Color RDM", ref _pluginConfiguration.JobColorRDM);
                    changed |= ImGui.ColorEdit4("Job Color BLU", ref _pluginConfiguration.JobColorBLU);

                    changed |= ImGui.ColorEdit4("NPC Color Hostile", ref _pluginConfiguration.NPCColorHostile);
                    changed |= ImGui.ColorEdit4("NPC Color Neutral", ref _pluginConfiguration.NPCColorNeutral);
                    changed |= ImGui.ColorEdit4("NPC Color Friendly", ref _pluginConfiguration.NPCColorFriendly);

                    //ImGui.Spacing();
                    ImGui.EndTabItem();
                }

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

                    changed |= ImGui.ColorEdit4("Fairy Bar Color", ref _pluginConfiguration.SchFairyColor);
                    changed |= ImGui.ColorEdit4("Aether Bar Color", ref _pluginConfiguration.SchAetherColor);
                    changed |= ImGui.ColorEdit4("Bar Not Full Color", ref _pluginConfiguration.SchEmptyColor);

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("White mage"))
                {

                    var lilyBarHeight = _pluginConfiguration.WHMLilyBarHeight;
                    if (ImGui.DragInt("Lily Bar Height", ref lilyBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.WHMLilyBarHeight = lilyBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var lilyBarWidth = _pluginConfiguration.WHMLilyBarWidth;
                    if (ImGui.DragInt("Lily Bar Width", ref lilyBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.WHMLilyBarWidth = lilyBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var lilyBarX = _pluginConfiguration.WHMLilyBarX;
                    if (ImGui.DragInt("Lily Bar X Offset", ref lilyBarX, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.WHMLilyBarX = lilyBarX;
                        _pluginConfiguration.Save();
                    }

                    var lilyBarY = _pluginConfiguration.WHMLilyBarY;
                    if (ImGui.DragInt("Lily Bar Y Offset", ref lilyBarY, .1f, -1000, 1000))
                    {
                        _pluginConfiguration.WHMLilyBarY = lilyBarY;
                        _pluginConfiguration.Save();
                    }

                    var whmLilyBarPad = _pluginConfiguration.WHMLilyBarPad;
                    if (ImGui.DragInt("Lily Bar Padding", ref whmLilyBarPad, .1f, -100, 1000))
                    {
                        _pluginConfiguration.WHMLilyBarPad = whmLilyBarPad;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Lily Bar Color", ref _pluginConfiguration.WHMLilyColor);
                    changed |= ImGui.ColorEdit4("Blood Lily Bar Color", ref _pluginConfiguration.WHMBloodLilyColor);
                    changed |= ImGui.ColorEdit4("Lily Bar Charging Color", ref _pluginConfiguration.WHMChargingColor);
                    changed |= ImGui.ColorEdit4("Lily Bar Empty", ref _pluginConfiguration.WHMEmptyColor);

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
                    changed |= ImGui.ColorEdit4("Nascent Chaos Ready Color", ref _pluginConfiguration.WARNascentChaosColor);
                    changed |= ImGui.ColorEdit4("Bar Not Full Color", ref _pluginConfiguration.WAREmptyColor);
                    
                    ImGui.EndTabItem();
                }

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

                    int pldInterBarOffset = _pluginConfiguration.PLDInterBarOffset;
                    if (ImGui.DragInt("Space Between Bars", ref pldInterBarOffset, 0.1f, -2000, 2000))
                    {
                        _pluginConfiguration.PLDInterBarOffset = pldInterBarOffset;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Mana Bar Color", ref _pluginConfiguration.PLDManaColor);
                    changed |= ImGui.ColorEdit4("Oath Gauge Color", ref _pluginConfiguration.PLDOathGaugeColor);
                    changed |= ImGui.ColorEdit4("Fight or Flight Color", ref _pluginConfiguration.PLDFightOrFlightColor);
                    changed |= ImGui.ColorEdit4("Requiescat Color", ref _pluginConfiguration.PLDRequiescatColor);
                    changed |= ImGui.ColorEdit4("Atonement Color", ref _pluginConfiguration.PLDAtonementColor);
                    changed |= ImGui.ColorEdit4("Bar Not Full Color", ref _pluginConfiguration.PLDEmptyColor);
                    
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            if (changed) {
                _pluginConfiguration.BuildColorMap();
                _pluginConfiguration.Save();
            }

            ImGui.End();
        }
    }
}
