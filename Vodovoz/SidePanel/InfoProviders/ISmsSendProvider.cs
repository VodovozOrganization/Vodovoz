using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.SidePanel.InfoProviders
{
    public interface ISmsSendProvider: IInfoProvider
    {
        Counterparty Counterparty { get; }
        Order Order { get; }

    }
}