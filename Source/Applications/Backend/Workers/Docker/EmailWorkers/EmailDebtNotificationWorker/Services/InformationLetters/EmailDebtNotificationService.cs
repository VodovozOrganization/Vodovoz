using BitrixApi.Library.Services;
using EmailDebtNotificationWorker.Services.Common.Factories;
using EmailDebtNotificationWorker.Services.Common.Generators;
using EmailDebtNotificationWorker.Services.Common.Selectors;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories.Document;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;

namespace EmailDebtNotificationWorker.Services.InformationLetters
{
	/// <summary>
	/// Сервис планирования и отправки email сообщений клиентам о задолженностях
	/// </summary>
	public class EmailDebtNotificationService : IEmailDebtNotificationService
	{
		private readonly ILogger<EmailDebtNotificationService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IEmailRepository _emailRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IDocumentOrganizationCounterRepository _documentOrganizationCounterRepository;
		private readonly IEmailAttachmentsCreateService _emailAttachmentsCreateService;
		private readonly IEmailBodyGenerator _emailBodyGenerator;
		private readonly IEmailLinkGenerator _emailLinkGenerator;
		private readonly IEmailMessageFactory _emailMessageFactory;
		private readonly IClientEmailSelector _clientEmailSelector;
		private readonly IBus _bus;
		private const int _maxEmailsPerMinute = 5;

