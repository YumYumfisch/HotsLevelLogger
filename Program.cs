using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Hots_Level_Logger
{
    public static class Program
    {
        /// <summary>
        /// Folder where the screenshots will be saved to.
        /// </summary>
        private static readonly string screenshotFolder = $"C:{Path.DirectorySeparatorChar}Temp{Path.DirectorySeparatorChar}HotsLevelLogs";

        #region Border Pixel Position Constants
        /* Border Pixel Position Constants:
         * Pixel positions that can be combined to get the top-left corner of the are to be screenshotted for a given player.
         */

        // X positions
        private const int BorderPosXLeft = 31;
        private const int BorderPosXRight = 2462;
        // Y positions
        private const int BorderPosY1 = 261;
        private const int BorderPosY2 = 393;
        private const int BorderPosY3 = 525;
        private const int BorderPosY4 = 657;
        private const int BorderPosY5 = 790;
        #endregion Border Pixel Position Constants

        public static void Main(string[] args)
        {
            #region Console Setup
            Console.Title = "Hots Level Logger";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.OutputEncoding = Encoding.UTF8;
            #endregion Console Setup

            Size areaSize = new Size(40, 15);
            List<Rectangle> areas = new List<Rectangle> {
                new Rectangle(new Point(BorderPosXLeft, BorderPosY1), areaSize),
                new Rectangle(new Point(BorderPosXLeft, BorderPosY2), areaSize),
                new Rectangle(new Point(BorderPosXLeft, BorderPosY3), areaSize),
                new Rectangle(new Point(BorderPosXLeft, BorderPosY4), areaSize),
                new Rectangle(new Point(BorderPosXLeft, BorderPosY5), areaSize),
                new Rectangle(new Point(BorderPosXRight, BorderPosY1), areaSize),
                new Rectangle(new Point(BorderPosXRight, BorderPosY2), areaSize),
                new Rectangle(new Point(BorderPosXRight, BorderPosY3), areaSize),
                new Rectangle(new Point(BorderPosXRight, BorderPosY4), areaSize),
                new Rectangle(new Point(BorderPosXRight, BorderPosY5), areaSize)
            };



#if DEBUG
            Bitmap bitmap = Image.FromFile($"C:{Path.DirectorySeparatorChar}Temp{Path.DirectorySeparatorChar}Capture_6_raw.png") as Bitmap;
            bitmap = ImageManipulation.ConnectedComponentAnalysis(ImageManipulation.SeparateDigits(bitmap));

            Console.WriteLine();
            Console.ReadLine();
#else



            while (true)
            {
                Console.WriteLine("Press [Enter] to analyze the screen.");
                Console.ReadLine();



                Console.WriteLine("Capturing...");
                if (!Directory.Exists(screenshotFolder))
                {
                    Directory.CreateDirectory(screenshotFolder);
                }

                for (int i = 0; i < areas.Count; i++)
                {
                    Bitmap bitmap = ScreenCapture.CaptureScreen(areas[i]);
                    bitmap.Save($"{screenshotFolder}{Path.DirectorySeparatorChar}Capture_{i}_raw.png");

                    bitmap = ImageManipulation.ConnectedComponentAnalysis(ImageManipulation.SeparateDigits(bitmap));
                    bitmap.Save($"{screenshotFolder}{Path.DirectorySeparatorChar}Capture_{i}_edit_{OpticalCharacterRecognition.GetNumberString(bitmap)}.png");
                }
                Console.WriteLine($"Saved captures at '{screenshotFolder}'.");
                Console.WriteLine();
            }
#endif
        }
    }
}
