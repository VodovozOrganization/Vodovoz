using EdoService.Library.Services;
using Mailjet.Api.Abstractions;
using MassTransit;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;
using EmailAttachment = Mailjet.Api.Abstractions.EmailAttachment;

namespace EmailDebtNotificationWorker.Services
{
	/// <summary>
	/// Сервис планирования и отправки email сообщений клиентам о задолженностях
	/// </summary>
	public partial class EmailDebtNotificationService : IEmailDebtNotificationService
	{
		private readonly ILogger<EmailDebtNotificationService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IEmailRepository _emailRepository;
		private readonly IEmailSettings _emailSettings;
		private readonly PrintableDocumentSaver _printableDocumentSaver;
		private readonly IBus _bus;

		private const int _maxEmailsPerMinute = 5;

		public EmailDebtNotificationService(
			ILogger<EmailDebtNotificationService> logger,
			IUnitOfWork uow,
			IEmailRepository emailRepository,
			IEmailSettings emailSettings,
			PrintableDocumentSaver printableDocumentSaver,
			IBus bus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_printableDocumentSaver = printableDocumentSaver ?? throw new ArgumentNullException(nameof(printableDocumentSaver));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		public async Task ScheduleDebtNotificationsAsync(CancellationToken cancellationToken)
		{
			var overdueOrdersByClient = await _emailRepository.GetAllOverdueOrderForDebtNotificationAsync(_uow, _maxEmailsPerMinute, cancellationToken);
			if(!overdueOrdersByClient.Any())
			{
				_logger.LogDebug("Нет писем для массовой рассылки");
				return;
			}

			_logger.LogInformation("В процессе {Count} писем для рассылки",
				overdueOrdersByClient.Keys.Count);

			foreach(var clientOrders in overdueOrdersByClient)
			{
				try
				{
					if(cancellationToken.IsCancellationRequested)
					{
						break;
					}

					var client = clientOrders.Value.Counterparty;
					var organization = clientOrders.Value.Organization;
					var order = clientOrders.Key;

					await ProcessSingleClientEmailAsync(client, organization, order, cancellationToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке письма о задолженности");
				}
			}
		}

		private async Task ProcessSingleClientEmailAsync(
			Counterparty client,
			Organization organization,
			Order order,
			CancellationToken cancellationToken
			)
		{
			if(client == null)
			{
				_logger.LogWarning("Попытка отправить письмо несуществующему клиенту");
				throw new ArgumentNullException(nameof(client));
			}

			if(organization == null)
			{
				_logger.LogWarning("Попытка отправить письмо клиенту {ClientId} от несуществующей организации", client.Id);
				throw new ArgumentNullException(nameof(organization));
			}

			if(order == null)
			{
				_logger.LogWarning("Попытка отправить письмо клиенту {ClientId} от организации {OrganizationId} по несуществующему заказу",
					client.Id,
					organization.Id);
				throw new ArgumentNullException(nameof(order));
			}

			var emailSubject = $"Информационное письмо о задолженности {client.Name} от {DateTime.Now:dd.MM.yyyy}";

			var emailAddress = SelectEmailForDebtNotification(client);
			if(string.IsNullOrWhiteSpace(emailAddress))
			{
				_logger.LogWarning("Клиент {ClientId} {ClientName} не имеет подходящего email для уведомления о задолженности", client.Id, client.FullName);
				throw new InvalidOperationException($"Клиент {client.Id} не имеет подходящего email для уведомления о задолженности");
			}

			var storedEmail = CreateStoredEmail(emailSubject, emailAddress, order.Id);
			if(storedEmail == null)
			{
				_logger.LogError("Не удалось создать запись о письме с уведомлением о задолженности для клиента {ClientId}", client.Id);
				throw new ArgumentNullException(nameof(storedEmail));
			}

			var letterOfDebtDocument = new LetterOfDebtDocument
			{
				Order = order,
				HideSignature = !organization.DebtMailingWithSignature,
				AttachedToOrder = order
			};

			var bulkEmail = new BulkEmail
			{
				StoredEmail = storedEmail,
				Counterparty = client,
				OrderDocument = letterOfDebtDocument
			};

			var bulkEmailOrder = new BulkEmailOrder
			{
				BulkEmail = bulkEmail,
				Order = order
			};

			await _uow.SaveAsync(letterOfDebtDocument, cancellationToken: cancellationToken);
			await _uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);
			await _uow.SaveAsync(bulkEmail, cancellationToken: cancellationToken);
			await _uow.SaveAsync(bulkEmailOrder, cancellationToken: cancellationToken);

			await _uow.CommitAsync(cancellationToken);

			await PublishDebtEmailMessageAsync(
				storedEmail,
				client,
				organization,
				order,
				letterOfDebtDocument,
				emailAddress,
				emailSubject,
				cancellationToken);
		}

		private static StoredEmail CreateStoredEmail(string subject, string email, int orderId)
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
				Description = $"Уведомление о задолженности по заказу: {string.Join(", ", orderId)}",
			};

