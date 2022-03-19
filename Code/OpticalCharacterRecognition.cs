using System.Drawing;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Handles Optical Character Recognition using Tesseract.
    /// </summary>
    public static class OpticalCharacterRecognition
    {
        /// <summary>
        /// Returns a string that contains all numbers recognized from the image.
        /// </summary>
        /// <param name="bitmap">The image to be analyzed. Preferrably dark text on a bright background.</param>
        /// <returns>The string of digits recognized in the image.</returns>
        public static int GetNumber(Bitmap bitmap, out double confidence)
        {
            confidence = 0;
            return 0;
        }
    }
}
