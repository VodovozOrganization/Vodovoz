using QS.Project.Journal;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CarManufacturerJournalNode : JournalEntityNodeBase<CarManufacturer>
	{
		public override string Title => Name;
		public string Name { get; set; }
	}
}
