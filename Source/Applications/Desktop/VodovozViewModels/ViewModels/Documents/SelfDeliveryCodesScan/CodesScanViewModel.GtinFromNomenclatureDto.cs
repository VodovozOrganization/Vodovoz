namespace Vodovoz.ViewModels.ViewModels.Documents.SelfDeliveryCodesScan
{
	public partial class CodesScanViewModel
	{
		/// <summary>
		/// Gtin номенклатуры
		/// </summary>
		public class GtinFromNomenclatureDto
		{
			/// <summary>
			/// Gtin
			/// </summary>
			public string GtinNumber { get; set; }
			/// <summary>
			/// Номенклатуры
			/// </summary>
			public string NomenclatureName { get; set; }
			/// <summary>
			/// Количество для группового gtin
			/// </summary>
			public int CodesCount { get; set; }
		}
	}
}
