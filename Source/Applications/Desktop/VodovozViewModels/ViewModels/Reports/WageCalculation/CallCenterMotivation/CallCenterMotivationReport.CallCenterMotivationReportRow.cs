using System.Collections.Generic;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Reports.WageCalculation.CallCenterMotivation
{
	public partial class CallCenterMotivationReport
	{
		/// <summary>
		/// Строка отчёта
		/// </summary>
		public partial class CallCenterMotivationReportRow
		{
			/// <summary>
			/// Заголовок
			/// </summary>
			public string Title { get; set; }

			/// <summary>
			/// Индекс
			/// </summary>
			public string Index { get; set; } = string.Empty;

			/// <summary>
			/// Тип строки
			/// </summary>
			public ReportRowType RowType { get; set; }

			/// <summary>
			/// Итоговая строка?
			/// </summary>
			public bool IsTotalsRow => RowType == ReportRowType.Totals;

			/// <summary>
			/// Подзаголовок?
			/// </summary>
			public bool IsSubheaderRow => RowType == ReportRowType.Subheader;

			/// <summary>
			/// Значения показателей
			/// </summary>
			public IList<ValueColumn> SliceColumnValues { get; set; } = new List<ValueColumn>();

			/// <summary>
			/// Динамика
			/// </summary>
			public IList<ValueColumn> DynamicColumns { get; set; } = new List<ValueColumn>();

			/// <summary>
			/// Единица измерения мотивации
			/// </summary>
			public NomenclatureMotivationUnitType? MotivationUnitType { get; set; }

			/// <summary>
			/// Денежный формат?
			/// </summary>
			public bool IsMoneyFormat => MotivationUnitType == NomenclatureMotivationUnitType.Percent;

			/// <summary>
			/// Не показывать продажи?
			/// </summary>
			public bool HideSold { get; set; }
		}
	}
}
