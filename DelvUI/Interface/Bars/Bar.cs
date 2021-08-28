using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin;
using ImGuiNET;

namespace DelvUI.Interface.Bars
{
    public class Bar
    {
        // TODO: Text on main bar
        public List<InnerBar> InnerBars { get; set; }
        public float XPosition { get; set; }
        public float YPosition { get; set; }
        public int BarHeight { get; set; }
        public int BarWidth { get; set; }
        public int ChunkPadding { get; set; }
        public float[] ChunkSizes { get; set; }

        public Bar(float xPosition, float yPosition, int height, int width)
        {
            XPosition = xPosition;
            YPosition = yPosition;
            BarHeight = height;
            BarWidth = width;
            InnerBars = new List<InnerBar>();
            ChunkPadding = 0;
            ChunkSizes = new[] {1f};
        }

        public int AddInnerBar(InnerBar innerBar)
        {
            innerBar.Parent = this;
            innerBar.ChildNum = InnerBars.Count;
            InnerBars.Add(innerBar);
            return innerBar.ChildNum;
        }

        public void Draw(ImDrawListPtr drawList)
        {
            var barWidth = BarWidth + ChunkPadding; // For loop adds one extra padding more than is needed
            var cursorPos = new Vector2(XPosition, YPosition);
            
            foreach (var chunkSize in ChunkSizes)
            {
                var barSize = new Vector2(barWidth * chunkSize - ChunkPadding, BarHeight);
                
                drawList.AddRectFilled(cursorPos, cursorPos + barSize, 0x88000000);
                
                cursorPos += new Vector2(barWidth * chunkSize, 0);
            }

            foreach (var innerBar in InnerBars)
            {
                innerBar.Draw(drawList);
            }
            
            foreach (var innerBar in InnerBars)
            {
                innerBar.DrawText(drawList);
            }
            
            cursorPos = new Vector2(XPosition, YPosition);
            
            foreach (var chunkSize in ChunkSizes)
            {
                var barSize = new Vector2(barWidth * chunkSize - ChunkPadding, BarHeight);
                
                drawList.AddRect(cursorPos, cursorPos + barSize, 0xFF000000);
                
                cursorPos += new Vector2(barWidth * chunkSize, 0);
            }
        }
    }
    
    public class InnerBar
    {
        public Bar Parent { get; set; }
        public int ChildNum { get; set; }
        public float MaximumValue { get; set; }
        public float CurrentValue { get; set; }
        public Dictionary<string, uint>[] ChunkColors { get; set; }
        public Dictionary<string, uint> PartialFillColor { get; set; }
        public BarTextMode TextMode { get; set; }
        public BarText[] Texts { get; set; }

        public void Draw(ImDrawListPtr drawList)
        {
            var barWidth = Parent.BarWidth + Parent.ChunkPadding; // For loop adds one extra padding more than is needed
            var barHeight = (float) 1 / Parent.InnerBars.Count * Parent.BarHeight;
            var yPos = Parent.YPosition + (float) ChildNum / Parent.InnerBars.Count * Parent.BarHeight;
            var cursorPos = new Vector2(Parent.XPosition, yPos);

            var currentFill = CurrentValue / MaximumValue;
            var i = 0;
            foreach (var chunkSize in Parent.ChunkSizes)
            {
                var barSize = new Vector2(barWidth * chunkSize - Parent.ChunkPadding, barHeight);
                var fillPercentage = (float) Math.Round(Math.Min(currentFill / chunkSize, 1f), 5); // Rounding due to floating point precision shenanigans

                if (fillPercentage >= 1f)
                {
                    currentFill -= chunkSize;
                    drawList.AddRectFilledMultiColor(
                        cursorPos, cursorPos + barSize,
                        ChunkColors[i]["gradientLeft"], ChunkColors[i]["gradientRight"], ChunkColors[i]["gradientRight"], ChunkColors[i]["gradientLeft"]
                    );
                }
                else
                {
                    currentFill = 0f;
                    if (PartialFillColor != null)
                    {
                        drawList.AddRectFilledMultiColor(
                            cursorPos, cursorPos + new Vector2(barSize.X * fillPercentage, barSize.Y),
                            PartialFillColor["gradientLeft"], PartialFillColor["gradientRight"], PartialFillColor["gradientRight"], PartialFillColor["gradientLeft"]
                        );
                    }
                    else
                    {
                        drawList.AddRectFilledMultiColor(
                            cursorPos, cursorPos + new Vector2(barSize.X * fillPercentage, barSize.Y),
                            ChunkColors[i]["gradientLeft"], ChunkColors[i]["gradientRight"], ChunkColors[i]["gradientRight"], ChunkColors[i]["gradientLeft"]
                        );
                    }
                }

                i++;
                cursorPos += new Vector2(barWidth * chunkSize, 0);
            }
        }

