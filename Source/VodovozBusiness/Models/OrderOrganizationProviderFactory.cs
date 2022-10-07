using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Parameters;

namespace Vodovoz.Models
{
	public class OrderOrganizationProviderFactory
	{
		public IOrganizationProvider CreateOrderOrganizationProvider()
		{
			var parametersProvider = new ParametersProvider();
			var organizationParametersProvider = new OrganizationParametersProvider(parametersProvider);
			var orderParametersProvider = new OrderParametersProvider(parametersProvider);
			var geographicGroupParametersProvider = new GeographicGroupParametersProvider(parametersProvider);
			var fastPaymentRepository = new FastPaymentRepository();

			return new Stage2OrganizationProvider(
				organizationParametersProvider,
				orderParametersProvider,
				geographicGroupParametersProvider,
				fastPaymentRepository);
		}
	}
}
