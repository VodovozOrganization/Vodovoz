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
			_settingsController.GetDateTimeValue($"{_parametersPrefix}{nameof(FuelTransactionsPerDayLastUpdateDate)}");

		public DateTime FuelTransactionsPerMonthLastUpdateDate =>
			_settingsController.GetDateTimeValue($"{_parametersPrefix}{nameof(FuelTransactionsPerMonthLastUpdateDate)}");

		public string FuelProductTypeId =>
			_settingsController.GetStringValue($"{_parametersPrefix}{nameof(FuelProductTypeId)}");

		public string LiterUnitId =>
			_settingsController.GetStringValue($"{_parametersPrefix}{nameof(LiterUnitId)}");

		public string RubleCurrencyId =>
			_settingsController.GetStringValue($"{_parametersPrefix}{nameof(RubleCurrencyId)}");

		public int LargusMaxDailyFuelLimit =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(LargusMaxDailyFuelLimit)}");

		public int TruckMaxDailyFuelLimit =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(TruckMaxDailyFuelLimit)}");

		public int GAZelleMaxDailyFuelLimit =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(GAZelleMaxDailyFuelLimit)}");

		public int LoaderMaxDailyFuelLimit =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(LoaderMaxDailyFuelLimit)}");

		public int MinivanMaxDailyFuelLimit =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(MinivanMaxDailyFuelLimit)}");
		
		public int LargusFuelLimitMaxTransactionsCount =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(LargusFuelLimitMaxTransactionsCount)}");

		public int GAZelleFuelLimitMaxTransactionsCount =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(GAZelleFuelLimitMaxTransactionsCount)}");

		public int TruckFuelLimitMaxTransactionsCount =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(TruckFuelLimitMaxTransactionsCount)}");

		public int LoaderFuelLimitMaxTransactionsCount =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(LoaderFuelLimitMaxTransactionsCount)}");

		public int MinivanFuelLimitMaxTransactionsCount =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(MinivanFuelLimitMaxTransactionsCount)}");

		public DateTime FuelPricesLastUpdateDate =>
			_settingsController.GetDateTimeValue($"{_parametersPrefix}{nameof(FuelPricesLastUpdateDate)}");

		public void SetFuelTransactionsPerDayLastUpdateDate(string value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(FuelTransactionsPerDayLastUpdateDate)}", value);
		}

		public void SetFuelTransactionsPerMonthLastUpdateDate(string value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(FuelTransactionsPerMonthLastUpdateDate)}", value);
		}

		public void SetLargusMaxDailyFuelLimit(int value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(LargusMaxDailyFuelLimit)}", value.ToString());
		}

		public void SetTruckMaxDailyFuelLimit(int value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(TruckMaxDailyFuelLimit)}", value.ToString());
		}

		public void SetGAZelleMaxDailyFuelLimit(int value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(GAZelleMaxDailyFuelLimit)}", value.ToString());
		}

		public void SetLoaderMaxDailyFuelLimit(int value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(LoaderMaxDailyFuelLimit)}", value.ToString());
		}

		public void SetMinivanMaxDailyFuelLimit(int value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(MinivanMaxDailyFuelLimit)}", value.ToString());
		}

		public void SetFuelPricesLastUpdateDate(string value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(FuelPricesLastUpdateDate)}", value);
		}
	}
}
