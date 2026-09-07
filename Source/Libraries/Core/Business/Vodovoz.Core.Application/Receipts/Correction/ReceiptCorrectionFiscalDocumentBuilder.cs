using Edo.Common;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Receipts;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public class ReceiptCorrectionFiscalDocumentBuilder
	{
		private readonly IEdoOrderContactProvider _edoOrderContactProvider;

		public ReceiptCorrectionFiscalDocumentBuilder(IEdoOrderContactProvider edoOrderContactProvider)
		{
			_edoOrderContactProvider = edoOrderContactProvider
				?? throw new ArgumentNullException(nameof(edoOrderContactProvider));
		}

		public EdoFiscalDocument CreateFromSource(
			EdoFiscalDocument sourceDocument,
			ReceiptEdoTask correctionTask,
			ReceiptCorrectionProcess process,
			ReceiptCorrectionProcessDocument processDocument)
		{
			if(sourceDocument == null)
			{
				throw new ArgumentNullException(nameof(sourceDocument));
			}

			if(correctionTask == null)
			{
				throw new ArgumentNullException(nameof(correctionTask));
			}

			if(process == null)
			{
				throw new ArgumentNullException(nameof(process));
			}

			if(processDocument == null)
			{
				throw new ArgumentNullException(nameof(processDocument));
			}

			var receiptEdoTask = correctionTask;
			var documentIndex = receiptEdoTask.FiscalDocuments.Any()
				? receiptEdoTask.FiscalDocuments.Max(x => x.Index) + 1
				: 0;

			var fiscalDocument = new EdoFiscalDocument
			{
				ReceiptEdoTask = receiptEdoTask,
				Stage = FiscalDocumentStage.Preparing,
				Status = FiscalDocumentStatus.None,
				DocumentGuid = processDocument.DocumentGuid,
				DocumentNumber = GetDocumentNumber(process, processDocument),
				DocumentType = processDocument.PlannedDocumentType,
				CheckoutTime = DateTime.Now,
				Contact = GetContact(sourceDocument, processDocument),
				ClientInn = GetClientInn(sourceDocument, processDocument),
				CashierName = sourceDocument.CashierName,
				PrintReceipt = false,
				Index = documentIndex
			};

			FillPositions(fiscalDocument, sourceDocument, process, processDocument);

			receiptEdoTask.FiscalDocuments.Add(fiscalDocument);

			return fiscalDocument;
		}

		private void FillPositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			ReceiptCorrectionProcess process,
			ReceiptCorrectionProcessDocument processDocument)
		{
			switch(processDocument.PlannedDocumentType)
			{
				case FiscalDocumentType.Return:
					FillReturnPositions(fiscalDocument, sourceDocument, process);
					break;
				case FiscalDocumentType.SaleCorrection:
					FillFromCurrentOrder(fiscalDocument, sourceDocument, allowEmptyFallbackToSource: true, useCurrentOrderPaymentType: true);
					break;
				case FiscalDocumentType.Sale:
					FillFromCurrentOrder(fiscalDocument, sourceDocument, allowEmptyFallbackToSource: false, useCurrentOrderPaymentType: true);
					break;
				default:
					CloneAllPositions(fiscalDocument, sourceDocument);
					break;
			}
		}

		/// <summary>
		/// Полный RETURN — все позиции исходного Sale.
		/// Частичный RETURN:
		/// - замена номенклатуры / смена цены штуки — возврат снятых или переоценённых позиций;
		/// - уменьшение количества: с маркой — по коду, без марки — дельта qty.
		/// </summary>
		private static void FillReturnPositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			ReceiptCorrectionProcess process)
		{
			if(process.ScenarioType == ReceiptCorrectionScenarioType.FullCancellation
				|| process.ScenarioType == ReceiptCorrectionScenarioType.OrganizationChange
				|| process.ScenarioType == ReceiptCorrectionScenarioType.ClientChange)
			{
				CloneAllPositions(fiscalDocument, sourceDocument);
				return;
			}

			if(process.ScenarioType == ReceiptCorrectionScenarioType.NomenclatureChange)
			{
				if(TryFillNomenclatureChangeReturnPositions(fiscalDocument, sourceDocument))
				{
					return;
				}

				// Без позиций к возврату полный клон не делаем — иначе обнулим весь чек зря.
				return;
			}

			if(TryFillPartialReturnPositions(fiscalDocument, sourceDocument))
			{
				return;
			}

			// Для уменьшения количества полный клон недопустим — иначе в кассу уйдёт весь чек.
			if(process.ScenarioType == ReceiptCorrectionScenarioType.QuantityOrAmountDecrease)
			{
				return;
			}

			CloneAllPositions(fiscalDocument, sourceDocument);
		}

		/// <summary>
		/// RETURN при замене товара / смене цены штуки:
		/// удалённая номенклатура или уменьшение qty — как частичный возврат;
		/// смена цены при том же qty — полный возврат позиции, новый приход в SALE.
		/// </summary>
		private static bool TryFillNomenclatureChangeReturnPositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument)
		{
			var order = sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
			if(order?.OrderItems == null)
			{
				return false;
			}

			var currentByNomenclature = order.OrderItems
				.Where(x => x.Nomenclature != null)
				.GroupBy(x => x.Nomenclature.Id)
				.ToDictionary(
					x => x.Key,
					x => new
					{
						Quantity = x.Sum(i => i.CurrentCount),
						Price = x.First().Price
					});

			var sourceByNomenclature = sourceDocument.InventPositions
				.Select(p => new
				{
					Position = p,
					NomenclatureId = p.OrderItems?.FirstOrDefault()?.Nomenclature?.Id
				})
				.Where(x => x.NomenclatureId.HasValue)
				.GroupBy(x => x.NomenclatureId.Value)
				.ToDictionary(
					x => x.Key,
					x => x.Select(i => i.Position).ToList());

			decimal returnedSum = 0;
			var hasReturn = false;

			foreach(var sourceGroup in sourceByNomenclature)
			{
				var oldQuantity = sourceGroup.Value.Sum(p => p.Quantity);
				var oldPrice = sourceGroup.Value.First().Price;
				currentByNomenclature.TryGetValue(sourceGroup.Key, out var current);
				var newQuantity = current?.Quantity ?? 0;
				var newPrice = current?.Price ?? 0;
				var priceChanged = current != null && oldPrice != newPrice;

				if(newQuantity <= 0)
				{
					hasReturn = true;
					foreach(var sourcePosition in sourceGroup.Value)
					{
						fiscalDocument.InventPositions.Add(CloneInventPosition(sourcePosition));
						returnedSum += sourcePosition.Price * sourcePosition.Quantity - sourcePosition.DiscountSum;
					}

					continue;
				}

				if(priceChanged)
				{
					// Смена цены штуки: полный возврат позиции, затем SALE с новой ценой.
					hasReturn = true;
					foreach(var sourcePosition in sourceGroup.Value)
					{
						fiscalDocument.InventPositions.Add(CloneInventPosition(sourcePosition));
						returnedSum += sourcePosition.Price * sourcePosition.Quantity - sourcePosition.DiscountSum;
					}

					continue;
				}

				var delta = oldQuantity - newQuantity;
				if(delta <= 0)
				{
					continue;
				}

				hasReturn = true;
				if(!TryAddMarkedReturnPositions(fiscalDocument, sourceGroup.Value, delta, ref returnedSum))
				{
					AddUnmarkedReturnPositions(fiscalDocument, sourceGroup.Value, delta, ref returnedSum);
				}
			}

			if(!hasReturn || !fiscalDocument.InventPositions.Any())
			{
				fiscalDocument.InventPositions.Clear();
				fiscalDocument.MoneyPositions.Clear();
				return false;
			}

			AddMoneyPosition(fiscalDocument, sourceDocument, returnedSum);
			return true;
		}

		private static bool TryFillPartialReturnPositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument)
		{
			var order = sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
			if(order?.OrderItems == null || !order.OrderItems.Any())
			{
				return false;
			}

			var currentByNomenclature = order.OrderItems
				.Where(x => x.Nomenclature != null)
				.GroupBy(x => x.Nomenclature.Id)
				.ToDictionary(
					x => x.Key,
					x => new
					{
						Quantity = x.Sum(i => i.CurrentCount),
						Discount = x.Sum(i => i.DiscountMoney)
					});

			var sourceByNomenclature = sourceDocument.InventPositions
				.Select(p => new
				{
					Position = p,
					NomenclatureId = p.OrderItems?.FirstOrDefault()?.Nomenclature?.Id
				})
				.Where(x => x.NomenclatureId.HasValue)
				.GroupBy(x => x.NomenclatureId.Value)
				.ToDictionary(
					x => x.Key,
					x => x.Select(i => i.Position).ToList());

			decimal returnedSum = 0;
			var hasPartial = false;

			foreach(var sourceGroup in sourceByNomenclature)
			{
				var oldQuantity = sourceGroup.Value.Sum(p => p.Quantity);
				currentByNomenclature.TryGetValue(sourceGroup.Key, out var current);
				var newQuantity = current?.Quantity ?? 0;
				var delta = oldQuantity - newQuantity;

				if(delta <= 0)
				{
					continue;
				}

				hasPartial = true;

				if(TryAddMarkedReturnPositions(fiscalDocument, sourceGroup.Value, delta, ref returnedSum))
				{
					continue;
				}

				AddUnmarkedReturnPositions(fiscalDocument, sourceGroup.Value, delta, ref returnedSum);
			}

			if(!hasPartial || !fiscalDocument.InventPositions.Any())
			{
				fiscalDocument.InventPositions.Clear();
				fiscalDocument.MoneyPositions.Clear();
				return false;
			}

			AddMoneyPosition(fiscalDocument, sourceDocument, returnedSum);
			return true;
		}

		/// <summary>
		/// Возврат по кодам маркировки: в RETURN попадают только возвращаемые единицы с их productMark.
		/// </summary>
		private static bool TryAddMarkedReturnPositions(
			EdoFiscalDocument fiscalDocument,
			IList<FiscalInventPosition> sourcePositions,
			decimal delta,
			ref decimal returnedSum)
		{
			var markedPositions = sourcePositions
				.Where(HasProductMark)
				.ToList();

			if(!markedPositions.Any())
			{
				return false;
			}

			var rejected = markedPositions
				.Where(IsRejectedMarkedPosition)
				.ToList();

			var candidates = (rejected.Any() ? rejected : markedPositions)
				.AsEnumerable()
				.Reverse()
				.ToList();

			var remainingDelta = delta;
			foreach(var sourcePosition in candidates)
			{
				if(remainingDelta <= 0)
				{
					break;
				}

				// Индивидуальный код обычно qty=1; берём целую позицию, чтобы сохранить productMark.
				if(sourcePosition.Quantity <= remainingDelta)
				{
					fiscalDocument.InventPositions.Add(CloneInventPosition(sourcePosition));
					returnedSum += sourcePosition.Price * sourcePosition.Quantity - sourcePosition.DiscountSum;
					remainingDelta -= sourcePosition.Quantity;
					continue;
				}

				// Групповой код с qty > дельты: уменьшаем количество, марка та же (как в исходной позиции).
				var take = remainingDelta;
				var ratio = sourcePosition.Quantity == 0 ? 0 : take / sourcePosition.Quantity;
				var discount = Math.Round(sourcePosition.DiscountSum * ratio, 2);
				fiscalDocument.InventPositions.Add(CloneInventPosition(sourcePosition, take, discount));
				returnedSum += sourcePosition.Price * take - discount;
				remainingDelta = 0;
			}

			return remainingDelta < delta;
		}

		private static void AddUnmarkedReturnPositions(
			EdoFiscalDocument fiscalDocument,
			IList<FiscalInventPosition> sourcePositions,
			decimal delta,
			ref decimal returnedSum)
		{
			var unmarkedPositions = sourcePositions
				.Where(p => !HasProductMark(p))
				.ToList();

			var positionsToReturn = unmarkedPositions.Any()
				? unmarkedPositions
				: sourcePositions;

			var remainingDelta = delta;
			foreach(var sourcePosition in positionsToReturn)
			{
				if(remainingDelta <= 0)
				{
					break;
				}

				var take = Math.Min(sourcePosition.Quantity, remainingDelta);
				var ratio = sourcePosition.Quantity == 0 ? 0 : take / sourcePosition.Quantity;
				var discount = Math.Round(sourcePosition.DiscountSum * ratio, 2);

				fiscalDocument.InventPositions.Add(CloneInventPosition(sourcePosition, take, discount));
				returnedSum += sourcePosition.Price * take - discount;
				remainingDelta -= take;
			}
		}

		private static bool HasProductMark(FiscalInventPosition position)
		{
			return position.EdoTaskItem != null || position.GroupCode != null;
		}

		private static bool IsRejectedMarkedPosition(FiscalInventPosition position)
		{
			var status = position.EdoTaskItem?.ProductCode?.SourceCodeStatus;
			return status == SourceProductCodeStatus.Rejected;
		}

		private static void FillFromCurrentOrder(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			bool allowEmptyFallbackToSource,
			bool useCurrentOrderPaymentType)
		{
			var order = sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
			var orderItems = order?.OrderItems?
				.Where(x => x.CurrentCount > 0 && x.Nomenclature != null)
				.ToList();

			if(orderItems == null || !orderItems.Any())
			{
				if(allowEmptyFallbackToSource)
				{
					CloneAllPositions(fiscalDocument, sourceDocument);
				}

				return;
			}

			decimal sum = 0;
			foreach(var orderItem in orderItems)
			{
				var vat = ResolveVat(sourceDocument, orderItem);
				var quantity = orderItem.CurrentCount;
				var discount = orderItem.DiscountMoney;
				sum += orderItem.Price * quantity - discount;

				fiscalDocument.InventPositions.Add(new FiscalInventPosition
				{
					Name = Truncate(orderItem.Nomenclature.OfficialName ?? orderItem.Nomenclature.Name, 128) ?? string.Empty,
					Quantity = quantity,
					Price = orderItem.Price,
					DiscountSum = discount,
					Vat = vat,
					OrderItems = new ObservableList<OrderItemEntity> { orderItem }
				});
			}

			var paymentType = useCurrentOrderPaymentType && order != null
				? MapOrderPaymentType(order.PaymentType)
				: sourceDocument.MoneyPositions.FirstOrDefault()?.PaymentType ?? FiscalPaymentType.Cash;

			AddMoneyPosition(fiscalDocument, paymentType, sum);
		}

		private static FiscalVat ResolveVat(EdoFiscalDocument sourceDocument, OrderItemEntity orderItem)
		{
			var fromSource = sourceDocument.InventPositions
				.FirstOrDefault(p => p.OrderItems?.Any(oi => oi.Nomenclature?.Id == orderItem.Nomenclature.Id) == true);

			if(fromSource != null)
			{
				return fromSource.Vat;
			}

			return sourceDocument.InventPositions.FirstOrDefault()?.Vat ?? FiscalVat.VatFree;
		}

		private static void CloneAllPositions(EdoFiscalDocument fiscalDocument, EdoFiscalDocument sourceDocument)
		{
			foreach(var sourcePosition in sourceDocument.InventPositions)
			{
				fiscalDocument.InventPositions.Add(CloneInventPosition(sourcePosition));
			}

			foreach(var sourceMoneyPosition in sourceDocument.MoneyPositions)
			{
				fiscalDocument.MoneyPositions.Add(new FiscalMoneyPosition
				{
					PaymentType = sourceMoneyPosition.PaymentType,
					Sum = sourceMoneyPosition.Sum
				});
			}
		}

		private static void AddMoneyPosition(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			decimal sum)
		{
			var paymentType = sourceDocument.MoneyPositions.FirstOrDefault()?.PaymentType
				?? FiscalPaymentType.Cash;

			AddMoneyPosition(fiscalDocument, paymentType, sum);
		}

		private static void AddMoneyPosition(
			EdoFiscalDocument fiscalDocument,
			FiscalPaymentType paymentType,
			decimal sum)
		{
			fiscalDocument.MoneyPositions.Add(new FiscalMoneyPosition
			{
				PaymentType = paymentType,
				Sum = Math.Round(sum, 2)
			});
		}

		private static FiscalPaymentType MapOrderPaymentType(Vodovoz.Domain.Client.PaymentType orderPaymentType)
		{
			switch(orderPaymentType)
			{
				case Vodovoz.Domain.Client.PaymentType.Terminal:
				case Vodovoz.Domain.Client.PaymentType.DriverApplicationQR:
				case Vodovoz.Domain.Client.PaymentType.SmsQR:
				case Vodovoz.Domain.Client.PaymentType.PaidOnline:
					return FiscalPaymentType.Card;
				default:
					return FiscalPaymentType.Cash;
			}
		}

		private static string GetDocumentNumber(
			ReceiptCorrectionProcess process,
			ReceiptCorrectionProcessDocument processDocument)
		{
			var typeSuffix = GetTypeSuffix(processDocument.PlannedDocumentType);
			return $"vod_{process.OrderId}_c{process.Id}_{typeSuffix}";
		}

		private static string GetTypeSuffix(FiscalDocumentType documentType)
		{
			switch(documentType)
			{
				case FiscalDocumentType.Return:
					return "ret";
				case FiscalDocumentType.Sale:
					return "sale";
				case FiscalDocumentType.SaleCorrection:
					return "corr";
				default:
					return documentType.ToString().ToLowerInvariant();
			}
		}

		private string GetContact(EdoFiscalDocument sourceDocument, ReceiptCorrectionProcessDocument processDocument)
		{
			if(processDocument.PlannedDocumentType != FiscalDocumentType.Sale
				&& processDocument.PlannedDocumentType != FiscalDocumentType.SaleCorrection)
			{
				return sourceDocument.Contact;
			}

			var order = sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
			if(order == null)
			{
				return sourceDocument.Contact;
			}

			try
			{
				return _edoOrderContactProvider.GetContact(order).StringValue ?? sourceDocument.Contact;
			}
			catch(OrderContactMissingException)
			{
				return sourceDocument.Contact;
			}
		}

		private static string GetClientInn(EdoFiscalDocument sourceDocument, ReceiptCorrectionProcessDocument processDocument)
		{
			if(processDocument.PlannedDocumentType != FiscalDocumentType.Sale
				&& processDocument.PlannedDocumentType != FiscalDocumentType.SaleCorrection)
			{
				return sourceDocument.ClientInn;
			}

			return sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order?.Client?.INN
				?? sourceDocument.ClientInn;
		}

		private static FiscalInventPosition CloneInventPosition(
			FiscalInventPosition source,
			decimal? quantityOverride = null,
			decimal? discountOverride = null)
		{
			return new FiscalInventPosition
			{
				Name = source.Name,
				Quantity = quantityOverride ?? source.Quantity,
				Price = source.Price,
				DiscountSum = discountOverride ?? source.DiscountSum,
				Vat = source.Vat,
				EdoTaskItem = source.EdoTaskItem,
				GroupCode = source.GroupCode,
				RegulatoryDocument = source.RegulatoryDocument,
				IndustryRequisiteData = source.IndustryRequisiteData,
				OrderItems = source.OrderItems == null
					? new ObservableList<OrderItemEntity>()
					: new ObservableList<OrderItemEntity>(source.OrderItems)
			};
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
