﻿using System.Text;

namespace _2
{
    internal class Program
    {
        public static int GetOverlap(string a, string b)
        {
            int n = a.Length;
            for (int i = 1; i < n; i++)
                for (int j = i; j < n; j++)
                    if (a[j] != b[j - i])
                        break;
                    else if (j == n - 1)
                        return i;
            return -1;
        }
        
        public static string AssembleGnome(List<string> Reads)
        {
            int n = Reads.Count;
            int current = 0;
            StringBuilder Gnome = new StringBuilder();
            Gnome.Append(Reads[current]);
            int Iterations = n - 1;
            bool[] Visited = new bool[n];
            List<Tuple<string, int>> Path = new List<Tuple<string, int>>();
            while (Iterations > 0)
            {
                Visited[current] = true;
                string Read = Reads[current];
                var NextRead = -1;
                var MaximumOverlapSize = -1;
                for (int i = 0; i < Reads.Count; i++)
                {
                    if (Visited[i])
                        continue;
                    var OverlapStartingIndex = GetOverlap(Read, Reads[i]);
                    if (OverlapStartingIndex == -1)
                        continue;
                    var OverlapSize = Read.Length - OverlapStartingIndex;
                    if (MaximumOverlapSize < OverlapSize)
                    {
                        MaximumOverlapSize = OverlapSize;
                        NextRead = i;
                    }
                }
                if (NextRead == -1)
                    break;
                current = NextRead;
                Path.Add(new Tuple<string, int>(Reads[NextRead], MaximumOverlapSize));
            }
            var FinalOverlap = GetOverlap(Reads[current], Reads[0]);
            var CutSize = Reads[current].Length - FinalOverlap;
            Path.Add(new Tuple<string, int>(Reads[0], CutSize));

            return Gnome.ToString();
        }

        public static void CurrectReads(List<Tuple<string, int>> path, int i)
        {

        }
        static void Main(string[] args)
        {
            int n = 1618;
            HashSet<string> Reads = new HashSet<string>();
            for (int i = 0; i < n; i++)
                Reads.Add(Console.ReadLine());
            Console.WriteLine(AssembleGnome(Reads.ToList()));
        }
    }
}