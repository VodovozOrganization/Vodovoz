using Vodovoz.Parameters;

namespace Vodovoz.Models
{
    public class OrderOrganizationProviderFactory
    {
        public IOrganizationProvider CreateOrderOrganizationProvider()
        {
            var organizationParametersProvider = new OrganizationParametersProvider(ParametersProvider.Instance); 
            var orderParametersProvider = new OrderParametersProvider(ParametersProvider.Instance);
            return new Stage2OrganizationProvider(organizationParametersProvider, orderParametersProvider);
        }
    }
}