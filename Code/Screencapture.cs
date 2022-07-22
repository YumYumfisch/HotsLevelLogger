using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Handles capturing of the content displayed on the screen.
    /// </summary>
    public static class ScreenCapture
    {
        /// <summary>
        /// Caches the screenshot of the primary screen.
        /// </summary>
        private static readonly Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

        /// <summary>
        /// Captures a screenshot of the primary monitor and saves it as a private field.
        /// </summary>
        internal static void CaptureScreen()
        {
            Graphics graphics = Graphics.FromImage(screenshot);
            graphics.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size);
            graphics.Dispose();
        }

        /// <summary>
        /// Selects a specific area of the previously captured screenshot.
        /// </summary>
        /// <param name="area">Area of the screenshot to be selected.</param>
        /// <returns>The Pixels in the specified area of the captured screenshot.</returns>
        internal static Bitmap GetScreenArea(Rectangle area)
        {
            if (screenshot == null)
            {
                return null;
            }

            Bitmap bitmap = screenshot.Clone(area, PixelFormat.Format32bppArgb);

            return bitmap;
        }
    }
}
