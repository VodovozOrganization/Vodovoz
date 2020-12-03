using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain {
    public interface INomenclatureFixedPriceProvider {
        bool ContainsFixedPrice(Order order, Nomenclature nomenclature);
        bool TryGetFixedPrice(Order order, Nomenclature nomenclature, out decimal fixedPrice);
        bool ContainsFixedPrice(Counterparty counterparty, Nomenclature nomenclature);
        bool TryGetFixedPrice(Counterparty counterparty, Nomenclature nomenclature, out decimal fixedPrice);
        bool ContainsFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature);
        bool TryGetFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, out decimal fixedPrice);
    }
}