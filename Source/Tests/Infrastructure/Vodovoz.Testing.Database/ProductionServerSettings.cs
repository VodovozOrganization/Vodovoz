using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Настройки прода из prod-server.cnf: параметры запуска и сверка запущенного сервера с ними.
	/// </summary>
	public class ProductionServerSettings
	{
		private const string _startupSection = "mysqld";
		private const string _verifySection = "verify";
		private const string _warningSection = "verify-warning";

		private readonly Dictionary<string, List<KeyValuePair<string, string>>> _sections;

		private ProductionServerSettings(Dictionary<string, List<KeyValuePair<string, string>>> sections)
		{
			_sections = sections;
		}

		public IReadOnlyList<KeyValuePair<string, string>> StartupVariables => Section(_startupSection);

		public static ProductionServerSettings Load()
		{
			var assembly = typeof(ProductionServerSettings).Assembly;

			using(var stream = assembly.GetManifestResourceStream("prod-server.cnf"))
			using(var reader = new StreamReader(stream))
			{
				return Parse(reader.ReadToEnd());
			}
		}

		public static ProductionServerSettings Parse(string content)
		{
			var sections = new Dictionary<string, List<KeyValuePair<string, string>>>(StringComparer.OrdinalIgnoreCase);
			List<KeyValuePair<string, string>> current = null;

			foreach(var rawLine in content.Split('\n'))
			{
				var line = rawLine.Trim();

				if(line.Length == 0 || line.StartsWith("#"))
				{
					continue;
				}

				if(line.StartsWith("[") && line.EndsWith("]"))
				{
					current = new List<KeyValuePair<string, string>>();
					sections[line.Substring(1, line.Length - 2)] = current;
					continue;
				}

				var separator = line.IndexOf('=');
				current.Add(new KeyValuePair<string, string>(line.Substring(0, separator).Trim(), line.Substring(separator + 1).Trim()));
			}

			return new ProductionServerSettings(sections);
		}

		/// <summary>
		/// Сверяет запущенный сервер с настройками прода.
		/// Расхождение по известным отличиям возвращается как предупреждение, по остальным переменным — исключение.
		/// </summary>
		public IList<string> Verify(MySqlConnection connection)
		{
			var errors = new List<string>();
			var version = Read(connection, "VERSION()");

			if(!version.StartsWith(MariaDbVersion.Version + "-", StringComparison.Ordinal))
			{
				errors.Add($"версия: ожидалась {MariaDbVersion.Version}, на сервере {version}");
			}

			foreach(var variable in Section(_startupSection).Concat(Section(_verifySection)))
			{
				if(string.Equals(variable.Key, "default_time_zone", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				var actual = Read(connection, $"@@global.{variable.Key}");

				if(!AreEqual(variable.Key, variable.Value, actual))
				{
					errors.Add($"{variable.Key}: ожидалось {variable.Value}, на сервере {actual}");
				}
			}

			if(errors.Count > 0)
			{
				throw new InvalidOperationException("Настройки тестовой MariaDB не совпадают с продом: " + string.Join("; ", errors));
			}

			return Section(_warningSection)
				.Select(variable => new { variable, actual = Read(connection, $"@@global.{variable.Key}") })
				.Where(x => !AreEqual(x.variable.Key, x.variable.Value, x.actual))
				.Select(x => $"Известное отличие от прода: {x.variable.Key} = {x.actual}, на проде {x.variable.Value}")
				.ToList();
		}

		private IReadOnlyList<KeyValuePair<string, string>> Section(string name) =>
			_sections.TryGetValue(name, out var section) ? section : new List<KeyValuePair<string, string>>();

		private static string Read(MySqlConnection connection, string expression)
		{
			using(var command = connection.CreateCommand())
			{
				command.CommandText = $"SELECT {expression}";
				return Convert.ToString(command.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		private static bool AreEqual(string name, string expected, string actual)
		{
			if(string.Equals(name, "sql_mode", StringComparison.OrdinalIgnoreCase))
			{
				return new HashSet<string>(expected.Split(','), StringComparer.OrdinalIgnoreCase)
					.SetEquals(actual.Split(','));
			}

			return string.Equals(Normalize(expected), Normalize(actual), StringComparison.OrdinalIgnoreCase);
		}

		private static string Normalize(string value)
		{
			if(string.Equals(value, "ON", StringComparison.OrdinalIgnoreCase))
			{
				return "1";
			}

			if(string.Equals(value, "OFF", StringComparison.OrdinalIgnoreCase))
			{
				return "0";
			}

			return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
				? number.ToString("0.######", CultureInfo.InvariantCulture)
				: value;
		}
	}
}
