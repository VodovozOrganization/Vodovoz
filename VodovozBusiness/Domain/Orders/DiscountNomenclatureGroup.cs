using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "группы товаров в основании скидки",
		Nominative = "группа товаров в основании скидки")]
	[EntityPermission]
	public class DiscountNomenclatureGroup : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		ProductGroup _productGroup;

		[Display(Name = "Группа товара")]
		public virtual ProductGroup ProductGroup
		{
			get => _productGroup;
			set => SetField(ref _productGroup, value);
		}

		DiscountReason _discountReason;

		[Display(Name = "Основание скидки")]
		public virtual DiscountReason DiscountReason
		{
			get => _discountReason;
			set => SetField(ref _discountReason, value);
		}
	}

}
