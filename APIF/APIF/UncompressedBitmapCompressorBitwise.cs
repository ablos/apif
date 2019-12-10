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
            //Creates a new BitStreamFIFO object where all bits will be written to
            BitStreamFIFO bitStream = new BitStreamFIFO();

            //Loop trough all lines of pixels
            for (int y = 0; y < source.height; y++)
            {
                //Loop trough all pixels in this line
                for (int x = 0; x < source.width; x++)
                {
                    //Write the bit of this channel from the current pixel to the output BitStream
                    bitStream.Write(source.GetPixelBit(x, y, bitLayer));
                }
            }

            //Return the BitStream
            return bitStream;
        }

        //Decompress byte array into aBitmap with help of width, length and bitdepth
        public static AccessibleBitmapBitwise Decompress(BitStreamFIFO inBits, AccessibleBitmapBitwise inBitmap, out BitStreamFIFO restBits, int bitLayer)
        {
            //Add the data from this stream to the incoming bitmap

            //Loop trough all lines of pixels
            for (int y = 0; y < inBitmap.height; y++)
            {
                //Loop trough all pixels in this line
                for (int x = 0; x < inBitmap.width; x++)
                {
                    //Write the bit of this channel from the input BitStream to the current pixel
                    inBitmap.SetPixelBit(x, y, bitLayer, inBits.ReadBool());
                }
            }

            //Set the output BitStream to the remainder of the input BitStream
            restBits = inBits;

            //Return the modified bitmap so the rest of the channels can be added to complete it
            return inBitmap;
        }
    }
}
