using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class CarTechInspectNode
	{
		public CarTypeOfUse CarTypeOfUse { get; set; }
		public string CarRegNumber { get; set; }
		public string DriverGeography { get; set; }
		public OdometerReading LastOdometerReading { get; set; }
		public int? LastTechInspectOdometer { get; set; }
		public int TeсhInspectInterval { get; set; }
		public int LeftUntilTechInspectKm { get; set; }
		public int? ManualTechInspectForKm { get; set; }
		public int UpcomingTechInspectKm =>
			ManualTechInspectForKm.HasValue
			? ManualTechInspectForKm.Value
			: LastTechInspectOdometer.HasValue
				? LastTechInspectOdometer.Value + TeсhInspectInterval
				: TeсhInspectInterval;
	}
}
