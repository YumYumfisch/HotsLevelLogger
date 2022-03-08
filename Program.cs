using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Hots_Level_Logger
{
    public static class Program
    {
        /// <summary>
        /// Folder where the screenshot will be saved.
        /// </summary>
        private static readonly string screenshotFolder = $"C:{Path.DirectorySeparatorChar}Temp";

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
            string filename = $"{screenshotFolder}{Path.DirectorySeparatorChar}Capture18.png";
            Bitmap bitmap = new Bitmap(filename);

            Console.WriteLine("Stripping capture...");
            bitmap = ImageManipulation.StripImage(bitmap);

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
