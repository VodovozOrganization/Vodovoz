using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "категория номенклатуры основания скидки",
		Nominative = "категория номенклатуры основания скидки")]
	public class DiscountReasonNomenclatureCategory : PropertyChangedBase, IDomainObject
	{
		private NomenclatureCategory _nomenclatureCategory;
		
		public virtual int Id { get; set; }

		public virtual NomenclatureCategory NomenclatureCategory
		{
			get => _nomenclatureCategory;
			set => SetField(ref _nomenclatureCategory, value);
		}
	}
}
