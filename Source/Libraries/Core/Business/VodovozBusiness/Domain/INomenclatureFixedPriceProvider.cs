using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain {
    public interface INomenclatureFixedPriceProvider {
        //bool ContainsFixedPrice(Order order, Nomenclature nomenclature);
        //bool TryGetFixedPrice(Order order, Nomenclature nomenclature, out decimal fixedPrice);
        bool ContainsFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal bottlesCount);
        bool TryGetFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal bottlesCount, out decimal fixedPrice);
        bool ContainsFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal bottlesCount);
        bool TryGetFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal bottlesCount, out decimal fixedPrice);
    }
}
