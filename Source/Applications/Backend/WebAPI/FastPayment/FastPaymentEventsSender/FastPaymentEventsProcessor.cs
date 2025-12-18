using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastPaymentEventsSender.Notifications;
using FastPaymentEventsSender.Options;
using Mailjet.Api.Abstractions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Settings.Common;

namespace FastPaymentEventsSender
{
	public class FastPaymentEventsProcessor : BackgroundService
	{
		private const string _changedStatusEventsProcessTitle = "Обработка событий изменения статуса оплаты";
		private const string _wrongSignatureEventsProcessTitle = "Обработка событий неправильной подписи";

		private readonly ILogger<FastPaymentEventsProcessor> _logger;
		private readonly IOptionsMonitor<SenderOptions> _senderOptionsMonitor;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGenericRepository<FastPaymentStatusUpdatedEvent> _statusUpdatedEventRepository;
		private readonly IGenericRepository<WrongSignatureFromReceivedFastPaymentEvent> _wrongSignatureEventRepository;
		private readonly IEmailSettings _emailSettings;

		public FastPaymentEventsProcessor(
			ILogger<FastPaymentEventsProcessor> logger,
			IOptionsMonitor<SenderOptions> senderOptionsMonitor,
			IServiceScopeFactory serviceScopeFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IGenericRepository<FastPaymentStatusUpdatedEvent> statusUpdatedEventRepository,
			IGenericRepository<WrongSignatureFromReceivedFastPaymentEvent> wrongSignatureEventRepository,
			IEmailSettings emailSettings
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_senderOptionsMonitor = senderOptionsMonitor ?? throw new ArgumentNullException(nameof(senderOptionsMonitor));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_statusUpdatedEventRepository = statusUpdatedEventRepository ?? throw new ArgumentNullException(nameof(statusUpdatedEventRepository));
			_wrongSignatureEventRepository = wrongSignatureEventRepository ?? throw new ArgumentNullException(nameof(wrongSignatureEventRepository));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
		}
		
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

				await ProcessFastPaymentStatusUpdatedEvent();
				await ProcessWrongSignatureFromReceivedFastPaymentEvent();

				var delay = _senderOptionsMonitor.CurrentValue.DelayInSeconds;
				_logger.LogInformation("Ожидаем {Delay}сек перед следующим запуском", delay);
				await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
			}
		}

		private async Task ProcessFastPaymentStatusUpdatedEvent()
		{
			try
			{
				_logger.LogInformation(_changedStatusEventsProcessTitle);
				using var uow = _unitOfWorkFactory.CreateWithoutRoot(_changedStatusEventsProcessTitle);
				using var scope = _serviceScopeFactory.CreateScope();
				
				var groupedEvents = _statusUpdatedEventRepository.Get(uow, x => x.HttpCode == null)
					.ToLookup(x => x.FastPayment.Id);
				
				var eventsCount = groupedEvents.Count;
				var notifier = scope.ServiceProvider.GetRequiredService<IFastPaymentStatusUpdatedNotifier>();

				_logger.LogInformation("Всего событий смены статуса: {ChangedStatusEventsCount}", eventsCount);
				
				foreach(var eventsGroup in groupedEvents)
				{
					var i = 0;
					var groupedCount = eventsGroup.Count();

					foreach(var @event in eventsGroup)
					{
						/*т.к. теоретически может быть несколько событий по одному быстрому платежу,
						 то отправляем последнее, а всем остальным проставляем не отправку*/
						if(i == groupedCount - 1)
						{
							await notifier.NotifyPaymentStatusChangeAsync(@event);
						}
						else
						{
							@event.DriverNotified = false;
							@event.HttpCode = 0;
						}
						
						await uow.SaveAsync(@event);
						await uow.CommitAsync();
						i++;
					}
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при обработке событий изменения статуса оплаты");
			}
		}
		
		private async Task ProcessWrongSignatureFromReceivedFastPaymentEvent()
		{
			try
			{
				_logger.LogInformation(_wrongSignatureEventsProcessTitle);
				using var uow = _unitOfWorkFactory.CreateWithoutRoot(_wrongSignatureEventsProcessTitle);
				using var scope = _serviceScopeFactory.CreateScope();
				
				var events = _wrongSignatureEventRepository.Get(uow, x => x.SentDate == null).ToList();
				var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
				var eventsCount = events.Count;

				_logger.LogInformation("Всего событий смены статуса: {WrongSignatureEventsCount}", eventsCount);
				
				foreach(var @event in events)
				{
					var message = CreateSendEmailMessage(@event);
					await publishEndpoint.Publish(message);

					@event.SentDate = DateTime.Now;
					await uow.SaveAsync(@event);
					await uow.CommitAsync();
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при обработке событий неправильной подписи");
			}
		}

		private SendEmailMessage CreateSendEmailMessage(WrongSignatureFromReceivedFastPaymentEvent @event)
		{
			var messageText = $"Оповещение о пришедшей оплате с неверной подписью: {@event.BankSignature}" +
				$" для платежа по заказу №{@event.OrderNumber}, shopId {@event.ShopId}, рассчитанная подпись {@event.GeneratedSignature}";

			return new SendEmailMessage
			{
				From = new EmailContact
				{
					Name = _emailSettings.DocumentEmailSenderName,
					Email = _emailSettings.DocumentEmailSenderAddress
				},

				To = new List<EmailContact>
				{
					new EmailContact
					{
						Name = "Уважаемый пользователь",
						Email = _emailSettings.InvalidSignatureNotificationEmailAddress
					}
				},

				Subject = $"Неккоректная подпись успешной оплаты заказа №{@event.OrderNumber}",

				TextPart = messageText,
				HTMLPart = messageText,
				Payload = new EmailPayload
				{
					Id = 0,
					Trackable = false
				}
			};
		}
	}
}
