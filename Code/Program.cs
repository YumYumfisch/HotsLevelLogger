using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hots_Level_Logger
{
    public static class Program
    {
        /// <summary>
        /// Folder where the number screenshots will be saved to.
        /// </summary>
        private static readonly string screenshotLevelFolder = $"E:{Path.DirectorySeparatorChar}HotsLevelLogger{Path.DirectorySeparatorChar}LevelLogs";

        /// <summary>
        /// Folder where the player screenshots will be saved to.
        /// </summary>
        private static readonly string screenshotPlayerFolder = $"E:{Path.DirectorySeparatorChar}HotsLevelLogger{Path.DirectorySeparatorChar}PlayerLogs";

        /// <summary>
        /// Path to the text file that stores the token of the discord bot.
        /// </summary>
        private static readonly string TokenFile = $"E:{Path.DirectorySeparatorChar}HotsLevelLogger{Path.DirectorySeparatorChar}token.txt";

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

        /// <summary>
        /// Entry point for the application.
        /// </summary>
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

#if DEBUG
            DebugOCR();
            return;
#endif

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
                        Discord.LogFile($"{screenshotPlayerFolder}{Path.DirectorySeparatorChar}{filename}", levels[i].ToString());
                    }

                    // Process additional information
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
                Console.WriteLine($"Saved captures at '{screenshotPlayerFolder}'.");
                Console.WriteLine();
            }
        }

#if DEBUG
        /// <summary>
        /// 
        /// </summary>
        private static void DebugOCR()
        {
            Console.WriteLine("Testing OCR...");

            DirectoryInfo folder = new DirectoryInfo(screenshotLevelFolder);
            int errors = 0;
            foreach (FileInfo file in folder.GetFiles("*.png"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(file.Name);

                int fileLevel = int.Parse(file.Name.Split('_')[0]);

                Bitmap LevelCaptureBmp = Image.FromFile(file.FullName) as Bitmap;
                Bitmap LevelProcessedBmp = ImageManipulation.PrepareImage(LevelCaptureBmp);
                int ocrLevel = OpticalCharacterRecognition.GetNumber(LevelProcessedBmp, out _);

                if (fileLevel == ocrLevel)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    errors++;
                }
                Console.WriteLine($"File: {fileLevel}, OCR: {ocrLevel}");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("Debugging complete.");
            Console.WriteLine($"Errors: {errors}/{folder.GetFiles("*.png").Length}");
            Console.WriteLine("Press any key to end the program.");
            Console.ReadKey(true);
        }
#endif

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
    }
}
