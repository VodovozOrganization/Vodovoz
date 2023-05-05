using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vodovoz.Models.TrueMark;

namespace CashReceiptApi
{
	public class CashReceiptService : CashReceiptServiceGrpc.CashReceiptServiceGrpcBase
	{
		private readonly ILogger<CashReceiptService> _logger;
		private readonly FiscalDocumentRefresher _fiscalDocumentRefresher;

		public CashReceiptService(ILogger<CashReceiptService> logger, FiscalDocumentRefresher fiscalDocumentRefresher)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fiscalDocumentRefresher = fiscalDocumentRefresher ?? throw new ArgumentNullException(nameof(fiscalDocumentRefresher));
		}

		public override async Task<Empty> RefreshFiscalDocument(RefreshReceiptRequest request, ServerCallContext context)
		{
			_logger.LogInformation("Обновление статуса фискального документа для чека Id {0}", request.CashReceiptId);

			try
			{
				await _fiscalDocumentRefresher.RefreshDocForReceipt(request.CashReceiptId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Обновление статуса фискального документа для чека Id {0} не удалось.", request.CashReceiptId);
			}

			return new Empty();
		}
	}
}
