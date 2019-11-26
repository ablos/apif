using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class VaryingIntLengthIntArrayCompressor
    {
        public static BitStreamFIFO Compress(int[] source)
        {
            //Get the amount of bits needed to store the longest run
            int bitDepth = (int)Math.Ceiling(Math.Log(source.Max() + 1, 2));
            bitDepth = bitDepth < 1 ? 1 : bitDepth;

            //Find all numbers which aren't used in the array of runs, so they can be used switch in bit depth & find the corresponding minimal amount of bits which can be used
            List<int> nonExisting = new List<int>();
            int minBits = bitDepth;
            for (int i = 0; i < source.Max(); i++)
            {
                if (!new List<int>(source).Contains((byte)i))
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
                distancesByPower[i] = new List<int>(source).FindAll(x => x < maxVal && x >= minVal).ToArray();
            }

            //Calculate the size of this layer using a every possible minimum of bits & choose the smallest amount
            int chosenMinBits = bitDepth;
            int smallestSize = int.MaxValue;
            //Calculate the size for every possible minimum bits
            //Console.WriteLine(source.Length * 8);
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
                //Console.WriteLine(baseBits + " - " + length);
            }

            //Write necessary info for decompressing to stream
            BitStreamFIFO bitStream = new BitStreamFIFO();
            bitStream.Write((byte)chosenMinBits);
            Console.WriteLine(chosenMinBits);
            bitStream.Write((byte)(bitDepth - chosenMinBits));
            Console.WriteLine((bitDepth - chosenMinBits));
            for (int i = 0; i < bitDepth - chosenMinBits - 1; i++)
            {
                bitStream.Write(nonExisting[i], chosenMinBits);
                Console.WriteLine(nonExisting[i]);
            }

            //Write all runs to the stream
            foreach (int i in source)
            {
                int extraBits = 0;
                while (Math.Pow(2, chosenMinBits + extraBits) - 1 < i) { extraBits++; }
                if (extraBits > 0) { bitStream.Write(nonExisting[extraBits - 1], chosenMinBits); }
                bitStream.Write(i, chosenMinBits + extraBits);
            }
            Console.WriteLine(bitStream.Length);
            Console.WriteLine(source.Length);
            return bitStream;
        }

        public static int[] DeCompress(BitStreamFIFO source)
        {
            Console.WriteLine(source.Length);
            int bitDepth = source.ReadByte();
            Console.WriteLine(bitDepth);

            //Read all switching numbers
            int[] specialValues = new int[source.ReadByte()];
            Console.WriteLine(specialValues.Length);
            for (int i = 0; i < specialValues.Length - 1; i++)
            {
                specialValues[i] = source.ReadInt(bitDepth);
                Console.WriteLine(specialValues[i]);
            }

            //Read the first run
            List<int> output = new List<int>();
            //while (source.Length > bitDepth)
            for(int i = 0; i < 518400; i++)
            {
                int tmpLengthTmp = source.ReadInt(bitDepth);
                if (specialValues.Contains(tmpLengthTmp))
                {
                    int extraLength = Array.IndexOf(specialValues, tmpLengthTmp) + 1;
                    tmpLengthTmp = source.ReadInt(bitDepth + extraLength);
                }
                output.Add(tmpLengthTmp);
                Console.WriteLine(tmpLengthTmp);
            }

            Console.WriteLine(output.Count);
            return output.ToArray();
        }
    }
}
