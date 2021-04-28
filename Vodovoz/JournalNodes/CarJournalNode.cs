using QS.Project.Journal;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.JournalNodes
{
	public class CarJournalNode : JournalEntityNodeBase<Car>
	{
		public override string Title => $"{Model}({RegistrationNumber})  {DriverName}";

		public string Model { get; set; }
		public string RegistrationNumber { get; set; }
		public string DriverName { get; set; }
		public bool IsArchive { get; set; }
	}
}
