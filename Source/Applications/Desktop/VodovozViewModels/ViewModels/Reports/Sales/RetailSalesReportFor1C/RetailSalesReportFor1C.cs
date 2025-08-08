using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Project.Services.FileDialog;
using QSProjectsLib;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Extensions;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Presentation.ViewModels.Reports;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales.RetailSalesReportFor1C
{
	/// <summary>
	/// Отчёт о розничных продажах для 1С
	/// </summary>
	[Appellative(Nominative = "Отчёт о розничных продажах для 1С")]
	public class RetailSalesReportFor1C : IClosedXmlReport
	{
		/// <summary>
		/// Итоговая сумма
		/// </summary>
		private decimal TotalSum => Rows.Sum(x => x.Sum);
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
		public string TotalSumInWords =>  RusCurrency.Str(TotalSum);
		
		/// <summary>
		/// Поставщик
		/// </summary>
		public string Supplier => $"{OrganizationForTitle.Name}, ИНН {OrganizationForTitle.INN}, " +
			$"{OrganizationVersionForTitle.JurAddress}, тел.: {Phone}";
		
		/// <summary>
		/// Строки отчёта
		/// </summary>
		public List<RetailSalesReportFor1CRow> Rows { get; set; } = new List<RetailSalesReportFor1CRow>();

		public string TemplatePath => @".\Reports\Sales\RetailSalesReportFor1C.xlsx";

		public void Generate(IList<Order> orders)
		{
			var rowItems = new List<RetailSalesReportFor1CRow>();
			
			foreach(var order in orders)
			{
				foreach(var item in order.OrderItems)
				{
					var rowItem = new RetailSalesReportFor1CRow
					{
						Amount = item.CurrentCount,
						Mesure = item.Nomenclature.Unit.Name,
						Code1c = item.Nomenclature.Code1c,
						Nomenclature =  item.Nomenclature.Name,
						Price =  item.Price,
						Sum =  item.Sum,
						Nds = item.CurrentNDS
					};
					
					rowItems.Add(rowItem);
				}
			}
			
			Rows = rowItems;
		}
		
		public void SaveReport(IDialogSettingsFactory dialogSettingsFactory, IFileDialogService fileDialogService)
		{
			var dialogSettings = dialogSettingsFactory.CreateForClosedXmlReport(this);

			var saveDialogResult = fileDialogService.RunSaveFileDialog(dialogSettings);

			if(saveDialogResult.Successful)
			{
				this.RenderTemplate().Export(saveDialogResult.Path);
			}
		}
		
		
	}
}
