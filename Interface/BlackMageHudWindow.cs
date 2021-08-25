using System;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;
using System.Linq;
using System.Collections;

namespace DelvUIPlugin.Interface
{
    public class BlackMageHudWindow : HudWindow
    {
        public override uint JobId => Jobs.BLM;

        private static int JobBarWidth = 180;

        public BlackMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawEnochian();
            DrawManaBar();
            DrawUmbralIceStacks();
            DrawPolyglot();
            DrawTargetBar();
            DrawTripleCast();
        }

        protected virtual void DrawManaBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float)actor.CurrentMp / actor.MaxMp;
            var barSize = new Vector2(JobBarWidth, 12);
            var cursorPos = new Vector2(CenterX - barSize.X / 2, CenterY + YOffset - 50);

            // mana bar
            var bgColor = 0x88000000;
            var leftColor = 0xFF450983;
            var rightColor = 0xFF8F53DC;

            if (gauge.InAstralFire())
            {
                bgColor = 0x88000022;
                leftColor = 0xFF0000AA;
                rightColor = 0xFF6666FF;
            } 
            else if (gauge.InUmbralIce())
            {
                bgColor = 0x88220000;
                leftColor = 0xFF592525;
                rightColor = 0xFFFFAAAA;
            }

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, bgColor);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                leftColor, rightColor, rightColor, leftColor
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            // element timer
            var time = gauge.ElementTimeRemaining > 10 ? gauge.ElementTimeRemaining / 1000 + 1 : 0;
            var text = $"{time,0}";
            var textSize = ImGui.CalcTextSize(text);
            DrawOutlinedText(text, new Vector2(CenterX - textSize.X / 2f, cursorPos.Y - 6));
        }

        protected virtual void DrawEnochian()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();
            if (!gauge.IsEnoActive())
            {
                return;
            }

            var barSize = new Vector2(JobBarWidth + 4, 16);
            var cursorPos = new Vector2(CenterX - barSize.X / 2, CenterY + YOffset - 52);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88FFFFFF);
        }

        protected virtual void DrawUmbralIceStacks()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var barSize = new Vector2((JobBarWidth - 6) / 3, 16);
            var cursorPos = new Vector2(CenterX - JobBarWidth / 2, CenterY + YOffset - 70);

            var drawList = ImGui.GetWindowDrawList();

            for (int i = 1; i <= 3; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                if (gauge.NumUmbralHearts >= i)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                        0xFFFAC4C3, 0xFFFFEDE8, 0xFFFFEDE8, 0xFFFAC4C3
                    );
                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos.X = cursorPos.X + barSize.X + 3;
            }
        }

        protected virtual void DrawPolyglot()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();
            var barSize = new Vector2(18, 12);
            var scale = gauge.IsEnoActive() ? (gauge.NumPolyglotStacks == 2 ? 1 : gauge.TimeUntilNextPolyglot / 30000f) : 1;
            scale = 1 - scale;

            var drawList = ImGui.GetWindowDrawList();

            if (gauge.NumPolyglotStacks == 0)
            {
                var cursorPos = new Vector2(CenterX - barSize.X / 2f, CenterY + YOffset - 85);
                DrawPolyglotStack(cursorPos, barSize, scale, false);
            } 
            else
            {
                // 1st stack (charged)
                var cursorPos = new Vector2(CenterX - barSize.X - 1, CenterY + YOffset - 85);
                DrawPolyglotStack(cursorPos, barSize, 1, true);

                // 2nd stack
                bool is2ndCharged = gauge.NumPolyglotStacks == 2;
                cursorPos.X = CenterX + 1;
                DrawPolyglotStack(cursorPos, barSize, is2ndCharged ? 1 : scale, is2ndCharged);
            }
        }

        private void DrawPolyglotStack(Vector2 position, Vector2 size, float scale, bool isFullyCharged)
        {
            var drawList = ImGui.GetWindowDrawList();

            // glow
            if (isFullyCharged)
            {
                var glowPosition = new Vector2(position.X - 1, position.Y - 1);
                var glowSize = new Vector2(size.X + 2, size.Y + 2);
                drawList.AddRectFilled(glowPosition, glowPosition + glowSize, 0xAAFFFFFF);
            }

            // background
            drawList.AddRectFilled(position, position + size, 0x88000000);

            // fill
            if (isFullyCharged)
            {
                drawList.AddRectFilledMultiColor(position, position + size, 0xFF5F3A93, 0xFFBE8FFF, 0xFFBE8FFF, 0xFF5F3A93);
            }
            else
            {
                drawList.AddRectFilled(position, position + new Vector2(size.X * scale, size.Y), 0xFF5F3A93);
            }

            // black border
            drawList.AddRect(position, position + size, 0xFF000000);
        }

        protected virtual void DrawTripleCast()
        {
            var tripleStackBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1211);
            var drawList = ImGui.GetWindowDrawList();

            var positions = new ArrayList();
            positions.Add(new Vector2(CenterX, CenterY + YOffset - 100));
            positions.Add(new Vector2(CenterX - JobBarWidth / 2f + 30, CenterY + YOffset - 85));
            positions.Add(new Vector2(CenterX + JobBarWidth / 2f - 30, CenterY + YOffset - 85));

            for (int i = 0; i < tripleStackBuff.StackCount; i++)
            {
                drawList.AddCircle((Vector2)positions[i], 10, 0xFFFFFFFF, 6, 4);
            }
        }
    }
}