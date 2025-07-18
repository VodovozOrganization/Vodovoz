﻿using System.Collections.Generic;

namespace Vodovoz.ViewModels.Store.Reports
{
	public partial class DefectiveItemsReport
	{
		/// <summary>
		/// Строка номенклатуры для таблицы номенклатуры-источник брака
		/// </summary>
		public class SummaryByNomenclatureRow
		{
			/// <summary>
			/// Название номенклатуры
			/// </summary>
			public string NomeclatureNameForSourceRow { get; set; }
			
			/// <summary>
			/// Количество по виновным string
			/// </summary>
			public IEnumerable<string> DynamicColumnsByNomenclatureRow { get; set; }
			
		}
	}
}
