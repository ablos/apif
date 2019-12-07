using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APIF.ApifEncoder;

namespace APIF
{
    class TreeWalker
    {
        int minBits;
        public BitStreamFIFO outputStream;
        int[] sortednumbers;
        int[][] treeints;
        public bool[][] numberVals;

        public TreeWalker(int[] sortedNumbers, int[][] treeInts)
        {
            sortednumbers = sortedNumbers;
            treeints = treeInts;

            numberVals = new bool[sortedNumbers.Length][];
            minBits = (int)Math.Ceiling(Math.Log(sortedNumbers.Max() + 1, 2));
            minBits = minBits < 1 ? 1 : minBits;

            outputStream = new BitStreamFIFO();
            outputStream.Write((byte)minBits);
            outputStream.Write(sortedNumbers.Length - 1, minBits);
            //Console.WriteLine(minBits + " " + sortedNumbers.Length);

            LoopTree(new bool[0], treeInts.Length - 1);
            //Console.WriteLine();
        }

        void LoopTree(bool[] currentTree, int index)
        {
            int[] node = treeints[index];
            if (node[1] == -1)
            {
                if(currentTree.Length == 0)
                {
                    currentTree = new bool[] { false };
                }

                //Console.Write("0 " + sortednumbers[node[2]] + " ");
                outputStream.Write(false);
                outputStream.Write(sortednumbers[node[2]], minBits);

                numberVals[node[2]] = currentTree.ToArray();
            }
            else
            {
                //Console.Write("1");
                outputStream.Write(true);

                bool[] passTree = new bool[currentTree.Length + 1];
                if (currentTree.Length > 0)
                {
                    Array.Copy(currentTree, passTree, currentTree.Length);
                }

                passTree[passTree.Length - 1] = true;
                LoopTree(passTree, node[2]);
                passTree[passTree.Length - 1] = false;
                LoopTree(passTree, node[1]);
            }
        }
    }

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
            int[] sortedNumbers = new int[numbers.Count];
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
                Console.WriteLine(sortedNumbers[i] + "\t" + sortedWeights[i]);
            }
            Console.WriteLine();*/

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

            Array.Reverse(sortedNumbers);
            TreeWalker walker = new TreeWalker(sortedNumbers, tree.ToArray());
            BitStreamFIFO outputStream = walker.outputStream;
            //Console.WriteLine("s" + outputStream.Length);
            bool[][] numberVals = walker.numberVals;

            /*int length = 0;
            for (int i = 0; i < numberVals.Length; i++)
            {
                Console.WriteLine(i + "\t" + sortedNumbers[i] + "\t" + sortedWeights[i] + "\t" + BoolArrString(numberVals[i]));
                length += sortedWeights[i] * numberVals[i].Length;
            }*/
            //Console.WriteLine(source.Length);
            //Console.WriteLine(length);

            foreach(int i in source)
            {
                int index = Array.IndexOf(sortedNumbers, i);
                outputStream.Write(numberVals[index]);
                //Console.WriteLine(BoolArrString(numberVals[index]) + "\t" + i);
                //Console.WriteLine(i);
            }
            //Console.WriteLine(source.Length * 8);
            //Console.WriteLine("length"+outputStream.Length);
            return outputStream;
        }

        static string BoolArrString(bool[] bools)
        {
            string s = "";
            foreach(bool b in bools)
            {
                s += b ? "1" : "0";
            }
            return s;
        }


        public static int[] DeCompress(BitStreamFIFO source)
        {
            //dictionary decompile
            //1: add '0' to end
            //2: read byte; attach current code to byte; remove al '1' from end; replace last '0' with '1'
            int minBits = source.ReadByte();
            int dictionaryLength = source.ReadInt(minBits) + 1;
            //Console.WriteLine(minBits + " " + dictionaryLength);
            SortedDictionary<string, int> dictionary = new SortedDictionary<string, int>();
            List<int> dicNumbers = new List<int>();
            List<bool[]> dicCodes = new List<bool[]>();
            string tmpCode = "";
            while (dictionary.Count < dictionaryLength)
            {
                if (source.ReadBool())
                {
                    tmpCode += "1";
                }
                else
                {
                    dictionary.Add(tmpCode, source.ReadInt(minBits));

                    if (tmpCode.Contains('1'))
                    {
                        while (tmpCode.Last() == '0')
                        {
                            tmpCode = tmpCode.Remove(tmpCode.Length - 1, 1);
                        }
                        tmpCode = tmpCode.Remove(tmpCode.Length - 1, 1);
                        tmpCode += '0';
                    }
                }
            }

            /*for(int i = 0; i < dicNumbers.Count; i++)
            {
                Console.WriteLine(dicNumbers[i] + " " + BoolArrString(dicCodes[i]));
            }
            Console.WriteLine();*/

            List<int> outputList = new List<int>();
            string tmpRead = "";
            while(source.Length > 0)
            {
                tmpRead += source.ReadBool()?'1':'0';
                int foundVal = 0;
                if (dictionary.TryGetValue(tmpRead, out foundVal))
                {
                    outputList.Add(foundVal);
                    tmpRead = "";
                }
            }

            return outputList.ToArray();
        }
    }
}
