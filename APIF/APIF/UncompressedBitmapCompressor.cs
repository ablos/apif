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
            return source.GetRawPixelBytes();
        }

        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            AccessibleBitmap aBitmap = new AccessibleBitmap(width, height, pixelBytes);
            aBitmap.SetRawPixelBytes(source);
            return aBitmap;
        }
    }
}
