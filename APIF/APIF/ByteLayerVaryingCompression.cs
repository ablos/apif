using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class ByteLayerVaryingCompression
    {
        //Compress aBitmap into byte array
        public static byte[] Compress(AccessibleBitmap source)
        {
            return new byte[1];
        }



        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            return new AccessibleBitmap(0,0,0);
        }

    }
}
