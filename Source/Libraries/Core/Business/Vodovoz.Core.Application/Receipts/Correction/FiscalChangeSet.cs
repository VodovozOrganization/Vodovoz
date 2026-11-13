using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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

		public IList<FiscalPositionChange> AllPositions { get; set; } = new List<FiscalPositionChange>();

		public int? PreviousOrganizationId { get; set; }
		public int? NewOrganizationId { get; set; }

		public int? PreviousCounterpartyId { get; set; }
		public int? NewCounterpartyId { get; set; }

		public int? PreviousContractId { get; set; }
		public int? NewContractId { get; set; }

		public string PreviousPaymentType { get; set; }
		public string NewPaymentType { get; set; }

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
				IsFullCancellation.ToString(),
				$"{PreviousOrganizationId}->{NewOrganizationId}",
				$"{PreviousCounterpartyId}->{NewCounterpartyId}",
				$"{PreviousContractId}->{NewContractId}",
				$"{PreviousPaymentType}->{NewPaymentType}"
			};

			foreach(var positionChange in PositionChanges
				.OrderBy(x => x.NomenclatureId ?? 0)
				.ThenBy(x => x.Name, StringComparer.Ordinal))
			{
				parts.Add(FormattableString.Invariant(
					$"{positionChange.NomenclatureId}:{positionChange.OldQuantity}->{positionChange.NewQuantity}:{positionChange.OldPrice}->{positionChange.NewPrice}:{positionChange.OldDiscountSum}->{positionChange.NewDiscountSum}"));
			}

			var raw = string.Join("|", parts);
			using(var sha = SHA256.Create())
			{
				var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
				return BitConverter.ToString(hash).Replace("-", string.Empty);
			}
		}
	}
}
