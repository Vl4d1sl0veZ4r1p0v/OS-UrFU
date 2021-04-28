cat marks.txt | awk 'BEGIN {result=0}\
{result+=$0}\
END {print(result)}'
