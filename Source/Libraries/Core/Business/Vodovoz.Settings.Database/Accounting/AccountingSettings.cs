using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vodovoz.Settings.Accounting;

namespace Vodovoz.Settings.Database.Accounting
{
	public class AccountingSettings : IAccountingSettings
	{
		private const string _accountingPeriodClosingDatesParameterName = "accounting_period_closing_dates";

		private readonly ISettingsController _settingsController;

		public AccountingSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}
		
		/// <inheritdoc/>
		public IEnumerable<DateTime> GetAccountingPeriodClosingDates()
		{
			var stringDates = _settingsController
				.GetValue<string>(_accountingPeriodClosingDatesParameterName)
				.Split(',');
			
			var dates = new List<DateTime>();

			foreach(var stringDate in stringDates)
			{
				if(!DateTime.TryParseExact(stringDate, "dd.MM", null, DateTimeStyles.None, out var parsedDate))
				{
					throw new InvalidOperationException(
						"Не удалось распарсить даты закрытия бухгалтерского периода (accounting_period_closing_dates) проверьте правильность формата!");
				}
				
				dates.Add(parsedDate);
			}

			if(!dates.Any())
			{
				throw new InvalidOperationException("Не найдены даты закрытия бухгалтерского периода (accounting_period_closing_dates)");
			}

			dates.Sort((x, y) =>
			{
				if(x.Month > y.Month) return 1;
				if(x.Month == y.Month) return 0;
				return -1;
			});

			return dates;
		}

		/// <inheritdoc/>
		public void UpdateAccountingPeriodClosingDates(DateTime firstQuarter, DateTime secondQuarter, DateTime thirdQuarter, DateTime fourthQuarter)
		{
			var value = string.Join(",",
				firstQuarter.ToString("dd.MM", CultureInfo.InvariantCulture),
				secondQuarter.ToString("dd.MM", CultureInfo.InvariantCulture),
				thirdQuarter.ToString("dd.MM", CultureInfo.InvariantCulture),
				fourthQuarter.ToString("dd.MM", CultureInfo.InvariantCulture));

			_settingsController.CreateOrUpdateSetting(_accountingPeriodClosingDatesParameterName, value);
		}
	}
}
