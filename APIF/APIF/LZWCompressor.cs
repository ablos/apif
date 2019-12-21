using System.Collections.Generic;
using static APIF.ApifEncoder;
using System.Linq;
using System;

namespace APIF
{
    static class LZWCompressor
    {
        // Maximum bitcount for dictionary values
        public const int maxBitCount = 12;

        // This function is used to compress the image using the LZW algorithm
        public static byte[] Compress(AccessibleBitmap source)
        {
            // Add first 255 standard values to LZWDictionary in LZWCompressor.cs
            string[] LZWDictionary = new string[256];
            for (int i = 0; i < 256; i++)
            {
                LZWDictionary[i] = ((char)i).ToString();
            }

            List<string> dictionary = new List<string>(LZWDictionary);                            // Clone dictionary of all bytes
            Queue<byte> bytes = new Queue<byte>(source.GetRawPixelBytes());     // Get all bytes from the source image
            BitStreamFIFO bs = new BitStreamFIFO();                             // Create bitstream for output
            int maxDictSize = (int)Math.Pow(2, maxBitCount);                    // Get maximum dictionary size
            string encodingString = ((char)bytes.Dequeue()).ToString();                 // Create string to add encoding to

            while (bytes.Count > 0)
            {
                // Clear dict if full
                if (dictionary.Count >= maxDictSize)
                {
                    dictionary = new List<string>(LZWDictionary);
                }

                char b = (char)bytes.Dequeue();

                if (dictionary.Contains(encodingString + b))
                {
                    encodingString += b;
                }else
                {
                    bs.Write(dictionary.FindIndex(x => x.StartsWith(encodingString)), maxBitCount);
                    dictionary.Add(encodingString + b);
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
