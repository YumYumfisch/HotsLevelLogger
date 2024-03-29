﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
#if !DEBUG
using Discord;
using System.Linq;
using System.Threading;
#else
using System.Diagnostics;
#endif

namespace Hots_Level_Logger
{
    public static class Program
    {
        /// <summary>
        /// Used to lock onto before manipulating Console colors or writing to the console.
        /// </summary>
        public static readonly object consoleLockObject = new object();

        /// <summary>
        /// Path to the text file that stores the token of the discord bot and the discord channel.
        /// </summary>
        private static readonly string configFile = $"E:{Path.DirectorySeparatorChar}HotsLevelLogger{Path.DirectorySeparatorChar}Config.txt";

        /// <summary>
        /// Folder where the number screenshots will be saved to.
        /// </summary>
        private static string screenshotLevelFolder;

#if DEBUG
        /// <summary>
        /// Folder where the preprocessed number screenshots will be saved to.
        /// </summary>
        private static readonly string screenshotLevelDebuggingFolder = $"E:{Path.DirectorySeparatorChar}HotsLevelLogger{Path.DirectorySeparatorChar}DebugLevelLogs";
#else

        /// <summary>
        /// Folder where the player screenshots will be saved to.
        /// </summary>
        private static string screenshotPlayerFolder;

        /// <summary>
        /// 
        /// </summary>
        private static string discordBotToken;

        /// <summary>
        /// 
        /// </summary>
        private static ulong discordChannelId;

        #region Border Pixel Position Constants
        /* Border Pixel Position Constants:
         * Pixel positions that can be combined to get the top-left corner and area size of the area to be screenshotted for each player.
         * Pixel positions are only valid on a 2560x1440p monitor.
         */

        // Level Area Size
        private static readonly Size LevelAreaSize = new Size(50, 18);
        // Level X positions
        private const int BorderPosLevelXLeft = 44;
        private const int BorderPosLevelXRight = 2431;
        // Level Y positions
        private const int BorderPosLevelY1 = 349;
        private const int BorderPosLevelY2 = 526;
        private const int BorderPosLevelY3 = 702;
        private const int BorderPosLevelY4 = 879;
        private const int BorderPosLevelY5 = 1055;

        // Player Area Size
        private static readonly Size PlayerAreaSize = new Size(411, 152);
        // Player X positions
        private const int BorderPosPlayerXLeft = 10;
        private const int BorderPosPlayerXRight = 2138;
        // Player Y positions
        private const int BorderPosPlayerY1 = 252;
        private const int BorderPosPlayerY2 = 429;
        private const int BorderPosPlayerY3 = 605;
        private const int BorderPosPlayerY4 = 781;
        private const int BorderPosPlayerY5 = 958;
        #endregion Border Pixel Position Constants
#endif

