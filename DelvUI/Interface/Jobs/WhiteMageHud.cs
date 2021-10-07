using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Newtonsoft.Json;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Statuses;

namespace DelvUI.Interface.Jobs
{
    public class WhiteMageHud : JobHud
    {
        private new WhiteMageConfig Config => (WhiteMageConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;
        private PluginConfigColor PartialFillColor => GlobalColors.Instance.PartialFillColor;
        private readonly SpellHelper _spellHelper = new();


        public WhiteMageHud(string id, WhiteMageConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.ShowLilyBars)
            {
                positions.Add(Config.Position + Config.LilyBarPosition);
                sizes.Add(Config.LilyBarSize);
                positions.Add(Config.Position + Config.BloodLilyBarPosition);
                sizes.Add(Config.BloodLilyBarSize);
            }

            if (Config.ShowDiaBar)
            {
                positions.Add(Config.Position + Config.DiaBarPosition);
                sizes.Add(Config.DiaBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawJobHud(Vector2 origin, PlayerCharacter player)
        {
            if (Config.ShowLilyBars) { DrawLilyBars(origin); }
            if (Config.ShowDiaBar) { DrawDiaBar(origin, player); }
            if (Config.ShowAsylumBar) { DrawAsylumBar(origin, player); }
            if (Config.ShowPresenceOfMindBar) { DrawPresenceOfMindBar(origin, player); }
            if (Config.ShowPlenaryBar) { DrawPlenaryBar(origin, player); }
            if (Config.ShowTemperanceBar) { DrawTemperanceBar(origin, player); }
        }

        private void DrawDiaBar(Vector2 origin, PlayerCharacter player)
        {
            var actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            Vector2 cursorPos = origin + Config.Position + Config.DiaBarPosition - Config.DiaBarSize / 2f;

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (actor is not BattleChara target)
            {
                if (Config.OnlyShowDiaWhenActive)
                {
                    return;
                }

                drawList.AddRectFilled(cursorPos, cursorPos + Config.DiaBarSize, EmptyColor.Background);
                drawList.AddRect(cursorPos, cursorPos + Config.DiaBarSize, 0xFF000000);

                return;
            }

            Status? dia = target.StatusList.FirstOrDefault(o => o.StatusId is 1871 or 144 or 143 && o.SourceID == player.ObjectId);

            float diaCooldown = 0f;
            float diaDuration = 0f;

            if (dia != null)
            {
                diaCooldown = dia.StatusId == 1871 ? 30f : 18f;
                diaDuration = Math.Abs(dia.RemainingTime);
            }

            if (Config.OnlyShowDiaWhenActive && diaDuration == 0)
            {
                return;
            }

            drawList.AddRectFilled(cursorPos, cursorPos + Config.DiaBarSize, EmptyColor.Background);

            drawList.AddRectFilled(
                cursorPos,
                cursorPos + new Vector2(Config.DiaBarSize.X / diaCooldown * diaDuration, Config.DiaBarSize.Y),
                Config.ShowDiaRefresh
                    ? diaDuration >= Config.DiaCustomRefresh
                        ? Config.DiaColor.BottomGradient
                        : Config.DiaRefreshColor.BottomGradient
                    : Config.DiaColor.BottomGradient
            );

            drawList.AddRect(cursorPos, cursorPos + Config.DiaBarSize, 0xFF000000);

            if (!Config.ShowDiaTimer)
            {
                return;
            }

            DrawHelper.DrawOutlinedText(
                string.Format(CultureInfo.InvariantCulture, "{0,2:N0}", diaDuration), // keeps 10 -> 9 from jumping
                new Vector2(
                    // smooths transition of counter to the right of the emptying bar
                    cursorPos.X
                  + Config.DiaBarSize.X * diaDuration / diaCooldown
                  - (Math.Abs(diaDuration - diaCooldown) < float.Epsilon
                        ? diaCooldown
                        : diaDuration > 3
                            ? 20
                            : diaDuration * (20f / 3f)),
                    cursorPos.Y + Config.DiaBarSize.Y / 2 - 12
                ),
                drawList
            );
        }

        private void DrawLilyBars(Vector2 origin)
        {
            WHMGauge gauge = Plugin.JobGauges.Get<WHMGauge>();

            const float lilyCooldown = 30000f;

            float GetScale(int num, float timer) => num + (timer / lilyCooldown);

            float lilyScale = GetScale(gauge.Lily, gauge.LilyTimer);

            if (lilyScale == 0 && Config.OnlyShowLilyWhenActive)
            {
                return;
            }

            var posX = origin.X + Config.Position.X + Config.LilyBarPosition.X - Config.LilyBarSize.X / 2f;
            var posY = origin.Y + Config.Position.Y + Config.LilyBarPosition.Y - Config.LilyBarSize.Y / 2f;

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            BarBuilder builder = BarBuilder.Create(posX, posY, Config.LilyBarSize.Y, Config.LilyBarSize.X).SetBackgroundColor(EmptyColor.Background);

            builder.SetChunks(3).SetChunkPadding(Config.LilyBarPad).AddInnerBar(lilyScale, 3, Config.LilyColor, PartialFillColor);

            if (Config.ShowLilyBarTimer)
            {
                string timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                Vector2 size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                float lilyChunkSize = (Config.LilyBarSize.X / 3f) + Config.LilyBarPad;
                float lilyChunkOffset = lilyChunkSize * (gauge.Lily + 1);

                if (gauge.Lily < 3)
                {
                    DrawHelper.DrawOutlinedText(timer, new Vector2(
                        posX + lilyChunkOffset - (lilyChunkSize / 2f) - (size.X / 2f),
                        posY - Config.LilyBarSize.Y - 4f), drawList);
                }
            }

            builder.Build().Draw(drawList);

            posX = origin.X + Config.Position.X + Config.BloodLilyBarPosition.X - Config.BloodLilyBarSize.X / 2f;
            posY = origin.Y + Config.Position.Y + Config.BloodLilyBarPosition.Y - Config.BloodLilyBarSize.Y / 2f;

            builder = BarBuilder.Create(posX, posY, Config.BloodLilyBarSize.Y, Config.BloodLilyBarSize.X).SetBackgroundColor(EmptyColor.Background);

            builder.SetChunks(3).SetChunkPadding(Config.BloodLilyBarPad).AddInnerBar(gauge.BloodLily, 3, Config.BloodLilyColor);

            drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawAsylumBar(Vector2 origin, PlayerCharacter player)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            const int spellAction = 3569; // Asylum Action

            float duration = player.StatusList.FirstOrDefault(o => o.StatusId is 739 or 1911 or 1912 && o.SourceID == player.ObjectId)?.RemainingTime ?? 0f;
            if (duration == 0 && Config.OnlyShowAsylumWhenActive) { return; }

            const float maxDuration = 24f;

            float cooldown = _spellHelper.GetSpellCooldown(spellAction);
            const float maxCooldown = 90f;

            float xPos = origin.X + Config.Position.X + Config.AsylumPosition.X - Config.AsylumSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.AsylumPosition.Y - Config.AsylumSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.AsylumSize.Y, Config.AsylumSize.X).SetBackgroundColor(EmptyColor.Background);
            var color = duration > 3 ? Config.AsylumColor : Config.AsylumCoooldownColor;

            if (Config.TrackAsylumCooldown)
            {
                float currentValue = cooldown >= 0 && duration <= 0 ? maxCooldown - cooldown : duration;
                float maximumValue = cooldown >= 0 && duration <= 0 ? maxCooldown : maxDuration;

                color = duration != 0 || (duration == 0 && cooldown == 0) ? Config.AsylumColor : Config.AsylumCoooldownColor;

                builder.AddInnerBar(currentValue, maximumValue, color);
            }
            else
            {
                builder.AddInnerBar(duration, maxDuration, color);
            }

            if (Config.ShowAsylumText)
            {
                var positon = BarTextPosition.CenterMiddle;
                var type = BarTextType.Custom;
                var mode = BarTextMode.Single;

                if (Config.TrackAsylumCooldown)
                {
                    var text = cooldown >= 0 && duration <= 0
                        ? cooldown == 0
                            ? "Ready"
                            : cooldown.ToString("N0")

                        : duration == 0
                            ? ""
                            : duration.ToString("N0")
                    ;

                    builder.SetText(positon, type, text);
                }
                else if (duration != 0)
                {
                    builder.SetText(positon, BarTextType.Current);
                }

                builder.SetTextMode(mode);
            }

            builder.Build().Draw(drawList);
        }

        private void DrawPresenceOfMindBar(Vector2 origin, PlayerCharacter player)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            const int spellStatus = 157; // Presence of Mind Status
            const int spellAction = 136; // Presence of Mind Action            

            float duration = player.StatusList.FirstOrDefault(o => o.StatusId == spellStatus)?.RemainingTime ?? 0f;
            if (duration == 0 && Config.OnlyShowPoMWhenActive) { return; }

            const float maxDuration = 15f;

            float cooldown = _spellHelper.GetSpellCooldown(spellAction);
            const float maxCooldown = 150f;

            float xPos = origin.X + Config.Position.X + Config.PresenceOfMindPosition.X - Config.PresenceOfMindSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.PresenceOfMindPosition.Y - Config.PresenceOfMindSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.PresenceOfMindSize.Y, Config.PresenceOfMindSize.X).SetBackgroundColor(EmptyColor.Base);
            var color = duration > 3 ? Config.PresenceOfMindColor : Config.PresenceOfMindCooldownColor;

            if (Config.TrackPoMCooldown)
            {
                var currentValue = cooldown >= 0 && duration <= 0 ? maxCooldown - cooldown : duration;
                var maximumValue = cooldown >= 0 && duration <= 0 ? maxCooldown : maxDuration;

                color = duration != 0 || (duration == 0 && cooldown == 0) ? Config.PresenceOfMindColor : Config.PresenceOfMindCooldownColor;

                builder.AddInnerBar(currentValue, maximumValue, color);
            }
            else
            {
                builder.AddInnerBar(duration, maxDuration, color);
            }

            if (Config.ShowPoMText)
            {
                var positon = BarTextPosition.CenterMiddle;
                var type = BarTextType.Custom;
                var mode = BarTextMode.Single;

                if (Config.TrackPoMCooldown)
                {
                    var text = cooldown >= 0 && duration <= 0
                        ? cooldown == 0
                            ? "Ready"
                            : cooldown.ToString("N0")

                        : duration == 0
                            ? ""
                            : duration.ToString("N0")
                    ;
                    builder.SetText(positon, type, text);
                }
                else if (duration != 0)
                {
                    builder.SetText(positon, BarTextType.Current);
                }

                builder.SetTextMode(mode);
            }

            builder.Build().Draw(drawList);
        }

