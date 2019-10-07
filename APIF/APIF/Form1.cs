using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APIF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool bmp2apif = true;

        byte[][] pixelArray = new byte[0][];

        private void UpdateProgressBar(int progress)
        {
            conversionProgressBar.Value = progress;
            conversionProgressLabel.Text = progress.ToString() + "%";
        }

        private void OpenFile(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "BMP Images|*.bmp|APIF Images|*.apif";
                openFileDialog.FilterIndex = 0;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (Path.GetExtension(openFileDialog.FileName).ToLower() == ".apif")
                    {
                        ManageDecoding(openFileDialog.FileName);
                    }
                    else
                    {
                        PrepareBitmap(openFileDialog.FileName);
                    }
                }
            }
        }

        private void SaveFile(object sender, EventArgs e)
        {

        }


        private void PrepareBitmap(string path)
        {
            Bitmap image = new Bitmap(path);
            imagepreview.Image = image;

            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);
            pixelArray = new byte[image.Height][];

            IntPtr ptr = bmpData.Scan0;
            for(int i = 0; i < image.Height; i++)
            {
                pixelArray[i] = new byte[Math.Abs(bmpData.Stride)];
                System.Runtime.InteropServices.Marshal.Copy(ptr, pixelArray[i], 0, Math.Abs(bmpData.Stride));
            }

            image.UnlockBits(bmpData);
        }

        private void ManageEncoding(string path)
        {

        }

        private void ManageDecoding(string path)
        {

        }
    }
}
