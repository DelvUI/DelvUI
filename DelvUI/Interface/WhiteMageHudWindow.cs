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
using ImGuiNET;
using Actor = Dalamud.Game.ClientState.Actors.Types.Actor;

namespace DelvUI.Interface
{
    public class WhiteMageHudWindow : HudWindow
    {
        public WhiteMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        public override uint JobId => 24;
        private WhiteMageHudConfig _config => (WhiteMageHudConfig)ConfigurationManager.GetInstance().GetConfiguration(new WhiteMageHudConfig());
        private Dictionary<string, uint> EmptyColor => PluginConfiguration.MiscColorMap["empty"];

        protected override void Draw(bool _)
        {
            if (_config.ShowLilyBar)
            {
                DrawSecondaryResourceBar();
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

        private void DrawSecondaryResourceBar()
        {
            WHMGauge gauge = PluginInterface.ClientState.JobGauges.Get<WHMGauge>();

            const int numChunks = 3;
            float barWidth = (_config.LilyBarSize.X - _config.LilyBarPad * numChunks) / numChunks;
            Vector2 barSize = new(barWidth, _config.LilyBarSize.Y);

            Vector2 barCoords = _config.BasePosition + _config.LilyBarPosition;
            float xPos = CenterX - barCoords.X;
            float yPos = CenterY + barCoords.Y;

            const float lilyCooldown = 30000f;
            float scale = gauge.NumLilies == 0 ? gauge.LilyTimer / lilyCooldown : 1;
            Vector2 cursorPos = new(xPos, yPos);
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            if (gauge.NumLilies >= 1)
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + new Vector2(barWidth * scale, _config.LilyBarSize.Y),
                    _config.LilyColor.Map["gradientLeft"],
                    _config.LilyColor.Map["gradientRight"],
                    _config.LilyColor.Map["gradientRight"],
                    _config.LilyColor.Map["gradientLeft"]
                );
            }
            else
            {
                drawList.AddRectFilledMultiColor(
                    cursorPos,
                    cursorPos + new Vector2(barWidth * scale, _config.LilyBarSize.Y),
                    _config.LilyChargingColor.Map["gradientLeft"],
                    _config.LilyChargingColor.Map["gradientRight"],
                    _config.LilyChargingColor.Map["gradientRight"],
                    _config.LilyChargingColor.Map["gradientLeft"]
                );
            }

            if (scale < 1)
            {
                string timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                Vector2 size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                DrawOutlinedText(timer, new Vector2(cursorPos.X + barWidth / 2f - size.X / 2f, cursorPos.Y - 23));
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + _config.LilyBarPad + barWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            if (gauge.NumLilies > 0)
            {
                scale = gauge.NumLilies == 1 ? gauge.LilyTimer / lilyCooldown : 1;

                if (gauge.NumLilies >= 2)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + new Vector2(barWidth * scale, _config.LilyBarSize.Y),
                        _config.LilyColor.Map["gradientLeft"],
                        _config.LilyColor.Map["gradientRight"],
                        _config.LilyColor.Map["gradientRight"],
                        _config.LilyColor.Map["gradientLeft"]
                    );
                }
                else
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + new Vector2(barWidth * scale, _config.LilyBarSize.Y),
                        _config.LilyChargingColor.Map["gradientLeft"],
                        _config.LilyChargingColor.Map["gradientRight"],
                        _config.LilyChargingColor.Map["gradientRight"],
                        _config.LilyChargingColor.Map["gradientLeft"]
                    );
                }

