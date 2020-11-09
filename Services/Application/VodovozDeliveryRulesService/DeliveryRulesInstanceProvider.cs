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
		private readonly IBackupDistrictService backupDistrictService;

		public DeliveryRulesInstanceProvider(IDeliveryRepository deliveryRepository, IBackupDistrictService backupDistrictService)
		{
			this.deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			this.backupDistrictService = backupDistrictService ?? throw new ArgumentNullException(nameof(backupDistrictService));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new DeliveryRulesService(deliveryRepository, backupDistrictService);
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
