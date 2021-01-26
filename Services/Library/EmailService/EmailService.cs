using System;
using System.ServiceModel.Web;
using EmailService.Mailjet;
using Vodovoz.Services;

namespace EmailService
{
	public class EmailService : IEmailService, IMailjetEventService, IEmailServiceWeb
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IBitrixServiceSettings bitrixServiceSettings;

		public EmailService(IBitrixServiceSettings bitrixServiceSettings)
		{
			EmailManager.Init();
			this.bitrixServiceSettings = bitrixServiceSettings ?? throw new ArgumentNullException(nameof(bitrixServiceSettings));
		}

		public void PostEvent(MailjetEvent content)
		{
			EmailManager.AddEvent(content);

			//Необходимо обязательно отправлять в ответ http code 200 - OK
			WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
		}

		public Tuple<bool, string> SendEmail(Email mail)
		{
			return EmailManager.AddEmail(mail);
		}

		public bool ServiceStatus()
		{
			int emailsInQueue = EmailManager.GetEmailsInQueue();
			if(emailsInQueue > bitrixServiceSettings.MaxEmailsInQueueForWorkingService) {
				return false;
			}
			return true;
		}
	}
}
