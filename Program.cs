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
// var tmp = 0;
// foreach (var s in S)
// {
//     X.Insert(s, -s);
//     // System.Console.WriteLine("\t" + tmp);
//     // System.Console.WriteLine(X.ToString());
//     if (X.size - tmp != 1) throw new SystemException("Error in Count");
//     tmp = X.size;
// }
// foreach (var s in S)
// {
//     X.Delete(s);
//     // System.Console.WriteLine(X.GetData() + s);
//     if (X.size - tmp != -1) throw new SystemException("Error in Count");
//     tmp = X.size;
// }
