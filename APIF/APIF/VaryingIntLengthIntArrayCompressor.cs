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
        //Compress integer array into BitStream
        public static BitStreamFIFO Compress(int[] source)
        {
            //Get the amount of bits needed to store the longest run
            int invariableMinBits = (int)Math.Ceiling(Math.Log(source.Max() + 1, 2));
            invariableMinBits = invariableMinBits < 1 ? 1 : invariableMinBits;

            //Initialize
            List<int> nonExisting = new List<int>();    //Create a list to which all values that aren't present in the source array will be added
            int minBits = invariableMinBits;            //The minimum amount of bits used for variable save bits, related to the list of nonexisting values

            //Loop trough all possible values within the range of the source values
            for (int i = 0; i < source.Max(); i++)
            {
                //If a value is not present in the source array
                if (!source.Contains(i))
                {
                    //Add current value to the list
                    nonExisting.Add(i);

                    //Set the minimum amount of bits to save this value
                    minBits = (int)Math.Ceiling(Math.Log(i + 1, 2));
                    minBits = minBits < 1 ? 1 : minBits;

                    //If the amount of unused values is equal to the difference between invariableMinBits and minBits
                    if (nonExisting.Count >= invariableMinBits - minBits)
                    {
                        //Minbits is now the smallest possible amount of bits in which the source array can be saved variable, so stop checking for more unused values
                        break;
                    }
                }
            }


            //Divide the source values in categories with the same minimum amount of bits to be saved in

            //Create all categories
            int[][] distancesByPower = new int[invariableMinBits][];

            //Loop trough all categories
            for (int i = 0; i < distancesByPower.Length; i++)
            {
                //Define the smalles and biggest value of the current category
                int minVal = (int)Math.Pow(2, i) - 1;
                int maxVal = (int)Math.Pow(2, i + 1);

                //Add all values within the specified range to this category
                distancesByPower[i] = new List<int>(source).FindAll(x => x < maxVal && x >= minVal).ToArray();
            }


            //Find out which possible minimum amount of bits used will compress the most

            //Initialize
            int chosenMinBits = invariableMinBits;  //The chosen minimum amount of bits
            int smallestSize = int.MaxValue;        //The size of the compressed data when using the chosen minimum of bits

            //Loop trough all possible minimum amounts of bits
            for (int i = 0; i <= invariableMinBits - minBits; i++)
            {
                //Calculate the length of the current minimum amount of bits

                //Initialize
                int length = 0;                         //The total length
                int baseBits = invariableMinBits - i;   //Define the current minimum amount of bits

                //Add the length of all data that will be saved in the minimum amount of bits to the length variable
                for (int j = 0; j < baseBits; j++)
                {
                    length += distancesByPower[j].Length * baseBits;
                }

                //Loop trough all data that needs more bits to be saved than the minimum
                for (int j = 0; j < i; j++)
                {
                    //Define the minimum bits for this value
                    int currentBits = invariableMinBits - j;

                    //Add the minimum bits of this value + the general minimum bits to the length variable, becouse an extra value should be used to indicate the extra amount of bits
                    length += distancesByPower[currentBits - 1].Length * (baseBits + currentBits);
                }

                //If the length of this minimum amount of bits is smaller than the chosen amount of bits
                if (length < smallestSize)
                {
                    //Set this smallest amount of bits as the chosen amount of bits
                    smallestSize = length;
                    chosenMinBits = baseBits;
                }
            }


            //Create a new BitStream as output
            BitStreamFIFO bitStream = new BitStreamFIFO();

            //Write necessary info for decompressing to the output stream
            bitStream.Write((byte)chosenMinBits);                       //The chosen default bits to save values
            bitStream.Write((byte)(invariableMinBits - chosenMinBits)); //The amount of values that trigger an increase of bits per value

            //Write all values that trigger an increase of bits per value
            for (int i = 0; i < invariableMinBits - chosenMinBits; i++)
            {
                bitStream.Write(nonExisting[i], chosenMinBits);
            }


            //Write all values to the stream

            //Loop trough all values
            foreach (int i in source)
            {
                //Get the amount of extra bits needed to save this value
                int extraBits = 0;
                while (Math.Pow(2, chosenMinBits + extraBits) - 1 < i) { extraBits++; }

                //Write trigger value to stream if extra bits are needed
                if (extraBits > 0) { bitStream.Write(nonExisting[extraBits - 1], chosenMinBits); }

                //Write the value to the stream using the correct amount of bits
                bitStream.Write(i, chosenMinBits + extraBits);
            }

            //Return the compressed data
            return bitStream;
        }


        //Decompress BitStream into integer array
        public static int[] Decompress(BitStreamFIFO source)
        {
            //Read necessary info from BitStream
            int bitDepth = source.ReadByte();                   //The default bits to read values
            int[] specialValues = new int[source.ReadByte()];   //The amount of values that trigger an increase of bits to read

            //Read all values that trigger an increase of bits to read
            for (int i = 0; i < specialValues.Length; i++)
            {
                specialValues[i] = source.ReadInt(bitDepth);
            }

            //Create a list of ints as output
            List<int> outputList = new List<int>();

            //Read data while the input stream contains data
            while (source.Length >= bitDepth)
            {
                //Read an integer using the default amount of bits
                int tmpLengthTmp = source.ReadInt(bitDepth);

                //If the current value is a value that triggers an increase of bits to read
                if (specialValues.Contains(tmpLengthTmp))
                {
                    //Read a new value with the correct amount of bits
                    int extraLength = Array.IndexOf(specialValues, tmpLengthTmp) + 1;

                    //Replace the current value with the new value
                    tmpLengthTmp = source.ReadInt(bitDepth + extraLength);
                }

                //Add the current value to the output
                outputList.Add(tmpLengthTmp);
            }

            //Return output as an array of integers
            return outputList.ToArray();
        }
    }
}
