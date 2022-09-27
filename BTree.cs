/*
 * 参考： 近藤嘉雪. 『定本Javaプログラマのためのアルゴリズムとデータ構造』. ソフトバンククリエイティブ, 2011.
 */
/// <summary>
/// B+木による列の実装。平衡性により任意箇所への挿入削除検索がO(log n)ででき、
/// 多分木性により参照の局所性を考慮した効率的なデータ処理が可能となっている(定数倍の改善)。
/// </summary>
/// <typeparam name="T">格納するデータの型</typeparam>
class BTree<T>
{
    #region MetaComment
    // コード内のコメントに用いる範囲記法
    //  a[i..j]={a[i],...,a[j-1]}
    //  a[i...j]={a[i],...,a[j]}
    //  a[i:j]={a[i],...,a[i+j-1]}
    // 以下誤解なき場合、節とその節が根である部分木とを同一視する。
    #endregion
    /// <summary>
    /// B+木の階数
    /// </summary>
    public static readonly int CAPACITY = 128;
    #region NodeClassDefinition
    /// <summary>
    /// 内部節、葉、仮想節を総称した節の抽象クラス
    /// </summary>
    internal abstract class Node
    {
#if DEBUG
        internal long serial;
#endif
        internal abstract int size { get; }
        internal int height { get; set; }
    }
    /// <summary>
    /// 内部節のクラス
    /// </summary>
    internal class InternalNode : Node
    {
        internal int nc;
        internal Node?[] children;
        internal int[] count; //Count[i]=0..(i+1)番目の部分木が持つ葉の数の合計
        ///constructor: 空の内部節を生成する
        internal InternalNode()
        {
#if DEBUG
            serial = serialNumber++;
#endif
            nc = 0;
            children = new Node?[CAPACITY];
            count = new int[CAPACITY];
            height = 2;
        }
        /// <summary>
        /// Aの各要素を子とした内部節を生成する。
        /// </summary>
        /// <param name="heightA">Aの各要素の高さ。returnsのよりちょうど1低い。</param>
        /// <param name="A">子の節の配列。</param>
        internal InternalNode(int heightA, Node?[] A)
        {
#if DEBUG
            serial = serialNumber++;
#endif
            children = new Node?[CAPACITY];
            count = new int[CAPACITY];
            this.height = heightA + 1;
            nc = A.Length;
            Array.Copy(A, children, A.Length);
            count[0] = A[0]?.size ?? 0;
            for (int i = 1; i < A.Length; i++)
                count[i] = count[i - 1] + (A[i]?.size ?? 0);
        }
        /// <summary>
        /// A[start..(start+len)]の各要素を子とした内部節を生成する。
        /// </summary>
        /// <param name="heightA"></param>
        /// <param name="A"></param>
        /// <param name="start">Aからの抽出の開始位置。</param>
        /// <param name="len">Aから抽出する要素の個数。</param>
        internal InternalNode(int heightA, Node?[] A, int start, int len)
        {
#if DEBUG
            serial = serialNumber++;
#endif
            children = new Node?[CAPACITY];
            count = new int[CAPACITY];
            this.height = heightA + 1;
            nc = len;
            Array.Copy(A, start, children, 0, len);
            count[0] = A[start]?.size ?? 0;
            for (int i = 1; i < len; i++)
                count[i] = count[i - 1] + (A[start + i]?.size ?? 0);
        }
        /// <summary>
        /// カウントがa prioriに分かっている場合のコンストラクタ。
        /// </summary>
        /// <param name="heightA"></param>
        /// <param name="A"></param>
        /// <param name="C">Aの各要素のサイズの累積和。C[0]==A[0].sizeに注意。</param>
        /// <param name="start"></param>
        /// <param name="len"></param>
        internal InternalNode(int heightA, Node?[] A, int[] C, int start, int len)
        {
#if DEBUG
            serial = serialNumber++;
#endif
            children = new Node?[CAPACITY];
            count = new int[CAPACITY];
            this.height = heightA + 1;
            nc = len;
            Array.Copy(A, start, children, 0, len);
            Array.Copy(C, start, count, 0, len);
            if (start != 0)
            {
                //Cは、this.countにとってはA[0..start]を余計にカウントしているので、
                //this.countからC[start-1]==Sum(A[0..start].Select(x=>x.size))を減算。
                var frontOver = count[start - 1];
                for (int i = 0; i < len; i++)
                    count[i] -= frontOver;
            }
        }

