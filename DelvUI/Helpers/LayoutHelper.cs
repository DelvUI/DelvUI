using System;
using System.Numerics;

namespace DelvUI.Helpers
{
    public struct LayoutInfo
    {
        public readonly uint TotalRowCount;
        public readonly uint TotalColCount;
        public readonly uint RealRowCount;
        public readonly uint RealColCount;
        public readonly Vector2 ContentSize;

        public LayoutInfo(uint totalRowCount, uint totalColCount, uint realRowCount, uint realColCount, Vector2 contentSize)
        {
            TotalRowCount = totalRowCount;
            TotalColCount = totalColCount;
            RealRowCount = realRowCount;
            RealColCount = realColCount;
            ContentSize = contentSize;
        }

        public LayoutInfo(uint rows, uint columns, Vector2 contentSize)
        {
            TotalRowCount = rows;
            TotalColCount = columns;
            RealRowCount = rows;
            RealColCount = columns;
            ContentSize = contentSize;
        }
    }

    public static class LayoutHelper
    {
        // Calculates rows and columns. Used for status effect lists and party frames.
        public static LayoutInfo CalculateLayout(
            Vector2 maxSize,
            Vector2 itemSize,
            uint count,
            Vector2 padding,
            bool fillRowsFirst
        )
        {
            uint rowCount = 1;
            uint colCount = 1;
            uint realRowCount = 1;
            uint realColCount = 1;

            if (maxSize.X < itemSize.X)
            {
                colCount = count;
                realColCount = count;
            }
            else if (maxSize.Y < itemSize.Y)
            {
                rowCount = count;
                realRowCount = count;
            }
            else
            {
                if (fillRowsFirst)
                {
                    colCount = (uint)(maxSize.X / itemSize.X);

                    if (itemSize.X * colCount + padding.X * (colCount - 1) > maxSize.X)
                    {
                        colCount = Math.Max(1, colCount - 1);
                    }

                    rowCount = (uint)Math.Ceiling((double)count / colCount);

                    int remaining = (int)(count - colCount);
                    while (remaining > 0)
                    {
                        realRowCount++;
                        remaining -= (int)colCount;
                    }

                    realColCount = Math.Min(count, colCount);
                }
                else
                {
                    rowCount = (uint)(maxSize.Y / itemSize.Y);

                    if (itemSize.Y * rowCount + padding.Y * (rowCount - 1) > maxSize.Y)
                    {
                        rowCount = Math.Max(1, rowCount - 1);
                    }

                    colCount = (uint)Math.Ceiling((double)count / rowCount);

                    int remaining = (int)(count - rowCount);
                    while (remaining > 0)
                    {
                        realColCount++;
                        remaining -= (int)rowCount;
                    }

                    realRowCount = Math.Min(count, rowCount);
                }
            }

            var contentSize = new Vector2(
                realColCount * itemSize.X + (realColCount - 1) * padding.X,
                realRowCount * itemSize.Y + (realRowCount - 1) * padding.Y
            );

            return new LayoutInfo(rowCount, colCount, realRowCount, realColCount, contentSize);
        }
    }
}
