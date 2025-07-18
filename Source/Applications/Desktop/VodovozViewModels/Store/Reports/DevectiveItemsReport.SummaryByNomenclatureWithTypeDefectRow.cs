﻿using System.Collections.Generic;

namespace Vodovoz.ViewModels.Store.Reports
{
	public partial class DefectiveItemsReport
	{
		/// <summary>
		/// Строка номенклатуры для таблицы номенклатуры-тип брака
		/// </summary>
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
