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
		private readonly IPaymentController paymentController;
		private readonly IDriverPaymentService driverPaymentService;
        private readonly ISmsPaymentStatusNotificationReciever smsPaymentStatusNotificationReciever;
        private readonly IOrderParametersProvider orderParametersProvider;
		private readonly SmsPaymentFileCache smsPaymentFileProdiver;

		public SmsPaymentServiceInstanceProvider(
			IPaymentController paymentController, 
			IDriverPaymentService driverPaymentService,
			ISmsPaymentStatusNotificationReciever smsPaymentStatusNotificationReciever,
			IOrderParametersProvider orderParametersProvider,
			SmsPaymentFileCache smsPaymentFileProdiver
		)
		{
			this.paymentController = paymentController ?? throw new ArgumentNullException(nameof(paymentController));
			this.driverPaymentService = driverPaymentService ?? throw new ArgumentNullException(nameof(driverPaymentService));
            this.smsPaymentStatusNotificationReciever = smsPaymentStatusNotificationReciever ?? throw new ArgumentNullException(nameof(smsPaymentStatusNotificationReciever));
            this.orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			this.smsPaymentFileProdiver = smsPaymentFileProdiver ?? throw new ArgumentNullException(nameof(smsPaymentFileProdiver));

		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new SmsPaymentService.SmsPaymentService(paymentController, driverPaymentService, smsPaymentStatusNotificationReciever, orderParametersProvider, smsPaymentFileProdiver);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return GetInstance(instanceContext);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance) { }

		#endregion
	}
}