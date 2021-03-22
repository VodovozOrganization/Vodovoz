using System;
using System.ServiceModel.Web;
using EmailService.Mailjet;
using Vodovoz.Services;

namespace EmailService
{
	public class EmailService : IEmailService, IMailjetEventService, IEmailServiceWeb
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IEmailServiceSettings emailServiceSettings;

		public EmailService(IEmailServiceSettings emailServiceSettings)
		{
			EmailManager.Init();
			this.emailServiceSettings = emailServiceSettings ?? throw new ArgumentNullException(nameof(emailServiceSettings));
		}

		public void PostEvent(MailjetEvent content)
		{
			EmailManager.AddEvent(content);

			//Необходимо обязательно отправлять в ответ http code 200 - OK
			WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
		}

        public Tuple<bool, string> SendOrderEmail(OrderEmail mail)
		{
			return EmailManager.AddEmail(mail);
		}
		
		public bool SendEmail(Email mail)
		{
			return EmailManager.SendEmail(mail).Result;
		}

		public bool ServiceStatus()
		{
			int emailsInQueue = EmailManager.GetEmailsInQueue();
			if(emailsInQueue > emailServiceSettings.MaxEmailsInQueueForWorkingService) {
				return false;
			}
			return true;
		}
	}
}
