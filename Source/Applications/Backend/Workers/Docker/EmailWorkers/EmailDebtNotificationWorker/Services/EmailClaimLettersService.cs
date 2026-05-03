using BitrixApi.Library.Services;
using EmailDebtNotificationWorker.Options;
using Mailjet.Api.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Common;
using VodovozBusiness.Domain.StoredEmails;
using StoredEmails = Vodovoz.Domain.StoredEmails;

namespace EmailDebtNotificationWorker.Services
{
	public class EmailClaimLettersService : IEmailClaimLettersService
	{
		private readonly ILogger<EmailClaimLettersService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IEmailAttachmentsCreateService _emailAttachmentsCreateService;
		private readonly IEmailSettings _emailSettings;
		private readonly IBus _bus;
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
			IEmailSettings emailSettings,
			IBus bus,
			IOptionsMonitor<EmailClaimLettersOptions> emailClaimLettersOptions)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new System.ArgumentNullException(nameof(orderRepository));
			_emailAttachmentsCreateService = emailAttachmentsCreateService ?? throw new System.ArgumentNullException(nameof(emailAttachmentsCreateService));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
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

			var data = overdueDebitorsDebtData.Take(1).ToList();

			foreach(var debtData in data)
			{
				var client = debtData.Value.Counterparty;
				var organizationId = debtData.Value.OrganizationId;
				var organizationFullName = debtData.Value.OrganizationFullName;
				var organizationEmailForMailing = debtData.Value.OrganizationEmailForMailing;
				var contract = debtData.Value.Contract;
				var orderIds = debtData.Value.OrderIds;
				var totalOverdueDebtorDebt = debtData.Value.TotalOverdueDebtorDebt;

				bool isEmailSent = await TrySendEmail(
					uow,
					client,
					organizationId,
					organizationFullName,
					organizationEmailForMailing,
					contract,
					orderIds,
					totalOverdueDebtorDebt,
					cancellationToken);
			}

			await Task.CompletedTask;
		}

		private async Task<bool> TrySendEmail(
			IUnitOfWork uow,
			Counterparty client,
			int organizationId,
			string organizationFullName,
			string organizationEmailForMailing,
			CounterpartyContract contract,
			IEnumerable<int> orderIds,
			decimal totalOverdueDebtorDebt,
			CancellationToken cancellationToken)
		{
			var attachments = CreateEmailAttachments(client.Id, organizationId, totalOverdueDebtorDebt.ToString("C"), orderIds);

			if(!attachments.Any())
			{
				_logger.LogWarning(
					"Не удалось создать вложения для письма с претензией для контрагента {CounterpartyId} и организации {OrganizationId}. " +
					"Письмо не будет отправлено.",
					client.Id,
					organizationId);
				return false;
			}

			var emailAddress = SelectEmailForDebtNotification(client);

			if(string.IsNullOrWhiteSpace(emailAddress))
			{
				_logger.LogWarning(
					"Клиент {ClientId} {ClientName} не имеет подходящего email для отправки претензионного письма",
					client.Id,
					client.FullName);
				return false;
			}

			var storedEmail = CreateStoredEmail("Письмо претензии", emailAddress, client.INN);
			await uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);

			var bulkEmail = new OrganizationBulkEmail
			{
				StoredEmail = storedEmail,
				Counterparty = client,
				OrganizationId = organizationId
			};

			await uow.SaveAsync(bulkEmail, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

			var emailMessage = CreateEmailMessage(
				uow,
				storedEmail,
				client,
				organizationFullName,
				organizationEmailForMailing,
				attachments,
				emailAddress,
				"Уведомление о задолженности",
				GenerateEmailBody(contract, GetUnsubscribeLink(storedEmail.Guid.Value)));

			try
			{
				await _bus.Publish(emailMessage, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при публикации сообщения на отправку письма с претензией для контрагента {CounterpartyId} и организации {OrganizationId}",
					client.Id,
					organizationId);
				return false;
			}

			return true;
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
			StoredEmail storedEmail,
			Counterparty client,
			string organizationFullName,
			string organizationEmailForMailing,
			IEnumerable<EmailAttachment> attachments,
			string emailAddress,
			string emailSubject,
			string messageText
			)
		{
			var instanceId = GetCurrentDatabaseId(uow);

			var unsubscribeUrl = storedEmail.Guid.HasValue
				? GetUnsubscribeLink(storedEmail.Guid.Value)
				: string.Empty;

			var sendEmailMessage = new SendEmailMessage()
			{
				From = new EmailContact
				{
					Name = organizationFullName,
					Email = organizationEmailForMailing,
				},
				To = new List<EmailContact>
				{
					new()
					{
						Name = client.FullName,
						Email = emailAddress
					}
				},
				Subject = emailSubject,
				TextPart = messageText,
				HTMLPart = messageText,
				Payload = new EmailPayload
				{
					Id = storedEmail.Id,
					Trackable = true,
					InstanceId = instanceId
				},
				Attachments = attachments.ToList(),
				Headers = new Dictionary<string, string>
				{
					{ "List-Unsubscribe", unsubscribeUrl }
				}
			};

			return sendEmailMessage;
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

		private string GetUnsubscribeLink(Guid guid) => $"{_emailSettings.UnsubscribeUrl}/{guid}";

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
					organizationId,
					counterpartyId,
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

		private static string GenerateEmailBody(
			CounterpartyContract contract,
			string unsubscribeUrl)
		{
			return $@"
				<p>Добрый день!</p>
				<p>Информируем, что у Вашей компании образовалась просроченная задолженность по договору {contract.Number} от {contract.IssueDate:dd.MM.yyyy}.</p> 
				<p>Настоятельно рекомендуем принять участие в мирном урегулировании данного вопроса, что позволит обеим сторонам сэкономить время и деньги.</p>
				<p>И позволит продолжить дальнейшее плодотворное сотрудничество наших компаний!</p>
				<p>______________</p>
				<p><a href='{unsubscribeUrl}' class='unsubscribe'>Отписаться от рассылки</a></p>
				<p>Вы всегда можете отписаться от нашей рассылки, нажав соответствующую кнопку.</p>
				<p><em>Это письмо отправлено автоматически.</em></p>";
		}
	}
}
