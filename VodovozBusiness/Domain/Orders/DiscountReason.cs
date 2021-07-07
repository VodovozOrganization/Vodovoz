using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "основания скидок",
		Nominative = "основание скидки")]
	[EntityPermission]
	public class DiscountReason : IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual string Name { get; set; }
	}
}
