using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageAnalizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string filename = null;
        string openImageFilter = "Image files (*.bmp, *.png, *.jpg, *.jpeg, *.jpe, *.jfif, *.webp)| *.bmp; *.png; *.jpg; *.jpeg; *.jpe; *.jfif; *.webp";

        Bitmap[] bitLayers;
        int currentLayer;
        int maxLayer;

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = openImageFilter;
            openFileDialog.Title = "Open Image";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                AccessibleBitmap image = new AccessibleBitmap(new Bitmap(openFileDialog.FileName));

                currentLayer = 0;
                maxLayer = image.pixelBytes * 8 - 1;
                bitLayers = new Bitmap[image.pixelBytes * 8];
                for(int i = 0; i < bitLayers.Length; i++)
                {
                    AccessibleBitmap tmpBitmap = new AccessibleBitmap(image.width, image.height, 3);
                    for(int y = 0; y < tmpBitmap.height; y++)
                    {
                        for (int x = 0; x < tmpBitmap.width; x++)
                        {
                            if (image.GetPixelBit(x, y, i))
                            {
                                tmpBitmap.SetPixel(x, y, new byte[] { 255, 255, 255 });
                            }
                            else
                            {
                                tmpBitmap.SetPixel(x, y, new byte[] { 0, 0, 0 });
                            }
                        }
                    }
                    bitLayers[i] = tmpBitmap.GetBitmap();
                }
                pictureBox1.Image = bitLayers[0];
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            currentLayer++;
            if (currentLayer > maxLayer) { currentLayer = 0; }
            pictureBox1.Image = bitLayers[currentLayer];
            label1.Text = currentLayer.ToString("00");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            currentLayer--;
            if (currentLayer < 0) { currentLayer = maxLayer; }
            pictureBox1.Image = bitLayers[currentLayer];
            label1.Text = currentLayer.ToString("00");
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
            if (bitmap == null)
            {
                throw new ArgumentNullException("Input bitmap may not be null");
            }

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
                    throw new ArgumentException("Invalid pixelformat");
            }

            Rectangle size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            Bitmap bmpFixed = bitmap.Clone(size, pixelFormat);
            BitmapData bmpData = bmpFixed.LockBits(size, ImageLockMode.ReadWrite, pixelFormat);

            height = bmpData.Height;
            width = bmpData.Width;
            pixelBytes = Image.GetPixelFormatSize(pixelFormat) / 8;
            byteArray = new byte[height * bmpData.Stride];

            for (int i = 0; i < height; i++)
            {
                IntPtr ptr = IntPtr.Add(bmpData.Scan0, i * bmpData.Stride);
                Marshal.Copy(ptr, byteArray, i * width * pixelBytes, width * pixelBytes);
            }

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
                    throw new ArgumentException("Invalid pixelformat");
            }

            if (bmpWidth < 1 || bmpHeight < 1)
            {
                throw new ArgumentException("AccessibleBitmap dimensions must be 1 or greater");
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
                throw new ArgumentException("Invalid input dimensions");
            }
        }

        public byte[] GetPixel(int x, int y)
        {
            if (x < width && y < height)
            {
                byte[] output = new byte[pixelBytes];
                Array.Copy(byteArray, y * width * pixelBytes + x * pixelBytes, output, 0, pixelBytes);
                return output;
            }
            else
            {
                throw new ArgumentException("Invalid input dimensions");
            }
        }

        public void SetPixelBit(int x, int y, int layer, bool bit)
        {
            if (x < width && y < height && layer < pixelBytes * 8)
            {
                int bitIndex = (y * width + x) * pixelBytes * 8 + layer;
                int byteIndex = bitIndex / 8;
                byte mask = (byte)(1 << bitIndex % 8);
                byteArray[byteIndex] = (byte)(bit ? (byteArray[byteIndex] | mask) : (byteArray[byteIndex] & ~mask));
            }
            else
            {
                throw new ArgumentException("Invalid input dimensions");
            }
        }

        public bool GetPixelBit(int x, int y, int layer)
        {
            if (x < width && y < height)
            {
                int bitIndex = (y * width + x) * pixelBytes * 8 + layer;
                int byteIndex = bitIndex / 8;
                byte b = byteArray[byteIndex];
                return (b & (1 << bitIndex % 8)) != 0;
            }
            else
            {
                throw new ArgumentException("Invalid input dimensions");
            }
        }


        public Bitmap GetBitmap()
        {
            Bitmap bitmap = new Bitmap(width, height, pixelFormat);

            Rectangle size = new Rectangle(0, 0, width, height);
            BitmapData bmpData = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);

            for (int i = 0; i < height; i++)
            {
                IntPtr ptr = IntPtr.Add(bmpData.Scan0, i * bmpData.Stride);
                Marshal.Copy(byteArray, i * width * pixelBytes, ptr, width * pixelBytes);
            }

            bitmap.UnlockBits(bmpData);

            return bitmap;
        }


        public byte[] GetRawPixelBytes()
        {
            return byteArray;
        }

        public void SetRawPixelBytes(byte[] rawPixelBytes)
        {
            if (rawPixelBytes.Length == byteArray.Length)
            {
                byteArray = rawPixelBytes;
            }
        }
    }
}