        private void DrawPlenaryBar(Vector2 origin, PlayerCharacter player)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            const int spellStatus = 1219; // Plenary Indulgence Status
            const int spellAction = 7433; // Plenary Indulgence Action            

            float duration = player.StatusList.FirstOrDefault(o => o.StatusId == spellStatus)?.RemainingTime ?? 0f;
            if (duration == 0 && Config.OnlyShowPlenaryWhenActive) { return; }

            const float maxDuration = 10f;

            float cooldown = _spellHelper.GetSpellCooldown(spellAction);
            const float maxCooldown = 60f;

            float xPos = origin.X + Config.Position.X + Config.PlenaryPosition.X - Config.PlenarySize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.PlenaryPosition.Y - Config.PlenarySize.Y / 2f;



            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.PlenarySize.Y, Config.PlenarySize.X).SetBackgroundColor(EmptyColor.Background);

            var color = duration > 3 ? Config.PlenaryColor : Config.PlenaryCooldownColor;

            if (Config.TrackPlenaryCooldown)
            {
                var currentValue = cooldown >= 0 && duration <= 0 ? maxCooldown - cooldown : duration;
                var maximumValue = cooldown >= 0 && duration <= 0 ? maxCooldown : maxDuration;

                color = duration != 0 || (duration == 0 && cooldown == 0) ? Config.PlenaryColor : Config.PlenaryCooldownColor;

                builder.AddInnerBar(currentValue, maximumValue, color);
            }
            else
            {
                builder.AddInnerBar(duration, maxDuration, color);
            }

