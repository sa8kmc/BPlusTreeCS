using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
//TODO: setとしてのB+木から、vectorとしてのB+木を作る。
//TODO: Countを実装。
/*
 * 参考： 近藤嘉雪. 『定本Javaプログラマのためのアルゴリズムとデータ構造』. ソフトバンククリエイティブ, 2011.
 */
/// <summary>
/// long型の昇順に並べたB+木。
/// </summary>
/// <typeparam name="T">long型のキーに紐付けて格納するデータの型。</typeparam>
class BTree<T>
{
    /// <summary>
    /// B+木の階数。
    /// </summary>
    public static readonly int CHILD_CAPACITY = 5;
    ///内部節、葉、仮想節を総称した節のクラス
    abstract class Node
    {
        internal int serial;
        internal abstract int size { get; }
    }
    ///内部節
    class InternalNode : Node
    {
        internal int nChildren;
        internal Node?[] children;
        internal long[] lowest; //各部分木の最小の要素
        internal int height;
        internal int[] count; //Count[i]=0..(i+1)番目の部分木が持つ葉の数の合計
        ///constructor: 空の内部節を生成する
        internal InternalNode()
        {
            serial = serialNumber++;
            nChildren = 0;
            children = new Node[CHILD_CAPACITY];
            lowest = new long[CHILD_CAPACITY];
            count = new int[CHILD_CAPACITY];
            height = 2;
        }
        internal override int size { get => count[nChildren - 1]; }
        /// <summary>
        /// キーkeyをもつデータは何番目の部分木に入るかを調べる。
        /// </summary>
        /// <param name="key">調べるべきキー</param>
        /// <returns>キーkeyが何番目の部分木に入るかを返す。</returns>
        internal int LocateSubtree(long key)
        {
            //seeks for the $i s.t. lowest[i]<=key<lowest[i+1].
            //最も左の部分木の最小要素より小さいときは、最も左の部分木に挿入。
            if (CHILD_CAPACITY <= 8)
            {
                for (int i = nChildren - 1; i >= 0; i--)
                {
                    if (lowest[i] <= key)
                        return i;
                }
                return 0;
            }
            else
            {
                if (key < lowest[0])
                    return 0;
                //L:={j| lowest[j]<=key};
                //0 in L、ゆえにLは空でない。
                int a = 0, b = nChildren; //Max(L) in 0...nChildren.
                while (b - a > 1)
                {
                    var mid = (a + b) / 2;
                    if (lowest[mid] <= key) //mid in L.
                        a = mid;
                    else //mid not in L.
                        b = mid;
                }
                return a;
            }
        }
    }
    /// <summary>
    /// 葉。ここにデータが格納される。
    /// </summary>
    class Leaf : Node
    {
        internal long key;
        internal T data;
        internal override int size { get => 1; }
        /// <summary>
        /// コンスラクタ：葉を生成する
        /// </summary>
        /// <param name="key">この葉がもつキー</param>
        /// <param name="data">この葉がもつデータ</param>
        internal Leaf(long key, T data)
        {
            serial = serialNumber++;
            this.key = key;
            this.data = data;
        }
    }
    /// <summary>
    /// B+木の根。
    /// </summary>
    /// <value>nullであれば空。Leaf型であれば唯1個の要素からなる木を表す。
    /// さもなくばInternal型で、2個以上の要素を持つ木を表す。</value>
    Node? root { get; set; }
    private static int serialNumber = 0;
    private static readonly int HALF_CHILD = (CHILD_CAPACITY + 1) / 2;

    private Leaf? currentLeaf { get; set; }
    public BTree()
    {
        root = null;
    }
    public int totalHeight
    {
        get
        {
            if (root == null) return 0;
            else if (root is Leaf) return 1;
            else return (root as InternalNode)!.height;
        }
    }
    public int size { get => root?.size ?? 0; }

    private static void UpdateSizeNaive(InternalNode I, int from)
    {
        if (from == 0) I.count[from] = I.children[0]!.size;
        for (int i = Math.Max(from, 1); i < I.nChildren; i++)
            I.count[i] = I.count[i - 1] + I.children[i]!.size;
    }

