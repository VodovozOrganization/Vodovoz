using System;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace VodovozSmsInformerService
{
	public class SmsInformerServiceHost : WebServiceHost
	{
		public SmsInformerServiceHost(SmsInformerInstanceProvider serviceStatusInstanceProvider) : base(typeof(SmsInformerService))
		{
			Description.Behaviors.Add(new SmsInformerServiceBehavior(serviceStatusInstanceProvider));
		}
	}
}