            if (Config.ShowPlenaryText)
            {
                var positon = BarTextPosition.CenterMiddle;
                var type = BarTextType.Custom;
                var mode = BarTextMode.Single;

                if (Config.TrackPlenaryCooldown)
                {
                    var text = cooldown >= 0 && duration <= 0
                        ? cooldown == 0
                            ? "Ready"
                            : cooldown.ToString("N0")

                        : duration == 0
                            ? ""
                            : duration.ToString("N0")
                    ;
                    builder.SetText(positon, type, text);
                }
                else if (duration != 0)
                {
                    builder.SetText(positon, BarTextType.Current);
                }

                builder.SetTextMode(mode);
            }

            builder.Build().Draw(drawList);
        }

        private void DrawTemperanceBar(Vector2 origin, PlayerCharacter player)
        {
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            const int spellStatus = 1872; // Temperance Status
            const int spellAction = 16536; // Temperance Action            

            float duration = player.StatusList.FirstOrDefault(o => o.StatusId == spellStatus)?.RemainingTime ?? 0f;
            if (duration == 0 && Config.OnlyShowTemperanceWhenActive) { return; }

            const float maxDuration = 20f;

            float cooldown = _spellHelper.GetSpellCooldown(spellAction);
            const float maxCooldown = 120f;

            float xPos = origin.X + Config.Position.X + Config.TemperancePosition.X - Config.TemperanceSize.X / 2f;
            float yPos = origin.Y + Config.Position.Y + Config.TemperancePosition.Y - Config.TemperanceSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.TemperanceSize.Y, Config.TemperanceSize.X).SetBackgroundColor(EmptyColor.Background);

            var color = duration > 3 ? Config.TemperanceColor : Config.TemperanceCooldownColor;

            if (Config.TrackTemperanceCooldown)
            {
                var currentValue = cooldown >= 0 && duration <= 0 ? maxCooldown - cooldown : duration;
                var maximumValue = cooldown >= 0 && duration <= 0 ? maxCooldown : maxDuration;

                color = duration != 0 || (duration == 0 && cooldown == 0) ? Config.TemperanceColor : Config.TemperanceCooldownColor;

                builder.AddInnerBar(currentValue, maximumValue, color);
            }
            else
            {
                builder.AddInnerBar(duration, maxDuration, color);
            }

            if (Config.ShowTemperanceText)
            {
                var positon = BarTextPosition.CenterMiddle;
                var type = BarTextType.Custom;
                var mode = BarTextMode.Single;

                if (Config.TrackTemperanceCooldown)
                {
                    var text = cooldown >= 0 && duration <= 0
                        ? cooldown == 0
                            ? "Ready"
                            : cooldown.ToString("N0")

                        : duration == 0
                            ? ""
                            : duration.ToString("N0")
                    ;


                    builder.SetText(positon, type, text);
                }
                else if (duration != 0)
                {
                    builder.SetText(positon, BarTextType.Current);
                }


                builder.SetTextMode(mode);
            }

            builder.Build().Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("White Mage", 1)]
    public class WhiteMageConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.WHM;
        public new static WhiteMageConfig DefaultConfig()
        {
            var config = new WhiteMageConfig();
            config.UseDefaultPrimaryResourceBar = true;
            return config;
        }

        #region Lily Bar
        [Checkbox("Lily" + "##Lily", separator = true)]
        [Order(30)]
        public bool ShowLilyBars = true;

        // hide lily bar if inactive
        [Checkbox("Only Show When Active" + "##Lily")]
        [Order(31, collapseWith = nameof(ShowLilyBars))]
        public bool OnlyShowLilyWhenActive = false;

        [Checkbox("Timer" + "##Lily")]
        [Order(35, collapseWith = nameof(ShowLilyBars))]
        public bool ShowLilyBarTimer = false;

        [DragFloat2("Position" + "##Lily", min = -4000f, max = 4000f)]
        [Order(40, collapseWith = nameof(ShowLilyBars))]
        public Vector2 LilyBarPosition = new(-64, -54);

        [DragFloat2("Size" + "##Lily", max = 2000f)]
        [Order(45, collapseWith = nameof(ShowLilyBars))]
        public Vector2 LilyBarSize = new(125, 20);

        [DragInt("Spacing" + "##Lily", min = 0, max = 1000)]
        [Order(50, collapseWith = nameof(ShowLilyBars))]
        public int LilyBarPad = 2;

        [ColorEdit4("Color" + "##Lily")]
        [Order(55, collapseWith = nameof(ShowLilyBars))]
        public PluginConfigColor LilyColor = new(new Vector4(0f / 255f, 64f / 255f, 1f, 1f));

        [ColorEdit4("Charging Color" + "##Lily")] //TODO CHANGE TO GLOBAL PARTIALLY FILLED COLOR?
        [Order(60, collapseWith = nameof(ShowLilyBars))]
        public PluginConfigColor LilyChargingColor = new(new Vector4(141f / 255f, 141f / 255f, 141f / 255f, 1f));
        #endregion

        #region Blood Lily Bar

        [DragFloat2("Position" + "##BloodLily", min = -4000f, max = 4000f, spacing = true)]
        [Order(65, collapseWith = nameof(ShowLilyBars))]
        public Vector2 BloodLilyBarPosition = new(64, -54);

        [DragFloat2("Size" + "##BloodLily", max = 2000f)]
        [Order(70, collapseWith = nameof(ShowLilyBars))]
        public Vector2 BloodLilyBarSize = new(125, 20);

        [DragInt("Spacing" + "##BloodLily", min = 0, max = 1000)]
        [Order(75, collapseWith = nameof(ShowLilyBars))]
        public int BloodLilyBarPad = 2;

        [ColorEdit4("Color" + "##BloodLily")]
        [Order(80, collapseWith = nameof(ShowLilyBars))]
        public PluginConfigColor BloodLilyColor = new(new Vector4(199f / 255f, 40f / 255f, 9f / 255f, 1f));
        #endregion

        #region Dia Bar

        // enable
        [Checkbox("Dia" + "##Dia", separator = true)]
        [Order(85)]
        public bool ShowDiaBar = true;

        // hide dia bar if inactive
        [Checkbox("Only Show When Active" + "##Dia")]
        [Order(86, collapseWith = nameof(ShowDiaBar))]
        public bool OnlyShowDiaWhenActive = false;

        // show dia timer
        [Checkbox("Timer" + "##Dia")]
        [Order(90, collapseWith = nameof(ShowDiaBar))]
        public bool ShowDiaTimer = false;

        // pos
        [DragFloat2("Position" + "##Dia", min = -4000f, max = 4000f)]
        [Order(95, collapseWith = nameof(ShowDiaBar))]
        public Vector2 DiaBarPosition = new(0, -32);

        // size
        [DragFloat2("Size " + "##Dia", max = 2000f)]
        [Order(100, collapseWith = nameof(ShowDiaBar))]
        public Vector2 DiaBarSize = new(254, 20);

        // color
        [ColorEdit4("Color" + "##Dia")]
        [Order(105, collapseWith = nameof(ShowDiaBar))]
        public PluginConfigColor DiaColor = new(new Vector4(0f / 255f, 64f / 255f, 1f, 1f));

        // refresh reminder enable
        [Checkbox("Show Refresh Reminder" + "##Dia", spacing = true)]
        [Order(110, collapseWith = nameof(ShowDiaBar))]
        public bool ShowDiaRefresh = false;

        // refresh reminder value
        [DragInt("Refresh Reminder" + "##Dia", min = 0, max = 30)]
        [Order(115, collapseWith = nameof(ShowDiaBar))]
        public int DiaCustomRefresh = 3;

        // refresh reminder color
        [ColorEdit4("Refresh Color" + "##Dia")]
        [Order(120, collapseWith = nameof(ShowDiaBar))]
        public PluginConfigColor DiaRefreshColor = new(new(190f / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        #endregion

        #region Asylum Bar

        [Checkbox("Asylum", separator = true)]
        [Order(130)]
        public bool ShowAsylumBar = false;

        [Checkbox("Only Show When Active" + "##Asylum")]
        [Order(135, collapseWith = nameof(ShowAsylumBar))]
        public bool OnlyShowAsylumWhenActive = false;

        [Checkbox("Timer" + "##Asylum")]
        [Order(140, collapseWith = nameof(ShowAsylumBar))]
        public bool ShowAsylumText = true;

        [Checkbox("Track Cooldown" + "##Asylum")]
        [Order(145, collapseWith = nameof(ShowAsylumBar))]
        public bool TrackAsylumCooldown = false;

        [DragFloat2("Position" + "##Asylum", min = -4000f, max = 4000f)]
        [Order(150, collapseWith = nameof(ShowAsylumBar))]
        public Vector2 AsylumPosition = new(-96, -74);

        [DragFloat2("Size " + "##Asylum", max = 2000f)]
        [Order(155, collapseWith = nameof(ShowAsylumBar))]
        public Vector2 AsylumSize = new(62, 15);

        [ColorEdit4("Color" + "##Asylum")]
        [Order(160, collapseWith = nameof(ShowAsylumBar))]
        public PluginConfigColor AsylumColor = new(new Vector4(241f / 255f, 217f / 255f, 125f / 255f, 100f / 100f));

        [ColorEdit4("Cooldown Color" + "##Asylum")]
        [Order(165, collapseWith = nameof(ShowAsylumBar))]
        public PluginConfigColor AsylumCoooldownColor = new(new Vector4(190 / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        #endregion

        #region Presence of Mind

        // enable
        [Checkbox("Presence of Mind", separator = true)]
        [Order(170)]
        public bool ShowPresenceOfMindBar = false;

        [Checkbox("Only Show When Active" + "##PresenceOfMind")]
        [Order(171, collapseWith = nameof(ShowPresenceOfMindBar))]
        public bool OnlyShowPoMWhenActive = false;

        [Checkbox("Timer" + "##PresenceOfMind")]
        [Order(172, collapseWith = nameof(ShowPresenceOfMindBar))]
        public bool ShowPoMText = true;

        [Checkbox("Track Cooldown" + "##PresenceOfMind")]
        [Order(173, collapseWith = nameof(ShowPresenceOfMindBar))]
        public bool TrackPoMCooldown = false;

        // pos
        [DragFloat2("Position" + "##PresenceOfMind", min = -4000f, max = 4000f)]
        [Order(175, collapseWith = nameof(ShowPresenceOfMindBar))]
        public Vector2 PresenceOfMindPosition = new(-32, -74);

        // size
        [DragFloat2("Size " + "##PresenceOfMind", max = 2000f)]
        [Order(180, collapseWith = nameof(ShowPresenceOfMindBar))]
        public Vector2 PresenceOfMindSize = new(62, 15);

        // color
        [ColorEdit4("Color" + "##PresenceOfMind")]
        [Order(185, collapseWith = nameof(ShowPresenceOfMindBar))]
        public PluginConfigColor PresenceOfMindColor = new(new Vector4(213f / 255f, 124f / 255f, 97f / 255f, 100f / 100f));

        [ColorEdit4("Cooldown Color" + "##PresenceOfMind")]
        [Order(186, collapseWith = nameof(ShowPresenceOfMindBar))]
        public PluginConfigColor PresenceOfMindCooldownColor = new(new Vector4(190 / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        #endregion

        #region Plenary

        // enable
        [Checkbox("Plenary Indulgence", separator = true)]
        [Order(190)]
        public bool ShowPlenaryBar = false;

        [Checkbox("Only Show When Active" + "##Plenary")]
        [Order(191, collapseWith = nameof(ShowPlenaryBar))]
        public bool OnlyShowPlenaryWhenActive = false;

        [Checkbox("Timer" + "##Plenary")]
        [Order(192, collapseWith = nameof(ShowPlenaryBar))]
        public bool ShowPlenaryText = true;

        [Checkbox("Track Cooldown" + "##Plenary")]
        [Order(193, collapseWith = nameof(ShowPlenaryBar))]
        public bool TrackPlenaryCooldown = false;

        // pos
        [DragFloat2("Position" + "##Plenary", min = -4000f, max = 4000f)]
        [Order(195, collapseWith = nameof(ShowPlenaryBar))]
        public Vector2 PlenaryPosition = new(32, -74);

        // size
        [DragFloat2("Size " + "##Plenary", max = 2000f)]
        [Order(200, collapseWith = nameof(ShowPlenaryBar))]
        public Vector2 PlenarySize = new(62, 15);

        // color
        [ColorEdit4("Color" + "##Plenary")]
        [Order(205, collapseWith = nameof(ShowPlenaryBar))]
        public PluginConfigColor PlenaryColor = new(new Vector4(26f / 255f, 167f / 255f, 109f / 255f, 100f / 100f));

        [ColorEdit4("Cooldown Color" + "##Plenary")]
        [Order(206, collapseWith = nameof(ShowPlenaryBar))]
        public PluginConfigColor PlenaryCooldownColor = new(new Vector4(190 / 255f, 28f / 255f, 57f / 255f, 100f / 100f));
        #endregion

        #region Temperance

        // enable
        [Checkbox("Temperance", separator = true)]
        [Order(220)]
        public bool ShowTemperanceBar = false;

        [Checkbox("Only Show When Active" + "##Temperance")]
        [Order(221, collapseWith = nameof(ShowTemperanceBar))]
        public bool OnlyShowTemperanceWhenActive = false;

        [Checkbox("Timer" + "##Temperance")]
        [Order(222, collapseWith = nameof(ShowTemperanceBar))]
        public bool ShowTemperanceText = true;

        [Checkbox("Track Cooldown" + "##Temperance")]
        [Order(223, collapseWith = nameof(ShowTemperanceBar))]
        public bool TrackTemperanceCooldown = false;

        // pos
        [DragFloat2("Position" + "##Temperance", min = -4000f, max = 4000f)]
        [Order(225, collapseWith = nameof(ShowTemperanceBar))]
        public Vector2 TemperancePosition = new(96, -74);

        // size
        [DragFloat2("Size " + "##Temperance", max = 2000f)]
        [Order(230, collapseWith = nameof(ShowTemperanceBar))]
        public Vector2 TemperanceSize = new(62, 15);

        // color
        [ColorEdit4("Color" + "##Temperance")]
        [Order(235, collapseWith = nameof(ShowTemperanceBar))]
        public PluginConfigColor TemperanceColor = new(new Vector4(100f / 255f, 207f / 255f, 211f / 255f, 100f / 100f));

        [ColorEdit4("Cooldown Color" + "##Temperance")]
        [Order(236, collapseWith = nameof(ShowTemperanceBar))]
        public PluginConfigColor TemperanceCooldownColor = new(new Vector4(190 / 255f, 28f / 255f, 57f / 255f, 100f / 100f));

        #endregion
    }
}
