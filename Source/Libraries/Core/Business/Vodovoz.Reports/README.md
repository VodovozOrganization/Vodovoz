# Отчеты
Отчеты хранятся в отдельном Shared проекте для того чтобы их можно было использовать в разных проектах не имея лишние копии отчетов.

## Использование в проектах
### .NET Framework
Чтобы использовать файлы отчетов в проектах для фреймворка необходимо в csproj файл добавить конструкцию как примере ниже, которая позволит копировать файлы отчетов в выходной каталог.
```XML
<ItemGroup>
	<ContentWithTargetPath  Include="..\..\..\Libraries\Core\Business\Vodovoz.Reports\Reports\**">
		<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
		<TargetPath>Reports\%(RecursiveDir)\%(Filename)%(Extension)</TargetPath>
	</ContentWithTargetPath>
</ItemGroup>
```
### .NET / .NET Core
Чтобы использовать файлы отчетов в проектах для фреймворка необходимо просто добавить reference на shared проект в своем проекте. После чего при сборке файлы автоматически будут копироваться в выходной каталог в соответствии с настройками файлов определенными в shared проекте.
