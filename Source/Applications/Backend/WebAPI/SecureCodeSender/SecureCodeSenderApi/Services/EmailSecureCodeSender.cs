using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecureCodeSender.Contracts.Responses;
using Mailjet.Api.Abstractions;
using MassTransit;
using QS.DomainModel.UoW;
using RabbitMQ.EmailSending.Contracts;
using RabbitMQ.MailSending;
using Vodovoz.Core.Domain.SecureCodes;
using Vodovoz.Domain.Client;
using Vodovoz.Settings.Common;

namespace SecureCodeSenderApi.Services
{
	public class EmailSecureCodeSender : IEmailSecureCodeSender
	{
		private readonly IEmailSettings _emailSettings;
		private readonly IRequestClient<SendEmailMessage> _client;
		
		public EmailSecureCodeSender(
			IEmailSettings emailSettings,
			IRequestClient<SendEmailMessage> client)
		{
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_client = client ?? throw new ArgumentNullException(nameof(client));
		}
		
		public async Task<bool> SendCodeToEmail(IUnitOfWork uow, GeneratedSecureCode secureCode)
		{
			var instanceId = Convert.ToInt32(uow.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			var message = $"Весёлый Водовоз: код для входа - {secureCode.Code}";

			Counterparty client = null;

			if(secureCode.CounterpartyId.HasValue)
			{
				client = uow.GetById<Counterparty>(secureCode.CounterpartyId.Value);
			}

			var sendEmailMessage = new SendEmailMessage
			{
				From = new EmailContact
				{
					Name = _emailSettings.DocumentEmailSenderName,
					Email = _emailSettings.DocumentEmailSenderAddress
				},

				To = new List<EmailContact>
				{
					new EmailContact
					{
						Name = client != null ? client.FullName : "Уважаемый пользователь",
						Email = secureCode.Target
					}
				},

				Subject = "Код авторизации",

				TextPart = message,
				HTMLPart = message,
				Payload = new EmailPayload
				{
					Id = 0,
					Trackable = false,
					InstanceId = instanceId
				},
				
				Attachments = new List<EmailAttachment>()
			};

			var response = await _client.GetResponse<SentEmailResponse>(sendEmailMessage);
			return response.Message.Sent;
		}
	}
}
