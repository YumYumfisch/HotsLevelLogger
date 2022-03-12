﻿using System;
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
        /// Adds additional padding between digits.
        /// </summary>
        /// <param name="source">Image to be manipulated.</param>
        /// <returns>Binarized (Black and White) bitmap with numbers in black on a white background.</returns>
        public static Bitmap ImageCleanup(Bitmap source)
        {
            Bitmap output = new Bitmap(source.Width + 8, source.Height);

            for (int x = 0; x < output.Width; x++)
            {
                for (int y = 0; y < output.Height; y++)
                {
                    // Remove border (2 lines each at the top and bottom and 3 rows each at the left and right)
                    if (y < 2 || y >= output.Height - 2 || x < 3 || x >= output.Width - 3)
                    {
                        output.SetPixel(x, y, Color.White);
                        continue;
                    }

                    // Add extra padding between digits
                    if ((x - 3) % 11 >= 9)
                    {
                        output.SetPixel(x, y, Color.White);
                        continue;
                    }

                    // Add digits
                    int digitIndex = (x - 3) / 11; // Ignoring border padding and accounting for digit width (7p plus one pixel of extra space each for error corection and one for padding on each side)
                    int pixelShift = digitIndex * 2 + 1; // Accounting for extra padding between digits
                    // Remove invalid pixels in the area of the digit
                    if (ValidatePixelColor(source.GetPixel(x - pixelShift, y)))
                    {
                        output.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        output.SetPixel(x, y, Color.White);
                    }
                }
            }
            return output;
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
