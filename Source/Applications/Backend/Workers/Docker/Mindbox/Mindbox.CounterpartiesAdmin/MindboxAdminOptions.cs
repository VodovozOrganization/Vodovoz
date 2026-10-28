using System;
using System.Collections.Generic;
using System.Globalization;
using Vodovoz.Domain.Client.Mindbox;

namespace Mindbox.CounterpartiesAdmin
{
	/// <summary>
	/// Параметры команд отчёта и повторной отправки
	/// </summary>
	public sealed class MindboxAdminOptions
	{
		public string Command { get; private set; }
		public List<MindboxCounterpartyOutboxStatus> Statuses { get; } = new List<MindboxCounterpartyOutboxStatus>();
		public List<int> CounterpartyIds { get; } = new List<int>();
		public string OutputFile { get; private set; }

		public static MindboxAdminOptions Parse(string[] args, out string error)
		{
			error = null;

			if(args.Length == 0)
			{
				error = "Не указана команда.";
				return null;
			}

			var options = new MindboxAdminOptions
			{
				Command = args[0].Trim().ToLowerInvariant()
			};

			if(options.Command != "report" && options.Command != "resend")
			{
				error = $"Неизвестная команда: {args[0]}. Ожидается report или resend.";
				return null;
			}

			for(var i = 1; i < args.Length; i++)
			{
				switch(args[i])
				{
					case "--status":
						if(++i >= args.Length || !TryParseStatuses(args[i], options.Statuses, out error))
						{
							return null;
						}

						break;
					case "--all-errors":
						options.Statuses.Clear();
						options.Statuses.Add(MindboxCounterpartyOutboxStatus.DataError);
						options.Statuses.Add(MindboxCounterpartyOutboxStatus.Error);

						break;
					case "--id":
						if(++i >= args.Length || !int.TryParse(args[i], out var id) || id <= 0)
						{
							error = "Не удалось разобрать --id.";
							return null;
						}

						options.CounterpartyIds.Add(id);

						break;
					case "--ids":
						if(++i >= args.Length || !TryParseIds(args[i], options.CounterpartyIds, out error))
						{
							return null;
						}

						break;
					case "--output":
						if(++i >= args.Length || string.IsNullOrWhiteSpace(args[i]))
						{
							error = "Не указан путь в --output.";
							return null;
						}

						options.OutputFile = args[i];

						break;
					case "--help":
					case "-h":
						error = null;
						return null;
					default:
						error = $"Неизвестный аргумент: {args[i]}";
						return null;
				}
			}

			if(options.Command == "report" && options.Statuses.Count == 0)
			{
				options.Statuses.Add(MindboxCounterpartyOutboxStatus.DataError);
				options.Statuses.Add(MindboxCounterpartyOutboxStatus.Error);
			}

			if(options.Command == "resend"
				&& options.CounterpartyIds.Count == 0
				&& options.Statuses.Count == 0)
			{
				error = "Для resend укажите --id, --ids или --status/--all-errors.";
				return null;
			}

			return options;
		}

		private static bool TryParseStatuses(
			string value,
			ICollection<MindboxCounterpartyOutboxStatus> statuses,
			out string error)
		{
			error = null;

			foreach(var part in value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if(part.Trim().Equals("all-errors", StringComparison.OrdinalIgnoreCase))
				{
					statuses.Add(MindboxCounterpartyOutboxStatus.DataError);
					statuses.Add(MindboxCounterpartyOutboxStatus.Error);
					continue;
				}

				if(!Enum.TryParse<MindboxCounterpartyOutboxStatus>(part.Trim(), true, out var status))
				{
					error = $"Неизвестный статус: {part}. Доступны: DataError, Error.";
					return false;
				}

				statuses.Add(status);
			}

			return true;
		}

		private static bool TryParseIds(string value, ICollection<int> ids, out string error)
		{
			error = null;

			foreach(var part in value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if(!int.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) || id <= 0)
				{
					error = $"Не удалось разобрать Id контрагента: {part}";
					return false;
				}

				ids.Add(id);
			}

			return true;
		}
	}
}
