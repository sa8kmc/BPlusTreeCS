using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
class BTree<T>
{
    ///内部節、葉、仮想節を総称した節のクラス
    abstract class Node
    {
        internal int serial;
    }
    ///内部節
    class InternalNode : Node
    {
        internal int nChildren;
        internal Node?[] children;
        internal long[] lowest; //各部分木の最小の要素
        ///constructor: 空の内部節を生成する
        internal InternalNode()
        {
            serial = serialNumber++;
            nChildren = 0;
            children = new Node[CHILD_CAPACITY];
            lowest = new long[CHILD_CAPACITY];
        }
        /**
         * キーkeyをもつデータは何番目の部分木に入るかを調べる
         *
         * @param key 調べるべきキー
         * @return キーkeyが何番目の部分木に入るかを返す
         */
        internal int LocateSubtree(long key)
        {
            //seeks for the $i s.t. lowest[i]<=key<lowest[i+1].
            //最も左の部分木の最小要素より小さいときは、最も左の部分木に挿入。
            if (key < lowest[0])
                return 0;
            //L:={j| lowest[j]<=key};
            //0\in L.
            int a = 0, b = nChildren; //Max(L)\in [0,nChildren).
            while (b - a > 1)
            {
                var mid = (a + b) / 2;
                if (lowest[mid] <= key) //mid\in L.
                    a = mid;
                else //mid\not\in L.
                    b = mid;
            }
            return a;
        }
    }
    ///葉
    class Leaf : Node
    {
        internal long key;
        internal T data;
        /**
         * コンスラクタ：葉を生成する
         *
         * @param key  この葉がもつキー
         * @param data この葉がもつデータ
         */
        internal Leaf(long key, T data)
        {
            serial = serialNumber++;
            this.key = key;
            this.data = data;
        }
    }

    Node? root { get; set; }
    private static int serialNumber = 0;
    public const int CHILD_CAPACITY = 5;
    private const int HALF_CHILD = (CHILD_CAPACITY + 1) / 2;

    private Leaf? currentLeaf { get; set; }
    public BTree()
    {
        root = null;
    }

    /**
     * B木からキーkeyを探索する。
     * キーkeyをもつ葉が見つかれば，それをcurrentLeafフィールドにセットする
     * 
     * このメソッドは探索の成否を示す情報だけを返す。
     * 実際にキーkeyに対応する値を得るには，searchに成功した後で
     * getDataメソッドを呼び出すこと。また，setDataメソッドを呼び出せば，
     * キーkeyに対応する値を変えることができる
     *
     * @param key 探索すべきキー
     * @return キーkeyをもつ葉が見つかればtrue，見つからなければfalseを返す
     */
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
    /**
     * 最後に成功したsearchメソッドが見つけた要素のデータを得る
     *
     * @return 直前にsearchされた要素のデータ（dataフィールド）。
     *         直前にsearch以外（insert，delete）が実行されていた場合，
     *         および直前のsearchが失敗した場合には，nullを返す
     */
    public T? GetData()
    {
        if (currentLeaf == null) return default(T);
        else return currentLeaf.data;
    }
    /**
     * 最後に成功したsearchメソッドが見つけた要素がもつデータをセットする
     *
     * @param data セットすべき値
     * @return セットに成功したらtrue，直前にsearch以外（insert，delete）が
     *         実行されていた場合，および直前のsearchが失敗した場合
     *         にはfalseを返す
     */
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

    /**
     * InsertAuxメソッドの結果
     * Node newNode; // 新しい節を作った場合に，その節が入る。
     * Integer lowest; // 新しい節を作った場合に，newNodeが指す部分木の最小要素が入る。
     */
    private record class InsertAuxResult(Node? newNode, long lowest);

