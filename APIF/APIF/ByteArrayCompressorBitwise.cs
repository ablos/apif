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
            bool[] tmpBools = new bool[8];      //
            int index = 0;


            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    //Write bit of current pixel to stream
                    tmpBools[index] = source.GetPixelBit(x, y, bitLayer);
                    index++;
                    if(index == 8)
                    {
                        index = 0;
                        ints.Add(new BitStreamFIFO().BoolArrayToInt(tmpBools));
                    }
                }
            }
            if(index > 0)
            {
                ints.Add(new BitStreamFIFO().BoolArrayToInt(tmpBools));
            }
            BitStreamFIFO bitStream = VaryingIntArrayCompressor.Compress(ints.ToArray());
            //Console.WriteLine(bitStream.Length + " " + bitLayer);
            return bitStream;
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBitwise Decompress(BitStreamFIFO inBits, AccessibleBitmapBitwise inBitmap, out BitStreamFIFO restBits, int bitLayer)
        {
            int beginLength = inBits.Length;
            //Console.WriteLine("kinker");
            int[] ints = VaryingIntArrayCompressor.Decompress(ref inBits);
            int intIndex = 0;
            bool[] tmpBools = new bool[8];
            int boolIndex = 8;
            for (int y = 0; y < inBitmap.height; y++)
            {
                for (int x = 0; x < inBitmap.width; x++)
                {
                    if(boolIndex == 8)
                    {
                        boolIndex = 0;
                        tmpBools = new BitStreamFIFO().IntToBoolArray(ints[intIndex], 8);
                        intIndex++;
                    }
                    inBitmap.SetPixelBit(x, y, bitLayer, tmpBools[boolIndex]);
                    boolIndex++;
                }
            }
            restBits = inBits;
            //Console.WriteLine((beginLength - restBits.Length) + " " + bitLayer);
            return inBitmap;
        }
    }
}
