using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain {
    public class NomenclatureFixedPrice : DomainObjectBase {
        
        Nomenclature nomenclature;
        [Display (Name = "Номенклатура")]
        public virtual Nomenclature Nomenclature {
            get => nomenclature;
            set => SetField (ref nomenclature, value);
        }
        
        DeliveryPoint deliveryPoint;
        [Display(Name = "Точка доставки")]
        public virtual DeliveryPoint DeliveryPoint {
            get => deliveryPoint;
            set => SetField(ref deliveryPoint, value);
        }
        
        Counterparty counterparty;
        [Display(Name = "Клиент")]
        public virtual Counterparty Counterparty {
            get => counterparty;
            set => SetField(ref counterparty, value);
        }

        decimal fixedPrice;
        [Display (Name = "Цена")]
        public virtual decimal FixedPrice {
            get => fixedPrice;
            set => SetField (ref fixedPrice, value);
        }
    }
}