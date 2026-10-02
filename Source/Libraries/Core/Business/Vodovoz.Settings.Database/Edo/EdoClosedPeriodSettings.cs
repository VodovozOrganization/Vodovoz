using System;
using Vodovoz.Settings.Edo;

namespace Vodovoz.Settings.Database.Edo
{
	public class EdoClosedPeriodSettings : IEdoClosedPeriodSettings
	{
		private const string _q1ClosingKey = "Edo.ClosedPeriod.Q1Closing";
		private const string _q2ClosingKey = "Edo.ClosedPeriod.Q2Closing";
		private const string _q3ClosingKey = "Edo.ClosedPeriod.Q3Closing";
		private const string _q4ClosingKey = "Edo.ClosedPeriod.Q4Closing";

		private static readonly (int Day, int Month) _defaultQ1Closing = (20, 4);
		private static readonly (int Day, int Month) _defaultQ2Closing = (20, 7);
		private static readonly (int Day, int Month) _defaultQ3Closing = (20, 10);
		private static readonly (int Day, int Month) _defaultQ4Closing = (20, 1);

		private readonly ISettingsController _settingsController;

		public EdoClosedPeriodSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public (int Day, int Month) Q1ClosingDayMonth => GetDayMonth(_q1ClosingKey, _defaultQ1Closing);
		public (int Day, int Month) Q2ClosingDayMonth => GetDayMonth(_q2ClosingKey, _defaultQ2Closing);
		public (int Day, int Month) Q3ClosingDayMonth => GetDayMonth(_q3ClosingKey, _defaultQ3Closing);
		public (int Day, int Month) Q4ClosingDayMonth => GetDayMonth(_q4ClosingKey, _defaultQ4Closing);

		public void UpdateQ1ClosingDayMonth(int day, int month) => UpdateDayMonth(_q1ClosingKey, day, month);
		public void UpdateQ2ClosingDayMonth(int day, int month) => UpdateDayMonth(_q2ClosingKey, day, month);
		public void UpdateQ3ClosingDayMonth(int day, int month) => UpdateDayMonth(_q3ClosingKey, day, month);
		public void UpdateQ4ClosingDayMonth(int day, int month) => UpdateDayMonth(_q4ClosingKey, day, month);

		public DateTime GetClosingDate(DateTime accountingDate)
		{
			return EdoClosedPeriodHelper.GetQuarterClosingDate(
				accountingDate,
				Q1ClosingDayMonth,
				Q2ClosingDayMonth,
				Q3ClosingDayMonth,
				Q4ClosingDayMonth);
		}

		public bool IsClosedPeriod(DateTime accountingDate, DateTime? today = null)
		{
			return EdoClosedPeriodHelper.IsClosedPeriod(accountingDate, GetClosingDate(accountingDate), today ?? DateTime.Today);
		}

		private (int Day, int Month) GetDayMonth(string key, (int Day, int Month) defaultValue)
		{
			if(!_settingsController.ContainsSetting(key))
			{
				return defaultValue;
			}

			string rawValue;
			try
			{
				rawValue = _settingsController.GetStringValue(key);
			}
			catch(SettingException)
			{
				return defaultValue;
			}

			if(!EdoClosedPeriodHelper.TryParseDayMonth(rawValue, out var day, out var month))
			{
				return defaultValue;
			}

			return (day, month);
		}

		private void UpdateDayMonth(string key, int day, int month)
		{
			if(month < 1 || month > 12)
			{
				throw new ArgumentOutOfRangeException(nameof(month), "Месяц должен быть от 1 до 12");
			}

			if(day < 1 || day > DateTime.DaysInMonth(2001, month))
			{
				throw new ArgumentOutOfRangeException(nameof(day), "Некорректный день для указанного месяца");
			}

			_settingsController.CreateOrUpdateSetting(key, EdoClosedPeriodHelper.ToDayMonthString(day, month));
		}
	}
}
