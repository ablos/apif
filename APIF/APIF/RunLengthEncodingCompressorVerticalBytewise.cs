using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class RunLengthEncodingCompressorVerticalBytewise
    {
        //Compress aBitmap into byte array
        public static byte[] Compress(AccessibleBitmapBytewise source, int byteLayer)
        {
            //Initialize
            List<int> distances = new List<int>();      //A list containing all the lenghts of same pixels
            List<int> pixels = new List<int>();         //A list containing all the pixels that correspond to the lengths in 'distances' list
            int tempDistance = -1;                      //The length of one run of bits with the same value, while it is not saved yet: -1 becouse it will be increased before the first check
            byte lastPixel = source.GetPixelByte(0, 0, byteLayer);   //The pixel of the last checked pixel, to compare with the current pixel: set value to the value of the first pixel so the first check will succeed

            //Loop trough all rows of pixels
            for (int x = 0; x < source.width; x++)
            {
                //Loop trough all pixels in this row
                for (int y = 0; y < source.height; y++)
                {
                    //Take value of the current pixel
                    byte currentPixel = source.GetPixelByte(x, y, byteLayer);

                    //If the value of the bit of this pixel matches the value of the bit of the previous pixel
                    if (currentPixel == lastPixel)
                    {
                        //Values are the same, so increase current run
                        tempDistance++;
                    }
                    else
                    {
                        //Values are not the same, so save the run
                        distances.Add(tempDistance);
                        pixels.Add(lastPixel);

                        //Set the bit value for the next comparison to the bit value of this pixel
                        lastPixel = currentPixel;

                        //Reset the run length for the new run
                        tempDistance = 0;
                    }
                }
            }
            //Save the last run becouse this never happens in the loop
            distances.Add(tempDistance);
            pixels.Add(lastPixel);

            //Compress the array of run lengths using different techniques
            BitStreamFIFO pixelStream = VaryingIntArrayCompressor.Compress(pixels.ToArray());

            //Compress the array of run lengths using different techniques
            BitStreamFIFO lengthStream = VaryingIntArrayCompressor.Compress(distances.ToArray());

            //Combine the compressed data of the runs with the compressed data of the pixel values, then return the BitStream
            return BitStreamFIFO.Merge(pixelStream, lengthStream).ToByteArray();
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBytewise Decompress(byte[] inBytes, AccessibleBitmapBytewise inBitmap, out byte[] restBytes, int byteLayer)
        {
            //Create a BitStream from the input bytes becouse the decompress functions need this as input
            BitStreamFIFO bitStream = new BitStreamFIFO(inBytes);

            //Decompress the BitStream to a queue of integers for the lengths
            Queue<int> pixels = new Queue<int>(VaryingIntArrayCompressor.Decompress(ref bitStream));

            //Decompress the BitStream to a queue of integers for the pixel values
            Queue<int> runs = new Queue<int>(VaryingIntArrayCompressor.Decompress(ref bitStream));

            //Initialize
            int pixelsToGo = runs.Dequeue() + 1;        //The amount of pixels that should be written before the next run starts
            byte currentPixel = (byte)pixels.Dequeue(); //The pixel value of the current run: initialize with the first pixel value

            //Loop trough all rows of pixels
            for (int x = 0; x < inBitmap.width; x++)
            {
                //Loop trough all pixels in this row
                for (int y = 0; y < inBitmap.height; y++)
                {
                    //Set the bit of the current pixel to the value of the current run
                    inBitmap.SetPixelByte(x, y, byteLayer, currentPixel);

                    //Decrease the length of the current run
                    pixelsToGo--;

                    //If the end of the run has been reached
                    if (pixelsToGo == 0 && (x * y != (inBitmap.height - 1) * (inBitmap.width - 1)))
                    {
                        //Read the new run length from the BitStream
                        pixelsToGo = runs.Dequeue() + 1;

                        //Take a byte from the queue of pixel values to put in the current channel
                        currentPixel = (byte)pixels.Dequeue();
                    }
                }
            }

            //Remove the bytes used for this channel from the incoming byte array and pass the rest of them to the next channel
            restBytes = new byte[bitStream.Length / 8];
            Array.Copy(inBytes, inBytes.Length - restBytes.Length, restBytes, 0, restBytes.Length);

            //Return the image as aBitmap
            return inBitmap;
        }
    }
}
