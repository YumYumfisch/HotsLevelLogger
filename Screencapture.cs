using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Handles capturing the screen and creating screenshots.
    /// </summary>
    public static class Screencapture
    {
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
    }
}
