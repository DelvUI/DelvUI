using System;
using System.Numerics;
using Dalamud.Game.ClientState.Structs.JobGauge;
using Dalamud.Plugin;
using ImGuiNET;
using System.Linq;
using System.Collections.Generic;

namespace DelvUIPlugin.Interface
{
    public class BlackMageHudWindow : HudWindow
    {
        public override uint JobId => Jobs.BLM;

        private float OriginX => CenterX;
        private float OriginY => CenterY + YOffset + PluginConfiguration.BLMVerticalOffset;
        private int ManaBarWidth => PluginConfiguration.BLMManaBarWidth;
        private int ManaBarHeight => PluginConfiguration.BLMManaBarHeight;
        private int UmbralHeartHeight => PluginConfiguration.BLMUmbralHeartHeight;
        private int PolyglotHeight => PluginConfiguration.BLMPolyglotHeight;
        private int PolyglotWidth => PluginConfiguration.BLMPolyglotWidth;
        private int VerticalSpaceBetweenBars => PluginConfiguration.BLMVerticalSpaceBetweenBars;
        private int HorizontalSpaceBetweenBars => PluginConfiguration.BLMHorizontalSpaceBetweenBars;
        private bool ShowTripleCast => PluginConfiguration.BLMShowTripleCast;
        private bool UseBarsForTripleCast => PluginConfiguration.BLMUseBarsForTripleCast;
        private int TripleCastHeight => PluginConfiguration.BLMTripleCastHeight;
        private int TripleCastWidth => PluginConfiguration.BLMTripleCastWidth;
        private int TripleCastRadius => PluginConfiguration.BLMTripleCastRadius;
        
