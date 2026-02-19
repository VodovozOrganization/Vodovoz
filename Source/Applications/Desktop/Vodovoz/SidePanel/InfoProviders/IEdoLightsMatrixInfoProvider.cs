using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.SidePanel.InfoProviders
{
    public interface IEdoLightsMatrixInfoProvider : IInfoProvider
    {
        Counterparty Counterparty { get; }
        Organization EdoLightMatrxiOrganization { get; }
    }
}
