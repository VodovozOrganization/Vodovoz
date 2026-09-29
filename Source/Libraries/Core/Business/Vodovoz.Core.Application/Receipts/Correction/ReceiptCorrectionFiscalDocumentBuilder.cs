using Edo.Common;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
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
			OrderEntity currentOrder = null,
			FiscalOrderSnapshot currentSnapshot = null,
			FiscalOrderSnapshot previousSnapshot = null)
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
				Contact = GetContact(sourceDocument, processDocument, orderForFill),
				ClientInn = GetClientInn(sourceDocument, processDocument, orderForFill),
				CashierName = sourceDocument.CashierName,
				PrintReceipt = false,
				Index = documentIndex
			};

			FillPositions(
				fiscalDocument,
				sourceDocument,
				process,
				processDocument,
				orderForFill,
				currentSnapshot,
				previousSnapshot);

			receiptEdoTask.FiscalDocuments.Add(fiscalDocument);

			return fiscalDocument;
		}

		private void FillPositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			ReceiptCorrectionProcess process,
			ReceiptCorrectionProcessDocument processDocument,
			OrderEntity currentOrder,
			FiscalOrderSnapshot currentSnapshot,
			FiscalOrderSnapshot previousSnapshot)
		{
			switch(processDocument.PlannedDocumentType)
			{
				case FiscalDocumentType.Return:
					FillReturnPositions(fiscalDocument, sourceDocument, process, currentOrder, previousSnapshot);
					EnsureReturnPositions(fiscalDocument, sourceDocument, currentOrder, previousSnapshot);
					break;
				case FiscalDocumentType.SaleCorrection:
					FillFromCurrentOrder(
						fiscalDocument,
						sourceDocument,
						currentOrder,
						currentSnapshot,
						allowEmptyFallbackToSource: true,
						useCurrentOrderPaymentType: true);
					break;
				case FiscalDocumentType.Sale:
					FillFromCurrentOrder(
						fiscalDocument,
						sourceDocument,
						currentOrder,
						currentSnapshot,
						allowEmptyFallbackToSource: false,
						useCurrentOrderPaymentType: true);
					break;
				default:
					CloneAllPositions(fiscalDocument, sourceDocument);
					break;
			}
		}

		private static void FillReturnPositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			ReceiptCorrectionProcess process,
			OrderEntity currentOrder,
			FiscalOrderSnapshot previousSnapshot)
		{
			if(process.ScenarioType == ReceiptCorrectionScenarioType.FullCancellation
				|| process.ScenarioType == ReceiptCorrectionScenarioType.OrganizationChange
				|| process.ScenarioType == ReceiptCorrectionScenarioType.ClientChange)
			{
				if(TryFillReturnFromPreviousSnapshot(fiscalDocument, sourceDocument, previousSnapshot))
				{
					return;
				}

				CloneAllPositions(fiscalDocument, sourceDocument);
				return;
			}

			if(process.ScenarioType == ReceiptCorrectionScenarioType.QuantityOrAmountDecrease)
			{
				FillDecreasedQuantityReturn(fiscalDocument, sourceDocument, currentOrder, previousSnapshot);
				return;
			}

			if(process.ScenarioType == ReceiptCorrectionScenarioType.NomenclatureChange)
			{
				TryFillNomenclatureChangeReturnPositions(fiscalDocument, sourceDocument, currentOrder, previousSnapshot);
				return;
			}

			if(TryFillPartialReturnPositions(fiscalDocument, sourceDocument, currentOrder))
			{
				return;
			}

			CloneAllPositions(fiscalDocument, sourceDocument);
		}

		private static void FillDecreasedQuantityReturn(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			OrderEntity currentOrder,
			FiscalOrderSnapshot previousSnapshot)
		{
			fiscalDocument.InventPositions.Clear();
			fiscalDocument.MoneyPositions.Clear();

			var order = currentOrder ?? sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
			var orderItems = GetOrderItems(order)
				.Where(x => x?.Nomenclature != null)
				.ToList();
			var sourcePositions = sourceDocument?.InventPositions?
				.Where(x => x != null && x.Quantity > 0)
				.ToList() ?? new List<FiscalInventPosition>();

			decimal sum = 0;
			foreach(var item in orderItems)
			{
				var matched = sourcePositions
					.Where(p => PositionMatchesAnyOrderItem(p, new[] { item }))
					.ToList();
				var sourceQty = matched.Sum(p => p.Quantity);
				var snapshotQty = SnapshotQuantity(previousSnapshot, item);
				var baseline = snapshotQty > 0 ? snapshotQty : sourceQty;
				var delta = baseline - item.CurrentCount;

				if(delta <= 0)
				{
					var discountDiff = SnapshotLineNet(previousSnapshot, item)
						- (item.Price * item.CurrentCount - ResolveOrderItemDiscount(item));
					if(discountDiff <= 0)
					{
						continue;
					}

					var takeQuantity = baseline > 0 ? baseline : item.CurrentCount;
					if(takeQuantity <= 0)
					{
						continue;
					}

					if(!AddDecreasedSourcePositions(fiscalDocument, matched, takeQuantity, discountDiff))
					{
						fiscalDocument.InventPositions.Add(new FiscalInventPosition
						{
							Name = Truncate(item.Nomenclature.OfficialName ?? item.Nomenclature.Name, 128) ?? string.Empty,
							Quantity = takeQuantity,
							Price = item.Price,
							DiscountSum = DiscountLeavingNet(item.Price, takeQuantity, discountDiff),
							Vat = sourceDocument == null ? FiscalVat.VatFree : ResolveVat(sourceDocument, item),
							OrderItems = new ObservableList<OrderItemEntity> { item }
						});
					}

					sum += discountDiff;
					continue;
				}

				var line = ResolveDecreasedLineMoney(item, delta, matched);
				if(line <= 0)
				{
					continue;
				}

				if(!AddDecreasedSourcePositions(fiscalDocument, matched, delta, line))
				{
					fiscalDocument.InventPositions.Add(new FiscalInventPosition
					{
						Name = Truncate(item.Nomenclature.OfficialName ?? item.Nomenclature.Name, 128) ?? string.Empty,
						Quantity = delta,
						Price = item.Price,
						DiscountSum = DiscountLeavingNet(item.Price, delta, line),
						Vat = sourceDocument == null ? FiscalVat.VatFree : ResolveVat(sourceDocument, item),
						OrderItems = new ObservableList<OrderItemEntity> { item }
					});
				}

				sum += line;
			}

			if(sum <= 0)
			{
				fiscalDocument.InventPositions.Clear();
				fiscalDocument.MoneyPositions.Clear();
				if(TryFillReturnFromSnapshotShortfall(fiscalDocument, sourceDocument, currentOrder, previousSnapshot))
				{
					return;
				}

				return;
			}

			AddMoneyPosition(fiscalDocument, sourceDocument, sum);
		}

		private static decimal SnapshotQuantity(FiscalOrderSnapshot previousSnapshot, OrderItemEntity item)
		{
			if(previousSnapshot?.Items == null || item?.Nomenclature == null)
			{
				return 0;
			}

			return previousSnapshot.Items
				.Where(x => x != null && x.Quantity > 0 && SnapshotItemMatchesOrderItem(x, item))
				.Sum(x => x.Quantity);
		}

		private static decimal SnapshotLineNet(FiscalOrderSnapshot previousSnapshot, OrderItemEntity item)
		{
			if(previousSnapshot?.Items == null || item?.Nomenclature == null)
			{
				return 0;
			}

			return previousSnapshot.Items
				.Where(x => x != null && x.Quantity > 0 && SnapshotItemMatchesOrderItem(x, item))
				.Sum(x => x.Sum);
		}

		private static decimal ResolveDecreasedLineMoney(
			OrderItemEntity item,
			decimal quantity,
			IList<FiscalInventPosition> matched)
		{
			if(matched != null && matched.Any())
			{
				var sourceQty = matched.Sum(p => p.Quantity);
				var sourceNet = matched.Sum(p => p.Price * p.Quantity - p.DiscountSum);
				if(sourceNet < 0)
				{
					sourceNet = 0;
				}

				if(sourceQty > 0 && sourceNet > 0)
				{
					var take = Math.Min(quantity, sourceQty);
					return Math.Round(sourceNet * take / sourceQty, 2, MidpointRounding.AwayFromZero);
				}
			}

			if(item.CurrentCount > 0)
			{
				var keptSum = item.Price * item.CurrentCount - item.DiscountMoney;
				if(keptSum > 0)
				{
					return Math.Round(keptSum / item.CurrentCount * quantity, 2, MidpointRounding.AwayFromZero);
				}
			}

			return Math.Round(item.Price * quantity, 2, MidpointRounding.AwayFromZero);
		}

		private static bool AddDecreasedSourcePositions(
			EdoFiscalDocument fiscalDocument,
			IList<FiscalInventPosition> matched,
			decimal quantity,
			decimal line)
		{
			if(matched == null || !matched.Any() || quantity <= 0)
			{
				return false;
			}

			var added = new List<FiscalInventPosition>();
			var left = quantity;
			foreach(var source in matched)
			{
				if(left <= 0)
				{
					break;
				}

				var take = Math.Min(source.Quantity, left);
				var discount = source.Quantity == 0
					? 0
					: Math.Round(source.DiscountSum * take / source.Quantity, 2, MidpointRounding.AwayFromZero);
				var gross = source.Price * take;
				if(discount > gross)
				{
					discount = gross;
				}

				var clone = CloneInventPosition(source, take, discount);
				fiscalDocument.InventPositions.Add(clone);
				added.Add(clone);
				left -= take;
			}

			if(!added.Any())
			{
				return false;
			}

			var net = added.Sum(p => p.Price * p.Quantity - p.DiscountSum);
			var diff = Math.Round(net - line, 2, MidpointRounding.AwayFromZero);
			if(diff != 0)
			{
				var last = added[added.Count - 1];
				var maxDiscount = last.Price * last.Quantity;
				last.DiscountSum = Math.Round(last.DiscountSum + diff, 2, MidpointRounding.AwayFromZero);
				if(last.DiscountSum < 0)
				{
					last.DiscountSum = 0;
				}

				if(maxDiscount > 0 && last.DiscountSum > maxDiscount)
				{
					last.DiscountSum = maxDiscount;
				}
			}

			return true;
		}

		private static decimal DiscountLeavingNet(decimal price, decimal quantity, decimal line)
		{
			var gross = price * quantity;
			var discount = Math.Round(gross - line, 2, MidpointRounding.AwayFromZero);
			if(discount < 0)
			{
				return 0;
			}

			if(gross > 0 && discount > gross)
			{
				return gross;
			}

			return discount;
		}

		private static bool CommitReturnIfPaid(EdoFiscalDocument fiscalDocument)
		{
			var paid = fiscalDocument.MoneyPositions?.Sum(x => x.Sum) ?? 0m;
			if(fiscalDocument.InventPositions.Any(p => p != null && p.Quantity > 0) && paid > 0)
			{
				return true;
			}

			fiscalDocument.InventPositions.Clear();
			fiscalDocument.MoneyPositions.Clear();
			return false;
		}

		private static void EnsureReturnPositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			OrderEntity currentOrder,
			FiscalOrderSnapshot previousSnapshot)
		{
			if(CommitReturnIfPaid(fiscalDocument))
			{
				return;
			}

			fiscalDocument.InventPositions.Clear();
			fiscalDocument.MoneyPositions.Clear();

			if(TryFillDecreasedAgainstSourcePositions(fiscalDocument, sourceDocument, currentOrder))
			{
				return;
			}

			TryFillReturnFromSnapshotShortfall(fiscalDocument, sourceDocument, currentOrder, previousSnapshot);
		}

		private static bool TryFillDecreasedAgainstSourcePositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			OrderEntity currentOrder)
		{
			var order = currentOrder ?? sourceDocument?.ReceiptEdoTask?.FormalEdoRequest?.Order;
			var orderItems = GetOrderItems(order)
				.Where(x => x?.Nomenclature != null)
				.ToList();
			var sourcePositions = sourceDocument?.InventPositions?
				.Where(x => x != null && x.Quantity > 0)
				.ToList();
			if(!orderItems.Any() || sourcePositions == null || !sourcePositions.Any())
			{
				return false;
			}

			decimal sum = 0;
			foreach(var item in orderItems)
			{
				var positions = sourcePositions
					.Where(p => PositionMatchesAnyOrderItem(p, new[] { item }))
					.ToList();
				if(!positions.Any())
				{
					continue;
				}

				var delta = positions.Sum(p => p.Quantity) - item.CurrentCount;
				if(delta <= 0)
				{
					continue;
				}

				foreach(var position in positions)
				{
					if(delta <= 0)
					{
						break;
					}

					var take = Math.Min(position.Quantity, delta);
					var discount = position.Quantity == 0
						? 0
						: Math.Round(position.DiscountSum * take / position.Quantity, 2, MidpointRounding.AwayFromZero);
					var gross = position.Price * take;
					if(discount > gross)
					{
						discount = gross;
					}

					sum += gross - discount;
					fiscalDocument.InventPositions.Add(CloneInventPosition(position, take, discount));
					delta -= take;
				}
			}

			if(!fiscalDocument.InventPositions.Any())
			{
				return false;
			}

			AddMoneyPosition(fiscalDocument, sourceDocument, sum);
			return CommitReturnIfPaid(fiscalDocument);
		}

		private static bool TryFillReturnFromSnapshotShortfall(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			OrderEntity currentOrder,
			FiscalOrderSnapshot previousSnapshot)
		{
			var previousItems = previousSnapshot?.Items?
				.Where(x => x != null && x.Quantity > 0)
				.ToList();
			if(previousItems == null || !previousItems.Any())
			{
				return false;
			}

			var order = currentOrder ?? sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
			var orderItems = GetOrderItems(order).Where(x => x?.Nomenclature != null).ToList();
			decimal sum = 0;

			foreach(var group in previousItems.GroupBy(SnapshotItemKey))
			{
				var currentQuantity = CurrentQuantityForSnapshotGroup(group.First(), orderItems);
				if(!currentQuantity.HasValue)
				{
					continue;
				}

				var groupQuantity = group.Sum(x => x.Quantity);
				var delta = groupQuantity - currentQuantity.Value;
				if(delta <= 0)
				{
					continue;
				}

				foreach(var item in group)
				{
					if(delta <= 0)
					{
						break;
					}

					var take = Math.Min(item.Quantity, delta);
					var discount = item.Quantity == 0
						? 0
						: Math.Round(item.DiscountSum * take / item.Quantity, 2, MidpointRounding.AwayFromZero);
					var gross = item.Price * take;
					var net = gross - discount;
					if(net <= 0 && item.Sum > 0 && item.Quantity > 0)
					{
						net = Math.Round(item.Sum * take / item.Quantity, 2, MidpointRounding.AwayFromZero);
					}

					if(net <= 0 && gross > 0)
					{
						net = gross;
						discount = 0;
					}
					else
					{
						discount = Math.Round(gross - net, 2, MidpointRounding.AwayFromZero);
						if(discount < 0)
						{
							discount = 0;
						}

						if(gross > 0 && discount > gross)
						{
							discount = gross;
						}

						net = gross - discount;
					}

					if(net < 0)
					{
						net = 0;
					}

					sum += net;
					var orderItem = ResolveOrderItemForSnapshot(item, orderItems);
					fiscalDocument.InventPositions.Add(new FiscalInventPosition
					{
						Name = Truncate(item.Name, 128) ?? string.Empty,
						Quantity = take,
						Price = item.Price,
						DiscountSum = discount,
						Vat = orderItem != null
							? ResolveVat(sourceDocument, orderItem)
							: sourceDocument.InventPositions.FirstOrDefault()?.Vat ?? FiscalVat.VatFree,
						OrderItems = orderItem != null
							? new ObservableList<OrderItemEntity> { orderItem }
							: new ObservableList<OrderItemEntity>()
					});
					delta -= take;
				}
			}

			if(!fiscalDocument.InventPositions.Any())
			{
				return false;
			}

			AddMoneyPosition(fiscalDocument, sourceDocument, sum);
			return CommitReturnIfPaid(fiscalDocument);
		}

		private static string SnapshotItemKey(FiscalOrderSnapshotItem item)
		{
			if(item.NomenclatureId.HasValue && item.NomenclatureId.Value > 0)
			{
				return "id:" + item.NomenclatureId.Value;
			}

			return "name:" + (item.Name?.Trim() ?? string.Empty).ToUpperInvariant();
		}

		private static decimal? CurrentQuantityForSnapshotGroup(
			FiscalOrderSnapshotItem sample,
			IList<OrderItemEntity> orderItems)
		{
			var matched = orderItems.Where(x => SnapshotItemMatchesOrderItem(sample, x)).ToList();
			if(!matched.Any())
			{
				return null;
			}

			return matched.Sum(x => x.CurrentCount);
		}

		private static OrderItemEntity ResolveOrderItemForSnapshot(
			FiscalOrderSnapshotItem item,
			IList<OrderItemEntity> orderItems)
		{
			return orderItems.FirstOrDefault(x => SnapshotItemMatchesOrderItem(item, x));
		}

		private static bool SnapshotItemMatchesOrderItem(FiscalOrderSnapshotItem item, OrderItemEntity orderItem)
		{
			if(item == null || OrderItemNomenclatureAccess.Of(orderItem) == null)
			{
				return false;
			}

			if(item.NomenclatureId.HasValue
				&& item.NomenclatureId.Value > 0
				&& orderItem.Nomenclature.Id == item.NomenclatureId.Value)
			{
				return true;
			}

			return InventNameMatchesNomenclature(item.Name, orderItem.Nomenclature);
		}

		private static decimal ResolveRemainingQuantity(OrderEntity order, IList<FiscalInventPosition> sourcePositions)
		{
			var items = GetOrderItems(order)
				.Where(x => x?.Nomenclature != null)
				.ToList();
			var matched = MatchOrderItemsToSourcePositions(items, sourcePositions);
			if(!matched.Any())
			{
				return 0;
			}

			return matched.Sum(x => x.CurrentCount);
		}

		private static List<OrderItemEntity> MatchOrderItemsToSourcePositions(
			IList<OrderItemEntity> orderItems,
			IList<FiscalInventPosition> sourcePositions)
		{
			var linkedIds = new HashSet<int>(sourcePositions
				.SelectMany(p => p.OrderItems ?? Enumerable.Empty<OrderItemEntity>())
				.Select(OrderItemNomenclatureAccess.Of)
				.Where(x => x != null)
				.Select(x => x.Id));

			var linked = orderItems
				.Where(x => linkedIds.Contains(x.Nomenclature.Id))
				.ToList();
			if(linked.Any())
			{
				return linked;
			}

			var byName = orderItems
				.Where(x => sourcePositions.Any(p => InventNameMatchesNomenclature(p.Name, x.Nomenclature)))
				.ToList();
			if(byName.Any())
			{
				return byName;
			}

			var positive = orderItems.Where(x => x.CurrentCount > 0).ToList();
			var sourceNames = sourcePositions
				.Select(p => p.Name?.Trim())
				.Where(n => !string.IsNullOrWhiteSpace(n))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.Count();
			if(sourceNames <= 1 && positive.Select(x => x.Nomenclature.Id).Distinct().Count() == 1)
			{
				return positive;
			}

			return new List<OrderItemEntity>();
		}

		private static bool PositionMatchesAnyOrderItem(
			FiscalInventPosition position,
			IList<OrderItemEntity> orderItems)
		{
			if(position == null || orderItems == null)
			{
				return false;
			}

			foreach(var linked in position.OrderItems ?? Enumerable.Empty<OrderItemEntity>())
			{
				OrderItemNomenclatureAccess.Of(linked);
			}

			foreach(var orderItem in orderItems)
			{
				OrderItemNomenclatureAccess.Of(orderItem);
			}

			var linkedIds = new HashSet<int>((position.OrderItems ?? Enumerable.Empty<OrderItemEntity>())
				.Where(x => x?.Nomenclature != null)
				.Select(x => x.Nomenclature.Id));
			if(orderItems.Any(x => linkedIds.Contains(x.Nomenclature.Id)))
			{
				return true;
			}

			return orderItems.Any(x => InventNameMatchesNomenclature(position.Name, x.Nomenclature));
		}

		private static void AlignReturnedMoney(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			decimal targetMoney)
		{
			var positions = fiscalDocument.InventPositions.Where(p => p != null).ToList();
			var inventSum = positions.Sum(p => p.Price * p.Quantity - p.DiscountSum);
			if(targetMoney <= 0)
			{
				if(inventSum < 0)
				{
					inventSum = 0;
				}

				AddMoneyPosition(fiscalDocument, sourceDocument, Math.Round(inventSum, 2, MidpointRounding.AwayFromZero));
				return;
			}

			var diff = Math.Round(inventSum - targetMoney, 2, MidpointRounding.AwayFromZero);
			if(diff != 0 && positions.Any())
			{
				var last = positions.Last();
				var maxDiscount = last.Price * last.Quantity;
				last.DiscountSum = Math.Round(last.DiscountSum + diff, 2, MidpointRounding.AwayFromZero);
				if(last.DiscountSum < 0)
				{
					last.DiscountSum = 0;
				}

				if(maxDiscount > 0 && last.DiscountSum > maxDiscount)
				{
					last.DiscountSum = maxDiscount;
				}

				inventSum = positions.Sum(p => p.Price * p.Quantity - p.DiscountSum);
			}

			if(inventSum <= 0 && positions.Any())
			{
				var last = positions.Last();
				last.DiscountSum = 0;
				if(last.Quantity > 0)
				{
					last.Price = Math.Round(targetMoney / last.Quantity, 2, MidpointRounding.AwayFromZero);
				}
				else
				{
					last.Price = targetMoney;
					last.Quantity = 1;
				}

				inventSum = last.Price * last.Quantity;
			}

			if(inventSum < 0)
			{
				inventSum = 0;
			}

			AddMoneyPosition(fiscalDocument, sourceDocument, Math.Round(inventSum, 2, MidpointRounding.AwayFromZero));
		}

		/// <summary>
		/// Весь предыдущий остаток: парный SALE переписывает заказ целиком.
		/// </summary>
		private static bool TryFillNomenclatureChangeReturnPositions(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			OrderEntity currentOrder,
			FiscalOrderSnapshot previousSnapshot)
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

			var sourceByNomenclature = GroupSourceInventByNomenclature(sourceDocument, order);
			var snapshotHasRemainder = previousSnapshot?.Items != null
				&& previousSnapshot.Items.Any(x => x != null && x.Quantity > 0);

			decimal returnedSum = 0;
			decimal snapshotMoney = 0;
			var hasSnapshotMoney = false;
			var hasReturn = false;

			foreach(var sourceGroup in sourceByNomenclature)
			{
				var sourceQuantity = sourceGroup.Value.Sum(p => p.Quantity);
				var oldQuantity = BaselineQuantityForGroup(sourceGroup.Value, previousSnapshot);
				if(oldQuantity <= 0)
				{
					if(snapshotHasRemainder)
					{
						continue;
					}

					oldQuantity = sourceQuantity;
				}

				if(oldQuantity <= 0)
				{
					continue;
				}

				hasReturn = true;
				var before = returnedSum;
				if(!TryAddMarkedReturnPositions(fiscalDocument, sourceDocument, sourceGroup.Value, oldQuantity, ref returnedSum))
				{
					AddUnmarkedReturnPositions(fiscalDocument, sourceDocument, sourceGroup.Value, oldQuantity, ref returnedSum);
				}

				var groupMoney = SnapshotMoneyForGroup(sourceGroup.Value, previousSnapshot);
				if(groupMoney.HasValue)
				{
					snapshotMoney += groupMoney.Value;
					hasSnapshotMoney = true;
				}
				else
				{
					snapshotMoney += returnedSum - before;
				}
			}

			if(!hasReturn || !fiscalDocument.InventPositions.Any())
			{
				fiscalDocument.InventPositions.Clear();
				fiscalDocument.MoneyPositions.Clear();
				return false;
			}

			if(hasSnapshotMoney && snapshotMoney + 0.009m < returnedSum)
			{
				AlignReturnedMoney(fiscalDocument, sourceDocument, snapshotMoney);
			}
			else
			{
				AddMoneyPosition(fiscalDocument, sourceDocument, returnedSum);
			}

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
				var mapped = currentByNomenclature.TryGetValue(sourceGroup.Key, out var current);
				var newQuantity = mapped
					? current.Quantity
					: ResolveRemainingQuantity(order, sourceGroup.Value);
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

			var orderItems = GetOrderItems(order)
				.Where(x => x.Nomenclature != null)
				.ToList();

			var deliveredNomenclatureIds = orderItems
				.Where(x => x.CurrentCount > 0)
				.Select(x => x.Nomenclature.Id)
				.Distinct()
				.ToList();

			if(deliveredNomenclatureIds.Count == 1)
			{
				foreach(var position in unmatched)
				{
					AddToNomenclatureGroup(result, deliveredNomenclatureIds[0], position);
				}

				return result;
			}

			var orderNomenclatureIds = orderItems
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
				var byPrice = orderItems
					.Where(x => x.CurrentCount > 0 && x.Price == position.Price)
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

		private static bool TryFillReturnFromPreviousSnapshot(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			FiscalOrderSnapshot previousSnapshot)
		{
			var items = previousSnapshot?.Items?
				.Where(x => x != null && x.Quantity > 0)
				.ToList();

			if(items == null || !items.Any() || sourceDocument?.InventPositions == null)
			{
				return false;
			}

			var sourcePositions = sourceDocument.InventPositions
				.Where(x => x != null && x.Quantity > 0)
				.ToList();
			if(!sourcePositions.Any())
			{
				return false;
			}

			var left = sourcePositions.Select(x => x.Quantity).ToList();
			decimal returnedSum = 0;
			var added = false;

			foreach(var item in items)
			{
				var need = item.Quantity;
				for(var i = 0; i < sourcePositions.Count && need > 0; i++)
				{
					if(left[i] <= 0 || !PositionMatchesSnapshotItem(sourcePositions[i], item))
					{
						continue;
					}

					var take = Math.Min(left[i], need);
					returnedSum += AddReturnInventPosition(fiscalDocument, sourceDocument, sourcePositions[i], take);
					left[i] -= take;
					need -= take;
					added = true;
				}
			}

			if(!added)
			{
				return false;
			}

			AddMoneyPosition(fiscalDocument, sourceDocument, returnedSum);
			return true;
		}

		private static bool PositionMatchesSnapshotItem(FiscalInventPosition position, FiscalOrderSnapshotItem item)
		{
			if(position == null || item == null)
			{
				return false;
			}

			if(!string.IsNullOrWhiteSpace(position.Name)
				&& !string.IsNullOrWhiteSpace(item.Name)
				&& string.Equals(position.Name.Trim(), item.Name.Trim(), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return item.Price > 0 && position.Price == item.Price;
		}

		private static decimal? SnapshotMoneyForGroup(
			IEnumerable<FiscalInventPosition> sourceGroup,
			FiscalOrderSnapshot previousSnapshot)
		{
			var positions = sourceGroup.ToList();
			if(!positions.Any() || previousSnapshot?.Items == null)
			{
				return null;
			}

			var sample = positions.First();
			var matched = previousSnapshot.Items
				.Where(x => x.Quantity > 0 && PositionMatchesSnapshotItem(sample, x))
				.ToList();
			if(!matched.Any())
			{
				return null;
			}

			return matched.Sum(x => x.Sum);
		}

		private static decimal BaselineQuantityForGroup(
			IEnumerable<FiscalInventPosition> sourceGroup,
			FiscalOrderSnapshot previousSnapshot)
		{
			var positions = sourceGroup.ToList();
			var sample = positions.First();
			if(previousSnapshot?.Items != null)
			{
				var matched = previousSnapshot.Items
					.Where(x => x.Quantity > 0 && PositionMatchesSnapshotItem(sample, x))
					.ToList();
				if(matched.Any())
				{
					return matched.Sum(x => x.Quantity);
				}
			}

			return positions.Sum(x => x.Quantity);
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
		/// Возвращаемые единицы с их productMark.
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

				// целой позицией, чтобы сохранить productMark
				if(sourcePosition.Quantity <= remainingDelta)
				{
					returnedSum += AddReturnInventPosition(
						fiscalDocument, sourceDocument, sourcePosition, sourcePosition.Quantity);
					remainingDelta -= sourcePosition.Quantity;
					continue;
				}

				// уменьшаем количество, марка та же
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
			var positionsToReturn = sourcePositions.ToList();

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
			FiscalOrderSnapshot currentSnapshot,
			bool allowEmptyFallbackToSource,
			bool useCurrentOrderPaymentType)
		{
			var order = currentOrder ?? sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
			var orderItems = GetOrderItems(order)
				.Where(x => x.CurrentCount > 0 && x.Nomenclature != null)
				.ToList();

			if(orderItems.Any())
			{
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
				return;
			}

			if(TryFillFromSnapshot(fiscalDocument, sourceDocument, order, currentSnapshot, useCurrentOrderPaymentType))
			{
				return;
			}

			if(allowEmptyFallbackToSource)
			{
				CloneAllPositions(fiscalDocument, sourceDocument);
			}
		}

		private static bool TryFillFromSnapshot(
			EdoFiscalDocument fiscalDocument,
			EdoFiscalDocument sourceDocument,
			OrderEntity order,
			FiscalOrderSnapshot currentSnapshot,
			bool useCurrentOrderPaymentType)
		{
			var snapshotItems = currentSnapshot?.Items?
				.Where(x => x != null && x.Quantity > 0)
				.ToList();

			if(snapshotItems == null || !snapshotItems.Any())
			{
				return false;
			}

			var orderItems = GetOrderItems(order).ToList();
			decimal sum = 0;

			foreach(var item in snapshotItems)
			{
				var orderItem = item.NomenclatureId.HasValue
					? orderItems.FirstOrDefault(oi => oi.Nomenclature?.Id == item.NomenclatureId.Value)
					: null;

				var discount = item.DiscountSum;
				sum += item.Price * item.Quantity - discount;

				fiscalDocument.InventPositions.Add(new FiscalInventPosition
				{
					Name = Truncate(item.Name, 128) ?? string.Empty,
					Quantity = item.Quantity,
					Price = item.Price,
					DiscountSum = discount,
					Vat = orderItem != null
						? ResolveVat(sourceDocument, orderItem)
						: sourceDocument.InventPositions.FirstOrDefault()?.Vat ?? FiscalVat.VatFree,
					OrderItems = orderItem != null
						? new ObservableList<OrderItemEntity> { orderItem }
						: new ObservableList<OrderItemEntity>()
				});
			}

			var paymentType = useCurrentOrderPaymentType && order != null
				? MapOrderPaymentType(order.PaymentType)
				: sourceDocument.MoneyPositions.FirstOrDefault()?.PaymentType ?? FiscalPaymentType.Cash;

			AddMoneyPosition(fiscalDocument, paymentType, sum);
			return true;
		}

		private static IEnumerable<OrderItemEntity> GetOrderItems(OrderEntity order)
		{
			if(order == null)
			{
				return Enumerable.Empty<OrderItemEntity>();
			}

			var result = new List<OrderItemEntity>();
			var seenItems = new HashSet<OrderItemEntity>();
			var seenIds = new HashSet<int>();

			void AddItems(IEnumerable<OrderItemEntity> items)
			{
				if(items == null)
				{
					return;
				}

				foreach(var item in items)
				{
					if(item == null || !seenItems.Add(item))
					{
						continue;
					}

					if(item.Id > 0 && !seenIds.Add(item.Id))
					{
						continue;
					}

					OrderItemNomenclatureAccess.Of(item);
					result.Add(item);
				}
			}

			if(order is Order domainOrder)
			{
				AddItems(domainOrder.OrderItems);
				AddItems(domainOrder.ObservableOrderItems);
			}
			else
			{
				AddItems(order.OrderItems);
			}

			return result;
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

			// money из invent со скидками
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

		private string GetContact(
			EdoFiscalDocument sourceDocument,
			ReceiptCorrectionProcessDocument processDocument,
			OrderEntity currentOrder)
		{
			if(processDocument.PlannedDocumentType != FiscalDocumentType.Sale
				&& processDocument.PlannedDocumentType != FiscalDocumentType.SaleCorrection)
			{
				return sourceDocument.Contact;
			}

			var order = currentOrder ?? sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;
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

		private static string GetClientInn(
			EdoFiscalDocument sourceDocument,
			ReceiptCorrectionProcessDocument processDocument,
			OrderEntity currentOrder)
		{
			string inn;
			if(processDocument.PlannedDocumentType != FiscalDocumentType.Sale
				&& processDocument.PlannedDocumentType != FiscalDocumentType.SaleCorrection)
			{
				inn = sourceDocument.ClientInn;
			}
			else
			{
				var client = currentOrder?.Client
					?? sourceDocument.ReceiptEdoTask?.FormalEdoRequest?.Order?.Client;
				if(client?.ReasonForLeaving == ReasonForLeaving.Resale
					&& !string.IsNullOrWhiteSpace(client.INN))
				{
					inn = client.INN.Trim();
				}
				else
				{
					inn = client == null ? sourceDocument.ClientInn : null;
				}
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

			var fromSiblings = ResolveReturnDiscountFromSiblingInvent(sourceDocument, source, quantity);
			if(fromSiblings > 0)
			{
				return fromSiblings;
			}

			var orderItem = ResolveReturnOrderItem(sourceDocument, source);
			if(orderItem != null)
			{
				var orderDiscount = orderItem.OriginalDiscountMoney
					?? (orderItem.DiscountMoney > 0 ? orderItem.DiscountMoney : (decimal?)null)
					?? 0;

				var baseQty = orderItem.Count > 0
					? orderItem.Count
					: (source.Quantity > 0 ? source.Quantity : quantity);

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

		private static decimal ResolveReturnDiscountFromSiblingInvent(
			EdoFiscalDocument sourceDocument,
			FiscalInventPosition source,
			decimal quantity)
		{
			if(sourceDocument?.InventPositions == null || string.IsNullOrWhiteSpace(source.Name))
			{
				return 0;
			}

			var sibling = sourceDocument.InventPositions
				.Where(p => p != null
					&& p.DiscountSum != 0
					&& p.Quantity != 0
					&& p.Price == source.Price
					&& string.Equals(p.Name?.Trim(), source.Name.Trim(), StringComparison.OrdinalIgnoreCase))
				.FirstOrDefault();

			if(sibling == null)
			{
				return 0;
			}

			return Math.Round(sibling.DiscountSum * (quantity / sibling.Quantity), 2);
		}

		private static OrderItemEntity ResolveReturnOrderItem(
			EdoFiscalDocument sourceDocument,
			FiscalInventPosition source)
		{
			var linked = source.OrderItems?.FirstOrDefault(x => x != null);
			if(linked != null)
			{
				return linked;
			}

			var order = sourceDocument?.ReceiptEdoTask?.FormalEdoRequest?.Order;
			if(order == null || string.IsNullOrWhiteSpace(source.Name))
			{
				return null;
			}

			var matches = GetOrderItems(order)
				.Where(x => x?.Nomenclature != null && InventNameMatchesNomenclature(source.Name, x.Nomenclature))
				.ToList();

			if(!matches.Any())
			{
				return null;
			}

			return matches.FirstOrDefault(x => x.OriginalDiscountMoney > 0)
				?? matches.FirstOrDefault(x => x.DiscountMoney > 0)
				?? matches.FirstOrDefault(x => x.Count > 0)
				?? matches.FirstOrDefault();
		}

		private static decimal ResolveReturnDiscountFromSourceMoney(
			EdoFiscalDocument sourceDocument,
			FiscalInventPosition source,
			decimal quantity)
		{
			if(sourceDocument?.InventPositions == null || source.Quantity == 0)
			{
				return 0;
			}

			var moneyPositions = sourceDocument.MoneyPositions;
			if(moneyPositions == null || !moneyPositions.Any())
			{
				return 0;
			}

			var inventGross = sourceDocument.InventPositions.Sum(p => p.Price * p.Quantity);
			if(inventGross <= 0)
			{
				return 0;
			}

			var inventDiscountTotal = sourceDocument.InventPositions.Sum(p => p.DiscountSum);
			var moneySum = moneyPositions.Sum(m => m.Sum);
			var missingDiscount = inventGross - inventDiscountTotal - moneySum;
			if(missingDiscount <= 0.01m)
			{
				return 0;
			}

			// только по позициям без скидки
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
		/// Скидка нового SALE из текущего заказа.
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
