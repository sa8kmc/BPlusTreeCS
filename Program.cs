using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Diagnostics;

var N = 26;
// Program.Benchmark(N);


// var X = new BTree<int>();
// var S = Enumerable.Range(0, N).OrderBy(i => Guid.NewGuid()).ToList();
// var rnd = new Random();
// var tmp = 0;
// for (int i = 0; i < N; i++)
// {
//     // X.InsertAt(rnd.Next(i + 1), -i);
//     X.PushBack(i);
//     X.PushFront(-i);
//     if (X.size - tmp != 2) throw new SystemException("Error in Count");
//     tmp = X.size;
// }
// System.Console.WriteLine(X);
// // var (L, R) = X.SplitAt(13);
// System.Console.WriteLine("");

// for (int i = 0; i < N; i++)
// {
//     X.DeleteAt(rnd.Next((N - 1) - i));
//     // System.Console.WriteLine(X);
//     if (X.size - tmp != -1) throw new SystemException("Error in Count");
//     tmp = X.size;
// }


var A = Enumerable.Range(0, N).ToArray();
var X = new BTree<int>(A);
System.Console.WriteLine(X);

var G = X.BuildFrom(A);

X.MergeRight(G);
System.Console.WriteLine(X);

