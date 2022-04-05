﻿using Discord;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
#if !DEBUG
using System.Threading;
#else
using System.Diagnostics;
#endif

namespace Hots_Level_Logger
{
    public static class Program
    {
        /// <summary>
        /// Folder where the number screenshots will be saved to.
        /// </summary>
        private static readonly string screenshotLevelFolder = $"E:{Path.DirectorySeparatorChar}HotsLevelLogger{Path.DirectorySeparatorChar}LevelLogs";

#if DEBUG
        /// <summary>
        /// Folder where the preprocessed number screenshots will be saved to.
        /// </summary>
        private static readonly string screenshotLevelDebugginFolder = $"E:{Path.DirectorySeparatorChar}HotsLevelLogger{Path.DirectorySeparatorChar}DebugLevelLogs";
#else
        /// <summary>
        /// Folder where the player screenshots will be saved to.
        /// </summary>
        private static readonly string screenshotPlayerFolder = $"E:{Path.DirectorySeparatorChar}HotsLevelLogger{Path.DirectorySeparatorChar}PlayerLogs";

        /// <summary>
        /// Path to the text file that stores the token of the discord bot and the discord channel.
        /// </summary>
        private static readonly string DiscordConfig = $"E:{Path.DirectorySeparatorChar}HotsLevelLogger{Path.DirectorySeparatorChar}DiscordConfig.txt";

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
#endif

        /// <summary>
        /// Entry point for the application.
        /// </summary>
        /// <param name="args">Discord Config. Index 0: Token, Index 1: ChannelID</param>
        public static void Main(string[] args)
        {
            #region Console Setup
            Console.Title = "Hots Level Logger";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.OutputEncoding = Encoding.UTF8;
            #endregion Console Setup

#if !DEBUG
            #region Discord Setup
            if (!File.Exists(DiscordConfig))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error 404: DiscordConfig.txt not found.");
                Console.ReadLine();
                return;
            }

            if (args == null || args.Length == 0)
            {
                args = File.ReadAllLines(DiscordConfig);
            }
            _ = Discord.Init(args[0].Split(';')[0].Trim(), ulong.Parse(args[1].Split(';')[0].Trim()));

            while (!Discord.IsReady())
            {
                Thread.Sleep(10);
            }

            Thread.Sleep(10);
            Console.WriteLine();
            #endregion Discord Setup
#endif
            Console.WriteLine("Setup Complete.");

#if DEBUG
            DebugOCR();
#else
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
                Console.WriteLine();

                if (!Directory.Exists(screenshotLevelFolder))
                {
                    Directory.CreateDirectory(screenshotLevelFolder);
                }
                if (!Directory.Exists(screenshotPlayerFolder))
                {
                    Directory.CreateDirectory(screenshotPlayerFolder);
                }

                int[] levels = new int[levelAreas.Count];

                Console.Write("Levels: {");
                List<FileAttachment> files = new List<FileAttachment>();
                for (int i = 0; i < levelAreas.Count; i++)
                {
                    // Capture and analyze screen
                    Bitmap LevelCaptureBmp = ScreenCapture.CaptureScreen(levelAreas[i]);
                    Bitmap LevelProcessedBmp = ImageManipulation.PrepareImage(LevelCaptureBmp);
                    levels[i] = OpticalCharacterRecognition.GetNumber(LevelProcessedBmp, out _);

                    // Save files
                    string filename = $"{levels[i].ToString().PadLeft(4, '0')}_{DateTime.UtcNow.ToString("yyyy.MM.dd-HH.mm.ss.fff")}.png";
                    LevelCaptureBmp.Save($"{screenshotLevelFolder}{Path.DirectorySeparatorChar}{filename}");
                    ScreenCapture.CaptureScreen(PlayerAreas[i]).Save($"{screenshotPlayerFolder}{Path.DirectorySeparatorChar}{filename}");

                    // Log funny numbers
                    if (IsFunnyNumber(levels[i]))
                    {
                        files.Add(new FileAttachment($"{screenshotPlayerFolder}{Path.DirectorySeparatorChar}{filename}", description: levels[i].ToString()));
                    }

                    // Log Levels during analysis
                    if (i == levelAreas.Count - 1)
                    {
                        Console.WriteLine(levels[i] + "}");
                    }
                    else
                    {
                        Console.Write($"{levels[i]}, ");
                    }
                }

                Console.WriteLine();
                Console.WriteLine($"Saved captures at '{screenshotPlayerFolder}'.");
                Console.WriteLine();

                LogMessage(levels, files);
            }
#endif
        }

