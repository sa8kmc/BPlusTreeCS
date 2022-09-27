set terminal pngcairo size 640,480
set grid
source='Comparison.csv'
set output 'algorithm.png'
set datafile separator "\t"
every_term=9
set logscale
set title 'operation time for algotithms'
set ylabel 'operation time[ms] / operation'
set xlabel 'operation entry[1]'
set boxwidth 1
array algoname[4]=['2-3-4 Tree','B+ Tree', 'Pancake', 'Naive Copy']
plotter=sprintf("plot source every %d::1 using 1:($3/$1) with lp t '%s', ",every_term, algoname[1])
i=3
do for [j=3:5:1] {
plotter=plotter.sprintf("source every %d::%d using 1:($%d/$1) with lp t '%s', ", every_term, 1+i, j, algoname[j-1])
}
eval plotter
set key right bottom