                if (scale < 1)
                {
                    string timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                    Vector2 size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                    DrawOutlinedText(timer, new Vector2(cursorPos.X + barWidth / 2f - size.X / 2f, cursorPos.Y - 23));
                }
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + _config.LilyBarPad + barWidth, cursorPos.Y);
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            if (gauge.NumLilies > 1)
            {
                scale = gauge.NumLilies == 2 ? gauge.LilyTimer / lilyCooldown : 1;

                if (gauge.NumLilies == 3)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + new Vector2(barWidth * scale, _config.LilyBarSize.Y),
                        _config.LilyColor.Map["gradientLeft"],
                        _config.LilyColor.Map["gradientRight"],
                        _config.LilyColor.Map["gradientRight"],
                        _config.LilyColor.Map["gradientLeft"]
                    );
                }
                else
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos,
                        cursorPos + new Vector2(barWidth * scale, _config.LilyBarSize.Y),
                        _config.LilyChargingColor.Map["gradientLeft"],
                        _config.LilyChargingColor.Map["gradientRight"],
                        _config.LilyChargingColor.Map["gradientRight"],
                        _config.LilyChargingColor.Map["gradientLeft"]
                    );
                }

                if (scale < 1)
                {
                    string timer = (lilyCooldown / 1000f - gauge.LilyTimer / 1000f).ToString("0.0");
                    Vector2 size = ImGui.CalcTextSize((lilyCooldown / 1000).ToString("0.0"));
                    DrawOutlinedText(timer, new Vector2(cursorPos.X + barWidth / 2f - size.X / 2f, cursorPos.Y - 23));
                }
            }

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            // Blood Lilies

            barCoords = _config.BasePosition + _config.BloodLilyBarPosition;
            barWidth = (_config.BloodLilyBarSize.X - _config.BloodLilyBarPad * numChunks) / numChunks;
            barSize = new Vector2(barWidth, _config.BloodLilyBarSize.Y);
            xPos = CenterX - barCoords.X;
            yPos = CenterY + barCoords.Y;

            cursorPos = new Vector2(xPos + _config.BloodLilyBarPad + barWidth, yPos);
            scale = gauge.NumBloodLily > 0 ? 1 : 0;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            drawList.AddRectFilledMultiColor(
                cursorPos,
                cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                _config.BloodLilyColor.Map["gradientLeft"],
                _config.BloodLilyColor.Map["gradientRight"],
                _config.BloodLilyColor.Map["gradientRight"],
                _config.BloodLilyColor.Map["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + _config.BloodLilyBarPad + barWidth, cursorPos.Y);
            scale = gauge.NumBloodLily > 1 ? 1 : 0;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            drawList.AddRectFilledMultiColor(
                cursorPos,
                cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                _config.BloodLilyColor.Map["gradientLeft"],
                _config.BloodLilyColor.Map["gradientRight"],
                _config.BloodLilyColor.Map["gradientRight"],
                _config.BloodLilyColor.Map["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            cursorPos = new Vector2(cursorPos.X + _config.BloodLilyBarPad + barWidth, cursorPos.Y);
            scale = gauge.NumBloodLily > 2 ? 1 : 0;
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, EmptyColor["background"]);

            drawList.AddRectFilledMultiColor(
                cursorPos,
                cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                _config.BloodLilyColor.Map["gradientLeft"],
                _config.BloodLilyColor.Map["gradientRight"],
                _config.BloodLilyColor.Map["gradientRight"],
                _config.BloodLilyColor.Map["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
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
        public bool ShowLilyBar = true;

        [DragFloat2("Lily Bar Size", max = 2000f)]
        [CollapseWith(0, 0)]
        public Vector2 LilyBarSize = new(127, 20);

        [DragFloat2("Lily Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(5, 0)]
        public Vector2 LilyBarPosition = new(127, 395);

        [DragInt("Lily Bar Padding", min = 0, max = 1000)]
        [CollapseWith(10, 0)]
        public int LilyBarPad = 2;

        [ColorEdit4("Lily Bar Color")]
        [CollapseWith(15, 0)]
        public PluginConfigColor LilyColor = new(new Vector4(0f / 255f, 64f / 255f, 1f, 1f));

        [ColorEdit4("Lily Bar Charging Color")]
        [CollapseWith(20, 0)]
        public PluginConfigColor LilyChargingColor = new(new Vector4(141f / 255f, 141f / 255f, 141f / 255f, 1f));

        #endregion

        #region Blood Lily Bar

        [DragFloat2("Blood Lily Bar Size", max = 2000f)]
        [CollapseWith(25, 0)]
        public Vector2 BloodLilyBarSize = new(127, 20);

        [DragFloat2("Blood Lily Bar Position", min = -4000f, max = 4000f)]
        [CollapseWith(30, 0)]
        public Vector2 BloodLilyBarPosition = new(42, 395);

        [DragInt("Blood Lily Bar Padding", min = 0, max = 1000)]
        [CollapseWith(35, 0)]
        public int BloodLilyBarPad = 2;

        [ColorEdit4("Blood Lily Bar Color")]
        [CollapseWith(40, 0)]
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
