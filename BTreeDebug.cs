// var N = 10000;
// var BT = new BTree<char>(Enumerable.Range(0, N).Select(i => (char)(i + 32)).ToArray());
// var raw = BT.ToArray();
// var M = 500;
// var Queries = new (int range, int count)[M];
// var rnd = new Random(1);
// for (int i = 0; i < M; i++)
// {
//     var range = rnd.Next(N) + 1;
//     var count = rnd.Next(1, range);
//     Queries[i] = (range, count);
// }
// for (int i = 0; i < M; i++)
// {
//     var (range, count) = Queries[i];
//     BT.Roll(range, count);
//     Extend.RollPancake(raw, N, range, count);
//     System.Console.WriteLine(BT.size);
// }
// System.Console.WriteLine(BT.isBPlusTree());
// System.Console.WriteLine(string.Join("", raw));
// System.Console.WriteLine(string.Join("", BT.ToArray()));