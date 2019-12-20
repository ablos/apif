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
            //Create int array with a size equal to the amount of pixels in the image
            int[] layerByteArray = new int[source.width * source.height];

            //Loop trough all lines of pixels
            for (int y = 0; y < source.height; y++)
            {
                //Loop trough all pixels in this line
                for (int x = 0; x < source.width; x++)
                {
                    //Write the byte of this channel from the current pixel to the correct position in the output array
                    layerByteArray[y * source.width + x] = source.GetPixelByte(x, y, byteLayer);
                }
            }

            //Compress the obtained array of integers using different techniques
            byte[] outputArray = VaryingIntArrayCompressor.Compress(layerByteArray).ToByteArray();

            //Return the output array
            return outputArray;
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBytewise Decompress(byte[] inBytes, AccessibleBitmapBytewise inBitmap, out byte[] restBytes, int byteLayer)
        {
            //Create a BitStreamFIFO class from the incoming bytes, becouse the decompressor for integer arrays uses it
            BitStreamFIFO layerBits = new BitStreamFIFO(inBytes);

            //Decompress the incoming data to an array of integers
            int[] layerByteArray = VaryingIntArrayCompressor.Decompress(ref layerBits);


            //Add the data from the array of integers to the incoming bitmap

            //Loop trough all lines of pixels
            for (int y = 0; y < inBitmap.height; y++)
            {
                //Loop trough all pixels in this line
                for (int x = 0; x < inBitmap.width; x++)
                {
                    //Write the integer of this channel from the correct position in the input array to the current pixel
                    inBitmap.SetPixelByte(x, y, byteLayer, (byte)layerByteArray[y * inBitmap.width + x]);
                }
            }

            //Remove the bytes used for this channel from the incoming byte array and pass the rest of them to the next channel
            restBytes = new byte[layerBits.Length / 8];
            Array.Copy(inBytes, inBytes.Length - restBytes.Length, restBytes, 0, restBytes.Length);

            //Return the modified bitmap so the rest of the channels can be added to complete it
            return inBitmap;
        }
    }
}
