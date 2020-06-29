using System;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace VodovozSalesReceiptsService
{
	public class SalesReceiptsInstanceProvider : IInstanceProvider
	{
		private readonly ISalesReceiptsServiceSettings salesReceiptsServiceSettings;
		private readonly IOrderRepository orderRepository;

		public SalesReceiptsInstanceProvider(ISalesReceiptsServiceSettings salesReceiptsServiceSettings, IOrderRepository orderRepository)
		{
			this.salesReceiptsServiceSettings = salesReceiptsServiceSettings ?? throw new ArgumentNullException(nameof(salesReceiptsServiceSettings));
			this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new SalesReceiptsService(salesReceiptsServiceSettings, orderRepository);
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
