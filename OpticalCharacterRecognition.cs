using IronOcr;
using System.Drawing;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Handles image analysis using IronOCR and Tesseract libraries.
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
            TesseractConfiguration config = new TesseractConfiguration
            {
                EngineMode = TesseractEngineMode.Default,
                PageSegmentationMode = TesseractPageSegmentationMode.SingleWord,
                ReadBarCodes = false,
                WhiteListCharacters = "0123456789"
            };
            IronTesseract ocr = new IronTesseract(config);

            OcrResult result;
            using (OcrInput input = new OcrInput(bitmap))
            {
                input.MinimumDPI = null;
                result = ocr.Read(input);
            }

            confidence = result.Confidence;
            if (result.Text.Trim() == "")
            {
                return 0;
            }
            return int.Parse(result.Text.Trim());
        }
    }
}
