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

namespace DelvUI.Interface.Jobs
{
    public class WhiteMageHud : JobHud
    {
        private new WhiteMageConfig Config => (WhiteMageConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;
        private PluginConfigColor PartialFillColor => GlobalColors.Instance.PartialFillColor;

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
            if (Config.ShowLilyBars)
            {
                DrawLilyBars(origin);
            }

            if (Config.ShowDiaBar)
            {
                DrawDiaBar(origin, player);
            }
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

            var dia = target.StatusList.Where(
                o => o.StatusId is 1871 or 144 or 143 && o.SourceID == player.ObjectId
            );

            float diaCooldown = 0f;
            float diaDuration = 0f;

            if (dia.Any())
            {
                diaCooldown = dia.First().StatusId == 1871 ? 30f : 18f;
                diaDuration = Math.Abs(dia.First().RemainingTime);
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
                )
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
                        posY - Config.LilyBarSize.Y - 4f));
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);

            posX = origin.X + Config.Position.X + Config.BloodLilyBarPosition.X - Config.BloodLilyBarSize.X / 2f;
            posY = origin.Y + Config.Position.Y + Config.BloodLilyBarPosition.Y - Config.BloodLilyBarSize.Y / 2f;

            builder = BarBuilder.Create(posX, posY, Config.BloodLilyBarSize.Y, Config.BloodLilyBarSize.X).SetBackgroundColor(EmptyColor.Background);

            builder.SetChunks(3).SetChunkPadding(Config.BloodLilyBarPad).AddInnerBar(gauge.BloodLily, 3, Config.BloodLilyColor);

            drawList = ImGui.GetWindowDrawList();
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
        public bool ShowLilyBarTimer = true;

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
    }
}
