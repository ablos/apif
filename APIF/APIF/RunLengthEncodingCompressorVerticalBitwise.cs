using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class RunLengthEncodingCompressorVerticalBitwise
    {
        //Compress aBitmap into byte array
        public static BitStreamFIFO Compress(AccessibleBitmapBitwise source, int bitLayer)
        {
            //Initialize
            List<int> distances = new List<int>();              //A list containing all the lenghts of same bits
            int tempDistance = -1;                              //The length of one run of bits with the same value, while it is not saved yet: -1 becouse it will be increased before the first check
            bool lastVal = source.GetPixelBit(0, 0, bitLayer);  //The bit value of the last checked pixel, to compare with the current pixel: set value to the value of the first pixel so the first check will succeed

            //Loop trough all rows of pixels
            for (int x = 0; x < source.width; x++)
            {
                //Loop trough all pixels in this row
                for (int y = 0; y < source.height; y++)
                {
                    //Take value of the current pixel
                    bool currentBool = source.GetPixelBit(x, y, bitLayer);

                    //If the value of the bit of this pixel matches the value of the bit of the previous pixel
                    if (currentBool == lastVal)
                    {
                        //Values are the same, so increase current run
                        tempDistance++;
                    }
                    else
                    {
                        //Values are not the same, so save the run
                        distances.Add(tempDistance);

                        //Set the bit value for the next comparison to the bit value of this pixel
                        lastVal = currentBool;

                        //Reset the run length for the new run
                        tempDistance = 0;
                    }
                }
            }
            //Save the last run becouse this never happens in the loop
            distances.Add(tempDistance);

            //Save the bit value of the first pixel, because the decompressor needs to know this
            bool initialVal = source.GetPixelBit(0, 0, bitLayer);

            //Compress the array of run lengths using different techniques
            BitStreamFIFO bitStream = VaryingIntArrayCompressor.Compress(distances.ToArray());

            //Combine the inititial bit value with the compressed data of the runs, then return the BitStream
            return BitStreamFIFO.Merge(new BitStreamFIFO(new bool[] { initialVal }), bitStream);
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBitwise Decompress(BitStreamFIFO inBits, AccessibleBitmapBitwise inBitmap, out BitStreamFIFO restBits, int bitLayer)
        {
            //Read necessary info from BitStream
            bool currentVal = inBits.ReadBool();    //The bit value of the first run

            //Decompress the BitStream to a queue of integers
            Queue<int> runs = new Queue<int>(VaryingIntArrayCompressor.Decompress(ref inBits));

            //Initialize
            int pixelsToGo = runs.Dequeue() + 1;    //The amount of pixels that should be written before the next run starts

            //Loop trough all rows of pixels
            for (int x = 0; x < inBitmap.width; x++)
            {
                //Loop trough all pixels in this row
                for (int y = 0; y < inBitmap.height; y++)
                {
                    //Set the bit of the current pixel to the value of the current run
                    inBitmap.SetPixelBit(x, y, bitLayer, currentVal);

                    //Decrease the length of the current run
                    pixelsToGo--;

                    //If the end of the run has been reached
                    if (pixelsToGo == 0 && (x * y != (inBitmap.height - 1) * (inBitmap.width - 1)))
                    {
                        //Read the new run length from the BitStream
                        pixelsToGo = runs.Dequeue() + 1;

                        //Toggle bit value, because a bit can just have 2 values, and this run cannot have the same value as the previous run
                        currentVal = !currentVal;
                    }
                }
            }

            //Set the output BitStream to the remaining bits of the input BitStream
            restBits = inBits;

            //Return the modified AccessibleBitmapBitwise so the rest of the channels can be added to complete it
            return inBitmap;
        }
    }
}
