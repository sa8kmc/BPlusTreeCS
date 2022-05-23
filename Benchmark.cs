using System.Diagnostics;
using MathNet.Numerics.Statistics;

partial class Program
{
    public static void Benchmark(in int N)
    {
        var testTimes = 50;
        System.Console.WriteLine($"{N} random elements, Rank {BTree<int>.CHILD_CAPACITY} tree, #test: {testTimes}");
        double[] timesInsertion = new double[testTimes], timesDeletion = new double[testTimes];
        for (int k = 0; k < testTimes; k++)
        {
            // Console.Write($"case {k}");
            var X = new BTree<int>();
            var S = Enumerable.Range(0, N).OrderBy(i => Guid.NewGuid()).ToList();
            var sw = new Stopwatch();
            sw.Start();
            foreach (var s in S)
            {
                X.Insert(s, -s);
            }
            sw.Stop();
            timesInsertion[k] = sw.ElapsedMilliseconds;
            S = S.OrderBy(i => Guid.NewGuid()).ToList();
            sw.Restart();
            foreach (var s in S)
            {
                X.Delete(s);
            }
            sw.Stop();
            timesDeletion[k] = sw.ElapsedMilliseconds;
            // Console.Write("\x1b[2K\r");
        }
        System.Console.WriteLine($"Insertion:{timesInsertion.Average(),10:f3}"
            + $"±{timesInsertion.PopulationStandardDeviation(),8:f3}ms");
        System.Console.WriteLine($"Deletion :{timesDeletion.Average(),10:f3}"
            + $"±{timesDeletion.PopulationStandardDeviation(),8:f3}ms");

    }
}