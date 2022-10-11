using QS.Project.Journal;
using Vodovoz.Domain.Employees;

namespace Vodovoz.JournalNodes
{
    public class UserJournalNode : JournalEntityNodeBase<User>
    {
		public override string Title => Name;

		public string Name { get; set; }
	
		public string Login { get; set; }

		public bool Deactivated { get; set; }

	}
}
