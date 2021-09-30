using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace DelvUI.Interface.Bars
{
    public class Bar
    {
        private uint _backgroundColor;
        private bool _backgroundColorSet;
        public bool DrawBorder;

        public Bar(float xPosition, float yPosition, int height, int width)
        {
            XPosition = xPosition;
            YPosition = yPosition;
            BarHeight = height;
            BarWidth = width;
            InnerBars = new List<InnerBar>();
            PrimaryTexts = new List<BarText>();
            ChunkPadding = 0;
            ChunkSizes = new[] { 1f };
            DrawBorder = true;
        }

        public List<InnerBar> InnerBars { get; set; }
        public float XPosition { get; set; }
        public float YPosition { get; set; }
        public int BarHeight { get; set; }
        public int BarWidth { get; set; }
        public bool Vertical { get; set; }
        public int ChunkPadding { get; set; }
        public float[] ChunkSizes { get; set; }
        public List<BarText> PrimaryTexts { get; set; }

        public uint BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColorSet = true;
                _backgroundColor = value;
            }
        }

        public int AddInnerBar(InnerBar innerBar)
        {
            innerBar.Parent = this;
            innerBar.ChildNum = InnerBars.Count;
            InnerBars.Add(innerBar);

            return innerBar.ChildNum;
        }

        public int AddInnerBooleanBar(BooleanInnerBar innerBar)
        {
            innerBar.Parent = this;
            innerBar.ChildNum = InnerBars.Count;
            InnerBars.Add(innerBar);

            return innerBar.ChildNum;
        }

        public Vector2 GetBarSize() => new(BarWidth, BarHeight);

        public void Draw(ImDrawListPtr drawList)
        {
            var barWidth = BarWidth + ChunkPadding;   // For loop adds one extra padding more than is needed
            var barHeight = BarHeight + ChunkPadding; // For loop adds one extra padding more than is needed
            var cursorPos = new Vector2(XPosition, YPosition);
            var backgroundColor = _backgroundColorSet ? BackgroundColor : 0x88000000;

            foreach (var chunkSize in ChunkSizes)
            {
                var barSize = Vertical ? new Vector2(BarWidth, barHeight * chunkSize - ChunkPadding) : new Vector2(barWidth * chunkSize - ChunkPadding, BarHeight);

                drawList.AddRectFilled(cursorPos, cursorPos + barSize, backgroundColor);

                cursorPos += Vertical ? new Vector2(0, barHeight * chunkSize) : new Vector2(barWidth * chunkSize, 0);
            }

            foreach (var innerBar in InnerBars)
            {
                innerBar.Draw(drawList);
            }

            cursorPos = new Vector2(XPosition, YPosition);

            if (DrawBorder)
            {
                foreach (var chunkSize in ChunkSizes)
                {
                    var barSize = Vertical ? new Vector2(BarWidth, barHeight * chunkSize - ChunkPadding) : new Vector2(barWidth * chunkSize - ChunkPadding, BarHeight);

                    drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);

                    cursorPos += Vertical ? new Vector2(0, barHeight * chunkSize) : new Vector2(barWidth * chunkSize, 0);
                }
            }

            foreach (var innerBar in InnerBars)
            {
                innerBar.DrawText(drawList);
            }

            DrawText(drawList);
        }

        public void DrawText(ImDrawListPtr drawList)
        {
            foreach (var text in PrimaryTexts)
            {
                var cursorPos = new Vector2(XPosition, YPosition);

                var strText = text.Type switch
                {
                    BarTextType.Current => throw new InvalidOperationException("Full bar text must be 'Custom' type."),
                    BarTextType.Remaining => throw new InvalidOperationException("Full bar text must be 'Custom' type."),
                    BarTextType.Maximum => throw new InvalidOperationException("Full bar text must be 'Custom' type."),
                    BarTextType.Percentage => throw new InvalidOperationException("Full bar text must be 'Custom' type."),
                    BarTextType.Custom => text.Text,
                    _ => "ERROR LOADING TEXT, INVALID TYPE"
                };

                if (strText == null)
                {
                    continue;
                }

                var textPos = text.CalcTextPosition(cursorPos, strText, BarWidth, BarHeight);
                DrawHelper.DrawOutlinedText(strText, textPos, text.Color, text.OutlineColor);
            }
        }
    }

    public class InnerBar
    {
        private bool[] _glowChunks = null!;
        private bool _glowChunksSet;
        private uint _glowColor;
        private bool _glowColorSet;
        public Bar Parent { get; set; } = null!;
        public int ChildNum { get; set; }
        public float MaximumValue { get; set; }
        public float CurrentValue { get; set; }
        public PluginConfigColor[] ChunkColors { get; set; } = null!;
        public PluginConfigColor? PartialFillColor { get; set; }

        public uint GlowColor
        {
            get => _glowColor;
            set
            {
                _glowColorSet = true;
                _glowColor = value;
            }
        }

        public bool[] GlowChunks
        {
            get => _glowChunks;
            set
            {
                _glowChunksSet = true;
                _glowChunks = value;
            }
        }

        public uint GlowSize { get; set; } = 1;
        public bool FlipDrainDirection { get; set; }
        public BarTextMode TextMode { get; set; }
        public BarText[]? Texts { get; set; }

        public virtual void Draw(ImDrawListPtr drawList)
        {
            var barWidth = Parent.Vertical ? (float)1 / Parent.InnerBars.Count * Parent.BarWidth : Parent.BarWidth + Parent.ChunkPadding;
            var barHeight = Parent.Vertical ? Parent.BarHeight + Parent.ChunkPadding : (float)1 / Parent.InnerBars.Count * Parent.BarHeight;
            var currentFill = CurrentValue / MaximumValue;
            var i = 0;

            if (!FlipDrainDirection)
            {
                var xPos = Parent.Vertical ? Parent.XPosition + (float)ChildNum / Parent.InnerBars.Count * Parent.BarWidth : Parent.XPosition;
                var yPos = Parent.Vertical ? Parent.YPosition : Parent.YPosition + (float)ChildNum / Parent.InnerBars.Count * Parent.BarHeight;
                var cursorPos = new Vector2(xPos, yPos);

                foreach (var chunkSize in Parent.ChunkSizes)
                {
                    var barSize = Parent.Vertical
                        ? new Vector2(barWidth, barHeight * chunkSize - Parent.ChunkPadding)
                        : new Vector2(barWidth * chunkSize - Parent.ChunkPadding, barHeight);

                    var fillPercentage = (float)Math.Round(Math.Min(currentFill / chunkSize, 1f), 5); // Rounding due to floating point precision shenanigans

                    if (fillPercentage >= 1f)
                    {
                        currentFill -= chunkSize;

                        DrawHelper.DrawGradientFilledRect(cursorPos, barSize, ChunkColors[i], drawList);
                    }
                    else
                    {
                        currentFill = 0f;
                        var fillVector = Parent.Vertical ? new Vector2(barSize.X, barSize.Y * fillPercentage) : new Vector2(barSize.X * fillPercentage, barSize.Y);

                        if (PartialFillColor != null)
                        {
                            DrawHelper.DrawGradientFilledRect(cursorPos, fillVector, PartialFillColor, drawList);
                        }
                        else
                        {
                            DrawHelper.DrawGradientFilledRect(cursorPos, fillVector, ChunkColors[i], drawList);
                        }
                    }

                    if (_glowColorSet && (!_glowChunksSet || GlowChunks[i]))
                    {
                        var glowPosition = new Vector2(cursorPos.X - 1, cursorPos.Y - 1);
                        var glowSize = new Vector2(barSize.X + 2, barSize.Y + 2);

                        drawList.AddRect(glowPosition, glowPosition + glowSize, GlowColor, 0, ImDrawFlags.None, GlowSize);
                    }

                    i++;
                    cursorPos += Parent.Vertical ? new Vector2(0, barHeight * chunkSize) : new Vector2(barWidth * chunkSize, 0);
                }
            }
            else
            {
                var xPos = Parent.Vertical ? Parent.XPosition - (float)ChildNum / Parent.InnerBars.Count * Parent.BarWidth : Parent.XPosition + barWidth;
                var yPos = Parent.Vertical ? Parent.YPosition + barHeight : Parent.YPosition + (float)ChildNum / Parent.InnerBars.Count * Parent.BarHeight;
                var cursorPos = new Vector2(xPos, yPos);

                foreach (var chunkSize in Parent.ChunkSizes.AsEnumerable().Reverse().ToList())
                {
                    cursorPos -= Parent.Vertical ? new Vector2(0, barHeight * chunkSize) : new Vector2(barWidth * chunkSize, 0);

                    var barSize = Parent.Vertical
                        ? new Vector2(barWidth, barHeight * chunkSize - Parent.ChunkPadding)
                        : new Vector2(barWidth * chunkSize - Parent.ChunkPadding, barHeight);

                    var fillPercentage = (float)Math.Round(Math.Min(currentFill / chunkSize, 1f), 5); // Rounding due to floating point precision shenanigans

                    if (fillPercentage >= 1f)
                    {
                        currentFill -= chunkSize;

                        DrawHelper.DrawGradientFilledRect(cursorPos, barSize, ChunkColors[i], drawList);
                    }
                    else
                    {
                        currentFill = 0f;
                        var fillVector = Parent.Vertical ? new Vector2(barSize.X, barSize.Y * fillPercentage) : new Vector2(barSize.X * fillPercentage, barSize.Y);

                        if (PartialFillColor != null)
                        {
                            DrawHelper.DrawGradientFilledRect(cursorPos + barSize - fillVector, fillVector, PartialFillColor, drawList);
                        }
                        else
                        {
                            DrawHelper.DrawGradientFilledRect(cursorPos + barSize - fillVector, fillVector, ChunkColors[i], drawList);
                        }
                    }

                    if (_glowColorSet)
                    {
                        var glowPosition = new Vector2(cursorPos.X - 1, cursorPos.Y - 1);
                        var glowSize = new Vector2(barSize.X + 2, barSize.Y + 2);

                        drawList.AddRect(glowPosition, glowPosition + glowSize, GlowColor);
                    }

                    i++;
                }
            }
        }

        public void DrawText(ImDrawListPtr drawList)
        {
            var barWidth = Parent.Vertical ? (float)1 / Parent.InnerBars.Count * Parent.BarWidth : Parent.BarWidth + Parent.ChunkPadding;
            var barHeight = Parent.Vertical ? Parent.BarHeight + Parent.ChunkPadding : (float)1 / Parent.InnerBars.Count * Parent.BarHeight;
            var cursorPos = new Vector2(Parent.XPosition, Parent.YPosition);

            if (TextMode == BarTextMode.Single && Texts?.Length > 0)
            {
                var textObj = Texts[0];

                var text = textObj.Type switch
                {
                    BarTextType.Current => Math.Round(CurrentValue).ToString(CultureInfo.InvariantCulture),
                    BarTextType.Remaining => Math.Round(MaximumValue - CurrentValue).ToString(CultureInfo.InvariantCulture),
                    BarTextType.Maximum => Math.Round(MaximumValue).ToString(CultureInfo.InvariantCulture),
                    BarTextType.Percentage => Math.Round(CurrentValue / MaximumValue * 100f).ToString(CultureInfo.InvariantCulture),
                    BarTextType.Custom => textObj.Text,
                    _ => "ERROR LOADING TEXT, INVALID TYPE"
                };

                if (text == null)
                {
                    return;
                }

                var textPos = textObj.CalcTextPosition(cursorPos, text, Parent.BarWidth, Parent.BarHeight);
                DrawHelper.DrawOutlinedText(text, textPos, textObj.Color, textObj.OutlineColor);
            }

            var currentFill = CurrentValue / MaximumValue;
            var i = 0;

            if (!FlipDrainDirection)
            {
                foreach (var chunkSize in Parent.ChunkSizes)
                {
                    var barValue = Math.Min(currentFill, chunkSize) * MaximumValue;
                    var barMaximum = chunkSize * MaximumValue;

                    var fillPercentage = Math.Min(currentFill / chunkSize, 1f);

                    var barSize = Parent.Vertical
                        ? new Vector2(barWidth, barHeight * chunkSize - Parent.ChunkPadding)
                        : new Vector2(barWidth * chunkSize - Parent.ChunkPadding, barHeight);

                    if (fillPercentage >= 1f)
                    {
                        currentFill -= chunkSize;
                    }
                    else
                    {
                        currentFill = 0f;
                    }

                    if (TextMode == BarTextMode.EachChunk && Texts?.Length > i)
                    {
                        var textObj = Texts[i];

                        var text = textObj.Type switch
                        {
                            BarTextType.Current => Math.Round(barValue).ToString(CultureInfo.InvariantCulture),
                            BarTextType.Remaining => Math.Round(barMaximum - barValue).ToString(CultureInfo.InvariantCulture),
                            BarTextType.Maximum => Math.Round(barMaximum).ToString(CultureInfo.InvariantCulture),
                            BarTextType.Percentage => Math.Round(currentFill * 100f).ToString(CultureInfo.InvariantCulture),
                            BarTextType.Custom => textObj.Text,
                            _ => "ERROR LOADING TEXT, INVALID TYPE"
                        };

                        if (text != null)
                        {
                            var textPos = Parent.Vertical
                                ? textObj.CalcTextPosition(cursorPos, text, Parent.BarWidth, barSize.Y)
                                : textObj.CalcTextPosition(cursorPos, text, barSize.X, Parent.BarHeight);

                            DrawHelper.DrawOutlinedText(text, textPos, textObj.Color, textObj.OutlineColor);
                        }
                    }

                    i++;
                    cursorPos += Parent.Vertical ? new Vector2(0, barHeight * chunkSize) : new Vector2(barWidth * chunkSize, 0);
                }
            }
            else
            {
                var xPos = Parent.Vertical ? Parent.XPosition : Parent.XPosition + barWidth;
                var yPos = Parent.Vertical ? Parent.YPosition + barHeight : Parent.YPosition;
                cursorPos = new Vector2(xPos, yPos);

                foreach (var chunkSize in Parent.ChunkSizes.AsEnumerable().Reverse().ToList())
                {
                    cursorPos -= Parent.Vertical ? new Vector2(0, barHeight * chunkSize) : new Vector2(barWidth * chunkSize, 0);

                    var barValue = Math.Min(currentFill, chunkSize) * MaximumValue;
                    var barMaximum = chunkSize * MaximumValue;

                    var fillPercentage = Math.Min(currentFill / chunkSize, 1f);

                    var barSize = new Vector2(barWidth * chunkSize - Parent.ChunkPadding, barHeight);

                    if (fillPercentage >= 1f)
                    {
                        currentFill -= chunkSize;
                    }
                    else
                    {
                        currentFill = 0f;
                    }

                    if (TextMode == BarTextMode.EachChunk && Texts?.Length > i)
                    {
                        var textObj = Texts[i];

                        var text = textObj.Type switch
                        {
                            BarTextType.Current => Math.Round(barValue).ToString(CultureInfo.InvariantCulture),
                            BarTextType.Remaining => Math.Round(barMaximum - barValue).ToString(CultureInfo.InvariantCulture),
                            BarTextType.Maximum => Math.Round(barMaximum).ToString(CultureInfo.InvariantCulture),
                            BarTextType.Percentage => Math.Round(currentFill * 100f).ToString(CultureInfo.InvariantCulture),
                            BarTextType.Custom => textObj.Text,
                            _ => "ERROR LOADING TEXT, INVALID TYPE"
                        };

                        if (text != null)
                        {
                            var textPos = Parent.Vertical
                                ? textObj.CalcTextPosition(cursorPos, text, Parent.BarWidth, barSize.Y)
                                : textObj.CalcTextPosition(cursorPos, text, barSize.X, Parent.BarHeight);

                            DrawHelper.DrawOutlinedText(text, textPos, textObj.Color, textObj.OutlineColor);
                        }
                    }

                    i++;
                }
            }
        }
    }

    public class BooleanInnerBar : InnerBar
    {
        private bool[] _enableArray = null!;

        public bool[] EnableArray
        {
            get => _enableArray;
            set
            {
                var trues = 0;

                foreach (var val in value)
                {
                    trues += val ? 1 : 0;
                }

                CurrentValue = trues;
                MaximumValue = value.Length;
                _enableArray = value;
            }
        }

        public override void Draw(ImDrawListPtr drawList)
        {
            var barWidth = Parent.Vertical ? (float)1 / Parent.InnerBars.Count * Parent.BarWidth : Parent.BarWidth + Parent.ChunkPadding;
            var barHeight = Parent.Vertical ? Parent.BarHeight + Parent.ChunkPadding : (float)1 / Parent.InnerBars.Count * Parent.BarHeight;
            var xPos = Parent.Vertical ? Parent.XPosition + (float)ChildNum / Parent.InnerBars.Count * Parent.BarWidth : Parent.XPosition;
            var yPos = Parent.Vertical ? Parent.YPosition : Parent.YPosition + (float)ChildNum / Parent.InnerBars.Count * Parent.BarHeight;
            var cursorPos = new Vector2(xPos, yPos);

            var i = 0;

            foreach (var chunkSize in Parent.ChunkSizes)
            {
                var barSize = Parent.Vertical
                    ? new Vector2(barWidth, barHeight * chunkSize - Parent.ChunkPadding)
                    : new Vector2(barWidth * chunkSize - Parent.ChunkPadding, barHeight);

                if (EnableArray[i])
                {
                    DrawHelper.DrawGradientFilledRect(cursorPos, barSize, ChunkColors[i], drawList);
                }
                else if (PartialFillColor != null)
                {
                    DrawHelper.DrawGradientFilledRect(cursorPos, barSize, PartialFillColor, drawList);
                }

                i++;
                cursorPos += Parent.Vertical ? new Vector2(0, barHeight * chunkSize) : new Vector2(barWidth * chunkSize, 0);
            }
        }
    }

    public class BarText
    {
        public BarText(BarTextPosition position, BarTextType type, Vector4 color, Vector4 outlineColor, string? text)
        {
            Position = position;
            Type = type;
            Color = color;
            OutlineColor = outlineColor;
            Text = text;
        }

        public BarText(BarTextPosition position, BarTextType type, string? text)
        {
            Position = position;
            Type = type;
            Text = text;
            Color = Vector4.One;
            OutlineColor = new Vector4(0f, 0f, 0f, 1f);
        }

        public BarText(BarTextPosition position, BarTextType type)
        {
            Position = position;
            Type = type;
            Text = null;
            Color = Vector4.One;
            OutlineColor = new Vector4(0f, 0f, 0f, 1f);
        }

        // TODO: Proper text tags
        public BarTextPosition Position { get; set; }
        public BarTextType Type { get; set; }
        public Vector4 Color { get; set; }
        public Vector4 OutlineColor { get; set; }
        public string? Text { get; set; }

        public Vector2 CalcTextPosition(Vector2 cursorPos, string text, float barWidth, float barHeight)
        {
            float textXPos;
            float textYPos;

            var textSize = ImGui.CalcTextSize(text);

            switch (Position)
            {
                case BarTextPosition.TopLeft:
                    textXPos = cursorPos.X + 2;
                    textYPos = cursorPos.Y - 2;

                    break;

                default:
                case BarTextPosition.TopMiddle:
                    textXPos = cursorPos.X + barWidth / 2f - textSize.X / 2f;
                    textYPos = cursorPos.Y - 2;

                    break;

                case BarTextPosition.TopRight:
                    textXPos = cursorPos.X + barWidth - textSize.X - 2;
                    textYPos = cursorPos.Y - 2;

                    break;

                case BarTextPosition.CenterLeft:
                    textXPos = cursorPos.X + 2;
                    textYPos = cursorPos.Y + barHeight / 2f - textSize.Y / 2f;

                    break;

                case BarTextPosition.CenterMiddle:
                    textXPos = cursorPos.X + barWidth / 2f - textSize.X / 2f;
                    textYPos = cursorPos.Y + barHeight / 2f - textSize.Y / 2f;

                    break;

                case BarTextPosition.CenterRight:
                    textXPos = cursorPos.X + barWidth - textSize.X - 2;
                    textYPos = cursorPos.Y + barHeight / 2f - textSize.Y / 2f;

                    break;

                case BarTextPosition.BottomLeft:
                    textXPos = cursorPos.X + 2;
                    textYPos = cursorPos.Y + barHeight - textSize.Y + 2;

                    break;

                case BarTextPosition.BottomMiddle:
                    textXPos = cursorPos.X + barWidth / 2f - textSize.X / 2f;
                    textYPos = cursorPos.Y + barHeight - textSize.Y + 2;

                    break;

                case BarTextPosition.BottomRight:
                    textXPos = cursorPos.X + barWidth - textSize.X - 2;
                    textYPos = cursorPos.Y + barHeight - textSize.Y + 2;

                    break;

                case BarTextPosition.AboveLeft:
                    textXPos = cursorPos.X + 2;
                    textYPos = cursorPos.Y + 2 - textSize.Y;

                    break;

                case BarTextPosition.AboveMiddle:
                    textXPos = cursorPos.X + barWidth / 2f - textSize.X / 2f;
                    textYPos = cursorPos.Y + 2 - textSize.Y;

                    break;

                case BarTextPosition.AboveRight:
                    textXPos = cursorPos.X + barWidth - textSize.X - 2;
                    textYPos = cursorPos.Y + 2 - textSize.Y;

                    break;

                case BarTextPosition.BelowLeft:
                    textXPos = cursorPos.X + 2;
                    textYPos = cursorPos.Y + barHeight - 2;

                    break;

                case BarTextPosition.BelowMiddle:
                    textXPos = cursorPos.X + barWidth / 2f - textSize.X / 2f;
                    textYPos = cursorPos.Y + barHeight - 2;

                    break;

                case BarTextPosition.BelowRight:
                    textXPos = cursorPos.X + barWidth - textSize.X - 2;
                    textYPos = cursorPos.Y + barHeight - 2;

                    break;

                case BarTextPosition.LeftUpper:
                    textXPos = cursorPos.X - textSize.X - 2;
                    textYPos = cursorPos.Y - 2;

                    break;

                case BarTextPosition.RightUpper:
                    textXPos = cursorPos.X + barWidth + 2;
                    textYPos = cursorPos.Y - 2;

                    break;

                case BarTextPosition.LeftCenter:
                    textXPos = cursorPos.X - textSize.X - 2;
                    textYPos = cursorPos.Y + barHeight / 2f - textSize.Y / 2f;

                    break;

                case BarTextPosition.RightCenter:
                    textXPos = cursorPos.X + barWidth + 2;
                    textYPos = cursorPos.Y + barHeight / 2f - textSize.Y / 2f;

                    break;

                case BarTextPosition.LeftLower:
                    textXPos = cursorPos.X - textSize.X - 2;
                    textYPos = cursorPos.Y + barHeight - textSize.Y + 2;

                    break;

                case BarTextPosition.RightLower:
                    textXPos = cursorPos.X + barWidth + 2;
                    textYPos = cursorPos.Y + barHeight - textSize.Y + 2;

                    break;
            }

            return new Vector2(textXPos, textYPos);
        }
    }

    public enum BarTextMode
    {
        Single,
        EachChunk,
        None
    }

    public enum BarTextType
    {
        Current,
        Remaining,
        Maximum,
        Percentage,
        Custom
    }

    public enum BarTextPosition
    {
        // INSIDE
        TopLeft,
        TopMiddle,
        TopRight,
        CenterLeft,
        CenterMiddle,
        CenterRight,
        BottomLeft,
        BottomMiddle,
        BottomRight,

        // OUTSIDE
        AboveLeft,
        AboveMiddle,
        AboveRight,
        LeftUpper,
        RightUpper,
        LeftCenter,
        RightCenter,
        LeftLower,
        RightLower,
        BelowLeft,
        BelowMiddle,
        BelowRight
    }
}
