$DockerfileContext = Select-Xml -Path ".\Mango.Service.csproj" -XPath "/Project/PropertyGroup/DockerfileContext" | ForEach-Object { $_.Node.InnerXML } | Select-Object -First 1

Write-Host "DockerfileContext: $DockerfileContext"

$imageName = "mango.service"
$tagName = "latest"

docker build -f .\Dockerfile -t "${imageName}:${tagName}" $DockerfileContext

docker tag mango.service:latest docker.vod.qsolution.ru:5100/mango.service:latest
docker push docker.vod.qsolution.ru:5100/mango.service:latest