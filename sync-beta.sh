#!/bin/bash
echo "Which version to upload?"
echo "1) Release"
echo "2) Debug"
read case;

ProjectName="Vodovoz"

case $case in
    1)
BinDir=./$ProjectName/bin/Release
;;
    2)
BinDir=./$ProjectName/bin/Debug
;;
esac

# Очистка бин от лишний файлов

rm -v ${BinDir}/*.mdb

cp -r -v ${BinDir}/* ./Beta-Sync

syncthing -verbose
