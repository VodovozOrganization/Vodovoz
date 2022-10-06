using System;
using System.ServiceModel;
using NLog;
using SmsSendInterface;

namespace VodovozInstantSmsService
{
	public class InstantSmsInformerService : IInstantSmsInformerService
	{
		public InstantSmsInformerService(ServiceHost instantSmsServiceHost, ISmsSender smsSender)
		{
			this.instantSmsServiceHost = instantSmsServiceHost ?? throw new ArgumentNullException(nameof(instantSmsServiceHost));
			this.smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
		}

		ServiceHost instantSmsServiceHost;
		ISmsSender smsSender;
		ILogger logger = LogManager.GetCurrentClassLogger();
		int minBalanceValue = 5;

		public bool ServiceStatus()
		{
			logger.Info("Запрос статуса службы моментальных смс уведомлений");
			try {
				if(instantSmsServiceHost.State != CommunicationState.Opened) {
					logger.Info($"Хост сервиса моментальных sms сообщений находится в состоянии {instantSmsServiceHost.State}");
					return false;
				}
				BalanceResponse balanceResponse = smsSender.GetBalanceResponse;
				if(balanceResponse.Status == BalanceResponseStatus.Error) {
					logger.Info($"Ошибка запроса баланса");
					return false;
				}
				if(balanceResponse.BalanceValue < minBalanceValue) {
					logger.Info($"Баланс на счёте менее {minBalanceValue} рублей");
					return false;
				}
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при проверке работоспособности службы смс уведомлений");
				return false;
			}
			return true;
		}
	}
}
