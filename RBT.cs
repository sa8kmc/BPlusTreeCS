using System.Runtime.CompilerServices;
/*
 * 参考：保坂 和宏. 『コピー＆ペースト (Copy and Paste) JOI 春合宿 2012 Day 4』. 2012. https://www.ioi-jp.org/camp/2012/2012-sp-tasks/2012-sp-day4-copypaste-slides.pdf
 * 参考：mitaki28.info. 『永続赤黒木を実装した時のメモ』. 2015. http://blog.mitaki28.info/1447078746296/
 */
/// <summary>
/// 情報を葉にのみ持たせた赤黒木。
/// 各ノードに対応した部分木の中間順をキーとする。
/// </summary>
/// <typeparam name="T">格納データの型。</typeparam>
class RBT<T>
{
    const string CSI = Extend.CSI;
    const string backColorCancelor = CSI + "40m";
    const string NL = "\n";
    const char SPACE_TAB = ' ';
    const char TRAVEL_TAB = '│';
    const char LEFT_TAB = '├';
    const char RIGHT_TAB = '└';
    #region NodeClassDefinition
    internal abstract class Node
    {
#if DEBUG
        internal long serial;
#endif
        internal abstract int size { get; }
        internal bool isRed { get; set; }
        internal abstract int rank { get; }
        internal virtual int nextRank { get => rank + (isRed ? 0 : 1); }
        public abstract int totalSize();
        internal string backColorPresentor { get => CSI + (isRed ? "41m" : "44m"); }
        internal abstract string ToStringRecursive(int level);
        internal abstract string ToStringRecursive(string head);
    }
    internal class Leaf : Node
    {
        internal T data;
        internal override int size { get => 1; }
        internal override int rank { get => 0; }
        internal override int nextRank { get => 1; }
        internal Leaf(T data)
        {
            isRed = false;
#if DEBUG
            serial = serialNumber++;
#endif
            this.data = data;
        }
        public override int totalSize() => 1;
        public override string ToString()
            => CSI + "48;5;232m" + data + backColorCancelor;
        internal override string ToStringRecursive(int level)
            => new string(SPACE_TAB, level) + this.ToString();
        internal override string ToStringRecursive(string head)
        {
            var outHead = "";
            if (head != "")
            {
                var isRight = head[^1] == SPACE_TAB;
                outHead = head[..^1] + (isRight ? RIGHT_TAB : LEFT_TAB);
            }
            return outHead + this.ToString();
        }
    }
    internal class InternalNode : Node
    {
        /// <summary>
        /// 探索では、key<bornderのときLに、key>=borderのときRに移動させる。
        /// </summary>
        /// <value></value>
        internal int border { get; set; }
        internal int counterBorder { get; set; }
        internal override int size { get => border + counterBorder; }
        private int _rank;
        internal override int rank { get => _rank; }
        internal Node L, R;
        public override int totalSize() => L.totalSize() + 1 + R.totalSize();
        internal void FlipColor() => isRed ^= true;
        internal void ChangeToRed(bool toRed) => isRed = toRed;
        internal InternalNode(int toRank, Node _L, Node _R, bool _isRed = false)
        {
#if DEBUG
            serial = serialNumber++;
#endif
            L = _L; R = _R;
            _rank = toRank;
            isRed = _isRed;
            border = _L.size;
            counterBorder = _R.size;
        }
        internal bool isRightSide(int i, out int remain)
        {
#if DEBUG
            if (i < 0 || i > size) throw new IndexOutOfRangeException();
#endif
            if (i < border)
            {
                remain = i;
                return false;
            }
            else
            {
                remain = i - border;
                return true;
            }
        }
        internal Node Succeed(int i, out int remain)
        {
            return isRightSide(i, out remain) ? R : L;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Node Child(bool rightward) => rightward ? R : L;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetChild(bool rightward, Node New)
        {
            if (rightward) R = New;
            else L = New;
        }
        internal void Update()
        {
            border = L.size;
            _rank = L.rank + (L.isRed ? 0 : 1);
#if DEBUG
            if (L.rank + (L.isRed ? 0 : 1) != R.rank + (R.isRed ? 0 : 1))
                throw new InvalidOperationException();
#endif
            counterBorder = R.size;
        }
        public override string ToString()
            => backColorPresentor + $"({_rank}){size}->{border}|{counterBorder}" + backColorCancelor;
        internal override string ToStringRecursive(int level)
            => new string(SPACE_TAB, level) + this.ToString() + NL
            + L.ToStringRecursive(level + 1) + NL
            + R.ToStringRecursive(level + 1);
        internal override string ToStringRecursive(string head)
        {
            var outHead = "";
            if (head != "")
            {
                var isRight = head[^1] == SPACE_TAB;
                outHead = head[..^1] + (isRight ? RIGHT_TAB : LEFT_TAB);
            }
            return outHead + this.ToString() + NL
                + L.ToStringRecursive(head + TRAVEL_TAB) + NL
                + R.ToStringRecursive(head + SPACE_TAB);
        }
    }
    #endregion
    #region PropertiesAndFields
    internal Node? root { get; private set; }
#if DEBUG
    private static long serialNumber = 0;
#endif
    public Leaf? currentLeaf { get; private set; }
    public int totalRank
    {
        get
        {
            if (root == null) return 0;
            else return root!.rank;
        }
    }
    public int size { get => root?.size ?? 0; }
    public int totalSize()
    {
        if (root == null) return 0;
        return root.totalSize();
    }
    #endregion
    #region ConstructorAndBuilder
    public RBT() { root = null; }
    public RBT(T[] A) { root = BuildFrom(A); }
    private Node? BuildFrom(T[] A)
    {
        if (A.Length == 0) return null;
        else if (A.Length == 1)
            return new Leaf(A[0]);
        Node[] LevelPrev = new Node[A.Length];
        for (int i = 0; i < A.Length; i++)
            LevelPrev[i] = new Leaf(A[i]);
        InternalNode[] LevelTmp;
        var level = 1;
        while (LevelPrev.Length > 1)
        {
            var LN = LevelPrev.Length / 2;
            LevelTmp = new InternalNode[LN];
            // LevelPrevのノード数が奇数の場合は、最初2個を赤ノードでまとめて、偶数個にする
            if (LevelPrev.Length % 2 != 0)
            {
                LevelPrev[1] = new InternalNode(level, LevelPrev[0], LevelPrev[1], true);
                for (int i = 0; i < LN; i++)
                {
                    LevelTmp[i] = new InternalNode(level, LevelPrev[2 * i + 1], LevelPrev[2 * i + 2], false);
                }
            }
            else
            {
                for (int i = 0; i < LN; i++)
                {
                    LevelTmp[i] = new InternalNode(level, LevelPrev[2 * i], LevelPrev[2 * i + 1], false);
                }
            }
            LevelPrev = LevelTmp;
            level++;
        }
        return LevelPrev[0];
    }
    #endregion
    #region ToString
    /// <summary>
    /// 先行順でノードの情報を出力する。
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (root is null)
            return "NIL";
        else if (root is Leaf L)
            return L.ToString();
        else
            return (root as InternalNode)!.ToStringRecursive("");
    }
    #endregion
    #region Indexer
    public bool SearchAt(int i)
    {
        currentLeaf = null;
        if (root == null) return false;
        var p = root as Node;
        while (p is InternalNode node)
            p = node.Succeed(i, out i);
        currentLeaf = p as Leaf;
        return true;
    }
    public T? GetData(int i)
    {
        SearchAt(i);
        if (currentLeaf == null) return default(T);
        else return currentLeaf.data;
    }
    public bool SetData(int i, T data)
    {
        SearchAt(i);
        if (currentLeaf == null)
            return false;
        else
        {
            currentLeaf.data = data;
            return true;
        }
    }
    #endregion
    #region Rotation
    private static Node MergeSub(Node A, Node B)
    {
        switch (A.rank - B.rank)
        {
            case < 0:
                var BI = B as InternalNode;
                var CI = MergeSub(A, BI!.L) as InternalNode;
                if (!BI.isRed && CI!.isRed && CI!.L.isRed)
                {
                    if (!BI.R.isRed)
                    {
                        //右単回転
                        var CR = CI.R;
                        BI.L = CR;
                        BI.ChangeToRed(true);
                        BI.Update();
                        CI.R = BI;
                        CI.ChangeToRed(false);
                        CI.Update();
                        return CI;
                    }
                    else
                    {
                        //色反転
                        (BI.R as InternalNode)!.ChangeToRed(false);
                        CI.ChangeToRed(false);
                        BI.L = CI;
                        BI.ChangeToRed(true);
                        BI.Update();
                        return BI;
                    }
                }
                else
                {
                    //単純結合
                    BI.L = CI!;
                    BI.Update();
                    return BI;
                }
            case > 0:
                var AI = A as InternalNode;
                var DI = MergeSub(AI!.R, B) as InternalNode;
                if (!AI.isRed && DI!.isRed && DI!.R.isRed)
                {
                    if (!AI.L.isRed)
                    {
                        //左単回転
                        var DL = DI.L;
                        AI.R = DL;
                        AI.ChangeToRed(true);
                        AI.Update();
                        DI.L = AI;
                        DI.ChangeToRed(false);
                        DI.Update();
                        return DI;
                    }
                    else
                    {
                        //色反転
                        (AI.L as InternalNode)!.ChangeToRed(false);
                        DI.ChangeToRed(false);
                        AI.R = DI;
                        AI.ChangeToRed(true);
                        AI.Update();
                        return AI;
                    }
                }
                else
                {
                    //単純結合
                    AI.R = DI!;
                    AI.Update();
                    return AI;
                }
            default:
                var an = A.nextRank;
#if DEBUG
                var bn = B.nextRank;
                if (an != bn) throw new BadImageFormatException();
#endif
                return new InternalNode(an, A, B, true);
        }
    }
    private Node? MergeRight(Node? B)
    {
        var C = Merge(root, B);
        if (C != null && C.isRed)
            (C as InternalNode)!.FlipColor();
        root = C;
        return root;
    }
    private Node? MergeLeft(Node? A)
    {
        var C = Merge(A, root);
        if (C != null && C.isRed)
            (C as InternalNode)!.FlipColor();
        root = C;
        return root;
    }
    private static Node? Merge(Node? A, Node? B)
    {
        if (A is null) return B;
        if (B is null) return A;
        var C = MergeSub(A!, B!);
        return AsRoot(C);
    }
    public void InsertAt(int i, T x)
    {
        var X = new Leaf(x);
        if (root is null)
        {
            root = X;
            return;
        }
        else if (root is Leaf F) //feuille
        {
            if ((i & 1) == 0) root = Merge(X, F); //0,-2
            else root = Merge(F, X); //1,-1
            return;
        }
        var RI = (InternalNode)root;
        bool rightwise;
        if (i >= 0)
        {
            //正方向
            rightwise = RI.border <= i;
            var (L, R) = Split(RI, i);
            if (rightwise) root = Merge(Merge(L, X), R);
            else root = Merge(L, Merge(X, R));
            return;
        }
        else
        {
            //逆方向
            rightwise = ~i <= RI.counterBorder;
            var (L, R) = Split(RI, RI.size - (~i));
            if (rightwise) root = Merge(Merge(L, X), R);
            else root = Merge(L, Merge(X, R));
            return;
        }
    }
    public void Push(T x)
    {
        var X = new Leaf(x);
        if (root is null)
        {
            root = X;
            return;
        }
        root = Merge(root, X);
        return;
    }
    public T Pop()
    {
        if (root is Leaf F)
        {
            root = null;
            return F.data;
        }
        var RI = (InternalNode)root!;
        var (L, R) = Split(RI, ~1);
        root = L;
        return ((Leaf)R!).data;
    }

