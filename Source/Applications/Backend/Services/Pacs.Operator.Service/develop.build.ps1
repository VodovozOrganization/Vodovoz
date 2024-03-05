$DockerfileContext = Select-Xml -Path ".\Pacs.Operator.Service.csproj" -XPath "/Project/PropertyGroup/DockerfileContext" | ForEach-Object { $_.Node.InnerXML } | Select-Object -First 1

Write-Host "DockerfileContext: $DockerfileContext"
$containerName = "pacs.operator.develop"
$imageName = "pacs.operator.service"
$tagName = "develop"

docker stop $containerName
docker remove $containerName

docker build -f .\Dockerfile -t "${imageName}:${tagName}" $DockerfileContext

docker run -d `
--name $containerName `
-p 5001:5001 `
-v ${PWD}/appsettings.Development.json:/app/appsettings.Development.json `
-v ${PWD}/bin/Debug/Docker/logs/:/var/log/$imageName `
--env ASPNETCORE_ENVIRONMENT=Development `
--env ASPNETCORE_URLS=http://+:5001 `
${imageName}:${tagName}
