using System;
using System.Linq;
using System.Collections.Generic;
using System.Security;
using System.Collections;
using System.Resources;
using System.Diagnostics;
using System.Security.Policy;
using System.Data.SqlTypes;

namespace DFA
{
    class DisjointSet<TKey>
    {
        Dictionary<TKey, int> ItemIndex = new Dictionary<TKey, int>();
        List<TKey> Items = new List<TKey>();
        List<int> parents = new List<int>();
        List<int> rank = new List<int>();
        public void MakeSet(TKey Item)
        {
            int n = parents.Count;
            Items.Add(Item);
            ItemIndex[Item] = n;
            parents.Add(n);
            rank.Add(1);
        }
        public int Find(TKey Item)
        {
            return Find(parents[ItemIndex[Item]]);
        }
        public int Find(int index)
        {
            if (parents[index] != index)
                parents[index] = Find(parents[index]);
            return parents[index];
        }
        public void Union(TKey Ki, TKey Kj)
        {
            int i = ItemIndex[Ki];
            int j = ItemIndex[Kj];
            int Iroot = Find(i);
            int Jroot = Find(j);
            if (Iroot == Jroot)
                return;
            if (rank[Iroot] == rank[Jroot])
            {
                parents[Iroot] = Jroot;
                rank[Jroot]++;
            }
            if (rank[Iroot] < rank[Jroot])
            {
                parents[Iroot] = Jroot;
            }
            else
            {
                parents[Jroot] = Iroot;
            }
        }
        public List<HashSet<TKey>> GetSets()
        {
            int n = parents.Count;
            Dictionary<int, HashSet<TKey>> RootToSet = new Dictionary<int, HashSet<TKey>>();
            for (int i = 0; i < n; i++)
            {
                int root = Find(i);
                if (!RootToSet.ContainsKey(root))
                    RootToSet[root] = new HashSet<TKey>() { Items[root] };
                RootToSet[root].Add(Items[i]);
            }
            return RootToSet.Values.ToList();
        }
        public void PrintParents()
        {
            parents.ToList().ForEach(x => Console.Write($"{x} "));
            Console.WriteLine();
        }
    }
    class FiniteStateMachine
    {
        public Dictionary<string, State> states;
        public HashSet<char> Alphabet;
        public State StartingState;
        public FiniteStateMachine(HashSet<char> Alphabet)
        {
            states = new Dictionary<string, State>();
            this.Alphabet = Alphabet;
        }
        public void AddState(string name, bool IsFinal)
        {
            var state = new State(name, IsFinal);
            if (states.Count == 0)
                StartingState = state;
            states.Add(name, state);
        }
        public void RemoveState(string name)
        {
            states.Remove(name);
        }
        public void AddTransition(string from, char Input, string to)
        {
            if (!Alphabet.Contains(Input) && Input != '$')
                throw new Exception("this input is not allowed");
            var a = states[from];
            var b = states[to];
            a.AddTransition(Input, b);
        }
        public bool MatchString(string input)
        {
            return Match(input, 0, StartingState);
        }
        bool Match(string text, int index, State x)
        {
            if (index == text.Length)
                return x.IsFinal;
            char NextCharachter = text[index];
            foreach (var transition in x.Transitions)
            {
                var input = transition.Key;
                foreach (var NextState in transition.Value)
                {
                    if ((input == NextCharachter && Match(text, index + 1, NextState)) ||
                        (input == '$' && Match(text, index, NextState)))
                        return true;
                }
            }
            return false;
        }
        public static FiniteStateMachine GetDFA(FiniteStateMachine NFA)
        {
            var Alphabet = NFA.Alphabet;
            FiniteStateMachine DFA = new FiniteStateMachine(Alphabet);
            HashSet<string> Proccesed = new HashSet<string>();
            Queue<HashSet<State>> queue = new Queue<HashSet<State>>();
            var initial = State.GetEpsilonClosure(new HashSet<State>() { NFA.StartingState });
            string initialName = State.GetName(initial);
            DFA.AddState(initialName, State.GetFinality(initial));
            DFA.AddState("ERR", false);
            bool ErrorNeeded = false;
            queue.Enqueue(initial);
            Proccesed.Add(initialName);
            while (queue.Count != 0)
            {
                var x = queue.Dequeue();
                string Xname = State.GetName(x);
                foreach (var letter in Alphabet)
                {
                    var Move = State.GetMove(x, letter);
                    if (Move.Count != 0)
                    {
                        var EpsilonClosure = State.GetEpsilonClosure(Move);
                        var EpsilonClosureName = State.GetName(EpsilonClosure);
                        if (!Proccesed.Contains(EpsilonClosureName))
                        {
                            Proccesed.Add(EpsilonClosureName);
                            DFA.AddState(EpsilonClosureName, State.GetFinality(EpsilonClosure));
                            queue.Enqueue(EpsilonClosure);
                        }
                        DFA.AddTransition(Xname, letter, EpsilonClosureName);
                    }
                    else
                    {
                        ErrorNeeded = true;
                        DFA.AddTransition(Xname, letter, "ERR");
                    }
                }
            }
            if (!ErrorNeeded)
                DFA.RemoveState("ERR");
            return DFA;
        }
        public int GetStateCount()
        {
            return states.Count;
        }
        public static FiniteStateMachine GetReducedDFA(FiniteStateMachine DFA)
        {
            var sets = DFA.partition();
            FiniteStateMachine ReducedDFA = new FiniteStateMachine(DFA.Alphabet);
            var transitions = DFA.GetAllTransitions();
            foreach (var transition in transitions)
            {
                var a = sets.Find(transition.Item1).ToString();
                var input = transition.Item2;
                var b = sets.Find(transition.Item3).ToString();
                if (!ReducedDFA.states.ContainsKey(a))
                    ReducedDFA.AddState(a, false);
                if (!ReducedDFA.states.ContainsKey(b))
                    ReducedDFA.AddState(b, false);
                if (transition.Item1.IsFinal)
                    ReducedDFA.states[a].IsFinal = true;
                if (transition.Item3.IsFinal)
                    ReducedDFA.states[b].IsFinal = true;
                ReducedDFA.AddTransition(a, input, b);
            }
            ReducedDFA.StartingState = ReducedDFA.states[sets.Find(DFA.StartingState).ToString()];
            return ReducedDFA;
        }
        public List<Tuple<State, char, State>> GetAllTransitions()
        {
            List<Tuple<State, char, State>> Transitions = new List<Tuple<State, char, State>>();
            foreach (var Item in states)
            {
                var state = Item.Value;
                foreach (var transition in state.Transitions)
                    Transitions.Add(new Tuple<State, char, State>(state, transition.Key, transition.Value.First()));
            }
            return Transitions;
        }
        public DisjointSet<State> partition()
        {
            var AreDistinct = MarkProcedure();
            var partitions = AssembleTheStates(AreDistinct);
            return partitions;
        }
        public DisjointSet<State> AssembleTheStates(Dictionary<StatePair, bool> AreDistinct)
        {
            DisjointSet<State> ds = new DisjointSet<State>();
            foreach (var state in states)
                ds.MakeSet(state.Value);
            var pairs = AreDistinct.Keys;
            foreach (var pair in pairs)
                if (!AreDistinct[pair])
                    ds.Union(pair.a, pair.b);
            return ds;
        }
        public Dictionary<StatePair, bool> MarkProcedure()
        {
            var pairs = GetAllPairs();
            Dictionary<StatePair, bool> Aredistinct = new Dictionary<StatePair, bool>();
            foreach (var pair in pairs)
                Aredistinct.Add(pair, pair.a.IsFinal ^ pair.b.IsFinal);
            bool finished = false;
            while (!finished)
                finished = !Mark(Aredistinct);
            return Aredistinct;
        }
        public void RemoveRedundantStates()
        {
            HashSet<State> mark = new HashSet<State>();
            HashSet<State> RedundantStates = new HashSet<State>();
            DFS(StartingState, mark);
            foreach (var state in states.Values)
                if (!mark.Contains(state))
                    RedundantStates.Add(state);
            foreach (var state in RedundantStates)
                RemoveState(state.name);
        }

