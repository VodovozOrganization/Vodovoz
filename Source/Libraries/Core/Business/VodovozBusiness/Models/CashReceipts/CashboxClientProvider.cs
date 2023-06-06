using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Models.CashReceipts.DTO;

namespace Vodovoz.Models.CashReceipts
{
	public class CashboxClientProvider
	{
		private readonly CashboxClientFactory _cashboxClientFactory;
		private readonly ICashboxSettingProvider _cashBoxProvider;

		private Dictionary<int, ICashboxClient> _cashboxes = new Dictionary<int, ICashboxClient>();
		private HashSet<int> _activeCashboxes = new HashSet<int>();
		private bool _cashBoxChecked;

		public CashboxClientProvider(
			CashboxClientFactory cashboxClientFactory,
			ICashboxSettingProvider cashBoxProvider
		)
		{
			_cashboxClientFactory = cashboxClientFactory ?? throw new ArgumentNullException(nameof(cashboxClientFactory));
			_cashBoxProvider = cashBoxProvider ?? throw new ArgumentNullException(nameof(cashBoxProvider));

			SetupCashboxClients();
		}

		private void SetupCashboxClients()
		{
			var cashBoxSettings = _cashBoxProvider.GetCashBoxSettings();
			if(!cashBoxSettings.Any())
			{
				throw new InvalidOperationException("Нет доступных кассовых апаратов для отправки чеков. Проверьте настройки сервиса.");
			}

			foreach(var cashBoxSetting in cashBoxSettings)
			{
				if(_cashboxes.ContainsKey(cashBoxSetting.Id))
				{
					continue;
				}

				var cashboxClient = _cashboxClientFactory.CreateClient(cashBoxSetting);
				_cashboxes.Add(cashBoxSetting.Id, cashboxClient);
			}
		}

		public async Task<ICashboxClient> GetCashboxAsync(CashReceipt cashReceipt, CancellationToken cancellationToken)
		{
			var results = new List<ReceiptSendResult>();

			await CheckCashboxes(cancellationToken);

			var cashboxClient = GetCashBoxClient(cashReceipt);
			if(!IsActiveCashbox(cashboxClient))
			{
				throw new InvalidOperationException($"Проверка кассового аппарата {cashboxClient.CashboxId} не пройдена.");
			}
			return cashboxClient;
		}

		private async Task CheckCashboxes(CancellationToken cancellationToken)
		{
			if(_cashBoxChecked)
			{
				return;
			}

			_activeCashboxes.Clear();

			foreach(var cashbox in _cashboxes)
			{
				var active = await cashbox.Value.CanFiscalizeAsync(cancellationToken);
				if(active)
				{
					_activeCashboxes.Add(cashbox.Key);
				}
			}

			_cashBoxChecked = true;
		}

		private ICashboxClient GetCashBoxClient(CashReceipt cashReceipt)
		{
			int cashBoxId;

			if(cashReceipt.CashboxId.HasValue)
			{
				cashBoxId = cashReceipt.CashboxId.Value;
			}
			else
			{
				var order = cashReceipt.Order;

				if(order.Contract == null)
				{
					throw new InvalidOperationException($"В заказе ({order.Id}) не указан договор.");
				}

				var organization = order.Contract.Organization;
				if(organization == null)
				{
					throw new InvalidOperationException($"В договоре заказа ({order.Id}) не указана организация.");
				}

				if(organization.CashBoxId == null)
				{
					throw new InvalidOperationException($"В организации ({organization.Id}) для заказа ({order.Id}) не указан код кассового аппарата.");
				}

				cashBoxId = organization.CashBoxId.Value;
			}

			if(!_cashboxes.TryGetValue(cashBoxId, out ICashboxClient cashboxClient))
			{
				throw new InvalidOperationException($"Не найден необходимый кассовый апарат ({cashBoxId}) в списке доступных.");
			}

			return cashboxClient;
		}

		private bool IsActiveCashbox(ICashboxClient cashboxClient)
		{
			return _activeCashboxes.Contains(cashboxClient.CashboxId);
		}
	}
}
