using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Diagnostics;

var N = 100;
// Program.Benchmark(N);


var X = new BTree<int>();
var S = Enumerable.Range(0, N).OrderBy(i => Guid.NewGuid()).ToList();
var rnd = new Random();
var tmp = 0;
for (int i = 0; i < N; i++)
{
    // X.InsertAt(rnd.Next(i + 1), -i);
    X.PushBack(i);
    if (X.size - tmp != 1) throw new SystemException("Error in Count");
    tmp = X.size;
}
// System.Console.WriteLine(X.isBPlusTree());
for (int i = 0; i < N; i++)
{
    var range = rnd.Next(N) + 1;
    var count = rnd.Next(range);
    X.Roll(range, count);
    if (X.size != N) throw new SystemException("Error in Count");
    if (!X.isBPlusTree()) System.Console.WriteLine("Warning: B+ structure Collapsed");
}


// for (int i = 0; i < N; i++)
// {
//     X.DeleteAt(rnd.Next((N - 1) - i));
//     // System.Console.WriteLine(X);
//     if (X.size - tmp != -1) throw new SystemException("Error in Count");
//     tmp = X.size;
// }



