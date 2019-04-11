using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "причины невозврата имущества",
		Nominative = "причина невозврата имущества")]
	public class NonReturnReason : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
}