using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Logistics;

namespace Vodovoz.Settings.Database.Logistics
{
	public class CarEventSettings : ICarEventSettings
	{
		private readonly ISettingsController _settingsController;

		public CarEventSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int CompensationFromInsuranceByCourtId => _settingsController.GetIntValue("compensation_from_insurance_by_court_id");
		public int CarEventStartNewPeriodDay => _settingsController.GetIntValue("CarEventStartNewPeriodDay");
	}
}
