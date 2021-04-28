#!/bin/bash
file="/etc/passwd"
IFS=$':'
for var in $(cat $file)
do 
echo " $var"
done

