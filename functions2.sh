#!/bin/bash
#http://linuxcommand.org/lc3_wss0120.php
nameTarBackUp="backup.tar"
backUpExtention="bu"
path="/home/xakep/Загрузки"
extention="xlsx"
savePath="/home/xakep/Документы/BackUps"
filename="seventh"

set -x
check_params () { 
    local code=1
    if [ -d "$path" ] \
    && [ -n "$extention" ] \
    && [ -d "$savePath" ] \
    && [ -n "$filename" ] \
    && [ ! -e "${savePath}/${filename}.${backUpExtention}" ]
    then
        code=0
    else 
      if [ ! -d "$path" ]
      then
        echo 'path is not exist'
      elif [ ! -n "$extention" ]
      then
        echo 'extention is empty'
      else 
        if [ ! -d "$savePath" ]
        then
          echo 'save path is not exist'
        elif [ ! -n "$filename" ]
        then
          echo 'name is empty'
        else
          if [ -e "${savePath}/${filename}.${backUpExtention}" ]
          then
            echo 'this name has already been used'
          fi
        fi
      fi
    fi
    return $code
}

make_archive () {
  local code=1
  collect_files
  if [ $? == 0 ]
  then 
    mkdir -m go-rwx "${savePath}/${filename}.${backUpExtention}" \
    && gzip "${path}/${nameTarBackUp}" \
    && mv "${path}/${nameTarBackUp}.gz" "${savePath}/${filename}.${backUpExtention}/${nameTarBackUp}.gz"
    if [ $? == 0 ]
    then
      code=0
    else 
      echo 'command failed'
    fi
  fi
  return $code
}

collect_files () { #https://unix.stackexchange.com/questions/407079/how-to-find-specific-file-types-and-tar-them Копейцев будет спрашивать все параметры, которые использовал!!!
  local code=1
  check_params
  if [ $? == 0 ]
  then 
    find "${path}" -maxdepth 1 -name "*.${extention}" -print0 | tar -cvf "${path}/${nameTarBackUp}" --null -T - \
    && chmod -R go-rwx "${path}/${nameTarBackUp}"
    if [ "$(tar --list -f "${path}/${nameTarBackUp}" | wc -l)" != "0" ]
    then
      code=0
    else 
      echo 'no files with this extension'
      sudo rm -f "${path}/${nameTarBackUp}"
    fi
  fi
  return $code
}

make_archive
set +x