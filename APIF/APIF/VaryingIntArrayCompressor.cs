using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class VaryingIntArrayCompressor
    {
        //Compress integer array into BitStream
        public static BitStreamFIFO Compress(int[] source)
        {
            //Compress the array of integers using all different compression techniques, where possible at the same time
            BitStreamFIFO[] compressionTechniques = new BitStreamFIFO[2];
            Parallel.For(0, compressionTechniques.Length, (i, state) =>
            {
                switch (i)
                {
                    //Huffmann: generate a dictionary of all used integer values where more frequent values have a code with less bits
                    case 0:
                        compressionTechniques[i] = HuffmanIntArrayCompressor.Compress(source);
                        break;

                    //Variable int length: save smaller integers with less bits than bigger integers
                    case 1:
                        compressionTechniques[i] = VaryingIntLengthIntArrayCompressor.Compress(source);
                        break;

                    //To add a compression technique, add a new case like the existing ones and increase the length of new byte[??][]
                }
            });

            //Choose the smallest compression type

            //Initialize
            int smallestID = 0;                 //The ID of the smallest compression type
            int smallestSize = int.MaxValue;    //The size ofthe smallest compression type: int.MaxValue is assigned to make sure that the first compression to be checked will be smaaller than this value

            //Loop trough all saved compression techniques
            for (int i = 0; i < compressionTechniques.Length; i++)
            {
                //If the current technique is smaller than the smallest technique which has been checked
                if (compressionTechniques[i].Length < smallestSize)
                {
                    //Mark this technique as smallest
                    smallestSize = compressionTechniques[i].Length;
                    smallestID = i;
                }
            }

            //Create a new BitStream to write necessary information for the decompressor
            BitStreamFIFO compressionInfo = new BitStreamFIFO();

            //Calculate the amount of bits needed to save the total length in bits of the compressed data
            int lengthSaveLength = (int)Math.Ceiling(Math.Log(smallestSize, 2) / 8);

            //Write data to the compression info
            compressionInfo.Write(smallestID, 2);                       //The compression technique used
            compressionInfo.Write(lengthSaveLength, 6);                 //The amount of bits needed to save the total length in bits of the compressed data
            compressionInfo.Write(smallestSize, lengthSaveLength * 8);  //The total length in bits of the compressed data

            //Merge the info about the compression type with the compressed data & return
            return BitStreamFIFO.Merge(compressionInfo, compressionTechniques[smallestID]);
        }

        //Decompress BitStream into integer array
        public static int[] Decompress(ref BitStreamFIFO source)
        {
            //Read necessary info for decompression
            int compressionType = source.ReadInt(2);                //The compression technique used
            int lengthSaveLength = source.ReadInt(6) * 8;           //The amount of bits needed to save the total length in bits of the compressed data
            int arrayDataLength = source.ReadInt(lengthSaveLength); //The total length in bits of the compressed data

            //Create a new BitStream of correct length from the incoming BitStream
            BitStreamFIFO arrayData = new BitStreamFIFO(source.ReadBoolArray(arrayDataLength));

            //Decompress using the correct compression type
            int[] intArray = new int[0];
            switch (compressionType)
            {
                //Huffmann
                case 0:
                    intArray = HuffmanIntArrayCompressor.DeCompress(arrayData);
                    break;

                //Variable int length
                case 1:
                    intArray = VaryingIntLengthIntArrayCompressor.Decompress(arrayData);
                    break;

                //To add a compression technique, add a new case like the existing ones and increase the length of new byte[??][]
            }

            //Return the decompressed array of integers
            return intArray;
        }
    }
}
