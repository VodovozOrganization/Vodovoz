using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
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

			if(latestCompletedProcess?.ScenarioType == ReceiptCorrectionScenarioType.FullCancellation)
			{
				return BuildEmptySnapshot(orderId, baselineDocument);
			}

			if(IsReturnOnlyProcess(latestCompletedProcess, baselineDocument))
			{
				return BuildSnapshotAfterPartialReturns(uow, orderId, latestCompletedProcess, baselineDocument);
			}

			var snapshot = BuildFromEdoFiscalDocument(baselineDocument, orderId);
			if(baselineDocument.DocumentType == FiscalDocumentType.Sale)
			{
				SubtractCompletedReturnsAfterSale(uow, orderId, snapshot, baselineDocument);
				if(!snapshot.Items.Any(x => x.Quantity > 0) || snapshot.Sum <= 0)
				{
					return BuildEmptySnapshot(orderId, baselineDocument);
				}
			}

			return snapshot;
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

		private static bool IsReturnOnlyProcess(ReceiptCorrectionProcess process, EdoFiscalDocument baselineDocument)
		{
			return baselineDocument.DocumentType == FiscalDocumentType.Return
				&& process?.Documents != null
				&& process.Documents.Any()
				&& process.Documents.All(x => x.PlannedDocumentType == FiscalDocumentType.Return);
		}

		private FiscalOrderSnapshot BuildSnapshotAfterPartialReturns(
			IUnitOfWork uow,
			int orderId,
			ReceiptCorrectionProcess returnProcess,
			EdoFiscalDocument returnDocument)
		{
			var sale = ResolveStandingSale(uow, orderId, returnProcess);
			if(sale == null)
			{
				return BuildFromEdoFiscalDocument(returnDocument, orderId);
			}

			var snapshot = BuildFromEdoFiscalDocument(sale, orderId);
			SubtractCompletedReturnsAfterSale(uow, orderId, snapshot, sale);

			if(!snapshot.Items.Any(x => x.Quantity > 0) || snapshot.Sum <= 0)
			{
				return BuildEmptySnapshot(orderId, sale);
			}

			return snapshot;
		}

		private EdoFiscalDocument ResolveStandingSale(
			IUnitOfWork uow,
			int orderId,
			ReceiptCorrectionProcess returnProcess)
		{
			if(returnProcess?.SourceEdoFiscalDocumentId != null)
			{
				var source = uow.GetById<EdoFiscalDocument>(returnProcess.SourceEdoFiscalDocumentId.Value);
				if(source?.DocumentType == FiscalDocumentType.Sale)
				{
					return source;
				}
			}

			return _receiptCorrectionRepository.GetLatestCompletedSaleDocumentForOrder(uow, orderId);
		}

		private void SubtractCompletedReturnsAfterSale(
			IUnitOfWork uow,
			int orderId,
			FiscalOrderSnapshot snapshot,
			EdoFiscalDocument sale)
		{
			if(sale == null || snapshot == null)
			{
				return;
			}

			var returns = _receiptCorrectionRepository
				.GetCompletedReturnDocumentsForOrder(uow, orderId)
				.Where(x => x.Id > sale.Id);

			foreach(var completedReturn in returns)
			{
				SubtractReturn(snapshot, completedReturn);
			}
		}

		private static void SubtractReturn(FiscalOrderSnapshot snapshot, EdoFiscalDocument returnDocument)
		{
			if(returnDocument?.InventPositions == null)
			{
				return;
			}

			foreach(var position in returnDocument.InventPositions.Where(x => x != null && x.Quantity > 0))
			{
				var left = position.Quantity;
				var candidates = snapshot.Items
					.Where(x => x.Quantity > 0 && NamesEqual(x.Name, position.Name))
					.ToList();

				if(!candidates.Any())
				{
					candidates = snapshot.Items
						.Where(x => x.Quantity > 0 && x.Price == position.Price)
						.ToList();
				}

				foreach(var item in candidates)
				{
					if(left <= 0)
					{
						break;
					}

					var take = Math.Min(item.Quantity, left);
					var discount = item.Quantity == 0
						? 0
						: Math.Round(item.DiscountSum * take / item.Quantity, 2, MidpointRounding.AwayFromZero);

					item.Quantity -= take;
					item.DiscountSum = Math.Max(0, item.DiscountSum - discount);
					item.Sum = Math.Round(item.Price * item.Quantity - item.DiscountSum, 2, MidpointRounding.AwayFromZero);
					if(item.Sum < 0)
					{
						item.Sum = 0;
					}

					left -= take;
				}
			}

			snapshot.Items = snapshot.Items.Where(x => x.Quantity > 0).ToList();
			snapshot.Sum = snapshot.Items.Sum(x => x.Sum);
		}

		private static bool NamesEqual(string left, string right)
		{
			return !string.IsNullOrWhiteSpace(left)
				&& !string.IsNullOrWhiteSpace(right)
				&& string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
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
				FiscalDocumentDate = fiscalDocument.FiscalTime
					?? fiscalDocument.StatusChangeTime
					?? fiscalDocument.CheckoutTime,
				Sum = fiscalDocument.InventPositions.Sum(x => x.Price * x.Quantity - x.DiscountSum)
			};

			foreach(var inventPosition in fiscalDocument.InventPositions)
			{
				var nomenclatureId = OrderItemNomenclatureAccess.Of(inventPosition.OrderItems.FirstOrDefault())?.Id
					?? ResolveNomenclatureIdByName(order, inventPosition.Name);

				snapshot.Items.Add(new FiscalOrderSnapshotItem
				{
					NomenclatureId = nomenclatureId,
					Name = Truncate(inventPosition.Name, 512) ?? string.Empty,
					Quantity = inventPosition.Quantity,
					Price = inventPosition.Price,
					DiscountSum = inventPosition.DiscountSum,
					Sum = inventPosition.Price * inventPosition.Quantity - inventPosition.DiscountSum
				});
			}

			return snapshot;
		}

		private static int? ResolveNomenclatureIdByName(OrderEntity order, string inventName)
		{
			if(order == null || string.IsNullOrWhiteSpace(inventName))
			{
				return null;
			}

			var invent = inventName.Trim();
			foreach(var orderItem in EnumerateOrderItems(order))
			{
				var nomenclature = OrderItemNomenclatureAccess.Of(orderItem);
				if(nomenclature == null)
				{
					continue;
				}

				var candidates = new[]
				{
					Truncate(nomenclature.OfficialName ?? nomenclature.Name, 128),
					Truncate(nomenclature.Name, 128),
					nomenclature.OfficialName,
					nomenclature.Name
				};

				if(candidates.Any(c =>
					!string.IsNullOrWhiteSpace(c)
					&& string.Equals(c.Trim(), invent, StringComparison.OrdinalIgnoreCase)))
				{
					return nomenclature.Id;
				}
			}

			return null;
		}

		private static IEnumerable<OrderItemEntity> EnumerateOrderItems(OrderEntity order)
		{
			if(order is Order domainOrder)
			{
				return domainOrder.OrderItems ?? Enumerable.Empty<OrderItemEntity>();
			}

			return order.OrderItems ?? Enumerable.Empty<OrderItemEntity>();
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
