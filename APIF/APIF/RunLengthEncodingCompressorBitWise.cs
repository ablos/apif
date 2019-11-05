using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class RunLengthEncodingCompressorBitwise
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

            //Get info about the collection of runs, to make sure that the longest run fits in every int, while trying to keep the ints as short as possible
            bool initialVal = source.GetPixelBit(0, 0, bitLayer);
            int bitDepth = (int)Math.Ceiling(Math.Log(distances.Max() + 1, 2));
            bitDepth = bitDepth < 1 ? 1 : bitDepth;

            int nonExisting = 0;
            int minBits = int.MaxValue;
            for (int i = 0; i < distances.Count; i++)
            {
                if (!distances.Contains(i))
                {
                    nonExisting = i;
                    minBits = (int)Math.Ceiling(Math.Log(i + 1, 2));
                    minBits = minBits < 1 ? 1 : minBits;
                    break;
                }
            }

            //Write necessary info for decompressing to stream
            bitStream.Write(initialVal);
            bitStream.Write((byte)bitDepth);
            bitStream.Write(minBits < bitDepth);
            if (minBits < bitDepth)
            {
                bitDepth = bitDepth - 1;
                bitStream.Write(nonExisting, bitDepth);
            }

            //Write all runs to the stream
            foreach (int i in distances)
            {
                if(Math.Pow(2, bitDepth) < i)
                {
                    bitStream.Write(nonExisting, bitDepth);
                    bitStream.Write(i, bitDepth + 1);
                }
                else
                {
                    bitStream.Write(i, bitDepth);
                }
            }
            return bitStream;
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBitwise Decompress(BitStreamFIFO inBits, AccessibleBitmapBitwise inBitmap, out BitStreamFIFO restBits, int bitLayer)
        {
            //Read necessary info from BitStream
            bool currentVal = inBits.ReadBool();
            int bitDepth = inBits.ReadByte() - 1;
            int unExisting = 0;
            if (inBits.ReadBool()) { unExisting = inBits.ReadInt(bitDepth); }

            int pixelsToGo = inBits.ReadInt(bitDepth) + 1;
            if (pixelsToGo == unExisting + 1) { pixelsToGo = inBits.ReadInt(bitDepth + 1) + 1; }

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
                        pixelsToGo = inBits.ReadInt(bitDepth) + 1;
                        if (pixelsToGo == unExisting + 1) { pixelsToGo = inBits.ReadInt(bitDepth + 1) + 1; }
                        currentVal = !currentVal;
                    }
                }
            }

            restBits = inBits;
            return inBitmap;
        }
    }
}
