using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "результаты рассмотрения рекламации по клиенту",
		Nominative = "результат рассмотрения рекламации по клиенту")]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintResultOfCounterparty : ComplaintResultBase
	{
		public override string Title => $"Результат рассмотрения рекламации по клиенту №{Id}";
		public override ComplaintResultType ComplaintResultType => ComplaintResultType.Counterparty;
	}
}
