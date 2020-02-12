using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "место, откуда проведены оплаты",
		Nominative = "место, откуда проведена оплата")]
	[HistoryTrace]
	[EntityPermission]
	public class PaymentFrom : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
}