using System.Diagnostics;
using MathNet.Numerics.Statistics;

partial class Program
{
    const char separator = '\t';
    public static double BenchmarkBTree(in int N)
    {
        var X = new BTree<int>();
        var rnd = new Random();
        var sw = new Stopwatch();
        sw.Start();
        for (int k = 0; k < N; k++)
        {
            switch (rnd.Next(10))
            {
                case > 6:
                    if (X.size != 0)
                        X.PopBack();
                    break;
                case > 0:
                    X.PushBack(k);
                    break;
                case 0:
                    var depth = 1 + rnd.Next(0, X.size);
                    X.Roll(depth, rnd.Next());
                    break;
            }
        }
        sw.Stop();
        return sw.ElapsedTicks / (double)Stopwatch.Frequency;
    }
    public static double BenchmarkPancake(in int N)
    {
        var L = new List<int>();
        var rnd = new Random();
        var sw = new Stopwatch();
        var Tmp = new int[1024];
        sw.Start();
        for (int k = 0; k < N; k++)
        {
            switch (rnd.Next(10))
            {
                case > 6:
                    if (L.Count != 0)
                        L.RemoveAt(L.Count - 1);
                    break;
                case > 0:
                    L.Add(k);
                    break;
                case 0:
                    if (L.Count == 0) break;
                    var depth = 1 + rnd.Next(0, L.Count);
                    var count = rnd.Next(0, depth);
                    L.RollPancake(depth, count);
                    break;
            }
        }
        sw.Stop();
        return sw.ElapsedTicks / (double)Stopwatch.Frequency;
    }
    public static double BenchmarkSwap(in int N)
    {
        var L = new List<int>();
        var rnd = new Random();
        var sw = new Stopwatch();
        var Tmp = new int[1024];
        sw.Start();
        for (int k = 0; k < N; k++)
        {
            switch (rnd.Next(10))
            {
                case > 6:
                    if (L.Count != 0)
                        L.RemoveAt(L.Count - 1);
                    break;
                case > 0:
                    L.Add(k);
                    break;
                case 0:
                    if (L.Count == 0) break;
                    var depth = 1 + rnd.Next(0, L.Count);
                    var count = rnd.Next(0, depth);
                    L.RollCopy(ref Tmp, depth, count);
                    break;
            }
        }
        sw.Stop();
        return sw.ElapsedTicks / (double)Stopwatch.Frequency;
    }
}