        /// <summary>
        /// Entry point for the application.
        /// </summary>
        /// <param name="args">Optional: contains path to the config file at index 0.</param>
        public static void Main(string[] args)
        {
            // Console Setup
            lock (consoleLockObject)
            {
                Console.Title = "Hots Level Logger";
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.OutputEncoding = Encoding.UTF8;
            }

            // Read Config
            if (args == null || args.Length == 0 || args[0] == null || args[0] == "")
            {
                ReadConfigFile(configFile);
            }
            else
            {
                ReadConfigFile(args[0]);
            }

#if !DEBUG
            InitDiscord();
#endif
            lock (consoleLockObject)
            {
                Console.WriteLine();
                Console.WriteLine("Setup Complete.");
            }

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

            // Main Loop
            while (true)
            {
                lock (consoleLockObject)
                {
                    Console.WriteLine("Press [Enter] to analyze the screen.");
                }
                Console.ReadLine();

                lock (consoleLockObject)
                {
                    Console.WriteLine("Capturing...");
                    Console.WriteLine();
                }
                ScreenCapture.CaptureScreen();

                if (!Directory.Exists(screenshotLevelFolder))
                {
                    Directory.CreateDirectory(screenshotLevelFolder);
                }
                if (!Directory.Exists(screenshotPlayerFolder))
                {
                    Directory.CreateDirectory(screenshotPlayerFolder);
                }

                int[] levels = new int[levelAreas.Count];
                string[] filenames = new string[levelAreas.Count];

                lock (consoleLockObject)
                {
                    Console.Write("Levels: {");
                    for (int i = 0; i < levelAreas.Count; i++)
                    {
                        // Capture and analyze screen
                        using (Bitmap LevelCaptureBmp = ScreenCapture.GetScreenArea(levelAreas[i]))
                        {
                            if (LevelCaptureBmp == null)
                            {
                                // Error handling
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Screen capture was not successful.");
                                Console.WriteLine("Press [Enter] to end the program.");
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.ReadLine();
                                Console.ForegroundColor = ConsoleColor.Green;
                                return;
                            }

                            using (Bitmap LevelProcessedBmp = ImageManipulation.PrepareImage(LevelCaptureBmp))
                            {
                                levels[i] = OpticalCharacterRecognition.GetNumber(LevelProcessedBmp, out _);
                            }

                            // Save files
                            filenames[i] = $"{levels[i].ToString().PadLeft(4, '0')}_{DateTime.UtcNow.ToString("yyyy.MM.dd-HH.mm.ss.fff")}.png";
                            LevelCaptureBmp.Save($"{screenshotLevelFolder}{Path.DirectorySeparatorChar}{filenames[i]}");
                        }
                        ScreenCapture.GetScreenArea(PlayerAreas[i]).Save($"{screenshotPlayerFolder}{Path.DirectorySeparatorChar}{filenames[i]}");

                        // Log Levels during analysis
                        Console.Write(levels[i] + (i == levelAreas.Count - 1 ? "}\r\n" : ", "));
                    }

                    Console.WriteLine();
                    Console.WriteLine($"Saved captures at '{screenshotPlayerFolder}'.");
                    Console.WriteLine();
                }
                LogMessage(levels, filenames);
            }
#endif
        }

        /// <summary>
        /// Reads and saves all parameters from the config file.
        /// </summary>
        /// <param name="filepath">Path to the Config File</param>
        private static void ReadConfigFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                lock (consoleLockObject)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error 404: Config.txt not found.");
                    Console.WriteLine("Press [Enter] to end the program.");
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                return;
            }

            string[] lines = File.ReadAllLines(filepath);
#if !DEBUG
            discordBotToken = lines[0].Split(';')[0].Trim();
            discordChannelId = ulong.Parse(lines[1].Split(';')[0].Trim());
            screenshotPlayerFolder = lines[3].Trim();
#endif
            screenshotLevelFolder = lines[2].Trim();
        }

