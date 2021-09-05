using System;
using System.Numerics;

namespace DelvUI.Helpers
{
    public static class LayoutHelper
    {
        // Calculates rows and columns. Used for status effect lists and party frames.
        public static void CalculateLayout(
            Vector2 maxSize,
            Vector2 itemSize,
            uint count,
            Vector2 padding,
            bool fillRowsFirst,
            out uint rowCount,
            out uint colCount
        )
        {

            CalculateLayout(maxSize, itemSize, count, (int)padding.X, (int)padding.Y, fillRowsFirst, out rowCount, out colCount);
        }

        public static void CalculateLayout(
            Vector2 maxSize,
            Vector2 itemSize,
            uint count,
            int horizontalPadding,
            int verticalPadding,
            bool fillRowsFirst,
            out uint rowCount,
            out uint colCount
        )
        {
            rowCount = 1;
            colCount = 1;

            if (maxSize.X < itemSize.X)
            {
                colCount = count;
                return;
            }

            if (maxSize.Y < itemSize.Y)
            {
                rowCount = count;
                return;
            }

            if (fillRowsFirst)
            {
                colCount = (uint)(maxSize.X / itemSize.X);

                if (itemSize.X * colCount + horizontalPadding * (colCount - 1) > maxSize.X)
                {
                    colCount = Math.Max(1, colCount - 1);
                }

                rowCount = (uint)Math.Ceiling((double)count / colCount);
            }
            else
            {
                rowCount = (uint)(maxSize.Y / itemSize.Y);

                if (itemSize.Y * rowCount + verticalPadding * (rowCount - 1) > maxSize.Y)
                {
                    rowCount = Math.Max(1, rowCount - 1);
                }

                colCount = (uint)Math.Ceiling((double)count / rowCount);
            }
        }
    }
}
