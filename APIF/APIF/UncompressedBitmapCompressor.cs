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
        //Compress aBitmap into byte array
        public static byte[] Compress(AccessibleBitmap source)
        {
            //Return raw aBitmap
            return source.GetRawPixelBytes();
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            //Create aBitmap from raw data
            AccessibleBitmap aBitmap = new AccessibleBitmap(width, height, pixelBytes);
            aBitmap.SetRawPixelBytes(source);
            return aBitmap;
        }
    }
}