        public void DrawText(ImDrawListPtr drawList)
        {
            var barWidth = Parent.BarWidth + Parent.ChunkPadding; // For loop adds one extra padding more than is needed
            var barHeight = (float) 1 / Parent.InnerBars.Count * Parent.BarHeight;
            var cursorPos = new Vector2(Parent.XPosition, Parent.YPosition);

            if (TextMode == BarTextMode.Single)
            {
                var textObj = Texts[0];
                var text = textObj.Type switch
                {
                    BarTextType.Current => Math.Round(CurrentValue).ToString(CultureInfo.InvariantCulture),
                    BarTextType.Maximum => Math.Round(MaximumValue).ToString(CultureInfo.InvariantCulture),
                    BarTextType.Percentage => Math.Round(CurrentValue / MaximumValue * 100f).ToString(CultureInfo.InvariantCulture),
                    BarTextType.Custom => textObj.Text,
                    _ => "ERROR LOADING TEXT, INVALID TYPE"
                };
                
                var textPos = textObj.CalcTextPosition(cursorPos, text, Parent.BarWidth, Parent.BarHeight);
                    
                DrawHelper.DrawOutlinedText(text, textPos, textObj.Color, textObj.OutlineColor);
            }

            var currentFill = CurrentValue / MaximumValue;
            var i = 0;
            foreach (var chunkSize in Parent.ChunkSizes)
            {
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

                if (TextMode == BarTextMode.EachChunk)
                {
                    var textObj = Texts[i];
                    var text = textObj.Type switch
                    {
                        BarTextType.Current => Math.Round(barValue).ToString(CultureInfo.InvariantCulture),
                        BarTextType.Maximum => Math.Round(barMaximum).ToString(CultureInfo.InvariantCulture),
                        BarTextType.Percentage => Math.Round(currentFill * 100f).ToString(CultureInfo.InvariantCulture),
                        BarTextType.Custom => textObj.Text,
                        _ => "ERROR LOADING TEXT, INVALID TYPE"
                    };

                    var textPos = textObj.CalcTextPosition(cursorPos, text, barSize.X, Parent.BarHeight);
                    
                    DrawHelper.DrawOutlinedText(text, textPos, textObj.Color, textObj.OutlineColor);
                }

                i++;
                cursorPos += new Vector2(barWidth * chunkSize, 0);
            }
        }
    }

    public class BooleanInnerBar : InnerBar
    {
        // TODO: Bar where any chunk can be filled without an order needed, for SAM stickers and other such bars
    }

    public class BarText
    {
        // TODO: Proper text tags
        public BarTextPosition Position { get; set; }
        public BarTextType Type { get; set; }
        public Vector4 Color { get; set; }
        public Vector4 OutlineColor { get; set; }
        public string Text { get; set; }

        public BarText(BarTextPosition position, BarTextType type, Vector4 color, Vector4 outlineColor, string text)
        {
            Position = position;
            Type = type;
            Color = color;
            OutlineColor = outlineColor;
            Text = text;
        }

        public BarText(BarTextPosition position, BarTextType type, string text)
        {
            Position = position;
            Type = type;
            Text = text;
            Color = Vector4.One;
            OutlineColor = new Vector4(0f, 0f, 0f, 1f);
        }

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
                    textXPos = cursorPos.X - textSize.X -  2;
                    textYPos = cursorPos.Y + barHeight / 2f - textSize.Y / 2f;
                    break;
                case BarTextPosition.RightCenter:
                    textXPos = cursorPos.X + barWidth + 2;
                    textYPos = cursorPos.Y + barHeight / 2f - textSize.Y / 2f;
                    break;
                case BarTextPosition.LeftLower:
                    textXPos = cursorPos.X  - textSize.X -  2;
                    textYPos = cursorPos.Y + barHeight - textSize.Y + 2;
                    break;
                case BarTextPosition.RightLower:
                    textXPos = cursorPos.X + barWidth  + 2;
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
        Maximum,
        Percentage,
        Custom
    }

    public enum BarTextPosition
    {
        // INSIDE
        TopLeft, TopMiddle, TopRight,
        CenterLeft, CenterMiddle, CenterRight,
        BottomLeft, BottomMiddle, BottomRight,
        
        // OUTSIDE
        AboveLeft, AboveMiddle, AboveRight,
        LeftUpper, RightUpper,
        LeftCenter, RightCenter,
        LeftLower, RightLower,
        BelowLeft, BelowMiddle, BelowRight
    }
}