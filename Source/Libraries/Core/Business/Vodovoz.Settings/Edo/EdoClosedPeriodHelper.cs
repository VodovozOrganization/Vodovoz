using System;
using System.Globalization;

namespace Vodovoz.Settings.Edo
{
	public static class EdoClosedPeriodHelper
	{
		private static readonly char[] _separators = { '.', '/', '-', ' ' };
		private static readonly string[] _parseFormats = { "d.M", "dd.M", "d.MM", "dd.MM" };

		public static bool TryParseDayMonth(string value, out int day, out int month)
		{
			day = 0;
			month = 0;

			if(string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			value = value.Trim();
			foreach(var separator in _separators)
			{
				value = value.Replace(separator, '.');
			}

			if(!DateTime.TryParseExact(value, _parseFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
			{
				return false;
			}

			day = parsed.Day;
			month = parsed.Month;
			return true;
		}

		public static string ToDayMonthString(int day, int month)
		{
			return $"{day:00}.{month:00}";
		}

		public static DateTime GetQuarterClosingDate(
			DateTime accountingDate,
			(int Day, int Month) q1Closing,
			(int Day, int Month) q2Closing,
			(int Day, int Month) q3Closing,
			(int Day, int Month) q4Closing)
		{
			var quarter = (accountingDate.Month - 1) / 3 + 1;

			(int Day, int Month) template;
			var year = accountingDate.Year;

			switch(quarter)
			{
				case 1:
					template = q1Closing;
					break;
				case 2:
					template = q2Closing;
					break;
				case 3:
					template = q3Closing;
					break;
				default:
					template = q4Closing;
					year += 1;
					break;
			}

			var day = Math.Min(template.Day, DateTime.DaysInMonth(year, template.Month));
			return new DateTime(year, template.Month, day);
		}

		public static bool IsClosedPeriod(DateTime accountingDate, DateTime closingDate, DateTime today)
		{
			return today.Date >= closingDate.Date;
		}
	}
}
