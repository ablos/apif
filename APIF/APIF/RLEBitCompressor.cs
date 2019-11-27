using System.Collections.Generic;
using static APIF.ApifEncoder;
using System.Linq;
using System;

namespace APIF
{
    /*
     * - Create an instance of this class to compress with Run Length Encoding (RLE).
     * - To compress, use the CompressHorizontal or CompressVertical function (sizes of these are to be compared later).
     * - To decompress, just use the Decompress function,this function will determine if the picture was vertically or horizontally compressed and
     *   will decompress it into a AccessibleBitmap.
     */

    static class RLEBitCompressor
    {
        // This function will compress the given bitmap into a byte array, the RLE will work horizontally
        public static byte[] CompressHorizontal(AccessibleBitmap source)
        {
            byte[] lastpixel = null;                                                            // Create variable to store the last pixel
            int colorCounter = 1;                                                               // Create counter for current color
            BitStreamFIFO bs = new BitStreamFIFO();                                             // Create new bitstream for all the bits
            int maxBitCount = (int)Math.Ceiling(Math.Log(source.width * source.height, 2));     // Get the maximum amount of bits needed to get the countervalue of all the pixels

            // Write one bit to the bitstream, so the decompressor knows to decompress horizontally
            bs.Write(false);

            // Iterate through every horizontal row
            for (int y = 0; y < source.height; y++)
            {
                // Iterate through every pixel in the horizontal row
                for (int x = 0; x < source.width; x++)
                {
                    // Check if the variable lastpixel is empty
                    if (lastpixel == null)
                    {
                        // If lastpixel is empty, set last pixel to the first pixel
                        lastpixel = source.GetPixel(x, y);
                    }
                    else
                    {
                        // If lastpixel isn't empty, compare last pixel with new pixel
                        if (lastpixel.SequenceEqual(source.GetPixel(x, y)))
                        {
                            // Pixels matched, so increase the counter value
                            colorCounter++;
                        }
                        else
                        {
                            // If the pixels don't match, add the counter with the last pixel to the bitstream
                            bs.Write(colorCounter, maxBitCount);
                            bs.Write(lastpixel);

                            // Reset the colorCounter and set the last pixel to the new pixel
                            colorCounter = 1;
                            lastpixel = source.GetPixel(x, y);
                        }
                    }
                }
            }

            // Add the remaining pixel(s) to the bitstream
            bs.Write(colorCounter, maxBitCount);
            bs.Write(lastpixel);

            // Return the bitsream as a byte[]
            return bs.ToByteArray();
        }

        public static byte[] CompressVertical(AccessibleBitmap source)
        {
            byte[] lastpixel = null;                                                            // Create variable to store the last pixel
            int colorCounter = 1;                                                               // Create counter for current color
            BitStreamFIFO bs = new BitStreamFIFO();                                             // Create new bitstream for all the bits
            int maxBitCount = (int)Math.Ceiling(Math.Log(source.width * source.height, 2));     // Get the maximum amount of bits needed to get the countervalue of all the pixels

            // Write one bit to the bitstream, so the decompressor knows to decompress vertically
            bs.Write(true);

            // Iterate through every vertical row
            for (int x = 0; x < source.width; x++)
            {
                // Iterate through every pixel in the vertical row
                for (int y = 0; y < source.height; y++)
                {
                    // Check if the variable lastpixel is empty
                    if (lastpixel == null)
                    {
                        // If lastpixel is empty, set last pixel to the first pixel
                        lastpixel = source.GetPixel(x, y);
                    }
                    else
                    {
                        // If lastpixel isn't empty, compare last pixel with new pixel
                        if (lastpixel.SequenceEqual(source.GetPixel(x, y)))
                        {
                            // Pixels matched, so increase the counter value
                            colorCounter++;
                        }
                        else
                        {
                            // If the pixels don't match, add the counter with the last pixel to the bitstream
                            bs.Write(colorCounter, maxBitCount);
                            bs.Write(lastpixel);

                            // Reset the colorCounter and set the last pixel to the new pixel
                            colorCounter = 1;
                            lastpixel = source.GetPixel(x, y);
                        }
                    }
                }
            }

            // Add the remaining pixel(s) to the bitstream
            bs.Write(colorCounter, maxBitCount);
            bs.Write(lastpixel);

            // Return the bitsream as a byte[]
            return bs.ToByteArray();
        }

        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            BitStreamFIFO bs = new BitStreamFIFO(source);                               // Convert the image into a bitstream
            int maxBitCount = (int)Math.Ceiling(Math.Log(width * height, 2));           // Get the maximum amount of bits needed to get the countervalue of all the pixels
            AccessibleBitmap bmp = new AccessibleBitmap(width, height, pixelBytes);     // Create new bitmap to write all pixels to
            bool verticallyCompressed = bs.ReadBool();                                  // Store if image was vertically compressed or not

            // Ints to keep track of coords
            int x = 0;
            int y = 0;

            // Loop while there are still bits to read
            while (bs.Length > maxBitCount)
            {
                int counterValue = bs.ReadInt(maxBitCount);     // Get the counter value of the next pixel value
                byte[] pixel = bs.ReadByteArray(pixelBytes);    // Get the pixel value

                for (int i = 0; i < counterValue; i++)
                {
                    bmp.SetPixel(x, y, pixel);
                    if (verticallyCompressed)
                    {
                        y++;
                        if (y >= height)
                        {
                            x++;
                            y = 0;
                        }
                    }else
                    {
                        x++;
                        if (x >= width)
                        {
                            y++;
                            x = 0;
                        }
                    }
                }
            }

            // Return the bitmap
            return bmp;
        }
    }
}
