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
    public static void Benchmark2(in int N)
    {
        var testTimes = Math.Min(300, Math.Max(10, (int)(10_000_000 / N)));
        double[] timesInsertion = new double[testTimes], timesDeletion = new double[testTimes];
        for (int k = 0; k < testTimes; k++)
        {
            // Console.Write($"case {k}");
            var ERG = Enumerable.Range(0, N).ToArray();
            var rnd = new Random();
            var sw = new Stopwatch();
            sw.Start();
            var X = new BTree<int>(ERG);
            sw.Stop();
            timesInsertion[k] = sw.ElapsedMilliseconds;
            sw.Restart();
            for (int i = 0; i < N; i++)
            {
                X.DeleteAt(0);
            }
            sw.Stop();
            timesDeletion[k] = sw.ElapsedMilliseconds;
            // Console.Write("\x1b[2K\r");
        }
        System.Console.WriteLine(
            string.Join(',', new long[] { N, BTree<int>.CAPACITY, testTimes })
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