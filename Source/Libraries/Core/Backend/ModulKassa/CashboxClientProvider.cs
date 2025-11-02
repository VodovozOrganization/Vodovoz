using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ModulKassa
{
	public class CashboxClientProvider : IDisposable
	{
		private readonly ILogger<CashboxClientProvider> _logger;
		private readonly ILoggerFactory _loggerFactory;
		private readonly IOptionsMonitor<CashboxesSetting> _settingsMonitor;
		private readonly IDisposable _settingsMonitorListener;

		private Dictionary<int, CashboxClientContainer> _cashboxes = new Dictionary<int, CashboxClientContainer>();

		public CashboxClientProvider(
			ILoggerFactory loggerFactory,
			IOptionsMonitor<CashboxesSetting> settingsMonitor
		)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_settingsMonitor = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
			_logger = _loggerFactory.CreateLogger<CashboxClientProvider>();

			_settingsMonitorListener = _settingsMonitor.OnChange(UpdateCashboxesSettings);

			UpdateCashboxesSettings(_settingsMonitor.CurrentValue);
		}

		private void UpdateCashboxesSettings(CashboxesSetting setting)
		{
			var newCashboxes = new Dictionary<int, CashboxClientContainer>();

			foreach(var cashboxSetting in setting.CashboxSettings)
			{
				var cashboxClientLogger = _loggerFactory.CreateLogger<CashboxClient>();
				var cashboxClient = new CashboxClient(cashboxClientLogger, cashboxSetting);
				var container = new CashboxClientContainer(cashboxSetting, cashboxClient);

				if(newCashboxes.ContainsKey(container.Setting.CashBoxId))
				{
					var ex = new CashboxException($"Невозможно установить настройки. " +
						$"Кассовый аппарат с идентификатором {cashboxSetting.CashBoxId} уже добавлен.");
					_logger.LogError(ex, "");
					throw ex;
				}

				newCashboxes.Add(container.Setting.CashBoxId, container);
				CheckFiscalization(container, CancellationToken.None).Wait();
			}

			foreach(var oldCashbox in _cashboxes)
			{
				if(newCashboxes.ContainsKey(oldCashbox.Key))
				{
					_cashboxes[oldCashbox.Key] = newCashboxes[oldCashbox.Key];
					newCashboxes.Remove(oldCashbox.Key);
				}
				else
				{
					_cashboxes.Remove(oldCashbox.Key);
				}
			}

			foreach(var newCashbox in newCashboxes)
			{
				_cashboxes.Add(newCashbox.Key, newCashbox.Value);
			}
		}



		public async Task<CashboxClient> GetCashboxAsync(int cashboxId, CancellationToken cancellationToken)
		{
			if(!_cashboxes.TryGetValue(cashboxId, out CashboxClientContainer cashboxContainer))
			{
				throw new CashboxException($"Не найден кассовый аппарат с идентификатором {cashboxId}.");
			}

			if(!cashboxContainer.IsActive || cashboxContainer.FiscalizationCheckRequired)
			{
				await CheckFiscalization(cashboxContainer, cancellationToken);
			}

			if(!cashboxContainer.IsActive)
			{
				throw new CashboxException($"Состояние кассового аппарата " +
					$"№{cashboxId} ({cashboxContainer.Setting.RetailPointName}) не позволяет фискализировать чеки.");
			}

			return cashboxContainer.CashboxClient;
		}

		private async Task CheckFiscalization(CashboxClientContainer container, CancellationToken cancellationToken)
		{
			var canFiscalize = await container.CashboxClient.CanFiscalizeAsync(cancellationToken);
			if(!canFiscalize)
			{
				_logger.LogWarning("Кассовый аппарат {CashBoxId} ({RetailPointName}) не прошел проверку " +
					"на возможность фискализации.", container.Setting.CashBoxId, container.Setting.RetailPointName);
			}
			container.IsActive = canFiscalize;
			container.LastActivityCheck = DateTime.Now;
		}


		//private ICashboxClient GetCashBoxClient(CashReceipt cashReceipt)
		//{
		//	int cashBoxId;

		//	if(cashReceipt.CashboxId.HasValue)
		//	{
		//		cashBoxId = cashReceipt.CashboxId.Value;
		//	}
		//	else
		//	{
		//		var order = cashReceipt.Order;

		//		if(order.Contract == null)
		//		{
		//			throw new InvalidOperationException($"В заказе ({order.Id}) не указан договор.");
		//		}

		//		var organization = order.Contract.Organization;
		//		if(organization == null)
		//		{
		//			throw new InvalidOperationException($"В договоре заказа ({order.Id}) не указана организация.");
		//		}

		//		if(organization.CashBoxId == null)
		//		{
		//			throw new InvalidOperationException($"В организации ({organization.Id}) для заказа ({order.Id}) не указан код кассового аппарата.");
		//		}

		//		cashBoxId = organization.CashBoxId.Value;
		//	}

		//	if(!_cashboxes.TryGetValue(cashBoxId, out ICashboxClient cashboxClient))
		//	{
		//		throw new InvalidOperationException($"Не найден необходимый кассовый апарат ({cashBoxId}) в списке доступных.");
		//	}

		//	return cashboxClient;
		//}

		public void Dispose()
		{
			_settingsMonitorListener.Dispose();
			_loggerFactory.Dispose();
		}

		private class CashboxClientContainer
		{
			public CashboxClientContainer(CashboxSetting setting, CashboxClient cashboxClient)
			{
				Setting = setting;
				CashboxClient = cashboxClient;
			}

			public CashboxSetting Setting { get; set; }
			public CashboxClient CashboxClient { get; set; }
			public bool IsActive { get; set; }
			public DateTime LastActivityCheck { get; set; } = DateTime.MinValue;

			public bool FiscalizationCheckRequired => DateTime.Now - LastActivityCheck > TimeSpan.FromMinutes(Setting.CheckIntervalMin);

			public override bool Equals(object obj)
			{
				return obj is CashboxClientContainer container &&
					Setting.CashBoxId == container.Setting.CashBoxId;
			}

			public override int GetHashCode()
			{
				return -2058089664 + EqualityComparer<int>.Default.GetHashCode(Setting.CashBoxId);
			}
		}
	}
}
