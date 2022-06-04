using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
public static class Extend
{
    public static int Ceil(this int a, int b) => (a + b - 1) / b;
    public static int GCD(int x, int y)
    {
        if (x == 0) return y;
        else if (y == 0) return x;
        var m = x % y;
        if (m < 0) m += y;
        return GCD(y, m);
    }
    /// <summary>
    /// 符号を除数と等しく取った剰余演算。
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int Mod(int x, int y)
    {
        int sign = 1;
        if (y == 0) throw new DivideByZeroException();
        else if (y < 0)
        {
            x = -x;
            sign = -1;
        }
        int abs = x % y;
        if (abs < 0) abs += y;
        return sign * abs;
    }
    public static void RollPancake<T>(this List<T> X, int depth, int count)
    {
        var N = X.Count;
        X.Reverse(N - depth, depth - count);
        X.Reverse(N - count, count);
        X.Reverse(N - depth, depth);
    }
    public static void RollStar<T>(this List<T> X, int depth, int count)
    {
        if (depth == 0 || depth > X.Count) return;
        var neck = X.Count - depth;
        count = Mod(count, depth);
        var g = GCD(depth, count);
        var road = depth / g;
        T tmp;
        for (int i = 0; i < g; i++)
        {
            tmp = X[neck + ((road - 1) * g + i) % depth];
            for (int j = road - 2; j >= 0; j--)
                X[neck + (g * (j + 1) + i) % depth] = X[neck + (g * j + i) % depth];
            X[neck + i % depth] = tmp;
        }
    }
    #region BinarySearch
    public static int lowerBound<T>(this T[] X, T key) where T : IComparable<T>
    {
        var N = X.Length;
        int i = -1, j = N - 1; // (i,j]
        if (X[j].CompareTo(key) < 0) return N;
        while (j - i > 1)
        {
            var mid = (i + j) / 2; //since j-i>=2, i<mid<j.
            if (X[mid].CompareTo(key) < 0)
                i = mid;
            else
                j = mid;
        }
        return j;
    }
    public static int upperBound<T>(this T[] X, T key) where T : IComparable<T>
    {
        var N = X.Length;
        int i = -1, j = N - 1; // (i,j]
        if (X[j].CompareTo(key) <= 0) return N;
        while (j - i > 1)
        {
            var mid = (i + j) / 2; //since j-i>=2, i<mid<j.
            if (X[mid].CompareTo(key) <= 0)
                i = mid;
            else
                j = mid;
        }
        return j;
    }
    public static int lowerBound(this long[] X, long key)
    {
        var N = X.Length;
        if (N == 0) return 0;
        if (X[^1] < key) return N;
        int i = -1, j = N - 1; // (i,j]
        while (j - i > 1)
        {
            var mid = (i + j) / 2; //since j-i>=2, i<mid<j.
            if (X[mid] < key)
                i = mid;
            else
                j = mid;
        }
        return j;
    }
    public static int upperBound(this long[] X, long key)
    {
        var N = X.Length;
        if (N == 0) return 0;
        if (X[^1] <= key) return N;
        int i = -1, j = N - 1; // (i,j]
        while (j - i > 1)
        {
            var mid = (i + j) / 2; //since j-i>=2, i<mid<j.
            if (X[mid] <= key)
                i = mid;
            else
                j = mid;
        }
        return j;
    }
    #endregion
}