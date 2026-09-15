using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Receipts;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public class FiscalOrderSnapshotBuilder : IFiscalOrderSnapshotBuilder
	{
		private readonly IReceiptCorrectionRepository _receiptCorrectionRepository;

		public FiscalOrderSnapshotBuilder(IReceiptCorrectionRepository receiptCorrectionRepository)
		{
			_receiptCorrectionRepository = receiptCorrectionRepository
				?? throw new ArgumentNullException(nameof(receiptCorrectionRepository));
		}

		public FiscalOrderSnapshot BuildPreviousSnapshot(IUnitOfWork uow, int orderId)
		{
			var latestCompletedProcess = _receiptCorrectionRepository.GetLatestCompletedProcessForOrder(uow, orderId);
			var baselineDocument = ResolveBaselineEdoDocument(uow, orderId, latestCompletedProcess);
			if(baselineDocument == null)
			{
				return null;
			}

			if(IsEmptyBaseline(latestCompletedProcess, baselineDocument))
			{
				return BuildEmptySnapshot(orderId, baselineDocument);
			}

			return BuildFromEdoFiscalDocument(baselineDocument, orderId);
		}

		public FiscalOrderSnapshot BuildFromOrder(Order order)
		{
			if(order == null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			var items = order.OrderItems
				.Where(x => x.CurrentCount > 0 && x.Nomenclature != null)
				.ToList();

			var snapshot = new FiscalOrderSnapshot
			{
				OrderId = order.Id,
				OrganizationId = order.Contract?.Organization?.Id,
				CounterpartyId = order.Client?.Id,
				ContractId = order.Contract?.Id,
				DeliveryDate = order.DeliveryDate,
				PaymentType = order.PaymentType,
				Sum = 0
			};

			foreach(var orderItem in items)
			{
				var quantity = orderItem.CurrentCount;
				var discountSum = orderItem.DiscountMoney;
				var sum = orderItem.Price * quantity - discountSum;

				snapshot.Items.Add(new FiscalOrderSnapshotItem
				{
					NomenclatureId = orderItem.Nomenclature.Id,
					Name = Truncate(orderItem.Nomenclature.OfficialName, 512) ?? string.Empty,
					Quantity = quantity,
					Price = orderItem.Price,
					DiscountSum = discountSum,
					Sum = sum
				});

				snapshot.Sum += sum;
			}

			return snapshot;
		}

		private EdoFiscalDocument ResolveBaselineEdoDocument(
			IUnitOfWork uow,
			int orderId,
			ReceiptCorrectionProcess latestCompletedProcess)
		{
			if(latestCompletedProcess != null)
			{
				var fromProcess = ResolveResultDocument(uow, latestCompletedProcess);
				if(fromProcess != null)
				{
					return fromProcess;
				}
			}

			return _receiptCorrectionRepository.GetLatestCompletedSaleDocumentForOrder(uow, orderId);
		}

		private static bool IsEmptyBaseline(ReceiptCorrectionProcess latestCompletedProcess, EdoFiscalDocument baselineDocument)
		{
			if(latestCompletedProcess?.ScenarioType == ReceiptCorrectionScenarioType.FullCancellation)
			{
				return true;
			}

			return baselineDocument.DocumentType == FiscalDocumentType.Return
				&& latestCompletedProcess?.Documents.All(x => x.PlannedDocumentType == FiscalDocumentType.Return) == true;
		}

		private static FiscalOrderSnapshot BuildEmptySnapshot(int orderId, EdoFiscalDocument baselineDocument)
		{
			var order = baselineDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;

			return new FiscalOrderSnapshot
			{
				OrderId = orderId,
				SourceEdoFiscalDocumentId = baselineDocument.Id,
				OrganizationId = order?.Contract?.Organization?.Id,
				CounterpartyId = order?.Client?.Id,
				ContractId = order?.Contract?.Id,
				CashboxId = baselineDocument.ReceiptEdoTask?.CashboxId,
				PaymentType = order?.PaymentType,
				ClientInn = Truncate(baselineDocument.ClientInn, 20),
				Contact = Truncate(baselineDocument.Contact, 255),
				DeliveryDate = order?.DeliveryDate,
				FiscalDocumentNumber = null,
				FiscalDocumentDate = baselineDocument.FiscalTime,
				Sum = 0
			};
		}

		public static FiscalOrderSnapshot BuildFromEdoFiscalDocument(EdoFiscalDocument fiscalDocument, int orderId)
		{
			if(fiscalDocument == null)
			{
				throw new ArgumentNullException(nameof(fiscalDocument));
			}

			var order = fiscalDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
			var snapshot = new FiscalOrderSnapshot
			{
				OrderId = orderId,
				SourceEdoFiscalDocumentId = fiscalDocument.Id,
				OrganizationId = order?.Contract?.Organization?.Id,
				CounterpartyId = order?.Client?.Id,
				ContractId = order?.Contract?.Id,
				CashboxId = fiscalDocument.ReceiptEdoTask?.CashboxId,
				PaymentType = order?.PaymentType,
				ClientInn = Truncate(fiscalDocument.ClientInn, 20),
				Contact = Truncate(fiscalDocument.Contact, 255),
				DeliveryDate = order?.DeliveryDate,
				FiscalDocumentNumber = Truncate(fiscalDocument.FiscalNumber, 64),
				FiscalDocumentDate = fiscalDocument.FiscalTime,
				Sum = fiscalDocument.InventPositions.Sum(x => x.Price * x.Quantity - x.DiscountSum)
			};

			foreach(var inventPosition in fiscalDocument.InventPositions)
			{
				snapshot.Items.Add(new FiscalOrderSnapshotItem
				{
					NomenclatureId = inventPosition.OrderItems.FirstOrDefault()?.Nomenclature?.Id,
					Name = Truncate(inventPosition.Name, 512) ?? string.Empty,
					Quantity = inventPosition.Quantity,
					Price = inventPosition.Price,
					DiscountSum = inventPosition.DiscountSum,
					Sum = inventPosition.Price * inventPosition.Quantity - inventPosition.DiscountSum
				});
			}

			return snapshot;
		}

		private static EdoFiscalDocument ResolveResultDocument(IUnitOfWork uow, ReceiptCorrectionProcess process)
		{
			var completedDocs = process.Documents
				.Where(x => x.Status == ReceiptCorrectionProcessStatus.Completed && x.EdoFiscalDocumentId.HasValue)
				.Select(x => uow.GetById<EdoFiscalDocument>(x.EdoFiscalDocumentId.Value))
				.Where(x => x != null)
				.ToList();

			if(!completedDocs.Any())
			{
				return null;
			}

			return completedDocs
				.OrderByDescending(GetDocumentTypePriority)
				.ThenByDescending(x => x.FiscalTime ?? x.StatusChangeTime)
				.ThenByDescending(x => x.Id)
				.First();
		}

		private static int GetDocumentTypePriority(EdoFiscalDocument document)
		{
			switch(document.DocumentType)
			{
				case FiscalDocumentType.Sale:
					return 3;
				case FiscalDocumentType.SaleCorrection:
					return 2;
				case FiscalDocumentType.Return:
					return 1;
				default:
					return 0;
			}
		}

		private static string Truncate(string value, int maxLength)
		{
			if(string.IsNullOrEmpty(value) || value.Length <= maxLength)
			{
				return value;
			}

			return value.Substring(0, maxLength);
		}
	}
}
