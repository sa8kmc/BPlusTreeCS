using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Diagnostics;

var N = 256;
// Program.Benchmark(N);

var X = new BTree<int>();
var S = Enumerable.Range(0, N).OrderBy(i => Guid.NewGuid()).ToList();
foreach (var s in S)
{
    X.Insert(s, -s);
}
foreach (var s in S)
{
    X.SearchAt(s);
    System.Console.WriteLine(X.GetData() + s);
}
