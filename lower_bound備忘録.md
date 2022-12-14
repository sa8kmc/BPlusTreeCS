#### $\mathtt{lower\_ bound},\,\mathtt{upper\_ bound}$の使い方

全順序集合$(X,\le)$について、$X$の有限部分集合全体の集合を$\mathfrak{X}$とし、その元$M$(即ち、$M$は$X$の任意の有限部分集合)をとる。
$N:=\# M$とし、$M$の各要素を$m_0,\dots,m_{N-1}\,(m_0\le m_1\le\dots\le m_{N-1})$で表す。
演算子$\mathtt{lower\_ bound},\,\mathtt{upper\_ bound}: \mathfrak{X}\times X\to \mathbb{N}_{\ge 0}$を、
$$
\begin{aligned}
\mathtt{lower\_ bound}(M;x)&:=\min\{i\mid x\le m_i\}\cup\{N\},\\
\mathtt{upper\_ bound}(M;x)&:=\min\{i\mid x <  m_i\}\cup\{N\}.
\end{aligned}
$$と定める。

----------
###### 補題
> $$
> \begin{aligned}
> \mathtt{lower\_ bound}(M;x)&=\#\{i\mid m_i <  x\},\\
> \mathtt{upper\_ bound}(M;x)&=\#\{i\mid m_i\le x\}.
> \end{aligned}
> $$

> $$
> \begin{aligned}
> \mathtt{lower\_ bound}(M;x)&=i \textrm{~s.t.~}(i=0\vee m_{i-1} < x)\wedge(i=N\vee x\le m_i)\\
> \mathtt{upper\_ bound}(M;x)&=i \textrm{~s.t.~}(i=0\vee m_{i-1}\le x)\wedge(i=N\vee x < m_i)
> \end{aligned}
> $$

> $$
> \begin{aligned}
> \#\{i\mid m_i\ge x\}&=N-\mathtt{lower\_ bound}(M;x),\\
> \#\{i\mid m_i >  x\}&=N-\mathtt{upper\_ bound}(M;x).
> \end{aligned}
> $$

> $$
> \begin{aligned}
> \mathtt{lower\_ bound}(M;x)-1&=\max\{i\mid m_i <  x\}\cup\{-1\},\\
> \mathtt{upper\_ bound}(M;x)-1&=\max\{i\mid m_i\le x\}\cup\{-1\}.
> \end{aligned}
> $$

