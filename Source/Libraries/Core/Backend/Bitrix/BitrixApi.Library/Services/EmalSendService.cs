using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Settings.Common;

namespace BitrixApi.Library.Services
{
	public class EmalSendService
	{
		private readonly ILogger<EmalSendService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmailAttachmentsCreateService _emailAttachmentsCreateService;
		private readonly IEmailSettings _emailSettings;

		public EmalSendService(
			ILogger<EmalSendService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmailAttachmentsCreateService emailAttachmentsCreateService,
			IEmailSettings emailSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_emailAttachmentsCreateService = emailAttachmentsCreateService ?? throw new ArgumentNullException(nameof(emailAttachmentsCreateService));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
		}

		//private Task SendRevisionDocumentByEmail(int counterpartyId, int organizationId)
		//{
		//	try
		//	{
		//		using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(EmalSendService)))
		//		{
		//			var attachments =
		//			_emailAttachmentsCreateService.CreateRevisionAttachments(counterpartyId, organizationId);

		//			if(attachments.Count() == 0)
		//			{
		//				return;
		//			}
		//			var instanceId = Convert.ToInt32(UnitOfWork.Session
		//				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
		//				.List<object>()
		//				.FirstOrDefault());
		//			string messageText = "Акт сверки";
		//			var emailMessage = new SendEmailMessage
		//			{
		//				From = new EmailContact
		//				{
		//					Name = _emailSettings.DefaultEmailSenderName,
		//					Email = _emailSettings.DefaultEmailSenderAddress
		//				},
		//				To = new List<EmailContact>
		//			{
		//				new EmailContact
		//				{
		//					Name = SelectedEmail != null ? SelectedEmail.Counterparty.FullName : "Уважаемый пользователь",
		//					Email = SelectedEmail.Address
		//				}
		//			},
		//				Subject = "Акт сверки",
		//				TextPart = messageText,
		//				HTMLPart = messageText,
		//				Payload = new EmailPayload
		//				{
		//					Id = 0,
		//					Trackable = false,
		//					InstanceId = instanceId
		//				},
		//				Attachments = attachments
		//			};

		//			_emailDirectSender.SendAsync(emailMessage);
		//		}
		//	}
		//	catch(Exception ex)
		//	{
		//	}
		//}
	}
}
