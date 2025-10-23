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

param(
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath
)

# Проверка и определение пути проекта
if (-not $ProjectPath) {
    # Если параметр не передан, проверяем, запущен ли скрипт из корня решения
    $currentPath = $PSScriptRoot
    $solutionFile = Get-ChildItem -Path $currentPath -Filter *.sln -File -ErrorAction SilentlyContinue | Select-Object -First 1
    
    if ($solutionFile) {
        Write-Host ""
        Write-Host "====================================================================" -ForegroundColor Red
        Write-Host "ОШИБКА: Скрипт запущен из корня решения!" -ForegroundColor Red
        Write-Host "Данный скрипт должен запускаться из папки проекта." -ForegroundColor Red
        Write-Host "Используйте локальный скрипт publish_using_profile.ps1 в проекте." -ForegroundColor Red
        Write-Host "====================================================================" -ForegroundColor Red
        Write-Host ""
        Write-Host "Нажмите любую клавишу для выхода..."
        [void][System.Console]::ReadKey($true)
        exit 1
    }
    
    # Если не из корня решения и параметр не передан, используем текущий путь
    $ProjectPath = $PSScriptRoot
}

function Find-SolutionRoot {
    param([string]$startPath)
    # Поскольку главный скрипт всегда находится в корне решения,
    # просто возвращаем директорию, где находится этот скрипт
    return $PSScriptRoot
}

function Get-RegistryUrlFromDirectoryBuild {
    param([string]$solutionRoot)
    $directoryBuildsPath = Join-Path $solutionRoot "Directory.Build.props"
    if (-not (Test-Path $directoryBuildsPath)) {
        Write-Warning "Directory.Build.props не найден в корне решения: $solutionRoot"
        return $null
    }
    
    try {
        [xml]$directoryBuildsXml = Get-Content $directoryBuildsPath
        $publishRepositoryUrl = $directoryBuildsXml.Project.PropertyGroup.RegistryUrl
        return $publishRepositoryUrl
    }
    catch {
        Write-Warning "Не удалось прочитать RegistryUrl из Directory.Build.props: $_"
        return $null
    }
}

function Test-DockerAuth {
    param([string]$registryUrl)
    
    $dockerConfigPath = Join-Path $env:USERPROFILE ".docker\config.json"
    
    if (-not (Test-Path $dockerConfigPath)) {
        Write-Host "Конфигурационный файл Docker не найден в $dockerConfigPath"
        return $false
    }
    
    try {
        $configContent = Get-Content $dockerConfigPath -Raw | ConvertFrom-Json
        
        if ($configContent.auths -and $configContent.auths.PSObject.Properties.Name -contains $registryUrl) {
            Write-Host "Найдена существующая аутентификация для $registryUrl в конфигурации Docker"
            return $true
        } else {
            Write-Host "Аутентификация для $registryUrl не найдена в конфигурации Docker"
            return $false
        }
    }
    catch {
        Write-Warning "Не удалось прочитать конфигурацию Docker: $_"
        return $false
    }
}

function Test-DockerEngine {
    try {
        # Проверяем доступность Docker Engine через named pipe
        $dockerPipe = "//./pipe/dockerDesktopLinuxEngine"
        
        # Попытка подключения к named pipe
        try {
            $pipeStream = New-Object System.IO.Pipes.NamedPipeClientStream(".", "dockerDesktopLinuxEngine", [System.IO.Pipes.PipeDirection]::InOut)
            $pipeStream.Connect(1000) # Timeout 1 секунда
            $pipeStream.Close()
            Write-Host "Docker Engine запущен (соединение через pipe успешно)"
            return $true
        }
        catch {
            Write-Host "Docker Engine не запущен (соединение через pipe не удалось)"
            return $false
        }
    }
    catch {
        Write-Host "Docker недоступен"
        return $false
    }
}

function Wait-ForDockerDesktop {
    Write-Host ""
    Write-Host "====================================================================" -ForegroundColor Yellow
    Write-Host "Docker Desktop не запущен или недоступен." -ForegroundColor Yellow
    Write-Host "Пожалуйста, запустите Docker Desktop и дождитесь его полной загрузки." -ForegroundColor Yellow
    Write-Host "====================================================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Ожидание запуска Docker Desktop..." -ForegroundColor Cyan
    
    $timeout = 300 # 5 минут
    $elapsed = 0
    $interval = 5
    
    while ($elapsed -lt $timeout) {
        if (Test-DockerEngine) {
            Write-Host "Docker Desktop успешно запущен!" -ForegroundColor Green
            return $true
        }
        
        Start-Sleep -Seconds $interval
        $elapsed += $interval
        
        # Каждые 30 секунд показываем сообщение
        if ($elapsed % 30 -eq 0) {
            Write-Host ""
            Write-Host "Все еще ожидаем запуска Docker Desktop... (прошло $elapsed секунд)" -ForegroundColor Cyan
        }
    }
    
    Write-Host ""
    Write-Error "Timeout: Docker Desktop не запустился в течение $timeout секунд"
    return $false
}

# Поиск всех профилей публикации в проекте
$pubProfilesPath = Join-Path $ProjectPath "Properties\PublishProfiles"
if (-not (Test-Path $pubProfilesPath)) {
    Write-Error "Папка с профилями публикации не найдена: $pubProfilesPath"
    exit 1
}

$profiles = @(Get-ChildItem -Path $pubProfilesPath -Filter *.pubxml | Select-Object -ExpandProperty BaseName)
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
$projectPath = $ProjectPath
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
    
    # Новая логика аутентификации
    Write-Host "Проверяем аутентификацию в реестре $registryUrl..."
    
    # Шаг 1: Проверяем, залогинен ли пользователь в реестр через config.json
    if (Test-DockerAuth -registryUrl $registryUrl) {
        Write-Host "Пользователь уже аутентифицирован в реестре $registryUrl" -ForegroundColor Green
    } else {
        Write-Host "Требуется аутентификация в реестре $registryUrl"
        
        # Шаг 2: Проверяем, запущен ли Docker Engine
        if (Test-DockerEngine) {
            # Шаг 3: Если запущен, выполняем логин
            Write-Host "Выполняется docker login..."
            docker login $registryUrl
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Не удалось выполнить docker login в $registryUrl"
                exit 1
            }
        } else {
            # Шаг 4: Если не запущен, ждем запуска Docker Desktop
            if (-not (Wait-ForDockerDesktop)) {
                exit 1
            }
            
            # После запуска Docker Desktop выполняем логин
            Write-Host "Выполняется docker login..."
            docker login $registryUrl
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Не удалось выполнить docker login в $registryUrl"
                exit 1
            }
        }
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
    Write-Error "MSBuild не найден."
    exit 1
}

# Находим проект, в котором лежит скрипт
$projectFile = Get-ChildItem -Path $ProjectPath -Filter *.csproj -File | Select-Object -First 1
if (-not $projectFile) {
    Write-Error "Файл проекта (*.csproj) не найден в $ProjectPath"
    exit 1
}

Write-Host "Запуск MSBuild для публикации..."
& $msbuildCmd $projectFile.FullName -restore:True /t:Publish /p:Configuration=Release /p:PublishProfile=$selectedProfile /maxcpucount:6

Write-Host "Нажмите любую клавишу для выхода..."
[void][System.Console]::ReadKey($true)
