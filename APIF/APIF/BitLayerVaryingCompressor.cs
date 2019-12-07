using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class BitLayerVaryingCompressor
    {
        //Compress aBitmap into byte array
        public static byte[] Compress(AccessibleBitmapBytewise source, int byteLayer)
        {
            //Loop trough all layers of bits, where possible at the same time
            AccessibleBitmapBitwise aBitmap = new AccessibleBitmapBitwise(source);
            BitStreamFIFO[] byteLayers = new BitStreamFIFO[8];
            Parallel.For(byteLayer * 8, byteLayer * 8 + 8, (z, state) => //for(int z = byteLayer * 8; z < byteLayer * 8 + 8; z++)
            {
                //Compress image using all different compression techniques, where possible at the same time
                BitStreamFIFO[] compressionTechniques = new BitStreamFIFO[3];
                Parallel.For(0, compressionTechniques.Length, (i, state2) =>
                {
                    switch (i)
                    {
                        //Uncompressed (only used if no compression technique is smaller)
                        case 0:
                            compressionTechniques[i] = UncompressedBitmapCompressorBitwise.Compress(aBitmap, z);
                            break;

                        //Run length compression: save the length of a sequence of bit values instead of saving them seperately
                        case 1:
                            compressionTechniques[i] = RunLengthEncodingCompressorBitwise.Compress(aBitmap, z);
                            break;

                        //Compress bit channel as an integer array using several techniques, using 8-bit integers
                        case 2:
                            compressionTechniques[i] = ByteArrayCompressorBitwise.Compress(aBitmap, z);
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

                //Merge the number of the compression type of this layer with corresponding bitStream
                BitStreamFIFO tmpStream = new BitStreamFIFO();
                tmpStream.Write(smallestID, 3);    //This 3-bit integer indicates which technique the decompressor should use, and should be before the image data
                byteLayers[z % 8] = BitStreamFIFO.Merge(tmpStream, compressionTechniques[smallestID]);
            });

            //Combine all bitstreams & convert the result to a byte array
            byte[] outputStream = BitStreamFIFO.Merge(byteLayers).ToByteArray();

            //Return the data of all the bit channels combined
            return outputStream;
        }


        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBytewise Decompress(byte[] inBytes, AccessibleBitmapBytewise inBitmap, out byte[] restBytes, int byteLayer)
        {
            //Create a AccessibleBitmapbitwise class from the incoming AccessibleBitmapbytewise class, for better access to individual bits
            AccessibleBitmapBitwise outputBitmap = new AccessibleBitmapBitwise(inBitmap);

            //Create a BitStreamFIFO class from the incoming bytes, to feed into the decompression algorithms
            BitStreamFIFO bitStream = new BitStreamFIFO(inBytes);

            //Loop trough all bit layers of current byte layer
            for (int i = byteLayer * 8; i < byteLayer * 8 + 8; i++)
            {
                //Read compression type as a 3-bit integer
                int compressionType = bitStream.ReadInt(3);

                //Decompress using the correct compression type
                switch (compressionType)
                {
                    //Uncompressed
                    case 0:
                        outputBitmap = UncompressedBitmapCompressorBitwise.Decompress(bitStream, outputBitmap, out bitStream, i);
                        break;

                    //Run length encoding
                    case 1:
                        outputBitmap = RunLengthEncodingCompressorBitwise.Decompress(bitStream, outputBitmap, out bitStream, i);
                        break;

                    //Bit channel compressed as 8-bit integers
                    case 2:
                        outputBitmap = ByteArrayCompressorBitwise.Decompress(bitStream, outputBitmap, out bitStream, i);
                        break;

                    //To add a decompression type add a new case like the existing ones

                    //Unknown compression type: error
                    default:
                        throw new Exception("Unexisting compression type");
                }
            }
            //Remove the bytes used for this channel from the incoming byte array and pass the rest of them to the next channel
            restBytes = new byte[bitStream.Length / 8];
            Array.Copy(inBytes, inBytes.Length - (bitStream.Length / 8), restBytes, 0, restBytes.Length);

            //Return the modified bitmap as AccessibleBitmapbytewise so the rest of the channels can be added to complete it
            return new AccessibleBitmapBytewise(outputBitmap.GetAccessibleBitmap());
        }
    }
}
