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
            BitStreamFIFO bitStream = new BitStreamFIFO();

            for (int k = 0; k < source.pixelBytes * 8; k++)
            {
                for (int i = 0; i < source.height; i++)
                {
                    for (int j = 0; j < source.width; j++)
                    {
                        bitStream.Write(source.GetPixelBit(j, i, k));
                    }
                }
            }

            return bitStream.ToByteArray();
        }

        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            AccessibleBitmap aBitmap = new AccessibleBitmap(width, height, pixelBytes);
            BitStreamFIFO bitStream = new BitStreamFIFO(source);

            for (int z = 0; z < pixelBytes * 8; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        aBitmap.SetPixelBit(x, y, z, bitStream.ReadBool());
                    }
                }
            }

            return aBitmap;
        }
    }
}