        internal override int size { get => count[nc - 1]; }
        public Node? this[int i]
        {
            get => this.children[i];
            set => this.children[i] = value;
        }
        /// <summary>
        /// x番目の挿入箇所が何番目の部分木に入るかを調べる。
        /// </summary>
        /// <param name="x">調べるべき番号</param>
        /// <returns>x番目の挿入箇所が何番目の部分木に入るかを返す。</returns>
        internal int LocateSubtreeAt(int x, out int remain)
        {
#if DEBUG
            if (x < 0 || x > size) throw new IndexOutOfRangeException();
#endif
            //seeks for the $i s.t. count[i-1]<=x<count[i]
            //implicit 0 at count[-1]に注意。
            if (CAPACITY <= 8)
            {
                for (int i = nc - 1; i >= 1; i--)
                {
                    if (count[i - 1] <= x)
                    {
                        remain = x - count[i - 1];
                        return i;
                    }
                }
                remain = x;
                return 0;
            }
            else
            {
                int i = -1, j = nc - 1; // (i,j]
                while (j - i > 1)
                {
                    var mid = (i + j) / 2; //since j-i>=2, i<mid<j.
                    if (count[mid] <= x)
                        i = mid;
                    else
                        j = mid;
                }
                remain = j == 0 ? x : x - count[j - 1];
                return j;
            }
        }
        internal int GetC0(int i) => i == 0 ? count[0] : count[i] - count[i - 1];
        /// <summary>
        /// X.Take(N).Select((s,i)=>i==at?UnderlineLiteral+X[at].ToString()+UnderlineRemovingLiteral:s.ToString());
        /// Xの先頭N要素のToString()を取ってX[at].ToString()を下線を引くエスケープシーケンスで囲んだ文字列の列を返す。
        /// </summary>
        /// <param name="X">データの配列</param>
        /// <param name="at">下線を引く要素の番号</param>
        /// <param name="N">先頭要素の数</param>
        /// <typeparam name="T">データの型。ToString()を実装していること。</typeparam>
        /// <returns></returns>
        internal IEnumerable<string> Highlight(int at)
            => count[0..at].Select(s => s.ToString())
                .Append($"\x1b[4m{count[at]}\x1b[0m")
                .Concat(count[(at + 1)..nc].Select(s => s.ToString()));
    }
    /// <summary>
    /// 葉。ここにデータが格納される。
    /// </summary>
    internal class Leaf : Node
    {
        internal T data;
        internal override int size { get => 1; }
        /// <summary>
        /// コンスラクタ：葉を生成する
        /// </summary>
        /// <param name="data">この葉がもつデータ</param>
        internal Leaf(T data)
        {
            height = 1;
#if DEBUG
            serial = serialNumber++;
#endif
            this.data = data;
        }
    }

    #endregion
    /// <summary>
    /// B+木の根。
    /// </summary>
    /// <value>nullであれば空。Leaf型であれば唯1個の要素からなる木を表す。
    /// さもなくばInternal型で、2個以上の要素を持つ木を表す。</value>
    #region PropertiesAndFields
    internal Node? root { get; private set; }
#if DEBUG
    private static long serialNumber = 0;
#endif
    /// <summary>
    /// 内部節が持つ子節数の下限
    /// </summary>
    private static readonly int HALF_CAPACITY = (CAPACITY + 1) / 2;
    /// <summary>
    /// 探索により指定された節を参照するフィールド
    /// </summary>
    /// <value></value>
    public Leaf? currentLeaf { get; private set; }
    /// <summary>
    /// 木全体の高さ
    /// </summary>
    public int totalHeight
    {
        get
        {
            if (root == null) return 0;
            else if (root is Leaf) return 1;
            else return (root as InternalNode)!.height;
        }
    }
    /// <summary>
    /// 木全体のデータ数(=葉の数)
    /// </summary>
    /// <value></value>
    public int size { get => root?.size ?? 0; }
    #endregion
    #region ConstructorAndBuilder
    /// <summary>
    /// 空のB+木。
    /// </summary>
    public BTree()
    {
        root = null;
    }
    /// <summary>
    /// 配列から生成されるB+木。
    /// </summary>
    /// <param name="A">データの配列</param>
    public BTree(T[] A)
    {
        root = BuildFrom(A);
    }
    /// <summary>
    /// ある節を根とするB+木。
    /// </summary>
    /// <param name="X">根</param>
    public BTree(Node? X)
    {
        root = X;
    }
    internal Node? BuildFrom(T[] A)
    {
        if (A.Length == 0) return null;
        else if (A.Length == 1)
            return new Leaf(A[0]);
        Node[] Level0 = new Leaf[A.Length];
        for (int i = 0; i < A.Length; i++)
            Level0[i] = new Leaf(A[i]);
        InternalNode[] Level;
        var height = 1;
        while (Level0.Length > 1)
        {
            var LN = (Level0.Length + CAPACITY - 1) / CAPACITY;
            Level = new InternalNode[LN];
            //ノード達の親を作成
            if (LN == 1)
                Level[0] = new InternalNode(height, Level0);
            else
            {
                for (int j = 0; j < LN - 2; j++)
                    Level[j] = new InternalNode(height, Level0, CAPACITY * j, CAPACITY);
                //Last 2 parents
                var remain = CAPACITY + ((Level0.Length - 1) % CAPACITY + 1);
                var leftSize = (remain + 1) / 2;
                Level[^2] = new InternalNode(height, Level0, CAPACITY * (LN - 2), leftSize);
                Level[^1] = new InternalNode(height, Level0, CAPACITY * (LN - 2) + leftSize, remain - leftSize);
            }
            Level0 = Level;
            height++;
        }
        return Level0[0];
    }

