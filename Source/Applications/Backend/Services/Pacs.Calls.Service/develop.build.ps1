$DockerfileContext = Select-Xml -Path ".\Pacs.Calls.Service.csproj" -XPath "/Project/PropertyGroup/DockerfileContext" | ForEach-Object { $_.Node.InnerXML } | Select-Object -First 1

Write-Host "DockerfileContext: $DockerfileContext"
$containerName = "pacs.calls.develop"
$imageName = "pacs.calls.service"
$tagName = "develop"

docker stop $containerName
docker remove $containerName

docker build -f .\Dockerfile -t "${imageName}:${tagName}" $DockerfileContext

docker run -d `
--name $containerName `
-v ${PWD}/appsettings.Development.json:/app/appsettings.Development.json `
-v ${PWD}/bin/Debug/Docker/logs/:/var/log/$imageName `
--env DOTNET_ENVIRONMENT=Development `
${imageName}:${tagName}