        private Dictionary<string, uint> ManaBarNoElementColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000];
        private Dictionary<string, uint> ManaBarIceColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 1];
        private Dictionary<string, uint> ManaBarFireColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 2];
        private Dictionary<string, uint> UmbralHeartColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 3];
        private Dictionary<string, uint> PolyglotColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 4];
        private Dictionary<string, uint> TriplecastColor => PluginConfiguration.JobColorMap[Jobs.BLM * 1000 + 5];

        public BlackMageHudWindow(DalamudPluginInterface pluginInterface, PluginConfiguration pluginConfiguration) : base(pluginInterface, pluginConfiguration) { }

        protected override void Draw(bool _)
        {
            DrawHealthBar();
            DrawFocusBar();
            DrawCastBar();
            DrawTargetBar();


            DrawEnochian();
            DrawManaBar();
            DrawUmbralHeartStacks();
            DrawPolyglot();

            if (ShowTripleCast) {
                DrawTripleCast();
            }
        }

        protected virtual void DrawManaBar()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var actor = PluginInterface.ClientState.LocalPlayer;
            var scale = (float)actor.CurrentMp / actor.MaxMp;
            var barSize = new Vector2(ManaBarWidth, ManaBarHeight);
            var cursorPos = new Vector2(OriginX - barSize.X / 2, OriginY - ManaBarHeight);

            // mana bar
            var color = gauge.InAstralFire() ? ManaBarFireColor : (gauge.InUmbralIce() ? ManaBarIceColor : ManaBarNoElementColor);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, color["background"]);
            drawList.AddRectFilledMultiColor(
                cursorPos, cursorPos + new Vector2(barSize.X * scale, barSize.Y),
                color["gradientLeft"], color["gradientRight"], color["gradientRight"], color["gradientLeft"]
            );

            drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

            // element timer
            var time = gauge.ElementTimeRemaining > 10 ? gauge.ElementTimeRemaining / 1000 + 1 : 0;
            var text = $"{time,0}";
            var textSize = ImGui.CalcTextSize(text);
            DrawOutlinedText(text, new Vector2(CenterX - textSize.X / 2f, OriginY - ManaBarHeight / 2f - textSize.Y / 2f));
        }

        protected virtual void DrawEnochian()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();
            if (!gauge.IsEnoActive())
            {
                return;
            }

            var barSize = new Vector2(ManaBarWidth + 4, ManaBarHeight + 4);
            var cursorPos = new Vector2(OriginX - barSize.X / 2f, OriginY - ManaBarHeight - 2);

            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88FFFFFF);
        }

        protected virtual void DrawUmbralHeartStacks()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var barSize = new Vector2((ManaBarWidth - (HorizontalSpaceBetweenBars * 2)) / 3, UmbralHeartHeight);
            var cursorPos = new Vector2(OriginX - ManaBarWidth / 2, OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight);

            var drawList = ImGui.GetWindowDrawList();

            for (int i = 1; i <= 3; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, UmbralHeartColor["background"]);
                if (gauge.NumUmbralHearts >= i)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                        UmbralHeartColor["gradientLeft"], UmbralHeartColor["gradientRight"], UmbralHeartColor["gradientRight"], UmbralHeartColor["gradientLeft"]
                    );
                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos.X = cursorPos.X + barSize.X + HorizontalSpaceBetweenBars;
            }
        }

        protected virtual void DrawPolyglot()
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();
            var barSize = new Vector2(PolyglotWidth, PolyglotHeight);
            var scale = gauge.IsEnoActive() ? (gauge.NumPolyglotStacks == 2 ? 1 : gauge.TimeUntilNextPolyglot / 30000f) : 1;
            scale = 1 - scale;

            var y = OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight - VerticalSpaceBetweenBars - PolyglotHeight;
            if (UseBarsForTripleCast)
            {
                y = y - VerticalSpaceBetweenBars - TripleCastHeight;
            }

            var drawList = ImGui.GetWindowDrawList();

            if (gauge.NumPolyglotStacks == 0)
            {
                var cursorPos = new Vector2(OriginX - barSize.X / 2f, y);
                DrawPolyglotStack(cursorPos, barSize, scale);
            } 
            else
            {
                // 1st stack (charged)
                var cursorPos = new Vector2(OriginX - barSize.X - (HorizontalSpaceBetweenBars / 2f), y);
                DrawPolyglotStack(cursorPos, barSize, 1);

                // 2nd stack
                cursorPos.X = CenterX + (HorizontalSpaceBetweenBars / 2f);
                DrawPolyglotStack(cursorPos, barSize, scale);
            }
        }

        private void DrawPolyglotStack(Vector2 position, Vector2 size, float scale)
        {
            var drawList = ImGui.GetWindowDrawList();

            // glow
            if (scale == 1)
            {
                var glowPosition = new Vector2(position.X - 1, position.Y - 1);
                var glowSize = new Vector2(size.X + 2, size.Y + 2);
                drawList.AddRectFilled(glowPosition, glowPosition + glowSize, 0xAAFFFFFF);
            }

            // background
            drawList.AddRectFilled(position, position + size, PolyglotColor["background"]);

            // fill
            if (scale == 1)
            {
                drawList.AddRectFilledMultiColor(position, position + size,
                    PolyglotColor["gradientLeft"], PolyglotColor["gradientRight"], PolyglotColor["gradientRight"], PolyglotColor["gradientLeft"]
                );
            }
            else
            {
                drawList.AddRectFilled(position, position + new Vector2(size.X * scale, size.Y), PolyglotColor["gradientLeft"]);
            }

            // black border
            drawList.AddRect(position, position + size, 0xFF000000);
        }

        protected virtual void DrawTripleCast()
        {
            var tripleStackBuff = PluginInterface.ClientState.LocalPlayer.StatusEffects.FirstOrDefault(o => o.EffectId == 1211);

            if (UseBarsForTripleCast)
            {
                DrawTripleCastBars(tripleStackBuff.StackCount);
            }
            else
            {
                DrawTeipleCastCircles(tripleStackBuff.StackCount);
            }
        }

        private void DrawTripleCastBars(int stackCount)
        {
            var gauge = PluginInterface.ClientState.JobGauges.Get<BLMGauge>();

            var barSize = new Vector2(TripleCastWidth, TripleCastHeight);
            var totalWidth = barSize.X * 3 + HorizontalSpaceBetweenBars * 2;
            var cursorPos = new Vector2(OriginX - totalWidth / 2, OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight - VerticalSpaceBetweenBars - barSize.Y);

            var drawList = ImGui.GetWindowDrawList();

            for (int i = 1; i <= 3; i++)
            {
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, TriplecastColor["background"]);
                if (stackCount >= i)
                {
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + new Vector2(barSize.X, barSize.Y),
                        TriplecastColor["gradientLeft"], TriplecastColor["gradientRight"], TriplecastColor["gradientRight"], TriplecastColor["gradientLeft"]
                    );
                }
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                cursorPos.X = cursorPos.X + barSize.X + HorizontalSpaceBetweenBars;
            }
        }

        private void DrawTeipleCastCircles(int stackCount)
        {
            var drawList = ImGui.GetWindowDrawList();

            var y = OriginY - ManaBarHeight - VerticalSpaceBetweenBars - UmbralHeartHeight - VerticalSpaceBetweenBars;
            var positions = new List<Vector2>();
            positions.Add(new Vector2(OriginX, y - PolyglotHeight - VerticalSpaceBetweenBars - TripleCastRadius));
            positions.Add(new Vector2(OriginX - ManaBarWidth / 2f + ManaBarWidth / 6f, y - TripleCastRadius));
            positions.Add(new Vector2(OriginX + ManaBarWidth / 2f - ManaBarWidth / 6f, y - TripleCastRadius));

            for (int i = 0; i < stackCount; i++)
            {
                drawList.AddCircle(positions[i], TripleCastRadius, TriplecastColor["base"], 6, 4);
            }
        }
    }
}