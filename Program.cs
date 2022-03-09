using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Hots_Level_Logger
{
    public static class Program
    {
        /// <summary>
        /// Folder where the screenshots will be saved to.
        /// </summary>
        private static readonly string screenshotFolder = $"C:{Path.DirectorySeparatorChar}Temp{Path.DirectorySeparatorChar}HotsLevelLogs";

        public static void Main(string[] args)
        {
            #region Console Setup
            AllocConsole();
            Console.Title = "Hots Level Logger";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.OutputEncoding = Encoding.UTF8;
            #endregion Console Setup

            if (!Directory.Exists(screenshotFolder))
            {
                Directory.CreateDirectory(screenshotFolder);
            }

            Size areaSize = new Size(40, 15);
            List<Rectangle> areas = new List<Rectangle>();
            areas.Add(new Rectangle(new Point(31, 261), areaSize));
            areas.Add(new Rectangle(new Point(31, 393), areaSize));
            areas.Add(new Rectangle(new Point(31, 525), areaSize));
            areas.Add(new Rectangle(new Point(31, 657), areaSize));
            areas.Add(new Rectangle(new Point(31, 790), areaSize));
            areas.Add(new Rectangle(new Point(2462, 261), areaSize));
            areas.Add(new Rectangle(new Point(2462, 393), areaSize));
            areas.Add(new Rectangle(new Point(2462, 525), areaSize));
            areas.Add(new Rectangle(new Point(2462, 657), areaSize));
            areas.Add(new Rectangle(new Point(2462, 790), areaSize));

            while (true)
            {
                Console.WriteLine("Press [Enter] to analyze the screen.");
                Console.ReadLine();



                Console.WriteLine("Capturing...");
                for (int i = 0; i < areas.Count; i++)
                {
                    Bitmap bitmap = ScreenCapture.CaptureScreen(areas[i]);
                    bitmap.Save($"{screenshotFolder}{Path.DirectorySeparatorChar}Capture_{i}_raw.png");

                    bitmap = ImageManipulation.ImageCleanup(bitmap);
                    bitmap.Save($"{screenshotFolder}{Path.DirectorySeparatorChar}Capture_{i}_edit_{OpticalCharacterRecognition.GetNumberString(bitmap)}.png");
                }
                Console.WriteLine($"Saved captures at '{screenshotFolder}'.");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Uses kernel32.dll to allocate a Windows console.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }
}
