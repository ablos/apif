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
            //Create byte array with a size equal to the amount of pixels in the image
            byte[] output = new byte[source.width * source.height];

            //Loop trough all lines of pixels
            for (int y = 0; y < source.height; y++)
            {
                //Loop trough all pixels in this line
                for (int x = 0; x < source.width; x++)
                {
                    //Write the byte of this channel from the current pixel to the correct position in the output array
                    output[y * source.width + x] = source.GetPixelByte(x, y, byteLayer);
                }
            }

            //Return all bytes of this channel
            return output;
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBytewise Decompress(byte[] inBytes, AccessibleBitmapBytewise inBitmap, out byte[] restBytes, int byteLayer)
        {
            //Add the data from this stream to the incoming bitmap

            //Loop trough all lines of pixels
            for (int y = 0; y < inBitmap.height; y++)
            {
                //Loop trough all pixels in this line
                for (int x = 0; x < inBitmap.width; x++)
                {
                    //Write the byte of this channel from the correct position in the input array to the current pixel
                    inBitmap.SetPixelByte(x, y, byteLayer, inBytes[y * inBitmap.width + x]);
                }
            }

            //Remove the bytes used for this channel from the incoming byte array and pass the rest of them to the next channel
            restBytes = new byte[inBytes.Length - (inBitmap.width * inBitmap.height)];
            Array.Copy(inBytes, inBitmap.width * inBitmap.height, restBytes, 0, restBytes.Length);

            //Return the modified bitmap so the rest of the channels can be added to complete it
            return inBitmap;
        }
    }
}
