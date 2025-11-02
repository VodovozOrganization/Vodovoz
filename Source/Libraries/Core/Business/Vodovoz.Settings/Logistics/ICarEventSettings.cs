namespace Vodovoz.Settings.Logistics
{
	public interface ICarEventSettings
	{
		int CarEventStartNewPeriodDay { get; }
		int CompensationFromInsuranceByCourtId { get; }
		int TechInspectCarEventTypeId { get; }
		int CarTechnicalCheckupEventTypeId { get; }

		int CarTransferEventTypeId { get; }
		int CarReceptionEventTypeId { get; }
		int[] CarsExcludedFromReportsIds { get; }
		int FuelBalanceCalibrationCarEventTypeId { get; }
	}
}