    /**
     * 指定した節に対して，キーkeyをもつ要素を挿入する（insertの下請け）
     *
     * @param pnode 内部節pnodeのnth番目の子に対して挿入を行う。pnodeがnullの場合は根が対象となる
     * @param nth   内部節pnodeのnth番目の子に対して挿入を行う
     * @param key   挿入する要素のキー
     * @param data  挿入する要素のデータ
     *
     * @return 結果を表すInsertAuxResult型のオブジェクト。
     *         キーkeyがすでに登録済みならnull
     *         @return?.newNodeは、操作により発生・伝播する特異な節を表す。
     *         特異な節が無くなれば、@return?.newNode==null.
     */
    private InsertAuxResult? InsertAux(InternalNode? pnode, int nth, long key, T data)
    {
        // 要素の挿入の対象となる節へのリンクを変数thisNodeに入れる
        var thisNode = pnode == null ? root : pnode.children[nth];
        if (thisNode is Leaf leaf) // thisNodeは葉であるか?
        {
            // すでに登録済みであれば，何もしないでnullを返す
            if (leaf.key == key) return null;
            // 新たに葉newLeafを割り当てる
            var newLeaf = new Leaf(key, data);
            // もし，割り当てた葉newLeafのほうが葉leafよりも小さいなら，
            // newLeafとleafの位置を入れ換える
            if (key < leaf.key)
            {
                // 元の節には，新しく割り当てた葉newLeafを入れる
                if (pnode == null)
                    root = newLeaf;
                else
                {
                    pnode.children[nth] = newLeaf;
                    if (nth == 0)
                        pnode.lowest[0] = Math.Min(key, pnode.lowest[0]);
                }
                // 新たに割り当てた葉として，leafを報告する (特異点の発生)
                return new(leaf, leaf.key);
            }
            else
            {
                // 新たに割り当てた葉として，newLeafを報告する (特異点の発生)
                return new(newLeaf, key);
            }
        }
        else
        {
            // この節は内部節である
            // これ以降，この節を内部節nodeとして参照する
            var node = (InternalNode)thisNode!;
            // 何番目の部分木に挿入するかを決める
            int pos = node.LocateSubtree(key);
            // 部分木に対して，自分自身を再帰呼び出しする
            var result = InsertAux(node, pos, key, data);
            // 部分木node(==pnode.children[nth])のlowest[0]の更新をpnodeにも反映させる。
            if (pnode != null && pos == 0)
                pnode.lowest[nth] = node.lowest[0];
            // もし分割が行われていなければ，そのまま戻る
            if (result == null || result.newNode == null)
                return result;
            // 分割が行われていたので，節nodeにそれ（result.newNode）を挿入する
            // 節nodeに追加の余地があるか？
            if (node.nChildren < CHILD_CAPACITY)
            {
                Array.Copy(node.children, pos + 1, node.children, pos + 2, (node.nChildren - 1) - (pos + 1) + 1);
                Array.Copy(node.lowest, pos + 1, node.lowest, pos + 2, (node.nChildren - 1) - (pos + 1) + 1);
                node.children[pos + 1] = result.newNode;
                node.lowest[pos + 1] = result.lowest;
                node.nChildren++;
                return new(null, -1);
            }
            else
            {
                // 追加の余地がないので，節nodeを2つに分割しなければならない
                // 新しい内部節newNodeを割り当てる
                var newNode = new InternalNode();
                // 節result.newNodeがどちらの節に挿入されるかで，場合分けする
                if (pos < HALF_CHILD - 1)
                {
                    // 節result.newNodeは，節nodeの側に挿入される
                    // まず，HALF_CHILD-1～MAX_CHILD-1番目の部分木を，
                    // 節nodeから節newNodeへと移す
                    Array.Copy(node.children, HALF_CHILD - 1, newNode.children, 0,
                        (CHILD_CAPACITY - 1) - (HALF_CHILD - 1) + 1);
                    Array.Copy(node.lowest, HALF_CHILD - 1, newNode.lowest, 0,
                        (CHILD_CAPACITY - 1) - (HALF_CHILD - 1) + 1);
                    // 0～HALF_CHILD-2番目の部分木の間の適切な位置に，
                    // 節result.newNodeを挿入する
                    Array.Copy(node.children, pos + 1, node.children, pos + 2, (HALF_CHILD - 2) - (pos + 1) + 1);
                    Array.Copy(node.lowest, pos + 1, node.lowest, pos + 2, (HALF_CHILD - 2) - (pos + 1) + 1);
                    node.children[pos + 1] = result.newNode;
                    node.lowest[pos + 1] = result.lowest;
                }
                else
                {
                    // 節result.newNodeは節newNodeの側に挿入されるHALF_CHILD～MAX_CHILD-1番目の部分木を，
                    // 節newNodeに移動する。同時に，節result.newNodeを適切な位置に挿入する。

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

                // 分割して作られた節をフィールドnewNodeに，
                // またその最小値をlowestフィールドに返す
                return new(newNode, newNode.lowest[0]);
            }
        }
    }

    /**
     * B木に要素を挿入する
     *
     * @param key  挿入する要素のキー
     * @param data 挿入する要素のデータ
     * @return 要素の挿入に成功したらtrue，すでにキーkeyをもつ要素が登録されていたら，何もしないでfalseを返す
     */
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
            // 木が空でない場合には，insertAuxメソッドを呼び出して，要素の挿入を行う
            var result = InsertAux(null, -1, key, data);
            // もし結果がnullなら，すでにキーkeyは登録されているので，そのままfalseを返す
            if (result == null)
                return false;

            // もし分割が行われたなら，木の高さを1段高くする
            if (result.newNode != null)
            {
                var newNode = new InternalNode();
                newNode.nChildren = 2;
                newNode.children[0] = root;
                newNode.children[1] = result.newNode;
                switch (root)
                {
                    case Leaf l: newNode.lowest[0] = l.key; break;
                    case InternalNode i: newNode.lowest[0] = i.lowest[0]; break;
                }
                newNode.lowest[1] = result.lowest;
                root = newNode;
            }
            return true;
        }
    }

    #endregion
    #region Deletion
    /**
     * 内部節pのx番目とx+1番目の部分木を再編成する。
     * もし，併合が必要なら，すべての要素をx番目の部分木に集めてtrueを返す。併合が不要ならfalseを返す
     *
     * @param p 内部節p
     * @param x 内部節pのx番目とx+1番目の部分木を再編成する
     * @return 併合が必要ならtrue，必要でなければfalse
     */
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
            a.nChildren += bn; // 子の数を更新する
            // ### ここでbを解放する ###
            return true; // 併合したことを通知する
        }
        else
        {
            // 部分木aとbとで，節を再分配する
            int move; // 移動する要素の個数
            // 部分木aに分配すべき子の数を求める
            int mid = (an + bn) / 2;
            if (an > mid)
            {
                // 部分木aから部分木bへと移動する
                move = an - mid; // move個の子をaからbへ移す
                // bの要素を右にずらす
                Array.Copy(b.children, 0, b.children, move, bn);
                Array.Copy(b.lowest, 0, b.lowest, move, bn);
                // aからbへmove個の子を移動する
                Array.Copy(a.children, mid, b.children, 0, move);
                Array.Clear(a.children, mid, move); // 不要な参照を消す
                Array.Copy(a.lowest, mid, b.lowest, 0, move);
            }
            else
            {
                // 部分木bから部分木aへと移動する
                move = mid - an; // move個の子をbからaへ移す
                // bからaへmove個の子を移動する
                Array.Copy(b.children, 0, a.children, an, move);
                Array.Copy(b.lowest, 0, a.lowest, an, move);
                // bの要素を左へ詰め合わせる
                Array.Copy(b.children, move, b.children, 0, bn - move);
                Array.Clear(b.children, bn - move, move); // 不要な参照を消す
                Array.Copy(b.lowest, move, b.lowest, 0, move);
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
    // 値の意味は，deleteAuxメソッドのコメントを参照のこと。
    private enum Deletion
    {
        OK,
        OK_REMOVED,
        OK_NEED_REORG,
        NOT_FOUND
    }

    /**
     * 節thisNodeから，キーkeyをもつ要素を削除する（deleteの下請け）
     *
     * @param thisNode この節（またはその部分木）から要素を削除する
     * @param key      削除する要素のキー
     * @return 以下の値を返す。
     *         OK: 削除に成功。thisNodeには何の変化もない
     *         OK_REMOVED: 削除に成功。thisNodeそのものが削除された
     *         OK_NEED_REORG:削除に成功。thisNodeの子が少なく（HALF_CHILD以下）なったので，再編成が必要になった
     *         NOT_FOUND: 削除に失敗。キーkeyをもつ子は見つからなかった
     */
    private static Deletion DeleteAux(Node thisNode, long key)
    {
        if (thisNode is Leaf leaf)
        {
            // この節は葉である

            // この葉のキーとkeyが等しければ，削除する
            if (leaf.key == key)
            {
                // ### ここでleafを解放する ###
                return Deletion.OK_REMOVED;
            }
            else
            {
                // キーが一致しない。つまり，与えられたキーをもつ要素は存在しなかった
                return Deletion.NOT_FOUND;
            }
        }
        else
        {
            // この節は内部節である
            // これ以降，この節を内部節nodeとして参照する
            var node = (InternalNode)thisNode;
            bool joined = false;// 再編成の結果，部分木が併合されたか？

            // どの部分木から削除するかを決める
            int pos = node.LocateSubtree(key);
            // その部分木に対して，自分自身を再帰呼び出しする
            Deletion result = DeleteAux(node.children[pos]!, key);
            // 部分木に何の変化もなければ，そのまま戻る
            if (result == Deletion.NOT_FOUND || result == Deletion.OK)
                return result;

            // 部分木posを再編成する必要があるか？
            if (result == Deletion.OK_NEED_REORG)
            {
                int sub = (pos == 0) ? 0 : pos - 1;
                // 部分木subとsub+1を再編成する
                joined = MergeNodes(node, sub);
                // もし，subとsub+1が併合されていたら，部分木sub+1をnodeから削除する必要がある
                if (joined) pos = sub + 1;
            }

            var myResult = Deletion.OK; // このメソッドが返す戻り値。とりあえずOKにしておく
            // 部分木posが削除された
            if (result == Deletion.OK_REMOVED || joined)
            {
                // nodeの部分木を詰め合わせる
                for (int i = pos; i < node.nChildren - 1; i++)
                {
                    node.children[i] = node.children[i + 1];
                    node.lowest[i] = node.lowest[i + 1];
                }
                node.children[node.nChildren - 1] = null; // 不要な参照を消す
                // もし，nodeの部分木の数のHALF_CHILDより小さいなら再編成が必要である
                if (--node.nChildren < HALF_CHILD)
                {
                    myResult = Deletion.OK_NEED_REORG;
                }
            }
            return myResult;
        }
    }
    /**
     * B木から要素を削除する
     *
     * @param key 削除する要素のキー
     * @return 削除に成功すればtrue，要素が存在しなければfalseを返す
     */
    public bool Delete(long key)
    {
        currentLeaf = null;
        // 木が空ならばfalseを返す
        if (root == null)
            return false;
        else // 木が空でない場合
        {
            // deleteAuxメソッドを呼び出して，キーkeyをもつ要素を削除する
            var result = DeleteAux(root, key);

            switch (result)
            {
                case Deletion.NOT_FOUND:    // 見つからなければ，falseを返す
                    return false;
                case Deletion.OK_REMOVED:   // 根が削除されたので，rootにnullを代入する（木が空になる）
                    root = null;
                    break;
                case Deletion.OK_NEED_REORG:
                    if (((InternalNode)root).nChildren == 1)
                    {
                        // 根が再編成された結果，根の子が1個だけになったら，
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
    /**
     * B木の内容を表す文字列を返す（toStringの下請け）
     *
     * @param p この節より下の部分を表す文字列を生成して返す
     * @return 節pより下の部分を表す文字列
     */
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
            s.Append($"Node #{n.serial} ({n.nChildren} children): "); //n.nChildren>=2
            for (int i = 0; i < n.nChildren; i++)
            {
                s.Append($"[{n.lowest[i]}] #{n.children[i]!.serial} ");
            }
            s.Append("\n");
            for (int i = 0; i < n.nChildren; i++)
            {
                s.Append(toStringAux(n.children[i]!) + "\n");
            }
            return s.ToString();
        }
    }

    /**
     * B木の内容を表す文字列を返す
     * （実際の処理はtoStringAuxメソッドが行う）
     *
     * @return B木の内容を表す文字列
     */
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