using QS.Project.Journal;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class ManufacturerCarsJournalNode : JournalEntityNodeBase<ManufacturerCars>
	{
		public override string Title => ManufacturerName;

		public string ManufacturerName { get; set; }
	}
}
