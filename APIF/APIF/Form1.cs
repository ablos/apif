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

        bool decode = true;

        Bitmap image = null;
        string filename = null;
        string openImageFilter = "Image files (*.apif, *.bmp, *.png, *.jpg, *.jpeg, *.jpe, *.jfif, *.webp)| *.apif; *.bmp; *.png; *.jpg; *.jpeg; *.jpe; *.jfif; *.webp";
        string saveImageFilter = "BMP Image (*.bmp)|*.bmp|PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|JFIF Image (*.jfif)|*.jfif|WEBP Image (*.webp)|*.webp";

        private void UpdateProgressBar(int progress)
        {
            conversionProgressBar.Value = progress;
            conversionProgressLabel.Text = progress.ToString() + "%";
        }

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
                decode = Path.GetExtension(openFileDialog.FileName).ToLower() == ".apif";
                if (decode)
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

        private void SaveFile(object sender, EventArgs e)
        {
            if (image != null)
            {
                if (decode)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = saveImageFilter;
                    saveFileDialog.Title = "Save Image";
                    saveFileDialog.FileName = filename;
                    saveFileDialog.RestoreDirectory = true;
                    DialogResult result = saveFileDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        image.Save(saveFileDialog.FileName);
                    }
                }
                else
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "APIF Image|*.apif";
                    saveFileDialog.Title = "Save APIF Image";
                    saveFileDialog.FileName = filename;
                    saveFileDialog.RestoreDirectory = true;
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        ManageEncoding(saveFileDialog.FileName);
                    }
                }
            }
        }


        private void ManageEncoding(string path)
        {
            ApifEncoder encoder = new ApifEncoder();
            byte[] file = encoder.Encode(image);
            File.WriteAllBytes(path, file);

            conversionProgressLabel.Text = encoder.GetEncodingTime().TotalMilliseconds.ToString("F1") + "ms";
            compressionLabel.Text = "compression = " + encoder.GetCompressionRate().ToString("F3");
        }

        private void ManageDecoding(string path)
        {
            ApifEncoder encoder = new ApifEncoder();
            image = encoder.Decode(File.ReadAllBytes(path));
            imagepreview.Image = image;

            conversionProgressLabel.Text = encoder.GetEncodingTime().TotalMilliseconds.ToString("F1") + "ms";
            compressionLabel.Text = "compression = " + encoder.GetCompressionRate().ToString("F3");
        }
    }
}
