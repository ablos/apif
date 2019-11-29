using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            //Get the amount of bits needed to store the longest run
            bool initialVal = source.GetPixelBit(0, 0, bitLayer);
            int bitDepth = (int)Math.Ceiling(Math.Log(distances.Max() + 1, 2));
            bitDepth = bitDepth < 1 ? 1 : bitDepth;

            //Find all numbers which aren't used in the array of runs, so they can be used switch in bit depth & find the corresponding minimal amount of bits which can be used
            List<int> nonExisting = new List<int>();
            int minBits = bitDepth;
            for (int i = 0; i < distances.Max(); i++)
            {
                if (!distances.Contains(i))
                {
                    nonExisting.Add(i);
                    minBits = (int)Math.Ceiling(Math.Log(i + 1, 2));
                    minBits = minBits < 1 ? 1 : minBits;
                    if (nonExisting.Count >= bitDepth - minBits)
                    {
                        break;
                    }
                }
            }

            //Find out for every amount of bits how many runs fit in a number, which can be used to calculate the size of this layer using a given minimum of bits
            int[][] distancesByPower = new int[bitDepth][];
            for (int i = 0; i < distancesByPower.Length; i++)
            {
                int minVal = (int)Math.Pow(2, i) - 1;
                int maxVal = (int)Math.Pow(2, i + 1);
                distancesByPower[i] = distances.FindAll(x => x < maxVal && x >= minVal).ToArray();
            }

            //Calculate the size of this layer using a every possible minimum of bits & choose the smallest amount
            int chosenMinBits = bitDepth;
            int smallestSize = int.MaxValue;
            //Calculate the size for every possible minimum bits
            for (int i = 0; i <= bitDepth - minBits; i++)
            {
                int length = 0;

                //Multiply the amount of runs which fit in the minimum amount of bits with the minimum amount of bits
                int baseBits = bitDepth - i;
                for (int j = 0; j < baseBits; j++)
                {
                    length += distancesByPower[j].Length * baseBits;
                }

                //Multiply the amount of runs which fit in a given amount of bits with that amount of bits
                for (int j = 0; j < i; j++)
                {
                    int currentBits = bitDepth - j;
                    length += distancesByPower[currentBits - 1].Length * (baseBits + currentBits);
                }

                //Save minimum bits if this minimum bits gives a smaller size than the previous smallest
                if (length < smallestSize)
                {
                    smallestSize = length;
                    chosenMinBits = baseBits;
                }
            }


            //Write necessary info for decompressing to stream
            bitStream.Write(initialVal);
            bitStream.Write((byte)chosenMinBits);
            bitStream.Write((byte)(bitDepth - chosenMinBits));
            for (int i = 0; i < bitDepth - chosenMinBits; i++)
            {
                bitStream.Write(nonExisting[i], chosenMinBits);
            }

            //Write all runs to the stream
            foreach (int i in distances)
            {
                int extraBits = 0;
                while (Math.Pow(2, chosenMinBits + extraBits) - 1 < i) { extraBits++; }
                if (extraBits > 0) { bitStream.Write(nonExisting[extraBits - 1], chosenMinBits); }
                bitStream.Write(i, chosenMinBits + extraBits);
            }
            return bitStream;
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBitwise Decompress(BitStreamFIFO inBits, AccessibleBitmapBitwise inBitmap, out BitStreamFIFO restBits, int bitLayer)
        {
            //Read necessary info from BitStream
            bool currentVal = inBits.ReadBool();
            int bitDepth = inBits.ReadByte();

            //Read all switching numbers
            int[] specialValues = new int[inBits.ReadByte()];
            for (int i = 0; i < specialValues.Length; i++)
            {
                specialValues[i] = inBits.ReadInt(bitDepth);
            }

            //Read the first run
            int tmpLengthTmp = inBits.ReadInt(bitDepth);
            if (specialValues.Contains(tmpLengthTmp))
            {
                int extraLength = Array.IndexOf(specialValues, tmpLengthTmp) + 1;
                tmpLengthTmp = inBits.ReadInt(bitDepth + extraLength);
            }
            int pixelsToGo = tmpLengthTmp + 1;

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
                        int tmpLength = inBits.ReadInt(bitDepth);
                        if (specialValues.Contains(tmpLength))
                        {
                            int extraLength = Array.IndexOf(specialValues, tmpLength) + 1;
                            tmpLength = inBits.ReadInt(bitDepth + extraLength);
                        }
                        pixelsToGo = tmpLength + 1;

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
