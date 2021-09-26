using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;

namespace DelvUI.Interface.Jobs
{
    public class DarkKnightHud : JobHud
    {
        private new DarkKnightConfig Config => (DarkKnightConfig)_config;

        public DarkKnightHud(string id, DarkKnightConfig config, string? displayName = null) : base(id, config, displayName)
        {

        }

        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;
        private PluginConfigColor PartialFillColor => GlobalColors.Instance.PartialFillColor;

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new();
            List<Vector2> sizes = new();

            if (Config.ShowManaBar)
            {
                positions.Add(Config.Position + Config.ManaBarPosition);
                sizes.Add(Config.ManaBarSize);
            }

            if (Config.ShowBloodGauge)
            {
                positions.Add(Config.Position + Config.BloodGaugePosition);
                sizes.Add(Config.BloodGaugeSize);
            }

            if (Config.ShowBuffBar)
            {
                positions.Add(Config.Position + Config.BuffBarPosition);
                sizes.Add(Config.BuffBarSize);
            }

            if (Config.ShowLivingShadowBar)
            {
                positions.Add(Config.Position + Config.LivingShadowBarPosition);
                sizes.Add(Config.LivingShadowBarSize);
            }

            return (positions, sizes);
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Config.ShowManaBar)
            {
                DrawManaBar(origin);
            }

            if (Config.ShowBloodGauge)
            {
                DrawBloodGauge(origin);
            }

            if (Config.ShowDarkside)
            {
                DrawDarkside(origin);
            }

            if (Config.ShowBuffBar)
            {
                DrawBuffBar(origin);
            }

