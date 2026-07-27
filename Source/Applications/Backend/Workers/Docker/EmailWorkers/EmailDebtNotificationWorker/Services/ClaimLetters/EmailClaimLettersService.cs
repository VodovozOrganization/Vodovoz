using BitrixApi.Library.Services;
using Email.Infrastructure.Factories;
using Email.Infrastructure.Generators;
using EmailDebtNotificationWorker.Options;
using EmailDebtNotificationWorker.Services.Common.Generators;
using EmailDebtNotificationWorker.Services.Common.Selectors;
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
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.EntityRepositories.Nodes;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;

namespace EmailDebtNotificationWorker.Services.ClaimLetters
{
	public class EmailClaimLettersService : IEmailClaimLettersService
	{
		private const string _emailSubject = "Претензия";

		private readonly ILogger<EmailClaimLettersService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IEmailRepository _emailRepository;
		private readonly IEmailMessageFactory _emailMessageFactory;
		private readonly IEmailBodyGenerator _emailBodyGenerator;
		private readonly IEmailLinkGenerator _emailLinkGenerator;
		private readonly IClientEmailSelector _clientEmailSelector;
		private readonly IEmailAttachmentsCreateService _emailAttachmentsCreateService;
		private readonly IBus _bus;
		private readonly IOptionsMonitor<EmailClaimLettersOptions> _emailClaimLettersOptions;
		private readonly IClaimLetterBillWithoutShipmentService _claimLetterBillWithoutShipmentService;

		private readonly RevenueStatus[] _excludeCounterpartyRevenueStatuses = new[]
		{
			RevenueStatus.Liquidating,
			RevenueStatus.Liquidated,
			RevenueStatus.Reorganizing,
			RevenueStatus.Bankrupt
		};

		public EmailClaimLettersService(
			ILogger<EmailClaimLettersService> logger,
			IUnitOfWorkFactory uowFactory,
			IOrderRepository orderRepository,
			IEmailRepository emailRepository,
			IEmailBodyGenerator emailBodyGenerator,
			IEmailLinkGenerator emailLinkGenerator,
			IClientEmailSelector clientEmailSelector,
			IEmailAttachmentsCreateService emailAttachmentsCreateService,
			IEmailMessageFactory emailMessageFactory,
			IBus bus,
			IOptionsMonitor<EmailClaimLettersOptions> emailClaimLettersOptions,
			IClaimLetterBillWithoutShipmentService claimLetterBillWithoutShipmentService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_emailBodyGenerator = emailBodyGenerator ?? throw new ArgumentNullException(nameof(emailBodyGenerator));
			_emailLinkGenerator = emailLinkGenerator ?? throw new ArgumentNullException(nameof(emailLinkGenerator));
			_clientEmailSelector = clientEmailSelector ?? throw new ArgumentNullException(nameof(clientEmailSelector));
			_emailAttachmentsCreateService = emailAttachmentsCreateService ?? throw new ArgumentNullException(nameof(emailAttachmentsCreateService));
			_emailMessageFactory = emailMessageFactory ?? throw new ArgumentNullException(nameof(emailMessageFactory));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
			_emailClaimLettersOptions = emailClaimLettersOptions ?? throw new ArgumentNullException(nameof(emailClaimLettersOptions));
			_claimLetterBillWithoutShipmentService = claimLetterBillWithoutShipmentService ?? throw new ArgumentNullException(nameof(claimLetterBillWithoutShipmentService));
		}

