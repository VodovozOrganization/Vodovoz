namespace Vodovoz.Core.Application.Receipts.Correction
{
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
