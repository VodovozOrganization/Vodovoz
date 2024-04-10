using System;
using Vodovoz.Settings.Fuel;

namespace Vodovoz.Settings.Database.Fuel
{
	public class FuelControlSettings : IFuelControlSettings
	{
		private readonly string _parametersPrefix = "FuelControl.";

		private readonly ISettingsController _settingsController;

		public FuelControlSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string ApiBaseAddress =>
			_settingsController.GetStringValue($"{_parametersPrefix}{nameof(ApiBaseAddress)}");

		public TimeSpan ApiSessionLifetime =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}{nameof(ApiSessionLifetime)}");

		public TimeSpan ApiRequesTimeout =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}{nameof(ApiRequesTimeout)}");

		public int TransactionsPerQueryLimit =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(TransactionsPerQueryLimit)}");

		public string OrganizationContractId =>
			_settingsController.GetStringValue($"{_parametersPrefix}{nameof(OrganizationContractId)}");

		public DateTime FuelTransactionsPerDayLastUpdateDate =>
			_settingsController.GetValue<DateTime>($"{_parametersPrefix}{nameof(FuelTransactionsPerDayLastUpdateDate)}");

		public DateTime FuelTransactionsPerMonthLastUpdateDate =>
			_settingsController.GetValue<DateTime>($"{_parametersPrefix}{nameof(FuelTransactionsPerMonthLastUpdateDate)}");

		public void SetFuelTransactionsPerDayLastUpdateDate(string value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(FuelTransactionsPerDayLastUpdateDate)}", value);
		}

		public void SetFuelTransactionsPerMonthLastUpdateDate(string value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(FuelTransactionsPerMonthLastUpdateDate)}", value);
		}
	}
}
