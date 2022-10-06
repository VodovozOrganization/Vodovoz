Write-Output "Что делаем?"
Write-Output "1) git pull"
Write-Output "2) nuget restore"
Write-Output "3) Очистить кэш nuget пакетов"
Write-Output "4) build Vodovoz.sln (for Windows, временно не работает)"
Write-Output "5) remove obj & bin folder"
Write-Output "Можно вызывать вместе, например git+nuget=12"
$selection = Read-Host

function Get-Tree($Path, $Include = '*') {
    @(Get-Item $Path -Include $Include -Force) +
    (Get-ChildItem $Path -Recurse -Filter $Include -Force) |
    Sort-Object pspath -Descending -unique
}

function Remove-Tree($Path, $Include = '*') {
    Get-Tree $Path $Include | Remove-Item -force -recurse -Verbose
}

if ($selection.Contains("5")) {    
	Set-Location (Get-Item .).parent.FullName
    Remove-Tree bin
    Remove-Tree obj
	Set-Location Vodovoz
}

if ($selection.Contains("3")) {
    dotnet nuget locals all --clear
}

if ($selection.Contains("1")){
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
}

if ($selection.Contains("2")){
    dotnet restore Vodovoz.sln;
    dotnet restore ../QSProjects/QSProjectsLib.sln;
    dotnet restore ../My-FyiReporting/MajorsilenceReporting-Linux-GtkViewer.sln
}

if ($selection.Contains("4")){
   dotnet msbuild /p:Configuration=DebugWin /p:Platform=x86 Vodovoz.sln
}

Write-Output "Выполнение завершено"