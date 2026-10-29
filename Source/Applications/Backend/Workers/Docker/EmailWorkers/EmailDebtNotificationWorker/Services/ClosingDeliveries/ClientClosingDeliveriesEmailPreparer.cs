using BitrixApi.Library.Services;
using Email.Infrastructure.Factories;
using Email.Infrastructure.Generators;
using EmailDebtNotificationWorker.DTO;
using EmailDebtNotificationWorker.Services.Common.Generators;
using EmailDebtNotificationWorker.Services.Common.Selectors;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Domain.StoredEmails;

namespace EmailDebtNotificationWorker.Services.ClosingDeliveries
{
	public class ClientClosingDeliveriesEmailPreparer : IClientClosingDeliveriesEmailPreparer
	{
		private readonly ILogger<ClientClosingDeliveriesEmailPreparer> _logger;
		private readonly IEmailAttachmentsCreateService _attachmentsService;
		private readonly IClosingDeliveriesSettings _closingDeliveriesSettings;
		private readonly IEmailMessageFactory _emailMessageFactory;
		private readonly IEmailBodyGenerator _emailBodyGenerator;
		private readonly IEmailLinkGenerator _emailLinkGenerator;
		private readonly IClientEmailSelector _clientEmailSelector;

		public ClientClosingDeliveriesEmailPreparer(
			ILogger<ClientClosingDeliveriesEmailPreparer> logger,
			IEmailAttachmentsCreateService attachmentsService,
			IClosingDeliveriesSettings closingDeliveriesSettings,
			IEmailMessageFactory emailMessageFactory,
			IEmailBodyGenerator emailBodyGenerator,
			IEmailLinkGenerator emailLinkGenerator,
			IClientEmailSelector clientEmailSelector
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_attachmentsService = attachmentsService ?? throw new ArgumentNullException(nameof(attachmentsService));
			_closingDeliveriesSettings = closingDeliveriesSettings ?? throw new ArgumentNullException(nameof(closingDeliveriesSettings));
			_emailMessageFactory = emailMessageFactory ?? throw new ArgumentNullException(nameof(emailMessageFactory));
			_emailBodyGenerator = emailBodyGenerator ?? throw new ArgumentNullException(nameof(emailBodyGenerator));
			_emailLinkGenerator = emailLinkGenerator ?? throw new ArgumentNullException(nameof(emailLinkGenerator));
			_clientEmailSelector = clientEmailSelector ?? throw new ArgumentNullException(nameof(clientEmailSelector));
		}

		public async Task<IReadOnlyList<SendEmailMessage>> PrepareClientEmails(
			IUnitOfWork uow,
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken)
		{
			if(!notificationInfos.Any())
			{
				return Array.Empty<SendEmailMessage>();
			}

			var sendEmailMessages = new List<SendEmailMessage>(notificationInfos.Count);

			foreach(var info in notificationInfos)
			{
				try
				{
					var sendEmailMessage = await PrepareClientEmail(uow, info, cancellationToken);

					sendEmailMessages.Add(sendEmailMessage);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка подготовки письма клиенту для счета без отгрузки на долг {OrderWithoutShipmentForDebt}", info.OrderWithoutShipmentForDebt.Id);
				}
			}

			return sendEmailMessages;
		}

		private async Task<SendEmailMessage> PrepareClientEmail(
			IUnitOfWork unitOfWork,
			OrderWithoutShipmentForDebtNotificationInfo notificationInfo,
			CancellationToken cancellationToken)
		{
			var orderWithoutShipmentForDebt = notificationInfo.OrderWithoutShipmentForDebt;

			var emailSentFrom = orderWithoutShipmentForDebt.Organization.ClosingDeliveriesNotificationEmailFrom;

			if(string.IsNullOrWhiteSpace(emailSentFrom))
			{
				throw new InvalidOperationException($"У организации {orderWithoutShipmentForDebt.Organization.Id} не заполнен {nameof(emailSentFrom)}");
			}

			var orderWithoutShipmentForDebtAttachment = _attachmentsService.CreateOrderWithoutShipmentForDebtAttachments(orderWithoutShipmentForDebt);

			var revisionStartDate = new DateTime(notificationInfo.OldestDebtOrderDate.Year, 1, 1);
			var revisionEndDate = DateTime.Today.AddDays(-1);
			var revisionAttachment = _attachmentsService.CreateRevisionAttachments(orderWithoutShipmentForDebt.Client.Id, orderWithoutShipmentForDebt.Organization.Id, revisionStartDate, revisionEndDate);

			var attachments = orderWithoutShipmentForDebtAttachment.Concat(revisionAttachment).ToList();

			if(!attachments.Any())
			{
				throw new InvalidOperationException("Нет вложений");
			}

			var emailSentTo = _clientEmailSelector.SelectEmailForDebtNotification(orderWithoutShipmentForDebt.Client);
			if(string.IsNullOrWhiteSpace(emailSentTo))
			{
				throw new InvalidOperationException($"У клиента {orderWithoutShipmentForDebt.Client.Id} не заполнена электронная почта");
			}

			var storedEmail = _emailMessageFactory.CreateStoredEmail(
				$"{orderWithoutShipmentForDebt.Client.FullName} (ИНН: {orderWithoutShipmentForDebt.Client.INN}) от {orderWithoutShipmentForDebt.Organization.FullName}",
				emailSentTo,
				"Стоп-отгрузка");

			await unitOfWork.SaveAsync(storedEmail, cancellationToken: cancellationToken);

			var closingDeliveriesEmail = new ClosingDeliveriesEmail
			{
				OrganizationId = orderWithoutShipmentForDebt.Organization.Id,
				OrderWithoutShipmentForDebt = orderWithoutShipmentForDebt,
				StoredEmail = storedEmail,
				Counterparty = orderWithoutShipmentForDebt.Client
			};

			await unitOfWork.SaveAsync(closingDeliveriesEmail, cancellationToken: cancellationToken);

			var unsubscribeUrl = _emailLinkGenerator.GetUnsubscribeLink(storedEmail.Guid.Value);

			var clientEmailBody = _emailBodyGenerator.GenerateClosingDeliveriesEmailBody(
				orderWithoutShipmentForDebt.DebtSum,
				_closingDeliveriesSettings.DaysBeforeClosingDeliveries,
				unsubscribeUrl);

			var sendEmailMessage = _emailMessageFactory.CreateSendEmailMessage(
				unitOfWork,
				storedEmail,
				orderWithoutShipmentForDebt.Client.FullName,
				orderWithoutShipmentForDebt.Organization.FullName,
				emailSentFrom,
				attachments,
				emailSentTo,
				storedEmail.Subject,
				clientEmailBody);

			return sendEmailMessage;
		}
	}
}
