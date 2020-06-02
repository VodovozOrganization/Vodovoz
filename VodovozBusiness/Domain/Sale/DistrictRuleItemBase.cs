using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Sale
{
    public abstract class DistrictRuleItemBase : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }

        District district;
        [Display(Name = "Район доставки")]
        public virtual District District {
            get => district;
            set => SetField(ref district, value, () => District);
        }

        DeliveryPriceRule deliveryPriceRule;
        [Display(Name = "Правило цены доставки")]
        public virtual DeliveryPriceRule DeliveryPriceRule {
            get => deliveryPriceRule;
            set => SetField(ref deliveryPriceRule, value, () => DeliveryPriceRule);
        }

        Decimal deliveryPrice;
        [Display(Name = "Цена доставки")]
        public virtual Decimal DeliveryPrice {
            get => deliveryPrice;
            set => SetField(ref deliveryPrice, value, () => DeliveryPrice);
        }

    }
}