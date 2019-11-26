using System.Collections.Generic;
using static APIF.ApifEncoder;
using System.Linq;
// Delete these:
using System;

namespace APIF
{
    /*
     * !!! WARNING: THIS CLASS IS OBSOLETE AND OLD, PLEASE USE RLEBitCompressor !!!
     * - Create an instance of this class to compress with Run Length Encoding (RLE).
     * - To compress, use the CompressHorizontal or CompressVertical function (sizes of these are to be compared later).
     * - To decompress, just use the Decompress function, this function will determine if the picture was vertically or horizontally compressed and
     *   will decompress it into a AccessibleBitmap.
     */

    static class RunLengthEncodingCompressor
    {
        // Create a new list to add bytes and will be returned after
        private static List<byte> bytes;

        // This function will compress the bitmap horizontally and return a new AccessibleBitmap
        public static byte[] CompressHorizontal(AccessibleBitmap source)
        {
            // Clear byte list
            bytes = new List<byte>();

            // Add to byte[] that compression is horizontal
            bytes.Add(0);

            byte[] lastpixel = null;            // Create variable to store the last pixel
            int colorCounter = 1;               // Create counter for current color

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
                    }else
                    {
                        // If lastpixel isn't empty, compare last pixel with new pixel
                        if (lastpixel.SequenceEqual(source.GetPixel(x, y)))
                        {
                            // If pixels match, check if the counter value didn't exceed the maximum value of 256.
                            if (colorCounter < 256)
                            {
                                // If color counter value didn't exceed the maximum value of 256, add one to the counter
                                colorCounter += 1;
                            }else
                            {
                                // If color counter value did exceed the maximum value of 256, add the bytes to the list and reset the counter.
                                AddBytes(colorCounter, lastpixel);
                                colorCounter = 1;
                            }
                        }else
                        {
                            // If the pixels don't match, add the lastpixel with the counter to the list of bytes, reset the counter and set the lastpixel variable to the new pixel
                            AddBytes(colorCounter, lastpixel);
                            colorCounter = 1;
                            lastpixel = source.GetPixel(x, y);
                        }
                    }
                }
            }

            // Add the remaining byte(s)
            AddBytes(colorCounter, lastpixel);

            // Return all compressed bytes
            return bytes.ToArray();
        }

        // This function will compress the bitmap vertically and return a new AccessibleBitmap
        public static byte[] CompressVertical(AccessibleBitmap source)
        {
            // Clear bytes list
            bytes = new List<byte>();
            
            byte[] lastpixel = null;            // Create variable to store the last pixel
            int colorCounter = 1;               // Create counter for current color

            // Add to byte[] that compression is vertical
            bytes.Add(1);

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
                            // If pixels match, check if the counter value didn't exceed the maximum value of 256.
                            if (colorCounter < 256)
                            {
                                // If color counter value didn't exceed the maximum value of 256, add one to the counter
                                colorCounter += 1;
                            }
                            else
                            {
                                // If color counter value did exceed the maximum value of 256, add the bytes to the list and reset the counter.
                                AddBytes(colorCounter, lastpixel);
                                colorCounter = 1;
                            }
                        }
                        else
                        {
                            // If the pixels don't match, add the lastpixel with the counter to the list of bytes, reset the counter and set the lastpixel variable to the new pixel
                            AddBytes(colorCounter, lastpixel);
                            colorCounter = 1;
                            lastpixel = source.GetPixel(x, y);
                        }
                    }
                }
            }

            // Add the remaining byte(s)
            AddBytes(colorCounter, lastpixel);

            // Return all compressed bytes
            return bytes.ToArray();
        }

        // This function will decompress the APIF which is compressed using this RLE Compressor and return a AccessibleBitmap
        public static AccessibleBitmap Decompress(byte[] source, int width, int height, int pixelBytes)
        {
            Console.WriteLine("Source:");
            foreach (byte b in source)
            {
                Console.Write(b.ToString() + " ");
            }
            Console.WriteLine();
            Console.WriteLine("Width: " + width);
            Console.WriteLine("Height: " + height);
            Console.WriteLine("Pixelbytes: " + pixelBytes);

            // Create new bitmap to add pixels to
            AccessibleBitmap bmp = new AccessibleBitmap(width, height, pixelBytes);

            // Create a queue to store all pixels from the APIF
            Queue<byte[]> queuedPixels = new Queue<byte[]>();

            // Loop through all pixels from the APIF
            for (int i = 1; i < ((source.Length - 1)); i += (1 + pixelBytes))
            {  
                // Get countervalue of pixel
                int counterValue = source[i];
                Console.WriteLine(counterValue);
                // Create byte[] to store pixel
                byte[] pixel = new byte[pixelBytes];

                // Get pixel value and store it
                for (int y = 0; y < pixelBytes; y++)
                {
                    pixel[y] = source[i + y + 1];
                }

                // Add pixel counterValue amount of times to queue
                for (int x = 0; x < counterValue; x++)
                {
                    queuedPixels.Enqueue(pixel);
                }
            }

            // Check if APIF is vertically or horizontally compressed
            if (source[0] == 0)
            {
                // APIF is horizontally compressed, so loop through every horizontal row
                for (int y = 0; y < height; y++)
                {
                    // Loop through every pixel on the horizontal row
                    for (int x = 0; x < width; x++)
                    {
                        // Set pixel value on these coördinates
                        bmp.SetPixel(x, y, queuedPixels.Dequeue());
                    }
                }
            }else
            {
                // APIF is vertically compressed, so loop through every vertical row
                for (int x = 0; x < width; x++)
                {
                    // Loop through every pixel on the vertical row
                    for (int y = 0; y < height; y++)
                    {
                        // Set pixel value on these coördinates
                        bmp.SetPixel(x, y, queuedPixels.Dequeue());
                    }
                }
            }

            // Return the completed AccessibleBitmap
            return bmp;
        }

        // This function will add the bytes to the byte list.
        private static void AddBytes(int colorCounter, byte[] pixel)
        {
            //Console.WriteLine("Pixel length: " + pixel.Length);
            bytes.Add((byte)(colorCounter - 1));
            
            // Add the pixel value
            bytes.AddRange(pixel);
        }
    }
}
