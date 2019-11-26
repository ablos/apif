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
            //Loop trough all layers of bytes
            AccessibleBitmapBytewise aBitmap = new AccessibleBitmapBytewise(source);
            byte[][] byteLayers = new byte[source.pixelBytes][];
            Parallel.For(0, source.pixelBytes, (z, state) => //for(int z = 0; z < source.pixelBytes; z++)
            {
                //Compress image using all different compression techniques
                byte[][] compressionTechniques = new byte[2][];
                Parallel.For(0, compressionTechniques.Length, (i, state2) =>
                {
                    switch (i)
                    {
                        //Uncompressed
                        case 0:
                            compressionTechniques[i] = UncompressedBitmapCompressorBytewise.Compress(aBitmap, z);
                            break;

                        case 1:
                            //compressionTechniques[i] = BitLayerVaryingCompressor.Compress(aBitmap, z);
                            compressionTechniques[i] = ByteArrayCompressorBytewise.Compress(aBitmap, z);
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

                //Merge the number of the compression type of this layer with corresponding byte array
                byteLayers[z] = new byte[compressionTechniques[smallestID].Length + 1];
                byteLayers[z][0] = (byte)smallestID;
                Array.Copy(compressionTechniques[smallestID], 0, byteLayers[z], 1, compressionTechniques[smallestID].Length);
            });

            //Combine all byte layers & return
            List<byte> output = new List<byte>();
            foreach (byte[] b in byteLayers)
            {
                output.AddRange(b);
            }
            return output.ToArray();
        }



        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            AccessibleBitmapBytewise outputBitmap = new AccessibleBitmapBytewise(new AccessibleBitmap(width, height, pixelBytes));
            byte[] inBytes;
            byte[] outBytes = source;
            for (int i = 0; i < pixelBytes; i++)
            {
                //Read compression type from byte array & remove that byte from the array
                inBytes = outBytes;
                int compressionType = inBytes[0];
                byte[] tmpArray = new byte[inBytes.Length - 1];
                Array.Copy(inBytes, 1, tmpArray, 0, tmpArray.Length);
                inBytes = tmpArray;

                //Decompress using the correct compression type
                switch (compressionType)
                {
                    //Uncompressed bitmap
                    case 0:
                        outputBitmap = UncompressedBitmapCompressorBytewise.Decompress(inBytes, outputBitmap, out outBytes, i);
                        break;

                    //BitLayerVarying
                    case 1:
                        outputBitmap = BitLayerVaryingCompressor.Decompress(inBytes, outputBitmap, out outBytes, i);
                        //outputBitmap = ByteArrayCompressorBytewise.Decompress(inBytes, outputBitmap, out outBytes, i);
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
