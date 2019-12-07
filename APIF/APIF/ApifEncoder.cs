using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APIF
{
    class ApifEncoder
    {
        private static int version = 1;

        #region PublicVars
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

        private int compressionType = 0;
        public double GetCompressionType()
        {
            if (encodingStart.TotalMilliseconds < encodingStop.TotalMilliseconds)
            {
                return compressionType;
            }
            else
            {
                return -1;
            }
        }

        private Action<string> statusReceiver;
        private Control classUI;
        public void SetStatusHandler(Action<string> statusReceiverFunction, Control classOfUI)
        {
            classUI = classOfUI;
            statusReceiver = statusReceiverFunction;
        }
        #endregion



        //Class for easily reading from & writing pixels to a bitmap
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

        //Class for easily reading from & writing pixels to a bitmap
        public class AccessibleBitmapBytewise
        {
            private byte[] byteArray;
            public int height;
            public int width;
            public int pixelBytes;

            public AccessibleBitmapBytewise(AccessibleBitmap aBitmap)
            {
                width = aBitmap.width;
                height = aBitmap.height;
                pixelBytes = aBitmap.pixelBytes;
                byteArray = aBitmap.GetRawPixelBytes();
            }


            public void SetPixelByte(int x, int y, int layer, byte byteValue)
            {
                byteArray[(y * width + x) * pixelBytes + layer] = byteValue;
            }

            public byte GetPixelByte(int x, int y, int layer)
            {
                return byteArray[(y * width + x) * pixelBytes + layer];
            }


            public AccessibleBitmap GetAccessibleBitmap()
            {
                AccessibleBitmap aBitmap = new AccessibleBitmap(width, height, pixelBytes);
                aBitmap.SetRawPixelBytes(byteArray);
                return aBitmap;
            }

            public byte[] GetRawPixelBytes()
            {
                return byteArray;
            }
        }

        //Class for easily reading from & writing pixels to a bitmap
        public class AccessibleBitmapBitwise
        {
            private bool[] boolArray;
            public int height;
            public int width;
            public int pixelBytes;

            public AccessibleBitmapBitwise(AccessibleBitmapBytewise aBitmap)
            {
                width = aBitmap.width;
                height = aBitmap.height;
                pixelBytes = aBitmap.pixelBytes;

                byte[] tmpBytes = aBitmap.GetRawPixelBytes();
                boolArray = new bool[tmpBytes.Length * 8];
                new BitArray(tmpBytes).CopyTo(boolArray, 0);
            }


            public void SetPixelBit(int x, int y, int layer, bool bit)
            {
                boolArray[(y * width + x) * pixelBytes * 8 + layer] = bit;
            }

            public bool GetPixelBit(int x, int y, int layer)
            {
                    return boolArray[(y * width + x) * pixelBytes * 8 + layer];
            }


            public AccessibleBitmap GetAccessibleBitmap()
            {
                byte[] tmpBytes = new byte[width * height * pixelBytes];
                new BitArray(boolArray).CopyTo(tmpBytes, 0);

                AccessibleBitmap aBitmap = new AccessibleBitmap(width, height, pixelBytes);
                aBitmap.SetRawPixelBytes(tmpBytes);

                return aBitmap;
            }
        }

        //Class for easily storing and accessing data which exceeds the boundary of 8 bits which byte arrays come with
        public class BitStreamFIFO
        {
            List<bool> allData;
            public int readIndex { get; private set; }
            public int Length { get { return allData.Count - readIndex; } }

            public BitStreamFIFO()
            {
                allData = new List<bool>();
                readIndex = 0;
            }

            public BitStreamFIFO(byte[] byteArray)
            {
                bool[] boolArray = new bool[byteArray.Length * 8];
                new BitArray(byteArray).CopyTo(boolArray, 0);
                allData = new List<bool>(boolArray);
                readIndex = 0;
            }

            public BitStreamFIFO(bool[] boolArray)
            {
                allData = new List<bool>(boolArray);
                readIndex = 0;
            }


            public void Write(bool[] boolArray)
            {
                allData.AddRange(boolArray);
            }

            public void Write(bool inputBool)
            {
                allData.Add(inputBool);
            }

            public void Write(int number, int bitCount)
            {
                if (bitCount < 0) { Debugger.Break(); }
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
                if (allData.Count - readIndex < length) { throw new ArgumentOutOfRangeException("'length' is greater than BitStreamFIFO length"); }

                bool[] output = allData.GetRange(readIndex, length).ToArray();
                readIndex += length;
                return output;
            }

            public bool ReadBool()
            {
                if (allData.Count - readIndex < 1) { throw new Exception("Cannot read from empty BitStreamFIFO"); }

                bool output = allData[readIndex];
                readIndex++;
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
                BitArray tempBits = new BitArray(input);
                bool[] output = new bool[tempBits.Count];
                tempBits.CopyTo(output, 0);
                return output;
            }


            public int BoolArrayToInt(bool[] source)
            {
                BitArray tempBits = new BitArray(source);
                int[] tempArray = new int[1];
                tempBits.CopyTo(tempArray, 0);
                return tempArray[0];
            }

            public byte[] BoolArrayToByteArray(bool[] source)
            {
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
                foreach (BitStreamFIFO bitStream in bitStreams)
                {
                    newLength += bitStream.Length;
                }

                bool[] mergedBools = new bool[newLength];
                int currentIndex = 0;
                foreach (BitStreamFIFO bitStream in bitStreams)
                {
                    Array.Copy(bitStream.ToBoolArray(), bitStream.readIndex, mergedBools, currentIndex, bitStream.Length);
                    currentIndex += bitStream.Length;
                }

                return new BitStreamFIFO(mergedBools);
            }

            public static BitStreamFIFO Merge(BitStreamFIFO bitStream1, BitStreamFIFO bitStream2)
            {
                int newLength = bitStream1.Length + bitStream2.Length;

                bool[] mergedBools = new bool[newLength];
                Array.Copy(bitStream1.ToBoolArray(), bitStream1.readIndex, mergedBools, 0, bitStream1.Length);
                Array.Copy(bitStream2.ToBoolArray(), bitStream2.readIndex, mergedBools, bitStream1.Length, bitStream2.Length);

                return new BitStreamFIFO(mergedBools);
            }
        }




        //Encodes a C# Bitmap image to a byte array containing this image compressed as APIF image
        public byte[] Encode(Bitmap bitmap)
        {
            //Start timer for compression time
            encodingStart = DateTime.Now.TimeOfDay;

            //Creates a AccessibleBitmap class for the input bitmap: this class makes reading pixels easier and faster
            AccessibleBitmap aBitmap = new AccessibleBitmap(bitmap);

            //Compress image using all different compression techniques, where possible at the same time
            byte[][] compressionTechniques = new byte[4][];
            Parallel.For(0, compressionTechniques.Length, (i, state) => 
            {
                switch (i)
                {
                    //Uncompressed (only used if no compression technique is smaller)
                    case 0:
                        compressionTechniques[i] = UncompressedBitmapCompressor.Compress(aBitmap);
                        break;

                    //Split colors in their channels and apply compression over them
                    case 1:
                        compressionTechniques[i] = ByteLayerVaryingCompression.Compress(aBitmap);
                        break;

                    //Run length compression: save the length of a sequence of pixels with the same color instead of saving them seperately
                    case 2:
                        compressionTechniques[i] = RLEBitCompressor.CompressHorizontal(aBitmap);
                        break;

                    case 3:
                        compressionTechniques[i] = RLEBitCompressor.CompressVertical(aBitmap);
                        break;

                    //To add a compression technique, add a new case like the existing ones and increase the length of new byte[??][]
                }
            });


            //Choose the smallest compression type

            //Initialize
            int smallestID = 0;
            int smallestSize = int.MaxValue;
            //Loop trough all saved compression techniques
            for(int i = 0; i < compressionTechniques.Length; i++)
            {
                //If the current technique is smaller than the smallest technique which has been checked
                if(compressionTechniques[i].Length < smallestSize)
                {
                    //Mark this technique as smallest
                    smallestSize = compressionTechniques[i].Length;
                    smallestID = i;
                }
            }

            //Set the output byte array to the output of the smallest compression technique
            byte[] image = compressionTechniques[smallestID];
            compressionType = smallestID;

            //Build the file header containing information for the decoder
            byte[] header = new byte[7];
            header[0] = (byte)version;                  //This byte indicates the version of the compressor, to handle possible changes in the future
            header[1] = (byte)aBitmap.pixelBytes;       //This byte indicates the amount of color channels in the image
            header[2] = (byte)(aBitmap.width >> 8);     //These 2 bytes together indicate the width of the image
            header[3] = (byte)aBitmap.width;            //The reason for using 2 bytes instead of 1, is that 1 byte can store a width of 0-255, while 2 bytes can store 0-65535
            header[4] = (byte)(aBitmap.height >> 8);    //These 2 bytes together indicate the heigth of the image
            header[5] = (byte)aBitmap.height;           //The reason for using 2 bytes instead of 1, is that 1 byte can store a width of 0-255, while 2 bytes can store 0-65535
            header[6] = (byte)compressionType;          //This contains the number of the compression technique used

            //Merge the header with the image data to form the fimal file data
            byte[] fileBytes = new byte[header.Length + image.Length];
            Array.Copy(header, fileBytes, header.Length);
            Array.Copy(image, 0, fileBytes, header.Length, image.Length);

            //Stop timer
            encodingStop = DateTime.Now.TimeOfDay;
            compressionRate = (double)fileBytes.Length / (aBitmap.width * aBitmap.height);

            //Return final file as byte array
            return fileBytes;
        }



        //Decodes a byte array containing a compressed APIF image to a C# Bitmap image
        public Bitmap Decode(byte[] bytes)
        {
            //Check if the file version matches the version of the this decoder
            if (bytes[0] != version) { throw new Exception("Version not matching"); }

            //Start timer for decoding time
            encodingStart = DateTime.Now.TimeOfDay;

            //Read the header info to the correct variables
            int pixelBytes = bytes[1];                  //The amount of color channels
            int width = bytes[2] * 256 + bytes[3];      //The image width
            int height = bytes[4] * 256 + bytes[5];     //The image heigth
            compressionType = bytes[6];                 //The compression type

            //Store the image data apart from the header
            byte[] image = new byte[bytes.Length - 7];
            Array.Copy(bytes, 7, image, 0, image.Length);

            //Initialize
            AccessibleBitmap outputBitmap = null;   //The final AccessibleBitmap object

            //Choose the right decoding type from the header info
            switch (compressionType)
            {
                //Uncompressed bitmap
                case 0:
                    outputBitmap = UncompressedBitmapCompressor.Decompress(image, width, height, pixelBytes);
                    break;

                //Individual compressed color channels merged together
                case 1:
                    outputBitmap = ByteLayerVaryingCompression.Decompress(image, width, height, pixelBytes);
                    break;

                //Run length encoding
                case 2:
                    outputBitmap = RLEBitCompressor.Decompress(image, width, height, pixelBytes);
                    break;

                case 3:
                    outputBitmap = RLEBitCompressor.Decompress(image, width, height, pixelBytes);
                    break;

                //To add a decompression type add a new case like the existing ones

                //Unknown compression type: error
                default:
                    throw new Exception("Unexisting compression type");
            }

            //Stop timer 
            encodingStop = DateTime.Now.TimeOfDay;
            SetStatus("Finished");

            //Calculate compression rate in bytes per pixel
            compressionRate = (double)bytes.Length / (outputBitmap.width * outputBitmap.height);

            //Return the output image as bitmap format
            return outputBitmap.GetBitmap();
        }



        private void SetStatus(string status)
        {
            try
            {
                classUI.Invoke((MethodInvoker)delegate { statusReceiver(status); });
            }
            catch { }
        }
    }
}
