using Fias.Service;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Services;

namespace VodovozDeliveryRulesService
{
	public class DeliveryRulesInstanceProvider : IInstanceProvider
	{
		private readonly IDeliveryRepository deliveryRepository;
		private readonly IBackupDistrictService backupDistrictService;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParameters;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly IFiasApiClient _fiasApiClient;

		public DeliveryRulesInstanceProvider(
			IDeliveryRepository deliveryRepository,
			IBackupDistrictService backupDistrictService,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			IFiasApiClient fiasApiClient,
			CancellationTokenSource cancellationTokenSource)
		{
			this.deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			this.backupDistrictService = backupDistrictService ?? throw new ArgumentNullException(nameof(backupDistrictService));
			_deliveryRulesParameters = deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_fiasApiClient = fiasApiClient ?? throw new ArgumentNullException(nameof(fiasApiClient));
			_cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new DeliveryRulesService(deliveryRepository, backupDistrictService, _deliveryRulesParameters, _fiasApiClient, _cancellationTokenSource);
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
