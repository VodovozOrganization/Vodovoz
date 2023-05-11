using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain {
    public interface INomenclatureFixedPriceProvider {
        bool ContainsFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal bottlesCount);
        bool ContainsFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal bottlesCount);
    }
}
