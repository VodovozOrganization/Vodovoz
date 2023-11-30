$DockerfileContext = Select-Xml -Path ".\Mango.Service.csproj" -XPath "/Project/PropertyGroup/DockerfileContext" | ForEach-Object { $_.Node.InnerXML } | Select-Object -First 1

Write-Host "DockerfileContext: $DockerfileContext"
$containerName = "mango.service.develop"
$imageName = "mango.service"
$tagName = "develop"

docker stop $containerName
docker remove $containerName

docker build -f .\Dockerfile -t "${imageName}:${tagName}" $DockerfileContext

docker run -d `
--name mango.service.develop `
-p 5002:5002 -p 5003:5001 `
-v ${PWD}/appsettings.Development.json:/app/appsettings.Development.json `
-v ${PWD}/bin/Debug/Docker/logs/:/var/log/mango.service `
--env ASPNETCORE_ENVIRONMENT=Development `
--env ASPNETCORE_URLS=http://+:5001 `
mango.service:develop
