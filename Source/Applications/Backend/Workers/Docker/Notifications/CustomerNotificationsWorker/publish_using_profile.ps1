# ================================================================
# Локальный скрипт публикации проекта
# ================================================================
# Этот скрипт находит главный скрипт publish_project_using_profile.ps1
# в корне решения и запускает его, передав путь к текущему проекту
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

try {
    # Определяем путь к текущему проекту
    $projectPath = $PSScriptRoot
    
    # Находим корень решения
    $solutionRoot = Find-SolutionRoot -startPath $projectPath
    
    # Ищем главный скрипт в корне решения
    $mainScriptPath = Join-Path $solutionRoot "publish_project_using_profile.ps1"
    
    if (-not (Test-Path $mainScriptPath)) {
        Write-Error "Главный скрипт не найден: $mainScriptPath"
        exit 1
    }
    
    Write-Host "Запуск главного скрипта: $mainScriptPath"
    Write-Host "Путь проекта: $projectPath"
    Write-Host ""
    
    # Запускаем главный скрипт с передачей пути проекта
    & $mainScriptPath -ProjectPath $projectPath
}
catch {
    Write-Error "Ошибка при выполнении скрипта: $_"
    Write-Host "Нажмите любую клавишу для выхода..."
    [void][System.Console]::ReadKey($true)
    exit 1
}