using System;
using System.Collections.Generic;

namespace DelvUI.Interface.Bars
{
    public class BarBuilder
    {
        private Bar _bar;

        private int currentInnerBar = -1;

        private BarBuilder(Bar initialBar)
        {
            _bar = initialBar;
        }

        public static BarBuilder Create(float xPosition, float yPosition, int height, int width)
        {
            var bar = new Bar(xPosition, yPosition, height, width);
            return new BarBuilder(bar);
        }

        public Bar Build() => _bar;

        public BarBuilder SetX(int xPos)
        {
            _bar.XPosition = xPos;
            return this;
        }

        public BarBuilder SetY(int yPos)
        {
            _bar.YPosition = yPos;
            return this;
        }

        public BarBuilder SetHeight(int height)
        {
            _bar.BarHeight = height;
            return this;
        }

        public BarBuilder SetWidth(int width)
        {
            _bar.BarWidth = width;
            return this;
        }

        public BarBuilder SetChunkPadding(int padding)
        {
            _bar.ChunkPadding = padding;
            return this;
        }

        public BarBuilder SetChunks(int count)
        {
            if (currentInnerBar != -1)
                throw new InvalidOperationException("You cannot set the chunk count once an inner bar has been added");
            var size = (float) 1 / count;
            float[] sizes = new float[count];
            for (var i = 0; i < count; i++)
            {
                sizes[i] = size;
            }

            _bar.ChunkSizes = sizes;
            return this;
        }

        public BarBuilder SetChunks(float[] sizes)
        {
            if (currentInnerBar != -1)
                throw new InvalidOperationException("You cannot set the chunk count once an inner bar has been added");
            _bar.ChunkSizes = sizes;
            return this;
        }

        public BarBuilder AddInnerBar(float currentValue, float maximumValue, Dictionary<string, uint> color)
        {
            return AddInnerBar(currentValue, maximumValue, color, null);
        }

        public BarBuilder AddInnerBar(float currentValue, float maximumValue, Dictionary<string, uint> color, Dictionary<string, uint> partialColor)
        {
            var colors = new Dictionary<string, uint>[_bar.ChunkSizes.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }

            return AddInnerBar(currentValue, maximumValue, colors, partialColor);
        }

        public BarBuilder AddInnerBar(float currentValue, float maximumValue, Dictionary<string, uint>[] colors)
        {
            return AddInnerBar(currentValue, maximumValue, colors, null);
        }

        public BarBuilder AddInnerBar(float currentValue, float maximumValue, Dictionary<string, uint>[] colors, Dictionary<string, uint> partialColor)
        {
            return AddInnerBar(currentValue, maximumValue, colors, partialColor, BarTextMode.None, null);
        }

        public BarBuilder AddInnerBar(float currentValue, float maximumValue, Dictionary<string, uint>[] chunkColors,
            Dictionary<string, uint> partialFillColor, BarTextMode textMode, BarText[] texts)
        {
            InnerBar innerBar = new InnerBar();
            innerBar.CurrentValue = currentValue;
            innerBar.MaximumValue = maximumValue;
            innerBar.ChunkColors = chunkColors;
            innerBar.PartialFillColor = partialFillColor; 
            innerBar.TextMode = textMode;
            innerBar.Texts = texts;

            if (chunkColors.Length != _bar.ChunkSizes.Length)
                throw new ArgumentException($"Amount of chunk colors (${chunkColors.Length}) must match amount of chunks in bar (${_bar.ChunkSizes.Length})");

            currentInnerBar = _bar.AddInnerBar(innerBar);
            return this;
        }

        public BarBuilder SetChunksColors(Dictionary<string, uint> color)
        {
            var colors = new Dictionary<string, uint>[_bar.ChunkSizes.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }

            return SetChunksColors(colors);
        }

        public BarBuilder SetChunksColors(Dictionary<string, uint>[] colors)
        {
            if (colors.Length != _bar.ChunkSizes.Length)
                throw new ArgumentException($"Amount of chunk colors (${colors.Length}) must match amount of chunks in bar (${_bar.ChunkSizes.Length})");
            _bar.InnerBars[currentInnerBar].ChunkColors = colors;
            return this;
        }

        public BarBuilder SetTextMode(BarTextMode mode)
        {
            _bar.InnerBars[currentInnerBar].TextMode = mode;
            return this;
        }

        public BarBuilder SetTexts(BarText text)
        {
            var texts = new BarText[_bar.ChunkSizes.Length];
            for (var i = 0; i < texts.Length; i++)
            {
                texts[i] = text;
            }

            _bar.InnerBars[currentInnerBar].Texts = texts;
            return this;
        }

        public BarBuilder SetTexts(BarText[] texts)
        {
            if (texts.Length != _bar.ChunkSizes.Length)
                throw new ArgumentException(
                    $"Amount of bar texts (${texts.Length}) must match amount of chunks in bar (${_bar.ChunkSizes.Length})");
            _bar.InnerBars[currentInnerBar].Texts = texts;
            return this;
        }
    }
}