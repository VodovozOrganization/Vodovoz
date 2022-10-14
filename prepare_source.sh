#!/bin/bash
echo "Что делаем?"
echo "1) git pull"
echo "2) nuget restore"
echo "3) cleanup packages directories"
echo "4) build Vodovoz.sln (for Linux)"
echo "5) remove obj & bin folders"
echo "Можно вызывать вместе, например git+nuget=12"
read case;

case $case in
    *5*)
find . -type d -regex '.*\(bin\|obj\)' -exec rm -rv {} + 
;;&
    *3*)
rm -v -f -R ./Source/packages/*
rm -v -f -R ./Source/Libraries/External/QSProjects/packages/*
rm -v -f -R ./Source/Libraries/External/My-FyiReporting/packages/*
;;&
    *1*)
git pull --autostash
cd ./Source/Libraries/External/GammaBinding
git pull --autostash
cd ../GMap.NET
git pull --autostash
cd ../Gtk.DataBindings
git pull --autostash
cd ../My-FyiReporting
git pull --autostash
cd ../QSProjects
git pull --autostash
cd ../../../../Vodovoz
;;&
    *2*)
nuget restore ./Source/Vodovoz.sln;
nuget restore ./Source/Libraries/External/QSProjects/QSProjectsLib.sln;
nuget restore ./Source/Libraries/External/My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln
;;&
    *4*)
msbuild /p:Configuration=LinuxDesktop /p:Platform=x86 Source/Vodovoz.sln -maxcpucount:4
;;&
esac

read -p "Press enter to exit"