    /// <summary>
    /// B木からキーkeyを探索する。キーkeyをもつ葉が見つかれば、それをcurrentLeafフィールドにセットする。
    /// このメソッドは探索の成否を示す情報だけを返す。
    /// 実際にキーkeyに対応する値を得るには、searchに成功した後でgetDataメソッドを呼び出すこと。
    /// また、setDataメソッドを呼び出せば、キーkeyに対応する値を変えることができる。
    /// </summary>
    /// <param name="key">探索すべきキー</param>
    /// <returns>キーkeyをもつ葉が見つかればtrue、見つからなければfalseを返す。</returns>
    public bool Search(long key)
    {
        currentLeaf = null;
        if (root == null) return false;
        var p = root as Node;
        while (p is InternalNode node)
            p = node.children[node.LocateSubtree(key)];
        var leaf = p as Leaf;
        if (key == leaf!.key)
        {
            currentLeaf = leaf;
            return true;
        }
        else return false;
    }
    /// <summary>
    /// 最後に成功したsearchメソッドが見つけた要素のデータを得る。
    /// </summary>
    /// <returns>直前にsearchされた要素のデータ（dataフィールド）。
    /// 直前にsearch以外（insert、delete）が実行されていた場合、および直前のsearchが失敗した場合には、nullを返す。</returns>
    public T? GetData()
    {
        if (currentLeaf == null) return default(T);
        else return currentLeaf.data;
    }
    /// <summary>
    /// 最後に成功したsearchメソッドが見つけた要素がもつデータをセットする。
    /// </summary>
    /// <param name="data">セットすべき値</param>
    /// <returns>セットに成功したらtrue、
    /// 直前にsearch以外（insert、delete）が実行されていた場合、および直前のsearchが失敗した場合にはfalseを返す。</returns>
    public bool SetData(T data)
    {
        if (currentLeaf == null)
            return false;
        else
        {
            currentLeaf.data = data;
            return true;
        }
    }
    #region Insertion

    /// <summary>
    /// InsertAuxメソッドの結果。
    /// </summary>
    /// <param name="newNode">新しい節を作った場合に、その節が入る。</param>
    /// <param name="lowest">新しい節を作った場合に、newNodeが指す部分木の最小キーが入る。</param>
    private record class InsertAuxResult(Node? newNode, long lowest);