    #endregion
    #region Indexer
    /// <summary>
    /// B+木から番号iの葉を探索する。番号iの葉が見つかれば、それをcurrentLeafフィールドにセットする。
    /// このメソッド自体は探索の成否を示すbool型情報だけを返す。
    /// 実際に番号iに対応する値を得るには、searchに成功した後でgetDataメソッドを呼び出すこと。
    /// また、setDataメソッドにより、番号iに対応する値を変えることができる。
    /// </summary>
    /// <param name="i">探索すべき番号</param>
    /// <returns>番号iの葉が見つかればtrue、見つからなければfalseを返す。</returns>
    public bool SearchAt(int i)
    {
        currentLeaf = null;
        if (root == null) return false;
        var p = root as Node;
        while (p is InternalNode node)
            p = node[node.LocateSubtreeAt(i, out i)];
        currentLeaf = p as Leaf;
        return true;
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
    /// 最後に成功したsearchメソッドが見つけた要素のデータを更新する。
    /// </summary>
    /// <param name="data">更新すべき値</param>
    /// <returns>更新に成功したらtrue、
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
    #endregion
    #region AuxiliaryModifications
    /// <summary>
    /// xが正のときはBの先頭x個をAの末尾に移動、負のときはその逆、0のときは何もしない。
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="x"></param>
    private static void Shift(InternalNode A, InternalNode B, int x)
    {
        var an = A.nc;
        var bn = B.nc;
        if (x > 0)
        {
            // 部分木bから部分木aへと移動する
            //a[does not move], b[move to a], b[shifting lower]
            var lc = A.count[an - 1];//a[0..an]
            var mc = B.count[x - 1];//b[0..x]
                                    // bからaへx個の子を移動する
            Array.Copy(B.children, 0, A.children, an, x);
            Array.Copy(B.count, 0, A.count, an, x);
            // bの要素を左へ詰め合わせる
            Array.Copy(B.children, x, B.children, 0, bn - x);
            Array.Copy(B.count, x, B.count, 0, bn - x);
            Array.Clear(B.children, bn - x, x);
            // now, a.Count=={a[0..1],...,a[0..an],a[an..an+1],...,a[an..an+move]}, b.Count=={a[an..an+move]+b[0..1],...,a[an..an+move]+b[0..bn-move]}
            for (int i = an; i < an + x; i++)
                A.count[i] += lc;
            for (int i = 0; i < bn - x; i++)
                B.count[i] -= mc;
            A.nc += x;
            B.nc -= x;
        }
        else if (x < 0)
        {
            x = -x;
            // 部分木aから部分木bへと移動する
            //a[does not move], a[move to b], b[shifting higher]
            var lc = A.count[an - 1 - x];//a[0..an-x]
            var mc = A.count[an - 1] - lc;//a[an-x..an]\equiv a[..an]-a[..an-x]
                                          // bの要素を右にずらす
            Array.Copy(B.children, 0, B.children, x, bn);
            Array.Copy(B.count, 0, B.count, x, bn);
            // aからbへx個の子を移動する
            Array.Copy(A.children, an - x, B.children, 0, x);
            Array.Copy(A.count, an - x, B.count, 0, x);
            Array.Clear(A.children, an - x, x);
            //b[0..move]\equiv a^{former}[mid..an]
            //now, b.Count=={a[0..mid]+b[0..1],...,a[0..mid]+b[0..move], b[move..move+1],..., b[move..move+bn]}
            for (int i = 0; i < x; i++)
                B.count[i] -= lc;
            for (int i = x; i < bn + x; i++)
                B.count[i] += mc;
            A.nc -= x;
            B.nc += x;
        }
    }
    /// <summary>
    /// I.count[from..I.nc]を、各要素のsizeを確認することによって更新する。
    /// </summary>
    /// <param name="I">内部節。</param>
    /// <param name="from">countの更新を始める番号。</param>
    private static void UpdateSizeNaive(InternalNode I, int from)
    {
        if (from == 0) I.count[from] = I[0]!.size;
        for (int i = Math.Max(from, 1); i < I.nc; i++)
            I.count[i] = I.count[i - 1] + I[i]!.size;
    }
    #endregion
    #region Insertion
    /// <summary>
    /// 部分木pnode[nth]のkey番目に要素dataを挿入する（insertの下請け）。
    /// </summary>
    /// <param name="pnode">内部節pnodeのnth番目の子に対して挿入を行う。pnodeがnullの場合はrootが対象となる。</param>
    /// <param name="nth">内部節pnodeのnth番目の子に対して挿入を行う。</param>
    /// <param name="key">挿入する要素の番号</param>
    /// <param name="data">挿入する要素のデータ</param>
    /// <returns>結果を表すInsertAuxResult型のオブジェクト。
    /// 番号iがすでに登録済みならnull。
    /// return.newNodeは、操作により発生・伝播する特異な節を表す。
    /// 特異な節が無くなれば、return.newNode==null.</returns>
    private Node? InsertAux(InternalNode? pnode, int nth, int key, T data)
    {
        // 要素の挿入の対象となる節へのリンクを変数thisNodeに入れる
        var thisNode = pnode == null ? root : pnode[nth];
        if (thisNode is Leaf leaf) // thisNodeは葉であるか?
        {
            // 新たに葉newLeafを割り当てる
            var newLeaf = new Leaf(data);
            // もし、割り当てた葉newLeafのほうが葉leafよりも小さいなら、
            // newLeafとleafの位置を入れ換える
            {
                // 元の節には、新しく割り当てた葉newLeafを入れる
                if (pnode == null)
                    root = newLeaf;
                else
                {
                    pnode[nth] = newLeaf;
                }
                // 新たに割り当てた葉として、leafを報告する (特異点の発生)
                return leaf;
            }
        }
        else
        {
            // この節は内部節である
            // これ以降、この節を内部節nodeとして参照する
            var node = (InternalNode)thisNode!;
            // 何番目の部分木に挿入するかを決める
            int pos = node.LocateSubtreeAt(key, out var subkey);
            // 部分木に対して、自分自身を再帰呼び出しする
            var result = InsertAux(node, pos, subkey, data);
            // もし分割が行われていなければ、そのまま戻る
            if (result == null)
            {
                //countをアップデート
                var c0 = node.GetC0(pos);
                var dn = node[pos]!.size - c0;
                for (int i = pos; i < node.nc; i++)
                    node.count[i] += dn;
                return result;
            }
            // 分割が行われていたので、節nodeの、posの直後にそれ（result.newNode）を挿入する
            // 節nodeに追加の余地があるか？
            if (node.nc < CAPACITY)
            {
                //pos+1 -thを開けるように右シフト
                //改変。Array.Copyにより一斉にコピー。
                var c0 = node.GetC0(pos);
                var nl = node[pos]!.size;
                var nr = result.size;
                var dn = (nl + nr) - c0;
                // Console.Write($"\t{c0} -> {nl}, {nr}");
                Array.Copy(node.children, pos + 1, node.children, pos + 2, node.nc - (pos + 1));
                Array.Copy(node.count, pos + 1, node.count, pos + 2, node.nc - (pos + 1));
                node[pos + 1] = result;
                node.count[pos] += nl - c0;
                node.count[pos + 1] = node.count[pos] + nr;
                node.nc++;
                for (int i = pos + 2; i < node.nc; i++)
                    node.count[i] += dn;
                return null;
            }
            else
            {
                // 追加の余地がないので、節nodeを2つに分割しなければならない
                // 新しい内部節newNodeを割り当てる
                var newNode = new InternalNode();
                newNode.height = node.height;
                //カウントチェック
                var c0 = node.GetC0(pos);
                var nl = node[pos]!.size;
                var nr = result.size;
                var dn = (nl + nr) - c0;
                // 節resultがどちらの節に挿入されるかで、場合分けする
                if (pos < HALF_CAPACITY - 1)
                {
                    // 節resultは、節nodeの側に挿入される
                    // まず、HALF_CHILD-1～MAX_CHILD-1番目の部分木を、節nodeから節newNodeへと移す
                    Array.Copy(node.children, HALF_CAPACITY - 1, newNode.children, 0, (CAPACITY - 1) - (HALF_CAPACITY - 1) + 1);
                    // 0～HALF_CHILD-2番目の部分木の間の適切な位置に、節resultを挿入する
                    Array.Copy(node.children, pos + 1, node.children, pos + 2, (HALF_CAPACITY - 2) - (pos + 1) + 1);
                    node[pos + 1] = result;
                }
                else
                {
                    // 節resultは節newNodeの側に挿入される
                    // HALF_CHILD～MAX_CHILD-1番目の部分木を、節newNodeに移動する。同時に、節resultを適切な位置に挿入する。

                    //HALF_CHILD~pos
                    Array.Copy(node.children, HALF_CAPACITY, newNode.children, 0, pos - HALF_CAPACITY + 1);
                    //result
                    newNode[pos - HALF_CAPACITY + 1] = result;
                    //pos+1~MAX_CHILD-1
                    Array.Copy(node.children, pos + 1, newNode.children, pos - HALF_CAPACITY + 2, CAPACITY - (pos + 1));
                }
                // 子の数nChildを更新する
                node.nc = HALF_CAPACITY;
                newNode.nc = (CAPACITY + 1) - HALF_CAPACITY;
                // カウントを更新
                var tmpCount = new int[CAPACITY + 1];
                Array.Copy(node.count, 0, tmpCount, 0, pos + 1);
                //1個ずつの挿入が前提なので、node.countが分割される場合、メソッドに入る前のcountは^1まで意味を持つ。
                Array.Copy(node.count, pos + 1, tmpCount, pos + 2, CAPACITY - (pos + 1));
                tmpCount[pos] += nl - c0;
                if (pos + 1 <= CAPACITY) tmpCount[pos + 1] = tmpCount[pos] + nr;
                for (int i = pos + 2; i < CAPACITY + 1; i++)
                    tmpCount[i] += dn;
                nl = tmpCount[HALF_CAPACITY - 1]; //左子全体のカウント
                for (int i = HALF_CAPACITY; i < CAPACITY + 1; i++)
                    tmpCount[i] -= nl;
                Array.Copy(tmpCount, 0, node.count, 0, HALF_CAPACITY);
                Array.Copy(tmpCount, HALF_CAPACITY, newNode.count, 0, (CAPACITY + 1) - HALF_CAPACITY);
                // 分割して作られた節をフィールドnewNodeに、
                // またその最小値をlowestフィールドに返す
                return newNode;
            }
        }
    }
    /// <summary>
    /// B木に要素を挿入する。
    /// </summary>
    /// <param name="at">挿入場所の番号</param>
    /// <param name="data">挿入する要素のデータ</param>
    /// <returns>要素の挿入に成功したらtrue、iがout of rangeであるときは何もしないでfalseを返す。</returns>
    public bool InsertAt(int at, T data)
    {
        currentLeaf = null;
        if (root == null)
        {
            root = new Leaf(data);
            return true;
        }
        else
        {
            // 木が空でない場合には、insertAuxメソッドを呼び出して、要素の挿入を行う
            var result = InsertAux(null, -1, at, data);
            // もし結果がnullなら、すでに番号iは登録されているので、そのままfalseを返す
            if (result == null)
                return false;

            // もし分割が行われたなら、木の高さを1段高くする
            else
            {
                var newNode = new InternalNode();
                newNode.height = root == null ? 1 : root is Leaf ? 2 : ((InternalNode)root).height + 1;
                newNode.nc = 2;
                newNode[0] = root;
                newNode[1] = result;
                switch (root)
                {
                    case Leaf:
                        newNode.count[0] = 1;
                        break;
                    case InternalNode ino:
                        newNode.count[0] = ino.size;
                        break;
                }
                newNode.count[1] = newNode.count[0] + result.size;
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
    private static bool LesserMerge(InternalNode p, int x)
    {
        var a = (InternalNode)p[x]!;
        var b = (InternalNode)p[x + 1]!;

        var an = a.nc;
        var bn = b.nc;
        if (an + bn <= CAPACITY)
        {
            // 部分木aとbを併合しなければならない
            // bの子をすべてaへ移動する
            Shift(a, b, bn);
            return true; // 併合したことを通知する
        }
        else
        {
            // 部分木aとbとで、節を再分配する
            // 部分木aに分配すべき子の数を求める
            int mid = (an + bn) / 2;
            Shift(a, b, mid - an);
            return false;
        }
    }
    /// <summary>
    /// DeleteAuxメソッドの返り値。 値の意味は、DeleteAuxメソッドのコメントを参照。
    /// </summary>
    private enum DeleteAuxResult
    {
        OK,
        OK_REMOVED,
        OK_NEED_REORG,
        NOT_FOUND
    }
    /// <summary>
    /// 部分木thisNodeから、番号iをもつ要素を削除する（Deleteの下請け）。
    /// </summary>
    /// <param name="thisNode">この節（またはその部分木）から要素を削除する。</param>
    /// <param name="key">削除する要素のキー</param>
    /// <returns>以下の値を返す。
    /// ‣OK: 削除に成功。thisNodeには何の変化もない
    /// ‣OK_REMOVED: 削除に成功。thisNodeそのものが削除された
    /// ‣OK_NEED_REORG:削除に成功。thisNodeの子が少なく（HALF_CHILD以下）なったので、再編成が必要になった
    /// ‣NOT_FOUND: 削除に失敗。番号iをもつ子は見つからなかった
    /// </returns>
    private DeleteAuxResult DeleteAux(Node thisNode, int key)
    {
        if (thisNode is Leaf leaf)
        {
            // この節は葉である
            currentLeaf = leaf;
            // この葉のキーとkeyが等しければ、削除する
            return DeleteAuxResult.OK_REMOVED;
        }
        else
        {
            // この節は内部節である
            // これ以降、この節を内部節nodeとして参照する
            var node = (InternalNode)thisNode;
            bool joined = false;// 再編成の結果、部分木が併合されたか？

            // どの部分木から削除するかを決める
            int pos = node.LocateSubtreeAt(key, out var subkey);
            // その部分木に対して、自分自身を再帰呼び出しする
            DeleteAuxResult result = DeleteAux(node[pos]!, subkey);
            //この時点で、自身の子まで統合は済んでいる。
            // 部分木に何の変化もなければ、そのまま戻る
            if (result == DeleteAuxResult.NOT_FOUND || result == DeleteAuxResult.OK)
            {
                //カウントを更新
                var c0 = node.GetC0(pos);
                var nc = node[pos]!.size;
                for (int i = pos; i < node.nc; i++)
                    node.count[i] += (nc - c0);
                return result;
            }

            // 部分木posを再編成する必要があるか？
            // Console.Write(result + "\t");
            //要素を左から借りてくる、なければ右から。
            int sub = (pos == 0) ? 0 : pos - 1;
            if (result == DeleteAuxResult.OK_NEED_REORG)
            {
                // 部分木subとsub+1を再編成する
                joined = LesserMerge(node, sub);
                // もし、subとsub+1が併合されていたら、部分木sub+1をnodeから削除する必要がある
                if (joined) pos = sub + 1;
            }
            var myResult = DeleteAuxResult.OK; // このメソッドが返す戻り値。とりあえずOKにしておく
            var removingExecuted = result == DeleteAuxResult.OK_REMOVED || joined;
            // 部分木posが削除された
            if (removingExecuted)
            {
                // nodeの部分木を詰め合わせる
                Array.Copy(node.children, pos + 1, node.children, pos, node.nc - (pos + 1));
                node[node.nc - 1] = null; // 不要な参照を消す
                // もし、nodeの部分木の数のHALF_CHILDより小さいなら再編成が必要である
                if (--node.nc < HALF_CAPACITY)
                {
                    myResult = DeleteAuxResult.OK_NEED_REORG;
                }
            }
            //カウントを更新
            //posが削除されたら、sub,sub+1は詰められる。
            //そうでなければ、sub,sub+1は再分配される。
            if (removingExecuted)
            {
                var fc = sub == 0 ? node.count[1] : node.count[sub + 1] - node.count[sub - 1];
                var nc = node[sub]!.size; //統合済
                Array.Copy(node.count, sub + 1, node.count, sub, (node.nc + 1) - (sub + 1));
                for (int i = sub; i < node.nc; i++)
                    node.count[i] += (nc - fc);
            }
            else
            {
                var f0c = sub == 0 ? 0 : node.count[sub - 1];
                var fc = node.count[sub + 1] - f0c;
                var nlc = node[sub]!.size;
                var nc = nlc + node[sub + 1]!.size;
                node.count[sub] = f0c + nlc;
                for (int i = sub + 1; i < node.nc; i++)
                    node.count[i] += (nc - fc);
            }
            return myResult;
        }
    }
    /// <summary>
    /// B木から要素を削除する。
    /// </summary>
    /// <param name="at">削除する要素のキー</param>
    /// <returns>削除に成功すればtrue、要素が存在しなければfalseを返す。</returns>
    public bool DeleteAt(int at)
    {
        currentLeaf = null;
        // 木が空ならばfalseを返す
        if (root == null)
            return false;
        else // 木が空でない場合
        {
            // deleteAuxメソッドを呼び出して、番号iをもつ要素を削除する
            var result = DeleteAux(root, at);

            switch (result)
            {
                case DeleteAuxResult.NOT_FOUND:    // 見つからなければ、falseを返す
                    return false;
                case DeleteAuxResult.OK_REMOVED:   // 根が削除されたので、rootにnullを代入する（木が空になる）
                    root = null;
                    break;
                case DeleteAuxResult.OK_NEED_REORG:
                    if (((InternalNode)root).nc == 1)
                    {
                        // 根が再編成された結果、根の子が1個だけになったら、
                        // 木の高さを1つ減らす
                        // ### Node p = root; ###
                        root = ((InternalNode)root)[0];
                        // ### ここでpを解放する ###
                    }
                    break;
            }
            return true;
        }
    }
    #endregion
    #region MergeAndSplit
    /// <summary>
    /// 2つのB+木の部分木を連結する。
    /// </summary>
    /// <param name="L">左の部分木</param>
    /// <param name="R">右の部分木</param>
    /// <param name="Hot">連結後の調整によりreturnsの右隣に増やされた部分木。</param>
    /// <returns>LとRを連結して適宜分割した部分木のうち左の部分。</returns>
    private static Node? MergeAux(Node L, Node R, out Node? Hot)
    {
        // L,Rがともに葉であるとき、新しく連結したノードを根とすればよい。
        // o/w: L,Rが両方とも根であるとき、returnsのノードもまた根である。
        // o/w: L,Rのいずれかが真部分木の節であるため、LI.nc+RI.nc\in[Ceil(CAP/2),2*CAP]
        if (L == null) { Hot = null; return R; }
        else if (R == null) { Hot = null; return L; }
        if (L is Leaf l && R is Leaf r)
        {
            Hot = r;
            return l;
        }
        if (L.height > R.height)
        {
            var LI = (L as InternalNode)!;
            var L1 = MergeAux(LI[LI.nc - 1]!, R, out var R1);
            LI[LI.nc - 1] = L1;
            // いま、L1とR1は、Lの子とRがマージされた部分木で、Lの子にあたる。
            if (R1 == null)
            {
                // 子が増えていない
                LI.count[LI.nc - 1] = (LI.nc == 1 ? 0 : LI.count[LI.nc - 2]) + L1!.size;
                Hot = null;
                return LI;
            }
            // H1をLの子にマージする。
            if (LI.nc < CAPACITY)
            {
                LI[LI.nc - 0] = R1;
                LI.count[LI.nc - 1] = (LI.nc == 1 ? 0 : LI.count[LI.nc - 2]) + L1!.size;
                LI.count[LI.nc - 0] = LI.count[LI.nc - 1] + R1!.size;
                LI.nc++;
                Hot = null;
                return LI;
            }
            else
            {
                var R0 = new InternalNode();
                R0.height = LI.height;
                var mnc = LI.nc - HALF_CAPACITY; // LI.nc==CAPACITY
                // LIの末尾をR0へコピー
                Array.Copy(LI.children, HALF_CAPACITY, R0.children, 0, mnc);
                Array.Copy(LI.count, HALF_CAPACITY, R0.count, 0, mnc);
                var lc = LI.count[HALF_CAPACITY - 1];
                var mc = LI.count[LI.nc - 1] - lc;
                // カウントを精算
                for (int i = 0; i < mnc; i++)
                    R0.count[i] -= lc;
                // サイズを更新
                LI.nc = HALF_CAPACITY;
                R0.nc = mnc + 1;
                // R1をR0の末尾に追加
                R0[mnc] = R1;
                R0.count[mnc - 1] = R0.count[mnc - 2] + L1!.size;
                R0.count[mnc] = R0.count[mnc - 1] + R1.size;
                Hot = R0;
                return LI;
            }
        }
        else if (L.height < R.height)
        {
            var RI = (R as InternalNode)!;
            var L1 = MergeAux(L, RI[0]!, out var R1);
            RI[0] = L1;
            if (R1 == null)
            {
                var dc0 = L1!.size - RI.count[0];
                for (int i = 0; i < RI.nc; i++)
                    RI.count[i] += dc0;
                Hot = null;
                return RI;
            }
            var c0 = RI.count[0];
            var lc = L1!.size;
            var rc = R1.size;
            var dc = (lc + rc) - c0;
            if (RI.nc < CAPACITY)
            {
                //R1は1番に挿入される
                Array.Copy(RI.children, 1, RI.children, 2, RI.nc - 1);
                Array.Copy(RI.count, 1, RI.count, 2, RI.nc - 1);
                //産まれた空隙にR1を差し込む
                RI[1] = R1;
                //サイズを更新
                RI.count[0] = L1!.size;
                RI.count[1] = L1!.size + R1.size;
                for (int i = 2; i <= RI.nc; i++)
                    RI.count[i] += dc;
                RI.nc++;
                Hot = null;
                return RI;
            }
            else
            {
                //pos==0
                // L1, R1, LI[1..]を分割
                var R0 = new InternalNode();
                R0.height = RI.height;
                Array.Copy(RI.children, HALF_CAPACITY - 1, R0.children, 0, CAPACITY - (HALF_CAPACITY - 1));
                Array.Copy(RI.children, 1, RI.children, 2, HALF_CAPACITY - 2);
                RI[1] = R1;
                // 子のncを更新する
                RI.nc = HALF_CAPACITY;
                R0.nc = (CAPACITY + 1) - HALF_CAPACITY;
                //カウントを更新
                var tmpCount = new int[CAPACITY + 1];
                Array.Copy(RI.count, 1, tmpCount, 2, CAPACITY - 1);
                tmpCount[0] = lc;
                tmpCount[1] = lc + rc;
                for (int i = 2; i <= CAPACITY; i++)
                    tmpCount[i] += dc;
                //左子の範囲を取り換え、新しい左子全体のカウント
                lc = tmpCount[HALF_CAPACITY - 1];
                for (int i = HALF_CAPACITY; i <= CAPACITY; i++)
                    tmpCount[i] -= lc;
                Array.Copy(tmpCount, 0, RI.count, 0, HALF_CAPACITY);
                Array.Copy(tmpCount, HALF_CAPACITY, R0.count, 0, (CAPACITY + 1) - HALF_CAPACITY);
                Hot = R0;
                return RI;
            }
        }
        else
        {
            InternalNode LI = (L as InternalNode)!, RI = (R as InternalNode)!;
            var lnc = LI.nc;
            var rnc = RI.nc;
            if (lnc + rnc <= CAPACITY)
            {
                //単純にRIをLIに統合する
                Shift(LI, RI, rnc);
                var lc = LI.size;
                Hot = null;
                return LI;
            }
            else
            {
                // 追加の余地がないので、節nodeを2つに分割しなければならない
                // remark: lnc+rnc>=CAP+1>=2*((CAP+1)/2)==2*Ceil(CAP÷2)
                var half = (lnc + rnc + 1) / 2; // LI.nc on exit
                Shift(LI, RI, half - lnc);
                Hot = RI;
                return LI;
            }
        }
    }
    private static Node? Merge(Node? L, Node? R)
    {
        if (L == null) return R;
        if (R == null) return L;
        var result = MergeAux(L!, R!, out var H);
        if (H == null) return result;
        // もし分割が行われたなら、木の高さを1段高くする
        var newNode = new InternalNode();
        newNode.height = H is Leaf ? 2 : H.height + 1;
        newNode.nc = 2;
        newNode[0] = result;
        newNode[1] = H;
        newNode.count[0] = result!.size;
        newNode.count[1] = newNode.count[0] + H!.size;
        return newNode;
    }
    public void PushBack(T data) => root = Merge(root, new Leaf(data));
    private void MergeRight(Node G) => root = Merge(this.root, G);
    public void PushFront(T data) => root = Merge(new Leaf(data), root);
    public T? PopBack()
    {
        DeleteAt(size - 1);
        return currentLeaf == null ? default(T) : currentLeaf.data;
    }
    /// <summary>
    /// 部分木Xを左からat個で切断する。
    /// </summary>
    /// <param name="L">左木。</param>
    /// <param name="R">左木。</param>
    /// <param name="X">部分木。B+木である。</param>
    /// <param name="at">at個の指定。</param>
    /// <returns></returns>
    private static (Node? L, Node? R) SplitAux(Node? X, int at)
    {
        //corner case
        if (X == null || at == 0) return (null, X);
        else if (at == X.size) return (X, null);
#if DEBUG
        if (at < 0 || at > X.size) throw new IndexOutOfRangeException();
#endif
        if (X is Leaf XL)
            return at == 0 ? (null, XL) : (XL, null);
        var XI = (X as InternalNode)!;
        var pos = XI.LocateSubtreeAt(at, out at);
        var (L1, R1) = SplitAux(XI[pos], at);
        // either of L,R is null \implies pos != 0,X.nc-1 resp. (\because denial is fallen above.)
        // so, if pos==0,X.nc-1, then L,R != null respectively.
        // replace and augment X[pos] to L and R, then return (X[..pos]+L, R+X[pos+1..]) (noted in X before entry.)
        if (pos == 0)
        {
            var XIR = new InternalNode(XI.height - 1, XI.children, XI.count, 1, XI.nc - 1);
            return (L1, Merge(R1, XIR));
        }
        else if (pos == XI.nc - 1)
        {
            var XIL = new InternalNode(XI.height - 1, XI.children, XI.count, 0, pos);
            return (Merge(XIL, L1), R1);
        }
        else
        {
            var XIL = new InternalNode(XI.height - 1, XI.children, XI.count, 0, pos);
            var XIR = new InternalNode(XI.height - 1, XI.children, XI.count, pos + 1, XI.nc - (pos + 1));
            return (Merge(XIL, L1), Merge(R1, XIR));
        }
    }
    /// <summary>
    /// L,Rを整形：先頭が唯1個の子を持つ内部節の鎖であるときは、それを除去した部分木を出力する。
    /// </summary>
    /// <param name="L"></param>
    /// <param name="X"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    private static (Node? L, Node? R) SplitAdjust(Node? X, int at)
    {
        var (L, R) = SplitAux(X, at);
        while (L != null && L is InternalNode LI && LI.nc == 1) L = LI[0];
        while (R != null && R is InternalNode RI && RI.nc == 1) R = RI[0];
        return (L, R);
    }
    /// <summary>
    /// 木の末尾depth個の要素を左(depth-count)個と右count個に分割し、それらの組を入れ替えた上で元の木の末尾に戻す。
    /// ただし、countの剰余操作はこのサブルーチン内で行う。
    /// </summary>
    /// <param name="depth"></param>
    /// <param name="count"></param>
    internal void Roll(int depth, int count)
    {
        if (depth == 0 || depth > size) return;
        count %= depth;
        if (count < 0) count += depth;
        var (Base, Effect) = SplitAdjust(root, this.size - depth);
        var (Float, Sink) = SplitAdjust(Effect, Effect!.size - count);
        var K = Merge(Sink, Float);
        root = Merge(Base, K);
    }
    #endregion
    #region Debug
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
#if DEBUG
            return $"Leaf #{l.serial} data={l.data}";
#else
            return $"Leaf #{l.GetHashCode()} data={l.data}";
#endif
        }
        else
        {
            // 内部節である
            InternalNode n = (InternalNode)p;
            var s = "";
#if DEBUG
            s += $"Node #{n.serial} ({n.nc} children, height {n.height}): "; //n.nc>=2
#else
            s += $"Node #{n.GetHashCode()} ({n.nc} children, height {n.height}): "; //n.nc>=2
#endif
            for (int i = 0; i < n.nc; i++)
            {
#if DEBUG
                s += $"#{n[i]!.serial} [{n.count[i]}] ";
#else
                s += $"#{n[i]!.GetHashCode()} [{n.count[i]}] ";
#endif
            }
            s += $"\n";
            for (int i = 0; i < n.nc; i++)
            {
                s += toStringAux(n[i]!) + "\n";
            }
            return s;
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
            return "\t" + this.GetHashCode() + "\n" + toStringAux(root);
        }
    }
    /// <summary>
    /// B+木の条件が満たされているかを判定する。
    /// </summary>
    /// <returns></returns>
    public bool isBPlusTree()
    {
        if (root == null) return true;
        if (root is Leaf) return true;
        var XI = (InternalNode)root;
        if (XI.nc < 2 || XI.nc > CAPACITY)
            return false;
        return XI.children[0..XI.nc].All(c => c != null && c!.height == totalHeight - 1 && isPartBPlusTree(c));
    }
    /// <summary>
    /// X以下の部分木がB+木の部分木の条件を満たしているかを判定する。
    /// </summary>
    /// <param name="X"></param>
    /// <returns></returns>
    private static bool isPartBPlusTree(Node? X)
    {
        if (X == null || X is Leaf) return true;
        var XI = (InternalNode)X;
        if (XI.nc < HALF_CAPACITY || XI.nc > CAPACITY)
            return false;
        return XI.children[0..XI.nc].All(c => c != null && c.height == X.height - 1 && isPartBPlusTree(c));
    }
    #endregion
}