            if (Config.ShowLivingShadowBar)
            {
                DrawLivingShadowBar(origin);
            }
        }

        private void DrawManaBar(Vector2 origin)
        {
            var actor = Plugin.ClientState.LocalPlayer;
            if (actor is null)
            {
                return;
            }

            var darkArtsBuff = Plugin.JobGauges.Get<DRKGauge>().HasDarkArts;

            var posX = origin.X + Config.Position.X + Config.ManaBarPosition.X - Config.ManaBarSize.X / 2f;
            var posY = origin.Y + Config.Position.Y + Config.ManaBarPosition.Y - Config.ManaBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(posX, posY, Config.ManaBarSize.Y, Config.ManaBarSize.X).SetBackgroundColor(EmptyColor.Background);

            if (Config.ChunkManaBar)
            {
                builder.SetChunks(3).SetChunkPadding(Config.ManaBarPadding).AddInnerBar(actor.CurrentMp, 9000, Config.ManaBarColor, PartialFillColor);
            }
            else
            {
                builder.AddInnerBar(actor.CurrentMp, actor.MaxMp, Config.ManaBarColor);
            }

            if (Config.ShowManaBarText)
            {
                var formattedManaText = TextTags.GenerateFormattedTextFromTags(actor, "[mana:current-short]");

                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterLeft, BarTextType.Custom, formattedManaText);
            }

            if (darkArtsBuff)
            {
                builder.SetGlowSize(2);
                builder.SetGlowColor(Config.DarkArtsColor.Base);
                builder.SetChunksColors(Config.DarkArtsColor);
                builder.SetPartialFillColor(Config.DarkArtsColor);
                builder.SetBackgroundColor(Config.DarkArtsColor.Background);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawDarkside(Vector2 origin)
        {
            var target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;
            if (target is not Character && Config.HideDarksideWhenNoTarget)
            {
                return;
            }

            float darksideTimer = Plugin.JobGauges.Get<DRKGauge>().DarksideTimeRemaining;
            float darksideDuration = Math.Abs(darksideTimer / 1000);
            var max = 60f;

            var posX = origin.X + Config.Position.X + Config.DarksidePosition.X - Config.DarksideSize.X / 2f;
            var posY = origin.Y + Config.Position.Y + Config.DarksidePosition.Y - Config.DarksideSize.Y / 2f;

            if (darksideDuration == 0 && Config.OnlyShowWhenActive)
            {
                return;
            }

            var darksideColor = darksideDuration > 5 ? Config.DarksideColor : Config.DarksideExpiryColor;
            BarBuilder builder = BarBuilder.Create(posX, posY, Config.DarksideSize.Y, Config.DarksideSize.X)
                .SetBackgroundColor(EmptyColor.Background)
                .AddInnerBar(darksideDuration, max, darksideColor);

            if (Config.ShowDarksideText)
            {
                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawBloodGauge(Vector2 origin)
        {
            var gauge = Plugin.JobGauges.Get<DRKGauge>();

            var posX = origin.X + Config.Position.X + Config.BloodGaugePosition.X - Config.BloodGaugeSize.X / 2f;
            var posY = origin.Y + Config.Position.Y + Config.BloodGaugePosition.Y - Config.BloodGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(posX, posY, Config.BloodGaugeSize.Y, Config.BloodGaugeSize.X).SetBackgroundColor(EmptyColor.Background);

            if (Config.ChunkBloodGauge)
            {
                builder.SetChunks(2).SetChunkPadding(Config.BloodGaugePadding).AddInnerBar(gauge.Blood, 100, Config.BloodColor, PartialFillColor);
            }
            else
            {
                if (gauge.Blood == 100)
                {
                    builder.AddInnerBar(gauge.Blood, 100, Config.BloodColorFull);
                }
                else if (gauge.Blood > 100)
                {
                    builder.AddInnerBar(gauge.Blood, 100, Config.BloodColor);
                }
                else
                {
                    builder.AddInnerBar(gauge.Blood, 100, PartialFillColor);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawBuffBar(Vector2 origin)
        {
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            IEnumerable<Status> bloodWeaponBuff = Plugin.ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 742);
            IEnumerable<Status> deliriumBuff = Plugin.ClientState.LocalPlayer.StatusList.Where(o => o.StatusId == 1972);

            var xPos = origin.X + Config.Position.X + Config.BuffBarPosition.X - Config.BuffBarSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.BuffBarPosition.Y - Config.BuffBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.BuffBarSize.Y, Config.BuffBarSize.X).SetBackgroundColor(EmptyColor.Background);

            if (bloodWeaponBuff.Any())
            {
                var fightOrFlightDuration = Math.Abs(bloodWeaponBuff.First().RemainingTime);
                builder.AddInnerBar(fightOrFlightDuration, 10, Config.BloodWeaponColor);

                if (Config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterLeft, BarTextType.Current, Config.BloodWeaponColor.Vector, Vector4.UnitW, null);
                }
            }

            if (deliriumBuff.Any())
            {
                var deliriumDuration = Math.Abs(deliriumBuff.First().RemainingTime);
                builder.AddInnerBar(deliriumDuration, 10, Config.DeliriumColor);

                if (Config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterRight, BarTextType.Current, Config.DeliriumColor.Vector, Vector4.UnitW, null);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }

        private void DrawLivingShadowBar(Vector2 origin)
        {
            var actor = Plugin.ClientState.LocalPlayer;
            if (actor is null)
            {
                return;
            }

            var shadowTimeRemaining = Plugin.JobGauges.Get<DRKGauge>().ShadowTimeRemaining / 1000;
            var livingShadow = actor.Level >= 80 && shadowTimeRemaining is > 0 and <= 24;

            var xPos = origin.X + Config.Position.X + Config.LivingShadowBarPosition.X - Config.LivingShadowBarSize.X / 2f;
            var yPos = origin.Y + Config.Position.Y + Config.LivingShadowBarPosition.Y - Config.LivingShadowBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, Config.LivingShadowBarSize.Y, Config.LivingShadowBarSize.X).SetBackgroundColor(EmptyColor.Background);

            if (livingShadow)
            {
                builder.AddInnerBar(shadowTimeRemaining, 24, Config.LivingShadowColor);

                if (Config.ShowLivingShadowBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterLeft, BarTextType.Current, Config.LivingShadowColor.Vector, Vector4.UnitW, null);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }

    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Dark Knight", 1)]
    public class DarkKnightConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.DRK;
        public new static DarkKnightConfig DefaultConfig() { return new DarkKnightConfig(); }

        #region Mana Bar
        [Checkbox("Mana", separator = true)]
        [CollapseControl(30, 0)]
        public bool ShowManaBar = true;

        [Checkbox("Text" + "##DRKManaBar")]
        [CollapseWith(0, 0)]
        public bool ShowManaBarText = false;

        [Checkbox("Split Bar")]
        [CollapseWith(5, 0)]
        public bool ChunkManaBar = true;

        [DragFloat2("Position" + "##DRKManaBar", min = -4000f, max = 4000f)]
        [CollapseWith(10, 0)]
        public Vector2 ManaBarPosition = new Vector2(0, -61);

        [DragFloat2("Size" + "##DRKManaBar", min = 0, max = 4000f)]
        [CollapseWith(15, 0)]
        public Vector2 ManaBarSize = new Vector2(254, 10);

        [DragInt("Spacing" + "##DRKManaBar", min = 0)]
        [CollapseWith(20, 0)]
        public int ManaBarPadding = 1;

        [CollapseWith(25, 0)]
        [ColorEdit4("Mana" + "##DRKManaBar")]
        public PluginConfigColor ManaBarColor = new(new Vector4(0f / 255f, 162f / 255f, 252f / 255f, 100f / 100f));

        [ColorEdit4("Dark Arts Proc" + "##DRKManaBar")]
        [CollapseWith(30, 0)]
        public PluginConfigColor DarkArtsColor = new(new Vector4(210f / 255f, 33f / 255f, 33f / 255f, 100f / 100f));
        #endregion

        #region Blood Gauge
        [Checkbox("Blood Gauge" + "##BloodGauge", separator = true)]
        [CollapseControl(35, 1)]
        public bool ShowBloodGauge = true;

        [Checkbox("Split Bar" + "##BloodGauge")]
        [CollapseWith(0, 1)]
        public bool ChunkBloodGauge = true;

        [DragFloat2("Position" + "##BloodGauge", min = -4000f, max = 4000f)]
        [CollapseWith(10, 1)]
        public Vector2 BloodGaugePosition = new Vector2(0, -49);

        [DragFloat2("Size" + "##BloodGauge", min = 0, max = 4000f)]
        [CollapseWith(15, 1)]
        public Vector2 BloodGaugeSize = new Vector2(254, 10);

        [DragInt("Spacing" + "##BloodGauge", min = 0)]
        [CollapseWith(20, 1)]
        public int BloodGaugePadding = 2;

        [ColorEdit4("Color Left" + "##BloodGauge")]
        [CollapseWith(25, 1)]
        public PluginConfigColor BloodColor = new(new Vector4(196f / 255f, 20f / 255f, 122f / 255f, 100f / 100f));

        [ColorEdit4("Color Filled" + "##BloodGauge")]
        [CollapseWith(30, 1)]
        public PluginConfigColor BloodColorFull = new(new Vector4(216f / 255f, 0f / 255f, 73f / 255f, 100f / 100f));
        #endregion

        #region Darkside
        [Checkbox("Darkside" + "##Darkside", separator = true)]
        [CollapseControl(40, 2)]
        public bool ShowDarkside = true;

        [Checkbox("Only Show When Active" + "##Darkside")]
        [CollapseWith(0, 2)]
        public bool OnlyShowWhenActive = true;

        [DragFloat2("Position" + "##Darkside", min = -4000f, max = 4000f)]
        [CollapseWith(5, 2)]
        public Vector2 DarksidePosition = new Vector2(0, -73);

        [DragFloat2("Size" + "##Darkside", min = 0, max = 4000f)]
        [CollapseWith(10, 2)]
        public Vector2 DarksideSize = new Vector2(254, 10);

        [ColorEdit4("Darkside" + "##Darkside")]
        [CollapseWith(15, 2)]
        public PluginConfigColor DarksideColor = new(new Vector4(209 / 255f, 38f / 255f, 204f / 255f, 100f / 100f));

        [ColorEdit4("Darkside Expiry" + "##Darkside")]
        [CollapseWith(20, 2)]
        public PluginConfigColor DarksideExpiryColor = new(new Vector4(160f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [Checkbox("Show Darkside Text" + "##Darkside")]
        [CollapseWith(25, 2)]
        public bool ShowDarksideText = true;

        [Checkbox("Hide When No Target" + "##Darkside")]
        [CollapseWith(30, 2)]
        public bool HideDarksideWhenNoTarget = false;
        #endregion

        #region Buff Bar
        [Checkbox("Blood Weapon & Delirium", separator = true)]
        [CollapseControl(45, 3)]
        public bool ShowBuffBar = false;

        [Checkbox("Timer" + "##DRKBuffBar")]
        [CollapseWith(0, 3)]
        public bool ShowBuffBarText = true;

        [DragFloat2("Position" + "##DRKBuffBar", min = -4000f, max = 4000f)]
        [CollapseWith(5, 3)]
        public Vector2 BuffBarPosition = new Vector2(0, -32);

        [DragFloat2("Size" + "##DRKBuffBar", min = 0, max = 4000f)]
        [CollapseWith(10, 3)]
        public Vector2 BuffBarSize = new Vector2(254, 20);

        [DragInt("Spacing" + "##DRKBuffBar", min = 0)]
        [CollapseWith(15, 3)]
        public int BuffBarPadding = 2;

        [ColorEdit4("Blood Weapon" + "##DRKBuffBar")]
        [CollapseWith(20, 3)]
        public PluginConfigColor BloodWeaponColor = new(new Vector4(160f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Delirium" + "##DRKBuffBar")]
        [CollapseWith(25, 3)]
        public PluginConfigColor DeliriumColor = new(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region Living Shadow
        [Checkbox("Living Shadow", separator = true)]
        [CollapseControl(50, 4)]
        public bool ShowLivingShadowBar = false;

        [Checkbox("Timer" + "##DRKLivingShadow")]
        [CollapseWith(0, 4)]
        public bool ShowLivingShadowBarText = true;

        [DragFloat2("Position" + "##DRKLivingShadow", min = -4000f, max = 4000f)]
        [CollapseWith(5, 4)]
        public Vector2 LivingShadowBarPosition = new Vector2(0, -10);

        [DragFloat2("Size" + "##DRKLivingShadow", min = 0, max = 4000f)]
        [CollapseWith(10, 4)]
        public Vector2 LivingShadowBarSize = new Vector2(254, 20);

        [DragInt("Spacing" + "##DRKLivingShadow", min = 0)]
        [CollapseWith(15, 4)]
        public int LivingShadowPadding = 2;

        [ColorEdit4("Color" + "##DRKLivingShadow")]
        [CollapseWith(20, 4)]
        public PluginConfigColor LivingShadowColor = new(new Vector4(225f / 255f, 105f / 255f, 205f / 255f, 100f / 100f));
        #endregion
    }
}
