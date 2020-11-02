using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using SmsPaymentService;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Services;

namespace VodovozSmsPaymentService
{
	public class SmsPaymentServiceInstanceProvider : IInstanceProvider
	{
		private readonly IPaymentController paymentController;
		private readonly IDriverPaymentService driverPaymentService;
		private readonly ISmsPaymentServiceParametersProvider smsPaymentServiceParametersProvider;
		private readonly SmsPaymentFileCache smsPaymentFileProdiver;
		private readonly ICashlessPaymentRepository cashlessPaymentRepository;

		public SmsPaymentServiceInstanceProvider(
			IPaymentController paymentController, 
			IDriverPaymentService driverPaymentService, 
			ISmsPaymentServiceParametersProvider smsPaymentServiceParametersProvider,
			SmsPaymentFileCache smsPaymentFileProdiver,
			ICashlessPaymentRepository cashlessPaymentRepository
		)
		{
			this.paymentController = paymentController ?? throw new ArgumentNullException(nameof(paymentController));
			this.driverPaymentService = driverPaymentService ?? throw new ArgumentNullException(nameof(driverPaymentService));
			this.smsPaymentServiceParametersProvider = smsPaymentServiceParametersProvider ?? throw new ArgumentNullException(nameof(smsPaymentServiceParametersProvider));
			this.smsPaymentFileProdiver = smsPaymentFileProdiver ?? throw new ArgumentNullException(nameof(smsPaymentFileProdiver));
			this.cashlessPaymentRepository = cashlessPaymentRepository;
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new SmsPaymentService.SmsPaymentService(paymentController, driverPaymentService, smsPaymentServiceParametersProvider, smsPaymentFileProdiver, cashlessPaymentRepository);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return GetInstance(instanceContext);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance) { }

		#endregion
	}
}