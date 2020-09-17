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
		private readonly IPaymentWorker paymentWorker;
		private readonly IDriverPaymentService driverPaymentService;
		private readonly ISmsPaymentServiceParametersProvider smsPaymentServiceParametersProvider;
		private readonly FileProvider fileProdiver;
		private readonly UnsavedPaymentsWorker unsavedPaymentsWorker;

		public SmsPaymentServiceInstanceProvider(
			IPaymentWorker paymentWorker, 
			IDriverPaymentService driverPaymentService, 
			ISmsPaymentServiceParametersProvider smsPaymentServiceParametersProvider,
			FileProvider fileProdiver,
			UnsavedPaymentsWorker unsavedPaymentsWorker
			)
		{
			this.paymentWorker = paymentWorker ?? throw new ArgumentNullException(nameof(paymentWorker));
			this.driverPaymentService = driverPaymentService ?? throw new ArgumentNullException(nameof(driverPaymentService));
			this.smsPaymentServiceParametersProvider = smsPaymentServiceParametersProvider ?? throw new ArgumentNullException(nameof(smsPaymentServiceParametersProvider));
			this.fileProdiver = fileProdiver ?? throw new ArgumentNullException(nameof(fileProdiver));
			this.unsavedPaymentsWorker = unsavedPaymentsWorker ?? throw new ArgumentNullException(nameof(unsavedPaymentsWorker));

		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new SmsPaymentService.SmsPaymentService(paymentWorker, driverPaymentService, smsPaymentServiceParametersProvider, fileProdiver, unsavedPaymentsWorker);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return GetInstance(instanceContext);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance) { }

		#endregion
	}
}