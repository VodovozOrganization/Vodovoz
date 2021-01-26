using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace VodovozEmailService
{
	public class EmailServiceHost : ServiceHost
	{
		public EmailServiceHost(EmailInstanceProvider emailInstanceProvider) : base(typeof(EmailService.EmailService))
		{
			Description.Behaviors.Add(new EmailServiceBehavior(emailInstanceProvider));
		}
	}
}