#if !DEBUG
        /// <summary>
        /// Initializes the Discord Bot.
        /// </summary>
        private static void InitDiscord()
        {
            _ = Discord.Init(discordBotToken, discordChannelId);

            while (!Discord.IsReady())
            {
                Thread.Sleep(10);
            }

            Thread.Sleep(10);
        }

        /// <summary>
        /// Logs the levels in discord and sends the corresponding screenshots.
        /// </summary>
        /// <param name="levels">Unsorted array of levels.</param>
        /// <param name="filenames">Unsorted array of filenames</param>
        private static void LogMessage(int[] levels, string[] filenames)
        {
            // Calculate values
            int[] levelsLeft = { levels[0], levels[1], levels[2], levels[3], levels[4] };
            int[] levelsRight = { levels[5], levels[6], levels[7], levels[8], levels[9] };

            int avgLeft = (int)levelsLeft.Average();
            int avgGame = (int)levels.Average();
            int avgRight = (int)levelsRight.Average();

            // Add screenshots to message
            List<FileAttachment> files = new List<FileAttachment>();
            for (int i = 0; i < levels.Length; i++)
            {
                // Ignore AI screenshots
                if ((i < levels.Length / 2 && avgLeft == 0) || // If left team is AI and first half of screenshots are being processed
                    (i >= levels.Length / 2 && avgRight == 0)) // Or if right team is AI and second half of screenshots are being processed
                {
                    continue;
                }

                // Log funny numbers
                if (IsFunnyNumber(levels[i]))
                {
                    files.Add(new FileAttachment($"{screenshotPlayerFolder}{Path.DirectorySeparatorChar}{filenames[i]}", description: levels[i].ToString()));
                }
            }

            // Build message string
            string hl = Pad(levelsLeft.Max());
            string hg = Pad(levels.Max());
            string hr = Pad(levelsRight.Max());
            string al = Pad(avgLeft);
            string ag = Pad(avgGame);
            string ar = Pad(avgRight);
            string ll = Pad(levelsLeft.Min());
            string lg = Pad(levels.Min());
            string lr = Pad(levelsRight.Min());

            Array.Sort(levels);
            Array.Sort(levelsLeft);
            Array.Sort(levelsRight);

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

            // Send Message
            Discord.LogFiles(files, message);
            lock (consoleLockObject)
            {
                Console.Write(message.Replace("```h", "").Replace("`", ""));
            }
            return;
        }

        /// <summary>
        /// Converts an integer to a string and adds padding.
        /// </summary>
        /// <param name="number">Integer to be padded.</param>
        /// <returns>Padded integer.</returns>
        private static string Pad(int number)
        {
            return number.ToString().PadLeft(4);
        }

        /// <summary>
        /// Determines whether a number is a funny number.
        /// </summary>
        /// <param name="number">Number to be analyzed.</param>
        /// <returns>Determines whether the number is funny.</returns>
        private static bool IsFunnyNumber(int number)
        {
            // High or low level
            if (number < 100 || number > 1000)
            {
                return true;
            }

            // Funny number
            string numberString = number.ToString();
            int[] funnyNumbers = { 187, 246, 314, 365, 404, 418, 420 };
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
            lock (consoleLockObject)
            {
                Console.WriteLine("Testing OCR...");
            }

            bool saveDebuggingScreenshots = false;
            if (!Directory.Exists(screenshotLevelDebuggingFolder))
            {
                lock (consoleLockObject)
                {
                    Console.WriteLine("Also saving debugging files...");
                }
                saveDebuggingScreenshots = true;
                try
                {
                    _ = Directory.CreateDirectory(screenshotLevelDebuggingFolder);
                }
                catch (IOException)
                {
                    lock (consoleLockObject)
                    {
                        ConsoleColor previousColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error while trying to create Directory '{screenshotLevelDebuggingFolder}'.");
                        Console.WriteLine("Debugging screenshots will not be saved.");
                        Console.ForegroundColor = previousColor;
                    }
                }
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DirectoryInfo folder = new DirectoryInfo(screenshotLevelFolder);
            if (!folder.Exists)
            {
                lock (consoleLockObject)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error while trying to access Directory '{folder.FullName}'.");
                    Console.WriteLine("Press [Enter] to end the program.");
                    Console.ForegroundColor = ConsoleColor.Black;
                    _ = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                return;
            }

            List<string> errorStrings = new List<string>();
            foreach (FileInfo file in folder.GetFiles("*.png"))
            {
                lock (consoleLockObject)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(file.Name);

                    int fileLevel = int.Parse(file.Name.Split('_')[0]);

                    Bitmap LevelCaptureBmp = Image.FromFile(file.FullName) as Bitmap;
                    Bitmap LevelProcessedBmp = ImageManipulation.PrepareImage(LevelCaptureBmp);
                    LevelCaptureBmp.Dispose();
                    if (saveDebuggingScreenshots)
                    {
                        LevelProcessedBmp.Save($"{screenshotLevelDebuggingFolder}{Path.DirectorySeparatorChar}{file.Name}");
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
            }

            stopwatch.Stop();

            lock (consoleLockObject)
            {
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
                _ = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
#endif
    }
}
