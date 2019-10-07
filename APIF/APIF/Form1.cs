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

        Bitmap image = new Bitmap(0,0);

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
                        image = new Bitmap(openFileDialog.FileName);
                        imagepreview.Image = image;
                    }
                }
            }
        }

        private void SaveFile(object sender, EventArgs e)
        {

        }


        private void ManageEncoding(string path)
        {
            ApifEncoder encoder = new ApifEncoder();
            encoder.Encode(image);
        }

        private void ManageDecoding(string path)
        {
            ApifEncoder encoder = new ApifEncoder();
            encoder.Decode(File.ReadAllBytes(path));
        }
    }
}
