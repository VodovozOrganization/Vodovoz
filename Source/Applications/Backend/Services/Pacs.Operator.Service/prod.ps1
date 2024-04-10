
#При изменении настроек необходимо изменить функцию локального запуска под свои нужды

$registry = "docker.vod.qsolution.ru:5100"
$projectFile = "Pacs.Operator.Service.csproj"
$imageName = "pacs.operator.service"
$tagName = "latest"
$containerName = "$imageName.$tagName"

Write-Host -ForegroundColor DarkCyan ("What needs to be done?")
Write-Host -ForegroundColor DarkCyan ("You can select multiple options, example: 12")
Write-Host -ForegroundColor DarkCyan ("1. Build")
Write-Host -ForegroundColor DarkCyan ("2. Push to registry")

function Build {
    try {
        Write-Host "Build project: $projectFile"
        $DockerfileContext = Select-Xml -Path ".\$projectFile" -XPath "/Project/PropertyGroup/DockerfileContext" | ForEach-Object { $_.Node.InnerXML } | Select-Object -First 1
        Write-Host "DockerfileContext: $DockerfileContext"

        docker build -f .\Dockerfile -t "${imageName}:${tagName}" $DockerfileContext
        Write-Host -ForegroundColor Green "Build project $projectFile done"
    } catch {
        Write-Host -ForegroundColor Red "Error: $_"
    }
}

function Push {
    try {
        Write-Host "Push ${imageName}:$tagName to registry $registry"

        docker login $registry
        docker tag ${imageName}:$tagName $registry/${imageName}:$tagName
        docker push $registry/${imageName}:$tagName
        
        Write-Host -ForegroundColor Green "Image $registry/${imageName}:$tagName was pushed"
    } catch {
        Write-Host -ForegroundColor Red "Error: $_"
    }
}

$userResponse = Read-Host
for ($i = 1; $i -le $userResponse.Length; $i++) {
    switch ($userResponse[$i-1]) {
        1 {
            Build
            break
        }
        2 {
            Push
            break
        }
        default {
            Write-Host "Unknown option: $($userResponse[$i-1])"
            break
        }
    }
}

Read-Host -Prompt "Press Enter to exit"