using EmailSchedulerWorker.Consumers;
using EmailSend.Library.Services;
using Mailganer.Api.Client.Dto;
using MassTransit;
using QS.DomainModel.UoW;
using System.Text;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using static EmailSchedulerWorker.Services.EmailSchedulingService;
using EmailAttachment = Mailganer.Api.Client.Dto.EmailAttachment;

namespace EmailSchedulerWorker.Services
{
	/// <summary>
	/// Сервис планирования и отправки email сообщений клиентам
	/// </summary>
	public partial class EmailSchedulingService : IEmailSchedulingService, IDisposable
	{
		private readonly ILogger<EmailSchedulingService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IConfiguration _configuration;
		private readonly IEmailRepository _emailRepository;
		private readonly IPdfGenerationService _pdfGenerationService;
		private readonly IPublishEndpoint _publishEndpoint;

		/// <summary>
		/// Надо ли, если мы ограничиваем количество писем в минуту через RabbitMQ?
		/// </summary>
		private const int _maxEmailsPerMinute = 10;

		public EmailSchedulingService(
			ILogger<EmailSchedulingService> logger,
			IUnitOfWork uow,
			IConfiguration configuration,
			IEmailRepository emailRepository,
			IPdfGenerationService pdfGenerationService,
			IPublishEndpoint publishEndpoint
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_pdfGenerationService = pdfGenerationService ?? throw new ArgumentNullException(nameof(pdfGenerationService));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		public async Task ProcessEmailQueueAsync(CancellationToken cancellationToken)
		{
			try
			{
				var overdueOrdersByClient = await _emailRepository.GetAllOverdueOrdersForDebtNotificationAsync(_uow, _maxEmailsPerMinute);
				if(!overdueOrdersByClient.Any())
				{
					_logger.LogDebug("Нет клиентов для массовой рассылки");
					return;
				}

				_logger.LogInformation("В процессе {Count} клиентов для рассылки",
					overdueOrdersByClient.Count);

				foreach(var clientOrders in overdueOrdersByClient)
				{
					if(cancellationToken.IsCancellationRequested)
					{
						break;
					}

					var client = clientOrders.Key;
					var orders = clientOrders.Value;

					await ProcessSingleClientEmailAsync(_uow, client, orders, cancellationToken);
				}

				await _uow.CommitAsync(cancellationToken);
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
			List<Order> overdueOrders,
			CancellationToken cancellationToken
			)
		{
			try
			{
				var freshClient = await uow.Session.GetAsync<Counterparty>(client.Id, cancellationToken: cancellationToken);
				if(freshClient == null)
				{
					_logger.LogDebug("Клиент {ClientId} не подписан", client.Id);
					return;
				}

				if(!overdueOrders.Any())
				{
					_logger.LogDebug("Нет просроченных заказов по клиенту {ClientId}", client.Id);
					return;
				}

				await SendDebtNotificationForOrdersAsync(client, overdueOrders, cancellationToken);

				//await _emailRepository.MarkOrdersAsSentAsync(uow, overdueOrders.Select(o => o.Id).ToList(), DateTime.UtcNow, cancellationToken);

				//await CreateCounterpartyDebtEmailRecordAsync(uow, client, overdueOrders, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing email for client {ClientId}", client.Id);
				return;
			}
		}

		private async Task SendDebtNotificationForOrdersAsync(Counterparty client, List<Order> orders, CancellationToken cancellationToken)
		{
			try
			{
				var pdfAttachment = CreateDebtPdfAttachmentAsync(client, orders);

				var emailMessage = CreateDebtEmailMessage(client, orders, pdfAttachment);

				var message = new ProcessClientEmailEvent
				{
					ClientId = client.Id,
					OrderIds = orders.Select(o => o.Id).ToList(),
					EmailMessage = emailMessage
				};

				await _publishEndpoint.Publish(message, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error sending debt notification for client {ClientId}", client.Id);
			}
		}

		private EmailMessage CreateDebtEmailMessage(Counterparty client, List<Order> orders, EmailAttachment pdfAttachment)
		{
			var emailAddress = SelectEmailForDebtNotification(client);

			if(string.IsNullOrWhiteSpace(emailAddress))
			{
				_logger.LogWarning("Client {ClientId} ({ClientName}) has no suitable email address for debt notification",
					client.Id, client.Name);
				throw new InvalidOperationException($"No suitable email address for client {client.Id}");
			}

			var unsubscribeToken = Guid.NewGuid().ToString();
			var unsubscribeUrl = $"{_configuration["Unsubscribe:BaseUrl"]}?email={client.Emails.FirstOrDefault()}&token={unsubscribeToken}";

			var emailSubject = $"Информационное письмо о задолженности {client.Name} от {DateTime.Now:dd.MM.yyyy}";

			return new EmailMessage
			{
				From = _configuration["EmailSettings:FromAddress"],
				FromAddress = _configuration["EmailSettings:FromAddress"],
				To = emailAddress,
				Subject = emailSubject,
				MessageText = GenerateDebtEmailBody(client, orders, unsubscribeUrl),
				CheckStopList = true,
				CheckLocalStopList = true,
				TrackId = GenerateTrackId(client.Id),
				Headers = new Dictionary<string, string>
				{
					["List-Unsubscribe"] = $"<{unsubscribeUrl}>",
					["List-Unsubscribe-Post"] = "List-Unsubscribe=One-Click",
					["Precedence"] = "bulk"
				},
				UnsubscribeUrl = unsubscribeUrl,
				Attachments = new List<EmailAttachment> { pdfAttachment }
			};
		}


		private EmailAttachment CreateDebtPdfAttachmentAsync(Counterparty client, List<Order> orders)
		{
			try
			{
				var pdfContent = _pdfGenerationService.GenerateDebtNotificationPdfAsync(client, orders);

				return new EmailAttachment
				{
					Filename = $"Задолженность_{client.Name}_{DateTime.Now:yyyyMMdd}.pdf",
					Base64Content = pdfContent.ToString()
				};
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error creating PDF attachment for client {ClientId}", client.Id);
				throw;
			}
		}

		private static string GenerateDebtEmailBody(Counterparty client, List<Order> orders, string unsubscribeUrl)
		{
			var orderDetails = new StringBuilder();

			foreach(var order in orders)
			{
				var dueDate = order.DeliveryDate;
				orderDetails.AppendLine($"<div>• Заказ №{order.Id} от {order.DeliveryDate:dd.MM.yyyy} (срок оплаты: {dueDate:dd.MM.yyyy})</div>");
			}

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
							<h2>Уважаемый(ая) {client.Name}!</h2>
						</div>
        
						<div class='content'>
							<p>Мы хотим напомнить вам, что на текущий момент у вас имеется задолженность по следующим заказам:</p>
            
							<div class='order-list'>
								{orderDetails}
							</div>
            
							<p><strong>Внимание!</strong> Срок оплаты по указанным заказам истек.</p>
            
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
				_logger.LogWarning("Client {ClientId} has no emails at all", client.Id);
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

			var untypedEmail = client.Emails
				.FirstOrDefault(x => x.EmailType == null)
				?.Address;

			if(!string.IsNullOrWhiteSpace(untypedEmail))
			{
				return untypedEmail;
			}

			return null;
		}

		private async Task<bool> IsClientStillSubscribedAsync(IUnitOfWork uow, int clientId)
		{
			try
			{
				var client = uow.Session.Get<Counterparty>(clientId);
				return client != null /*&&
					   client.IsSubscribed &&
					   client.UnsubscribeDate == null*/;
			}
			catch
			{
				return false;
			}
		}

		public async Task<int> GetPendingEmailsCountAsync()
		{
			try
			{
				var cutoffDate = DateTime.UtcNow.AddDays(-1);

				/*var count = _uow.Session.QueryOver<Counterparty>()
					*//*.Where(c => c.IsSubscribed == true)
					.Where(c => c.IsActive == true)
					.Where(c => c.Email != null && c.Email != string.Empty)
					.Where(c => c.EmailConfirmed == true)
					.Where(c => c.LastEmailSent == null || c.LastEmailSent < cutoffDate)
					.Where(c => c.EmailSendCount < MAX_EMAILS_PER_CLIENT)
					.Where(c => c.UnsubscribeDate == null)*//*
					.RowCount();*/
				var count = await _emailRepository.GetPendingEmailsCountAsync(_uow, cutoffDate);
				return count;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error getting pending emails count");
				return 0;
			}
		}

		public void Dispose() => _uow.Dispose();
	}
}
