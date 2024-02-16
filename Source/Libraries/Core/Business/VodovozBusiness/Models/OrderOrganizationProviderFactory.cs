using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Parameters;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Models
{
	public class OrderOrganizationProviderFactory
	{
		private readonly IOrganizationSettings _organizationSettings;

		public OrderOrganizationProviderFactory(IOrganizationSettings organizationSettings)
		{
			_organizationSettings = organizationSettings ?? throw new System.ArgumentNullException(nameof(organizationSettings));
		}

		public IOrganizationProvider CreateOrderOrganizationProvider()
		{
			var parametersProvider = new ParametersProvider();
			var orderParametersProvider = new OrderParametersProvider(parametersProvider);
			var geographicGroupParametersProvider = new GeographicGroupParametersProvider(parametersProvider);
			var fastPaymentRepository = new FastPaymentRepository();
			var cashReceiptRepository = new CashReceiptRepository(ServicesConfig.UnitOfWorkFactory, orderParametersProvider);

			return new Stage2OrganizationProvider(
				_organizationSettings,
				orderParametersProvider,
				geographicGroupParametersProvider,
				fastPaymentRepository,
				cashReceiptRepository);
		}
	}
}
