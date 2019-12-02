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
            BitStreamFIFO bitStream = new BitStreamFIFO();
            //Get the amount of bits needed to store the longest run
            int bitDepth = (int)Math.Ceiling(Math.Log(source.Max() + 1, 2));
            bitDepth = bitDepth < 1 ? 1 : bitDepth;

            List<int> nonExisting = new List<int>();
            int minBits = bitDepth;
            for (int i = 0; i < source.Max(); i++)
            {
                if (!source.Contains(i))
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

            int[][] distancesByPower = new int[bitDepth][];
            for (int i = 0; i < distancesByPower.Length; i++)
            {
                int minVal = (int)Math.Pow(2, i) - 1;
                int maxVal = (int)Math.Pow(2, i + 1);
                distancesByPower[i] = new List<int>(source).FindAll(x => x < maxVal && x >= minVal).ToArray();
            }

            int chosenMinBits = bitDepth;
            int smallestSize = int.MaxValue;
            for (int i = 0; i <= bitDepth - minBits; i++)
            {
                int length = 0;

                int baseBits = bitDepth - i;
                for (int j = 0; j < baseBits; j++)
                {
                    length += distancesByPower[j].Length * baseBits;
                }

                for (int j = 0; j < i; j++)
                {
                    int currentBits = bitDepth - j;
                    length += distancesByPower[currentBits - 1].Length * (baseBits + currentBits);
                }

                if (length < smallestSize)
                {
                    smallestSize = length;
                    chosenMinBits = baseBits;
                }
            }


            //Write necessary info for decompressing to stream
            bitStream.Write((byte)chosenMinBits);
            Console.WriteLine(chosenMinBits);
            bitStream.Write((byte)(bitDepth - chosenMinBits));
            Console.WriteLine((bitDepth - chosenMinBits));
            for (int i = 0; i < bitDepth - chosenMinBits; i++)
            {
                bitStream.Write(nonExisting[i], chosenMinBits);
                Console.WriteLine(nonExisting[i]);
            }
            Console.WriteLine();

            //Write all runs to the stream
            foreach (int i in source)
            {
                int extraBits = 0;
                while (Math.Pow(2, chosenMinBits + extraBits) - 1 < i) { extraBits++; }
                if (extraBits > 0) { bitStream.Write(nonExisting[extraBits - 1], chosenMinBits); /*Console.WriteLine(nonExisting[extraBits - 1]);*/ }
                bitStream.Write(i, chosenMinBits + extraBits);
                //Console.WriteLine(i);
            }
            return bitStream;
        }

        public static int[] Decompress(BitStreamFIFO source)
        {
            //Read necessary info from BitStream
            int bitDepth = source.ReadByte();
            Console.WriteLine(bitDepth);
            int[] specialValues = new int[source.ReadByte()];
            Console.WriteLine(specialValues.Length);
            for (int i = 0; i < specialValues.Length; i++)
            {
                specialValues[i] = source.ReadInt(bitDepth);
                Console.WriteLine(specialValues[i]);
            }
            Console.WriteLine();

            List<int> outputList = new List<int>();
            while (source.Length > bitDepth)
            {
                int tmpLengthTmp = source.ReadInt(bitDepth);
                if (specialValues.Contains(tmpLengthTmp))
                {
                    int extraLength = Array.IndexOf(specialValues, tmpLengthTmp) + 1;
                    tmpLengthTmp = source.ReadInt(bitDepth + extraLength);
                }
                outputList.Add(tmpLengthTmp);
            }
            return outputList.ToArray();
        }
    }
}
