using Vodovoz.Domain.Client;

namespace Vodovoz.SidePanel.InfoProviders
{
    public interface IFixedPricesHolderProvider : IInfoProvider
    {
        Counterparty Counterparty { get; }
        DeliveryPoint DeliveryPoint { get; }
    }
}