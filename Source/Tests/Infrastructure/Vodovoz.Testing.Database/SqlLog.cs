using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Журнал команд и точка вмешательства в момент перед выполнением команды.
	/// Тесты с БД идут последовательно, поэтому журнал общий.
	/// </summary>
	public static class SqlLog
	{
		private static readonly object _lock = new object();
		private static readonly List<LoggedCommand> _commands = new List<LoggedCommand>();

		/// <summary>
		/// Вызывается перед выполнением каждой команды. Заменяет триггеры БД там, где тесту нужно вмешаться в ход вставки.
		/// </summary>
		public static Action<DbCommand> BeforeCommand { get; set; }

		public static IReadOnlyList<LoggedCommand> Commands
		{
			get
			{
				lock(_lock)
				{
					return _commands.ToList();
				}
			}
		}

		public static void Clear()
		{
			lock(_lock)
			{
				_commands.Clear();
			}
		}

		public static void Reset()
		{
			Clear();
			BeforeCommand = null;
		}

		public static int Count(string prefix, string table) =>
			Commands.Count(x => x.Text.TrimStart().StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
				&& x.Text.IndexOf(table, StringComparison.OrdinalIgnoreCase) >= 0);

		public static int Inserts(string table) => Count("INSERT", table);

		public static int Updates(string table) => Count("UPDATE", table);

		public static int Selects(string table) => Count("SELECT", table);

		public static int Writes() => Commands.Count(x => IsWrite(x.Text));

		internal static void Record(DbCommand command)
		{
			lock(_lock)
			{
				_commands.Add(new LoggedCommand(command.CommandText, command.Connection, command.Transaction));
			}

			BeforeCommand?.Invoke(command);
		}

		private static bool IsWrite(string text)
		{
			var trimmed = text.TrimStart();

			return trimmed.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase)
				|| trimmed.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase)
				|| trimmed.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase);
		}
	}
}
