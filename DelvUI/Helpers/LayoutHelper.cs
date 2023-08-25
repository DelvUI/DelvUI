using System;
using System.Collections.Generic;
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
                    float remainingWidth = maxSize.X;
                    colCount = 0;
                    while (remainingWidth > 0)
                    {
                        remainingWidth -= (itemSize.X + padding.X);
                        colCount++;
                    }

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
                    float remainingHeight = maxSize.Y;
                    rowCount = 0;
                    while (remainingHeight > 0)
                    {
                        remainingHeight -= (itemSize.Y + padding.Y);
                        rowCount++;
                    }

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

            Vector2 contentSize = new Vector2(
                realColCount * itemSize.X + (realColCount - 1) * padding.X,
                realRowCount * itemSize.Y + (realRowCount - 1) * padding.Y
            );

            return new LayoutInfo(rowCount, colCount, realRowCount, realColCount, contentSize);
        }

        private static List<GrowthDirections> DirectionOptionsValues = new List<GrowthDirections>()
        {
            GrowthDirections.Right | GrowthDirections.Down,
            GrowthDirections.Right | GrowthDirections.Up,
            GrowthDirections.Left | GrowthDirections.Down,
            GrowthDirections.Left | GrowthDirections.Up,
            GrowthDirections.Centered | GrowthDirections.Up,
            GrowthDirections.Centered | GrowthDirections.Down,
            GrowthDirections.Centered | GrowthDirections.Left,
            GrowthDirections.Centered | GrowthDirections.Right
        };
        public static GrowthDirections GrowthDirectionsFromIndex(int index)
        {
            if (index > 0 && index < DirectionOptionsValues.Count)
            {
                return DirectionOptionsValues[index];
            }

            return DirectionOptionsValues[0];
        }

        public static int IndexFromGrowthDirections(GrowthDirections directions)
        {
            int index = DirectionOptionsValues.FindIndex(d => d == directions);

            return index > 0 ? index : 0;
        }

        public static bool GetFillsRowsFirst(bool fallback, GrowthDirections directions)
        {
            if ((directions & GrowthDirections.Centered) != 0)
            {
                if ((directions & GrowthDirections.Up) != 0 || (directions & GrowthDirections.Down) != 0)
                {
                    return true;
                }
                else if ((directions & GrowthDirections.Left) != 0 || (directions & GrowthDirections.Right) != 0)
                {
                    return false;
                }
            }

            return fallback;
        }

        public static void CalculateAxisDirections(
            GrowthDirections growthDirections,
            int row,
            int col, 
            uint elementCount,
            Vector2 size,
            Vector2 iconSize,
            Vector2 iconPadding,
            out Vector2 direction,
            out Vector2 offset)
        {
            if ((growthDirections & GrowthDirections.Centered) != 0)
            {
                if ((growthDirections & GrowthDirections.Up) != 0 || (growthDirections & GrowthDirections.Down) != 0)
                {
                    int elementsPerRow = (int)(size.X / (iconSize.X + iconPadding.X));
                    long elementsInRow = Math.Min(elementsPerRow, elementCount - (elementsPerRow * row));

                    direction.X = 1;
                    direction.Y = (growthDirections & GrowthDirections.Down) != 0 ? 1 : -1;
                    offset.X = -(iconSize.X + iconPadding.X) * elementsInRow / 2f;
                    offset.Y = direction.Y == 1 ? 0 : -iconSize.Y;
                }

                else// if ((growthDirections & GrowthDirections.Left) != 0 || (growthDirections & GrowthDirections.Right) != 0)
                {
                    int elementsPerCol = (int)(size.Y / (iconSize.Y + iconPadding.Y));
                    long elementsInCol = Math.Min(elementsPerCol, elementCount - (elementsPerCol * col));

                    direction.X = (growthDirections & GrowthDirections.Left) != 0 ? -1 : 1;
                    direction.Y = 1;
                    offset.X = direction.X == 1 ? 0 : -iconSize.X;
                    offset.Y = -(iconSize.Y + iconPadding.Y) * elementsInCol / 2f;
                }
            }
            else
            {
                direction.X = (growthDirections & GrowthDirections.Right) != 0 ? 1 : -1;
                direction.Y = (growthDirections & GrowthDirections.Down) != 0 ? 1 : -1;
                offset.X = direction.X == 1 ? 0 : -iconSize.X;
                offset.Y = direction.Y == 1 ? 0 : -iconSize.Y;
            }
        }

        public static Vector2 CalculateStartPosition(Vector2 position, Vector2 size, GrowthDirections growthDirections)
        {
            Vector2 area = size;
            if ((growthDirections & GrowthDirections.Left) != 0)
            {
                area.X = -area.X;
            }

            if ((growthDirections & GrowthDirections.Up) != 0)
            {
                area.Y = -area.Y;
            }

            Vector2 startPos = position;
            if ((growthDirections & GrowthDirections.Centered) != 0)
            {
                if ((growthDirections & GrowthDirections.Up) != 0 || (growthDirections & GrowthDirections.Down) != 0)
                {
                    startPos.X = position.X - size.X / 2f;
                }
                else if ((growthDirections & GrowthDirections.Left) != 0 || (growthDirections & GrowthDirections.Right) != 0)
                {
                    startPos.Y = position.Y - size.Y / 2f;
                }
            }

            Vector2 endPos = position + area;

            if (endPos.X < position.X)
            {
                startPos.X = endPos.X;
            }

            if (endPos.Y < position.Y)
            {
                startPos.Y = endPos.Y;
            }

            return startPos;
        }

        public static (List<Vector2>, Vector2, Vector2) CalculateIconPositions(
            GrowthDirections directions,
            uint count,
            Vector2 position,
            Vector2 size,
            Vector2 iconSize,
            Vector2 iconPadding,
            bool fillRowsFirst,
            LayoutInfo layoutInfo)
        {
            List<Vector2> list = new List<Vector2>();
            Vector2 minPos = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPos = Vector2.Zero;

            int row = 0;
            int col = 0;

            for (int i = 0; i < count; i++)
            {
                CalculateAxisDirections(
                    directions,
                    row,
                    col, 
                    count,
                    size,
                    iconSize,
                    iconPadding,
                    out Vector2 direction,
                    out Vector2 offset
                );

                Vector2 pos = new Vector2(
                    position.X + offset.X + iconSize.X * col * direction.X + iconPadding.X * col * direction.X,
                    position.Y + offset.Y + iconSize.Y * row * direction.Y + iconPadding.Y * row * direction.Y
                );

                minPos.X = Math.Min(pos.X, minPos.X);
                minPos.Y = Math.Min(pos.Y, minPos.Y);
                maxPos.X = Math.Max(pos.X + iconSize.X, maxPos.X);
                maxPos.Y = Math.Max(pos.Y + iconSize.Y, maxPos.Y);

                list.Add(pos);

                // rows / columns
                if (fillRowsFirst)
                {
                    col += 1;
                    if (col >= layoutInfo.TotalColCount)
                    {
                        col = 0;
                        row += 1;
                    }
                }
                else
                {
                    row += 1;
                    if (row >= layoutInfo.TotalRowCount)
                    {
                        row = 0;
                        col += 1;
                    }
                }
            }

            return (list, minPos, maxPos);
        }
    }

    [Flags]
    public enum GrowthDirections : short
    {
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        Centered = 16,
    }
}
