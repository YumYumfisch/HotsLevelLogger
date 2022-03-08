using System;
using System.Drawing;
using System.Linq;

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
        public static Bitmap ImageCleanup(Bitmap bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    if (ValidatePixelColor(pixelColor))
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
        private static bool ValidatePixelColor(Color color)
        {
            int saturation = (int)(color.GetSaturation() * 100);
            int brightness = (int)(color.GetBrightness() * 100);
            int hue = (int)color.GetHue();

            int[] validHues = { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 51, 52, 53, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 174, 175, 176, 177, 213, 214, 215, 290, 291, 292, 293, 294 };
            if (validHues.Contains(hue))
            {
                return saturation > 10 && brightness > 50 && brightness < 95;
            }
            else
            {
                return false;
            }
        }
    }
}
