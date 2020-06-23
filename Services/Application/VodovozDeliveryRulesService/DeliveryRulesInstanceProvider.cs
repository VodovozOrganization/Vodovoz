using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Vodovoz.EntityRepositories.Delivery;

namespace VodovozDeliveryRulesService
{
	public class DeliveryRulesInstanceProvider : IInstanceProvider
	{
		private readonly IDeliveryRepository deliveryRepository;

		public DeliveryRulesInstanceProvider(IDeliveryRepository deliveryRepository)
		{
			this.deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new DeliveryRulesService(deliveryRepository);
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
