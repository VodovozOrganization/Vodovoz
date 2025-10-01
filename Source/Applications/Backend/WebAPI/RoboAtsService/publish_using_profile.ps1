# ================================================================
# Скрипт публикации проекта с выбором профиля
# ================================================================
# Возможности скрипта:
# - Автоматический поиск и отображение всех доступных профилей публикации
# - Интерактивный выбор профиля для публикации
# - При публикации в реестр будет вызвана аутентификация в реестре, для того
#   чтобы, если не был сохранен логин в докере, публикация не упала без ошибки,
#   логин и пароль запрашивается только в первый раз
# - RegistryUrl берется из Directory.Build.props в корневой папке решения, 
#   но может быть переопределен в профиле публикации
# - Если MSBuild не установлен в PATH то используется напрямую из каталога Visual Studio 2022 Community
# ================================================================

function Find-SolutionRoot {
    param([string]$startPath)
    $current = Get-Item $startPath
    while ($current -ne $null) {
        $sln = Get-ChildItem -Path $current.FullName -Filter *.sln -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($sln) {
            return $current.FullName
        }
        $current = $current.Parent
    }
    throw "Solution file (.sln) not found in parent directories."
}

function Get-RegistryUrlFromDirectoryBuild {
    param([string]$solutionRoot)
    $directoryBuildsPath = Join-Path $solutionRoot "Directory.Build.props"
    if (-not (Test-Path $directoryBuildsPath)) {
        Write-Warning "Directory.Build.props not found in solution root: $solutionRoot"
        return $null
    }
    
    try {
        [xml]$directoryBuildsXml = Get-Content $directoryBuildsPath
        $publishRepositoryUrl = $directoryBuildsXml.Project.PropertyGroup.RegistryUrl
        return $publishRepositoryUrl
    }
    catch {
        Write-Warning "Failed to read RegistryUrl from Directory.Build.props: $_"
        return $null
    }
}

# Поиск всех профилей публикации в проекте
$pubProfilesPath = Join-Path $PSScriptRoot "Properties\PublishProfiles"
if (-not (Test-Path $pubProfilesPath)) {
    Write-Error "Папка с профилями публикации не найдена: $pubProfilesPath"
    exit 1
}

$profiles = Get-ChildItem -Path $pubProfilesPath -Filter *.pubxml | Select-Object -ExpandProperty BaseName
if ($profiles.Count -eq 0) {
    Write-Error "Не найдено ни одного профиля публикации в $pubProfilesPath"
    exit 1
}

Write-Host "Доступные профили публикации:"
for ($i = 0; $i -lt $profiles.Count; $i++) {
    Write-Host "$($i+1): $($profiles[$i])"
}

do {
    $selection = Read-Host "Выберите профиль публикации (1-$($profiles.Count))"
} while (-not ($selection -as [int]) -or $selection -lt 1 -or $selection -gt $profiles.Count)

$selectedProfile = $profiles[$selection - 1]
Write-Host "Выбран профиль: $selectedProfile"

# Определяем путь к проекту и находим корневую папку решения
$projectPath = $PSScriptRoot
$solutionRoot = Find-SolutionRoot -startPath $projectPath


# Проверяем, используется ли публикация в удаленный реестр образов
$profileFile = Join-Path $pubProfilesPath "$selectedProfile.pubxml"
[xml]$profileXml = Get-Content $profileFile

$publishProvider = $profileXml.Project.PropertyGroup.PublishProvider

if ($publishProvider -eq "ContainerRegistry") {
    Write-Host "Обнаружена публикация в удаленный реестр (PublishProvider: $publishProvider)"
    
    # Проверяем, есть ли RegistryUrl в самом профиле публикации
    $registryUrl = $profileXml.Project.PropertyGroup.RegistryUrl
    if ($registryUrl) {
        Write-Host "Используется RegistryUrl из профиля публикации: $registryUrl"
    } else {
        # Загружаем RegistryUrl из Directory.Build.props для docker login
        $registryUrl = Get-RegistryUrlFromDirectoryBuild -solutionRoot $solutionRoot
        if (-not $registryUrl) {
            Write-Error "Не удалось получить RegistryUrl ни из профиля публикации, ни из Directory.Build.props"
            exit 1
        }
        Write-Host "Используется RegistryUrl из Directory.Build.props: $registryUrl"
    }
    Write-Host "Выполняется docker login..."
    docker login $registryUrl
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Не удалось выполнить docker login в $registryUrl"
        exit 1
    }
}

# Переход в корневую папку решения (найденную выше)
Set-Location -Path $solutionRoot

$msbuildPath = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
if (Get-Command msbuild -ErrorAction SilentlyContinue) {
    $msbuildCmd = "msbuild"
} elseif (Test-Path $msbuildPath) {
    $msbuildCmd = "`"$msbuildPath`""
} else {
    Write-Error "MSBuild not found."
    exit 1
}

# Находим проект, в котором лежит скрипт
$projectFile = Get-ChildItem -Path $projectPath -Filter *.csproj -File | Select-Object -First 1
if (-not $projectFile) {
    Write-Error "Project file (*.csproj) not found in $projectPath"
    exit 1
}

Write-Host "Запуск MSBuild для публикации..."
& $msbuildCmd $projectFile.FullName -restore:True /t:Publish /p:Configuration=Release /p:PublishProfile=$selectedProfile /maxcpucount:6

Write-Host "Press any key to exit..."
[void][System.Console]::ReadKey($true)
