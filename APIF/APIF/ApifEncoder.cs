using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace APIF
{
    class ApifEncoder
    {
        private TimeSpan encodingStart = TimeSpan.FromMilliseconds(1);
        private TimeSpan encodingStop = TimeSpan.FromMilliseconds(0);
        public TimeSpan GetEncodingTime()
        {
            if (encodingStart.TotalMilliseconds < encodingStop.TotalMilliseconds)
            {
                return encodingStop.Subtract(encodingStart);
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        private double compressionRate = 0;
        public double GetCompressionRate()
        {
            if (encodingStart.TotalMilliseconds < encodingStop.TotalMilliseconds)
            {
                return compressionRate;
            }
            else
            {
                return -1;
            }
        }


        public class AccessibleBitmap
        {
            private byte[] byteArray;
            public int height;
            public int width;
            public int pixelBytes;
            private PixelFormat pixelFormat;

            public AccessibleBitmap(Bitmap bitmap)
            {
                switch (bitmap.PixelFormat)
                {
                    case PixelFormat.Format24bppRgb:
                        pixelFormat = PixelFormat.Format24bppRgb;
                        break;

                    case PixelFormat.Format32bppRgb:
                        pixelFormat = PixelFormat.Format24bppRgb;
                        break;

                    case PixelFormat.Format32bppArgb:
                        pixelFormat = PixelFormat.Format32bppArgb;
                        break;

                    case PixelFormat.Format32bppPArgb:
                        pixelFormat = PixelFormat.Format32bppArgb;
                        break;

                    case PixelFormat.Format48bppRgb:
                        pixelFormat = PixelFormat.Format48bppRgb;
                        break;

                    case PixelFormat.Format64bppArgb:
                        pixelFormat = PixelFormat.Format64bppArgb;
                        break;

                    case PixelFormat.Format64bppPArgb:
                        pixelFormat = PixelFormat.Format64bppArgb;
                        break;

                    default:
                        throw new FormatException("invalid pixelformat");
                }

                Rectangle size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                Bitmap bmpFixed = bitmap.Clone(size, pixelFormat);
                BitmapData bmpData = bmpFixed.LockBits(size, ImageLockMode.ReadWrite, pixelFormat);

                height = bmpData.Height;
                width = bmpData.Width;
                pixelBytes = Image.GetPixelFormatSize(pixelFormat) / 8;
                byteArray = new byte[height * width * pixelBytes];

                IntPtr ptr = bmpData.Scan0;
                Marshal.Copy(ptr, byteArray, 0, byteArray.Length);

                bmpFixed.UnlockBits(bmpData);
            }

            public AccessibleBitmap(int bmpWidth, int bmpHeight, int bmpPixelFormat)
            {
                    switch (bmpPixelFormat)
                    {
                        case 3:
                            pixelFormat = PixelFormat.Format24bppRgb;
                            break;

                        case 4:
                            pixelFormat = PixelFormat.Format32bppArgb;
                            break;

                        case 6:
                            pixelFormat = PixelFormat.Format48bppRgb;
                            break;

                        case 8:
                            pixelFormat = PixelFormat.Format64bppArgb;
                            break;

                        default:
                            throw new FormatException("invalid pixelformat");
                    }

                    byteArray = new byte[bmpHeight * bmpWidth * bmpPixelFormat];
                    height = bmpHeight;
                    width = bmpWidth;
                    pixelBytes = bmpPixelFormat;
            }

            public void SetPixel(int x, int y, byte[] pixelData)
            {
                if (x < width && y < height && pixelData.Length == pixelBytes)
                {
                    for (int i = 0; i < pixelBytes; i++)
                    {
                        byteArray[y * width * pixelBytes + x * pixelBytes + i] = pixelData[i];
                    }
                }
                else
                {
                    throw new FormatException("invalid input dimensions");
                }
            }

            public byte[] GetPixel(int x, int y)
            {
                if (x < width && y < height)
                {
                    byte[] output = new byte[pixelBytes];
                    for (int i = 0; i < pixelBytes; i++)
                    {
                        output[i] = byteArray[y * width * pixelBytes + x * pixelBytes + i];
                    }
                    return output;
                }
                else
                {
                    throw new FormatException("invalid input dimensions");
                }
            }

            public void SetPixelBit(int x, int y, int layer, bool bit)
            {
                if (x < width && y < height && layer < pixelBytes * 8)
                {
                    int bitIndex = (y * width + x) * pixelBytes * 8 + layer;
                    BitArray bArray = new BitArray(byteArray);
                    bArray[bitIndex] = bit;
                    bArray.CopyTo(byteArray, 0);
                }
                else
                {
                    throw new FormatException("invalid input dimensions");
                }
            }

            public bool GetPixelBit(int x, int y, int layer)
            {
                if (x < width && y < height)
                {
                    int bitIndex = (y * width + x) * pixelBytes * 8 + layer;
                    BitArray bArray = new BitArray(byteArray);
                    return bArray[bitIndex];
                }
                else
                {
                    throw new FormatException("invalid input dimensions");
                }
            }

            public Bitmap GetBitmap()
            {
                Bitmap bitmap = new Bitmap(width, height, pixelFormat);

                Rectangle size = new Rectangle(0, 0, width, height);
                BitmapData bmpData = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                IntPtr ptr = bmpData.Scan0;
                Marshal.Copy(byteArray, 0, ptr, byteArray.Length);
                bitmap.UnlockBits(bmpData);

                return bitmap;
            }
        }

        public class BitWrapper
        {
            bool read;

            public BitWrapper()
            {
                read = false;
            }

            //public BitWrapper()
            //{

            //}
        }


        public byte[] Encode(Bitmap bitmap)
        {
            encodingStart = DateTime.Now.TimeOfDay;

            AccessibleBitmap aBitmap = new AccessibleBitmap(bitmap);

            byte[] header = new byte[5];
            header[0] = (byte)aBitmap.pixelBytes;
            header[1] = (byte)(aBitmap.width >> 8);
            header[2] = (byte)aBitmap.width;
            header[3] = (byte)(aBitmap.height >> 8);
            header[4] = (byte)aBitmap.height;

            byte[] image = UncompressedBitmapCompressor.Compress(aBitmap);
            byte[] fileBytes = new byte[header.Length + image.Length];
            Array.Copy(header, fileBytes, header.Length);
            Array.Copy(image, 0, fileBytes, header.Length, image.Length);

            encodingStop = DateTime.Now.TimeOfDay;
            compressionRate = (double)fileBytes.Length / (aBitmap.width * aBitmap.height);
            return fileBytes;
        }

        public Bitmap Decode(byte[] bytes)
        {
            encodingStart = DateTime.Now.TimeOfDay;

            int pixelBytes = bytes[0];
            int width = bytes[3] * 256 + bytes[4];
            int height = bytes[1] * 256 + bytes[2];
            AccessibleBitmap emptyBitmap = new AccessibleBitmap(width, height, pixelBytes);

            byte[] image = new byte[bytes.Length - 5];
            Array.Copy(bytes, 5, image, 0, image.Length);

            AccessibleBitmap outputBitmap = UncompressedBitmapCompressor.Decompress(image, emptyBitmap);

            encodingStop = DateTime.Now.TimeOfDay;
            compressionRate = (double)bytes.Length / (outputBitmap.width * outputBitmap.height);
            return outputBitmap.GetBitmap();
        }
    }
}
