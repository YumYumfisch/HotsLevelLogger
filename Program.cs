﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace Hots_Level_Logger
{
    public static class Program
    {
        /// <summary>
        /// Folder where the screenshots will be saved to.
        /// </summary>
        private static readonly string screenshotLevelFolder = $"C:{Path.DirectorySeparatorChar}Temp{Path.DirectorySeparatorChar}HotsLevelLogs";

        /// <summary>
        /// Folder where the screenshots will be saved to.
        /// </summary>
        private static readonly string screenshotPlayerFolder = $"C:{Path.DirectorySeparatorChar}Temp{Path.DirectorySeparatorChar}HotsPlayerLogs";

        private static readonly string TokenFile = $"C:{Path.DirectorySeparatorChar}Temp{Path.DirectorySeparatorChar}_token.txt";

        #region Border Pixel Position Constants
        /* Border Pixel Position Constants:
         * Pixel positions that can be combined to get the top-left corner and area size of the area to be screenshotted for each player.
         * Pixel positions are only valid on a 2560x1080p monitor.
         */

        // Level Area Size
        private static readonly Size LevelAreaSize = new Size(40, 15);
        // Level X positions
        private const int BorderPosLevelXLeft = 31;
        private const int BorderPosLevelXRight = 2462;
        // Level Y positions
        private const int BorderPosLevelY1 = 261;
        private const int BorderPosLevelY2 = 393;
        private const int BorderPosLevelY3 = 525;
        private const int BorderPosLevelY4 = 657;
        private const int BorderPosLevelY5 = 790;

        // Player Area Size
        private static readonly Size PlayerAreaSize = new Size(308, 113);
        // Player X positions
        private const int BorderPosPlayerXLeft = 8;
        private const int BorderPosPlayerXRight = 2245;
        // Player Y positions
        private const int BorderPosPlayerY1 = 190;
        private const int BorderPosPlayerY2 = 322;
        private const int BorderPosPlayerY3 = 454;
        private const int BorderPosPlayerY4 = 586;
        private const int BorderPosPlayerY5 = 719;
        #endregion Border Pixel Position Constants

        public static void Main(string[] args)
        {
            #region Console Setup
            Console.Title = "Hots Level Logger";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.OutputEncoding = Encoding.UTF8;
            #endregion Console Setup
            #region Discord Setup
            if (!File.Exists(TokenFile))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error 404: token.txt not found.");
                Console.ReadLine();
                return;
            }

            _ = Discord.Init(928355348123885588, File.ReadAllText(TokenFile).Trim());

            while (!Discord.IsReady())
            {
                Thread.Sleep(10);
            }

            Thread.Sleep(10);
            Console.WriteLine();
            #endregion Discord Setup
            Console.WriteLine("Setup Complete.");

            #region Screenshot Areas
            List<Rectangle> levelAreas = new List<Rectangle> {
                new Rectangle(new Point(BorderPosLevelXLeft, BorderPosLevelY1), LevelAreaSize),
                new Rectangle(new Point(BorderPosLevelXLeft, BorderPosLevelY2), LevelAreaSize),
                new Rectangle(new Point(BorderPosLevelXLeft, BorderPosLevelY3), LevelAreaSize),
                new Rectangle(new Point(BorderPosLevelXLeft, BorderPosLevelY4), LevelAreaSize),
                new Rectangle(new Point(BorderPosLevelXLeft, BorderPosLevelY5), LevelAreaSize),
                new Rectangle(new Point(BorderPosLevelXRight, BorderPosLevelY1), LevelAreaSize),
                new Rectangle(new Point(BorderPosLevelXRight, BorderPosLevelY2), LevelAreaSize),
                new Rectangle(new Point(BorderPosLevelXRight, BorderPosLevelY3), LevelAreaSize),
                new Rectangle(new Point(BorderPosLevelXRight, BorderPosLevelY4), LevelAreaSize),
                new Rectangle(new Point(BorderPosLevelXRight, BorderPosLevelY5), LevelAreaSize)
            };

            List<Rectangle> PlayerAreas = new List<Rectangle> {
                new Rectangle(new Point(BorderPosPlayerXLeft, BorderPosPlayerY1), PlayerAreaSize),
                new Rectangle(new Point(BorderPosPlayerXLeft, BorderPosPlayerY2), PlayerAreaSize),
                new Rectangle(new Point(BorderPosPlayerXLeft, BorderPosPlayerY3), PlayerAreaSize),
                new Rectangle(new Point(BorderPosPlayerXLeft, BorderPosPlayerY4), PlayerAreaSize),
                new Rectangle(new Point(BorderPosPlayerXLeft, BorderPosPlayerY5), PlayerAreaSize),
                new Rectangle(new Point(BorderPosPlayerXRight, BorderPosPlayerY1), PlayerAreaSize),
                new Rectangle(new Point(BorderPosPlayerXRight, BorderPosPlayerY2), PlayerAreaSize),
                new Rectangle(new Point(BorderPosPlayerXRight, BorderPosPlayerY3), PlayerAreaSize),
                new Rectangle(new Point(BorderPosPlayerXRight, BorderPosPlayerY4), PlayerAreaSize),
                new Rectangle(new Point(BorderPosPlayerXRight, BorderPosPlayerY5), PlayerAreaSize)
            };
            #endregion Screenshot Areas

            while (true)
            {
                Console.WriteLine("Press [Enter] to analyze the screen.");
                Console.ReadLine();

                Console.WriteLine("Capturing...");
                if (!Directory.Exists(screenshotLevelFolder))
                {
                    Directory.CreateDirectory(screenshotLevelFolder);
                }
                if (!Directory.Exists(screenshotPlayerFolder))
                {
                    Directory.CreateDirectory(screenshotPlayerFolder);
                }

                int[] levels = new int[levelAreas.Count];
                int sum = 0;
                int max = 0;
                int min = 0;
                Console.Write("Levels: {");
                string discordMessage = "```h\r\nLevels: {";
                for (int i = 0; i < levelAreas.Count; i++)
                {
                    Bitmap LevelCaptureBmp = ScreenCapture.CaptureScreen(levelAreas[i]);
                    Bitmap LevelProcessedBmp = ImageManipulation.ConnectedComponentAnalysis(ImageManipulation.SeparateDigits(LevelCaptureBmp));
                    levels[i] = OpticalCharacterRecognition.GetNumber(LevelProcessedBmp, out _);
                    string filename = $"{levels[i].ToString().PadLeft(4, '0')}_{DateTime.UtcNow.ToString("yyyy.MM.dd-HH.mm.ss.fff")}.png";
                    LevelCaptureBmp.Save($"{screenshotLevelFolder}{Path.DirectorySeparatorChar}{filename}");
                    ScreenCapture.CaptureScreen(PlayerAreas[i]).Save($"{screenshotPlayerFolder}{Path.DirectorySeparatorChar}{filename}");

                    sum += levels[i];

                    if (levels[i] > max)
                    {
                        max = levels[i];
                    }
                    if (min == 0)
                    {
                        min = levels[i];
                    }
                    else if (levels[i] != 0 && levels[i] < min)
                    {
                        min = levels[i];
                    }

                    if (i == levelAreas.Count - 1)
                    {
                        Console.WriteLine(levels[i] + "}");
                        discordMessage += levels[i] + "}\r\n";
                    }
                    else
                    {
                        Console.Write($"{levels[i]}, ");
                        discordMessage += $"{levels[i]}, ";
                    }
                }
                string stats = $"\r\nHighest level = {max}\r\nLowest  level = {min}\r\nAverage level = {sum / levelAreas.Count}";
                Console.WriteLine(stats);
                discordMessage += $"{stats}\r\n```";
                Discord.Log(discordMessage);
                Console.WriteLine();
                Console.WriteLine($"Saved captures at '{screenshotLevelFolder}'.");
                Console.WriteLine();
            }
        }
    }
}
