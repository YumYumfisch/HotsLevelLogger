using System;
using System.Collections.Generic;
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
        public static Bitmap SeparateDigits(Bitmap source)
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
        /// Removes connected components with less than 10 pixels.
        /// </summary>
        /// <param name="bitmap">Binarized Bitmap (Black foreground, white background)</param>
        /// <returns>Denoised bitmap</returns>
        public static Bitmap ConnectedComponentAnalysis(Bitmap bitmap)
        {
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
                    if (x > 0 && bitmap.GetPixel(x - 1, y).ToArgb() == Color.Black.ToArgb())
                    {
                        neighbors.Add(new Point(x - 1, y));
                    }
                    if (y > 0 && bitmap.GetPixel(x, y - 1).ToArgb() == Color.Black.ToArgb())
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

#if DEBUG
            DebugPrint(pixelLabels);
#endif

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
                    equivalenceMapping.TryGetValue(pixelLabels[x, y], out labels);
                    pixelLabels[x, y] = labels.Min();
                }
            }

#if DEBUG
            DebugPrint(pixelLabels);
#endif

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
                if (labelCount < 15) // Components that are smaller than 15 pixels will be removed.
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

#if DEBUG
            DebugPrint(pixelLabels);
#endif

            return bitmap;
        }

#if DEBUG
        /// <summary>
        /// Debugging only: Prints a twodimensional int array.
        /// </summary>
        private static void DebugPrint(int[,] pixelLabels)
        {
            Console.WriteLine();
            for (int y = 0; y < pixelLabels.GetLength(1); y++)
            {
                for (int x = 0; x < pixelLabels.GetLength(0); x++)
                {
                    if (pixelLabels[x, y] == 0)
                    {
                        Console.Write("  ,");
                    }
                    else
                    {
                        Console.Write($"{pixelLabels[x, y],2},");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
#endif

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
