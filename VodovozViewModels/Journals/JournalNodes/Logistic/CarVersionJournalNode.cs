using System;
using QS.Project.Journal;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CarVersionJournalNode : JournalEntityNodeBase<CarVersion>
	{
		public override string Title => RegNumber;
		
		public string RegNumber { get; set; }
		
		public DateTime StartDate { get; set; }
		
		public DateTime EndDate { get; set; }
	}
}
