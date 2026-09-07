using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	/// <summary>
	/// Результат сравнения предыдущей и текущей фискальных операций.
	/// </summary>
	public class FiscalChangeSet
	{
		public bool HasChanges { get; set; }

		public bool HasOrganizationChange { get; set; }

		public bool HasClientChange { get; set; }

		public bool HasContractChange { get; set; }

		public bool HasPaymentTypeChange { get; set; }

		public bool HasDeliveryDateChange { get; set; }

		public bool HasNomenclatureChange { get; set; }

		public bool HasQuantityOrAmountDecrease { get; set; }

		public bool HasQuantityOrAmountIncrease { get; set; }

		public bool HasPieceItemPriceChange { get; set; }

		public bool IsFullCancellation { get; set; }

		public IList<FiscalPositionChange> PositionChanges { get; set; } = new List<FiscalPositionChange>();

		public string BuildFingerprint()
		{
			var parts = new List<string>
			{
				HasOrganizationChange.ToString(),
				HasClientChange.ToString(),
				HasContractChange.ToString(),
				HasPaymentTypeChange.ToString(),
				HasDeliveryDateChange.ToString(),
				HasNomenclatureChange.ToString(),
				HasQuantityOrAmountDecrease.ToString(),
				HasQuantityOrAmountIncrease.ToString(),
				HasPieceItemPriceChange.ToString(),
				IsFullCancellation.ToString()
			};

			foreach(var positionChange in PositionChanges.OrderBy(x => x.NomenclatureId ?? 0).ThenBy(x => x.Name))
			{
				parts.Add($"{positionChange.NomenclatureId}:{positionChange.OldQuantity}->{positionChange.NewQuantity}:{positionChange.OldPrice}->{positionChange.NewPrice}");
			}

			return string.Join("|", parts);
		}
	}

	public class FiscalPositionChange
	{
		public int? NomenclatureId { get; set; }

		public string Name { get; set; }

		public decimal OldQuantity { get; set; }

		public decimal NewQuantity { get; set; }

		public decimal OldPrice { get; set; }

		public decimal NewPrice { get; set; }

		public bool IsPieceItem { get; set; }
	}
}
