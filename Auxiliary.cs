using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
public static class Extend
{
    public static int Ceil(this int a, int b) => (a + b - 1) / b;
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
}