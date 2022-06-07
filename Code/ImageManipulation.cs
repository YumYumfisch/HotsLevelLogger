using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Handles image manipulation to preprocess them for OCR.
    /// </summary>
    public static class ImageManipulation
    {
        /// <summary>
        /// Processes an image of a player level to prepare it for OCR.
        /// Binarizes and denoises the image and adds spacing between digits.
        /// </summary>
        /// <param name="source">Image to be processed.</param>
        /// <returns>Processed image with black digits on white background.</returns>
        public static Bitmap PrepareImage(Bitmap source)
        {
#if DEBUG
            return Binarize(ConnectedComponentAnalysis(CompareDigitColors(ConnectedComponentAnalysis(ValidatePixelColor(SeparateDigits(source))))));
#else
            return Binarize(ConnectedComponentAnalysis(CompareDigitColors(ConnectedComponentAnalysis(ValidatePixelColor(SeparateDigits(source))))));
#endif
        }

#if DEBUG
        /// <summary>
        /// Add additional padding around a bitmap.
        /// </summary>
        /// <param name="source">Bitmap to be padded</param>
        /// <returns>Padded bitmap</returns>
        private static Bitmap ExtraPadding(Bitmap source)
        {
            Bitmap output = new Bitmap(source.Width + 80, source.Height + 60);
            for (int x = 0; x < output.Width; x++)
            {
                for (int y = 0; y < output.Height; y++)
                {
                    output.SetPixel(x, y, Color.White);
                }
            }

            Graphics graphics = Graphics.FromImage(output);
            graphics.DrawImage(source, 40, 30, source.Width, source.Height);

            return output;
        }

        /// <summary>
        /// Changes every black pixel to white and every white pixel to black.
        /// Ignores pixels with different colors
        /// </summary>
        /// <param name="source">Bitmap to be inverted.</param>
        /// <returns>Inverted bitmap</returns>
        private static Bitmap Invert(Bitmap source)
        {
            Bitmap output = new Bitmap(source.Width, source.Height);
            for (int x = 0; x < output.Width; x++)
            {
                for (int y = 0; y < output.Height; y++)
                {
                    if (source.GetPixel(x, y).ToArgb() == Color.White.ToArgb())
                    {
                        output.SetPixel(x, y, Color.Black);
                    }
                    else if (source.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                    {
                        output.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        output.SetPixel(x, y, source.GetPixel(x, y));
                    }
                }
            }

            return output;
        }
#endif

        /// <summary>
        /// Compares Hue value of pixels to filter parts from profile pictures that have a different color than the digits.
        /// </summary>
        /// <param name="source">Bitmap with colorfull digits and noise on white background.</param>
        /// <returns>Bitmap with colorfull digits on white background.</returns>
        private static Bitmap CompareDigitColors(Bitmap source)
        {
            int hueRange = 15; // Determines the amount that the hue can differ from the average hue of the first digit

            // Get average hue of columns 3 to 12
            double averageHue = 0;
            int pixelCounter = 0;
            for (int x = 3; x < 13; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    // Ignore background
                    if (source.GetPixel(x, y).ToArgb() == Color.White.ToArgb())
                    {
                        continue;
                    }

                    averageHue += source.GetPixel(x, y).GetHue();
                    pixelCounter++;
                }
            }
            averageHue /= pixelCounter;

            // Remove hues that differ from the average
            Bitmap output = new Bitmap(source.Width, source.Height);
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    // Ignore background
                    if (source.GetPixel(x, y).ToArgb() == Color.White.ToArgb())
                    {
                        output.SetPixel(x, y, Color.White);
                        continue;
                    }

                    float hue = source.GetPixel(x, y).GetHue();
                    if (hue < averageHue - hueRange || hue > averageHue + hueRange)
                    {
                        output.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        output.SetPixel(x, y, source.GetPixel(x, y));
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Changes every pixel of the bitmap to be black except if it is white.
        /// </summary>
        /// <param name="source">Bitmap to be binarized.</param>
        /// <returns>Binarized bitmap.</returns>
        private static Bitmap Binarize(Bitmap source)
        {
            Bitmap output = new Bitmap(source.Width, source.Height);

            for (int x = 0; x < output.Width; x++)
            {
                for (int y = 0; y < output.Height; y++)
                {
                    if (source.GetPixel(x, y).ToArgb() == Color.White.ToArgb())
                    {
                        output.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        output.SetPixel(x, y, Color.Black);
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Sets every pixel to black if it has a valid color.
        /// Otherwise it will be set to white.
        /// Adds additional padding between digits.
        /// </summary>
        /// <param name="source">Image to be processed.</param>
        /// <returns>Binarized (Black and White) bitmap with numbers in black on a white background.</returns>
        private static Bitmap SeparateDigits(Bitmap source)
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

                    output.SetPixel(x, y, source.GetPixel(x - pixelShift, y));
                }
            }
            return output;
        }

        /// <summary>
        /// Sets every pixel to white that has an invalid color.
        /// </summary>
        /// <param name="source">Bitmap to be processed.</param>
        /// <returns>Validated Bitmap.</returns>
        private static Bitmap ValidatePixelColor(Bitmap source)
        {
            Bitmap output = new Bitmap(source.Width, source.Height);
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    // Remove invalid pixels in the area of the digit
                    if (ValidatePixelColor(source.GetPixel(x, y)))
                    {
                        output.SetPixel(x, y, source.GetPixel(x, y));
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

        /// <summary>
        /// Removes connected components that are too small to be a digit.
        /// </summary>
        /// <param name="bitmap">Bitmap with white background.</param>
        /// <returns>Denoised bitmap.</returns>
        private static Bitmap ConnectedComponentAnalysis(Bitmap bitmap)
        {
            int minimumComponentPixelSize = 22; // The smallest digit (1) has 29 pixels

            int nextLabel = 1;
            int[,] pixelLabels = new int[bitmap.Width, bitmap.Height];
            Dictionary<int, List<int>> equivalenceMapping = new Dictionary<int, List<int>>();

            // First pass
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    // Ignore Background
                    if (bitmap.GetPixel(x, y).ToArgb() == Color.White.ToArgb())
                    {
                        pixelLabels[x, y] = 0;
                        continue;
                    }

                    List<Point> neighbors = new List<Point>();
                    if (x > 0 && bitmap.GetPixel(x - 1, y).ToArgb() != Color.White.ToArgb())
                    {
                        neighbors.Add(new Point(x - 1, y));
                    }
                    if (y > 0 && bitmap.GetPixel(x, y - 1).ToArgb() != Color.White.ToArgb())
                    {
                        neighbors.Add(new Point(x, y - 1));
                    }

                    if (neighbors.Count == 0)
                    {
                        List<int> list = new List<int> { nextLabel };
                        equivalenceMapping.Add(nextLabel, list);

                        pixelLabels[x, y] = nextLabel;
                        nextLabel++;
                    }
                    else
                    {
                        List<int> neighborLabels = new List<int>();
                        foreach (Point neighbor in neighbors)
                        {
                            neighborLabels.Add(pixelLabels[neighbor.X, neighbor.Y]);
                        }
                        pixelLabels[x, y] = neighborLabels.Min();

                        // Store equivalences
                        foreach (int neighborLabel in neighborLabels)
                        {
                            List<int> unitedList;
                            if (!equivalenceMapping.TryGetValue(neighborLabel, out unitedList))
                            {
                                throw new Exception($"Cannot get key {neighborLabel} from equivalences.");
                            }
                            foreach (int internalNeighborLabel in neighborLabels)
                            {
                                if (!unitedList.Contains(internalNeighborLabel))
                                {
                                    unitedList.Add(internalNeighborLabel);
                                }
                            }
                            equivalenceMapping[neighborLabel] = unitedList;
                        }
                    }
                }
            }

            // Complete equivalence mapping
            bool done = false;
            while (!done)
            {
                done = true;
                for (int key = 1; key < equivalenceMapping.Count + 1; key++)
                {
                    List<int> values = equivalenceMapping[key];

                    foreach (List<int> internalValues in equivalenceMapping.Values)
                    {
                        if (internalValues.Contains(key))
                        {
                            foreach (int value in internalValues)
                            {
                                if (!values.Contains(value))
                                {
                                    values.Add(value);
                                    done = false;
                                }
                            }
                        }
                    }

                    equivalenceMapping[key] = values;
                }
            }

            // Second pass
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    // Ignore Background
                    if (bitmap.GetPixel(x, y).ToArgb() == Color.White.ToArgb())
                    {
                        continue;
                    }

                    List<int> labels;
                    _ = equivalenceMapping.TryGetValue(pixelLabels[x, y], out labels);
                    pixelLabels[x, y] = labels.Min();
                }
            }

            // Remove small components
            List<int> smallComponentLabels = new List<int>();
            for (int i = 0; i < nextLabel; i++)
            {
                int labelCount = 0;
                foreach (int label in pixelLabels)
                {
                    if (label == i)
                    {
                        labelCount++;
                    }
                }
                if (labelCount < minimumComponentPixelSize)
                {
                    smallComponentLabels.Add(i);
                }
            }

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    if (smallComponentLabels.Contains(pixelLabels[x, y]))
                    {
                        pixelLabels[x, y] = 0;
                        bitmap.SetPixel(x, y, Color.White);
                    }
                }
            }
            return bitmap;
        }
    }
}
