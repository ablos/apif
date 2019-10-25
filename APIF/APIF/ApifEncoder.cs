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
        private static int version = 1;

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

                if(bmpWidth < 1 || bmpHeight < 1)
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

        public class BitStreamFIFO
        {
            LinkedList<bool> allData;
            public int Length { get { return allData.Count; } }

            public BitStreamFIFO()
            {
                allData = new LinkedList<bool>();
            }

            public BitStreamFIFO(byte[] byteArray)
            {
                BlockNull(byteArray);

                bool[] boolArray = new bool[byteArray.Length * 8];
                new BitArray(byteArray).CopyTo(boolArray, 0);
                allData = new LinkedList<bool>(boolArray);
            }

            public BitStreamFIFO(bool[] boolArray)
            {
                BlockNull(boolArray);
                allData = new LinkedList<bool>(boolArray);
            }


            public void Write(bool[] boolArray)
            {
                BlockNull(boolArray);
                foreach (bool b in boolArray)
                {
                    allData.AddLast(b);
                }
            }

            public void Write(bool inputBool)
            {
                BlockNull(inputBool);
                allData.AddLast(inputBool);
            }

            public void Write(int number, int bitCount)
            {
                Write(IntToBoolArray(number, bitCount));
            }

            public void Write(byte input)
            {
                Write(ByteToBoolArray(input));
            }

            public void Write(byte[] input)
            {
                Write(ByteArrayToBoolArray(input));
            }


            public bool[] ReadBoolArray(int length)
            {
                BlockNull(length);
                if (allData.Count < length) { throw new ArgumentOutOfRangeException("'length' is greater than BitStreamFIFO length"); }

                bool[] output = new bool[length];
                for (int i = 0; i < length; i++)
                {
                    output[i] = allData.First.Value;
                    allData.RemoveFirst();
                }
                return output;
            }

            public bool ReadBool()
            {
                if (allData.Count < 1) { throw new Exception("Cannot read from empty BitStreamFIFO"); }

                bool output = allData.First.Value;
                allData.RemoveFirst();
                return output;
            }

            public int ReadInt(int length)
            {
                return BoolArrayToInt(ReadBoolArray(length));
            }

            public byte ReadByte()
            {
                return BoolArrayToByte(ReadBoolArray(8));
            }

            public byte[] ReadByteArray(int length, bool lengthInBits = false)
            {
                return BoolArrayToByteArray(ReadBoolArray(lengthInBits?length:(length*8)));
            }



            public bool[] IntToBoolArray(int number, int bitCount)
            {
                BlockNull(number);
                BlockNull(bitCount);

                BitArray tempBits = new BitArray(new int[] { number });
                tempBits.Length = bitCount;
                bool[] output = new bool[bitCount];
                tempBits.CopyTo(output, 0);
                return output;
            }

            public bool[] ByteToBoolArray(byte input)
            {
                return IntToBoolArray(input, 8);
            }

            public bool[] ByteArrayToBoolArray(byte[] input)
            {
                BlockNull(input);

                BitArray tempBits = new BitArray(input);
                bool[] output = new bool[tempBits.Count];
                tempBits.CopyTo(output, 0);
                return output;
            }


            public int BoolArrayToInt(bool[] source)
            {
                BlockNull(source);

                BitArray tempBits = new BitArray(source);
                int[] tempArray = new int[1];
                tempBits.CopyTo(tempArray, 0);
                return tempArray[0];
            }

            public byte[] BoolArrayToByteArray(bool[] source)
            {
                BlockNull(source);

                BitArray tempBits = new BitArray(source);
                byte[] output = new byte[(source.Length + 7) / 8];
                tempBits.CopyTo(output, 0);
                return output;
            }

            public byte BoolArrayToByte(bool[] source)
            {
                return (byte)BoolArrayToInt(source);
            }


            public byte[] ToByteArray()
            {
                return BoolArrayToByteArray(allData.ToArray());
            }

            public bool[] ToBoolArray()
            {
                return allData.ToArray();
            }

            public static BitStreamFIFO Merge(BitStreamFIFO[] bitStreams)
            {
                int newLength = 0;
                foreach(BitStreamFIFO bitStream in bitStreams)
                {
                    newLength += bitStream.Length;
                }

                bool[] mergedBools = new bool[newLength];
                int currentIndex = 0;
                foreach (BitStreamFIFO bitStream in bitStreams)
                {
                    Array.Copy(bitStream.ToBoolArray(), 0, mergedBools, currentIndex, bitStream.Length);
                    currentIndex += bitStream.Length;
                }

                return new BitStreamFIFO(mergedBools);
            }



            private void BlockNull(object var)
            {
                if(var == null)
                {
                    throw new ArgumentNullException("Input var may not be null");
                }
            }
        }


        public byte[] Encode(Bitmap bitmap)
        {
            encodingStart = DateTime.Now.TimeOfDay;

            AccessibleBitmap aBitmap = new AccessibleBitmap(bitmap);

            int compressionType = 0;
            byte[] image = UncompressedBitmapCompressor.Compress(aBitmap);

            byte[] tempImage = BitLayerVaryingCompresor.Compress(aBitmap);
            if (tempImage.Length < image.Length)
            {
                image = tempImage;
                compressionType = 1;
            }


            byte[] header = new byte[7];
            header[0] = (byte)version;
            header[1] = (byte)aBitmap.pixelBytes;
            header[2] = (byte)(aBitmap.width >> 8);
            header[3] = (byte)aBitmap.width;
            header[4] = (byte)(aBitmap.height >> 8);
            header[5] = (byte)aBitmap.height;
            header[6] = (byte)compressionType;

            byte[] fileBytes = new byte[header.Length + image.Length];
            Array.Copy(header, fileBytes, header.Length);
            Array.Copy(image, 0, fileBytes, header.Length, image.Length);

            encodingStop = DateTime.Now.TimeOfDay;
            compressionRate = (double)fileBytes.Length / (aBitmap.width * aBitmap.height);
            return fileBytes;
        }

        public Bitmap Decode(byte[] bytes)
        {
            if (bytes[0] != version) { throw new Exception("Version not matching"); }

            encodingStart = DateTime.Now.TimeOfDay;

            int pixelBytes = bytes[1];
            int width = bytes[2] * 256 + bytes[3];
            int height = bytes[4] * 256 + bytes[5];
            int compressionType = bytes[6];

            byte[] image = new byte[bytes.Length - 7];
            Array.Copy(bytes, 7, image, 0, image.Length);

            AccessibleBitmap outputBitmap = null;
            switch (compressionType)
            {
                case 0:
                    outputBitmap = UncompressedBitmapCompressor.Decompress(image, width, height, pixelBytes);
                    break;

                case 1:
                    outputBitmap = BitLayerVaryingCompresor.Decompress(image, width, height, pixelBytes);
                    break;

                default:
                    throw new Exception("Unexisting compression type");
            }

            encodingStop = DateTime.Now.TimeOfDay;
            compressionRate = (double)bytes.Length / (outputBitmap.width * outputBitmap.height);
            return outputBitmap.GetBitmap();
        }
    }
}
