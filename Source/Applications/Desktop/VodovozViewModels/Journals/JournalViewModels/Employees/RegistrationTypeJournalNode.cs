using QS.Project.Journal;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class RegistrationTypeJournalNode : JournalNodeBase
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public override string Title => Name;
	}
}
