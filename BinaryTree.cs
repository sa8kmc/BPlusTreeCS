using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
class BinaryTree1<T>//split-mergeベース
{
    //from:https://qiita.com/QCFium/items/3cf26a6dc2d49ef490d7
    public class Node1
    {
        public Node1 Left, Right;
        public T val;
        public int size, height;
        public Node1(Node1 _Left, Node1 _Right, int _size, int _height, T _val)
        {
            Left = _Left;
            Right = _Right;
            val = _val;
            size = _size;
            height = _height;
        }
        internal Node1 Fetch()
        {
            size = Left.size + 1 + Right.size;
            height = Math.Max(Left.height, Right.height) + 1;
            return this;
        }
        internal Node1 RotateL()
        {
            var newNode = Right;
            this.Right = newNode.Left;
            newNode.Left = this;
            this.Fetch();
            return newNode.Fetch();
        }
        internal Node1 RotateR()
        {
            var newNode = Left;
            this.Left = newNode.Right;
            newNode.Right = this;
            this.Fetch();
            return newNode.Fetch();
        }
        internal Node1 RotateLR()
        {
            Left = Left.RotateL();
            return RotateR();
        }
        internal Node1 RotateRL()
        {
            Right = Right.RotateR();
            return RotateL();
        }

        int heightDiff() => Left.height - Right.height;
        internal Node1 Balance()
        {
            var dif = this.heightDiff();
            switch (dif)
            {
                case 2:
                    if (Left.heightDiff() < 0) Left = Left.RotateL();
                    return RotateR();
                case -2:
                    if (Right.heightDiff() > 0) Right = Right.RotateR();
                    return RotateL();
                default:
                    return Fetch();
            }
        }
    }
    protected readonly static Node1 NIL = new Node1(null, null, 0, 0, default);
    protected Node1 Root;
    public static Node1 NewNode(T val) { return new Node1(NIL, NIL, 1, 1, val); }
    public BinaryTree1()
    {
        Root = NIL;
    }
    public BinaryTree1(T _val)
    {
        Root = NewNode(_val);
    }
    private protected Node1 MergeWithMid(Node1 Left, Node1 Mid, Node1 Right)
    //MidはLeft,Rightと繋がっていない
    {
        Mid.Left = Left;
        Mid.Right = Right;
        return Mid.Fetch();
    }
    private Node1 RemoveRightest(Node1 From, out Node1 Rightest)
    //Rightestに自身の最も右の子孫を格納し、それを削除したときに自身の嘗ての親の新たな子となるノードを返す関数。
    //副処理として、Rightestにある子孫を孤立させる。
    //Fromより上のFetchが行われないことに注意。
    //Root更新は入る。
    {
        if (From.Right != NIL)
        {
            From.Right = RemoveRightest(From.Right, out Rightest);
            return From.Fetch();
        }
        else
        {
            Rightest = From;
            var Res = From.Left;
            From.Left = NIL;
            From.Fetch();
            if (Root == From) Root = Res;
            return Res;
        }
    }
    private Node1 RemoveLeftest(Node1 From, out Node1 Leftest)
    //Leftestに自身の最も左の子孫を格納し、それを削除したときに自身の嘗ての親の新たな子となるノードを返す関数。
    //副処理として、Leftestにある子孫を孤立させる。
    //Fromより上のFetchが行われないことに注意。
    //Root更新は入る。
    {
        if (From.Left != NIL)
        {
            From.Left = RemoveLeftest(From.Left, out Leftest);
            return From.Fetch();
        }
        else
        {
            Leftest = From;
            var Res = From.Right;
            From.Right = NIL;
            From.Fetch();
            if (Root == From) Root = Res;
            return Res;
        }
    }
    private protected Node1 Merge(Node1 Left, Node1 Right)
    //Left、Rightが何かの子であるかどうかは分からない。
    //Root更新は入る。
    {
        if (Left == NIL) { if (Root == NIL) Root = Right; return Right; }
        if (Right == NIL) { if (Root == NIL) Root = Left; return Left; }
        Left = RemoveRightest(Left, out var Mid);
        var Res = MergeWithMid(Left, Mid, Right);
        if (Root == Left || Root == Right) Root = Res;
        return Res;
    }

    private protected (Node1 L, Node1 R) Split(Node1 From, int lc)
    //部分木Fromを、左lc個とそれ以外に分割する
    {
        if (lc < 0 || lc > From.size) throw new ArgumentOutOfRangeException();
        if (lc == 0) return (NIL, From);
        if (lc == From.size) return (From, NIL);
        var comp = lc - From.Left.size;
        switch (comp)
        {
            case < 0:
                var (LL, LR) = Split(From.Left, lc); // LL, LR not NIL
                var XR = MergeWithMid(LR, From, From.Right);
                return (LL, XR);
            case 0:
                var L = From.Left;
                From.Left = NIL;
                return (L, From.Fetch());
            case 1:
                var R = From.Right;
                From.Right = NIL;
                return (From.Fetch(), R);
            case > 1:
                var (RL, RR) = Split(From.Right, comp - 1);
                var XL = MergeWithMid(From.Left, From, RL);
                return (XL, RR);
        }
    }
    private protected (Node1 L, Node1 R) Split(int lc) => Split(Root, lc);
    private Node1 SplitBack(int lc, out Node1 Back)
    {
        var (L, R) = Split(lc);
        Root = L;
        Back = R;
        return L;
    }
    private Node1 SplitFront(int lc, out Node1 Front)
    {
        var (L, R) = Split(lc);
        Root = R;
        Front = L;
        return R;
    }

