using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

var X = new BTree<int>();
var rnd = new Random();
var S = Enumerable.Range(0, 257).OrderBy(i => Guid.NewGuid());
// var S = new int[] { 2, 3, 4, 6, 7, 8, 1 };
foreach (var s in S)
{
    X.Insert(s, -s);
}
System.Console.WriteLine(X.ToString());
// System.Console.ReadLine();
S = S.OrderBy(i => Guid.NewGuid());
foreach (var s in S)
{
    X.Delete(s);
}
System.Console.WriteLine(X.ToString());
