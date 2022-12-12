using System.Drawing;

namespace Image_Analysis
{
    public class Program
    {
        private static string FolderPath { get; } = "E:\\HotsLevelLogger\\DebugLevelLogs";

        /// <summary>
        /// Counts the number of occurrances for each digit
        /// </summary>
        private static int[] DigitCount { get; } = new int[10];

        private static List<int>[] DigitPixelCounts { get; }

        static Program()
        {
            DigitPixelCounts = new List<int>[10];
            for (int i = 0; i < DigitPixelCounts.Length; i++)
            {
                DigitPixelCounts[i] = new List<int>();
            }
        }

        public static void Main(string[] args)
        {
            _ = args;

            Console.ForegroundColor = ConsoleColor.Green;

            if (!Directory.Exists(FolderPath))
            {
                Console.WriteLine($"Folder {FolderPath} does not exist.");
                return;
            }

            foreach (string file in Directory.GetFiles(FolderPath, "*.png"))
            {
                FileInfo fileInfo = new FileInfo(file);

                string digitString = fileInfo.Name.Substring(0, 4);

                int[] digits = ExtractDigits(digitString);

                Bitmap? rawFileBitmap = Image.FromFile(fileInfo.FullName) as Bitmap;

                ExtractDigitImages(rawFileBitmap!, digits);

                Console.WriteLine("Digit Averages:");
                for (int i = 0; i < DigitPixelCounts.Length; i++)
                {
                    Console.WriteLine($"Digit {i}: {DigitPixelCounts[i].Average()}");
                }
            }
        }

        private static void ExtractDigitImages(Bitmap rawFileBitmap, int[] digits)
        {
            foreach (int digit in digits)
            {
                if (digit < 0)
                {
                    continue;
                }

                throw new NotImplementedException();
                // TODO:
                // Extract region
                // Count Pixels
                // Add to DigitPixelCounts
            }
        }

        /// <summary>
        /// Saves all digits in the string to the global Field, ignoring leading zeros
        /// </summary>
        /// <returns>The digits contained in the string or -1 if the digit was a leading zero</returns>
        private static int[] ExtractDigits(string digitString)
        {
            int[] stringDigits = new int[digitString.Length];
            Array.Fill(stringDigits, -1);

            bool leadingZero = true;
            for (int i = 0; i < digitString.Length; i++)
            {
                char digitChar = digitString[i];

                int digit;
                if (!int.TryParse(digitChar.ToString(), out digit))
                {
                    continue;
                }

                if (digit > 0)
                {
                    leadingZero = false;
                }

                if (leadingZero)
                {
                    continue;
                }

                stringDigits[i] = digit;
                DigitCount[digit]++;
            }

            return stringDigits;
        }
    }
}
