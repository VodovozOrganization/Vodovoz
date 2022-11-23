using Vodovoz.Domain.Client;

namespace Vodovoz.SidePanel.InfoProviders
{
    public interface IEdoLightsMatrixInfoProvider : IInfoProvider
    {
        Counterparty Counterparty { get; }
    }
}
