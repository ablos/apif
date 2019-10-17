using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using static APIF.ApifEncoder;

namespace APIF
{
    class RunLengthEncodingCompressor
    {
        // This function will compress te bitmap and return a new AccessibleBitmap
        public static byte[] Compress(AccessibleBitmap source)
        {
            Color[,] colors = new Color[source.width, source.height];         // 2D color array for setting the bytes to colors for easy comparison

            // Iterate through all the pixels of the bitmap, get their ARGB values and store them as colors in the 2D color array
            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    byte[] pixel = source.GetPixel(x, y);
                    Color c;
                    if (pixel.Length % 3 == 0)
                    {
                        // Has no alpha channel
                        if (pixel.Length == 3)
                            c = Color.FromArgb(pixel[0], pixel[1], pixel[2]);
                        else
                            c = Color.FromArgb(pixel[0] * 256 + pixel[1], pixel[2] * 256 + pixel[3], pixel[4] * 256 + pixel[5]);
                    }
                    else
                    {
                        // Has alpha channel
                        if (pixel.Length == 4)
                            c = Color.FromArgb(pixel[0], pixel[1], pixel[2], pixel[3]);
                        else
                            c = Color.FromArgb(pixel[0] * 256 + pixel[1], pixel[2] * 256 + pixel[3], pixel[4] * 256 + pixel[5], pixel[6] * 256 + pixel[7]);
                    }

                    colors[x, y] = c;
                }
            }

            // Compare per row the pixels next to eachother and compress
            // Turn all the color values back to byte arrays and save them in a AccessibleBitmap

            return null;
        }

        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            //decompress & return aBitmap
            return null;
        }
    }
}
