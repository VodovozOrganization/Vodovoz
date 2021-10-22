using QS.Project.Journal;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.JournalNodes
{
	public class CarModelJournalNode: JournalEntityNodeBase<CarModel>
	{
		public override string Title => $"{Name}({Type} - {ManufacturedCars})";

		public string Name { get; set; }
		public string Type { get; set; }
		
		public string ManufacturedCars { get; set; }
	}
}
