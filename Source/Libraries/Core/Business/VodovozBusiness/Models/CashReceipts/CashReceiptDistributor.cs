using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Models.CashReceipts.DTO;

namespace Vodovoz.Models.CashReceipts
{
	public class CashReceiptDistributor
	{
		private readonly CashboxClientFactory _cashboxClientFactory;
		private readonly ICashboxSettingProvider _cashBoxProvider;

		private Dictionary<int, ICashboxClient> _cashboxes = new Dictionary<int, ICashboxClient>();
		private HashSet<int> _activeCashboxes = new HashSet<int>();

		public CashReceiptDistributor(
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

		public async Task<IEnumerable<ReceiptSendResult>> SendReceipts(IEnumerable<ReceiptSendData> receiptsData, CancellationToken cancellationToken)
		{
			var results = new List<ReceiptSendResult>();

			await CheckCashboxes(cancellationToken);

			foreach(var receiptData in receiptsData)
			{
				var cashReceipt = receiptData.CashReceipt;
				var result = new ReceiptSendResult
				{
					CashReceipt = cashReceipt
				};

				var cashboxClient = GetCashBoxClient(cashReceipt);
				if(IsActiveCashbox(cashboxClient))
				{
					result.FiscalizationResult = await cashboxClient.SendFiscalDocument(receiptData.FiscalDocument, cancellationToken);
				}
				else
				{
					result.FiscalizationResult = CreateFailResult("Чек не отправлен, так как проверка кассового аппарата не пройдена.");
				}

				results.Add(result);
			}

			return results;
		}

		private async Task CheckCashboxes(CancellationToken cancellationToken)
		{
			_activeCashboxes.Clear();

			foreach(var cashbox in _cashboxes)
			{
				var active = await cashbox.Value.CanFiscalizeAsync(cancellationToken);
				if(active)
				{
					_activeCashboxes.Add(cashbox.Key);
				}
			}
		}

		private bool IsActiveCashbox(ICashboxClient cashboxClient)
		{
			return _activeCashboxes.Contains(cashboxClient.CashboxId);
		}

		private FiscalizationResult CreateFailResult(string description)
		{
			var result = new FiscalizationResult();
			result.SendStatus = SendStatus.Error;
			result.FailDescription = description;

			return result;
		}

		private ICashboxClient GetCashBoxClient(CashReceipt cashReceipt)
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

			var cashBoxId = organization.CashBoxId;
			if(cashBoxId == null)
			{
				throw new InvalidOperationException($"В организации ({organization.Id}) для заказа ({order.Id}) не указан код кассового аппарата.");
			}

			if(!_cashboxes.TryGetValue(cashBoxId.Value, out ICashboxClient cashboxClient))
			{
				throw new InvalidOperationException($"Не найден необходимый кассовый апарат ({cashBoxId.Value}) в списке доступных.");
			}

			return cashboxClient;
		}
	}
}
