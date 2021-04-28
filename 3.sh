cat marks.txt | awk 'BEGIN {result=0} \
/grade/ && $2>10 {++result}\
END {print(result)}'
