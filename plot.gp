set terminal pngcairo size 640,480
set grid
source='Comparison_capacities.csv'
set output 'capacities.png'
set datafile separator "\t"
every_term=9
set logscale
set title 'insertion time for capacities'
set ylabel 'insertion time[ms] / operation'
set xlabel 'entries[1]'
plotter=sprintf("plot source every %d::%d using 1:($4/$1):($5/$1/sqrt($3)) with errorlines t 'cap=%d'", every_term, 1, 4)
do for [i=4:8:4] {
plotter=plotter.sprintf(", source every %d::%d using 1:($4/$1):($5/$1/sqrt($3)) with errorlines t 'cap=%d'", every_term, 1+i, 2**(i+2))
}
set key right bottom
eval plotter
#------------------------------------------
set terminal pngcairo size 640,480
set grid
source='Comparison.csv'
set output 'algorithm.png'
set datafile separator ","
every_term=4
set logscale
set format y "10^{%L}"
set title 'operation time for algorithms'
set ylabel 'operation time[sec] per operation'
set xlabel '#operation[1]'
set boxwidth 1
array algoname[4]=['2-3-4 Tree','B+ Tree', 'Pancake', 'Naive Copy']
plotter = 'plot '
i=3
do for [j=1:4:1] {
plotter=plotter.sprintf("source using 1:($%d/$1) with lp t '%s'", j+1, algoname[j])
if (j!=4) {
    plotter = plotter.', '
}
}
eval plotter
set key right bottom

