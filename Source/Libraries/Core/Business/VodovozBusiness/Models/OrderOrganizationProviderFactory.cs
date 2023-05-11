using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Cash;
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
			var cashReceiptRepository = new CashReceiptRepository(UnitOfWorkFactory.GetDefaultFactory, orderParametersProvider);

			return new Stage2OrganizationProvider(
				organizationParametersProvider,
				orderParametersProvider,
				geographicGroupParametersProvider,
				fastPaymentRepository,
				cashReceiptRepository);
		}
	}
}
