using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using CashReceiptApi.HealthChecks;
using Vodovoz.Models.TrueMark;

namespace CashReceiptApi
{
	public class CashReceiptService : CashReceiptServiceGrpc.CashReceiptServiceGrpcBase
	{
		private readonly ILogger<CashReceiptService> _logger;
		private readonly FiscalDocumentRefresher _fiscalDocumentRefresher;
		private readonly CashReceiptApiHealthCheck _cashReceiptApiHealthCheck;

		public CashReceiptService(ILogger<CashReceiptService> logger, FiscalDocumentRefresher fiscalDocumentRefresher, CashReceiptApiHealthCheck cashReceiptApiHealthCheck)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fiscalDocumentRefresher = fiscalDocumentRefresher ?? throw new ArgumentNullException(nameof(fiscalDocumentRefresher));
			_cashReceiptApiHealthCheck = cashReceiptApiHealthCheck ?? throw new ArgumentNullException(nameof(cashReceiptApiHealthCheck));
		}

		public override async Task<Empty> RefreshFiscalDocument(RefreshReceiptRequest request, ServerCallContext context)
		{
			var isHealthy = true;

			_logger.LogInformation("Обновление статуса фискального документа для чека Id {0}", request.CashReceiptId);

			try
			{
				await _fiscalDocumentRefresher.RefreshDocForReceipt(request.CashReceiptId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				isHealthy = false;
				_logger.LogError(ex, "Обновление статуса фискального документа для чека Id {0} не удалось.", request.CashReceiptId);
			}

			_cashReceiptApiHealthCheck.IsHealthy = isHealthy;

			return new Empty();
		}
	}
}
