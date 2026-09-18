using Edo.Common;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Receipts;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;

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
			ReceiptCorrectionProcessDocument processDocument,
			OrderEntity currentOrder = null)
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

			var orderForFill = currentOrder
				?? correctionTask.FormalEdoRequest?.Order
				?? sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;

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

			FillPositions(fiscalDocument, sourceDocument, process, processDocument, orderForFill);

			receiptEdoTask.FiscalDocuments.Add(fiscalDocument);

			return fiscalDocument;
		}

		private void FillPositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			ReceiptCorrectionProcess process,
			ReceiptCorrectionProcessDocument processDocument,
			OrderEntity currentOrder)
		{
			switch(processDocument.PlannedDocumentType)
			{
				case FiscalDocumentType.Return:
					FillReturnPositions(fiscalDocument, sourceDocument, process, currentOrder);
					break;
				case FiscalDocumentType.SaleCorrection:
					FillFromCurrentOrder(
						fiscalDocument,
						sourceDocument,
						currentOrder,
						allowEmptyFallbackToSource: true,
						useCurrentOrderPaymentType: true);
					break;
				case FiscalDocumentType.Sale:
					FillFromCurrentOrder(
						fiscalDocument,
						sourceDocument,
						currentOrder,
						allowEmptyFallbackToSource: false,
						useCurrentOrderPaymentType: true);
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
			ReceiptCorrectionProcess process,
			OrderEntity currentOrder)
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
				if(TryFillNomenclatureChangeReturnPositions(fiscalDocument, sourceDocument, currentOrder))
				{
					return;
				}

				// Без позиций к возврату полный клон не делаем — иначе обнулим весь чек зря.
				return;
			}

			if(TryFillPartialReturnPositions(fiscalDocument, sourceDocument, currentOrder))
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
			EdoFiscalDocument sourceDocument,
			OrderEntity currentOrder)
		{
			var order = currentOrder ?? sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
			if(order == null)
			{
				return false;
			}

			var orderItems = GetOrderItems(order).ToList();
			if(!orderItems.Any())
			{
				return false;
			}

			var currentByNomenclature = orderItems
				.Where(x => x.Nomenclature != null)
				.GroupBy(x => x.Nomenclature.Id)
				.ToDictionary(
					x => x.Key,
					x => new
					{
						Quantity = x.Sum(i => i.CurrentCount),
						Price = x.First().Price
					});

			var sourceByNomenclature = GroupSourceInventByNomenclature(sourceDocument, order);

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
						returnedSum += AddReturnInventPosition(
							fiscalDocument, sourceDocument, sourcePosition, sourcePosition.Quantity);
					}

					continue;
				}

				if(priceChanged)
				{
					hasReturn = true;
					foreach(var sourcePosition in sourceGroup.Value)
					{
						returnedSum += AddReturnInventPosition(
							fiscalDocument, sourceDocument, sourcePosition, sourcePosition.Quantity);
					}

					continue;
				}

				var delta = oldQuantity - newQuantity;
				if(delta <= 0)
				{
					continue;
				}

				hasReturn = true;
				if(!TryAddMarkedReturnPositions(fiscalDocument, sourceDocument, sourceGroup.Value, delta, ref returnedSum))
				{
					AddUnmarkedReturnPositions(fiscalDocument, sourceDocument, sourceGroup.Value, delta, ref returnedSum);
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
			EdoFiscalDocument sourceDocument,
			OrderEntity currentOrder)
		{
			var order = currentOrder ?? sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
			if(order == null)
			{
				return false;
			}

			var orderItems = GetOrderItems(order).ToList();
			if(!orderItems.Any())
			{
				return false;
			}

			if(sourceDocument.InventPositions == null || !sourceDocument.InventPositions.Any())
			{
				return false;
			}

			var currentByNomenclature = orderItems
				.Where(x => x.Nomenclature != null)
				.GroupBy(x => x.Nomenclature.Id)
				.ToDictionary(
					x => x.Key,
					x => new
					{
						Quantity = x.Sum(i => i.CurrentCount),
						Discount = x.Sum(i => i.DiscountMoney)
					});

			var sourceByNomenclature = GroupSourceInventByNomenclature(sourceDocument, order);

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

				if(TryAddMarkedReturnPositions(fiscalDocument, sourceDocument, sourceGroup.Value, delta, ref returnedSum))
				{
					continue;
				}

				AddUnmarkedReturnPositions(fiscalDocument, sourceDocument, sourceGroup.Value, delta, ref returnedSum);
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

		private static Dictionary<int, List<FiscalInventPosition>> GroupSourceInventByNomenclature(
			EdoFiscalDocument sourceDocument,
			OrderEntity order)
		{
			var result = new Dictionary<int, List<FiscalInventPosition>>();
			var unmatched = new List<FiscalInventPosition>();

			foreach(var position in sourceDocument.InventPositions ?? Enumerable.Empty<FiscalInventPosition>())
			{
				var nomenclatureId = ResolveInventNomenclatureId(position, order);
				if(!nomenclatureId.HasValue)
				{
					unmatched.Add(position);
					continue;
				}

				AddToNomenclatureGroup(result, nomenclatureId.Value, position);
			}

			if(unmatched.Count == 0)
			{
				return result;
			}

			var orderNomenclatureIds = GetOrderItems(order)
				.Where(x => x.Nomenclature != null)
				.Select(x => x.Nomenclature.Id)
				.Distinct()
				.ToList();

			if(orderNomenclatureIds.Count == 1)
			{
				foreach(var position in unmatched)
				{
					AddToNomenclatureGroup(result, orderNomenclatureIds[0], position);
				}

				return result;
			}

			foreach(var position in unmatched)
			{
				var byPrice = GetOrderItems(order)
					.Where(x => x.Nomenclature != null && x.Price == position.Price)
					.Select(x => x.Nomenclature.Id)
					.Distinct()
					.ToList();

				if(byPrice.Count == 1)
				{
					AddToNomenclatureGroup(result, byPrice[0], position);
				}
			}

			return result;
		}

		private static void AddToNomenclatureGroup(
			Dictionary<int, List<FiscalInventPosition>> groups,
			int nomenclatureId,
			FiscalInventPosition position)
		{
			if(!groups.TryGetValue(nomenclatureId, out var list))
			{
				list = new List<FiscalInventPosition>();
				groups[nomenclatureId] = list;
			}

			list.Add(position);
		}

		private static int? ResolveInventNomenclatureId(FiscalInventPosition position, OrderEntity order)
		{
			var fromOrderItems = position.OrderItems?
				.Select(x => x?.Nomenclature?.Id)
				.FirstOrDefault(id => id.HasValue && id.Value > 0);

			if(fromOrderItems.HasValue)
			{
				return fromOrderItems;
			}

			if(order == null || string.IsNullOrWhiteSpace(position.Name))
			{
				return null;
			}

			var match = GetOrderItems(order)
				.Where(x => x.Nomenclature != null)
				.FirstOrDefault(x => InventNameMatchesNomenclature(position.Name, x.Nomenclature));

			return match?.Nomenclature?.Id;
		}

		private static bool InventNameMatchesNomenclature(string inventName, Vodovoz.Core.Domain.Goods.NomenclatureEntity nomenclature)
		{
			if(string.IsNullOrWhiteSpace(inventName) || nomenclature == null)
			{
				return false;
			}

			var invent = inventName.Trim();
			var candidates = new[]
			{
				Truncate(nomenclature.OfficialName ?? nomenclature.Name, 128),
				Truncate(nomenclature.Name, 128),
				nomenclature.OfficialName,
				nomenclature.Name
			};

			return candidates.Any(c =>
				!string.IsNullOrWhiteSpace(c)
				&& string.Equals(c.Trim(), invent, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Возврат по кодам маркировки: в RETURN попадают только возвращаемые единицы с их productMark.
		/// </summary>
		private static bool TryAddMarkedReturnPositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
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
					returnedSum += AddReturnInventPosition(
						fiscalDocument, sourceDocument, sourcePosition, sourcePosition.Quantity);
					remainingDelta -= sourcePosition.Quantity;
					continue;
				}

				// Групповой код с qty > дельты: уменьшаем количество, марка та же (как в исходной позиции).
				var take = remainingDelta;
				returnedSum += AddReturnInventPosition(fiscalDocument, sourceDocument, sourcePosition, take);
				remainingDelta = 0;
			}

			return remainingDelta < delta;
		}

		private static void AddUnmarkedReturnPositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
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
				returnedSum += AddReturnInventPosition(fiscalDocument, sourceDocument, sourcePosition, take);
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
			OrderEntity currentOrder,
			bool allowEmptyFallbackToSource,
			bool useCurrentOrderPaymentType)
		{
			var order = currentOrder ?? sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
			var orderItems = GetOrderItems(order)
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
				var discount = ResolveOrderItemDiscount(orderItem);
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

		private static IEnumerable<OrderItemEntity> GetOrderItems(OrderEntity order)
		{
			if(order == null)
			{
				return Enumerable.Empty<OrderItemEntity>();
			}

			if(order is Order domainOrder)
			{
				return domainOrder.OrderItems ?? Enumerable.Empty<OrderItemEntity>();
			}

			return order.OrderItems ?? Enumerable.Empty<OrderItemEntity>();
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
			decimal sum = 0;
			foreach(var sourcePosition in sourceDocument.InventPositions)
			{
				sum += AddReturnInventPosition(
					fiscalDocument, sourceDocument, sourcePosition, sourcePosition.Quantity);
			}

			var paymentType = sourceDocument.MoneyPositions.FirstOrDefault()?.PaymentType
				?? FiscalPaymentType.Cash;

			// Money всегда из invent (со скидками), а не слепое копирование — иначе при обнулении
			// DiscountSum в сессии уйдёт расхождение invent/money.
			AddMoneyPosition(fiscalDocument, paymentType, sum);
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
			string inn;
			if(processDocument.PlannedDocumentType != FiscalDocumentType.Sale
				&& processDocument.PlannedDocumentType != FiscalDocumentType.SaleCorrection)
			{
				inn = sourceDocument.ClientInn;
			}
			else
			{
				inn = sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order?.Client?.INN
					?? sourceDocument.ClientInn;
			}

			return string.IsNullOrWhiteSpace(inn) ? null : inn.Trim();
		}

		private static decimal AddReturnInventPosition(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			FiscalInventPosition source,
			decimal quantity)
		{
			var discount = ResolveReturnDiscount(sourceDocument, source, quantity);
			fiscalDocument.InventPositions.Add(CloneInventPosition(source, quantity, discount));
			return source.Price * quantity - discount;
		}

		private static decimal ResolveReturnDiscount(
			EdoFiscalDocument sourceDocument,
			FiscalInventPosition source,
			decimal quantity)
		{
			if(source == null || quantity <= 0)
			{
				return 0;
			}

			if(source.DiscountSum != 0 && source.Quantity != 0)
			{
				return Math.Round(source.DiscountSum * (quantity / source.Quantity), 2);
			}

			var orderItem = source.OrderItems?.FirstOrDefault();
			if(orderItem != null)
			{
				var orderDiscount = orderItem.OriginalDiscountMoney
					?? (orderItem.DiscountMoney > 0 ? orderItem.DiscountMoney : (decimal?)null)
					?? 0;

				var baseQty = orderItem.Count > 0
					? orderItem.Count
					: (orderItem.CurrentCount > 0 ? orderItem.CurrentCount : source.Quantity);

				if(orderDiscount > 0 && baseQty > 0)
				{
					return Math.Round(orderDiscount * (quantity / baseQty), 2);
				}

				var percent = orderItem.OriginalDiscount ?? (orderItem.Discount > 0 ? orderItem.Discount : (decimal?)null);
				if(percent.HasValue && percent.Value > 0)
				{
					return Math.Round(source.Price * quantity * percent.Value / 100m, 2);
				}
			}

			return ResolveReturnDiscountFromSourceMoney(sourceDocument, source, quantity);
		}

		private static decimal ResolveReturnDiscountFromSourceMoney(
			EdoFiscalDocument sourceDocument,
			FiscalInventPosition source,
			decimal quantity)
		{
			if(sourceDocument?.InventPositions == null || sourceDocument.MoneyPositions == null || source.Quantity == 0)
			{
				return 0;
			}

			var inventGross = sourceDocument.InventPositions.Sum(p => p.Price * p.Quantity);
			if(inventGross <= 0)
			{
				return 0;
			}

			var inventDiscountTotal = sourceDocument.InventPositions.Sum(p => p.DiscountSum);
			var moneySum = sourceDocument.MoneyPositions.Sum(m => m.Sum);
			var missingDiscount = inventGross - inventDiscountTotal - moneySum;
			if(missingDiscount <= 0.01m)
			{
				return 0;
			}

			// Распределяем только по позициям без DiscountSum — у кого скидка уже есть, не трогаем.
			var zeroDiscountGross = sourceDocument.InventPositions
				.Where(p => p.DiscountSum == 0)
				.Sum(p => p.Price * p.Quantity);

			if(zeroDiscountGross <= 0 || source.DiscountSum != 0)
			{
				return 0;
			}

			var positionGross = source.Price * source.Quantity;
			var positionMissing = missingDiscount * (positionGross / zeroDiscountGross);
			return Math.Round(positionMissing * (quantity / source.Quantity), 2);
		}

		/// <summary>
		/// Скидка для нового SALE из текущего заказа / закрытия МЛ (CurrentCount + DiscountMoney / Original*).
		/// </summary>
		private static decimal ResolveOrderItemDiscount(OrderItemEntity orderItem)
		{
			if(orderItem == null || orderItem.CurrentCount <= 0)
			{
				return 0;
			}

			if(orderItem.DiscountMoney > 0)
			{
				return orderItem.DiscountMoney;
			}

			if(orderItem.Discount > 0)
			{
				return Math.Round(orderItem.Price * orderItem.CurrentCount * orderItem.Discount / 100m, 2);
			}

			if(orderItem.OriginalDiscountMoney.HasValue && orderItem.Count > 0)
			{
				return Math.Round(orderItem.OriginalDiscountMoney.Value * (orderItem.CurrentCount / orderItem.Count), 2);
			}

			if(orderItem.OriginalDiscount.HasValue && orderItem.OriginalDiscount.Value > 0)
			{
				return Math.Round(orderItem.Price * orderItem.CurrentCount * orderItem.OriginalDiscount.Value / 100m, 2);
			}

			return 0;
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
