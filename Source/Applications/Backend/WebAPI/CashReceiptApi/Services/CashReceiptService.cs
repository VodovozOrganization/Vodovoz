using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vodovoz.Models.CashReceipts;
using Vodovoz.Models.TrueMark;

namespace CashReceiptApi
{
	public class CashReceiptService : CashReceiptServiceGrpc.CashReceiptServiceGrpcBase
	{
		private readonly ILogger<CashReceiptService> _logger;
		private readonly FiscalDocumentRefresher _fiscalDocumentRefresher;
		private readonly FiscalDocumentRequeueService _fiscalDocumentRequeueService;

		public CashReceiptService(
			ILogger<CashReceiptService> logger,
			FiscalDocumentRefresher fiscalDocumentRefresher,
			FiscalDocumentRequeueService fiscalDocumentRequeueService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fiscalDocumentRefresher = fiscalDocumentRefresher ?? throw new ArgumentNullException(nameof(fiscalDocumentRefresher));
			_fiscalDocumentRequeueService = fiscalDocumentRequeueService ?? throw new ArgumentNullException(nameof(fiscalDocumentRequeueService));
		}

		[Authorize]
		public override async Task<RequestProcessingResult> RefreshFiscalDocument(RefreshReceiptRequest request, ServerCallContext context)
		{
			_logger.LogInformation("Обновление статуса фискального документа для чека Id {0}", request.CashReceiptId);
			var response = new RequestProcessingResult();

			try
			{
				await _fiscalDocumentRefresher.RefreshDocForReceiptManually(request.CashReceiptId, context.CancellationToken);

				response.IsSuccess = true;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Обновление статуса фискального документа для чека Id {0} не удалось.", request.CashReceiptId);

				response.IsSuccess = false;
				response.Error = ex.Message;
			}

			return response;
		}

		[Authorize]
		public override async Task<RequestProcessingResult> RequeueFiscalDocument(RequeueDocumentRequest request, ServerCallContext context)
		{
			_logger.LogInformation("Повторное проведение фискального документа для чека Id {0}", request.CashReceiptId);
			var response = new RequestProcessingResult();

			try
			{
				await _fiscalDocumentRequeueService.RequeueDocForReceiptManually(request.CashReceiptId, context.CancellationToken);

				response.IsSuccess = true;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Повторное проведение фискального документа для чека Id {0} не удалось.", request.CashReceiptId);

				response.IsSuccess = false;
				response.Error = ex.Message;
			}

			return response;
		}
	}
}
