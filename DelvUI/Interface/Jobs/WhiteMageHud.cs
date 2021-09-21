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
using System.Globalization;
using System.Linq;
using System.Numerics;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface.Jobs
{
    public class WhiteMageHud : JobHud
    {
        private new WhiteMageConfig Config => (WhiteMageConfig)_config;
        private PluginConfigColor EmptyColor => GlobalColors.Instance.EmptyColor;
        private PluginConfigColor PartialFillColor => GlobalColors.Instance.PartialFillColor;

        public WhiteMageHud(string id, WhiteMageConfig config, string displayName = null) : base(id, config, displayName)
        {

        }

        protected override (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            List<Vector2> positions = new List<Vector2>();
            List<Vector2> sizes = new List<Vector2>();

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

        public override void DrawChildren(Vector2 origin)
        {
            if (Config.ShowLilyBars)
            {
                DrawLilyBars(origin);
            }

            if (Config.ShowDiaBar)
            {
                DrawDiaBar(origin);
            }
        }

        private void DrawDiaBar(Vector2 origin)
        {
            Actor target = Plugin.TargetManager.SoftTarget ?? Plugin.TargetManager.CurrentTarget;
            Vector2 cursorPos = origin + Config.Position + Config.DiaBarPosition - Config.DiaBarSize / 2f;

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (target is not Chara)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + Config.DiaBarSize, EmptyColor.Background);
                drawList.AddRect(cursorPos, cursorPos + Config.DiaBarSize, 0xFF000000);

                return;
            }

            StatusEffect dia = target.StatusEffects.FirstOrDefault(
                o => o.EffectId == 1871 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId
                  || o.EffectId == 144 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId
                  || o.EffectId == 143 && o.OwnerId == Plugin.ClientState.LocalPlayer.ActorId
            );

            float diaCooldown = dia.EffectId == 1871 ? 30f : 18f;
            float diaDuration = dia.Duration;

            drawList.AddRectFilled(cursorPos, cursorPos + Config.DiaBarSize, EmptyColor.Background);

            drawList.AddRectFilled(
                cursorPos,
                cursorPos + new Vector2(Config.DiaBarSize.X / diaCooldown * diaDuration, Config.DiaBarSize.Y),
                Config.DiaColor.BottomGradient
            );

            drawList.AddRect(cursorPos, cursorPos + Config.DiaBarSize, 0xFF000000);

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

            float getScale(int num, float timer, float cooldown)
            {
                return num + (timer / lilyCooldown);
            }

            float lilyScale = getScale(gauge.NumLilies, gauge.LilyTimer, lilyCooldown);

            var posX = origin.X + Config.Position.X + Config.LilyBarPosition.X - Config.LilyBarSize.X / 2f;
            var posY = origin.Y + Config.Position.Y + Config.LilyBarPosition.Y - Config.LilyBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(posX, posY, Config.LilyBarSize.Y, Config.LilyBarSize.X).SetBackgroundColor(EmptyColor.Background);

            builder.SetChunks(3).SetChunkPadding(Config.LilyBarPad).AddInnerBar(lilyScale, 3, Config.LilyColor, PartialFillColor);

            if (Config.ShowLilyBarTimer)
            {
                string timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                Vector2 size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                float lilyChunkSize = (Config.LilyBarSize.X / 3f) + Config.LilyBarPad;
                float lilyChunkOffset = lilyChunkSize * ((int)gauge.NumLilies + 1);

                if (gauge.NumLilies < 3)
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

            builder.SetChunks(3).SetChunkPadding(Config.BloodLilyBarPad).AddInnerBar(gauge.NumBloodLily, 3, Config.BloodLilyColor);

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
        [CollapseControl(30, 0)]
        public bool ShowLilyBars = true;

        [Checkbox("Timer" + "##Lily")]
        [CollapseWith(0, 0)]
        public bool ShowLilyBarTimer = true;
        
        [DragFloat2("Position" + "##Lily", min = -4000f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 LilyBarPosition = new(-64, -54);
        
        [DragFloat2("Size" + "##Lily", max = 2000f)]
        [CollapseWith(10, 0)]
        public Vector2 LilyBarSize = new(125, 20);

        [DragInt("Spacing" + "##Lily", min = 0, max = 1000)]
        [CollapseWith(15, 0)]
        public int LilyBarPad = 2;

        [ColorEdit4("Color" + "##Lily")]
        [CollapseWith(20, 0)]
        public PluginConfigColor LilyColor = new(new Vector4(0f / 255f, 64f / 255f, 1f, 1f));

        [ColorEdit4("Charging Color" + "##Lily")] //TODO CHANGE TO GLOBAL PARTIALLY FILLED COLOR?
        [CollapseWith(25, 0)]
        public PluginConfigColor LilyChargingColor = new(new Vector4(141f / 255f, 141f / 255f, 141f / 255f, 1f));
        #endregion

        #region Blood Lily Bar
        
        [DragFloat2("Position" + "##BloodLily", min = -4000f, max = 4000f, spacing = true)]
        [CollapseWith(30, 0)]
        public Vector2 BloodLilyBarPosition = new(64, -54);
        
        [DragFloat2("Size" + "##BloodLily", max = 2000f)]
        [CollapseWith(35, 0)]
        public Vector2 BloodLilyBarSize = new(125, 20);

        [DragInt("Spacing" + "##BloodLily", min = 0, max = 1000)]
        [CollapseWith(40, 0)]
        public int BloodLilyBarPad = 2;

        [ColorEdit4("Color" + "##BloodLily")]
        [CollapseWith(45, 0)]
        public PluginConfigColor BloodLilyColor = new(new Vector4(199f / 255f, 40f / 255f, 9f / 255f, 1f));
        #endregion

        #region Dia Bar
        [Checkbox("Dia", separator = true)]
        [CollapseControl(35, 1)]
        public bool ShowDiaBar = true;

        [DragFloat2("Size " + "##Dia", max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 DiaBarSize = new(254, 20);

        [DragFloat2("Position" + "##Dia", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 DiaBarPosition = new(0, -32);

        [ColorEdit4("Color" + "##Dia")]
        [CollapseWith(10, 1)]
        public PluginConfigColor DiaColor = new(new Vector4(0f / 255f, 64f / 255f, 1f, 1f));
        #endregion
    }
}
