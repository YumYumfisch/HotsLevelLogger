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
        /// <returns>The numbers recognized in the image.</returns>
        public static string GetNumberString(Bitmap bitmap)
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

                //input.DeNoise(); // Might be helpful

                result = ocr.Read(input);
            }

            return $"Output: ({result.Confidence}%):\r\n{result.Text}";
        }
    }
}
