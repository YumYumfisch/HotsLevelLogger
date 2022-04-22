using Hots_Level_Logger.Code.OcrApi;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using Tesseract;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Handles Optical Character Recognition.
    /// </summary>
    public static class OpticalCharacterRecognition
    {
        /// <summary>
        /// Returns the number recognized in the image using Tesseract.
        /// </summary>
        /// <param name="bitmap">The image to be analyzed. Preferrably dark text on a bright background.</param>
        /// <returns>The number recognized in the image.</returns>
        public static int GetNumber(Bitmap bitmap, out float confidence)
        {
            string text;
            using (TesseractEngine engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                // Tesseract configuration
                engine.SetVariable("tessedit_char_whitelist", "0123456789"); // Whitelist of chars to recognize
                engine.SetVariable("tessedit_unrej_any_wd", "1"); // Dont bother with word plausibility
                engine.SetVariable("textord_noise_rejrows", "0"); // Reject noise-like rows
                engine.SetVariable("textord_noise_rejwords", "0"); // Reject noise-like words
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
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the number recognized in the image using the ocr.space api.
        /// </summary>
        /// <param name="filepath">The path of the png to be analyzed. Preferrably without noise or background but even with noise it works.</param>
        /// <param name="apiKey">Ocr.space api key</param>
        /// <returns>The number recognized in the image.</returns>
        public static async void GetNumber(string filepath, string apiKey)
        {
            Console.WriteLine("API zeug");

            HttpClient httpClient = new HttpClient
            {
                Timeout = new TimeSpan(1, 1, 1)
            };

            MultipartFormDataContent httpForm = new MultipartFormDataContent
                {
                    { new StringContent(apiKey), "apikey" },
                    { new StringContent("eng"), "language" },
                    { new StringContent("1"), "ocrengine" },
                    { new StringContent("true"), "scale" },
                    { new StringContent("true"), "istable" }
                };

            byte[] imageData = File.ReadAllBytes(filepath);
            httpForm.Add(new ByteArrayContent(imageData, 0, imageData.Length), "image", "image.jpg");

            HttpResponseMessage response = await httpClient.PostAsync("https://api.ocr.space/Parse/Image", httpForm);
            string strContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"content: '{strContent}' das wars");

            Rootobject ocrResult = JsonConvert.DeserializeObject<Rootobject>(strContent);

            if (ocrResult.OCRExitCode == 1)
            {
                for (int i = 0; i < ocrResult.ParsedResults.Length; i++)
                {
                    Console.WriteLine("parsed text: " + ocrResult.ParsedResults[i].ParsedText); // DEBUG
                    Console.WriteLine(GetNumber(ocrResult.ParsedResults[i].ParsedText));
                    //return GetNumber(ocrResult.ParsedResults[i].ParsedText);
                }
            }
            else
            {
                MessageBox.Show($"API ERROR: \r\n{strContent}");
                //return 0;
            }
        }

        /// <summary>
        /// Ignores all characters except for digits and parses them to an integer.
        /// </summary>
        /// <param name="input">String to be parsed.</param>
        /// <returns>Number contained in the string.</returns>
        private static int GetNumber(string input)
        {
            string number = "";
            char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            foreach (char ch in input)
            {
                if (digits.Contains(ch))
                {
                    number += ch;
                }
            }
            return int.Parse(number);
        }
    }
}
