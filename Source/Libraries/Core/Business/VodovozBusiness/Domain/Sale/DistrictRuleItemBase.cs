using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Sale
{
	public abstract class DistrictRuleItemBase : PropertyChangedBase, IDomainObject, ICloneable
	{
		private District _district;
		private DeliveryPriceRule _deliveryPriceRule;
		private decimal _price;

		public virtual int Id { get; set; }

		[Display(Name = "Район доставки")]
		public virtual District District
		{
			get => _district;
			set => SetField(ref _district, value);
		}

		[Display(Name = "Правило цены доставки")]
		public virtual DeliveryPriceRule DeliveryPriceRule
		{
			get => _deliveryPriceRule;
			set => SetField(ref _deliveryPriceRule, value);
		}

		[Display(Name = "Цена доставки")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}

		public abstract object Clone();
	}
}
