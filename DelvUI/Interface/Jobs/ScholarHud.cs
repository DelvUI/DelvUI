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
            float fairyGauge = Plugin.JobGauges.Get<SCHGauge>().FairyGauge;
            float seraphTimer = Plugin.JobGauges.Get<SCHGauge>().SeraphTimer;
            float seraphDuration = Math.Abs(seraphTimer / 1000);

            Vector2 barSize = Config.FairySize;
            Vector2 position = origin + Config.Position + Config.FairyPosition - barSize / 2f;

            BarBuilder builder = BarBuilder.Create(position, barSize);

            if (seraphDuration > 0)
            {
                builder.AddInnerBar(seraphDuration, 22f, Config.SeraphColor)
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
                builder.AddInnerBar(fairyGauge, 100f, Config.FairyColor)
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
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            var aetherFlowBuff = Plugin.ClientState.LocalPlayer.StatusList.FirstOrDefault(o => o.StatusId == 304);
            Vector2 barSize = Config.AetherSize;
            Vector2 position = origin + Config.Position + Config.AetherPosition - barSize / 2f;

            if (!Config.ShowAether)
            {
                return;
            }

            Bar bar = BarBuilder.Create(position, barSize)
                                .SetChunks(3)
                                .SetChunkPadding(Config.AetherPadding)
                                .AddInnerBar(aetherFlowBuff?.StackCount ?? 0, 3, Config.AetherColor)
                                .SetBackgroundColor(EmptyColor.Background)
                                .Build();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            bar.Draw(drawList);
        }

        private void DrawBioBar(Vector2 origin)
        {
            Debug.Assert(Plugin.ClientState.LocalPlayer != null, "Plugin.ClientState.LocalPlayer != null");
            var actor = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.Target;

            float bioDuration = 0;

            if (actor is BattleChara target)
            {
                var bio = target.StatusList.FirstOrDefault(
                    o => o.StatusId == 179 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                      || o.StatusId == 189 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                      || o.StatusId == 1895 && o.SourceID == Plugin.ClientState.LocalPlayer.ObjectId
                );

                bioDuration = Math.Abs(bio?.RemainingTime ?? 0f);
            }

            PluginConfigColor bioColor = bioDuration > 5 ? Config.BioColor : Config.ExpireColor;

            Vector2 barSize = Config.BioSize;
            Vector2 position = origin + Config.Position + Config.BioPosition - barSize / 2f;


            BarBuilder builder = BarBuilder.Create(position, barSize);

            Bar bioBar = builder.AddInnerBar(bioDuration, 30f, bioColor)
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
        [Checkbox("Aether" + "##Aether",separator = true)]
        [Order(30)]
        public bool ShowAether = true;
        
        [DragFloat2("Position" + "##Aether", min = -4000f, max = 4000f)]
        [Order(35, collapseWith = nameof(ShowAether))]
        public Vector2 AetherPosition = new(0, -76);
        
        [DragFloat2("Size" + "##Aether", min = 1f, max = 2000f)]
        [Order(40, collapseWith = nameof(ShowAether))]
        public Vector2 AetherSize = new(254, 20);

        [DragInt("Spacing" + "##Aether", max = 1000)]
        [Order(45, collapseWith = nameof(ShowAether))]
        public int AetherPadding = 2;

        [ColorEdit4("Color" + "##Aether")]
        [Order(50, collapseWith = nameof(ShowAether))]
        public PluginConfigColor AetherColor = new(new Vector4(0f / 255f, 255f / 255f, 0f / 255f, 100f / 100f));
        #endregion

        #region fairy
        [Checkbox("Fairy Gauge" + "##Fairy",separator = true)]
        [Order(55)]
        public bool ShowFairy = true;

        [Checkbox("Text" + "##Fairy")]
        [Order(60, collapseWith = nameof(ShowFairy))]
        public bool ShowFairyText = true;

        [DragFloat2("Position" + "##Fairy", min = -4000f, max = 4000f)]
        [Order(65, collapseWith = nameof(ShowFairy))]
        public Vector2 FairyPosition = new(0, -54);

        [DragFloat2("Size" + "##Fairy", min = 1f, max = 2000f)]
        [Order(70, collapseWith = nameof(ShowFairy))]
        public Vector2 FairySize = new(254, 20);

        [ColorEdit4("Color" + "##Fairy")]
        [Order(75, collapseWith = nameof(ShowFairy))]
        public PluginConfigColor FairyColor = new(new Vector4(69f / 255f, 199 / 255f, 164f / 255f, 100f / 100f));
        
        [Checkbox("Seraph" + "##Seraph", spacing = true)]
        [Order(80, collapseWith = nameof(ShowFairy))]
        public bool ShowSeraph = true;
        //TODO NOT ASSIGNED? ^
        
        [Checkbox("Timer" + "##Seraph")]
        [Order(85, collapseWith = nameof(ShowSeraph))]
        public bool ShowSeraphText = true;
        
        [ColorEdit4("Color" + "##SeraphColor")]
        [Order(90, collapseWith = nameof(ShowSeraph))]
        public PluginConfigColor SeraphColor = new(new Vector4(232f / 255f, 255f / 255f, 255f / 255f, 100f / 100f));
        #endregion

        #region bio
        [Checkbox("Bio" + "##Bio",separator = true)]
        [Order(95)]
        public bool ShowBio = true;

        [Checkbox("Timer" + "##Bio")]
        [Order(100, collapseWith = nameof(ShowBio))]
        public bool ShowBioText = true;

        [Checkbox("Invert Growth" + "##Bio")]
        [Order(105, collapseWith = nameof(ShowBio))]
        public bool BioInverted = false;

        [DragFloat2("Position" + "##Bio", min = -4000f, max = 4000f)]
        [Order(110, collapseWith = nameof(ShowBio))]
        public Vector2 BioPosition = new(0, -32);
        
        [DragFloat2("Size" + "##Bio", max = 2000f)]
        [Order(115, collapseWith = nameof(ShowBio))]
        public Vector2 BioSize = new(254, 20);
        
        [ColorEdit4("Color" + "##Bio")]
        [Order(120, collapseWith = nameof(ShowBio))]
        public PluginConfigColor BioColor = new(new Vector4(50f / 255f, 93f / 255f, 37f / 255f, 1f));
        
        [ColorEdit4("Expire Color" + "##Bio")]
        [Order(125, collapseWith = nameof(ShowBio))]
        public PluginConfigColor ExpireColor = new(new Vector4(230f / 255f, 33f / 255f, 33f / 255f, 53f / 100f));
        #endregion

        
    }
}
