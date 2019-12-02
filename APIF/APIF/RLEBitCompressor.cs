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
            byte[] lastpixel = null;                                // Create variable to store the last pixel
            int colorCounter = 1;                                   // Create counter for current color
            BitStreamFIFO bs = new BitStreamFIFO();                 // Create new bitstream for all the bits
            int maxCount = 0;                                       // Create variable to store the max bitcount
            Queue<PixelArray> output = new Queue<PixelArray>();     // Create queue to store all the pixelvalues in

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
                            // If the pixels don't match, add the counter with the last pixel to the output queue
                            output.Enqueue(new PixelArray(colorCounter, lastpixel));
                            // Check if the new countervalue is higher then the last one, if so set maxBitCount to that
                            if (colorCounter > maxCount)
                                maxCount = colorCounter;

                            // Reset the colorCounter and set the last pixel to the new pixel
                            colorCounter = 1;
                            lastpixel = source.GetPixel(x, y);
                        }
                    }
                }
            }

            // Add the remaining pixel(s) to the output queue
            output.Enqueue(new PixelArray(colorCounter, lastpixel));
            // Check if the new countervalue is higher then the last one, if so set maxBitCount to that
            if (colorCounter > maxCount)
                maxCount = colorCounter;

            // Write the maxCount to the bitstream
            bs.Write((byte)Math.Ceiling(Math.Log(maxCount, 2)));

            // Add all the pixels from the queue to the bitstream
            while (output.Count > 0)
            {
                PixelArray pixel = output.Dequeue();
                bs.Write(pixel.Count, (int)Math.Ceiling(Math.Log(maxCount, 2)));
                bs.Write(pixel.Pixel);
            }

            // Return the bitsream as a byte[]
            return bs.ToByteArray();
        }

        public static byte[] CompressVertical(AccessibleBitmap source)
        {
            byte[] lastpixel = null;                                // Create variable to store the last pixel
            int colorCounter = 1;                                   // Create counter for current color
            BitStreamFIFO bs = new BitStreamFIFO();                 // Create new bitstream for all the bits
            int maxCount = 0;                                       // Create variable to store the max bitcount
            Queue<PixelArray> output = new Queue<PixelArray>();     // Create list to store all the pixelvalues in

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
                            // If the pixels don't match, add the counter with the last pixel to the output queue
                            output.Enqueue(new PixelArray(colorCounter, lastpixel));
                            // Check if the new countervalue is higher then the last one, if so set maxBitCount to that
                            if (colorCounter > maxCount)
                                maxCount = colorCounter;

                            // Reset the colorCounter and set the last pixel to the new pixel
                            colorCounter = 1;
                            lastpixel = source.GetPixel(x, y);
                        }
                    }
                }
            }

            // Add the remaining pixel(s) to the bitstream
            output.Enqueue(new PixelArray(colorCounter, lastpixel));
            // Check if the new countervalue is higher then the last one, if so set maxBitCount to that
            if (colorCounter > maxCount)
                maxCount = colorCounter;

            // Write the maxCount to the bitstream
            bs.Write((byte)Math.Ceiling(Math.Log(maxCount, 2)));

            // Add all the pixels from the queue to the bitstream
            while (output.Count > 0)
            {
                PixelArray pixel = output.Dequeue();
                bs.Write(pixel.Count, (int)Math.Ceiling(Math.Log(maxCount, 2)));
                bs.Write(pixel.Pixel);
            }

            // Return the bitsream as a byte[]
            return bs.ToByteArray();
        }

        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            BitStreamFIFO bs = new BitStreamFIFO(source);                               // Convert the image into a bitstream
            bool verticallyCompressed = bs.ReadBool();                                  // Store if image was vertically compressed or not
            int maxBitCount = bs.ReadByte();                                            // Get the highest bitcount value
            AccessibleBitmap bmp = new AccessibleBitmap(width, height, pixelBytes);     // Create new bitmap to write all pixels to

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

        // Class to store the pixels with countervalues
        private class PixelArray
        {
            public int Count;          // The countervalue
            public byte[] Pixel;       // The pixel

            // Constructor to set the countervalue and the pixel value
            public PixelArray(int Count, byte[] Pixel)
            {
                this.Count = Count;
                this.Pixel = Pixel;
            }
        }
    }
}
