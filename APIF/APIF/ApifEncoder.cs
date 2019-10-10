using System;
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
        class AccessibleBitmap
        {
            public byte[] byteArray;
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

            public AccessibleBitmap(int bmpHeight, int bmpWidth, int bmpPixelFormat)
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

        public byte[] Encode(Bitmap bitmap)
        {
            AccessibleBitmap aBitmap = new AccessibleBitmap(bitmap);

            byte[] byteArray = new byte[aBitmap.pixelBytes * aBitmap.height * aBitmap.width + 5];
            byteArray[0] = (byte)aBitmap.pixelBytes;
            byteArray[1] = (byte)(aBitmap.width >> 8);
            byteArray[2] = (byte)aBitmap.width;
            byteArray[3] = (byte)(aBitmap.height >> 8);
            byteArray[4] = (byte)aBitmap.height;

            Console.WriteLine(aBitmap.pixelBytes);
            for (int i = 0; i < aBitmap.height; i++)
            {
                for (int j = 0; j < aBitmap.width; j++)
                {
                    byte[] pixel = aBitmap.GetPixel(j, i);
                    for (int k = 0; k < pixel.Length; k++)
                    {
                        byteArray[5 + i * aBitmap.width * aBitmap.pixelBytes + j * aBitmap.pixelBytes + k] = pixel[k];
                    }
                }
            }

            return byteArray;
        }

        public Bitmap Decode(byte[] bytes)
        {
            AccessibleBitmap aBitmap = new AccessibleBitmap(bytes[3] * 256 + bytes[4], bytes[1] * 256 + bytes[2], bytes[0]);

            int x = 0;
            int y = 0;
            for (int i = 5; i < bytes.Length; i = i + aBitmap.pixelBytes)
            {
                byte[] pixel = new byte[aBitmap.pixelBytes];
                for (int j = 0; j < pixel.Length; j++)
                {
                    pixel[j] = bytes[i + j];
                }
                aBitmap.SetPixel(x, y, pixel);

                x++;
                if (x == aBitmap.width)
                {
                    x = 0;
                    y++;
                }
            }

            return aBitmap.GetBitmap();
        }
    }
}
