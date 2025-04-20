namespace Edo.Problems.Validation.Sources
{
	public partial class AllResaleCodesScannedEdoValidator
	{
		private class ScannedNomenclatureGtinDto
		{
			public int NomenclatureId { get; set; }
			public string Gtin { get; set; }
			public decimal Amount { get; set; }
		}
	}
}
