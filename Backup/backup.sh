#!/bin/bash

set -x
nameTarBackUp="backup.tar"
backUpExtention="bu"
nameCheckSum="checksum"
path=
extention=
savePath=
filename=
(( bound=5 ))
usage="$(basename "$0" .sh) 

where:
    -h  show this help text
    -s  set the seed value (default: 42)"

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
      elif [ -z "$extention" ]
      then
        echo 'extention is empty'
      else 
        if [ ! -d "$savePath" ]
        then
          echo 'save path is not exist'
        elif [ -z "$filename" ]
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

set_repeatity () {
  export EDITOR=/bin/nano
  crontab -l > mycron
  echo "*/${time} * * * * "${pwd}/${0}" -p \""${path}"\" -e \""${extention}"\" -s \""${savePath}"\" -n \$(crontab -l | grep "${scriptPath}" | wc -l)_\$(date '+%Y_%m_%d_%H_%M_%S')" -a $bound >> mycron
  crontab mycron
  rm mycron
}

check_archive () {
  local code=1
  if [ -e "${path}/${nameCheckSum}" ]
  then 
    code=0
    cd "${path}" \
    && sha256sum -c "${nameCheckSum}"
  else 
    echo 'unexpected object'
  fi
  return $code
}

erase () {
  local code=0
  local count=1
  for file in $(ls -t "${savePath}" | grep -E ".${backUpExtention}")
  do
    if [ $count -ge $bound ]
    then 
      if rm -r "${savePath}/${file}" 2>/dev/null
      then
        code=1
      fi
    fi
    (( ++count ))
  done
  return $code
}

make_archive () {
  local code=1 
  if collect_files
  then 
    if [ -n "$bound" ] && [ "$bound" -eq "$bound" ] 2>/dev/null;
    then
      if erase && mkdir -m go-rwx "${path}/${filename}.${backUpExtention}" \
      && gzip "${path}/${nameTarBackUp}" \
      && mv "${path}/${nameTarBackUp}.gz" "${path}/${filename}.${backUpExtention}/${nameTarBackUp}.gz" \
      && mv "${path}/${filename}.${backUpExtention}" "${savePath}/${filename}.${backUpExtention}" \
      && cd "${savePath}/${filename}.${backUpExtention}/" \
      && sha256sum "${nameTarBackUp}.gz" > "${nameCheckSum}"
      then
        code=0
      else 
        echo 'command failed'
      fi
      else
        echo 'argument is not a number'
    fi
  fi
  return $code
}

collect_files () { #https://unix.stackexchange.com/questions/407079/how-to-find-specific-file-types-and-tar-them Копейцев будет спрашивать все параметры, которые использовал!!!
  local code=1
  if check_params
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

while [ "$1" != "" ]; do
    case $1 in
        -p | --path )           shift
                                path=$1
                                shift
                                while [ "$1" != "" ]; do
                                    case $1 in
                                        -e | --extention )      shift 
                                                                extention=$1
                                                                ;;
                                        -s | --save-path )      shift
                                                                savePath=$1
                                                                ;;
                                        -n | --name )           shift
                                                                filename=$1
                                                                ;;
                                        -a | --amount )         shift
                                                                (( bound=$1 ))
                                                                ;;                                                                                
                                        *)                      echo "incorrect you have to use -e for extention and -s for save-path, also -n for name"
                                                                exit 1
                                                                ;;
                                    esac
                                    shift
                                done
                                make_archive
                                ;;
        -r | --repeat )         shift                              
                                while [ "$1" != "" ]; do
                                    case $1 in
                                        -p | --path )           shift
                                                                path=$1
                                                                ;;
                                        -e | --extention )      shift 
                                                                extention=$1
                                                                ;;
                                        -s | --save-path )      shift
                                                                savePath=$1
                                                                ;;                                        
                                        -a | --amount )         shift
                                                                (( bound=$1 ))
                                                                ;;
                                        *)                      echo "incorrect you have to use -e for extention and -s for save-path, also -n for name"
                                                                exit 1
                                                                ;;
                                    esac
                                    shift
                                done
                                make_archive
                                ;;
        -c | --check )          shift
                                path=$1
                                check_archive
                                exit 0
                                ;;
        -h | --help )           echo "$usage"
                                exit 0
                                ;;
        * )                     echo "arguments are incorrect"
                                exit 1
                                ;;
    esac
    shift
done
exit 0
set +x