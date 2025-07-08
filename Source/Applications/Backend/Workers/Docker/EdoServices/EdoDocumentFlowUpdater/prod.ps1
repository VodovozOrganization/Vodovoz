
#При изменении настроек необходимо изменить функцию локального запуска под свои нужды

$registry = "docker.vod.qsolution.ru:5100"
$projectFile = "EdoDocumentFlowUpdater.csproj"
$baseImageName = "edo-services.document-flow-updater"
$tagName = "latest"

Write-Host -ForegroundColor DarkCyan ("What needs to be done?")
Write-Host -ForegroundColor DarkCyan ("You can select multiple options, example: 123")
Write-Host -ForegroundColor DarkCyan ("1. Build with context `".`"")
Write-Host -ForegroundColor DarkCyan ("2. Push as `"edo-services.document-flow-updater`"")
Write-Host -ForegroundColor DarkCyan ("3. Push as `"taxcom-docflow-vv-north.document-flow-updater`"")
Write-Host -ForegroundColor DarkCyan ("4. Push as `"taxcom-docflow-vv-south.document-flow-updater`"")
Write-Host -ForegroundColor DarkCyan ("5. Push as `"taxcom-docflow-beverages-world.document-flow-updater`"")
Write-Host -ForegroundColor DarkCyan ("6. Push as `"taxcom-docflow-non-alcoholic-beverages-world.document-flow-updater`"")

function Build {
    try {
        Write-Host "Build project: $projectFile"
        #$DockerfileContext = Select-Xml -Path ".\$projectFile" -XPath "/Project/PropertyGroup/DockerfileContext" | ForEach-Object { $_.Node.InnerXML } | Select-Object -First 1
        $DockerfileContext = "."
        Write-Host "DockerfileContext: $DockerfileContext"

        docker build -f .\Dockerfile -t "${baseImageName}:${tagName}" $DockerfileContext
        Write-Host -ForegroundColor Green "Build project $projectFile done"
    } catch {
        Write-Host -ForegroundColor Red "Error: $_"
    }
}

function Push {
	param (
		[string]$ImageName
	)
    try {
        Write-Host "Push ${ImageName}:$tagName to registry $registry"

        docker login $registry
        docker tag ${baseImageName}:$tagName $registry/${ImageName}:$tagName
        docker push $registry/${ImageName}:$tagName
        
        Write-Host -ForegroundColor Green "Image $registry/${ImageName}:$tagName was pushed"
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
            Push -ImageName "edo-services.document-flow-updater"
            break
        }
        3 {
			Push -ImageName "taxcom-docflow-vv-north.document-flow-updater"
            break
        }
        4 {
			Push -ImageName "taxcom-docflow-vv-south.document-flow-updater"
            break
        }
        5 {
			Push -ImageName "taxcom-docflow-beverages-world.document-flow-updater"
            break
        }
        6 {
			Push -ImageName "taxcom-docflow-non-alcoholic-beverages-world.document-flow-updater"
            break
        }
        default {
            Write-Host "Unknown option: $($userResponse[$i-1])"
            break
        }
    }
}

Read-Host -Prompt "Press Enter to exit"
