#!/bin/bash
echo "Что делаем?"
echo "1) git pull"
echo "2) nuget restore"
echo "3) cleanup packages directories"
echo "Можно вызывать вместе, например git+nuget=12"
read case;

case $case in
    *3*)
rm -v -f -R ./packages/*
rm -v -f -R ../QSProjects/packages/*
rm -v -f -R ../My-FyiReporting/packages/*
rm -v -f -R ../VodovozService/packages/*
;;&
    *1*)
git pull --autostash
cd ../VodovozService
git pull --autostash
cd ../QSProjects
git pull --autostash
cd ../Gtk.DataBindings
git pull --autostash
cd ../GammaBinding
git pull --autostash
cd ../GMap.NET
git pull --autostash
cd ../Vodovoz
;;&
    *2*)
nuget restore Vodovoz.sln;
nuget restore ../QSProjects/QSProjectsLib.sln;
nuget restore ../My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln
nuget restore ../VodovozService/VodovozService.sln
;;&
esac


read -p "Press enter to exit"