			return storedEmail;
		}

		private SendEmailMessage CreateDebtEmailMessage(
			StoredEmail storedEmail,
			Counterparty client,
			Organization organization,
			Order order,
			LetterOfDebtDocument letterOfDebt,
			string emailAddress,
			string emailSubject
			)
		{
			var instanceId = GetCurrentDatabaseId();

			var unsubscribeUrl = storedEmail.Guid.HasValue
				? GetUnsubscribeLink(storedEmail.Guid.Value)
				: string.Empty;

			if(!storedEmail.Guid.HasValue)
			{
				_logger.LogError("StoredEmail.Guid пустой для письма о задолженности клиенту {ClientId}", client.Id);
			}

			var messageText = GenerateDebtEmailBody(client, order, unsubscribeUrl);
			var attachment = CreateDebtEmailAttachment(order, letterOfDebt);

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
				Attachments = new List<EmailAttachment>()
				{
					attachment
				},
				Headers = new Dictionary<string, string>
				{
					{ "List-Unsubscribe", unsubscribeUrl }
				}
			};

			return sendEmailMessage;
		}

		private int GetCurrentDatabaseId()
		{
			var instanceId = Convert.ToInt32(
				_uow.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			return instanceId;
		}

		private string? SelectEmailForDebtNotification(Counterparty client)
		{
			if(client.Emails == null || !client.Emails.Any())
			{
				_logger.LogWarning("Клиент {ClientId} не имеет email адресов", client.Id);
				return null;
			}

			var billEmail = client.Emails
				.LastOrDefault(x => x.EmailType?.EmailPurpose == EmailPurpose.ForBills)
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

		private static string GenerateDebtEmailBody(Counterparty client, Order order, string unsubscribeUrl)
		{
			string deliveryDateFormatted = order.DeliveryDate?.ToString("dd.MM.yyyy")
				?? string.Empty;

			string dueDateFormatted = order.DeliveryDate?.AddDays(client.DelayDaysForBuyers).ToString("dd.MM.yyyy") ?? string.Empty;

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
						.order-list {{ margin: 15px 0; padding-left: 20px; }}
						.footer {{ margin-top: 30px; padding-top: 15px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
						.unsubscribe {{ color: #007bff; text-decoration: none; }}
					</style>
				</head>
				<body>
					<div class='container'>
						<div class='header'>
							<h2>Уважаемый клиент!</h2>
						</div>
        
						<div class='content'>
							<p>Мы хотим напомнить вам, что на текущий момент у вас имеется задолженность по заказу №{order.Id} от {deliveryDateFormatted},
							срок оплаты по которому истёк <strong>{dueDateFormatted}.</strong></p>
            
							<p>Мы ценим ваше сотрудничество и надеемся на оперативное урегулирование задолженности. 
							Для уточнения деталей Вы можете связаться с нами по телефону <strong>8 812-317-00-00 доб. 900</strong> 
							или электронной почте <strong>client.buh@vodovoz-spb.ru</strong>.</p>
            
							<p>Просим вас принять меры к погашению задолженности в ближайшее время, чтобы избежать возможных последствий, 
							включая, при необходимости, обращение в суд.</p>
            
							<p>Благодарим за внимание и надеемся на скорое решение вопроса.</p>
						</div>
        
						<div class='footer'>
							<p>Подробная информация о задолженности приложена к письму в формате PDF.</p>
							<p><a href='{unsubscribeUrl}' class='unsubscribe'>Отписаться от рассылки</a></p>
							<p>Вы всегда можете отписаться от нашей рассылки, нажав соответствующую кнопку.</p>
							<p><em>Это письмо отправлено автоматически.</em></p>
						</div>
					</div>
				</body>
				</html>";
		}

		private EmailAttachment CreateDebtEmailAttachment(Order order, LetterOfDebtDocument letterOfDebt)
		{
			byte[] pdfBytes = _printableDocumentSaver.SaveToPdf(letterOfDebt);

			var attachment = new EmailAttachment
			{
				ContentType = "application/pdf",
				Filename = $"Задолженность_{order.Id}.pdf",
				Base64Content = Convert.ToBase64String(pdfBytes)
			};
			return attachment;
		}

		private async Task PublishDebtEmailMessageAsync(
			StoredEmail storedEmail,
			Counterparty client,
			Organization organization,
			Order order,
			LetterOfDebtDocument letterOfDebtDocument,
			string emailAddress,
			string emailSubject,
			CancellationToken cancellationToken
			)
		{
			var emailMessage = CreateDebtEmailMessage(storedEmail, client, organization, order, letterOfDebtDocument, emailAddress, emailSubject);
			await _bus.Publish(emailMessage, cancellationToken);
		}
	}
}
