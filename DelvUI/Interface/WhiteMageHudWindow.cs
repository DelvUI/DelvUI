using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using DelvUI.Config;
using DelvUI.Config.Attributes;
using DelvUI.Helpers;
using DelvUI.Interface.Bars;
using ImGuiNET;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public class WhiteMageHudWindow : HudWindow
    {
        public WhiteMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 24;
        private WhiteMageHudConfig _config => (WhiteMageHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new WhiteMageHudConfig());

        private Vector2 Origin => new Vector2(CenterX + _config.BasePosition.X, CenterY + _config.BasePosition.Y);
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];
        private Dictionary<string, uint> PartialFillColor => PluginConfiguration.MiscColorMap["partial"];

        protected override void Draw(bool _)
        {
            if (_config.ShowLilyBars)
            {
                DrawLilyBars();
            }

            if (_config.ShowDiaBar)
            {
                DrawDiaBar();
            }
        }

        protected override void DrawPrimaryResourceBar()
        {
            if (!_config.ShowPrimaryResourceBar)
            {
                return;
            }

            base.DrawPrimaryResourceBar();
        }

        private void DrawDiaBar()
        {
            Actor target = PluginInterface.ClientState.Targets.SoftTarget ?? PluginInterface.ClientState.Targets.CurrentTarget;
            Vector2 barCoords = _config.BasePosition + _config.DiaBarPosition - _config.DiaBarSize / 2f;
            Vector2 cursorPos = new(CenterX + barCoords.X, CenterY + barCoords.Y);

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (target is not Chara)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + _config.DiaBarSize, EmptyColor["background"]);
                drawList.AddRect(cursorPos, cursorPos + _config.DiaBarSize, 0xFF000000);

                return;
            }

            StatusEffect dia = target.StatusEffects.FirstOrDefault(
                o => o.EffectId == 1871 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                  || o.EffectId == 144 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
                  || o.EffectId == 143 && o.OwnerId == PluginInterface.ClientState.LocalPlayer.ActorId
            );

            float diaCooldown = dia.EffectId == 1871 ? 30f : 18f;
            float diaDuration = dia.Duration;

            drawList.AddRectFilled(cursorPos, cursorPos + _config.DiaBarSize, EmptyColor["background"]);

            drawList.AddRectFilled(
                cursorPos,
                cursorPos + new Vector2(_config.DiaBarSize.X / diaCooldown * diaDuration, _config.DiaBarSize.Y),
                _config.DiaColor.Map["gradientRight"]
            );

            drawList.AddRect(cursorPos, cursorPos + _config.DiaBarSize, 0xFF000000);

            DrawOutlinedText(
                string.Format(CultureInfo.InvariantCulture, "{0,2:N0}", diaDuration), // keeps 10 -> 9 from jumping
                new Vector2(
                    // smooths transition of counter to the right of the emptying bar
                    cursorPos.X
                  + _config.DiaBarSize.X * diaDuration / diaCooldown
                  - (Math.Abs(diaDuration - diaCooldown) < float.Epsilon
                        ? diaCooldown
                        : diaDuration > 3
                            ? 20
                            : diaDuration * (20f / 3f)),
                    cursorPos.Y + _config.DiaBarSize.Y / 2 - 12
                )
            );
        }

        private void DrawLilyBars()
        {
            WHMGauge gauge = PluginInterface.ClientState.JobGauges.Get<WHMGauge>();

            const float lilyCooldown = 30000f;

            float getScale(int num, float timer, float cooldown)
            {
                return num + (timer / lilyCooldown);
            }

            float lilyScale = getScale(gauge.NumLilies, gauge.LilyTimer, lilyCooldown);

            var posX = Origin.X + _config.LilyBarPosition.X - _config.LilyBarSize.X / 2f;
            var posY = Origin.Y + _config.LilyBarPosition.Y - _config.LilyBarSize.Y / 2f;

            BarBuilder builder = BarBuilder.Create(posX, posY, _config.LilyBarSize.Y, _config.LilyBarSize.X).SetBackgroundColor(EmptyColor["background"]);

            builder.SetChunks(3).SetChunkPadding(_config.LilyBarPad).AddInnerBar(lilyScale, 3, _config.LilyColor.Map, PartialFillColor);

            if (_config.ShowLilyBarTimer)
            {
                string timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                Vector2 size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                float lilyChunkSize = (_config.LilyBarSize.X / 3f) + _config.LilyBarPad;
                float lilyChunkOffset = lilyChunkSize * ((int)gauge.NumLilies + 1);

                if (gauge.NumLilies < 3)
                {
                    DrawOutlinedText(timer, new Vector2(
                        posX + lilyChunkOffset - (lilyChunkSize / 2f) - (size.X / 2f),
                        posY - _config.LilyBarSize.Y - 4f));
                }
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);

            posX = Origin.X + _config.BloodLilyBarPosition.X - _config.BloodLilyBarSize.X / 2f;
            posY = Origin.Y + _config.BloodLilyBarPosition.Y - _config.BloodLilyBarSize.Y / 2f;

            builder = BarBuilder.Create(posX, posY, _config.BloodLilyBarSize.Y, _config.BloodLilyBarSize.X).SetBackgroundColor(EmptyColor["background"]);

            builder.SetChunks(3).SetChunkPadding(_config.BloodLilyBarPad).AddInnerBar(gauge.NumBloodLily, 3, _config.BloodLilyColor.Map);

            drawList = ImGui.GetWindowDrawList();
            builder.Build().Draw(drawList, PluginConfiguration);
        }
    }

    [Serializable]
    [Section("Job Specific Bars")]
    [SubSection("Healer", 0)]
    [SubSection("White Mage", 1)]
    public class WhiteMageHudConfig : PluginConfigObject
    {
        [DragFloat2("Base Position", min = -4000f, max = 4000f)]
        [Order(0)]
        public Vector2 BasePosition = new(0, 0);

        [Checkbox("Show Primary Resource Bar")]
        [Order(5)]
        public bool ShowPrimaryResourceBar = true;

        #region Lily Bar

        [Checkbox("Show Lily Bars")]
        [CollapseControl(10, 0)]
        public bool ShowLilyBars = true;

        [Checkbox("Show Lily Bar Timer")]
        [CollapseWith(0, 0)]
        public bool ShowLilyBarTimer = true;

        [DragFloat2("Lily Bar Size", max = 2000f)]
        [CollapseWith(5, 0)]
        public Vector2 LilyBarSize = new(125, 20);

        [DragFloat2("Lily Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(10, 0)]
        public Vector2 LilyBarPosition = new(-64, 405);

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
        public Vector2 BloodLilyBarPosition = new(64, 405);

        [DragInt("Blood Lily Bar Padding", min = 0, max = 1000)]
        [CollapseWith(40, 0)]
        public int BloodLilyBarPad = 2;

        [ColorEdit4("Blood Lily Bar Color")]
        [CollapseWith(45, 0)]
        public PluginConfigColor BloodLilyColor = new(new Vector4(199f / 255f, 40f / 255f, 9f / 255f, 1f));

        #endregion

        #region Dia Bar

        [Checkbox("Show Dia Bar")]
        [CollapseControl(15, 1)]
        public bool ShowDiaBar = true;

        [DragFloat2("Dia Bar Size", max = 2000f)]
        [CollapseWith(0, 1)]
        public Vector2 DiaBarSize = new(254, 20);

        [DragFloat2("Dia Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 1)]
        public Vector2 DiaBarPosition = new(0, 427);

        [ColorEdit4("Dia Bar Color")]
        [CollapseWith(10, 1)]
        public PluginConfigColor DiaColor = new(new Vector4(0f / 255f, 64f / 255f, 1f, 1f));

        #endregion
    }
}
