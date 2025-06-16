using QS.Project.Journal;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.JournalNodes
{
    public class UserJournalNode : JournalEntityNodeBase<User>
    {
		public override string Title => Name;
		public string Name { get; set; }
		public string Login { get; set; }
		public int? EmployeeId { get; set; }
		public string EmployeeFIO { get; set; }
		public bool IsAdmin { get; set; }
		public bool Deactivated { get; set; }
	}
}
