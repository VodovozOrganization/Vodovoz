using System;
using System.Linq;
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
		public int TechInspectCarEventTypeId => _settingsController.GetIntValue(nameof(TechInspectCarEventTypeId));
		public int CarTechnicalCheckupEventTypeId => _settingsController.GetIntValue("CarTechnicalCheckup.EventTypeId");

		public int CarTransferEventTypeId => _settingsController.GetIntValue(nameof(CarTransferEventTypeId));
		public int CarReceptionEventTypeId => _settingsController.GetIntValue(nameof(CarReceptionEventTypeId));
		public int[] CarsExcludedFromReportsIds => _settingsController
			.GetStringValue(nameof(CarsExcludedFromReportsIds))
			.Split(new [] { "," }, StringSplitOptions.RemoveEmptyEntries)
			.Select(x => int.Parse(x.Trim(' ')))
			.ToArray();

		public int[] AllowedCarEventTypeIdsForDriverSchedule => _settingsController
			.GetStringValue(nameof(AllowedCarEventTypeIdsForDriverSchedule))
			.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
			.Select(x => int.Parse(x.Trim(' ')))
			.ToArray();

		public int FuelBalanceCalibrationCarEventTypeId => _settingsController.GetIntValue(nameof(FuelBalanceCalibrationCarEventTypeId));
	}
}