    public Node1 InsertBack(T val)
    {
        return Merge(Root, NewNode(val));
    }
    public Node1 InsertFront(T val)
    {
        return Merge(NewNode(val), Root);
    }
    public Node1 RemoveBack()
    {
        RemoveRightest(Root, out var Res);
        return Res;
    }
    public Node1 RemoveFront()
    {
        RemoveLeftest(Root, out var Res);
        return Res;
    }
    public Node1 InsertAt(int i, T val)
    //Root更新
    {
        var New = NewNode(val);
        var leftHeavy = 2 * i <= Root.size;
        var (L, R) = Split(i);
        if (leftHeavy) Root = Merge(L, Merge(New, R));
        else Root = Merge(Merge(L, New), R);
        return New;
    }
    private protected Node1 FindAt(Node1 From, int i)
    {
        var comp = i - From.Left.size;
        switch (comp)
        {
            case < 0:
                return FindAt(From.Left, i);
            case 0:
                return From;
            case > 0:
                return FindAt(From.Right, comp - 1);
        }
    }
    public virtual Node1 FindAt(int i) => FindAt(Root, i);
    public Node1 RemoveAt(int i)
    //Root更新
    {
        var (L, R) = Split(i);
        R = RemoveLeftest(R, out var Res);
        Root = Merge(L, R);
        return Res;
    }

    private void _InOrder(Node1 X, int d, List<(Node1 x, int depth)> order)
    {
        if (X == NIL) return;
        _InOrder(X.Left, d + 1, order);
        order.Add((X, d));
        _InOrder(X.Right, d + 1, order);
    }

    public List<(Node1 place, int depth)> InOrder()
    {
        var Res = new List<(Node1 place, int depth)>();
        Res.Capacity = Root.size;
        _InOrder(Root, 0, Res);
        return Res;
    }

    public void EmitInOrder()
    {
        System.Console.WriteLine(NIL.GetHashCode().ToString("x8"));
        var Locs = InOrder();
        var ordInd = 0;
        foreach (var (p, d) in Locs)
        {
            var ordSuf = "th";
            if (ordInd % 10 == 1 && ordInd % 100 != 11) ordSuf = "st";
            if (ordInd % 10 == 2 && ordInd % 100 != 12) ordSuf = "nd";
            if (ordInd % 10 == 3 && ordInd % 100 != 13) ordSuf = "rd";
            System.Console.WriteLine("{0,4:D}{1} -> loc:{2,4:D}, dep:{3,4:D}, size:{4,4:D}, height:{5,4:D} "
            + "-> val:{6,4}, Left:{7,4:D}, Right:{8,4:D}",
            ordInd++, ordSuf, p.GetHashCode().ToString("x8"), d, p.size, p.height,
            p.val, p.Left.GetHashCode().ToString("x8"), p.Right.GetHashCode().ToString("x8"));
        }
    }
    public void EmitTree(int valDigit = 4)
    //木の表示のジャグ配列を作り、その後各行に値を書いて、改行で繋げる。
    {
        var Locs = InOrder();
        var Ord = new Dictionary<Node1, int>(); //place to order
        for (int i = 0; i < Root.size; i++)
            Ord.Add(Locs[i].place, i);
        var Branches = new char[Root.size][];
        for (int i = 0; i < Root.size; i++)
            Branches[i] = Enumerable.Repeat(' ', Locs[i].depth).ToArray();

        var SB = new StringBuilder();
        for (int i = 0; i < Root.size; i++)
        {
            var at = Locs[i].place;
            var atRank = Ord[at];
            var dep = Locs[i].depth;
            if (at.Left != NIL)
            {
                var lO = Ord[at.Left];
                Branches[lO][dep] = '┌';
                for (int ii = lO + 1; ii < atRank; ii++)
                    Branches[ii][dep] = '│';
            }
            if (at.Right != NIL)
            {
                var rO = Ord[at.Right];
                Branches[rO][dep] = '└';
                for (int ii = rO - 1; ii > atRank; ii--)
                    Branches[ii][dep] = '│';
            }
        }
        for (int i = 0; i < Root.size; i++)
        {
            SB.Append(Branches[i]);
            SB.Append(Locs[i].place.val);
            SB.Append('\n');
        }
        System.Console.Write(SB.ToString());
    }

