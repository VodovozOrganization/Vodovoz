using BitrixApi.Library.Services;
using EdoService.Library.Services;
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
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
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
		private readonly IEmailSettings _emailSettings;
		private readonly IEmailAttachmentsCreateService _emailAttachmentsCreateService;
		private readonly IBus _bus;

		private const int _maxEmailsPerMinute = 5;

		public EmailDebtNotificationService(
			ILogger<EmailDebtNotificationService> logger,
			IUnitOfWorkFactory uowFactory,
			IEmailRepository emailRepository,
			IDocumentOrganizationCounterRepository documentOrganizationCounterRepository,
			IEmailSettings emailSettings,
			IEmailAttachmentsCreateService emailAttachmentsCreateService,
			IBus bus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_documentOrganizationCounterRepository = documentOrganizationCounterRepository ?? throw new ArgumentNullException(nameof(documentOrganizationCounterRepository));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_emailAttachmentsCreateService = emailAttachmentsCreateService ?? throw new ArgumentNullException(nameof(emailAttachmentsCreateService));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
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

			var emailSubject = "У вас имеется просроченная дебиторская задолженность!";

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

			var bulkEmail = new GeneralBillDocumentEmail
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

		private static int GetCurrentDatabaseId(IUnitOfWork uow)
		{
			var instanceId = Convert.ToInt32(
				uow.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			return instanceId;
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
			string emailAddress,
			string emailSubject,
			string messageText
			)
		{
			var instanceId = GetCurrentDatabaseId(uow);

			var unsubscribeUrl = storedEmail.Guid.HasValue
				? GetUnsubscribeLink(storedEmail.Guid.Value)
				: string.Empty;

			var attachment = _emailAttachmentsCreateService.CreateGeneralBillAttachments(
				client.Id,
				organization.Id,
				orders.Select(x => x.Id));

			var sendEmailMessage = new SendEmailMessage()
			{
				From = new EmailContact
				{
					Name = organization.FullName,
					Email = organization.EmailForMailing,
				},
				To = new List<EmailContact>
				{
					new()
					{
						Name = client.FullName,
						//Email = emailAddress
						Email = "work.semen.sd@gmail.com",
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
				Attachments = attachment.ToList(),
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
						<td style='padding: 8px 0; text-align: right;'>{orderAmount:N2} руб. - </td>
						<td style='padding: 8px 0; text-align: center;'>{daysOverdue}</td>
					</tr>");
			}

			string contractNumber = orders
				.Select(o => o.Contract.Number)
				.FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "Не указан";

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
							<p>Сообщаем, что на текущий момент у вас имеется задолженность по договору <strong>{contractNumber}</strong> и следующим заказам:</p>
                    
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

							<p>Просим вас произвести оплату в кратчайшие сроки. Если оплата уже была направлена, пожалуйста, пришлите платежное поручение.</p>
                    
							<p>Также сообщаем, что в том случае, если просроченная задолженность по оплате по заказу составит 7 календарных дней, 
							мы будем вынуждены приостановить поставки продукции до момента полного погашения задолженности, в соответствии с условиями договора. 
							В случае поступления оплаты, поставки будут возобновлены.</p>
                    
							<p>При возникновении вопросов по сумме или срокам оплаты вы можете связаться с нами — будем рады помочь. 
							Благодарим за сотрудничество и рассчитываем на скорейшее урегулирование вопроса.</p>
                    
							<div class='signature'>
								<p>С уважением,<br />
								Отдел сопровождения клиентов<br />
								<span class='phone'>+7(812) 3170000 доб. 700</span><br />
								client.buh@vodovoz-spb.ru</p>
							</div>
						</div>

						<div class='footer'>
							<p><a href='{unsubscribeUrl}' class='unsubscribe'>Отписаться от рассылки</a></p>
							<p>Вы всегда можете отписаться от нашей рассылки, нажав соответствующую кнопку.</p>
							<p><em>Это письмо отправлено автоматически.</em></p>
						</div>
					</div>
				</body>
				</html>";
		}
	}
}
