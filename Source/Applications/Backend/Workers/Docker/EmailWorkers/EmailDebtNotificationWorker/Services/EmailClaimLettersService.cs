using BitrixApi.Library.Services;
using EmailDebtNotificationWorker.Options;
using Mailjet.Api.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using StoredEmails = Vodovoz.Domain.StoredEmails;

namespace EmailDebtNotificationWorker.Services
{
	public class EmailClaimLettersService : IEmailClaimLettersService
	{
		private readonly ILogger<EmailClaimLettersService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IEmailAttachmentsCreateService _emailAttachmentsCreateService;
		private readonly IOptionsMonitor<EmailClaimLettersOptions> _emailClaimLettersOptions;
		private readonly OrderStatus[] _orderStatuses =
			new[] { OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };

		private readonly RevenueStatus[] _excludeCounterpartyRevenueStatuses =
			new[] { RevenueStatus.Liquidating, RevenueStatus.Liquidated, RevenueStatus.Reorganizing, RevenueStatus.Bankrupt };

		public EmailClaimLettersService(
			ILogger<EmailClaimLettersService> logger,
			IUnitOfWorkFactory uowFactory,
			IOrderRepository orderRepository,
			IEmailAttachmentsCreateService emailAttachmentsCreateService,
			IOptionsMonitor<EmailClaimLettersOptions> emailClaimLettersOptions)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new System.ArgumentNullException(nameof(orderRepository));
			_emailAttachmentsCreateService = emailAttachmentsCreateService ?? throw new System.ArgumentNullException(nameof(emailAttachmentsCreateService));
			_emailClaimLettersOptions = emailClaimLettersOptions ?? throw new System.ArgumentNullException(nameof(emailClaimLettersOptions));
		}

		public async Task SendClaimLetters(CancellationToken cancellationToken)
		{
			using var uow = _uowFactory.CreateWithoutRoot("Получение списка должников в воркере по рассылке писем о претензиях");

			var overdueDebitorsDebtData = await _orderRepository.GetCounterpartyOverdueDebtorDebtData(
				uow,
				_emailClaimLettersOptions.CurrentValue.OverdueDebtorDebtExpiredDaysAgo,
				_orderStatuses,
				_excludeCounterpartyRevenueStatuses,
				cancellationToken);

			if(overdueDebitorsDebtData.Count == 0)
			{
				_logger.LogDebug("Нет писем для массовой рассылки");
				return;
			}

			foreach(var debtData in overdueDebitorsDebtData)
			{
				var clientId = debtData.Key.CounterpartyId;
				var client = debtData.Value.Counterparty;
				var organizationId = debtData.Key.OrganizationId;
				var orderIds = debtData.Value.OrderIds;
				var totalOverdueDebtorDebt = debtData.Value.TotalOverdueDebtorDebt;

				var attachments = CreateEmailAttachments(clientId, organizationId, totalOverdueDebtorDebt.ToString("C"), orderIds);

				if(!attachments.Any())
				{
					_logger.LogWarning(
						"Не удалось создать вложения для письма с претензией для контрагента {CounterpartyId} и организации {OrganizationId}. " +
						"Письмо не будет отправлено.",
						clientId,
						organizationId);
					continue;
				}

				var emailAddress = SelectEmailForDebtNotification(client);

				if(string.IsNullOrWhiteSpace(emailAddress))
				{
					_logger.LogWarning(
						"Клиент {ClientId} {ClientName} не имеет подходящего email для отправки претензионного письма",
						client.Id,
						client.FullName);
					continue;
				}

				var storedEmail = CreateStoredEmail("Письмо претензии", emailAddress, client.INN);
				await uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);
			}

			await Task.CompletedTask;
		}

		private StoredEmails.StoredEmail CreateStoredEmail(string subject, string email, string inn)
		{
			var storedEmail = new StoredEmails.StoredEmail
			{
				State = StoredEmails.StoredEmailStates.PreparingToSend,
				Author = null,
				ManualSending = true,
				SendDate = DateTime.Now,
				StateChangeDate = DateTime.Now,
				Subject = subject,
				RecipientAddress = email,
				Guid = Guid.NewGuid()
			};

			return storedEmail;
		}

		private SendEmailMessage CreateEmailMessage(
			IUnitOfWork uow,
			CounterpartyEntity counterparty,
			string email,
			string messageText,
			int payloadId,
			IEnumerable<EmailAttachment> attachments)
		{
			var instanceId = GetCurrentDatabaseId(uow);

			var emailMessage = new SendEmailMessage
			{
				From = new EmailContact
				{
					Name = _emailSettings.DefaultEmailSenderName,
					Email = _emailSettings.DefaultEmailSenderAddress
				},
				To = new List<EmailContact>
				{
					new EmailContact
					{
						Name = string.IsNullOrWhiteSpace(counterparty.FullName) ? "Уважаемый пользователь" : counterparty.FullName,
						Email = email
					}
				},
				Subject = messageText,
				TextPart = messageText,
				HTMLPart = messageText,
				Payload = new EmailPayload
				{
					Id = payloadId,
					Trackable = false,
					InstanceId = instanceId
				},
				Attachments = attachments.ToArray()
			};

			return emailMessage;
		}

		private static int GetCurrentDatabaseId(IUnitOfWork uow)
		{
			var instanceId = Convert.ToInt32(
				uow.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			return instanceId;
		}

		private IEnumerable<EmailAttachment> CreateEmailAttachments(
			int counterpartyId,
			int organizationId,
			string totalOverdueDebtorDebtFormatted,
			IEnumerable<int> orderIds)
		{
			var attachments = new List<EmailAttachment>();
			try
			{
				var generalBillAttachments = _emailAttachmentsCreateService.CreateGeneralBillAttachments(
					counterpartyId,
					organizationId,
					orderIds);

				var letterOfClaimAttachments = _emailAttachmentsCreateService.CreateLetterOfClaimAttachments(
					counterpartyId,
					organizationId,
					totalOverdueDebtorDebtFormatted);

				attachments.AddRange(generalBillAttachments);
				attachments.AddRange(letterOfClaimAttachments);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при создании вложений для письма с претензией для контрагента {CounterpartyId} и организации {OrganizationId}",
					counterpartyId,
					organizationId);

				return Enumerable.Empty<EmailAttachment>();
			}

			return attachments;
		}

		private string? SelectEmailForDebtNotification(Counterparty client)
		{
			if(client.Emails is null || !client.Emails.Any())
			{
				_logger.LogWarning("Клиент {ClientId} не имеет email адресов", client.Id);
				return null;
			}

			var billEmail = client.Emails
				.LastOrDefault(x => x.EmailType?.EmailPurpose is EmailPurpose.ForBills)
				?.Address;

			if(!string.IsNullOrWhiteSpace(billEmail))
			{
				return billEmail;
			}

			var defaultEmail = client.Emails
				.LastOrDefault()
				?.Address;

			if(!string.IsNullOrWhiteSpace(defaultEmail))
			{
				return defaultEmail;
			}

			return null;
		}
	}
}