    /// <summary>
    /// 指定した節に対して、キーkeyをもつ要素を挿入する（insertの下請け）。
    /// </summary>
    /// <param name="pnode">内部節pnodeのnth番目の子に対して挿入を行う。pnodeがnullの場合は根が対象となる。</param>
    /// <param name="nth">内部節pnodeのnth番目の子に対して挿入を行う。</param>
    /// <param name="key">挿入する要素のキー</param>
    /// <param name="data">挿入する要素のデータ</param>
    /// <returns>結果を表すInsertAuxResult型のオブジェクト。
    /// キーkeyがすでに登録済みならnull。
    /// return.newNodeは、操作により発生・伝播する特異な節を表す。
    /// 特異な節が無くなれば、return.newNode==null.</returns>
    private InsertAuxResult? InsertAux(InternalNode? pnode, int nth, long key, T data)
    {
        // 要素の挿入の対象となる節へのリンクを変数thisNodeに入れる
        var thisNode = pnode == null ? root : pnode.children[nth];
        if (thisNode is Leaf leaf) // thisNodeは葉であるか?
        {
            // すでに登録済みであれば、何もしないでnullを返す
            if (leaf.key == key) return null;
            // 新たに葉newLeafを割り当てる
            var newLeaf = new Leaf(key, data);
            // もし、割り当てた葉newLeafのほうが葉leafよりも小さいなら、
            // newLeafとleafの位置を入れ換える
            if (key < leaf.key)
            {
                // 元の節には、新しく割り当てた葉newLeafを入れる
                if (pnode == null)
                    root = newLeaf;
                else
                {
                    pnode.children[nth] = newLeaf;
                    // 改変。nth==0のとき、newLeafとleafの交換により、自身の親による調整が効かないので、ここで反映。
                    if (nth == 0)
                        pnode.lowest[0] = Math.Min(key, pnode.lowest[0]);
                }
                // 新たに割り当てた葉として、leafを報告する (特異点の発生)
                return new(leaf, leaf.key);
            }
            else
            {
                // 新たに割り当てた葉として、newLeafを報告する (特異点の発生)
                return new(newLeaf, key);
            }
        }
        else
        {
            // この節は内部節である
            // これ以降、この節を内部節nodeとして参照する
            var node = (InternalNode)thisNode!;
            // 何番目の部分木に挿入するかを決める
            int pos = node.LocateSubtree(key);
            // 部分木に対して、自分自身を再帰呼び出しする
            var result = InsertAux(node, pos, key, data);
            // 改変。部分木nodeのlowest[0]の更新をpnodeに反映。
            if (pnode != null && pos == 0)
            {
                pnode.lowest[nth] = node.lowest[0];
            }
            // もし分割が行われていなければ、そのまま戻る
            if (result == null || result.newNode == null)
            {
                //TODO: データ依存の解消
                UpdateSizeNaive(node, pos);
                return result;
            }
            // 分割が行われていたので、節nodeにそれ（result.newNode）を挿入する
            // 節nodeに追加の余地があるか？
            if (node.nChildren < CHILD_CAPACITY)
            {
                //改変。Array.Copyにより一斉にコピー。
                Array.Copy(node.children, pos + 1, node.children, pos + 2, node.nChildren - (pos + 1));
                Array.Copy(node.lowest, pos + 1, node.lowest, pos + 2, node.nChildren - (pos + 1));
                node.children[pos + 1] = result.newNode;
                node.lowest[pos + 1] = result.lowest;
                node.nChildren++;
                //TODO: データ依存の解消
                UpdateSizeNaive(node, pos);
                return new(null, -1);
            }
            else
            {
                // 追加の余地がないので、節nodeを2つに分割しなければならない
                // 新しい内部節newNodeを割り当てる
                var newNode = new InternalNode();
                newNode.height = node.height;
                // 節result.newNodeがどちらの節に挿入されるかで、場合分けする
                if (pos < HALF_CHILD - 1)
                {
                    // 節result.newNodeは、節nodeの側に挿入される
                    // まず、HALF_CHILD-1～MAX_CHILD-1番目の部分木を、節nodeから節newNodeへと移す
                    Array.Copy(node.children, HALF_CHILD - 1, newNode.children, 0, (CHILD_CAPACITY - 1) - (HALF_CHILD - 1) + 1);
                    Array.Copy(node.lowest, HALF_CHILD - 1, newNode.lowest, 0, (CHILD_CAPACITY - 1) - (HALF_CHILD - 1) + 1);
                    // 0～HALF_CHILD-2番目の部分木の間の適切な位置に、節result.newNodeを挿入する
                    Array.Copy(node.children, pos + 1, node.children, pos + 2, (HALF_CHILD - 2) - (pos + 1) + 1);
                    Array.Copy(node.lowest, pos + 1, node.lowest, pos + 2, (HALF_CHILD - 2) - (pos + 1) + 1);
                    node.children[pos + 1] = result.newNode;
                    node.lowest[pos + 1] = result.lowest;
                }
                else
                {
                    // 節result.newNodeは節newNodeの側に挿入される
                    // HALF_CHILD～MAX_CHILD-1番目の部分木を、節newNodeに移動する。同時に、節result.newNodeを適切な位置に挿入する。

                    //HALF_CHILD~pos
                    Array.Copy(node.children, HALF_CHILD, newNode.children, 0, pos - HALF_CHILD + 1);
                    Array.Copy(node.lowest, HALF_CHILD, newNode.lowest, 0, pos - HALF_CHILD + 1);
                    //result
                    newNode.children[pos - HALF_CHILD + 1] = result.newNode;
                    newNode.lowest[pos - HALF_CHILD + 1] = result.lowest;
                    //pos+1~MAX_CHILD-1
                    Array.Copy(node.children, pos + 1, newNode.children, pos - HALF_CHILD + 2, CHILD_CAPACITY - (pos + 1));
                    Array.Copy(node.lowest, pos + 1, newNode.lowest, pos - HALF_CHILD + 2, CHILD_CAPACITY - (pos + 1));
                }
                // 子の数nChildを更新する
                node.nChildren = HALF_CHILD;
                newNode.nChildren = (CHILD_CAPACITY + 1) - HALF_CHILD;
                // 分割してないですよね？
                //TODO:データ依存の解消
                UpdateSizeNaive(node, pos);
                UpdateSizeNaive(newNode, 0);

                // 分割して作られた節をフィールドnewNodeに、
                // またその最小値をlowestフィールドに返す
                return new(newNode, newNode.lowest[0]);
            }
        }
    }

