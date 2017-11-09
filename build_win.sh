#!/bin/bash
echo "Чем собрать проект?"
echo "1) msbuild"
echo "2) xbuild"
echo "По умолчанию 1."
read case;

case $case in
    2)
xbuild /p:Configuration=DebugShortWin /p:Platform=x86
;;
    1|*)
msbuild /p:Configuration=DebugShortWin /p:Platform=x86
;;
esac
