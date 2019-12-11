using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class RunLengthEncodingCompressor
    {
        //Compress aBitmap into byte array
        public static byte[] Compress(AccessibleBitmap source)
        {
            //Initialize
            List<int> distances = new List<int>();              //A list containing all the lenghts of same bits
            List<int> pixels = new List<int>();              //A list containing all the lenghts of same bits
            int tempDistance = -1;                              //The length of one run of bits with the same value, while it is not saved yet: -1 becouse it will be increased before the first check
            byte[] lastPixel = source.GetPixel(0, 0);  //The bit value of the last checked pixel, to compare with the current pixel: set value to the value of the first pixel so the first check will succeed

            //Loop trough all lines of pixels
            for (int y = 0; y < source.height; y++)
            {
                //Loop trough all pixels in this line
                for (int x = 0; x < source.width; x++)
                {
                    //Take value of the current pixel
                    byte[] currentPixel = source.GetPixel(x, y);

                    //If the value of the bit of this pixel matches the value of the bit of the previous pixel
                    if (currentPixel.SequenceEqual(lastPixel))
                    {
                        //Values are the same, so increase current run
                        tempDistance++;
                    }
                    else
                    {
                        //Values are not the same, so save the run
                        distances.Add(tempDistance);
                        pixels.AddRange(Array.ConvertAll(lastPixel, b => (int)b));

                        //Set the bit value for the next comparison to the bit value of this pixel
                        lastPixel = currentPixel;

                        //Reset the run length for the new run
                        tempDistance = 0;
                    }
                }
            }
            //Save the last run becouse this never happens in the loop
            distances.Add(tempDistance);
            pixels.AddRange(Array.ConvertAll(lastPixel, b => (int)b));

            //Compress the array of run lengths using different techniques
            BitStreamFIFO pixelStream = VaryingIntArrayCompressor.Compress(pixels.ToArray());
            BitStreamFIFO lengthStream = VaryingIntArrayCompressor.Compress(distances.ToArray());

            //Combine the inititial bit value with the compressed data of the runs, then return the BitStream
            return BitStreamFIFO.Merge(pixelStream, lengthStream).ToByteArray();
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            BitStreamFIFO bitStream = new BitStreamFIFO(source);

            AccessibleBitmap aBitmap = new AccessibleBitmap(width, height, pixelBytes);

            //Decompress the BitStream to a queue of integers
            Queue<int> pixels = new Queue<int>(VaryingIntArrayCompressor.Decompress(ref bitStream));
            Queue<int> runs = new Queue<int>(VaryingIntArrayCompressor.Decompress(ref bitStream));

            //Initialize
            int pixelsToGo = runs.Dequeue() + 1;    //The amount of pixels that should be written before the next run starts

            byte[] currentPixel = new byte[pixelBytes];
            for(int i = 0; i < pixelBytes; i++)
            {
                currentPixel[i] = (byte)pixels.Dequeue();
            }

            //Loop trough all lines of pixels
            for (int y = 0; y < height; y++)
            {
                //Loop trough all pixels in this line
                for (int x = 0; x < width; x++)
                {
                    //Set the bit of the current pixel to the value of the current run
                    aBitmap.SetPixel(x, y, currentPixel);

                    //Decrease the length of the current run
                    pixelsToGo--;

                    //If the end has been reached
                    if (pixelsToGo == 0 && (x * y != (height - 1) * (width - 1)))
                    {
                        //Read the new run length from the BitStream
                        pixelsToGo = runs.Dequeue() + 1;

                        //Toggle bit value, because a bit can just have 2 values, and this run cannot have the same value as the previous run
                        for (int i = 0; i < pixelBytes; i++)
                        {
                            currentPixel[i] = (byte)pixels.Dequeue();
                        }
                    }
                }
            }

            //Return the modified AccessibleBitmapBitwise so the rest of the channels can be added to complete it
            return aBitmap;
        }
    }
}
