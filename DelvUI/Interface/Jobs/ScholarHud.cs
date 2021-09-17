using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using DelvUI.Interface.GeneralElements;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface.Jobs
{
    public class ScholarHud : JobHud
    {
        private new ScholarConfig Config => (ScholarConfig)_config;

        public ScholarHud(string id, ScholarConfig config, string displayName = null) : base(id, config, displayName)
        {

        }

        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

            if (Config.ShowAether)
            {
                positions.Add(Config.Position + Config.AetherPosition);
                sizes.Add(Config.AetherSize);
            }

            if (Config.ShowFairy)
            {
                positions.Add(Config.Position + Config.FairyPosition);
                sizes.Add(Config.FairySize);
            }

            if (Config.ShowBio)
            {
                positions.Add(Config.Position + Config.BioPosition);
                sizes.Add(Config.BioSize);
            }

            return (positions, sizes);
        }

        public override void DrawChildren(Vector2 origin)
        {
            if (Config.ShowFairy)
            {
                DrawFairyBar(origin);
            }

            if (Config.ShowBio)
            {
                DrawBioBar(origin);
            }

            if (Config.ShowAether)
            {
                DrawAetherBar(origin);
            }
        }

        private void DrawFairyBar(Vector2 origin)
        {
            float fairyGauge = Plugin.JobGauges.Get<SCHGauge>().FairyGaugeAmount;
            float seraphDuration = Math.Abs(Plugin.JobGauges.Get<SCHGauge>().SeraphTimer / 1000);            

            Vector2 barSize = Config.FairySize;
            Vector2 position = origin + Config.Position + Config.FairyPosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            if (seraphDuration > 0)
            {
                builder.AddInnerBar(seraphDuration, 22f, Config.SeraphColor.Map)
                    .SetBackgroundColor(EmptyColor.Background);

                if (Config.ShowSeraphText)
                {
                    builder.SetTextMode(BarTextMode.Single)
                        .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }

                var drawList = ImGui.GetWindowDrawList();
                builder.Build().Draw(drawList);
            }
            else
            {
                builder.AddInnerBar(fairyGauge, 100f, Config.FairyColor.Map)
                    .SetBackgroundColor(EmptyColor.Background);

                if (Config.ShowFairyText)
                {
                    builder.SetTextMode(BarTextMode.Single)
                        .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
                }

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                builder.Build().Draw(drawList);
            }
        }

        private void DrawAetherBar(Vector2 origin)
        {
            StatusEffect aetherFlowBuff = Plugin.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 304);
            Vector2 barSize = Config.AetherSize;
            Vector2 position = origin + Config.Position + Config.AetherPosition - barSize / 2f;

            if (!Config.ShowAether)
            {
                return;
            }

            Bar bar = BarBuilder.Create(position, barSize)
                                .SetChunks(3)
                                .SetChunkPadding(Config.AetherPadding)
                                .AddInnerBar(aetherFlowBuff.StackCount, 3, Config.AetherColor.Map)
                                .SetBackgroundColor(EmptyColor.Background)
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawBioBar(Vector2 origin)
        {
            Actor target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.CurrentTarget;

            float bioDuration = 0;

            if (target is Chara)
            {
                StatusEffect bio = target.StatusEffects.FirstOrDefault(
                    o => o.EffectId == 179 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 189 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId
                      || o.EffectId == 1895 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId
                );

                bioDuration = Math.Abs(bio.Duration);
            }

            PluginConfigColor bioColor = bioDuration > 5 ? Config.BioColor : Config.ExpireColor;

            Vector2 barSize = Config.BioSize;
            Vector2 position = origin + Config.Position + Config.BioPosition - barSize / 2f;


            BarBuilder builder = BarBuilder.Create(position, barSize);

            Bar bioBar = builder.AddInnerBar(bioDuration, 30f, bioColor.Map)
                                .SetFlipDrainDirection(Config.BioInverted)
                                .Build();

            if (Config.ShowBioText && bioDuration != 0)
            {
                builder.SetTextMode(BarTextMode.Single)
                       .SetText(BarTextPosition.CenterMiddle, BarTextType.Current);
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bioBar.Draw(drawList);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("Scholar", 1)]
    public class ScholarConfig : JobConfig
    {
        [JsonIgnore] public override uint JobId => JobIDs.SCH;
        public new static ScholarConfig DefaultConfig()
        {
            var config = new ScholarConfig();
            config.UseDefaultPrimaryResourceBar = true;
            return config;
        }

        #region aether
        [Checkbox("Aether Tracker Enabled")]
        [CollapseControl(30, 1)]
        public bool ShowAether = true;

        [DragFloat2("Aether Tracker Size", min = 1f, max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 AetherSize = new(254, 20);

        [DragFloat2("Aether Tracker Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 AetherPosition = new(0, -76);

        [DragInt("Aether Padding", max = 1000)]
        [CollapseWith(10, 1)]
        public int AetherPadding = 2;

        [ColorEdit4("Aether Tracker Color")]
        [CollapseWith(15, 1)]
        public PluginConfigColor AetherColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region fairy
        [Checkbox("Fairy Gauge Enabled")]
        [CollapseControl(35, 2)]
        public bool ShowFairy = true;

        [Checkbox("Fairy Gauge Text")]
        [CollapseWith(0, 2)]
        public bool ShowFairyText = true;
               
        [Checkbox("Seraph Bar Enabled")]
        [CollapseWith(5, 2)]
        public bool ShowSeraph = true;

        [Checkbox("Seraph Bar Text")]
        [CollapseWith(10, 2)]
        public bool ShowSeraphText = true;

        [DragFloat2("Fairy Gauge Size", min = 1f, max = 2000f)]
        [CollapseWith(15, 2)]
        public Vector2 FairySize = new(254, 20);

        [DragFloat2("Fairy Gauge Position", min = -4000f, max = 4000f)]
        [CollapseWith(20, 2)]
        public Vector2 FairyPosition = new(0, -54);

        [ColorEdit4("Fairy Gauge Color")]
        [CollapseWith(25, 2)]
        public PluginConfigColor FairyColor = new(new Vector4(69f / 255f, 199 / 255f, 164f / 255f, 100f / 100f));

        [ColorEdit4("Seraph Bar Color")]
        [CollapseWith(30, 2)]
        public PluginConfigColor SeraphColor = new(new Vector4(232f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));        
        #endregion

        #region bio
        [Checkbox("Bio Enabled")]
        [CollapseControl(40, 3)]
        public bool ShowBio = true;

        [Checkbox("Bio Text")]
        [CollapseWith(0, 3)]
        public bool ShowBioText = true;

        [Checkbox("Bio Inverted")]
        [CollapseWith(5, 3)]
        public bool BioInverted = false;

        [DragFloat2("Bio Size", max = 2000f)]
        [CollapseWith(10, 3)]
        public Vector2 BioSize = new(254, 20);

        [DragFloat2("Bio Position", min = -4000f, max = 4000f)]
        [CollapseWith(15, 3)]
        public Vector2 BioPosition = new(0, -32);

        [ColorEdit4("Bio Color")]
        [CollapseWith(20, 3)]
        public PluginConfigColor BioColor = new(new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 1f));
        #endregion

        [ColorEdit4("DoT Expire Color")]
        [Order(45)]
        public PluginConfigColor ExpireColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));
    }
}
