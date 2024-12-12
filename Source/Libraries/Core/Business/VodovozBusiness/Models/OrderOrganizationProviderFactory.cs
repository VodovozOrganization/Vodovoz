using Autofac;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Models
{
	public class OrderOrganizationProviderFactory : IOrderOrganizationProviderFactory
	{
		private readonly ILifetimeScope _scope;

		public OrderOrganizationProviderFactory(ILifetimeScope scope)
		{
			_scope = scope ?? throw new System.ArgumentNullException(nameof(scope));
		}

		public IOrganizationProvider CreateOrderOrganizationProvider()
		{
			return new Stage2OrganizationProvider(
				_scope.Resolve<IOrganizationSettings>(),
				_scope.Resolve<IOrderSettings>(),
				_scope.Resolve<IGeographicGroupSettings>(),
				_scope.Resolve<IFastPaymentRepository>(),
				_scope.Resolve<ICashReceiptRepository>());
		}
	}
}
