set terminal pngcairo size 640,480
set grid
source='Comparison2.csv'
set output 'capacities.png'
set datafile separator ","
every_term=9
set logscale
set title 'insertion time for capacities'
set ylabel 'insertion time[ms] / operation'
set xlabel 'entries[1]'
plotter='plot '
do for [i=0:8:1] {
plotter=plotter.sprintf("source every %d::%d using 1:($3/$1) with lp t 'cap=%d'", every_term, 1+i, 2**(i+2))
if(i!=8){
    plotter = plotter.", "
}
}
set key left bottom
eval plotter
#------------------------------------------
set terminal pngcairo size 640,480
set grid
source='Comparison1.csv'
set output 'algorithm.png'
set datafile separator ","
every_term=4
set logscale
set format y "10^{%L}"
set title 'operation time for algorithms'
set ylabel 'time[sec] per operation'
set xlabel '#operation[1]'
set boxwidth 1
array algoname[5]=['Red-Black Tree','2-3-4 Tree','B+ Tree', 'Reversal', 'Naive Copy']
plotter = 'plot '
do for [j=1:5:1] {
plotter=plotter.sprintf("source using 1:($%d/$1) with lp t '%s', ", j+1, algoname[j])
}
plotter = plotter."x/10**12 with lines lt -1 lw 1 dt (10,5) lc 'gray60' t '{/Symbol Q}(N)', "
plotter = plotter."log(x)/10**7 with lines lt -1 lw 1 dt (20,10) lc 'gray60' t '{/Symbol Q}(log(N))'"
set key right bottom
eval plotter
#------------------------------------------
set terminal pngcairo size 640,480
set grid
source='Comparison1.csv'
set output 'algorithm_part.png'
set datafile separator ","
every_term=4
set logscale
set format y "10^{%L}"
set title 'operation time for algorithms'
set ylabel 'time[sec] per operation'
set xlabel '#operation[1]'
set boxwidth 1
array algoname[5]=['Red-Black Tree','2-3-4 Tree','B+ Tree', 'Reversal', 'Naive Copy']
array indices[2]=[3,5]
plotter = 'plot '
do for [i=1:2:1] {
j=indices[i]
plotter=plotter.sprintf("source using 1:($%d/$1) with lp t '%s' lc %d, ", j+1, algoname[j], j)
}
plotter = plotter."x/10**12 with lines lt -1 lw 1 dt (10,5) lc 'gray60' t '{/Symbol Q}(N)', "
plotter = plotter."log(x)/10**7 with lines lt -1 lw 1 dt (20,10) lc 'gray60' t '{/Symbol Q}(log(N))'"
set key right bottom
eval plotter
