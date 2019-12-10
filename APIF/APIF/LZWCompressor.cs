using System.Collections.Generic;
using static APIF.ApifEncoder;
using System.Linq;
using System;

namespace APIF
{
    static class LZWCompressor
    {
        // The first 255 values, added in Program.cs
        public static List<string> LZWDictionary = new List<string>();
        // Maximum bitcount for dictionary values
        public const int maxBitCount = 12;

        // This function is used to compress the image using the LZW algorithm
        public static byte[] Compress(AccessibleBitmap source)
        {
            List<string> dictionary = LZWDictionary;                            // Clone dictionary of all bytes
            Queue<byte> bytes = new Queue<byte>(source.GetRawPixelBytes());     // Get all bytes from the source image
            BitStreamFIFO bs = new BitStreamFIFO();                             // Create bitstream for output
            int maxDictSize = (int)Math.Pow(2, maxBitCount);                    // Get maximum dictionary size
            string encodingString = bytes.Dequeue().ToString();                 // Create string to add encoding to

            while (bytes.Count > 0)
            {
                int b = bytes.Dequeue();

                if (dictionary.Contains(encodingString + "." + b.ToString()))
                {
                    encodingString += "." + b.ToString();
                }else
                {
                    bs.Write(dictionary.FindIndex(x => x.StartsWith(encodingString)), maxBitCount);
                    dictionary.Add(encodingString + "." + b.ToString());
                    encodingString = b.ToString();
                }
            }

            // Write remaining byte to bitstream
            bs.Write(dictionary.FindIndex(x => x.StartsWith(encodingString)), maxBitCount);

            // Return the bitstream as byte array
            return bs.ToByteArray();
        }

        // This function is used to decompress LZW compressed images
        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {


            return null;
        }
    }
}
