using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

var X = new BTree<int>();
var rnd = new Random();
var S = Enumerable.Range(0, 26).OrderBy(i => Guid.NewGuid());
System.Console.WriteLine(string.Join(',', S));
// var S = new int[] { 2, 3, 4, 6, 7, 8, 1 };
var i = 0;
foreach (var s in S)
{
    System.Console.WriteLine($"\t Level {++i}");
    X.Insert(s, -s);
    System.Console.WriteLine(X.ToString());
}

S = S.OrderBy(i => Guid.NewGuid());
foreach (var s in S)
{
    System.Console.WriteLine($"\t Level {--i}");
    X.Delete(s);
    System.Console.WriteLine(X.ToString());

}