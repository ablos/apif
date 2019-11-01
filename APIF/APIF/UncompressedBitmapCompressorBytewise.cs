using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class UncompressedBitmapCompressorBytewise
    {
        //Compress aBitmap into byte array
        public static byte[] Compress(AccessibleBitmapBytewise source, int byteLayer)
        {
            //Return all byte of the correct layer
            byte[] output = new byte[source.width * source.height];
            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    //Write bit of current pixel to stream
                    output[y * source.width + x] = source.GetPixelByte(x, y, byteLayer);
                }
            }
            return output;
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBytewise Decompress(byte[] inBytes, AccessibleBitmapBytewise inBitmap, out byte[] restBytes, int byteLayer)
        {
            //Create aBitmap from pixel data
            for (int y = 0; y < inBitmap.height; y++)
            {
                for (int x = 0; x < inBitmap.width; x++)
                {
                    //Write bit of stream to current pixel
                    inBitmap.SetPixelByte(x, y, byteLayer, inBytes[y * inBitmap.width + x]);
                }
            }
            restBytes = new byte[inBytes.Length - (inBitmap.width * inBitmap.height)];
            Array.Copy(inBytes, inBitmap.width * inBitmap.height, restBytes, 0, restBytes.Length);
            return inBitmap;
        }
    }
}
