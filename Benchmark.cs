using System.Diagnostics;
using MathNet.Numerics.Statistics;
using System.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

partial class Program
{
    const char separator = '\t';
    public static double BenchmarkBTree(in int N)
    {
        var CU = new ContinuousUniform(0.0, 1.0);
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
                    var range = X.size;
                    var depth = Math.Min((int)(Math.Exp(CU.Sample() * Math.Log(range))), range);
                    X.Roll(depth, rnd.Next());
                    break;
            }
        }
        sw.Stop();
        return sw.ElapsedTicks / (double)Stopwatch.Frequency;
    }
    public static double BenchmarkStar(in int N)
    {
        var CU = new ContinuousUniform(0.0, 1.0);
        var L = new List<int>();
        var rnd = new Random();
        var sw = new Stopwatch();
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
                    var range = L.Count;
                    var depth = Math.Min((int)(Math.Exp(CU.Sample() * Math.Log(range))), range);
                    var count = rnd.Next(0, depth);
                    L.RollStar(depth, count);
                    break;
            }
        }
        sw.Stop();
        return sw.ElapsedTicks / (double)Stopwatch.Frequency;
    }
    public static double BenchmarkPancake(in int N)
    {
        var CU = new ContinuousUniform(0.0, 1.0);
        var A = new int[1024];
        var rnd = new Random();
        var sw = new Stopwatch();
        sw.Start();
        var end = 0;
        for (int k = 0; k < N; k++)
        {
            switch (rnd.Next(10))
            {
                case > 6:
                    if (end != 0) end--;
                    break;
                case > 0:
                    if (end >= A.Length)
                        Array.Resize(ref A, 2 * A.Length);
                    A[end] = k;
                    end++;
                    break;
                case 0:
                    if (end == 0) break;
                    var range = end;
                    var depth = Math.Min((int)(Math.Exp(CU.Sample() * Math.Log(range))), range);
                    var count = rnd.Next(0, depth);
                    A.RollPancake(range, depth, count);
                    break;
            }
        }
        sw.Stop();
        return sw.ElapsedTicks / (double)Stopwatch.Frequency;
    }
    public static double BenchmarkSwap(in int N)
    {
        var CU = new ContinuousUniform(0.0, 1.0);
        var A = new int[1024];
        var rnd = new Random();
        var sw = new Stopwatch();
        var Tmp = new int[1024];
        sw.Start();
        var end = 0;
        for (int k = 0; k < N; k++)
        {
            switch (rnd.Next(10))
            {
                case > 6:
                    if (end != 0) end--;
                    break;
                case > 0:
                    if (end >= A.Length)
                    {
                        Array.Resize(ref A, 2 * end);
                        Array.Resize(ref Tmp, 2 * end);
                    }
                    A[end] = k;
                    end++;
                    break;
                case 0:
                    if (end == 0) break;
                    var range = end;
                    var depth = Math.Min((int)(Math.Exp(CU.Sample() * Math.Log(range))), range);
                    var count = rnd.Next(0, depth);
                    A.RollCopy(range, ref Tmp, depth, count);
                    break;
            }
        }
        sw.Stop();
        return sw.ElapsedTicks / (double)Stopwatch.Frequency;
    }
    public static double BenchmarkRBT(in int N)
    {
        var CU = new ContinuousUniform(0.0, 1.0);
        var X = new RBT<int>();
        var rnd = new Random();
        var sw = new Stopwatch();
        sw.Start();
        for (int k = 0; k < N; k++)
        {
            switch (rnd.Next(10))
            {
                case > 6:
                    if (X.size != 0)
                        X.Pop();
                    break;
                case > 0:
                    X.Push(k);
                    break;
                case 0:
                    var range = X.size;
                    var depth = Math.Min((int)(Math.Exp(CU.Sample() * Math.Log(range))), range);
                    X.Roll(depth, rnd.Next());
                    break;
            }
        }
        sw.Stop();
        return sw.ElapsedTicks / (double)Stopwatch.Frequency;
    }
}