using Gamma.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Sms.External.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Sms;
using Vodovoz.Infrastructure;
using Vodovoz.SmsInformerWorker.Options;
using Vodovoz.SmsInformerWorker.Services;
using Vodovoz.Zabbix.Sender;

namespace Vodovoz.SmsInformerWorker
{
	internal abstract class SmsInformerWorkerBase : TimerBackgroundServiceBase
	{
		protected readonly ILogger<SmsInformerWorkerBase> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		protected readonly ISmsSender _smsSender;
		private readonly ISmsBalanceNotifier _smsBalanceNotifier;
		private readonly ILowBalanceNotificationService _lowBalanceNotificationService;
		protected bool _sendingInProgress = false;

		public SmsInformerWorkerBase(
			IOptions<SmsInformerOptions> options,
			ILogger<SmsInformerWorkerBase> logger,
			IServiceScopeFactory serviceScopeFactory,
			ISmsSender smsSender,
			ISmsBalanceNotifier smsBalanceNotifier,
			ILowBalanceNotificationService lowBalanceNotificationService)
		{
			if(options is null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			Interval = options.Value.SmsScanInterval;

			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory
				?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_smsSender = smsSender
				?? throw new ArgumentNullException(nameof(smsSender));
			_smsBalanceNotifier = smsBalanceNotifier
				?? throw new ArgumentNullException(nameof(smsBalanceNotifier));
			_lowBalanceNotificationService = lowBalanceNotificationService
				?? throw new ArgumentNullException(nameof(lowBalanceNotificationService));

			_smsBalanceNotifier.OnBalanceChange -= _lowBalanceNotificationService.BalanceNotifierOnBalanceChange;
			_smsBalanceNotifier.OnBalanceChange += _lowBalanceNotificationService.BalanceNotifierOnBalanceChange;

			_logger.LogInformation("Запущена отправка смс уведомлений. Проверка новых уведомлений каждые {ScanInterval} сек.", Interval.TotalSeconds);
		}

		protected override TimeSpan Interval { get; }

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				_logger.LogDebug("Новый вызов отправки смс уведомлений");

				using var scope = _serviceScopeFactory.CreateScope();

				SendNewNotifications(scope.ServiceProvider, stoppingToken);

				await Task.CompletedTask;
			}
			catch(Exception ex)
			{
				_logger.LogCritical(ex, "Возвращено необработанное исключение при отправке сообщений!");
			}
		}

		public abstract IEnumerable<SmsNotification> GetNotifications(IUnitOfWork unitOfWork, IServiceProvider serviceProvider);

		private void SendNewNotifications(IServiceProvider serviceProvider, CancellationToken stoppingToken)
		{
			var zabbixSender = serviceProvider.GetRequiredService<IZabbixSender>();
			var unitOfWorkFactory = serviceProvider.GetRequiredService<IUnitOfWorkFactory>();

			zabbixSender.SendIsHealthyAsync(stoppingToken);

			if(_sendingInProgress)
			{
				_logger.LogWarning("Отменена отправка до завершения предыдущей. Проверьте настройки интервала отправки и состояние провайдера отправки сообщений");
				return;
			}

			_sendingInProgress = true;

			try
			{
				using var unitOfWork = unitOfWorkFactory
					.CreateWithoutRoot(nameof(SmsInformerWorkerBase));

				var newNotifications = GetNotifications(unitOfWork, serviceProvider);

				if(!newNotifications.Any())
				{
					return;
				}

				//закрытие просроченных уведомлений
				CloseExpiredNotifications(unitOfWork, newNotifications);

				newNotifications = newNotifications
					.Where(x => x.Status == SmsNotificationStatus.New)
					.ToList();

				foreach(var notification in newNotifications)
				{
					SendNotification(notification);
					unitOfWork.Save(notification);
				}

				unitOfWork.Commit();
			}
			catch(Exception ex)
			{
				_logger.LogCritical(ex, ex.Message);
			}
			finally
			{
				_sendingInProgress = false;
			}
		}

		public virtual void SendNotification(SmsNotification notification)
		{
			try
			{
				SmsMessage smsMessage = new SmsMessage(notification.MobilePhone, notification.Id.ToString(), notification.MessageText);

				var result = _smsSender.SendSms(smsMessage);
				_logger.LogInformation("Отправлено уведомление новому клиенту. Тел.: {MobilePhoneNumber}, результат: {SendingResult}", smsMessage.MobilePhoneNumber, result.GetEnumTitle());

				if(result.IsSuccefullStatus())
				{
					notification.Status = SmsNotificationStatus.Accepted;
				}
				else
				{
					notification.ErrorDescription = result.GetEnumTitle();
					notification.Status = SmsNotificationStatus.Error;
				}
			}
			catch(Exception ex)
			{
				notification.Status = SmsNotificationStatus.Error;
				notification.ErrorDescription = $"Ошибка при отправке смс сообщения. {ex.Message}";
				_logger.LogError(ex, notification.ErrorDescription);
			}
		}

		private void CloseExpiredNotifications(
			IUnitOfWork unitOfWork,
			IEnumerable<SmsNotification> notifications)
		{
			var expiredNotifications = notifications
				.Where(x => x.Status == SmsNotificationStatus.New)
				.Where(x => x.ExpiredTime < DateTime.Today);

			if(expiredNotifications.Any())
			{
				_logger.LogInformation(
					"Были закрыты без отправки следующие просроченные уведомления: {ExpiredNotifications}",
					string.Join(", ", expiredNotifications.Select(x => x.Id)));
			}
			foreach(var expiredNotification in expiredNotifications)
			{
				expiredNotification.Status = SmsNotificationStatus.SendExpired;
				unitOfWork.Save(expiredNotification);
			}
		}

		protected override void OnStopService()
		{
			_smsBalanceNotifier.OnBalanceChange -= _lowBalanceNotificationService.BalanceNotifierOnBalanceChange;

			base.OnStopService();
		}
	}
}
