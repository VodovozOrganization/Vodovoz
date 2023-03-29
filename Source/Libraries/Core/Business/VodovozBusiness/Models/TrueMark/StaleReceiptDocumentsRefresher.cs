using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Models.TrueMark
{
	public class StaleReceiptDocumentsRefresher
	{
		private readonly ILogger<StaleReceiptDocumentsRefresher> _logger;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private readonly FiscalDocumentRefresher _fiscalDocumentRefresher;

		public StaleReceiptDocumentsRefresher(
			ILogger<StaleReceiptDocumentsRefresher> logger,
			ICashReceiptRepository cashReceiptRepository,
			FiscalDocumentRefresher fiscalDocumentRefresher
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			_fiscalDocumentRefresher = fiscalDocumentRefresher ?? throw new ArgumentNullException(nameof(fiscalDocumentRefresher));
		}

		public async Task RefreshDocuments(CancellationToken cancellationToken)
		{
			var receiptsToRefresh = _cashReceiptRepository.GetUnfinishedReceiptIds(50);
			_logger.LogInformation("Чеков на обновление фискального документа: {0}", receiptsToRefresh.Count());

			foreach(var receiptId in receiptsToRefresh)
			{
				try
				{
					await _fiscalDocumentRefresher.RefreshDocForReceipt(receiptId, cancellationToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Не удалось обновить фискальный документ для чека Id {0}.", receiptId);
				}
			}
		}
	}
}