    /// <summary>
    /// B木に要素を挿入する。
    /// </summary>
    /// <param name="key">挿入する要素のキー</param>
    /// <param name="data">挿入する要素のデータ</param>
    /// <returns>要素の挿入に成功したらtrue、すでにキーkeyをもつ要素が登録されていたら、何もしないでfalseを返す。</returns>
    public bool Insert(long key, T data)
    {
        currentLeaf = null;
        if (root == null)
        {
            root = new Leaf(key, data);
            return true;
        }
        else
        {
            // 木が空でない場合には、insertAuxメソッドを呼び出して、要素の挿入を行う
            var result = InsertAux(null, -1, key, data);
            // もし結果がnullなら、すでにキーkeyは登録されているので、そのままfalseを返す
            if (result == null)
                return false;

            // もし分割が行われたなら、木の高さを1段高くする
            if (result.newNode != null)
            {
                var newNode = new InternalNode();
                newNode.height = root == null ? 1 : root is Leaf ? 2 : ((InternalNode)root).height + 1;
                newNode.nChildren = 2;
                newNode.children[0] = root;
                newNode.children[1] = result.newNode;
                switch (root)
                {
                    case Leaf l:
                        newNode.lowest[0] = l.key;
                        newNode.count[0] = 1;
                        break;
                    case InternalNode i:
                        newNode.lowest[0] = i.lowest[0];
                        newNode.count[0] = i.size;
                        break;
                }
                newNode.lowest[1] = result.lowest;
                newNode.count[1] = newNode.count[0] + result.newNode.size;
                root = newNode;
            }
            return true;
        }
    }

    #endregion
    #region Deletion
    /// <summary>
    /// 内部節pのx番目とx+1番目の部分木を再編成する。
    /// もし、併合が必要なら、すべての要素をx番目の部分木に集めてtrueを返す。併合が不要ならfalseを返す。
    /// </summary>
    /// <param name="p">内部節p</param>
    /// <param name="x">内部節pのx番目とx+1番目の部分木を再編成する。</param>
    /// <returns>併合が必要ならtrue、必要でなければfalse。</returns>
    private static bool MergeNodes(InternalNode p, int x)
    {
        var a = (InternalNode)p.children[x]!;
        var b = (InternalNode)p.children[x + 1]!;
        b.lowest[0] = p.lowest[x + 1];

        var an = a.nChildren;
        var bn = b.nChildren;
        if (an + bn <= CHILD_CAPACITY)
        {
            // 部分木aとbを併合しなければならない
            // bの子をすべてaへ移動する
            Array.Copy(b.children, 0, a.children, an, bn);
            Array.Clear(b.children, 0, bn); // 不要な参照を消す
            Array.Copy(b.lowest, 0, a.lowest, an, bn);
            Array.Copy(b.count, 0, a.count, an, bn);
            // now, a.Count=={a[0..1],...,a[0..an], a[an..an+1],...,a[an..an+bn]}
            var ac = a.count[a.nChildren - 1]; //a[0...an-1]@=a[0..an]
            a.nChildren += bn; // 子の数を更新する
            // ### ここでbを解放する ###
            for (int i = an; i < an + bn; i++)
                a.count[i] += ac;
            return true; // 併合したことを通知する
        }
        else
        {
            // 部分木aとbとで、節を再分配する
            int move; // 移動する要素の個数
            // 部分木aに分配すべき子の数を求める
            int mid = (an + bn) / 2;
            if (an > mid)
            {
                // 部分木aから部分木bへと移動する
                //a[does not move], a[move to b], b[shifting higher]
                move = an - mid; // move個の子をaからbへ移す
                var former = a.count[mid - 1];//a[0..mid]
                var latter = a.count[an - 1] - former;//a[mid..an]\equiv a[..an]-a[..mid]
                // bの要素を右にずらす
                Array.Copy(b.children, 0, b.children, move, bn);
                Array.Copy(b.lowest, 0, b.lowest, move, bn);
                Array.Copy(b.count, 0, b.count, move, bn);
                // aからbへmove個の子を移動する
                Array.Copy(a.children, mid, b.children, 0, move);
                Array.Clear(a.children, mid, move); // 不要な参照を消す
                Array.Copy(a.lowest, mid, b.lowest, 0, move);
                Array.Copy(a.count, mid, b.count, 0, move);
                //b[0..move]\equiv a^{former}[mid..an]
                //now, b.Count=={a[0..mid]+b[0..1],...,a[0..mid]+b[0..move], b[move..move+1],..., b[move..move+bn]}
                for (int i = 0; i < move; i++)
                    b.count[i] -= former;
                for (int i = move; i < move + bn; i++)
                    b.count[i] += latter;
            }
            else
            {
                // 部分木bから部分木aへと移動する
                //a[does not move], b[move to a], b[shifting lower]
                move = mid - an; // move個の子をbからaへ移す
                // bからaへmove個の子を移動する
                var ac = a.count[an - 1]; //a[0..an]
                var former = b.count[move - 1]; //b[0..move]
                Array.Copy(b.children, 0, a.children, an, move);
                Array.Copy(b.lowest, 0, a.lowest, an, move);
                Array.Copy(b.count, 0, a.count, an, move);
                // bの要素を左へ詰め合わせる
                Array.Copy(b.children, move, b.children, 0, bn - move);
                Array.Clear(b.children, bn - move, move); // 不要な参照を消す
                Array.Copy(b.lowest, move, b.lowest, 0, bn - move);
                Array.Copy(b.count, move, b.count, 0, bn - move);
                // now, a.Count=={a[0..1],...,a[0..an],a[an..an+1],...,a[an,...,an+move]}, b.Count=={a[an..an+move]+b[0..1],...,a[an..an+move]+b[0..bn-move]}
                for (int i = an; i < an + move; i++)
                    a.count[i] += ac;
                for (int i = 0; i < bn - move; i++)
                    b.count[i] -= former;
            }
            // 子の個数を更新する
            a.nChildren = mid;
            b.nChildren = an + bn - mid;
            // 部分木bの最小値を節pにセットする
            p.lowest[x + 1] = b.lowest[0];
            return false;
        }
    }

