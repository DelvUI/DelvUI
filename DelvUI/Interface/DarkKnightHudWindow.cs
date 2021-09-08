using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using DelvUI.Config.Attributes;

namespace DelvUI.Interface
{
    public class DarkKnightHudWindow : HudWindow
    {
        public override uint JobId => Jobs.DRK;

        private DarkKnightHudConfig _config => (DarkKnightHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new DarkKnightHudConfig());
        private Vector2 Origin => new Vector2(CenterX + _config.Position.X, CenterY + _config.Position.Y);

        private PluginConfigColor EmptyColor;
        private PluginConfigColor PartialFillColor;

        public DarkKnightHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration)
        {
            EmptyColor = new PluginConfigColor(PluginConfiguration.EmptyColor);
            PartialFillColor = new PluginConfigColor(PluginConfiguration.PartialFillColor);
        }

        protected override void Draw(bool _)
        {
            if (_config.ShowManaBar)
            {
                DrawManaBar();
            }

            if (_config.ShowBloodGauge)
            {
                DrawBloodGauge();
            }

            if (_config.ShowBuffBar)
            {
                DrawBuffBar();
            }

            if (_config.ShowLivingShadowBar)
            {
                DrawLivingShadowBar();
            }
        }

        protected override void DrawPrimaryResourceBar() { }

