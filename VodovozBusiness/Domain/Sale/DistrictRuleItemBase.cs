using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.Domain.Sale
{
    public abstract class DistrictRuleItemBase : PropertyChangedBase, IDomainObject, ICloneable
    {
        public virtual int Id { get; set; }

        DeliveryPriceRule deliveryPriceRule;
        [Display(Name = "Правило цены доставки")]
        public virtual DeliveryPriceRule DeliveryPriceRule {
            get => deliveryPriceRule;
            set => SetField(ref deliveryPriceRule, value, () => DeliveryPriceRule);
        }

        Decimal price;
        [Display(Name = "Цена доставки")]
        public virtual Decimal Price {
            get => price;
            set => SetField(ref price, value, () => Price);
        }

        public abstract object Clone();
    }
}