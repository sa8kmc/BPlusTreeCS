using System.Diagnostics;
using MathNet.Numerics.Statistics;

partial class Program
{
    public static void Benchmark(in int N)
    {
        var testTimes = Math.Max(10, (int)(1_000_000 / N * Math.Log10(N)));
        double[] timesInsertion = new double[testTimes], timesDeletion = new double[testTimes];
        for (int k = 0; k < testTimes; k++)
        {
            // Console.Write($"case {k}");
            var X = new BTree<int>();
            var rnd = new Random();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < N; i++)
            {
                X.InsertAt(rnd.Next(i), -i);
            }
            sw.Stop();
            timesInsertion[k] = sw.ElapsedTicks;
            sw.Restart();
            for (int i = 0; i < N; i++)
            {
                X.DeleteAt(rnd.Next((N - 1) - i));
            }
            sw.Stop();
            timesDeletion[k] = sw.ElapsedTicks;
            // Console.Write("\x1b[2K\r");
        }
        System.Console.WriteLine(
            string.Join(',', new long[] { N, BTree<int>.CAPACITY, testTimes
    })
                + "," + string.Join(
                    ',', new string[] {
                    string.Format("{0:f3}", 1000.0 / Stopwatch.Frequency * timesInsertion.Average()),
                    string.Format("{0:f3}", 1000.0 / Stopwatch.Frequency * timesInsertion.PopulationStandardDeviation()),
                    string.Format("{0:f3}", 1000.0 / Stopwatch.Frequency * timesDeletion.Average()),
                    string.Format("{0:f3}", 1000.0 / Stopwatch.Frequency * timesDeletion.PopulationStandardDeviation()),
                        }
                    )
                );
    }
    public static void BenchmarkRoll(in int N)
    {
        double timeB, timePancake;
        var L = new List<int>();
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
        timeB = sw.ElapsedTicks / (double)Stopwatch.Frequency;
        sw.Restart();
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
        timePancake = sw.ElapsedTicks / (double)Stopwatch.Frequency;
        System.Console.WriteLine(
            string.Join(',', new long[] { N, BTree<int>.CAPACITY })
            + "," + string.Join(
                ',', new string[] {
                    string.Format("{0:f3}", timeB),
                    string.Format("{0:f3}", timePancake),
                    }
                )
            );
    }
}