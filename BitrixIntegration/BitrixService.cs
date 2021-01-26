using System;
using System.ServiceModel.Web;
using BitrixIntegration.DTO;
using BitrixIntegration.DTO.Mailjet;
using Vodovoz.Services;

namespace BitrixIntegration
{
	public class BitrixService : IBitrixService, IMailjetEventService, IBitrixServiceWeb
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IBitrixServiceSettings bitrixServiceSettings;

		public BitrixService(IBitrixServiceSettings bitrixServiceSettings)
		{
			BitrixManager.Init();
			this.bitrixServiceSettings = bitrixServiceSettings ?? throw new ArgumentNullException(nameof(bitrixServiceSettings));
		}

		public void PostEvent(MailjetEvent content)
		{
			BitrixManager.AddEvent(content);

			//Необходимо обязательно отправлять в ответ http code 200 - OK
			WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
		}

		public Tuple<bool, string> SendEmail(Email mail)
		{
			return BitrixManager.AddEmail(mail);
		}

		public bool ServiceStatus()
		{
			int emailsInQueue = BitrixManager.GetEmailsInQueue();
			if(emailsInQueue > bitrixServiceSettings.MaxEmailsInQueueForWorkingService) {
				return false;
			}
			return true;
		}
	}
}
