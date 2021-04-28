cat marks.txt | awk 'BEGIN {result=0}\
/grade/ {result+=2}\
END {print(result)}'
