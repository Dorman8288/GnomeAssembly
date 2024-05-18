using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Puzzel
{
    class Piece
    {
        public string Up;
        public string Left;
        public string Down;
        public string Right;
        public Piece(string up, string left, string down, string right)
        {
            Up = up;
            Left = left;
            Down = down;
            Right = right;
        }
    }

    internal class Program
    {
        static int n = 5;
        static Piece[][] Table;
        static Piece[] pieces;
        static bool[] used;

        static Tuple<int, int> FindNext()
        {
            for(int i = 1; i <= n; i++) 
                for (int j = 1; j <= n; j++)
                    if (Table[i][j] == null)
                        return new Tuple<int, int>(i, j);
            return null;
        }

        static bool Finished = false;
        static void Solve(int x, int y)
        {
            for(int i = 0; i < pieces.Length; i++)
            {
                if (!used[i])
                {
                    var piece = pieces[i];
                    if ((Table[x + 1][y] == null || Table[x + 1][y].Up == piece.Down) &&
                        (Table[x - 1][y] == null || Table[x - 1][y].Down == piece.Up) &&
                        (Table[x][y + 1] == null || Table[x][y + 1].Left == piece.Right) &&
                        (Table[x][y - 1] == null || Table[x][y - 1].Right == piece.Left))
                    {
                        Table[x][y] = piece;
                        used[i] = true;
                        var next = FindNext();
                        if(next == null)
                        {
                            Finished = true;
                            return;
                        }
                        else
                        {
                            Solve(next.Item1, next.Item2);
                            if (Finished)
                                return;
                            else
                                used[i] = false;
                        }
                    }
                }
            }
            Table[x][y] = null;
            return;
        }
        static void Main(string[] args)
        {
            Table = new Piece[n + 2][];
            for (int i = 0; i < n + 2; i++)
                Table[i] = new Piece[n + 2];
            Piece Black = new Piece("black", "black", "black", "black");
            for (int i = 0; i < n + 2; i++)
            {
                Table[i][0] = Black;
                Table[i][n + 1] = Black;
                Table[0][i] = Black;
                Table[n + 1][i] = Black;
            }
            pieces = new Piece[n * n];
            used = new bool[n * n];
            for(int i = 0; i < n * n; i++)
            {
                var colors = Console.ReadLine().Trim('(', ')').Split(',');
                var up = colors[0];
                var left = colors[1];
                var down = colors[2];
                var right = colors[3];
                pieces[i] = new Piece(up, left, down, right);
            }
            Solve(1, 1);
            for(int i = 1; i <= n; i++)
            {
                for(int j = 1; j <= n; j++)
                {
                    Console.Write($"({Table[i][j].Up},{Table[i][j].Left},{Table[i][j].Down},{Table[i][j].Right})");
                    if (j != n)
                        Console.Write(";");
                }
                Console.WriteLine();
            }
        }
    }
}
/*
(black, black, blue, cyan)
(black, brown, maroon, red)
(black, cyan, yellow, brown)
(black, red, green, black)
(black, red, white, red)
(blue, black, orange, yellow)
(blue, cyan, white, black)
(brown, maroon, orange, yellow)
(green, blue, blue, black)
(maroon, black, yellow, purple)
(maroon, blue, black, orange)
(maroon, orange, brown, orange)
(maroon, yellow, white, cyan)
(orange, black, maroon, cyan)
(orange, orange, black, black)
(orange, purple, maroon, cyan)
(orange, purple, purple, purple)
(purple, brown, black, blue)
(red, orange, black, orange)
(white, cyan, red, orange)
(white, orange, maroon, blue)
(white, orange, orange, black)
(yellow, black, black, brown)
(yellow, cyan, orange, maroon)
(yellow, yellow, yellow, orange)
*/