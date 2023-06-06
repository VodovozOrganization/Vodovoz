using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Models.CashReceipts.DTO;

namespace Vodovoz.Models.CashReceipts
{
	public class CashReceiptDistributor
	{
		private readonly CashboxClientProvider _cashboxClientProvider;

		public CashReceiptDistributor(
			CashboxClientProvider cashboxClientProvider
		)
		{
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
		}

		public async Task<IEnumerable<ReceiptSendResult>> SendReceipts(IEnumerable<ReceiptSendData> receiptsData, CancellationToken cancellationToken)
		{
			var results = new List<ReceiptSendResult>();
			foreach(var receiptData in receiptsData)
			{
				var cashReceipt = receiptData.CashReceipt;
				var result = new ReceiptSendResult
				{
					CashReceipt = cashReceipt
				};

				try
				{
					var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(cashReceipt, cancellationToken);
					result.FiscalizationResult = await cashboxClient.SendFiscalDocument(receiptData.FiscalDocument, cancellationToken);
					result.CashReceipt.CashboxId = cashboxClient.CashboxId;
				}
				catch(Exception ex)
				{
					result.FiscalizationResult = CreateFailResult($"Чек не отправлен. Ошибка: {ex.Message}");
				}

				results.Add(result);
			}

			return results;
		}

		private FiscalizationResult CreateFailResult(string description)
		{
			var result = new FiscalizationResult();
			result.SendStatus = SendStatus.Error;
			result.FailDescription = description;

			return result;
		}
	}
}
