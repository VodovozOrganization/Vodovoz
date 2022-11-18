using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "результаты рассмотрения рекламации по сотрудникам",
		Nominative = "результат рассмотрения рекламации по сотрудникам")]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintResultOfEmployees : ComplaintResultBase
	{
		public override string Title => $"Результат рассмотрения рекламации по сотрудникам №{Id}";
		public override ComplaintResultType ComplaintResultType => ComplaintResultType.Employees;
	}
}
