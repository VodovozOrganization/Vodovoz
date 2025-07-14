using System.Collections.Generic;

namespace Vodovoz.ViewModels.Store.Reports
{
	public partial class DefectiveItemsReport
	{
		public class SummaryByNomenclatureWithTypeDefectRow
		{
			/// <summary>
			/// Название номенклатуры
			/// </summary>
			public string NomeclatureNameForDefectRow { get; set; }
			
			/// <summary>
			/// Количество по типам дефекта
			/// </summary>
			public IEnumerable<string> DynamicColumnsByNomenclatureWithTypeDefectRow { get; set; }
			
		}
	}
}
