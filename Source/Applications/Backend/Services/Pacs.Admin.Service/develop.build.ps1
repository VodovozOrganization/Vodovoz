$DockerfileContext = Select-Xml -Path ".\Pacs.Admin.Service.csproj" -XPath "/Project/PropertyGroup/DockerfileContext" | ForEach-Object { $_.Node.InnerXML } | Select-Object -First 1

Write-Host "DockerfileContext: $DockerfileContext"
$containerName = "pacs.admin.develop"
$imageName = "pacs.admin.service"
$tagName = "develop"

docker stop $containerName
docker remove $containerName

docker build -f .\Dockerfile -t "${imageName}:${tagName}" $DockerfileContext

docker run -d `
--name $containerName `
-p 5000:5000 `
-v ${PWD}/appsettings.Development.json:/app/appsettings.Development.json `
-v ${PWD}/bin/Debug/Docker/logs/:/var/log/$imageName `
--env ASPNETCORE_ENVIRONMENT=Development `
--env ASPNETCORE_URLS=http://+:5000 `
${imageName}:${tagName}
