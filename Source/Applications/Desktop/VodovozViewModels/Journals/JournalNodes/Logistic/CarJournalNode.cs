using QS.Project.Journal;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CarJournalNode : JournalEntityNodeBase<Car>
	{
		public override string Title => $"{ManufacturerName} {ModelName} ({RegistrationNumber}) {DriverName}";

		public string CarOwner { get; set; }
		public string ModelName { get; set; }
		public string ManufacturerName { get; set; }
		public string RegistrationNumber { get; set; }
		public string DriverName { get; set; }
		public string Insurer { get; set; }
		public bool IsArchive { get; set; }
		public bool IsUpcomingTechInspect { get; set; }
		public bool IsOsagoInsuranceExpires { get; set; }
		public bool IsKaskoInsuranceExpires { get; set; }
		public bool IsShowBackgroundColorNotification { get; set; }
	}
}
