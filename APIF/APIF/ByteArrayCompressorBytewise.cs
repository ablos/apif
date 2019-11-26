using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class ByteArrayCompressorBytewise
    {
        //Compress aBitmap into byte array
        public static byte[] Compress(AccessibleBitmapBytewise source, int byteLayer)
        {
            //Return all byte of the correct layer
            int[] layerByteArray = new int[source.width * source.height];
            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    //Write bit of current pixel to stream
                    layerByteArray[y * source.width + x] = source.GetPixelByte(x, y, byteLayer);
                }
            }
            return VaryingIntArrayCompressor.Compress(layerByteArray).ToByteArray();
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBytewise Decompress(byte[] inBytes, AccessibleBitmapBytewise inBitmap, out byte[] restBytes, int byteLayer)
        {
            BitStreamFIFO layerBits = new BitStreamFIFO(inBytes);
            int[] layerByteArray = VaryingIntArrayCompressor.Decompress(ref layerBits);

            //Create aBitmap from pixel data
            for (int y = 0; y < inBitmap.height; y++)
            {
                for (int x = 0; x < inBitmap.width; x++)
                {
                    //Write bit of stream to current pixel
                    inBitmap.SetPixelByte(x, y, byteLayer, (byte)layerByteArray[y * inBitmap.width + x]);
                }
            }

            restBytes = new byte[(int)Math.Ceiling(layerBits.Length / 8.0) - 1];
            Array.Copy(inBytes, inBytes.Length - (int)Math.Ceiling(layerBits.Length / 8.0) + 1, restBytes, 0, restBytes.Length);
            return inBitmap;
        }
    }
}
