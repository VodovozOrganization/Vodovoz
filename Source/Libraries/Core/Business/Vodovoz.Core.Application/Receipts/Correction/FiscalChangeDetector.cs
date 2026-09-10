using System;
using System.Linq;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public class FiscalChangeDetector : IFiscalChangeDetector
	{
		public FiscalChangeSet DetectChanges(FiscalOrderSnapshot previous, FiscalOrderSnapshot current, bool isFullCancellation)
		{
			if(previous == null)
			{
				throw new ArgumentNullException(nameof(previous));
			}

			if(current == null)
			{
				throw new ArgumentNullException(nameof(current));
			}

			var changeSet = new FiscalChangeSet
			{
				IsFullCancellation = isFullCancellation
			};

			changeSet.HasOrganizationChange = previous.OrganizationId != current.OrganizationId;
			changeSet.HasClientChange = previous.CounterpartyId != current.CounterpartyId;
			changeSet.HasContractChange = previous.ContractId != current.ContractId;
			changeSet.HasPaymentTypeChange = previous.PaymentType != current.PaymentType;

			var previousDeliveryDate = previous.DeliveryDate?.Date;
			var currentDeliveryDate = current.DeliveryDate?.Date;
			changeSet.HasDeliveryDateChange = previousDeliveryDate != currentDeliveryDate;

			CompareItems(previous, current, changeSet);

			changeSet.HasChanges = changeSet.IsFullCancellation
				|| changeSet.HasOrganizationChange
				|| changeSet.HasClientChange
				|| changeSet.HasContractChange
				|| changeSet.HasPaymentTypeChange
				|| changeSet.HasDeliveryDateChange
				|| changeSet.HasNomenclatureChange
				|| changeSet.HasQuantityOrAmountDecrease
				|| changeSet.HasQuantityOrAmountIncrease
				|| changeSet.HasPieceItemPriceChange;

			return changeSet;
		}

		private static void CompareItems(FiscalOrderSnapshot previous, FiscalOrderSnapshot current, FiscalChangeSet changeSet)
		{
			var previousItems = (previous.Items ?? Enumerable.Empty<FiscalOrderSnapshotItem>())
				.GroupBy(x => x.NomenclatureId ?? 0)
				.ToDictionary(
					x => x.Key,
					x => new
					{
						Quantity = x.Sum(p => p.Quantity),
						Price = x.First().Price,
						Name = x.First().Name,
						NomenclatureId = x.First().NomenclatureId
					});

			var currentItems = (current.Items ?? Enumerable.Empty<FiscalOrderSnapshotItem>())
				.GroupBy(x => x.NomenclatureId ?? 0)
				.ToDictionary(
					x => x.Key,
					x => new
					{
						Quantity = x.Sum(p => p.Quantity),
						Price = x.First().Price,
						Name = x.First().Name,
						NomenclatureId = x.First().NomenclatureId
					});

			var allNomenclatureIds = previousItems.Keys.Union(currentItems.Keys).Distinct();

			foreach(var nomenclatureId in allNomenclatureIds)
			{
				previousItems.TryGetValue(nomenclatureId, out var previousItem);
				currentItems.TryGetValue(nomenclatureId, out var currentItem);

				var oldQuantity = previousItem?.Quantity ?? 0;
				var newQuantity = currentItem?.Quantity ?? 0;
				var oldPrice = previousItem?.Price ?? 0;
				var newPrice = currentItem?.Price ?? 0;

				if(previousItem == null || currentItem == null)
				{
					changeSet.HasNomenclatureChange = true;
				}

				if(newQuantity < oldQuantity)
				{
					changeSet.HasQuantityOrAmountDecrease = true;
				}

				if(newQuantity > oldQuantity)
				{
					changeSet.HasQuantityOrAmountIncrease = true;
				}

				if(oldPrice != newPrice && oldQuantity > 0 && newQuantity > 0)
				{
					changeSet.HasPieceItemPriceChange = true;
				}

				if(oldQuantity != newQuantity || oldPrice != newPrice || previousItem == null || currentItem == null)
				{
					changeSet.PositionChanges.Add(new FiscalPositionChange
					{
						NomenclatureId = previousItem?.NomenclatureId ?? currentItem?.NomenclatureId,
						Name = previousItem?.Name ?? currentItem?.Name,
						OldQuantity = oldQuantity,
						NewQuantity = newQuantity,
						OldPrice = oldPrice,
						NewPrice = newPrice,
						IsPieceItem = oldPrice != newPrice
					});
				}
			}
		}
	}
}
