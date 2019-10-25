using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class BitLayerVaryingCompresor
    {
        //Compress aBitmap into byte array
        public static byte[] Compress(AccessibleBitmap source)
        {
            BitStreamFIFO[] bitStreams = new BitStreamFIFO[source.pixelBytes * 8];

            //Iterate trough all layers of bitdepth
            for (int z = 0; z < source.pixelBytes * 8; z++)
            {
                bitStreams[z] = Uncompressed(source, z);

                //Compress layer using RunLength and replace previous compression if smaller
                BitStreamFIFO tmpStream = RunLength(source, z);
                if (tmpStream.Length < bitStreams[z].Length) { bitStreams[z] = tmpStream; }
            }

            //Merge all layers & return byte array
            return BitStreamFIFO.Merge(bitStreams).ToByteArray();
        }


        //Return bit layer uncompressed
        private static BitStreamFIFO Uncompressed(AccessibleBitmap source, int z)
        {
            BitStreamFIFO bitStream = new BitStreamFIFO();

            //Write 3-bit int to specify compression type & iterate trough all pixels of aBitmap
            bitStream.Write(0, 3);
            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    //Write bit of current pixel to stream
                    bitStream.Write(source.GetPixelBit(x, y, z));
                }
            }
            return bitStream;
        }

        //Return bit layer compressed using RunLength
        private static BitStreamFIFO RunLength(AccessibleBitmap source, int z)
        {
            //Initialize vars
            BitStreamFIFO bitStream = new BitStreamFIFO();
            List<int> distances = new List<int>();
            int tempDistance = -1;
            bool lastVal = source.GetPixelBit(0, 0, z);

            //Iterate trough pixels
            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    //Take value of pixel & compare with previous value
                    bool currentBool = source.GetPixelBit(x, y, z);
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
            bool initialVal = source.GetPixelBit(0, 0, z);
            int bitDepth = (int)Math.Ceiling(Math.Log(distances.Max(), 2));

            //Write necessary info for decompressing to stream
            bitStream.Write(1, 3);
            bitStream.Write(initialVal);
            bitStream.Write((byte)bitDepth);

            //Write all runs to the stream
            foreach (int i in distances)
            {
                bitStream.Write(i, bitDepth);
            }

            return bitStream;
        }



        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            //Create aBitmap for output & create BitStream from byte array for easy reading of bits
            AccessibleBitmap aBitmap = new AccessibleBitmap(width, height, pixelBytes);
            BitStreamFIFO bitStream = new BitStreamFIFO(source);

            //Iterate trough all bit layers
            for (int z = 0; z < pixelBytes * 8; z++)
            {
                //Read 3-bit int containing the compression type, and decompress using the correct method
                switch (bitStream.ReadInt(3))
                {
                    //Uncompressed
                    case 0:
                        //Iterate trough all pixels
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                //Read bit from bitstream & set bit for the current pixel
                                aBitmap.SetPixelBit(x, y, z, bitStream.ReadBool());
                            }
                        }
                        break;

                    //RunLength
                    case 1:
                        //Read necessary info from BitStream
                        bool currentVal = bitStream.ReadBool();
                        int bitDepth = bitStream.ReadByte();
                        int pixelsToGo = bitStream.ReadInt(bitDepth) + 1;

                        //Iterate trough all pixels
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                //Set the bit of the current pixel to the value of the current run
                                aBitmap.SetPixelBit(x, y, z, currentVal);

                                //Decrease the length of the current run & check if the end has bin reached
                                pixelsToGo--;
                                if (pixelsToGo == 0 && (x * y != (height - 1) * (width - 1)))
                                {
                                    //Read the new run length from the BitStream & reverse the run bit
                                    pixelsToGo = bitStream.ReadInt(bitDepth) + 1;
                                    currentVal = !currentVal;
                                }
                            }
                        }
                        break;

                    //Unknown compression type: error
                    default:
                        throw new Exception("Decode type with this value does not exist");
                }
            }

            return aBitmap;
        }
    }
}
