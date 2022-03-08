using System;
using System.Drawing;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Handles image manipulation to improve them for OCR.
    /// </summary>
    public static class ImageManipulation
    {
        /// <summary>
        /// Sets every pixel to black if it has a valid color.
        /// Otherwise it will be set to white.
        /// </summary>
        /// <param name="bitmap">Image to be manipulated.</param>
        /// <returns>Binarized (Black and White) bitmap with numbers in black on a white background.</returns>
        public static Bitmap StripImage(Bitmap bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    if (ValidColor(pixelColor))
                    {
                        bitmap.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        bitmap.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Determines whether a color is a color that is used for displaying numbers.
        /// </summary>
        /// <param name="color">Color to be validated.</param>
        private static bool ValidColor(Color color)
        {
            // TODO
            return new Random().Next(0, 2) == 0;
        }
    }
}
