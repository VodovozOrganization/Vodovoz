using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.TrueMark
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "код честного знака",
			Nominative = "код честного знака"
		)
	]
	public class TrueMarkWaterIdentificationCodeEntity : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }
	}
}
