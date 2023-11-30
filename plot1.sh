#!/bin/bash
for ((current = 0; current < 7500; current++)); do
cat << EOF | gnuplot
set terminal pngcairo enhanced size 1920,1080 background rgb "black"
set output 'output/$current.png'
set datafile separator ','
set title 'Constellation' tc rgb "white"
set xrange [-2.5:2.5]
set yrange [-2.5:2.5]
set xtics 0.25
set ytics 0.25
set grid lc rgb "white"
set border 0 lc rgb "gray25"
unset key
set object circle at first 0,0 radius char 0.2 fillcolor rgb 'white' fillstyle solid noborder
plot 'constellations/$current.csv' using 2:3 with points lc rgb "green"
EOF
done
