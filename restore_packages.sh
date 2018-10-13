#!/bin/bash
echo "Что делаем?"
echo "1) nuget restore"
echo "2) cleanup packages directories"
echo "По умолчанию 1."
read case;

case $case in
    2)
rm -v -f -R ./packages/*
rm -v -f -R ../QSProjects/packages/*
rm -v -f -R ../My-FyiReporting/packages/*
rm -v -f -R ../VodovozService/packages/*
;;
    1|*)
nuget restore Vodovoz/Vodovoz.sln;
nuget restore QSProjects/QSProjectsLib.sln;
nuget restore My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln
nuget restore VodovozService/VodovozService.sln
;;
esac


read -p "Press enter to exit"