		public async Task SendClaimLetters(CancellationToken cancellationToken)
		{
			using var uow = _uowFactory.CreateWithoutRoot(nameof(EmailClaimLettersService));

			var todaySentLetterOfClaimsCount = _emailRepository.GetTodaySentLetterOfClaimsCount(uow);

			if(todaySentLetterOfClaimsCount >= _emailClaimLettersOptions.CurrentValue.MaxCountPerDay)
			{
				_logger.LogDebug("Макс. количество писем для рассылки в день {MaxLettersPerDay} уже достигнуто. Сегодня отправлено {TodaySentLetterOfClaimsCount} писем",
					_emailClaimLettersOptions.CurrentValue.MaxCountPerDay,
					todaySentLetterOfClaimsCount);
				return;
			}

			var lettersToSendCount = Math.Min(
				_emailClaimLettersOptions.CurrentValue.MaxCountPerDay - todaySentLetterOfClaimsCount,
				_emailClaimLettersOptions.CurrentValue.MaxCountPerInterval);

			var overdueDebitorsDebtData = (await _orderRepository.GetOverdueDebtorDebtDataForLettersOfClaim(
				uow,
				_emailClaimLettersOptions.CurrentValue.LettersOfClaimTimeoutDays,
				_excludeCounterpartyRevenueStatuses,
				_emailClaimLettersOptions.CurrentValue.ResendIntervalDays,
				lettersToSendCount,
				cancellationToken)).ToList();

			if(!overdueDebitorsDebtData.Any())
			{
				_logger.LogDebug("Нет клиентов для отправки претензионных писем");
				return;
			}

			var debtDataWithBills = await PrepareDebtDataWithBillsAsync(uow, overdueDebitorsDebtData, cancellationToken);

			await uow.CommitAsync(cancellationToken);

			var emailMessages = await PrepareEmailMessagesAsync(uow, debtDataWithBills, cancellationToken);

			await uow.CommitAsync(cancellationToken);

			foreach(var emailMessage in emailMessages)
			{
				await PublishEmailMessage(emailMessage, cancellationToken);
			}
		}

		private async Task<List<(CounterpartyOverdueDebtorDebtAggregatedNode DebtData, OrderWithoutShipmentForDebt Bill)>> PrepareDebtDataWithBillsAsync(
			IUnitOfWork uow,
			List<CounterpartyOverdueDebtorDebtAggregatedNode> overdueDebitorsDebtData,
			CancellationToken cancellationToken)
		{
			var debtDataWithBills = new List<(CounterpartyOverdueDebtorDebtAggregatedNode DebtData, OrderWithoutShipmentForDebt Bill)>();

			foreach(var debtData in overdueDebitorsDebtData)
			{
				try
				{
					var bill = await _claimLetterBillWithoutShipmentService.GetOrCreateOrderWithoutShipmentForDebtAsync(
						uow,
						debtData.CounterpartyId,
						debtData.OrganizationId,
						debtData.TotalOverdueDebtorDebt,
						cancellationToken);

					await uow.SaveAsync(bill, cancellationToken: cancellationToken);

					debtDataWithBills.Add((debtData, bill));
				}
				catch(Exception ex)
				{
					_logger.LogError(ex,
						"Ошибка при подготовке счета без отгрузки на долг для контрагента {CounterpartyId} и организации {OrganizationId}",
						debtData.CounterpartyId,
						debtData.OrganizationId);
				}
			}

			return debtDataWithBills;
		}

		private async Task<List<SendEmailMessage>> PrepareEmailMessagesAsync(
			IUnitOfWork uow,
			IEnumerable<(CounterpartyOverdueDebtorDebtAggregatedNode DebtData, OrderWithoutShipmentForDebt Bill)> debtDataWithBills,
			CancellationToken cancellationToken)
		{
			var emailMessages = new List<SendEmailMessage>();

			foreach(var (data, bill) in debtDataWithBills)
			{
				try
				{
					var client = await uow.Session.GetAsync<Counterparty>(data.CounterpartyId, cancellationToken)
						?? throw new InvalidOperationException($"Клиент с Id {data.CounterpartyId} не найден");

					var contract = await uow.Session.GetAsync<CounterpartyContract>(data.ContractId, cancellationToken)
						?? throw new InvalidOperationException($"Договор с Id {data.ContractId} не найден");

					var emailMessage = await CreateSendEmailMessage(
						uow,
						client,
						data.OrganizationId,
						data.OrganizationFullName,
						data.OrganizationEmailForMailing,
						contract,
						data.OrderIds,
						data.TotalOverdueDebtorDebt,
						bill,
						cancellationToken);

					emailMessages.Add(emailMessage);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex,
						"Ошибка при создании и публикации сообщения на отправку письма с претензией для контрагента {CounterpartyId} и организации {OrganizationId}",
						data.CounterpartyId,
						data.OrganizationId);
				}
			}

			return emailMessages;
		}

		private async Task<SendEmailMessage> CreateSendEmailMessage(
			IUnitOfWork uow,
			Counterparty client,
			int organizationId,
			string organizationFullName,
			string organizationEmailForMailing,
			CounterpartyContract contract,
			IEnumerable<int> orderIds,
			decimal totalOverdueDebtorDebt,
			OrderWithoutShipmentForDebt billWithoutShipment,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(organizationEmailForMailing))
			{
				_logger.LogWarning(
					"Организация {OrganizationId} не имеет email для рассылки, указанного в настройках. " +
					"Письмо с претензией для контрагента {CounterpartyId} отправлено не будет.",
					organizationId,
					client.Id);
				throw new InvalidOperationException(
					$"Организация {organizationId} не имеет email для рассылки, указанного в настройках");
			}

