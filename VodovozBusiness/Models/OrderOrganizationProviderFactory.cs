using Vodovoz.Parameters;

namespace Vodovoz.Models
{
	public class OrderOrganizationProviderFactory
	{
		public IOrganizationProvider CreateOrderOrganizationProvider()
		{
			var organizationParametersProvider = new OrganizationParametersProvider(SingletonParametersProvider.Instance);
			var orderParametersProvider = new OrderParametersProvider(SingletonParametersProvider.Instance);
			var geographicGroupParametersProvider = new GeographicGroupParametersProvider(SingletonParametersProvider.Instance);

			return new Stage2OrganizationProvider(
				organizationParametersProvider,
				orderParametersProvider,
				geographicGroupParametersProvider
			);
		}
	}
}
