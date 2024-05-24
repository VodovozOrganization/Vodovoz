using QS.Project.Journal;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Pacs
{
	public class OperatorNode : JournalNodeBase
	{
		public int Id { get; set; }
		public override string Title => Operator == null ? "" : Operator.GetPersonNameWithInitials();
		public Employee Operator { get; set; }
		public string WorkshiftName{ get; set; }
		public bool PacsEnabled { get; set; }
	}
}
