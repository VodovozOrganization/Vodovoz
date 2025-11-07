using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.Domain.Organizations;
using VodovozInfrastructure.Utils;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales.RetailSalesReportFor1c
{
	public partial class RetailSalesReportFor1c
	{
		/// <summary>
		/// Итоговая сумма
		/// </summary>
		private decimal TotalSum => Rows.Sum(x => x.Sum);

		/// <summary>
		/// Итоговая сумма
		/// </summary>
		private decimal TotalNds => Rows.Sum(x => x.Nds);

		/// <summary>
		/// Номер продажи
		/// </summary>
		public string Number => $"{SaleDate.Month:D2}-{SaleDate.Day:D2}-{Suffix}";

		/// <summary>
		/// Заголовок отчёта
		/// </summary>
		public string Title => $"Отчёт о розничных продажах {Number} от {DateTime.Today:dd MMMM yyyy} г.";

		/// <summary>
		/// Дата продажи
		/// </summary>
		public DateTime SaleDate { get; set; }

		/// <summary>
		/// Суффикс организации
		/// </summary>
		public string Suffix => OrganizationVersionForTitle.Organization.Suffix;

		/// <summary>
		/// Версия организации
		/// </summary>
		public OrganizationVersion OrganizationVersionForTitle { get; set; }

		/// <summary>
		/// Организация
		/// </summary>
		public Organization OrganizationForTitle { get; set; }

		/// <summary>
		/// Телефон
		/// </summary>
		public string Phone { get; set; }

		/// <summary>
		/// Итоговая информация
		/// </summary>
		public string TotalInfo => $"Всего наименований {Rows.Count}, на сумму {TotalSum:N2}";

		/// <summary>
		/// Сумма прописью
		/// </summary>
		public string TotalSumInWords => RusCurrency.Str(TotalSum);

		/// <summary>
		/// Поставщик
		/// </summary>
		public string Supplier => $"{OrganizationForTitle.Name}, ИНН {OrganizationForTitle.INN}, " +
			$"{OrganizationVersionForTitle.JurAddress}, тел.: {Phone}";

		/// <summary>
		/// Строки отчёта
		/// </summary>
		public List<RetailSalesReportFor1cRow> Rows { get; set; } = new List<RetailSalesReportFor1cRow>();
	}
}
