using System;
using Vodovoz.Settings.Car;

namespace Vodovoz.Settings.Database.Car
{
	public class CarInsuranceSettings : ICarInsuranceSettings
	{
		private readonly string _parametersPrefix = "CarInsurance.";
		private readonly ISettingsController _settingsController;

		public CarInsuranceSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int OsagoEndingNotifyDaysBefore =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(OsagoEndingNotifyDaysBefore)}");

		public int KaskoEndingNotifyDaysBefore =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(KaskoEndingNotifyDaysBefore)}");

		public void SetOsagoEndingNotifyDaysBefore(int value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(OsagoEndingNotifyDaysBefore)}", value.ToString());
		}

		public void SetKaskoEndingNotifyDaysBefore(int value)
		{
			_settingsController.CreateOrUpdateSetting($"{_parametersPrefix}{nameof(KaskoEndingNotifyDaysBefore)}", value.ToString());
		}
	}
}
