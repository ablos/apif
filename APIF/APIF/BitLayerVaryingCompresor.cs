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
        public static byte[] Compress(AccessibleBitmap source)
        {
            BitStreamFIFO[] bitStreams = new BitStreamFIFO[source.pixelBytes * 8];

            for (int z = 0; z < source.pixelBytes * 8; z++)
            {
                bitStreams[z] = Uncompressed(source, z);

                BitStreamFIFO tmpStream = RunLength(source, z);
                if (tmpStream.Length < bitStreams[z].Length) { bitStreams[z] = tmpStream; }
            }

            return BitStreamFIFO.Merge(bitStreams).ToByteArray();
        }


        private static BitStreamFIFO Uncompressed(AccessibleBitmap source, int z)
        {
            BitStreamFIFO bitStream = new BitStreamFIFO();
            bitStream.Write(0, 3);
            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    bitStream.Write(source.GetPixelBit(x, y, z));
                }
            }
            return bitStream;
        }

        private static BitStreamFIFO RunLength(AccessibleBitmap source, int z)
        {
            BitStreamFIFO bitStream = new BitStreamFIFO();
            List<int> distances = new List<int>();
            int tempDistance = -1;
            bool lastVal = source.GetPixelBit(0, 0, z);

            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    bool currentBool = source.GetPixelBit(x, y, z);
                    if (currentBool == lastVal)
                    {
                        tempDistance++;
                    }
                    else
                    {
                        distances.Add(tempDistance);
                        lastVal = currentBool;
                        tempDistance = 0;
                    }
                }
            }
            distances.Add(tempDistance);

            bool initialVal = source.GetPixelBit(0, 0, z);
            int bitDepth = (int)Math.Ceiling(Math.Log(distances.Max(), 2));
            bitStream.Write(1, 3);
            bitStream.Write(initialVal);
            bitStream.Write((byte)bitDepth);

            foreach (int i in distances)
            {
                bitStream.Write(i, bitDepth);
            }

            return bitStream;
        }



        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            AccessibleBitmap aBitmap = new AccessibleBitmap(width, height, pixelBytes);
            BitStreamFIFO bitStream = new BitStreamFIFO(source);

            for (int z = 0; z < pixelBytes * 8; z++)
            {
                switch (bitStream.ReadInt(3))
                {
                    case 0:
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                aBitmap.SetPixelBit(x, y, z, bitStream.ReadBool());
                            }
                        }
                        break;

                    case 1:
                        bool currentVal = bitStream.ReadBool();
                        int bitDepth = bitStream.ReadByte();
                        int pixelsToGo = bitStream.ReadInt(bitDepth) + 1;

                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                aBitmap.SetPixelBit(x, y, z, currentVal);
                                pixelsToGo--;
                                if (pixelsToGo == 0 && (x * y != (height - 1) * (width - 1)))
                                {
                                    pixelsToGo = bitStream.ReadInt(bitDepth) + 1;
                                    currentVal = !currentVal;
                                }
                            }
                        }
                        break;

                    default:
                        throw new Exception("Decode type with this value does not exist");
                }
            }

            return aBitmap;
        }
    }
}
