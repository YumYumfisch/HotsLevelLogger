using IronOcr;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Screenshot analysis testing
    /// </summary>
    class Program
    {
        /// <summary>
        /// Folder where the screenshot will be saved.
        /// </summary>
        private static string screenshotFolder = $"C:{Path.DirectorySeparatorChar}Temp";

        /// <summary>
        /// Takes and analyses a screenshot.
        /// </summary>
        public static void Main(string[] args)
        {
            AllocConsole();
            Console.Title = "Hots Level Logger";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.OutputEncoding = Encoding.UTF8;

            // Capture screenshot
#if false
            Console.WriteLine("Press [Enter] to capture a screenshot.");
            Console.ReadLine();
            Console.WriteLine("Capturing...");
            CaptureScreen(screenshotFolder);
            Console.WriteLine("Successfully captured screen.");
            Console.WriteLine($"Saved screenshot at '{screenshotFolder}'.");
#endif

            // Analyse Screenshot
            Console.WriteLine("Analysing capture...");
            OcrResult result;// = new IronTesseract().Read($"{path}{Path.DirectorySeparatorChar}Capture.png");

            for (int i = 3; i < 14; i++)
            {
                IronTesseract tesseract = new IronTesseract();
                using (OcrInput input = new OcrInput($"{screenshotFolder}{Path.DirectorySeparatorChar}Capture{i}.png"))
                {
                    input.ToGrayScale();
                    input.DeNoise();
                    input.Contrast();
                    //input.Binarize();
                    //input.Invert();

                    result = tesseract.Read(input);
                }

                // Output
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Text {i} ({result.Confidence}%):");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(result.Text);
            }

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
