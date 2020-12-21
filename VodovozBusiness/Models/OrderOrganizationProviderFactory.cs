using Vodovoz.Parameters;

namespace Vodovoz.Models
{
    public class OrderOrganizationProviderFactory
    {
        public IOrganizationProvider CreateOrderOrganizationProvider()
        {
            var organizationParametersProvider = new OrganizationParametersProvider(ParametersProvider.Instance); 
            var orderParametersProvider = new OrderPrametersProvider(ParametersProvider.Instance);
            var financialDistrictProvider = new FinancialDistrictProvider(); 
            return new Stage2OrganizationProvider(organizationParametersProvider, orderParametersProvider, financialDistrictProvider);
        }
    }
}