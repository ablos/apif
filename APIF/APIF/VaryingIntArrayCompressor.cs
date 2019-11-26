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
        public static BitStreamFIFO Compress(int[] source)
        {
            //Compress image using all different compression techniques
            BitStreamFIFO[] compressionTechniques = new BitStreamFIFO[2];
            Parallel.For(0, compressionTechniques.Length, (i, state) =>
            {
                switch (i)
                {
                    //VariableIntLength
                    case 0:
                        compressionTechniques[i] = VaryingIntLengthIntArrayCompressor.Compress(source);
                        break;

                    //Huffmann
                    case 1:
                        compressionTechniques[i] = HuffmanIntArrayCompressor.Compress(source);
                        break;

                    //To add a compression technique, add a new case like the existing ones and increase the length of new byte[??][]
                }
            });

            //Choose the smallest compression type
            int smallestID = 0;
            int smallestSize = int.MaxValue;
            for (int i = 0; i < compressionTechniques.Length; i++)
            {
                if (compressionTechniques[i].Length < smallestSize)
                {
                    smallestSize = compressionTechniques[i].Length;
                    smallestID = i;
                }
            }

            //Merge the info about the compression type with the compressed data & return
            BitStreamFIFO compressionInfo = new BitStreamFIFO();
            compressionInfo.Write(smallestID, 2);

            int lengthSaveLength = (int)Math.Ceiling(Math.Log(smallestSize, 2) / 8);
            compressionInfo.Write(lengthSaveLength, 2);
            compressionInfo.Write(smallestSize, lengthSaveLength * 8);

            return BitStreamFIFO.Merge(compressionInfo, compressionTechniques[smallestID]);
        }

        public static int[] Decompress(ref BitStreamFIFO source)
        {
            int compressionType = source.ReadInt(2);
            int lengthSaveLength = source.ReadInt(2) * 8;
            int arrayDataLength = source.ReadInt(lengthSaveLength);
            BitStreamFIFO arrayData = new BitStreamFIFO(source.ReadBoolArray(arrayDataLength));

            int[] intArray = new int[0];
            switch (compressionType)
            {
                //VariableIntLength
                case 0:
                    intArray = VaryingIntLengthIntArrayCompressor.DeCompress(arrayData);
                    break;

                //Huffmann
                case 1:
                    intArray = HuffmanIntArrayCompressor.DeCompress(arrayData);
                    break;

                //To add a compression technique, add a new case like the existing ones and increase the length of new byte[??][]
            }
            return intArray;
        }
    }
}