#if !DEBUG
        /// <summary>
        /// Logs the levels in discord and sends the provided files.
        /// </summary>
        /// <param name="levels">Unsorted array of levels.</param>
        /// <param name="files">Files to be sent with the message.</param>
        private static void LogMessage(int[] levels, IEnumerable<FileAttachment> files)
        {
            int[] levelsLeft = { levels[0], levels[1], levels[2], levels[3], levels[4] };
            int[] levelsRight = { levels[5], levels[6], levels[7], levels[8], levels[9] };

            Array.Sort(levels);
            Array.Sort(levelsLeft);
            Array.Sort(levelsRight);

            int avgLeft = 0;
            int avgGame = 0;
            int avgRight = 0;

            foreach (int level in levelsLeft)
            {
                avgLeft += level;
            }
            foreach (int level in levels)
            {
                avgGame += level;
            }
            foreach (int level in levelsRight)
            {
                avgRight += level;
            }

            avgLeft /= levelsLeft.Length;
            avgGame /= levels.Length;
            avgRight /= levelsRight.Length;

            string hl = levelsLeft[^1].ToString().PadLeft(4);
            string hg = levels[^1].ToString().PadLeft(4);
            string hr = levelsRight[^1].ToString().PadLeft(4);
            string al = avgLeft.ToString().PadLeft(4);
            string ag = avgGame.ToString().PadLeft(4); ;
            string ar = avgRight.ToString().PadLeft(4); ;
            string ll = levelsLeft[0].ToString().PadLeft(4);
            string lg = levels[0].ToString().PadLeft(4); ;
            string lr = levelsRight[0].ToString().PadLeft(4); ;

            string message = $@"```h
[{string.Join(", ", levels)}]
[{string.Join(", ", levelsLeft)}] vs [{string.Join(", ", levelsRight)}]

┌───────┬────┬────┬─────┐
│levels │left│game│right│
├───────┼────┼────┼─────┤
│highest│{hl}│{hg}│{hr} │
│average│{al}│{ag}│{ar} │
│lowest │{ll}│{lg}│{lr} │
└───────┴────┴────┴─────┘
```";

            Discord.LogFiles(files, message);
            Console.Write(message.Replace("```h", "").Replace("`", ""));
            return;
        }

        /// <summary>
        /// Determines whether a number is a funny number.
        /// </summary>
        /// <param name="number">Number to be analyzed.</param>
        private static bool IsFunnyNumber(int number)
        {
            // High or low level
            if (number < 100 || number > 1000)
            {
                return true;
            }

            // Funny number
            string numberString = number.ToString();
            int[] funnyNumbers = { 187, 246, 314, 404, 418, 420 };
            if (funnyNumbers.Contains(number) || number % 100 == 0 || numberString.Contains("69"))
            {
                return true;
            }

            // Same digits
            if (numberString[0] == numberString[1] && numberString[0] == numberString[2])
            {
                return true;
            }

            // Increasing or decreasing digits
            if ((numberString[0] == numberString[1] + 1 && numberString[0] == numberString[2] + 2) ||
                (numberString[0] == numberString[1] - 1 && numberString[0] == numberString[2] - 2))
            {
                return true;
            }

            return false;
        }

#else
        /// <summary>
        /// Tests the accuracy of the OCR Library by comparing its results to the filenames of already processed images.
        /// </summary>
        private static void DebugOCR(bool overrideOCR = false)
        {
            Console.WriteLine("Testing OCR...");

            bool saveDebuggingScreenshots = false;
            if (!Directory.Exists(screenshotLevelDebugginFolder))
            {
                Console.WriteLine("Also saving debugging files...");
                saveDebuggingScreenshots = true;
                Directory.CreateDirectory(screenshotLevelDebugginFolder);
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DirectoryInfo folder = new DirectoryInfo(screenshotLevelFolder);
            List<string> errorStrings = new List<string>();
            foreach (FileInfo file in folder.GetFiles("*.png"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(file.Name);

                int fileLevel = int.Parse(file.Name.Split('_')[0]);

                Bitmap LevelCaptureBmp = System.Drawing.Image.FromFile(file.FullName) as Bitmap;
                Bitmap LevelProcessedBmp = ImageManipulation.PrepareImage(LevelCaptureBmp);
                LevelCaptureBmp.Dispose();
                if (saveDebuggingScreenshots)
                {
                    LevelProcessedBmp.Save($"{screenshotLevelDebugginFolder}{Path.DirectorySeparatorChar}{file.Name}");
                }
                int ocrLevel = overrideOCR ? 0 : OpticalCharacterRecognition.GetNumber(LevelProcessedBmp, out _);
                LevelProcessedBmp.Dispose();
                string statistics;

                if (fileLevel == ocrLevel)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    statistics = $" File: {fileLevel}, OCR: {ocrLevel}";
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    statistics = $" File: {fileLevel}, OCR: {ocrLevel}, Difference: {Math.Abs(fileLevel - ocrLevel)}";
                    errorStrings.Add(file.Name + statistics);
                }
                Console.WriteLine(statistics);
            }

            stopwatch.Stop();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("Debugging complete.");
            Console.WriteLine($"Elapsed time: {stopwatch.Elapsed.ToString()}");
            Console.WriteLine($"Errors: {errorStrings.Count}/{folder.GetFiles("*.png").Length} ({(100.0 * errorStrings.Count / folder.GetFiles("*.png").Length).ToString("00.00")}%)");
            Console.WriteLine();
            Console.WriteLine("Files containing errors:");
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (string error in errorStrings)
            {
                Console.WriteLine(error);
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("Press [Enter] to end the program.");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Green;
        }
#endif
    }
}
