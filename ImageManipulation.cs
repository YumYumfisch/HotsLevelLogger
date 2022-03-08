using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Hots_Level_Logger
{
    /// <summary>
    /// Handles image manipulation to improve them for OCR.
    /// </summary>
    public static class ImageManipulation
    {
        public static Bitmap StripImage(Bitmap bitmap)
        {
            for (int bitmapWidthIndex = 0; bitmapWidthIndex < bitmap.Width; bitmapWidthIndex++)
            {
                for (int bitmapHeightIndex = 0; bitmapHeightIndex < bitmap.Height; bitmapHeightIndex++)
                {
                    // If (Pixelfarbe in ListeMitGültigenPixelfarben)
                    // {  Pixel Schwarz setzen  } (Vordergrund)
                    // Else 
                    // {  Pixel Weiß setzen  } (Hintergrund)
                }
            }

            return bitmap;
        }
    }
}
