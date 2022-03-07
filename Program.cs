using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Takes a screenshot.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Folder where the screenshot will be saved.
        /// </summary>
        private static string path = $"C:{Path.DirectorySeparatorChar}Temp";

        /// <summary>
        /// Takes a screenshot.
        /// </summary>
        public static void Main(string[] args)
        {
            AllocConsole();
            Console.Title = "Hots Level Logger";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Press [Enter] to capture a screenshot.");
            Console.ReadLine();
            Console.WriteLine("Capturing...");
            CaptureScreen(path);
            Console.WriteLine("Successfully captured screen.");
            Console.WriteLine($"Saved screenshot at '{path}'.");

            Console.WriteLine();
            Console.WriteLine("Press [Enter] to end the program.");
            Console.ReadLine();
        }

        /// <summary>
        /// Saves a screenshot of the primary screen as 'Capture.png' at the specified path.
        /// </summary>
        /// <param name="folderPath">Folder where the screenshot will be saved.</param>
        private static void CaptureScreen(string folderPath)
        {
            //Create a new bitmap and graphics Objects that fit the primary screen.
            Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);

            // Take screenshot from upper left to bottom right corner of primary screen.
            graphics.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            // Save screenshot to specified path.
            bitmap.Save($"{folderPath}{Path.DirectorySeparatorChar}Capture.png", ImageFormat.Png);
        }

        /// <summary>
        /// Uses kernel32.dll to allocate a Windows console.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }
}
