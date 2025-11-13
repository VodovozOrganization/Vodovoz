using System;
using Gamma.Utilities;
using Vodovoz.Domain.Documents;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Store.Reports
{
	public partial class DefectiveItemsReport
	{
		/// <summary>
		/// Строка браковоной номенклатуры для отчета. Используется для последующих отчетов как источник данных
		/// </summary>
		public class DefectiveItemsReportRow
		{
			/// <summary>
			/// ID
			/// </summary>
			public int Id { get; set; }
			
			/// <summary>
			/// Дата брака
			/// </summary>
			public DateTime Date { get; set; }
			
			/// <summary>
			/// ID склада
			/// </summary>
			public int WarehouseId { get; internal set; }
			
			/// <summary>
			/// Количество брака
			/// </summary>
			public decimal Amount { get; set; }
			
			/// <summary>
			/// ID Номенклатуры
			/// </summary>
			public int NomenclatureId { get; set; }
			
			/// <summary>
			/// Old ID Номенклатуры
			/// </summary>
			public int OldNomenclatureId { get; set; }
			
			/// <summary>
			/// Название бракованой номенклатуры
			/// </summary>
			public string DefectiveItemName { get; set; }
			
			/// <summary>
			/// Название бракованой номенклатуры до пересортицы
			/// </summary>
			public string DefectiveItemOldName { get; set; }
			
			/// <summary>
			/// Фамилия водителя
			/// </summary>
			public string DriverLastName { get; set; }
			
			/// <summary>
			/// ID МЛ
			/// </summary>
			public int? RouteListId { get; set; }
			
			/// <summary>
			/// Тип документа
			/// </summary>
			public Type DocumentType { get; set; }
			
			/// <summary>
			/// ID брака
			/// </summary>
			public int DefectTypeId { get; set; }
			
			/// <summary>
			/// Название брака
			/// </summary>
			public string DefectTypeName { get; set; }
			
			/// <summary>
			/// Источник брака
			/// </summary>
			public DefectSource DefectSource { get; set; }
			
			/// <summary>
			/// Источник брака в виде строки
			/// </summary>
			public string DefectSourceName => DefectSource.GetEnumTitle();
			
			/// <summary>
			/// Фамилия автора
			/// </summary>
			public string AuthorLastName { get; set; }
			
			/// <summary>
			/// Комментарий
			/// </summary>
			public string Comment {  get; set; }
			
			/// <summary>
			/// Время обнаружения брака
			/// </summary>
			public string DefectDetectedAt { get; set; }
			
			/// <summary>
			/// Тип документа в виде строки
			/// </summary>
			public string DocumentTypeName => DocumentType.GetClassUserFriendlyName().Nominative.CapitalizeSentence();
		}
	}
}
