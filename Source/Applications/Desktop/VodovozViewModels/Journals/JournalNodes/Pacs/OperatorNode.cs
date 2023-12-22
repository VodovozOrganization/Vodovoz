using QS.Project.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Pacs
{
	public class OperatorNode : JournalNodeBase
	{
		public override string Title => Operator == null ? "" : Operator.GetPersonNameWithInitials();
		public Employee Operator { get; set; }
		public string WorkshiftName{ get; set; }
	}
}
