Профили публикации оптимизированные под запуск как из UI так и из консоли
Скрипт публикации через консоль

Протестировано на:
-<Project Sdk="Microsoft.NET.Sdk.Worker">
-<Project Sdk="Microsoft.NET.Sdk.Web">

Желательно убрать из проекта: Microsoft.NET.Sdk.Publish

Что делать:
Скопировать файлы из Project в свой проект.
Поменять имя образа в RepositorySettings.props
Убрать из проекта свойства:
	-<ContainerRepository>
	-<ContainerRegistry>
	-<ContainerImageTag>
	-и другие которые совпадают с профилем публикации (в проекте не нужно ничего указывать что относиться к публикации)
Остальные скопированные файлы менять не нужно
Дополнительные свойства, например ContainerBaseImage, следует добавлять в RepositorySettings.props

Если DockerFile специально не используется (если не знаешь, значит не используется), то желательно: 
убрать DockerFile
убрать свойства связанные с DockerFile:
  - <DockerDefaultTargetOS>
  - <DockerfileContext>
Это не влияет на текущую публикацию, просто чтобы не было не используемого мусора.
