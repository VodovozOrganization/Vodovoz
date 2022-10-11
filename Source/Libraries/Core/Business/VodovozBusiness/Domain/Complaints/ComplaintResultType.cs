using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Complaints
{
	public enum ComplaintResultType
	{
		[Display(Name = "По клиенту")]
		Counterparty,
		[Display(Name = "По сотрудникам")]
		Employees
	}
}
