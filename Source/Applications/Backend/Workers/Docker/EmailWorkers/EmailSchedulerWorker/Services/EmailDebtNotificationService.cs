using EdoDocumentsPreparer;
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
		private readonly IConfiguration _configuration;
		private readonly IEmailRepository _emailRepository;
		private readonly IBus _publishEndpoint;
		private readonly PrintableDocumentSaver _printableDocumentSaver;

		private const int _maxEmailsPerMinute = 10;

		public EmailDebtNotificationService(
			ILogger<EmailDebtNotificationService> logger,
			IUnitOfWork uow,
			IConfiguration configuration,
			IEmailRepository emailRepository,
			PrintableDocumentSaver printableDocumentSaver,
			IBus publishEndpoint
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_printableDocumentSaver = printableDocumentSaver ?? throw new ArgumentNullException(nameof(printableDocumentSaver));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		public async Task ProcessEmailQueueAsync(CancellationToken cancellationToken)
		{
			try
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
					if(cancellationToken.IsCancellationRequested)
					{
						break;
					}

					var client = clientOrders.Value.Item1;
					var organization = clientOrders.Value.Item2;
					var order = clientOrders.Key;

					await ProcessSingleClientEmailAsync(_uow, client, organization, order, cancellationToken);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка в процессе формирования очереди");
				throw;
			}
		}

		private async Task ProcessSingleClientEmailAsync(
			IUnitOfWork uow,
			Counterparty client,
			Organization organization,
			Order order,
			CancellationToken cancellationToken
			)
		{
			try
			{
				if(client == null)
				{
					_logger.LogWarning("Попытка отправить письмо несуществующему клиенту");
					return;
				}

				if(organization == null)
				{
					_logger.LogWarning("Попытка отправить письмо клиенту {ClientId} от несуществующей организации", client.Id);
					return;
				}

				if(order == null)
				{
					_logger.LogWarning("Попытка отправить письмо клиенту {ClientId} от организации {OrganizationId} по несуществующему заказу",
						client.Id,
						organization.Id);
					return;
				}

				var emailMessage = CreateDebtEmailMessage(client, organization, order, uow);

				await CreateCounterpartyDebtEmailRecordAsync(client, order.Id, emailMessage, cancellationToken);
				//await _uow.CommitAsync(cancellationToken);
				await _publishEndpoint.Publish(emailMessage, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке письма для клиента {ClientId}", client.Id);
				throw;
			}
		}

		private SendEmailMessage CreateDebtEmailMessage(Counterparty client, Organization organization, Order order, IUnitOfWork uow)
		{
			var instanceId = Convert.ToInt32(uow.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			var trackId = 0; // storedEmail.Id;

			var emailSubject = $"Информационное письмо о задолженности {client.Name} от {DateTime.Now:dd.MM.yyyy}";

			var emailAddress = SelectEmailForDebtNotification(client);
			if(string.IsNullOrWhiteSpace(emailAddress))
			{
				_logger.LogWarning("Клиент {ClientId} {ClientName} не имеет подходящего email для уведомления о задолженности", client.Id, client.FullName);
				throw new InvalidOperationException($"No suitable email address for client {client.Id}");
			}

			var unsubscribeToken = Guid.NewGuid().ToString();
			var unsubscribeUrl = $"{_configuration["Unsubscribe:BaseUrl"]}?email={client.Emails.FirstOrDefault()}&token={unsubscribeToken}";
			var messageText = GenerateDebtEmailBody(client, order, unsubscribeUrl);

			var attachment = CreateDebtEmailAttachment(order, organization);

			var sendEmailMessage = new SendEmailMessage()
			{
				From = new EmailContact
				{
					Name = organization.FullName,
					Email = organization.EmailForMailing,
				},

				To = new List<EmailContact>
				{
					new() {
						Name = client.FullName,
						//Email = emailAddress
						Email = "work.semen.sd@gmail.com"
					}
				},

				Subject = emailSubject,

				TextPart = messageText,
				HTMLPart = messageText,
				Payload = new EmailPayload
				{
					Id = trackId,
					Trackable = true,
					InstanceId = instanceId
				},
				Attachments = new List<EmailAttachment>() 
				{ 
					attachment 
				}
			};

			return sendEmailMessage;
		}

		private EmailAttachment CreateDebtEmailAttachment(Order order, Organization organization)
		{
			var letterOfDebtDocument = new LetterOfDebtDocument
			{
				Order = order,
				HideSignature = organization.DebtMailingWithSignature
			};

			byte[] pdfBytes = _printableDocumentSaver.SaveToPdf(letterOfDebtDocument);

			var attachment = new EmailAttachment
			{
				ContentType = "application/pdf",
				Filename = $"Письмо_о_задолженности_{order.Id}.pdf",
				Base64Content = Convert.ToBase64String(pdfBytes)
			};
			return attachment;
		}

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

		private string GenerateTrackId(int clientId) => $"{_configuration["EmailSettings:Login"]}-{DateTime.UtcNow:yyyyMMddHHmmss}-{clientId}";

		private string? SelectEmailForDebtNotification(Counterparty client)
		{
			if(client.Emails == null || !client.Emails.Any())
			{
				_logger.LogWarning("Клиент {ClientId} не имеет email адресов", client.Id);
				return null;
			}

			var billEmail = client.Emails
				.FirstOrDefault(x => x.EmailType?.EmailPurpose == EmailPurpose.ForBills)
				?.Address;

			if(!string.IsNullOrWhiteSpace(billEmail))
			{
				return billEmail;
			}

			var defaultEmail = client.Emails
				.FirstOrDefault(x => x.EmailType?.EmailPurpose == EmailPurpose.Default)
				?.Address;

			if(!string.IsNullOrWhiteSpace(defaultEmail))
			{
				return defaultEmail;
			}

			var otherEmail = client.Emails
				.FirstOrDefault()
				?.Address;

			if(!string.IsNullOrWhiteSpace(otherEmail))
			{
				return otherEmail;
			}

			return null;
		}

		/// <summary>
		/// Записывает в БД факт создания письма с уведомлением о задолженности
		/// </summary>
		private async Task CreateCounterpartyDebtEmailRecordAsync(
			Counterparty client,
			int ordersId,
			EmailMessage emailMessage,
			CancellationToken cancellationToken)
		{
			try
			{
				var storedEmail = new StoredEmail
				{
					State = StoredEmailStates.WaitingToSend,
					Author = null,
					ManualSending = false,
					SendDate = DateTime.Now,
					StateChangeDate = DateTime.Now,
					Subject = emailMessage.Subject,
					RecipientAddress = emailMessage?.To?.FirstOrDefault()?.Email,
					Guid = Guid.NewGuid(),
					Description = $"Уведомление о задолженности по заказам: {string.Join(", ", ordersId)}",
				};


				var bulkEmail = new BulkEmail
				{
					StoredEmail = storedEmail,
					Counterparty = client
				};

				await _uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);
				await _uow.SaveAsync(bulkEmail, cancellationToken: cancellationToken);

				_logger.LogInformation("Создана запись о письме с уведомлением о задолженности для контрагента {ClientId} по заказу {Order}",
					client.Id, ordersId);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					 "Ошибка при создании записи о письме с уведомлением о задолженности для контрагента {ClientId}",
					 client.Id);
				throw;
			}
		}
	}
}
