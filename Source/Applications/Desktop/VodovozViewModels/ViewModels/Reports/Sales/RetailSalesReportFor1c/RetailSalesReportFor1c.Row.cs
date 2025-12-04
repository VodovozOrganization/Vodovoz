using System;
using Vodovoz.Domain.Client;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales.RetailSalesReportFor1c
{
	public partial class RetailSalesReportFor1c
	{
		public class RetailSalesReportFor1cRow
		{
			/// <summary>
			/// Код 1С номенклатуры
			/// </summary>
			public string Code1c { get; set; }

			/// <summary>
			/// Товар
			/// </summary>
			public string Nomenclature { get; set; }

			/// <summary>
			/// Количество
			/// </summary>
			public decimal Amount { get; set; }

			/// <summary>
			/// Единица измерения
			/// </summary>
			public string Mesure { get; set; }

			/// <summary>
			/// Цена
			/// </summary>
			public decimal Price { get; set; }

			/// <summary>
			/// НДС
			/// </summary>
			public decimal Nds { get; set; }

			/// <summary>
			/// Сумма
			/// </summary>
			public decimal Sum { get; set; }
		}
	}
}
