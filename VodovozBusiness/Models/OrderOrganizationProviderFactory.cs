using Vodovoz.Parameters;

namespace Vodovoz.Models
{
    public class OrderOrganizationProviderFactory
    {
        public IOrganizationProvider CreateOrderOrganizationProvider()
        {
            var organizationParametersProvider = new OrganizationParametersProvider(ParametersProvider.Instance); 
            var orderParametersProvider = new OrderPrametersProvider(ParametersProvider.Instance); 
            return new Stage1OrganizationProvider(organizationParametersProvider, orderParametersProvider);
        }
    }
}