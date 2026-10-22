using System;
using System.Globalization;
using Vodovoz.Mindbox.Export;

namespace Mindbox.CounterpartiesExport
{
	/// <summary>
	/// Разбор аргументов командной строки
	/// </summary>
	public static class MindboxExportOptionsParser
	{
		public static MindboxExportOptions Parse(string[] args, out string error)
		{
			error = null;
			var options = new MindboxExportOptions();

			for(var i = 0; i < args.Length; i++)
			{
				switch(args[i])
				{
					case "--from":
						if(++i >= args.Length
							|| !DateTime.TryParseExact(
								args[i],
								"yyyy-MM-dd",
								CultureInfo.InvariantCulture,
								DateTimeStyles.None,
								out var fromDate))
						{
							error = "Не удалось разобрать дату в аргументе --from. Ожидается формат yyyy-MM-dd.";
							return null;
						}

						options.FromDate = fromDate;

						break;
					case "--dry-run":
						options.DryRun = true;
						break;
					case "--output":
						if(++i >= args.Length)
						{
							error = "Не указан путь к файлу в аргументе --output.";
							return null;
						}

						options.OutputFile = args[i];

						break;
					case "--batch-size":
						if(++i >= args.Length
							|| !int.TryParse(args[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var batchSize)
							|| batchSize <= 0)
						{
							error = "Не удалось разобрать размер пакета в аргументе --batch-size. Ожидается положительное целое число.";
							return null;
						}

						options.BatchSize = batchSize;

						break;
					default:
						error = $"Неизвестный аргумент: {args[i]}";
						return null;
				}
			}

			return options;
		}
	}
}
