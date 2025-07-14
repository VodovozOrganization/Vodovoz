using System.Collections.Generic;

namespace Vodovoz.ViewModels.Store.Reports
{
	public partial class DefectiveItemsReport
	{
		public class SummaryByNomenclatureRow
		{
			/// <summary>
			/// Название номенклатуры
			/// </summary>
			public string NomeclatureName { get; set; }
			
			/// <summary>
			/// Количество по виновным string
			/// </summary>
			public IEnumerable<string> DynamicColumns { get; set; }
			
			/// <summary>
			/// Количество по виновным decimal
			/// </summary>
			public IEnumerable<decimal> DynamicColumnsValue { get; set; }
		}
	}
}
