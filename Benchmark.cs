using System.Diagnostics;
using MathNet.Numerics.Statistics;

partial class Program
{
    public static void Benchmark(in int N)
    {
        var testTimes = 50;
        // System.Console.WriteLine($"{N} random elements, Rank {BTree<int>.CHILD_CAPACITY} tree, #test: {testTimes}");
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
            timesInsertion[k] = sw.ElapsedMilliseconds;
            sw.Restart();
            for (int i = 0; i < N; i++)
            {
                X.DeleteAt(rnd.Next((N - 1) - i));
            }
            sw.Stop();
            timesDeletion[k] = sw.ElapsedMilliseconds;
            // Console.Write("\x1b[2K\r");
        }
        // System.Console.WriteLine($"Insertion:{timesInsertion.Average(),10:f3}"
        //     + $"±{timesInsertion.PopulationStandardDeviation(),8:f3}ms");
        // System.Console.WriteLine($"Deletion :{timesDeletion.Average(),10:f3}"
        //     + $"±{timesDeletion.PopulationStandardDeviation(),8:f3}ms");
        System.Console.WriteLine(
            string.Join(',', new long[] { N, BTree<int>.CHILD_CAPACITY, testTimes })
            + "," + string.Join(
                ',', new string[] {
                    string.Format("{0:f3}", timesInsertion.Average()),
                    string.Format("{0:f3}", timesInsertion.PopulationStandardDeviation()),
                    string.Format("{0:f3}", timesDeletion.Average()),
                    string.Format("{0:f3}", timesDeletion.PopulationStandardDeviation()),
                    }
                )
            );
    }
}