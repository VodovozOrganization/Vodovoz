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
cd ../
find . -type d -regex '.*\(bin\|obj\)' -exec rm -rv {} + 
cd Vodovoz
;;&
    *3*)
rm -v -f -R ./packages/*
rm -v -f -R ../QSProjects/packages/*
rm -v -f -R ../My-FyiReporting/packages/*
;;&
    *1*)
git pull --autostash
cd ../GammaBinding
git pull --autostash
cd ../GMap.NET
git pull --autostash
cd ../Gtk.DataBindings
git pull --autostash
cd ../My-FyiReporting
git pull --autostash
cd ../QSProjects
git pull --autostash
cd ../Vodovoz
;;&
    *2*)
nuget restore Vodovoz.sln;
nuget restore ../QSProjects/QSProjectsLib.sln;
nuget restore ../My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln
;;&
    *4*)
msbuild /p:Configuration=Debug /p:Platform=x86 Vodovoz.sln
;;&
esac

read -p "Press enter to exit"
