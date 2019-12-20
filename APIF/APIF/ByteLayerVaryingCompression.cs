using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class ByteLayerVaryingCompression
    {
        //Compress aBitmap into byte array
        public static byte[] Compress(AccessibleBitmap source)
        {
            //Loop trough all layers of bytes, where possible at the same time
            AccessibleBitmapBytewise aBitmap = new AccessibleBitmapBytewise(source);
            byte[][] byteLayers = new byte[source.pixelBytes][];
            Parallel.For(0, source.pixelBytes, (z, state) => //for(int z = 0; z < source.pixelBytes; z++)
            {
                //Compress image using all different compression techniques, where possible at the same time
                byte[][] compressionTechniques = new byte[5][];
                Parallel.For(0, compressionTechniques.Length, (i, state2) =>
                {
                    switch (i)
                    {
                        //Uncompressed (only used if no compression technique is smaller)
                        case 0:
                            compressionTechniques[i] = UncompressedBitmapCompressorBytewise.Compress(aBitmap, z);
                            break;

                        //Split color channel in its bit channels and apply compression over them
                        case 1:
                            compressionTechniques[i] = BitLayerVaryingCompressor.Compress(aBitmap, z);
                            break;

                        //Compress color channel as an integer array using several techniques
                        case 2:
                            compressionTechniques[i] = ByteArrayCompressorBytewise.Compress(aBitmap, z);
                            break;

                        //Run length compression: save the length of a sequence of pixels with the same color instead of saving them seperately
                        case 3:
                            compressionTechniques[i] = RunLengthEncodingCompressorBytewise.Compress(aBitmap, z);
                            break;

                        //Run length compression vertical: run length compression, but scan the pixels horizontally, becouse with some images this yields better results
                        case 4:
                            compressionTechniques[i] = RunLengthEncodingCompressorVerticalBytewise.Compress(aBitmap, z);
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

                //Merge the number of the compression type of this layer with corresponding byte array
                byteLayers[z] = new byte[compressionTechniques[smallestID].Length + 1];
                byteLayers[z][0] = (byte)smallestID;    //This byte indicates which technique the decompressor should use, and should be before the image data
                Array.Copy(compressionTechniques[smallestID], 0, byteLayers[z], 1, compressionTechniques[smallestID].Length);
            });

            //Combine all byte layers by looping trough all of them and adding them after each other
            List<byte> output = new List<byte>();
            foreach (byte[] b in byteLayers)
            {
                output.AddRange(b);
            }

            //Return the data of all the color channels combined
            return output.ToArray();
        }



        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            //Create a empty AccessibleBitmapBytewise class with the correct dimensions and color channels
            AccessibleBitmapBytewise outputBitmap = new AccessibleBitmapBytewise(new AccessibleBitmap(width, height, pixelBytes));

            //Initialize
            byte[] inBytes;             //Stores the bytes to feed to the decompressor
            byte[] outBytes = source;   //Stores the bytes which are left by the previous decompressed channel: starts with all the input data

            //Loop trough all color channels
            for (int i = 0; i < pixelBytes; i++)
            {
                //Read compression type from outBytes & copy the rest of them to inBytes so they can be used for the next channel
                int compressionType = outBytes[0];
                inBytes = new byte[outBytes.Length - 1];
                Array.Copy(outBytes, 1, inBytes, 0, inBytes.Length);

                //Decompress using the correct compression type
                switch (compressionType)
                {
                    //Uncompressed
                    case 0:
                        outputBitmap = UncompressedBitmapCompressorBytewise.Decompress(inBytes, outputBitmap, out outBytes, i);
                        break;

                    //Individual compressed bit channels added together
                    case 1:
                        outputBitmap = BitLayerVaryingCompressor.Decompress(inBytes, outputBitmap, out outBytes, i);
                        break;

                    //Color channel compressed as integers
                    case 2:
                        outputBitmap = ByteArrayCompressorBytewise.Decompress(inBytes, outputBitmap, out outBytes, i);
                        break;

                    //Run length compression
                    case 3:
                        outputBitmap = RunLengthEncodingCompressorBytewise.Decompress(inBytes, outputBitmap, out outBytes, i);
                        break;

                    //Run length encoding vertical
                    case 4:
                        outputBitmap = RunLengthEncodingCompressorVerticalBytewise.Decompress(inBytes, outputBitmap, out outBytes, i);
                        break;

                    //To add a decompression type add a new case like the existing ones

                    //Unknown compression type: error
                    default:
                        throw new Exception("Unexisting compression type");
                }
            }
            return outputBitmap.GetAccessibleBitmap();
        }
    }
}
