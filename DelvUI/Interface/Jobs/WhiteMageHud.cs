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

        public WhiteMageHud(string id, WhiteMageConfig config) : base(id, config)
        {

        }

        public override void Draw(Vector2 origin)
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
            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            Vector2 cursorPos = origin + Config.Position + Config.DiaBarPosition - Config.DiaBarSize / 2f;

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (target is not Chara)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + Config.DiaBarSize, EmptyColor.Background);
                drawList.AddRect(cursorPos, cursorPos + Config.DiaBarSize, 0xFF000000);

                return;
            }

            StatusEffect dia = target.StatusEffects.FirstOrDefault(
                o => o.EffectId == 1871 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                  || o.EffectId == 144 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                  || o.EffectId == 143 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
            );

            float diaCooldown = dia.EffectId == 1871 ? 30f : 18f;
            float diaDuration = dia.Duration;

            drawList.AddRectFilled(cursorPos, cursorPos + Config.DiaBarSize, EmptyColor.Background);

            drawList.AddRectFilled(
                cursorPos,
                cursorPos + new Vector2(Config.DiaBarSize.X / diaCooldown * diaDuration, Config.DiaBarSize.Y),
                Config.DiaColor.Map["gradientRight"]
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
            WHMGauge gauge = PluginInterface.ClientState.JobGauges.Get<WHMGauge>();

            const float lilyCooldown = 30000f;

            float getScale(int num, float timer, float cooldown)
            {
                return num + (timer / lilyCooldown);
            }

            float lilyScale = getScale(gauge.NumLilies, gauge.LilyTimer, lilyCooldown);

            var posX = origin.X + Config.Position.X + Config.LilyBarPosition.X - Config.LilyBarSize.X / 2f;
            var posY = origin.Y + Config.Position.Y + Config.LilyBarPosition.Y - Config.LilyBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(posX, posY, Config.LilyBarSize.Y, Config.LilyBarSize.X).SetBackgroundColor(EmptyColor.Background);

            builder.SetChunks(3).SetChunkPadding(Config.LilyBarPad).AddInnerBar(lilyScale, 3, Config.LilyColor.Map, PartialFillColor.Map);

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

            builder.SetChunks(3).SetChunkPadding(Config.BloodLilyBarPad).AddInnerBar(gauge.NumBloodLily, 3, Config.BloodLilyColor.Map);

            drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("White Mage", 1)]
    public class WhiteMageConfig : JobConfig
    {
        [JsonIgnore] public new uint JobId = JobIDs.WHM;
        public new static WhiteMageConfig DefaultConfig() { return new WhiteMageConfig(); }

        #region Lily Bar
        [Checkbox("Show Lily Bars")]
        [CollapseControl(30, 0)]
        public bool ShowLilyBars = true;

        [Checkbox("Show Lily Bar Timer")]
        [CollapseWith(0, 0)]
        public bool ShowLilyBarTimer = true;

        [DragFloat2("Lily Bar Size", max = 2000f)]
        [CollapseWith(5, 0)]
        public Vector2 LilyBarSize = new(125, 20);

        [DragFloat2("Lily Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 0)]
        public Vector2 LilyBarPosition = new(-64, HUDConstants.JobHudsBaseY - 32);

        [DragInt("Lily Bar Padding", min = 0, max = 1000)]
        [CollapseWith(15, 0)]
        public int LilyBarPad = 2;

        [ColorEdit4("Lily Bar Color")]
        [CollapseWith(20, 0)]
        public PluginConfigColor LilyColor = new(new Vector4(0f / 255f, 64f / 255f, 1f, 1f));

        [ColorEdit4("Lily Bar Charging Color")]
        [CollapseWith(25, 0)]
        public PluginConfigColor LilyChargingColor = new(new Vector4(141f / 255f, 141f / 255f, 141f / 255f, 1f));
        #endregion

        #region Blood Lily Bar
        [DragFloat2("Blood Lily Bar Size", max = 2000f)]
        [CollapseWith(30, 0)]
        public Vector2 BloodLilyBarSize = new(125, 20);

        [DragFloat2("Blood Lily Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(35, 0)]
        public Vector2 BloodLilyBarPosition = new(64, HUDConstants.JobHudsBaseY - 32);

        [DragInt("Blood Lily Bar Padding", min = 0, max = 1000)]
        [CollapseWith(40, 0)]
        public int BloodLilyBarPad = 2;

        [ColorEdit4("Blood Lily Bar Color")]
        [CollapseWith(45, 0)]
        public PluginConfigColor BloodLilyColor = new(new Vector4(199f / 255f, 40f / 255f, 9f / 255f, 1f));
        #endregion

        #region Dia Bar
        [Checkbox("Show Dia Bar")]
        [CollapseControl(35, 1)]
        public bool ShowDiaBar = true;

        [DragFloat2("Dia Bar Size", max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 DiaBarSize = new(254, 20);

        [DragFloat2("Dia Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 DiaBarPosition = new(0, HUDConstants.JobHudsBaseY - 10);

        [ColorEdit4("Dia Bar Color")]
        [CollapseWith(10, 1)]
        public PluginConfigColor DiaColor = new(new Vector4(0f / 255f, 64f / 255f, 1f, 1f));
        #endregion
    }
}
