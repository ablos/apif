using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class RunLengthEncodingCompressorBitwise2
    {
        //Compress aBitmap into byte array
        public static BitStreamFIFO Compress(AccessibleBitmapBitwise source, int bitLayer)
        {
            //Initialize vars
            BitStreamFIFO bitStream = new BitStreamFIFO();
            List<int> distances = new List<int>();
            int tempDistance = -1;
            bool lastVal = source.GetPixelBit(0, 0, bitLayer);

            //Iterate trough pixels
            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    //Take value of pixel & compare with previous value
                    bool currentBool = source.GetPixelBit(x, y, bitLayer);
                    if (currentBool == lastVal)
                    {
                        //Values are the same, so increase current run
                        tempDistance++;
                    }
                    else
                    {
                        //Values are not the same, so save the run and create a new one
                        distances.Add(tempDistance);
                        lastVal = currentBool;
                        tempDistance = 0;
                    }
                }
            }
            //Save the last run becouse this never happens in the loop
            distances.Add(tempDistance);


            bool initialVal = source.GetPixelBit(0, 0, bitLayer);
            bitStream.Write(initialVal);

            BitStreamFIFO output = BitStreamFIFO.Merge(bitStream, VaryingIntArrayCompressor.Compress(distances.ToArray()));
            return output;
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBitwise Decompress(BitStreamFIFO inBits, AccessibleBitmapBitwise inBitmap, out BitStreamFIFO restBits, int bitLayer)
        {
            //Read necessary info from BitStream
            bool currentVal = inBits.ReadBool();
            Queue<int> runs = new Queue<int>(VaryingIntArrayCompressor.Decompress(ref inBits));

            int pixelsToGo = runs.Dequeue() + 1;

            //Iterate trough all pixels
            for (int y = 0; y < inBitmap.height; y++)
            {
                for (int x = 0; x < inBitmap.width; x++)
                {
                    //Set the bit of the current pixel to the value of the current run
                    inBitmap.SetPixelBit(x, y, bitLayer, currentVal);

                    //Decrease the length of the current run & check if the end has bin reached
                    pixelsToGo--;
                    if (pixelsToGo == 0 && (x * y != (inBitmap.height - 1) * (inBitmap.width - 1)))
                    {
                        //Read the new run length from the BitStream & reverse the run bit
                        pixelsToGo = runs.Dequeue() + 1;

                        //Toggle bit value
                        currentVal = !currentVal;
                    }
                }
            }

            //Return rest of bits & return bitmap
            restBits = inBits;
            return inBitmap;
        }
    }
}
