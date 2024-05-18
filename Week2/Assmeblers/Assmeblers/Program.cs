using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;
namespace Assmeblers
{
    class Assembler
    {
        List<string> Composition;
        Dictionary<string, int> ReadToGraph;
        Dictionary<int, string> GraphToRead;
        List<List<int>> Debruijn;
        public Assembler(List<string> Composition)
        {
            this.Composition = Composition;
        }
        public void MakeDebruijn()
        {
            Debruijn = new List<List<int>>();
            ReadToGraph = new Dictionary<string, int>();
            GraphToRead = new Dictionary<int, string>();
            for (int i = 0; i < Composition.Count; i++)
            {
                var from = Composition[i].Substring(0, Composition[i].Length - 1);
                var to = Composition[i].Substring(1);
                if (!ReadToGraph.ContainsKey(from))
                {
                    var Newindex = ReadToGraph.Count;
                    Debruijn.Add(new List<int>());
                    ReadToGraph.Add(from, Newindex);
                    GraphToRead.Add(Newindex, from);
                }
                if (!ReadToGraph.ContainsKey(to))
                {
                    var Newindex = ReadToGraph.Count;
                    Debruijn.Add(new List<int>());
                    ReadToGraph.Add(to, Newindex);
                    GraphToRead.Add(Newindex, to);
                }
                Debruijn[ReadToGraph[from]].Add(ReadToGraph[to]);
            }
        }
        public static bool IsBalanced(List<int>[] Graph)
        {
            int n = Graph.Length;
            int[] InDegree = new int[n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < Graph[i].Count; j++)
                    InDegree[Graph[i][j]]++;
            for (int i = 0; i < n; i++)
                if (InDegree[i] != Graph[i].Count)
                    return false;
            return true;
        }
        public static List<Tuple<int, int>> GetEulerianCycle(List<int>[] Graph)
        {
            //if (!IsBalanced(Graph))
            //    return null;
            int n = Graph.Length;
            List<bool>[] Visited = new List<bool>[n];
            int m = 0;
            for (int i = 0; i < n; i++)
            {
                Visited[i] = new bool[Graph[i].Count].ToList();
                m += Graph[i].Count;
            }
            LinkedList<Tuple<int, int>> deque = new LinkedList<Tuple<int, int>>();
            deque.AddLast(new Tuple<int, int>(0, Graph[0][0]));
            Visited[0][0] = true;
            while (deque.Count != m)
            {
                var v = deque.Last.Value.Item2;
                Tuple<int, int> Next = null;
                for (int i = 0; i < Graph[v].Count; i++)
                    if (Visited[v][i] == false)
                    {
                        Next = new Tuple<int, int>(v, Graph[v][i]);
                        Visited[v][i] = true;
                        break;
                    }
                if (Next == null)
                {
                    deque.AddFirst(deque.Last.Value);
                    deque.RemoveLast();
                    continue;
                }
                deque.AddLast(Next);
            }
            return deque.ToList();
        }
        public string GetStringFromPath(List<Tuple<int, int>> Path)
        {
            StringBuilder result = new StringBuilder();
            result.Append(GraphToRead[Path[0].Item1]);
            for(int i = 0; i < Path.Count; i++)
            {
                string Read = GraphToRead[Path[i].Item2];
                result.Append(Read[Read.Length - 1]);
            }
            var offset = Composition[0].Length - 1;
            result.Remove(result.Length - offset, offset);
            return result.ToString();
        }
        public string Assmeble()
        {
            MakeDebruijn();
            var Path = GetEulerianCycle(Debruijn.ToArray());
            return GetStringFromPath(Path);
        }
    }
    internal class Program
    {
        public static List<string> GetBinaryComposition(int x)
        {
            if(x == 0)
            {
                return new List<string>() { "0", "1" };
            }
            var PrevComposition = GetBinaryComposition(x - 1);
            List<string> Composition = new List<string>();
            foreach(var binary in PrevComposition)
            {
                Composition.Add(binary + "1");
                Composition.Add(binary + "0");
            }
            return Composition;
        }
        static void Main(string[] args)
        {
            int n = int.Parse(Console.ReadLine());
            var Composition = GetBinaryComposition(n - 1);
            //List<string> Composition = new List<string>();
            //for (int i = 0; i < n; i++)
            //    Composition.Add(Console.ReadLine());
            Assembler assembler = new Assembler(Composition);
            Console.WriteLine(assembler.Assmeble());
        }
    }
}