		public EmailDebtNotificationService(
			ILogger<EmailDebtNotificationService> logger,
			IUnitOfWorkFactory uowFactory,
			IEmailRepository emailRepository,
			IEmployeeRepository employeeRepository,
			IDocumentOrganizationCounterRepository documentOrganizationCounterRepository,
			IEmailAttachmentsCreateService emailAttachmentsCreateService,
			IEmailBodyGenerator emailBodyGenerator,
			IEmailLinkGenerator emailLinkGenerator,
			IEmailMessageFactory emailMessageFactory,
			IClientEmailSelector clientEmailSelector,
			IBus bus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_documentOrganizationCounterRepository = documentOrganizationCounterRepository ?? throw new ArgumentNullException(nameof(documentOrganizationCounterRepository));
			_emailAttachmentsCreateService = emailAttachmentsCreateService ?? throw new ArgumentNullException(nameof(emailAttachmentsCreateService));
			_emailBodyGenerator = emailBodyGenerator ?? throw new ArgumentNullException(nameof(emailBodyGenerator));
			_emailLinkGenerator = emailLinkGenerator ?? throw new ArgumentNullException(nameof(emailLinkGenerator));
			_emailMessageFactory = emailMessageFactory ?? throw new ArgumentNullException(nameof(emailMessageFactory));
			_clientEmailSelector = clientEmailSelector ?? throw new ArgumentNullException(nameof(clientEmailSelector));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		public async Task ScheduleDebtNotificationsAsync(CancellationToken cancellationToken)
		{
			using var uow = _uowFactory.CreateWithoutRoot("Получение списка должников в воркере по рассылке писем о задолженнности");

			var overdueOrders = (await _emailRepository.GetAllOverdueOrdersForDebtNotificationAsync(uow, _maxEmailsPerMinute, cancellationToken)).ToList();
			if(!overdueOrders.Any())
			{
				_logger.LogDebug("Нет писем для массовой рассылки");
				return;
			}

			var groupedOverdueOrders = overdueOrders
				.GroupBy(x => (x.CounterpartyId, x.OrganizationId))
				.ToDictionary(g => g.Key, g => g.Select(x => (x.OrderId, x.Debt)).ToList());

			_logger.LogInformation("В процессе {Count} писем для рассылки",
				groupedOverdueOrders.Count);

			foreach(var orderWithDebtNode in groupedOverdueOrders)
			{
				try
				{
					using var clientUow = _uowFactory.CreateWithoutRoot($"Отправка письма клиенту в воркере по рассылке писем о задолженнности");

					var clientId = orderWithDebtNode.Key.CounterpartyId;
					var organizationId = orderWithDebtNode.Key.OrganizationId;

					await ProcessSingleClientEmailAsync(clientUow, clientId, organizationId, orderWithDebtNode.Value, cancellationToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке письма о задолженности");
				}
			}
		}

		private async Task ProcessSingleClientEmailAsync(
			IUnitOfWork uow,
			int clientId,
			int organizationId,
			IList<(int OrderIds, decimal Debt)> ordersIdsWithDebt,
			CancellationToken cancellationToken
			)
		{
			var client = await uow.Session.GetAsync<Counterparty>(clientId, cancellationToken);
			if(client is null)
			{
				_logger.LogWarning("Попытка отправить письмо несуществующему клиенту");
				throw new InvalidOperationException(nameof(client));
			}

			var organization = await uow.Session.GetAsync<Organization>(organizationId, cancellationToken);
			if(organization is null)
			{
				_logger.LogWarning("Попытка отправить письмо клиенту {ClientId} от несуществующей организации", client.Id);
				throw new InvalidOperationException(nameof(organization));
			}

			if(ordersIdsWithDebt is null)
			{
				_logger.LogWarning("Попытка отправить письмо клиенту {ClientId} от организации {OrganizationId} по несуществующим заказам",
					client.Id,
					organization.Id);
				throw new ArgumentNullException(nameof(ordersIdsWithDebt));
			}

			var orderIds = ordersIdsWithDebt.Select(x => x.OrderIds).ToList();

			var orders = await uow.Session.QueryOver<Order>()
				.WhereRestrictionOn(o => o.Id).IsIn(orderIds)
				.ListAsync(cancellationToken);

			if(!orders.Any())
			{
				_logger.LogWarning("Попытка отправить письмо клиенту {ClientId} от организации {OrganizationId} по несуществующим заказам",
						client.Id,
						organization.Id);

				throw new InvalidOperationException(nameof(orders));
			}

			var emailSubject = $"{client.FullName} ({client.INN}). У вас имеется просроченная задолженность!";

			var emailAddress = _clientEmailSelector.SelectEmailForDebtNotification(client);
			if(string.IsNullOrWhiteSpace(emailAddress))
			{
				_logger.LogWarning("Клиент {ClientId} {ClientName} не имеет подходящего email для уведомления о задолженности", client.Id, client.FullName);
				throw new InvalidOperationException($"Клиент {client.Id} не имеет подходящего email для уведомления о задолженности");
			}

			var storedEmail = _emailMessageFactory.CreateStoredEmail(emailSubject, emailAddress, "Уведомление о ПДЗ");
			if(storedEmail is null)
			{
				_logger.LogError("Не удалось создать запись о письме с уведомлением о задолженности для клиента {ClientId}", client.Id);
				throw new InvalidOperationException($"StoredEmail не может быть пустым для клиента {client.Id}");
			}

			if(!storedEmail.Guid.HasValue)
			{
				_logger.LogError("StoredEmail.Guid пустой для письма о задолженности клиенту {ClientId}", client.Id);
				throw new InvalidOperationException($"StoredEmail.Guid не может быть пустым для клиента {client.Id}");
			}

			await uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);

			var totalDebt = ordersIdsWithDebt.Sum(o => o.Debt);

			var author = _employeeRepository.GetEmployeeForCurrentUser(uow);

			var orderWithoutShipmentForDebt = new OrderWithoutShipmentForDebt
			{
				Client = client,
				Organization = organization,
				DebtSum = totalDebt,
				Author = author
			};

			await uow.SaveAsync(orderWithoutShipmentForDebt, cancellationToken: cancellationToken);

			var bulkEmail = new InformationLetterEmail
			{
				StoredEmail = storedEmail,
				OrganizationId = organization.Id,
				Counterparty = client
			};

			await uow.SaveAsync(bulkEmail, cancellationToken: cancellationToken);

			await uow.CommitAsync(cancellationToken);

			var documentNumbersDict = await _documentOrganizationCounterRepository.GetDocumentNumbersByOrderIds(
				uow,
				orderIds,
				cancellationToken);

			var ordersWithDebts = (from order in orders
								  join debt in ordersIdsWithDebt on order.Id equals debt.OrderIds
								  select (order, debt.Debt)).ToList();

			var unsubscribeUrl = _emailLinkGenerator.GetUnsubscribeLink(storedEmail.Guid.Value);
			var messageText = _emailBodyGenerator.GenerateDebtEmailBody(client, ordersWithDebts, documentNumbersDict, unsubscribeUrl);

			await PublishDebtEmailMessageAsync(
					uow,
					storedEmail,
					client,
					organization,
					orders,
					orderWithoutShipmentForDebt,
					emailAddress,
					emailSubject,
					messageText,
					cancellationToken);
		}

		private async Task PublishDebtEmailMessageAsync(
			IUnitOfWork uow,
			StoredEmail storedEmail,
			Counterparty client,
			Organization organization,
			IEnumerable<Order> orders,
			OrderWithoutShipmentForDebt orderWithoutShipmentForDebt,
			string emailAddress,
			string emailSubject,
			string messageText,
			CancellationToken cancellationToken
			)
		{
			var attachment = _emailAttachmentsCreateService.CreateGeneralBillAttachments(
				client.Id,
				organization.Id,
				orders.Select(x => x.Id));

			var today = DateTime.Today;
			var yesterday = today.AddDays(-1);

			DateTime startDateForRevision;
			if(orders != null && orders.Any())
			{
				var earliestDeliveryDate = orders.Min(o => o.DeliveryDate ?? today);
				startDateForRevision = new DateTime(earliestDeliveryDate.Year, 1, 1);
			}
			else
			{
				startDateForRevision = new DateTime(today.Year, 1, 1);
			}

			var endDateForRevision = yesterday;

			var orderWithoutShipmentAttachments = _emailAttachmentsCreateService.CreateOrderWithoutShipmentForDebtAttachments(
				orderWithoutShipmentForDebt);

			var revisionAttachments = _emailAttachmentsCreateService.CreateRevisionAttachments(
				client.Id,
				organization.Id,
				startDateForRevision,
				endDateForRevision);

			var allAttachments = orderWithoutShipmentAttachments.Concat(revisionAttachments).ToList();

			var emailMessage = _emailMessageFactory.CreateSendEmailMessage(
				uow,
				storedEmail,
				client.FullName,
				organization.FullName,
				organization.EmailForMailing,
				allAttachments,
				emailAddress,
				emailSubject,
				messageText);

			await _bus.Publish(emailMessage, cancellationToken);
		}
	}
}
