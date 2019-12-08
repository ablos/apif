using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class ByteArrayCompressorBitwise
    {
        //Compress aBitmap into byte array
        public static BitStreamFIFO Compress(AccessibleBitmapBitwise source, int bitLayer)
        {
            //Initialize
            List<int> ints = new List<int>();   //List of all integers generated from the bits of this channel
            bool[] tmpBools = new bool[8];      //The collection of bits that will be converted to an interger
            int index = 0;                      //The index in tmpBools where a new bit should be inserted

            //Loop trough all lines of pixels
            for (int y = 0; y < source.height; y++)
            {
                //Loop trough all pixels in this line
                for (int x = 0; x < source.width; x++)
                {
                    //Write bit of current pixel to the correct position in tmpBools
                    tmpBools[index] = source.GetPixelBit(x, y, bitLayer);

                    //Increase index for next bit
                    index++;

                    //If index is 8, tmpBools is full
                    if(index == 8)
                    {
                        //Set index to 0, so the next bit will be written to the start
                        index = 0;

                        //Convert tmpBools to an integer & add the result to the list of integers
                        ints.Add(new BitStreamFIFO().BoolArrayToInt(tmpBools));
                    }
                }
            }

            //If index is not 0, it has not been reset at last, so tmpBools should be saved for the last bits it contains
            if(index > 0)
            {
                //Convert tmpBools to an integer & add the result to the list of integers
                ints.Add(new BitStreamFIFO().BoolArrayToInt(tmpBools));
            }

            //Compress the obtained array of integers using different techniques
            BitStreamFIFO bitStream = VaryingIntArrayCompressor.Compress(ints.ToArray());

            //Return the output array
            return bitStream;
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBitwise Decompress(BitStreamFIFO inBits, AccessibleBitmapBitwise inBitmap, out BitStreamFIFO restBits, int bitLayer)
        {
            //Decompress the incoming data to an array of integers
            int[] ints = VaryingIntArrayCompressor.Decompress(ref inBits);

            //Initialize
            int intIndex = 0;               //The index in the array of intergers, from where the next integer should be taken
            bool[] tmpBools = new bool[8];  //The array of bits from which bits will be read to be applied to pixels
            int boolIndex = 8;              //The index in the array of bits, from where the next bit should be taken


            //Add the data from the array of integers to the incoming bitmap

            //Loop trough all lines of pixels
            for (int y = 0; y < inBitmap.height; y++)
            {
                //Loop trough all pixels in this line
                for (int x = 0; x < inBitmap.width; x++)
                {
                    //If index is 8, all bits are used, so tmpBools should be renewed by using the next integer in the array of integers
                    if (boolIndex == 8)
                    {
                        //Reset the index to 0, so the next bit will be read from the start
                        boolIndex = 0;

                        //Convert the next integer in the array to a bool array, and write it to tmpBools
                        tmpBools = new BitStreamFIFO().IntToBoolArray(ints[intIndex], 8);

                        //Increase the index of the integer array, so the next integer will be read correctly
                        intIndex++;
                    }

                    //Write the bit of this channel from the correct position in the array of bits to the current pixel
                    inBitmap.SetPixelBit(x, y, bitLayer, tmpBools[boolIndex]);

                    //Increase index for next bit
                    boolIndex++;
                }
            }

            //Set the output BitStream to the remaining bits of the input BitStream
            restBits = inBits;

            //Return the modified AccessibleBitmapBitwise so the rest of the channels can be added to complete it
            return inBitmap;
        }
    }
}