			var earliestOrder = await _orderRepository.GetEarliestOrder(uow, orderIds, cancellationToken);
			var earliestOrderDate = earliestOrder?.DeliveryDate ?? DateTime.Today;

			var attachments = CreateEmailAttachments(
				client.Id,
				organizationId,
				GetFormattedSum(totalOverdueDebtorDebt),
				earliestOrderDate,
				billWithoutShipment);

			if(!attachments.Any())
			{
				_logger.LogWarning(
					"Не удалось создать вложения для письма с претензией для контрагента {CounterpartyId} и организации {OrganizationId}. " +
					"Письмо не будет отправлено.",
					client.Id,
					organizationId);
				throw new InvalidOperationException(
					$"Не удалось создать вложения для письма с претензией для контрагента {client.Id} и организации {organizationId}");
			}

			var emailSubject = $"{_emailSubject} {client.FullName} (ИНН: {client.INN}) от {organizationFullName}";

			var emailAddress = _clientEmailSelector.SelectEmailForDebtNotification(client);

			if(string.IsNullOrWhiteSpace(emailAddress))
			{
				_logger.LogWarning(
					"Клиент {ClientId} {ClientName} не имеет подходящего email для отправки претензионного письма",
					client.Id,
					client.FullName);
				throw new InvalidOperationException(
					$"Клиент {client.Id} {client.FullName} не имеет подходящего email для отправки претензионного письма");
			}

			var storedEmail = _emailMessageFactory.CreateStoredEmail(emailSubject, emailAddress, null);

			await uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);

			var bulkEmail = new LetterOfClaimEmail
			{
				StoredEmail = storedEmail,
				Counterparty = client,
				OrganizationId = organizationId
			};

			await uow.SaveAsync(bulkEmail, cancellationToken: cancellationToken);

			var messageText = _emailBodyGenerator.GenerateClaimEmailBody(client, contract, billWithoutShipment.DebtSum, _emailLinkGenerator.GetUnsubscribeLink(storedEmail.Guid.Value));

			var emailMessage = _emailMessageFactory.CreateSendEmailMessage(
				uow,
				storedEmail,
				client.FullName,
				organizationFullName,
				organizationEmailForMailing,
				attachments,
				emailAddress,
				emailSubject,
				messageText);

			return emailMessage;
		}

		private async Task PublishEmailMessage(SendEmailMessage emailMessage, CancellationToken cancellationToken)
		{
			try
			{
				await _bus.Publish(emailMessage, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при публикации сообщения на отправку письма с претензией. StoredEmailId = {StoredEmailId}",
					emailMessage.Payload.Id);
			}
		}

		private IEnumerable<EmailAttachment> CreateEmailAttachments(
			int counterpartyId,
			int organizationId,
			string totalOverdueDebtorDebtFormatted,
			DateTime earliestOrderDate,
			OrderWithoutShipmentForDebt billWithoutShipment)
		{
			var attachments = new List<EmailAttachment>();

			var today = DateTime.Today;
			var yesterday = today.AddDays(-1);
			var startDateForRevision = new DateTime(earliestOrderDate.Year, 1, 1);
			var endDateForRevision = yesterday;
			try
			{
				var revisionAttachments = _emailAttachmentsCreateService.CreateRevisionAttachments(
					counterpartyId,
					organizationId,
					startDateForRevision,
					endDateForRevision);

				var letterOfClaimAttachments = _emailAttachmentsCreateService.CreateLetterOfClaimAttachments(
					organizationId,
					counterpartyId,
					totalOverdueDebtorDebtFormatted);

				var billAttachments = _emailAttachmentsCreateService.CreateOrderWithoutShipmentForDebtAttachments(billWithoutShipment);

				attachments.AddRange(billAttachments);
				attachments.AddRange(letterOfClaimAttachments);
				attachments.AddRange(revisionAttachments);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при создании вложений для письма с претензией для контрагента {CounterpartyId} и организации {OrganizationId}",
					counterpartyId,
					organizationId);
			}

			return attachments;
		}

		private static string GetFormattedSum(decimal sum)
		{
			int rubles = (int)sum;
			int kopeks = (int)((sum - rubles) * 100);
			string result = $"{rubles} руб. {kopeks:D2} коп.";

			return result;
		}
	}
}