    // deleteAuxメソッドの戻り値
    // 値の意味は、deleteAuxメソッドのコメントを参照のこと。
    private enum DeleteAuxResult
    {
        OK,
        OK_REMOVED,
        OK_NEED_REORG,
        NOT_FOUND
    }

    /// <summary>
    /// 節thisNodeから、キーkeyをもつ要素を削除する（deleteの下請け）。
    /// </summary>
    /// <param name="thisNode">この節（またはその部分木）から要素を削除する。</param>
    /// <param name="key">削除する要素のキー</param>
    /// <returns>以下の値を返す。
    /// ‣OK: 削除に成功。thisNodeには何の変化もない
    /// ‣OK_REMOVED: 削除に成功。thisNodeそのものが削除された
    /// ‣OK_NEED_REORG:削除に成功。thisNodeの子が少なく（HALF_CHILD以下）なったので、再編成が必要になった
    /// ‣NOT_FOUND: 削除に失敗。キーkeyをもつ子は見つからなかった
    /// </returns>
    private static DeleteAuxResult DeleteAux(Node thisNode, long key)
    {
        if (thisNode is Leaf leaf)
        {
            // この節は葉である

            // この葉のキーとkeyが等しければ、削除する
            if (leaf.key == key)
            {
                // ### ここでleafを解放する ###
                return DeleteAuxResult.OK_REMOVED;
            }
            else
            {
                // キーが一致しない。つまり、与えられたキーをもつ要素は存在しなかった
                return DeleteAuxResult.NOT_FOUND;
            }
        }
        else
        {
            // この節は内部節である
            // これ以降、この節を内部節nodeとして参照する
            var node = (InternalNode)thisNode;
            bool joined = false;// 再編成の結果、部分木が併合されたか？

            // どの部分木から削除するかを決める
            int pos = node.LocateSubtree(key);
            // その部分木に対して、自分自身を再帰呼び出しする
            DeleteAuxResult result = DeleteAux(node.children[pos]!, key);
            //この時点で、自身の子まで統合は済んでいる。
            // 改変。子のCountの更新を自身に反映。
            // 改変。子のlowestの更新を自身に反映。
            if (result == DeleteAuxResult.OK_REMOVED)
            {
                if (pos != node.nChildren - 1)
                    node.lowest[pos] = node.lowest[pos + 1];
                //else => node.lowest[pos]の値は、nChildrenの減少により意味を持つ範囲から外れる。
            }
            else if (node.children[pos] is InternalNode ic)
            {
                node.lowest[pos] = ic.lowest[0];
            }
            // 部分木に何の変化もなければ、そのまま戻る
            if (result == DeleteAuxResult.NOT_FOUND || result == DeleteAuxResult.OK)
            {
                //TODO:データ依存の解消
                UpdateSizeNaive(node, pos);
                return result;
            }

            // 部分木posを再編成する必要があるか？
            if (result == DeleteAuxResult.OK_NEED_REORG)
            {
                int sub = (pos == 0) ? 0 : pos - 1;
                // 部分木subとsub+1を再編成する
                joined = MergeNodes(node, sub);
                // もし、subとsub+1が併合されていたら、部分木sub+1をnodeから削除する必要がある
                if (joined) pos = sub + 1;
            }
            var myResult = DeleteAuxResult.OK; // このメソッドが返す戻り値。とりあえずOKにしておく
            // 部分木posが削除された
            if (result == DeleteAuxResult.OK_REMOVED || joined)
            {
                // nodeの部分木を詰め合わせる
                Array.Copy(node.children, pos + 1, node.children, pos, node.nChildren - (pos + 1));
                Array.Copy(node.lowest, pos + 1, node.lowest, pos, node.nChildren - (pos + 1));
                node.children[node.nChildren - 1] = null; // 不要な参照を消す
                // もし、nodeの部分木の数のHALF_CHILDより小さいなら再編成が必要である
                if (--node.nChildren < HALF_CHILD)
                {
                    myResult = DeleteAuxResult.OK_NEED_REORG;
                }
            }
            //TODO:データ依存の解消
            UpdateSizeNaive(node, 0);
            return myResult;
        }
    }
    /// <summary>
    /// B木から要素を削除する。
    /// </summary>
    /// <param name="key">削除する要素のキー</param>
    /// <returns>削除に成功すればtrue、要素が存在しなければfalseを返す。</returns>
    public bool Delete(long key)
    {
        currentLeaf = null;
        // 木が空ならばfalseを返す
        if (root == null)
            return false;
        else // 木が空でない場合
        {
            // deleteAuxメソッドを呼び出して、キーkeyをもつ要素を削除する
            var result = DeleteAux(root, key);

            switch (result)
            {
                case DeleteAuxResult.NOT_FOUND:    // 見つからなければ、falseを返す
                    return false;
                case DeleteAuxResult.OK_REMOVED:   // 根が削除されたので、rootにnullを代入する（木が空になる）
                    root = null;
                    break;
                case DeleteAuxResult.OK_NEED_REORG:
                    if (((InternalNode)root).nChildren == 1)
                    {
                        // 根が再編成された結果、根の子が1個だけになったら、
                        // 木の高さを1つ減らす
                        // ### Node p = root; ###
                        root = ((InternalNode)root).children[0];
                        // ### ここでpを解放する ###
                    }
                    break;
            }
            return true;
        }
    }
    #endregion
    #region ToString
    /// <summary>
    /// B木の内容を表す文字列を返す（toStringの下請け）。
    /// </summary>
    /// <param name="p">この節より下の部分を表す文字列を生成して返す。</param>
    /// <returns>節pより下の部分を表す文字列</returns>
    private static string toStringAux(Node p)
    {
        // 葉か内部節かで処理を分ける
        if (p is Leaf l)
        {
            // 葉である
            return $"Leaf #{l.serial} key={l.key}";
        }
        else
        {
            // 内部節である
            InternalNode n = (InternalNode)p;
            var s = new StringBuilder();
            s.Append($"Node #{n.serial} ({n.nChildren} children, height {n.height}): "); //n.nChildren>=2
            for (int i = 0; i < n.nChildren; i++)
            {
                s.Append($"[{n.lowest[i]}] #{n.children[i]!.serial} ");
            }
            s.Append($"\n");
            for (int i = 0; i < n.nChildren; i++)
            {
                s.Append(toStringAux(n.children[i]!) + "\n");
            }
            return s.ToString();
        }
    }

    /// <summary>
    /// B木の内容を表す文字列を返す（実際の処理はtoStringAuxメソッドが行う）。
    /// </summary>
    /// <returns>B木の内容を表す文字列</returns>
    public override string ToString()
    {
        if (root == null)
        {
            return "<木は空です>";
        }
        else
        {
            return toStringAux(root);
        }
    }
    #endregion

}