        public void DFS(State x, HashSet<State> mark)
        {
            mark.Add(x);
            var neighbors = x.Transitions.Values;
            foreach (var neighbor in neighbors)
                if (!mark.Contains(neighbor.First()))
                    DFS(neighbor.First(), mark);
        }
        public bool Mark(Dictionary<StatePair, bool> AreDistinct)
        {
            var pairs = AreDistinct.Keys;
            HashSet<StatePair> HaveChanged = new HashSet<StatePair>();
            foreach (var pair in pairs)
            {
                var a = pair.a;
                var b = pair.b;
                foreach (var input in Alphabet)
                {
                    var NextA = a.Transitions[input].First();
                    var NextB = b.Transitions[input].First();
                    var NextPair = new StatePair(NextA, NextB);
                    if ((!AreDistinct[pair] && !HaveChanged.Contains(pair)) && (AreDistinct[NextPair] || HaveChanged.Contains(NextPair)))
                    {
                        HaveChanged.Add(pair);
                        break;
                    }
                }
            }
            if (HaveChanged.Count == 0)
                return false;
            foreach (var pair in HaveChanged)
                AreDistinct[pair] = true;
            return true;
        }
        public HashSet<StatePair> GetAllPairs()
        {
            HashSet<StatePair> pairs = new HashSet<StatePair>();
            foreach (var a in states)
            {
                foreach (var b in states)
                {
                    var pair = new StatePair(a.Value, b.Value);
                    if (!pairs.Contains(pair))
                        pairs.Add(pair);
                }
            }
            return pairs;
        }
    }
    struct StatePair
    {
        public State a;
        public State b;
        public StatePair(State a, State b)
        {
            if (a.name.CompareTo(b.name) < 0)
            {
                this.a = a;
                this.b = b;
            }
            else
            {
                this.a = b;
                this.b = a;
            }
        }
    }
    class State
    {
        public string name;
        public bool IsFinal;
        public Dictionary<char, HashSet<State>> Transitions;
        public State(string name, bool IsFinal = false)
        {
            this.name = name;
            this.IsFinal = IsFinal;
            this.Transitions = new Dictionary<char, HashSet<State>>();
        }
        public void AddTransition(char Input, State NextState)
        {
            if (Transitions.ContainsKey(Input))
                Transitions[Input].Add(NextState);
            else
                Transitions.Add(Input, new HashSet<State> { NextState });
        }
        public static HashSet<State> GetEpsilonClosure(HashSet<State> initial)
        {
            HashSet<State> EpsilonClosure = new HashSet<State>();
            Queue<State> queue = new Queue<State>();
            foreach (var state in initial)
                queue.Enqueue(state);
            while (queue.Count != 0)
            {
                var x = queue.Dequeue();
                EpsilonClosure.Add(x);
                if (x.Transitions.ContainsKey('$'))
                {
                    var NextStates = x.Transitions['$'];
                    foreach (var NextState in NextStates)
                        queue.Enqueue(NextState);
                }
            }
            return EpsilonClosure;
        }
        public static HashSet<State> GetMove(HashSet<State> initial, char c)
        {
            HashSet<State> Move = new HashSet<State>();
            foreach (var state in initial)
                if (state.Transitions.ContainsKey(c))
                    Move.UnionWith(state.Transitions[c]);
            return Move;
        }
        public static bool GetFinality(HashSet<State> Closure)
        {
            foreach (var state in Closure)
                if (state.IsFinal)
                    return true;
            return false;
        }
        public static string GetName(HashSet<State> Closure)
        {
            var names = Closure.Select(x => x.name).ToArray();
            Array.Sort(names);
            var name = string.Join("|", names);
            return name;
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            var buffer = Console.ReadLine().Split();
            int Q = int.Parse(buffer[0]);
            int A = int.Parse(buffer[1]);
            int F = int.Parse(buffer[2]);
            int T = int.Parse(buffer[3]);
            int N = int.Parse(buffer[4]);
            HashSet<char> Alphabet = new HashSet<char>();
            for(int i = 0; i < A; i++)
                Alphabet.Add(Console.ReadLine()[0]);
            FiniteStateMachine nfa = new FiniteStateMachine(Alphabet);
            int startState = int.Parse(Console.ReadLine());
            HashSet<int> endStates = new HashSet<int>();
            for (int i = 0; i < F; i++)
                endStates.Add(int.Parse(Console.ReadLine()));
            nfa.AddState(startState.ToString(), endStates.Contains(startState));
            for (int i = 0; i < Q; i++)
                if(i != startState)
                    nfa.AddState(i.ToString(), endStates.Contains(i));
            for(int i = 0; i < T; i++)
            {
                buffer = Console.ReadLine().Split();
                int start = int.Parse(buffer[0]);
                char input = buffer[1][0];
                int end = int.Parse(buffer[2]);
                nfa.AddTransition(start.ToString(), input, end.ToString());
            }

            FiniteStateMachine dfa = FiniteStateMachine.GetDFA(nfa);

            for(int i = 0; i < N; i++)
            {
                string input = Console.ReadLine();
                Console.WriteLine(dfa.MatchString(input));
            }

            Console.WriteLine("----------");
            var n = dfa.states.Count;
            var transitions = new HashSet<Tuple<string, char, string>>();
            var states = dfa.states;
            var finalStates = new HashSet<string>();
            foreach (var state in states)
            {
                foreach (var endstate in state.Value.Transitions)
                    transitions.Add(new Tuple<string, char, string>(state.Key, endstate.Key, endstate.Value.First().name));
                if (state.Value.IsFinal)
                    finalStates.Add(state.Value.name);
            }
            Console.WriteLine($"{n} {finalStates.Count} {transitions.Count}");
            Console.WriteLine(dfa.StartingState.name);
            foreach (var state in finalStates)
                Console.WriteLine($"{state}");
            foreach(var transition in transitions)
                Console.WriteLine($"{transition.Item1} {transition.Item2} {transition.Item3}");
        }
    }
}