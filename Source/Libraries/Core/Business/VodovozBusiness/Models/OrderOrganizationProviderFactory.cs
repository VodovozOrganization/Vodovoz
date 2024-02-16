using Autofac;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Parameters;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Models
{
	public class OrderOrganizationProviderFactory
	{
		private readonly ILifetimeScope _scope;

		public OrderOrganizationProviderFactory(ILifetimeScope scope)
		{
			_scope = scope ?? throw new System.ArgumentNullException(nameof(scope));
		}

		public IOrganizationProvider CreateOrderOrganizationProvider()
		{
			var parametersProvider = new ParametersProvider();
			var geographicGroupParametersProvider = new GeographicGroupParametersProvider(parametersProvider);
			var fastPaymentRepository = new FastPaymentRepository();

			return new Stage2OrganizationProvider(
				_scope.Resolve<IOrganizationSettings>(),
				_scope.Resolve<IOrderSettings>(),
				geographicGroupParametersProvider,
				fastPaymentRepository,
				_scope.Resolve<ICashReceiptRepository>());
		}
	}
}
