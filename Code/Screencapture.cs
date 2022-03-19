using System.Drawing;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Handles capturing of the content displayed on the screen.
    /// </summary>
    public static class ScreenCapture
    {
        /// <summary>
        /// Captures a screenshot in a specific area of the screen.
        /// </summary>
        /// <param name="area">Area on the screen to be captured.</param>
        /// <returns>The Pixels currently displayed on screen in the specified area.</returns>
        public static Bitmap CaptureScreen(Rectangle area)
        {
            Bitmap bitmap = new Bitmap(area.Width, area.Height);

            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(area.Left, area.Top, 0, 0, area.Size);

            return bitmap;
        }
    }
}
