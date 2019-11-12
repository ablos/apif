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
            //Loop trough all layers of bytes
            AccessibleBitmapBitwise aBitmap = new AccessibleBitmapBitwise(source);
            BitStreamFIFO[] byteLayers = new BitStreamFIFO[8];
            Parallel.For(byteLayer * 8, byteLayer * 8 + 8, (z, state) => //for(int z = byteLayer * 8; z < byteLayer * 8 + 8; z++)
            {
                //Compress image using all different compression techniques
                BitStreamFIFO[] compressionTechniques = new BitStreamFIFO[3];
                Parallel.For(0, compressionTechniques.Length, (i, state2) =>
                {
                    switch (i)
                    {
                        //Uncompressed
                        case 0:
                            compressionTechniques[i] = UncompressedBitmapCompressorBitwise.Compress(aBitmap, z);
                            break;

                        //RunLengthEncodingBitwise
                        case 1:
                            compressionTechniques[i] = RunLengthEncodingCompressorBitwise.Compress(aBitmap, z);
                            break;

                        //RunLengthEncodingBitwiseVertical => 0,07% avarage better than horizontal only, but doubles compression time
                        //case 2:
                        //    compressionTechniques[i] = RunLengthEncodingCompressorBitwiseVertical.Compress(aBitmap, z);
                        //    break;

                        //To add a compression technique, add a new case like the existing ones and increase the length of new byte[??][]
                    }
                });

                //Choose the smallest compression type
                int smallestID = 0;
                int smallestSize = int.MaxValue;
                for (int i = 0; i < compressionTechniques.Length; i++)
                {
                    if (compressionTechniques[i] != null)
                    {
                        if (compressionTechniques[i].Length < smallestSize)
                        {
                            smallestSize = compressionTechniques[i].Length;
                            smallestID = i;
                        }
                    }
                }

                //Merge the number of the compression type of this layer with corresponding bitStream
                BitStreamFIFO tmpStream = new BitStreamFIFO();
                tmpStream.Write(smallestID, 3);
                byteLayers[z % 8] = BitStreamFIFO.Merge(tmpStream, compressionTechniques[smallestID]);
            });

            //Combine all byte layers & return
            return BitStreamFIFO.Merge(byteLayers).ToByteArray();
        }


        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBytewise Decompress(byte[] inBytes, AccessibleBitmapBytewise inBitmap, out byte[] restBytes, int byteLayer)
        {
            AccessibleBitmapBitwise outputBitmap = new AccessibleBitmapBitwise(inBitmap);
            BitStreamFIFO bitStream = new BitStreamFIFO(inBytes);

            //Loop trough all bit layers of current byte layer
            for (int i = byteLayer * 8; i < byteLayer * 8 + 8; i++)
            {
                //Read compression type & decompress using that technique
                int compressionType = bitStream.ReadInt(3);
                switch (compressionType)
                {
                    //Uncompressed bitmap
                    case 0:
                        outputBitmap = UncompressedBitmapCompressorBitwise.Decompress(bitStream, outputBitmap, out bitStream, i);
                        break;

                    //RunLengthEncodingBitwise
                    case 1:
                        outputBitmap = RunLengthEncodingCompressorBitwise.Decompress(bitStream, outputBitmap, out bitStream, i);
                        break;

                    //RunLengthEncodingBitwiseVertical
                    case 2:
                        outputBitmap = RunLengthEncodingCompressorBitwiseVertical.Decompress(bitStream, outputBitmap, out bitStream, i);
                        break;

                    //To add a decompression type add a new case like the existing ones

                    //Unknown compression type: error
                    default:
                        throw new Exception("Unexisting compression type");
                }
            }
            restBytes = new byte[(int)Math.Floor(bitStream.Length / 8.0)];
            Array.Copy(inBytes, inBytes.Length - (int)Math.Floor(bitStream.Length / 8.0), restBytes, 0, restBytes.Length);
            return new AccessibleBitmapBytewise(outputBitmap.GetAccessibleBitmap());
        }
    }
}