    public void FullEmit()
    {
        var Locs = InOrder();
        var Ord = new Dictionary<Node1, int>(); //place to order
        for (int i = 0; i < Root.size; i++)
            Ord.Add(Locs[i].place, i);
        var Branches = new char[Root.size][];
        for (int i = 0; i < Root.size; i++)
            Branches[i] = Enumerable.Repeat(' ', Locs[i].depth).ToArray();
        for (int i = 0; i < Root.size; i++)
        {
            var at = Locs[i].place;
            var atRank = Ord[at];
            var dep = Locs[i].depth;
            if (at.Left != NIL)
            {
                var lO = Ord[at.Left];
                Branches[lO][dep] = '┌';
                for (int ii = lO + 1; ii < atRank; ii++)
                    Branches[ii][dep] = '│';
            }
            if (at.Right != NIL)
            {
                var rO = Ord[at.Right];
                Branches[rO][dep] = '└';
                for (int ii = rO - 1; ii > atRank; ii--)
                    Branches[ii][dep] = '│';
            }
        }

        var ord = 0;
        foreach (var (p, d) in Locs)
        {
            var ordSuf = "th";
            if (ord % 10 == 1 && ord % 100 != 11) ordSuf = "st";
            if (ord % 10 == 2 && ord % 100 != 12) ordSuf = "nd";
            if (ord % 10 == 3 && ord % 100 != 13) ordSuf = "rd";
            System.Console.Write("{0,4:D}{1} -> dep:{2,4:D}, size:{3,4:D}, height:{4,4:D}, val:{5,4} ",
                ord, ordSuf, d, p.size, p.height, p.val);

            System.Console.Write(Branches[ord]);
            System.Console.Write(Locs[ord].place.val);
            System.Console.Write('\n');
            ord++;
        }

    }
    public void RotateBack(int range, int degree)
    {
        var (Base, Target) = Split(Root.size - range);
        var (TL, TR) = Split(Target, degree);
        Merge(Base, Merge(TR, TL));
    }
}
class SplayTree1<T> : BinaryTree1<T>
{
    internal Node1 ZigZigRR(Node1 at)
    // ((lllc-llc=llrc)=lc=lrc)=at-rc -> lllc-llc=(llrc=lc=(lrc=at-rc))
    {
        Node1 lc = at.Left, llc = lc.Left;
        Node1 lrc = lc.Right, llrc = llc.Right;
        llc.Right = lc;
        lc.Right = at;
        lc.Left = llrc;
        at.Left = lrc;
        //size update
        at.Fetch();
        lc.Fetch();
        llc.Fetch();
        return llc;
    }
    internal Node1 ZigZigLL(Node1 at)
    // lc-at=(rlc=rc=(rrlc=rrc=rrrc)) -> ((lc-at=rlc)=rc=rrlc)=rrc=rrrc
    {
        Node1 rc = at.Right, rrc = rc.Right;
        Node1 rlc = rc.Left, rrlc = rrc.Left;
        rrc.Left = rc;
        rc.Left = at;
        rc.Right = rrlc;
        at.Right = rlc;
        //size update
        at.Fetch();
        rc.Fetch();
        rrc.Fetch();
        return rrc;
    }

    internal void _Splay(List<(Node1 place, bool isR)> Path)
    {
        if (!Path.Any()) return;
        for (int j = Path.Count - 1; j >= 0;)
        {
            (Node1 par, bool isAtR) = Path[j];
            if (j == 0)
            {
                if (isAtR) Root = par.RotateL();
                else Root = par.RotateR();
                j--;
            }
            else
            {
                (Node1 gPar, bool isParR) = Path[j - 1];
                (Node1 ggPar, bool isGParR) = j >= 2 ? Path[j - 2] : (NIL, false);
                if (isParR)
                {
                    Node1 NPar;
                    if (isAtR) NPar = ZigZigLL(gPar);
                    else NPar = gPar.RotateRL();
                    if (ggPar == NIL) Root = NPar;
                    else if (isGParR) ggPar.Right = NPar;
                    else ggPar.Left = NPar;
                }
                else
                {
                    Node1 NPar;
                    if (isAtR) NPar = gPar.RotateLR();
                    else NPar = ZigZigRR(gPar);
                    if (ggPar == NIL) Root = NPar;
                    else if (isGParR) ggPar.Right = NPar;
                    else ggPar.Left = NPar;
                }
                j -= 2;
            }
        }
    }

    private Node1 auxFindAt(Node1 From, int i, List<(Node1 place, bool isR)> Path)
    {
        var comp = i - From.Left.size;
        switch (comp)
        {
            case < 0:
                Path.Add((From, false));
                return auxFindAt(From.Left, i, Path);
            case 0:
                return From;
            case > 0:
                Path.Add((From, true));
                return auxFindAt(From.Right, comp - 1, Path);
        }
    }
    private Node1 auxFindAt(int i, out List<(Node1 place, bool isR)> Path)
    {
        Path = new List<(Node1 place, bool isR)>();
        return auxFindAt(Root, i, Path);
    }
    public override Node1 FindAt(int i)
    {
        var at = auxFindAt(i, out var Path);
        // Splay Operation
        _Splay(Path);
        return at;

    }
}