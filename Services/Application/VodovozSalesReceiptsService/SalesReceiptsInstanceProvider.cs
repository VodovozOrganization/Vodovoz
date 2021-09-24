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
		private readonly ISalesReceiptsServiceSettings _salesReceiptsServiceSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrderParametersProvider _orderParametersProvider;

		public SalesReceiptsInstanceProvider(
			ISalesReceiptsServiceSettings salesReceiptsServiceSettings,
			IOrderRepository orderRepository,
			IOrderParametersProvider orderParametersProvider)
		{
			_salesReceiptsServiceSettings =
				salesReceiptsServiceSettings ?? throw new ArgumentNullException(nameof(salesReceiptsServiceSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new SalesReceiptsService(_salesReceiptsServiceSettings, _orderRepository, _orderParametersProvider);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return GetInstance(instanceContext);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{ }

		#endregion IInstanceProvider implementation
	}
}
