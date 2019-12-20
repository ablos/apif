using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageComparison
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to image comparison.");
            Console.WriteLine();
            Console.WriteLine("Please drag in the first image:");
            Bitmap bm1 = new Bitmap(Console.ReadLine());
            Console.WriteLine();
            Console.WriteLine("First image selected! Please drag in the second image:");
            Bitmap bm2 = new Bitmap(Console.ReadLine());
            Console.WriteLine();
            Console.WriteLine("Second image selected, comparing...");

            bool pixelsmatch = ComparePixels(bm1, bm2);

            if (bm1.Width == bm2.Width && bm1.Height == bm2.Height && pixelsmatch)
            {
                Console.WriteLine("Images are exactly the same!");
            }else
            {
                Console.WriteLine("Images don't match.");
                Console.WriteLine("Resolution first image: " + bm1.Width + "x" + bm1.Height);
                Console.WriteLine("Resolution second image: " + bm2.Width + "x" + bm2.Height);
                Console.WriteLine("Pixels match: " + pixelsmatch);
            }

            Console.ReadKey();
        }

        static bool ComparePixels(Bitmap bm1, Bitmap bm2)
        {
            for (int x = 0; x < bm1.Width; x++)
            {
                for (int y = 0; y < bm1.Height; y++)
                {
                    if (bm1.GetPixel(x, y) != bm2.GetPixel(x, y))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
