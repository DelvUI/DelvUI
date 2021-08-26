using System.Numerics;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUIPlugin.Interface {
    public class ConfigurationWindow {
        public bool IsVisible = false;
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

            int ViewportWidth = (int) ImGui.GetMainViewport().Size.X;
            int ViewportHeight = (int) ImGui.GetMainViewport().Size.Y;
            int XOffsetLimit = ViewportWidth / 2;
            int YOffsetLimit = ViewportHeight / 2;

            if (ImGui.BeginTabBar("##settings-tabs")) {
                if (ImGui.BeginTabItem("General"))
                {
                    var verticalOffset = _pluginConfiguration.VerticalOffset;
                    if (ImGui.DragInt("Vertical Offset", ref verticalOffset, 1f, -2000, 2000))
                    {
                        _pluginConfiguration.VerticalOffset = verticalOffset;
                        _pluginConfiguration.Save();
                    }

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
                    if (ImGui.DragInt("Focus Width", ref totBarWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.FocusBarWidth = focusBarWidth;
                        _pluginConfiguration.Save();
                    }

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
                    if (ImGui.DragInt("Castbar X Offset", ref castBarXOffset, .1f, -XOffsetLimit, XOffsetLimit))
                    {
                        _pluginConfiguration.CastBarXOffset = castBarXOffset;
                        _pluginConfiguration.Save();
                    }

                    var castBarYOffset = _pluginConfiguration.CastBarYOffset;
                    if (ImGui.DragInt("Castbar Y Offset", ref castBarYOffset, .1f, -YOffsetLimit, YOffsetLimit))
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
                }

                if (ImGui.BeginTabItem("Black Mage"))
                {
                    var BLMVerticalOffset = _pluginConfiguration.BLMVerticalOffset;
                    if (ImGui.DragInt("Vertical Offset", ref BLMVerticalOffset, 1f, -1000, 1000))
                    {
                        _pluginConfiguration.BLMVerticalOffset = BLMVerticalOffset;
                        _pluginConfiguration.Save();
                    }

                    var BLMManaBarHeight = _pluginConfiguration.BLMManaBarHeight;
                    if (ImGui.DragInt("Mana Bar Height", ref BLMManaBarHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BLMManaBarHeight = BLMManaBarHeight;
                        _pluginConfiguration.Save();
                    }

                    var BLMManaBarWidth = _pluginConfiguration.BLMManaBarWidth;
                    if (ImGui.DragInt("Mana Bar Width", ref BLMManaBarWidth, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.BLMManaBarWidth = BLMManaBarWidth;
                        _pluginConfiguration.Save();
                    }

                    var BLMUmbralHeartHeight = _pluginConfiguration.BLMUmbralHeartHeight;
                    if (ImGui.DragInt("Umbral Heart Height", ref BLMUmbralHeartHeight, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.BLMUmbralHeartHeight = BLMUmbralHeartHeight;
                        _pluginConfiguration.Save();
                    }

                    var BLMPolyglotHeight = _pluginConfiguration.BLMPolyglotHeight;
                    if (ImGui.DragInt("Polyglot Height", ref BLMPolyglotHeight, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BLMPolyglotHeight = BLMPolyglotHeight;
                        _pluginConfiguration.Save();
                    }

                    var BLMPolyglotWidth = _pluginConfiguration.BLMPolyglotWidth;
                    if (ImGui.DragInt("Polyglot Width", ref BLMPolyglotWidth, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BLMPolyglotWidth = BLMPolyglotWidth;
                        _pluginConfiguration.Save();
                    }

                    var BLMVerticalSpaceBetweenBars = _pluginConfiguration.BLMVerticalSpaceBetweenBars;
                    if (ImGui.DragInt("Vertical Padding", ref BLMVerticalSpaceBetweenBars, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BLMVerticalSpaceBetweenBars = BLMVerticalSpaceBetweenBars;
                        _pluginConfiguration.Save();
                    }

                    var BLMHorizontalSpaceBetweenBars = _pluginConfiguration.BLMHorizontalSpaceBetweenBars;
                    if (ImGui.DragInt("Horizontal Padding", ref BLMHorizontalSpaceBetweenBars, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BLMHorizontalSpaceBetweenBars = BLMHorizontalSpaceBetweenBars;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.Checkbox("Show Triplecast", ref _pluginConfiguration.BLMShowTripleCast);
                    changed |= ImGui.Checkbox("Use Bars for Triplecast", ref _pluginConfiguration.BLMUseBarsForTripleCast);

                    var BLMTripleCastHeight = _pluginConfiguration.BLMTripleCastHeight;
                    if (ImGui.DragInt("Triplecast Bar Height", ref BLMTripleCastHeight, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.BLMTripleCastHeight = BLMTripleCastHeight;
                        _pluginConfiguration.Save();
                    }

                    var BLMTripleCastWidth = _pluginConfiguration.BLMTripleCastWidth;
                    if (ImGui.DragInt("Triplecast Bar Width", ref BLMTripleCastWidth, .1f, -2000, 2000))
                    {
                        _pluginConfiguration.BLMTripleCastWidth = BLMTripleCastWidth;
                        _pluginConfiguration.Save();
                    }

                    var BLMTripleCastRadius = _pluginConfiguration.BLMTripleCastRadius;
                    if (ImGui.DragInt("Triplecast radius", ref BLMTripleCastRadius, .1f, 1, 1000))
                    {
                        _pluginConfiguration.BLMTripleCastRadius = BLMTripleCastRadius;
                        _pluginConfiguration.Save();
                    }

                    changed |= ImGui.ColorEdit4("Mana Bar Color", ref _pluginConfiguration.BLMManaBarNoElementColor);
                    changed |= ImGui.ColorEdit4("Mana Bar Ice Color", ref _pluginConfiguration.BLMManaBarIceColor);
                    changed |= ImGui.ColorEdit4("Mana Bar Fire Color", ref _pluginConfiguration.BLMManaBarFireColor);
                    changed |= ImGui.ColorEdit4("Umbral Heart Color", ref _pluginConfiguration.BLMUmbralHeartColor);
                    changed |= ImGui.ColorEdit4("Polyglot Color", ref _pluginConfiguration.BLMPolyglotColor);
                    changed |= ImGui.ColorEdit4("Triplecast Color", ref _pluginConfiguration.BLMTriplecastColor);
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