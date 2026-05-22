using BitrixApi.Library.Services;
using EmailDebtNotificationWorker.Repositories;
using Mailjet.Api.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories.Document;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Settings.Common;

namespace EmailDebtNotificationWorker.Services
{
	/// <summary>
	/// Сервис планирования и отправки email сообщений клиентам о задолженностях
	/// </summary>
	public partial class EmailDebtNotificationService : IEmailDebtNotificationService
	{
		private readonly ILogger<EmailDebtNotificationService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IEmailRepository _emailRepository;
		private readonly IDocumentOrganizationCounterRepository _documentOrganizationCounterRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IEmailSettings _emailSettings;
		private readonly IEmailAttachmentsCreateService _emailAttachmentsCreateService;
		private readonly IBus _bus;
		private readonly IDatabaseRepository _databaseRepository;
		private const int _maxEmailsPerMinute = 5;

		public EmailDebtNotificationService(
			ILogger<EmailDebtNotificationService> logger,
			IUnitOfWorkFactory uowFactory,
			IEmailRepository emailRepository,
			IDocumentOrganizationCounterRepository documentOrganizationCounterRepository,
			IEmployeeRepository employeeRepository,
			IEmailSettings emailSettings,
			IEmailAttachmentsCreateService emailAttachmentsCreateService,
			IBus bus,
			IDatabaseRepository databaseRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_documentOrganizationCounterRepository = documentOrganizationCounterRepository ?? throw new ArgumentNullException(nameof(documentOrganizationCounterRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_emailAttachmentsCreateService = emailAttachmentsCreateService ?? throw new ArgumentNullException(nameof(emailAttachmentsCreateService));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
			_databaseRepository = databaseRepository ?? throw new ArgumentNullException(nameof(databaseRepository));
		}

		public async Task ScheduleDebtNotificationsAsync(CancellationToken cancellationToken)
		{
			using var uow = _uowFactory.CreateWithoutRoot("Получение списка должников в воркере по рассылке писем о задолженнности");

			var overdueOrdersByClient = await _emailRepository.GetAllOverdueOrdersForDebtNotificationAsync(uow, _maxEmailsPerMinute, cancellationToken);
			if(!overdueOrdersByClient.Any())
			{
				_logger.LogDebug("Нет писем для массовой рассылки");
				return;
			}

			_logger.LogInformation("В процессе {Count} писем для рассылки",
				overdueOrdersByClient.Keys.Count);

			var clientByOrganizationDict = overdueOrdersByClient.Keys;
			foreach(var clientByOrganization in clientByOrganizationDict)
			{
				try
				{
					using var clientUow = _uowFactory.CreateWithoutRoot($"Отправка письма клиенту в воркере по рассылке писем о задолженнности");

					var client = clientByOrganization.Counterparty;
					var organization = clientByOrganization.Organization;

					if(overdueOrdersByClient.TryGetValue(clientByOrganization, out var orders))
					{
						await ProcessSingleClientEmailAsync(clientUow, client, organization, orders, cancellationToken);
					}
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке письма о задолженности");
				}
			}
		}

		private async Task ProcessSingleClientEmailAsync(
			IUnitOfWork uow,
			Counterparty client,
			Organization organization,
			IEnumerable <Order> orders,
			CancellationToken cancellationToken
			)
		{
			if(client is null)
			{
				_logger.LogWarning("Попытка отправить письмо несуществующему клиенту");
				throw new ArgumentNullException(nameof(client));
			}

			if(organization is null)
			{
				_logger.LogWarning("Попытка отправить письмо клиенту {ClientId} от несуществующей организации", client.Id);
				throw new ArgumentNullException(nameof(organization));
			}

			if(orders is null)
			{
				_logger.LogWarning("Попытка отправить письмо клиенту {ClientId} от организации {OrganizationId} по несуществующему заказу",
					client.Id,
					organization.Id);
				throw new ArgumentNullException(nameof(orders));
			}

			var emailSubject = $"{client.FullName} ({client.INN}). У вас имеется просроченная задолженность!";

			var emailAddress = SelectEmailForDebtNotification(client);
			if(string.IsNullOrWhiteSpace(emailAddress))
			{
				_logger.LogWarning("Клиент {ClientId} {ClientName} не имеет подходящего email для уведомления о задолженности", client.Id, client.FullName);
				throw new InvalidOperationException($"Клиент {client.Id} не имеет подходящего email для уведомления о задолженности");
			}

			var storedEmail = CreateStoredEmail(emailSubject, emailAddress);
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

			var totalDebt = orders.Sum(o => o.OrderSum);

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
				Counterparty = client
			};

			await uow.SaveAsync(bulkEmail, cancellationToken: cancellationToken);

			await uow.CommitAsync(cancellationToken);

			var documentNumbersDict = await _documentOrganizationCounterRepository.GetDocumentNumbersByOrderIds(
				uow,
				orders.Select(x => x.Id),
				cancellationToken);

			var unsubscribeUrl = GetUnsubscribeLink(storedEmail.Guid.Value);
			var messageText = GenerateDebtEmailBody(client, orders, documentNumbersDict, unsubscribeUrl);

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

		private static StoredEmail CreateStoredEmail(string subject, string email)
		{
			var storedEmail = new StoredEmail
			{
				State = StoredEmailStates.SendingComplete,
				Author = null,
				ManualSending = false,
				SendDate = DateTime.Now,
				StateChangeDate = DateTime.Now,
				Subject = subject,
				RecipientAddress = email,
				Guid = Guid.NewGuid(),
				Description = $"Уведомление о ПДЗ",
			};

			return storedEmail;
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

		private string GetUnsubscribeLink(Guid guid) => $"{_emailSettings.UnsubscribeUrl}/{guid}";

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
			var emailMessage = CreateDebtEmailMessage(
				uow,
				storedEmail,
				client,
				organization,
				orders,
				orderWithoutShipmentForDebt,
				emailAddress,
				emailSubject,
				messageText);

			await _bus.Publish(emailMessage, cancellationToken);
		}

		private SendEmailMessage CreateDebtEmailMessage(
			IUnitOfWork uow,
			StoredEmail storedEmail,
			Counterparty client,
			Organization organization,
			IEnumerable<Order> orders,
			OrderWithoutShipmentForDebt orderWithoutShipmentForDebt,
			string emailAddress,
			string emailSubject,
			string messageText
			)
		{
			var instanceId = _databaseRepository.GetCurrentDatabaseId(uow);

			var unsubscribeUrl = storedEmail.Guid.HasValue
				? GetUnsubscribeLink(storedEmail.Guid.Value)
				: string.Empty;

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

			var sendEmailMessage = new SendEmailMessage()
			{
				From = new EmailContact
				{
					Name = organization.FullName,
					Email = organization.EmailForInformationLetters,
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
				Attachments = allAttachments,
				Headers = new Dictionary<string, string>
				{
					{ "List-Unsubscribe", unsubscribeUrl }
				}
			};

			return sendEmailMessage;
		}

		private static string GenerateDebtEmailBody(Counterparty client, IEnumerable<Order> orders, Dictionary<int, string> documentNumbersDict, string unsubscribeUrl)
		{
			var ordersList = orders.ToList();

			var ordersHtml = new StringBuilder();
			decimal totalDebt = 0;

			foreach(var order in ordersList)
			{
				var deliveryDate = order.DeliveryDate ?? DateTime.Today;
				var dueDate = deliveryDate.AddDays(client.DelayDaysForBuyers);
				var daysOverdue = (DateTime.Today - dueDate).Days;
				var orderAmount = order.OrderSum;

				totalDebt += orderAmount;

				var documentNumber = documentNumbersDict.GetValueOrDefault(order.Id);
				if(string.IsNullOrWhiteSpace(documentNumber))
				{
					documentNumber = order.Id.ToString();
				}

				ordersHtml.AppendLine($@"
					<tr>
						<td style='padding: 8px 0;'>№ {documentNumber}</td>
						<td style='padding: 8px 0; text-align: right;'>{orderAmount:N2} руб.</td>
						<td style='padding: 8px 0; text-align: center;'>{daysOverdue}</td>
					</tr>");
			}

			string organizationName = orders
				.Where(o => o?.Contract?.Organization?.FullName != null)
				.Select(o => o.Contract.Organization.FullName)
				.FirstOrDefault() ?? "Не указана";

			return $@"
				<!DOCTYPE html>
				<html>
				<head>
					<meta charset='utf-8'>
					<style>
						body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
						.container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
						.header {{ background-color: #f8f9fa; padding: 20px; border-radius: 5px; }}
						.content {{ margin: 20px 0; }}
						.debt-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
						.debt-table th {{ background-color: #e9ecef; padding: 10px; text-align: left; }}
						.debt-table td {{ border-bottom: 1px solid #dee2e6; }}
						.total-row {{ font-weight: bold; background-color: #f8f9fa; }}
						.footer {{ margin-top: 30px; padding-top: 15px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
						.unsubscribe {{ color: #007bff; text-decoration: none; }}
						.phone {{ white-space: nowrap; }}
						.signature {{ margin-top: 20px; }}
					</style>
				</head>
				<body>
					<div class='container'>
						<div class='header'>
							<h2>Уважаемый клиент!</h2>
						</div>

						<div class='content'>
							<p>На данный момент у вас имеется задолженность перед <strong>{organizationName}</strong> по следующим заказам:</p>
                    
							<table class='debt-table'>
								<thead>
									<tr>
										<th>№ заказа</th>
										<th>Сумма заказа</th>
										<th>Дней после истечения отсрочки</th>
									</tr>
								</thead>
								<tbody>
									{ordersHtml}
								</tbody>
								<tfoot>
									<tr class='total-row'>
										<td style='padding: 10px 0;'><strong>Общая задолженность:</strong></td>
										<td style='padding: 10px 0; text-align: right;'><strong>{totalDebt:N2} руб.</strong></td>
										<td style='padding: 10px 0;'></td>
									</tr>
								</tfoot>
							</table>

							<p>Просим оплатить задолженность в ближайшее время. Если вы уже произвели оплату, пожалуйста, направьте подтверждение платежи.</p>
                    
							<p>Обращаем внимание: если просрочка по оплате превысит 7 календарных дней, 
								поставки продукции будут приостановлены до полного погашения долга в соответствии с условиями договора. 
								После поступления оплаты поставки будут возобновлены.</p>
                    
							<p>Если у вас есть вопросы по сумме или срокам оплаты, свяжитесь с нами - мы будем рады помочь.</p>
                    
							<div class='signature'>
								<p>С уважением,<br />
								Отдел сопровождения клиентов<br />
								<span class='phone'>+7(812) 3170000 доб. 700</span><br />
								client.buh@vodovoz-spb.ru</p>
							</div>
						</div>

						<div class='footer'>
							<p><a href='{unsubscribeUrl}' class='unsubscribe'>Отписаться от рассылки</a></p>
							<p>Вы можете отказаться от рассылки, воспользовавшись соответствующей ссылкой в письме.</p>
							<p><em>Это письмо отправлено автоматически.</em></p>
						</div>
					</div>
				</body>
				</html>";
		}
	}
}
