set terminal wxt size 1920,1080
source='Comparison.csv'
every_term=6
set logscale
plot source skip 1 every ::0::(every_term-1) using 1:4:($4-$5/sqrt($3)):($4-$5/sqrt($3)) with errorlines
do for [i=1:8:1] {
replotter=sprintf("replot source skip 1 every ::%d::%d using 1:4:($4-$5/sqrt($3)):($4-$5/sqrt($3)) with errorlines", i*every_term, (i+1)*every_term-1)
eval replotter
}
pause -1