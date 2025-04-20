namespace Edo.Problems.Validation.Sources
{
	public partial class AllResaleCodesScannedEdoValidator
	{
		private class ScannedDto
		{
			public string Gtin { get; set; }
			public decimal Amount { get; set; }
			public int NomenclatureId { get; set; }
		}
	}
}
