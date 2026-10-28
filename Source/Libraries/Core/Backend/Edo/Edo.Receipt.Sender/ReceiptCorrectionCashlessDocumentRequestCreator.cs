using Edo.Transport;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Receipts;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Receipt.Sender
{
	/// <summary>
	/// После зануления чека создаёт первичную заявку ЭДО.
	/// Планировщик по текущей безналичной оплате сам выбирает УПД, сохранение кодов или опт.
	/// </summary>
	public class ReceiptCorrectionCashlessDocumentRequestCreator
	{
		private readonly ILogger<ReceiptCorrectionCashlessDocumentRequestCreator> _logger;
		private readonly IEdoRequestCreatedEventPublisher _edoRequestCreatedEventPublisher;

		public ReceiptCorrectionCashlessDocumentRequestCreator(
			ILogger<ReceiptCorrectionCashlessDocumentRequestCreator> logger,
			IEdoRequestCreatedEventPublisher edoRequestCreatedEventPublisher)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoRequestCreatedEventPublisher = edoRequestCreatedEventPublisher
				?? throw new ArgumentNullException(nameof(edoRequestCreatedEventPublisher));
		}

		public bool IsCashlessReceiptZeroing(ReceiptCorrectionProcess process)
		{
			return process != null
				&& process.ScenarioType == ReceiptCorrectionScenarioType.PaymentTypeChange
				&& process.Documents != null
				&& process.Documents.Count == 1
				&& process.Documents.All(x => x.PlannedDocumentType == FiscalDocumentType.SaleReturnCorrection);
		}

		public async Task<int?> TryCreateAsync(
			IUnitOfWork uow,
			ReceiptCorrectionProcess process,
			CancellationToken cancellationToken)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(process == null)
			{
				throw new ArgumentNullException(nameof(process));
			}

			var order = await uow.Session.GetAsync<OrderEntity>(process.OrderId, cancellationToken);
			if(order == null || order.PaymentType.ToString() != "Cashless")
			{
				return null;
			}

			var requests = await uow.Session.Query<PrimaryEdoRequest>()
				.Where(x => x.Order.Id == process.OrderId)
				.ToListAsync(cancellationToken);
			if(requests.Any(IsDocumentFlowRequest))
			{
				_logger.LogInformation(
					"По заказу {OrderId} заявка ЭДО на безнал уже есть, повторно не создаётся.",
					process.OrderId);
				return null;
			}

			var request = new PrimaryEdoRequest
			{
				Type = CustomerEdoRequestType.Order,
				Time = DateTime.Now,
				Source = EdoRequestSource.Manual,
				DocumentType = EdoDocumentType.UPD,
				Order = order,
				ProductCodes = new ObservableList<TrueMarkProductCode>()
			};

			if(process.SourceEdoFiscalDocumentId.HasValue)
			{
				var sourceDocument = await uow.Session.GetAsync<EdoFiscalDocument>(
					process.SourceEdoFiscalDocumentId.Value,
					cancellationToken);
				AddProductCodes(request, sourceDocument);
			}

			foreach(var processDocument in process.Documents)
			{
				if(!processDocument.EdoFiscalDocumentId.HasValue)
				{
					continue;
				}

				var correctionDocument = await uow.Session.GetAsync<EdoFiscalDocument>(
					processDocument.EdoFiscalDocumentId.Value,
					cancellationToken);
				AddProductCodes(request, correctionDocument);
			}

			await uow.SaveAsync(request, cancellationToken: cancellationToken);
			await uow.Session.FlushAsync(cancellationToken);
			if(request.Id <= 0)
			{
				_logger.LogError(
					"Не удалось сохранить заявку ЭДО после зануления чека заказа {OrderId}.",
					process.OrderId);
				return null;
			}

			_logger.LogInformation(
				"После зануления чека заказа {OrderId} создана заявка ЭДО {RequestId}.",
				process.OrderId,
				request.Id);
			return request.Id;
		}

		public Task PublishAsync(int requestId, CancellationToken cancellationToken)
		{
			return _edoRequestCreatedEventPublisher.Publish(
				requestId,
				"Смена оплаты на безнал: заявка после зануления чека",
				cancellationToken);
		}

		private static bool IsDocumentFlowRequest(PrimaryEdoRequest request)
		{
			var taskType = request?.Task?.TaskType;
			return taskType == EdoTaskType.Document
				|| taskType == EdoTaskType.SaveCode
				|| taskType == EdoTaskType.BulkAccounting
				|| (request != null && request.Task == null && request.Source == EdoRequestSource.Manual);
		}

		private static void AddProductCodes(PrimaryEdoRequest request, EdoFiscalDocument document)
		{
			if(document?.InventPositions == null)
			{
				return;
			}

			foreach(var productCode in document.InventPositions
				.Select(x => x.EdoTaskItem?.ProductCode)
				.Where(x => x != null))
			{
				if(request.ProductCodes.Any(x => x.Id == productCode.Id))
				{
					continue;
				}

				productCode.CustomerEdoRequest = request;
				request.ProductCodes.Add(productCode);
			}
		}
	}
}
