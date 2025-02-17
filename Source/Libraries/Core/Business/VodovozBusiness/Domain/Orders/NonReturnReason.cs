using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "причины невозврата имущества",
		Nominative = "причина невозврата имущества")]
	[HistoryTrace]
	[EntityPermission]
	public class NonReturnReason : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		
		/// <summary>
		/// Необходимо начислять неустойку
		/// </summary>
		public virtual bool NeedForfeit { get; set; }
	}
}