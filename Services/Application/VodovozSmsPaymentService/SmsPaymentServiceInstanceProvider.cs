using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using SmsPaymentService;

namespace VodovozSmsPaymentService
{
	public class SmsPaymentServiceInstanceProvider : IInstanceProvider
	{
		private readonly IPaymentWorker paymentWorker;
		private readonly IDriverPaymentService driverPaymentService;

		public SmsPaymentServiceInstanceProvider(IPaymentWorker paymentWorker, IDriverPaymentService driverPaymentService)
		{
			this.paymentWorker = paymentWorker ?? throw new ArgumentNullException(nameof(paymentWorker));
			this.driverPaymentService = driverPaymentService ?? throw new ArgumentNullException(nameof(driverPaymentService));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new SmsPaymentService.SmsPaymentService(paymentWorker, driverPaymentService);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return GetInstance(instanceContext);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance) { }

		#endregion
	}
}