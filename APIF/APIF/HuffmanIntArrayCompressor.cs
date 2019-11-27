using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class HuffmanIntArrayCompressor
    {
        public static BitStreamFIFO Compress(int[] source)
        {
            List<int> numbers = new List<int>();
            List<int> weights = new List<int>();
            foreach (int b in source)
            {
                if (numbers.Contains(b))
                {
                    weights[numbers.IndexOf(b)]++;
                }
                else
                {
                    numbers.Add(b);
                    weights.Add(1);
                }
            }

            List<int> sortedWeightTmp = weights;
            sortedWeightTmp.Sort();
            int[] sortedWeightsInts = sortedWeightTmp.ToArray();
            sortedNumbers = new int[numbers.Count];
            int[] sortedWeights = new int[weights.Count];
            for (int i = 0; i < sortedWeightTmp.Count; i++)
            {
                int oldIndex = weights.IndexOf(sortedWeightsInts[i]);
                sortedNumbers[i] = numbers[oldIndex];
                sortedWeights[i] = weights[oldIndex];
                weights[oldIndex] = 0;
            }

            /*for (int i = 0; i < sortedNumbers.Length; i++)
            {
                Console.WriteLine(sortedNumbers[i] + " - " + sortedWeights[i]);
            }*/

            List<int[]> tree = new List<int[]>();
            List<int[]> sources = new List<int[]>();

            for (int i = 0; i < sortedWeights.Length; i++)
            {
                sources.Add(new int[] { sortedWeights[i], -1, i });
            }

            while (sources.Count > 1)
            {
                tree.AddRange(sources.GetRange(0, 2));
                sources.RemoveRange(0, 2);
                sources.Add(new int[] { tree[tree.Count - 1][0] + tree[tree.Count - 2][0], tree.Count - 1, tree.Count - 2 });
                sources.Sort((s, t) => s[0] - t[0]);
            }
            tree.Add(sources[0]);

            numberVals = new string[numbers.Count];

            treeInts = tree.ToArray();
            LoopTree("", treeInts.Length - 1);
            Console.WriteLine();

            int length = 0;
            for (int i = 0; i < numberVals.Length; i++)
            {
                Console.WriteLine(i + " - " + sortedNumbers[i] + " : " + sortedWeights[i] + " , " + numberVals[i]);
                length += sortedWeights[i] * numberVals[i].Length;
            }
            //Console.WriteLine(length / 8);
            return new BitStreamFIFO();
        }

        static int[] sortedNumbers;
        static string[] numberVals;
        static int[][] treeInts;
        static void LoopTree(string currentTree, int index)
        {
            //if(currentTree != "") { Console.Write(currentTree.Last()); }
            
            int[] node = treeInts[index];
            if (node[1] == -1)
            {
                //Console.Write("0 " + sortedNumbers[node[2]] + " ");
                numberVals[node[2]] = currentTree;
            }
            else
            {
                //Console.Write("1");
                LoopTree(currentTree + "0", node[1]);
                LoopTree(currentTree + "1", node[2]);
            }
        }


        public static int[] DeCompress(BitStreamFIFO source)
        {
            //dictionary decompile
            //1: add '0' to end
            //2: read byte; attach current code to byte; remove al '1' from end; replace last '0' with '1'
            return new int[0];
        }
    }
}
