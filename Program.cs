using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Diagnostics;

var N = 100000;
Program.Benchmark(N);

// var X = new BTree<int>();
// var S = Enumerable.Range(0, N).OrderBy(i => Guid.NewGuid()).ToList();
// var rnd = new Random();
// var tmp = 0;
// for (int i = 0; i < N; i++)
// {
//     X.InsertAt(rnd.Next(i + 1), -i);
//     System.Console.WriteLine(X.size);
//     if (X.size - tmp != 1) throw new SystemException("Error in Count");
//     tmp = X.size;
// }
// for (int i = 0; i < N; i++)
// {
//     X.DeleteAt(rnd.Next((N - 1) - i));
//     System.Console.WriteLine(X.size);
//     if (X.size - tmp != -1) throw new SystemException("Error in Count");
//     tmp = X.size;
// }