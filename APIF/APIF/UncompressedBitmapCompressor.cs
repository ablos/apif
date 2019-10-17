using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class UncompressedBitmapCompressor
    {
        public static byte[] Compress(AccessibleBitmap source)
        {
            byte[] byteArray = new byte[source.pixelBytes * source.height * source.width];

            for (int i = 0; i < source.height; i++)
            {
                for (int j = 0; j < source.width; j++)
                {
                    byte[] pixel = source.GetPixel(j, i);
                    for (int k = 0; k < pixel.Length; k++)
                    {
                        byteArray[i * source.width * source.pixelBytes + j * source.pixelBytes + k] = pixel[k];
                    }
                }
            }

            return byteArray;
        }

        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            AccessibleBitmap aBitmap = new AccessibleBitmap(width, height, pixelBytes);

            int x = 0;
            int y = 0;
            for (int i = 0; i < source.Length; i = i + aBitmap.pixelBytes)
            {
                byte[] pixel = new byte[aBitmap.pixelBytes];
                for (int j = 0; j < pixel.Length; j++)
                {
                    pixel[j] = source[i + j];
                }
                aBitmap.SetPixel(x, y, pixel);

                x++;
                if (x == aBitmap.width)
                {
                    x = 0;
                    y++;
                }
            }

            return aBitmap;
        }

    }
}