    /// <summary>
    /// 部分木Xを先頭at個とそれ以外とのそれぞれからなる二分木に分割する。
    /// </summary>
    /// <param name="L"></param>
    /// <param name="X"></param>
    /// <param name="at">非負のときは先頭at個とそれ以外、負のときは後方(~at)個とそれ以外</param>
    /// <returns></returns>
    private static (Node? L, Node? R) Split(Node? X, int at)
    {
        if (X is null) return (null, null);
#if DEBUG
        var i = at < 0 ? ~at : at;
        if (i > X!.size) throw new IndexOutOfRangeException();
#endif
        if (at == 0 || ~at == X!.size) return (null, X);
        if (~at == 0 || at == X!.size) return (X, null);
        if (at < 0) at = X!.size - (~at);
        var XI = (InternalNode)X;
        var remain = at - XI.border;
        if (remain < 0)
        {
            var (LL, LR) = Split(XI.L, at);
            return (LL, Merge(LR, AsRoot(XI.R)));
        }
        else if (remain > 0)
        {
            var (RL, RR) = Split(XI.R, remain);
            return (Merge(AsRoot(XI.L), RL), RR);
        }
        else
            return (AsRoot(XI.L), AsRoot(XI.R));
    }
    private static Node? AsRoot(Node? A)
    {
        if (A is null) return null;
        else if (A is Leaf) return A;
        var AI = (InternalNode)A;
        AI.ChangeToRed(false);
        return AI;
    }
    /// <summary>
    /// i番目にある要素を削除する。
    /// </summary>
    /// <param name="i">非負のときは通常のインデックス、負のときは右の番兵を0としたオフセットで指定。</param>
    public T DeleteAt(int i)
    {
        if (root is null) throw new IndexOutOfRangeException();
        else if (root is Leaf R) { var res = R.data; root = null; return res; }
        var RI = (InternalNode)root;
        if (i < 0) i = RI.size + i;
        var (A, B0) = Split(RI, i);
        var (BL, BR) = Split(B0, 1);
        root = Merge(A, BR);
        return ((Leaf)BL!).data;
    }
    public void Roll(int depth, int count)
    {
        if (depth == 0 || depth > size) return;
        count %= depth;
        if (count < 0) count += depth;
        var (Base, Effect) = Split(root, ~depth);
        var (Float, Sink) = Split(Effect, ~count);
        var Swapped = Merge(Sink, Float);
        root = Merge(Base, Swapped);
    }
    #endregion
}