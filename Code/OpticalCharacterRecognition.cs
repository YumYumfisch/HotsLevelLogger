using System.Drawing;
using Tesseract;

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
        public static int GetNumber(Bitmap bitmap, out float confidence)
        {
            string text;
            using (TesseractEngine engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                // Tesseract configuration
                _ = engine.SetVariable("tessedit_char_whitelist", "0123456789"); // Whitelist of chars to recognize
                _ = engine.SetVariable("tessedit_unrej_any_wd", "1"); // Dont bother with word plausibility
                _ = engine.SetVariable("textord_noise_rejrows", "0"); // Reject noise-like rows
                _ = engine.SetVariable("textord_noise_rejwords", "0"); // Reject noise-like words
                //engine.SetVariable("user_words_file", "filename.txt"); // A filename of user-provided words

                // Character recognition
                using (Page page = engine.Process(bitmap, PageSegMode.SingleWord))
                {
                    text = page.GetText().Trim();
                    confidence = page.GetMeanConfidence();
                }
            }


            if (text == null || text == "")
            {
                return 0;
            }
            try
            {
                // Try catch is still necessary because even if null and "" is filtered out, the text can still contain spaces between digits if the image isnt processed properly during debugging.
                return int.Parse(text);
            }
            catch (System.Exception)
            {
                return 0;
            }
        }
    }
}
