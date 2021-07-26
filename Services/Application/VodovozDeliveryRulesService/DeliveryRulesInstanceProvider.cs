using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Vodovoz.EntityRepositories.Sectors;
using Vodovoz.Services;

namespace VodovozDeliveryRulesService
{
	public class DeliveryRulesInstanceProvider : IInstanceProvider
	{
		private readonly ISectorsRepository _sectorsRepository;
		private readonly IBackupDistrictService backupDistrictService;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParameters;

		public DeliveryRulesInstanceProvider(
			ISectorsRepository sectorsRepository,
			IBackupDistrictService backupDistrictService,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider)
		{
			_sectorsRepository = sectorsRepository ?? throw new ArgumentNullException(nameof(sectorsRepository));
			this.backupDistrictService = backupDistrictService ?? throw new ArgumentNullException(nameof(backupDistrictService));
			_deliveryRulesParameters = deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new DeliveryRulesService(_sectorsRepository, backupDistrictService, _deliveryRulesParameters);
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
