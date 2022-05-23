#if BenchmarkMode
var N = 100000;
Program.Benchmark(N);
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Diagnostics;


var X = new BTree<int>();
var keys = Enumerable.Range(0, 300000).OrderBy(s => Guid.NewGuid());
var tmp = 0;
foreach (var key in keys)
{
    X.Insert(key, -key);
    if (X.size - tmp != 1) throw new SystemException("Error in Count");
    tmp = X.size;
}
foreach (var key in keys)
{
    X.Delete(key);
    if (X.size - tmp != -1) throw new SystemException("Error in Count");
    tmp = X.size;
}