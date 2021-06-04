using System;
using Vodovoz.Services;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Vodovoz.EntityRepositories.Orders;

namespace VodovozSalesReceiptsService
{
	public class SalesReceiptsInstanceProvider : IInstanceProvider
	{
		private readonly ISalesReceiptsServiceSettings salesReceiptsServiceSettings;
		private readonly IOrderRepository orderRepository;
		private readonly IOrderParametersProvider orderParametersProvider;
		private readonly IOrganizationParametersProvider organizationParametersProvider;
		private readonly ISalesReceiptsParametersProvider _salesReceiptsParametersProvider;

		public SalesReceiptsInstanceProvider(
			ISalesReceiptsServiceSettings salesReceiptsServiceSettings,
			IOrderRepository orderRepository,
			IOrderParametersProvider orderParametersProvider,
			IOrganizationParametersProvider organizationParametersProvider,
			ISalesReceiptsParametersProvider salesReceiptsParametersProvider
			)
		{
			this.salesReceiptsServiceSettings = salesReceiptsServiceSettings ?? throw new ArgumentNullException(nameof(salesReceiptsServiceSettings));
			this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			this.orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			this.organizationParametersProvider = organizationParametersProvider ?? throw new ArgumentNullException(nameof(organizationParametersProvider));
			_salesReceiptsParametersProvider = salesReceiptsParametersProvider ?? throw new ArgumentNullException(nameof(salesReceiptsParametersProvider));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new SalesReceiptsService(salesReceiptsServiceSettings, orderRepository, orderParametersProvider, organizationParametersProvider, _salesReceiptsParametersProvider);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return GetInstance(instanceContext);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{
		}

		#endregion IInstanceProvider implementation
	}
}
