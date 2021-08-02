using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using SmsPaymentService;
using Vodovoz.Services;

namespace VodovozSmsPaymentService
{
	public class SmsPaymentServiceInstanceProvider : IInstanceProvider
	{
		private readonly IPaymentController _paymentController;
		private readonly IDriverPaymentService _driverPaymentService;
		private readonly ISmsPaymentStatusNotificationReciever _smsPaymentStatusNotificationReciever;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly SmsPaymentFileCache _smsPaymentFileProdiver;
		private readonly ISmsPaymentDTOFactory _smsPaymentDTOFactory;

		public SmsPaymentServiceInstanceProvider(
			IPaymentController paymentController,
			IDriverPaymentService driverPaymentService,
			ISmsPaymentStatusNotificationReciever smsPaymentStatusNotificationReciever,
			IOrderParametersProvider orderParametersProvider,
			SmsPaymentFileCache smsPaymentFileProdiver,
			ISmsPaymentDTOFactory smsPaymentDTOFactory
		)
		{
			_paymentController = paymentController ?? throw new ArgumentNullException(nameof(paymentController));
			_driverPaymentService = driverPaymentService ?? throw new ArgumentNullException(nameof(driverPaymentService));
			_smsPaymentStatusNotificationReciever = smsPaymentStatusNotificationReciever ??
				throw new ArgumentNullException(nameof(smsPaymentStatusNotificationReciever));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			_smsPaymentFileProdiver = smsPaymentFileProdiver ?? throw new ArgumentNullException(nameof(smsPaymentFileProdiver));
			_smsPaymentDTOFactory = smsPaymentDTOFactory ?? throw new ArgumentNullException(nameof(smsPaymentDTOFactory));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new SmsPaymentService.SmsPaymentService(_paymentController, _driverPaymentService, _smsPaymentStatusNotificationReciever,
				_orderParametersProvider, _smsPaymentFileProdiver, _smsPaymentDTOFactory);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return GetInstance(instanceContext);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{ }

		#endregion
	}
}
