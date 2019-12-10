using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class ByteArrayCompressor
    {
        //Compress aBitmap into byte array
        public static byte[] Compress(AccessibleBitmap source)
        {
            return VaryingIntArrayCompressor.Compress(Array.ConvertAll(source.GetRawPixelBytes(), Convert.ToInt32)).ToByteArray();
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
            {
            BitStreamFIFO layerBits = new BitStreamFIFO(source);
            int[] byteArray = VaryingIntArrayCompressor.Decompress(ref layerBits);

            //Create aBitmap from pixel data
            AccessibleBitmap aBitmap = new AccessibleBitmap(width, height, pixelBytes);
            aBitmap.SetRawPixelBytes(Array.ConvertAll(byteArray, Convert.ToByte));

            return aBitmap;
        }
    }
}
