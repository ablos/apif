using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIF
{
    class ApifEncoder
    {
        public byte[] Encode(Bitmap bitmap)
        {
            /*
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);
            pixelArray = new byte[image.Height][];

            IntPtr ptr = bmpData.Scan0;
            for (int i = 0; i < image.Height; i++)
            {
                pixelArray[i] = new byte[Math.Abs(bmpData.Stride)];
                System.Runtime.InteropServices.Marshal.Copy(ptr, pixelArray[i], 0, Math.Abs(bmpData.Stride));
            }

            image.UnlockBits(bmpData);
            */
            return new byte[0];
        }

        public Bitmap Decode(byte[] bytes)
        {
            return new Bitmap(0,0);
        }
    }
}
