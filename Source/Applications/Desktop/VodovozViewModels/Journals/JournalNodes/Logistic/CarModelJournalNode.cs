using QS.Project.Journal;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CarModelJournalNode : JournalEntityNodeBase<CarModel>
	{
		public override string Title => $"{ManufactererName} {Name}";
		public string Name { get; set; }
		public CarTypeOfUse TypeOfUse { get; set; }
		public string ManufactererName { get; set; }
		public bool IsArchive { get; set; }
	}
}
