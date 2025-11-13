namespace Vodovoz.ViewModels.ViewModels.Documents.SelfDeliveryCodesScan
{
	public partial class CodesScanViewModel
	{
		/// <summary>
		/// Информация о прогрессе сканирования
		/// </summary>
		public class CodesScanProgressRow
		{
			/// <summary>
			/// Номенклатура
			/// </summary>
			public string NomenclatureName { get; set; }
			/// <summary>
			/// Отсканировано
			/// </summary>
			public int InSelfDelivery { get; set; }
			/// <summary>
			/// Осталось отсканировать
			/// </summary>
			public int LeftToScan { get; set; }
			/// <summary>
			/// Gtin
			/// </summary>
			public string Gtin { get; set; }
		}
	}
}
