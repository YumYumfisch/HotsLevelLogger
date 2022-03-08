using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Hots_Level_Logger
{
    public static class Program
    {
        /// <summary>
        /// Folder where the images will be read from and saved to.
        /// </summary>
        private static readonly string screenshotFolder = $"C:{Path.DirectorySeparatorChar}Temp";

        /// <summary>
        /// Filename of png to be analyzed.
        /// </summary>
        private static readonly string filename = "Capture18.png";

        public static void Main(string[] args)
        {
            #region Console Setup
            AllocConsole();
            Console.Title = "Hots Level Logger";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.OutputEncoding = Encoding.UTF8;
            #endregion Console Setup

            // Capture screenshot
#if false
            Console.WriteLine("Press [Enter] to capture a screenshot.");
            Console.ReadLine();
            Console.WriteLine("Capturing...");
            CaptureScreen(screenshotFolder);
            Console.WriteLine("Successfully captured screen.");
            Console.WriteLine($"Saved screenshot at '{screenshotFolder}'.");
#endif

            Console.WriteLine("Reading capture...");
            string filepath = $"{screenshotFolder}{Path.DirectorySeparatorChar}{filename}";
            Bitmap bitmap = new Bitmap(filepath);

            Console.WriteLine("Manipulating capture...");
            bitmap = ImageManipulation.ImageCleanup(bitmap);

            bitmap.Save($"{screenshotFolder}{Path.DirectorySeparatorChar}Capture_Edit.png");

            Console.WriteLine("Analyzing capture...");
            Console.WriteLine(OpticalCharacterRecognition.GetNumberString(bitmap));



            // End
            Console.WriteLine();
            Console.WriteLine("Press [Enter] to end the program.");
            Console.ReadLine();
        }

        /// <summary>
        /// Uses kernel32.dll to allocate a Windows console.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }
}
