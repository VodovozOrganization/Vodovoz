namespace Edo.Problems.Validation.Sources
{
	public partial class AllResaleCodesScannedEdoValidator
	{
		/// <summary>
		/// Отсканированная номенклатура с GTIN 
		/// </summary>
		private class ScannedNomenclatureGtinDto
		{
			/// <summary>
			/// Id номенклатуры
			/// </summary>
			public int NomenclatureId { get; set; }
			/// <summary>
			/// Gtin номенклатуры
			/// </summary>
			public string Gtin { get; set; }
			/// <summary>
			/// Количество
			/// </summary>
			public decimal Amount { get; set; }
		}
	}
}
