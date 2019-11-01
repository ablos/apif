using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class UncompressedBitmapCompressorBitwise
    {
        //Compress aBitmap into byte array
        public static BitStreamFIFO Compress(AccessibleBitmapBitwise source, int bitLayer)
        {
            BitStreamFIFO bitStream = new BitStreamFIFO();
            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    //Write bit of current pixel to stream
                    bitStream.Write(source.GetPixelBit(x, y, bitLayer));
                }
            }
            return bitStream;
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBitwise Decompress(BitStreamFIFO inBits, AccessibleBitmapBitwise inBitmap, out BitStreamFIFO restBits, int bitLayer)
        {
            for (int y = 0; y < inBitmap.height; y++)
            {
                for (int x = 0; x < inBitmap.width; x++)
                {
                    //Read bit from bitstream & set bit for the current pixel
                    inBitmap.SetPixelBit(x, y, bitLayer, inBits.ReadBool());
                }
            }
            restBits = inBits;
            return inBitmap;
        }
    }
}
