using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using QS.DomainModel.UoW;
using SmsPaymentService;
using SmsPaymentService.PaymentControllers;
using Vodovoz.NotificationRecievers;
using Vodovoz.Services;

namespace VodovozSmsPaymentService
{
	public class SmsPaymentServiceInstanceProvider : IInstanceProvider
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IPaymentController _paymentController;
		private readonly ISmsPaymentStatusNotificationReciever _smsPaymentStatusNotificationReciever;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly SmsPaymentFileCache _smsPaymentFileProdiver;
		private readonly ISmsPaymentDTOFactory _smsPaymentDTOFactory;
		private readonly ISmsPaymentValidator _smsPaymentValidator;

		public SmsPaymentServiceInstanceProvider(
			IUnitOfWorkFactory uowFactory,
			IPaymentController paymentController,
			ISmsPaymentStatusNotificationReciever smsPaymentStatusNotificationReciever,
			IOrderParametersProvider orderParametersProvider,
			SmsPaymentFileCache smsPaymentFileProdiver,
			ISmsPaymentDTOFactory smsPaymentDTOFactory,
			ISmsPaymentValidator smsPaymentValidator
		)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_paymentController = paymentController ?? throw new ArgumentNullException(nameof(paymentController));
			_smsPaymentStatusNotificationReciever = smsPaymentStatusNotificationReciever ??
				throw new ArgumentNullException(nameof(smsPaymentStatusNotificationReciever));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			_smsPaymentFileProdiver = smsPaymentFileProdiver ?? throw new ArgumentNullException(nameof(smsPaymentFileProdiver));
			_smsPaymentDTOFactory = smsPaymentDTOFactory ?? throw new ArgumentNullException(nameof(smsPaymentDTOFactory));
			_smsPaymentValidator = smsPaymentValidator ?? throw new ArgumentNullException(nameof(smsPaymentValidator));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new SmsPaymentService.SmsPaymentService(_uowFactory, _paymentController, _smsPaymentStatusNotificationReciever,
				_orderParametersProvider, _smsPaymentFileProdiver, _smsPaymentDTOFactory, _smsPaymentValidator);
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
