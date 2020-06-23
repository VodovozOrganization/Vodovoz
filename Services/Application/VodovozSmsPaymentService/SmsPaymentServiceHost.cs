using System.ServiceModel;

namespace VodovozSmsPaymentService
{
	public class SmsPaymentServiceHost : ServiceHost
	{
		public SmsPaymentServiceHost(SmsPaymentServiceInstanceProvider smsPaymentServiceInstanceProvider) : base(typeof(SmsPaymentService.SmsPaymentService))
		{
			Description.Behaviors.Add(new SmsPaymentServiceBehavior(smsPaymentServiceInstanceProvider));
		}
	}
}
