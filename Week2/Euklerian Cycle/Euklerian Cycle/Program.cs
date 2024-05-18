using System;
using System.Linq;
using System.Collections.Generic;
namespace Euklerian_Cycle
{
    internal class Program
    {
        public static bool IsBalanced(List<int>[] Graph)
        {
            int n = Graph.Length;
            int[] InDegree = new int[n];
            for(int i = 0; i < n; i++)
                for(int j = 0; j < Graph[i].Count; j++)
                    InDegree[Graph[i][j]]++;
            for (int i = 0; i < n; i++)
                if (InDegree[i] != Graph[i].Count)
                    return false;
            return true;
        }
        public static List<Tuple<int, int>> GetEulerianCycle(List<int>[] Graph)
        {
            if (!IsBalanced(Graph))
                return null;
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
        static void Main(string[] args)
        {
            var buffer = Console.ReadLine().Split();
            int n = int.Parse(buffer[0]);
            int m = int.Parse(buffer[1]);
            List<int>[] Graph = new List<int>[n];
            for (int i = 0; i < n; i++)
                Graph[i] = new List<int>();
            for(int i = 0; i < m; i++)
            {
                buffer = Console.ReadLine().Split();
                int v = int.Parse(buffer[0]) - 1;   
                int u = int.Parse(buffer[1]) - 1;
                Graph[v].Add(u);
            }
            var Path = GetEulerianCycle(Graph);
            if (Path == null)
            {
                Console.WriteLine(0);
                return;
            }
            Console.WriteLine(1);
            for(int i = 0; i < Path.Count; i++)
            {
                Console.Write($"{Path[i].Item2 + 1} ");
            }
        }
    }
}