        private void DrawManaBar()
        {
            Debug.Assert(PluginInterface.ClientState.LocalPlayer != null, "PluginInterface.ClientState.LocalPlayer != null");
            var actor = PluginInterface.ClientState.LocalPlayer;
            var darkArtsBuff = PluginInterface.ClientState.JobGauges.Get<DRKGauge>().HasDarkArts();

            var posX = Origin.X + _config.ManaBarPosition.X - _config.ManaBarSize.X / 2f;
            var posY = Origin.Y + _config.ManaBarPosition.Y - _config.ManaBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(posX, posY, _config.ManaBarSize.Y, _config.ManaBarSize.X).SetBackgroundColor(EmptyColor.Background);

            if (_config.ChunkManaBar)
            {
                builder.SetChunks(3).SetChunkPadding(_config.ManaBarPadding).AddInnerBar(actor.CurrentMp, 9000, _config.ManaBarColor.Map, PartialFillColor.Map);
            }
            else
            {
                builder.AddInnerBar(actor.CurrentMp, actor.MaxMp, _config.ManaBarColor.Map);
            }

            if (_config.ShowManaBarText)
            {
                var formattedManaText = TextTags.GenerateFormattedTextFromTags(actor, "[mana:current-short]");

                builder.SetTextMode(BarTextMode.Single).SetText(BarTextPosition.CenterLeft, BarTextType.Custom, formattedManaText);
            }

            if (darkArtsBuff)
            {
                builder.SetGlowSize(2);
                builder.SetGlowColor(_config.DarkArtsColor.Base);
                builder.SetChunksColors(_config.DarkArtsColor.Map);
                builder.SetPartialFillColor(_config.DarkArtsColor.Map);
                builder.SetBackgroundColor(_config.DarkArtsColor.Background);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBloodGauge()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<DRKGauge>();

            var posX = Origin.X + _config.BloodGaugePosition.X - _config.BloodGaugeSize.X / 2f;
            var posY = Origin.Y + _config.BloodGaugePosition.Y - _config.BloodGaugeSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(posX, posY, _config.BloodGaugeSize.Y, _config.BloodGaugeSize.X).SetBackgroundColor(EmptyColor.Background);

            if (_config.ChunkBloodGauge)
            {
                builder.SetChunks(2).SetChunkPadding(_config.BloodGaugePadding).AddInnerBar(gauge.Blood, 100, _config.BloodColor.Map, PartialFillColor.Map);
            }
            else
            {
                if (gauge.Blood == 100)
                {
                    builder.AddInnerBar(gauge.Blood, 100, _config.BloodColorFull.Map);
                }
                else if (gauge.Blood > 100)
                {
                    builder.AddInnerBar(gauge.Blood, 100, _config.BloodColor.Map);
                }
                else
                {
                    builder.AddInnerBar(gauge.Blood, 100, PartialFillColor.Map);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawBuffBar()
        {
            IEnumerable<StatusEffect> bloodWeaponBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 742);
            IEnumerable<StatusEffect> deliriumBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.Where(o => o.EffectId == 1972);

            var xPos = Origin.X + _config.BuffBarPosition.X - _config.BuffBarSize.X / 2f;
            var yPos = Origin.Y + _config.BuffBarPosition.Y - _config.BuffBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.BuffBarSize.Y, _config.BuffBarSize.X).SetBackgroundColor(EmptyColor.Background);

            if (bloodWeaponBuff.Any())
            {
                var fightOrFlightDuration = Math.Abs(bloodWeaponBuff.First().Duration);
                builder.AddInnerBar(fightOrFlightDuration, 10, _config.BloodWeaponColor.Map);

                if (_config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterLeft, BarTextType.Current, _config.BloodWeaponColor.Vector, Vector4.UnitW, null);
                }
            }

            if (deliriumBuff.Any())
            {
                var deliriumDuration = Math.Abs(deliriumBuff.First().Duration);
                builder.AddInnerBar(deliriumDuration, 10, _config.DeliriumColor.Map);

                if (_config.ShowBuffBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterRight, BarTextType.Current, _config.DeliriumColor.Vector, Vector4.UnitW, null);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }

        private void DrawLivingShadowBar()
        {
            var actor = PluginInterface.ClientState.LocalPlayer;
            var shadowTimeRemaining = PluginInterface.ClientState.JobGauges.Get<DRKGauge>().ShadowTimeRemaining / 1000;
            var livingShadow = actor.Level >= 80 && shadowTimeRemaining is > 0 and <= 24;

            var xPos = Origin.X + _config.LivingShadowBarPosition.X - _config.LivingShadowBarSize.X / 2f;
            var yPos = Origin.Y + _config.LivingShadowBarPosition.Y - _config.LivingShadowBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(xPos, yPos, _config.LivingShadowBarSize.Y, _config.LivingShadowBarSize.X).SetBackgroundColor(EmptyColor.Background);

            if (livingShadow)
            {
                builder.AddInnerBar(shadowTimeRemaining, 24, _config.LivingShadowColor.Map);

                if (_config.ShowLivingShadowBarText)
                {
                    builder.SetTextMode(BarTextMode.EachChunk).SetText(BarTextPosition.CenterLeft, BarTextType.Current, _config.LivingShadowColor.Vector, Vector4.UnitW, null);
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Tank", 0)]
    [SubSection("Dark Knight", 1)]
    public class DarkKnightHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Position" + "##DarkKnight", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 Position = new Vector2(0, 0);

        #region Mana Bar
        [Checkbox("Show Mana Bar")]
        [CollapseControl(5, 0)]
        public bool ShowManaBar = true;

        [Checkbox("Show Text" + "##DRKManaBar")]
        [CollapseWith(0, 0)]
        public bool ShowManaBarText = false;

        [Checkbox("Chunk Mana Bar")]
        [CollapseWith(5, 0)]
        public bool ChunkManaBar = true;

        [DragFloat2("Position" + "##DRKManaBar", min = -4000f, max = 4000f)]
        [CollapseWith(10, 0)]
        public Vector2 ManaBarPosition = new Vector2(0, 442);

        [DragFloat2("Size" + "##DRKManaBar", min = 0, max = 4000f)]
        [CollapseWith(15, 0)]
        public Vector2 ManaBarSize = new Vector2(254, 10);

        [DragInt("Padding" + "##DRKManaBar", min = 0)]
        [CollapseWith(20, 0)]
        public int ManaBarPadding = 1;

        [CollapseWith(25, 0)]
        [ColorEdit4("Mana Color" + "##DRKManaBar")]
        public PluginConfigColor ManaBarColor = new(new Vector4(0f / 255f, 142f / 255f, 254f / 255f, 100f / 100f));

        [ColorEdit4("Dark Arts Buff Color" + "##DRKManaBar")]
        [CollapseWith(30, 0)]
        public PluginConfigColor DarkArtsColor = new(new Vector4(210f / 255f, 33f / 255f, 33f / 255f, 100f / 100f));
        #endregion

        #region Blood Gauge
        [Checkbox("Show Blood Gauge")]
        [CollapseControl(10, 1)]
        public bool ShowBloodGauge = true;

        [Checkbox("Chunk Blood Gauge")]
        [CollapseWith(0, 1)]
        public bool ChunkBloodGauge = true;

        [DragFloat2("Position" + "##DRKBloodGauge", min = -4000f, max = 4000f)]
        [CollapseWith(10, 1)]
        public Vector2 BloodGaugePosition = new Vector2(0, 454);

        [DragFloat2("Size" + "##DRKBloodGauge", min = 0, max = 4000f)]
        [CollapseWith(15, 1)]
        public Vector2 BloodGaugeSize = new Vector2(254, 10);

        [DragInt("Padding" + "##DRKBloodGauge", min = 0)]
        [CollapseWith(20, 1)]
        public int BloodGaugePadding = 2;

        [ColorEdit4("Blood Color Left" + "##DRKBloodGauge")]
        [CollapseWith(25, 1)]
        public PluginConfigColor BloodColor = new(new Vector4(196f / 255f, 20f / 255f, 122f / 255f, 100f / 100f));

        [ColorEdit4("Blood Color Full" + "##DRKBloodGauge")]
        [CollapseWith(30, 1)]
        public PluginConfigColor BloodColorFull = new(new Vector4(216f / 255f, 0f / 255f, 73f / 255f, 100f / 100f));
        #endregion

        #region Buff Bar
        [Checkbox("Show Buff Bar")]
        [CollapseControl(15, 2)]
        public bool ShowBuffBar = false;

        [Checkbox("Show Text" + "##DRKBuffBar")]
        [CollapseWith(0, 2)]
        public bool ShowBuffBarText = true;

        [DragFloat2("Position" + "##DRKBuffBar", min = -4000f, max = 4000f)]
        [CollapseWith(5, 2)]
        public Vector2 BuffBarPosition = new Vector2(0, 471);

        [DragFloat2("Size" + "##DRKBuffBar", min = 0, max = 4000f)]
        [CollapseWith(10, 2)]
        public Vector2 BuffBarSize = new Vector2(254, 20);

        [DragInt("Padding" + "##DRKBuffBar", min = 0)]
        [CollapseWith(15, 2)]
        public int BuffBarPadding = 2;

        [ColorEdit4("Blood Weapon Color" + "##DRKBuffBar")]
        [CollapseWith(20, 2)]
        public PluginConfigColor BloodWeaponColor = new(new Vector4(160f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));

        [ColorEdit4("Delirium Color" + "##DRKBuffBar")]
        [CollapseWith(25, 2)]
        public PluginConfigColor DeliriumColor = new(new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region Living Shadow
        [Checkbox("Show Living Shadow Bar")]
        [CollapseControl(20, 3)]
        public bool ShowLivingShadowBar = false;

        [Checkbox("Show Text" + "##DRKLivingShadow")]
        [CollapseWith(0, 3)]
        public bool ShowLivingShadowBarText = true;

        [DragFloat2("Position" + "##DRKLivingShadow", min = -4000f, max = 4000f)]
        [CollapseWith(5, 3)]
        public Vector2 LivingShadowBarPosition = new Vector2(0, 493);

        [DragFloat2("Size" + "##DRKLivingShadow", min = 0, max = 4000f)]
        [CollapseWith(10, 3)]
        public Vector2 LivingShadowBarSize = new Vector2(254, 20);

        [DragInt("Padding" + "##DRKLivingShadow", min = 0)]
        [CollapseWith(15, 3)]
        public int LivingShadowPadding = 2;

        [ColorEdit4("Color" + "##DRKLivingShadow")]
        [CollapseWith(20, 3)]
        public PluginConfigColor LivingShadowColor = new(new Vector4(225f / 255f, 105f / 255f, 205f / 255f, 100f / 100f));
        #endregion
    }
}
