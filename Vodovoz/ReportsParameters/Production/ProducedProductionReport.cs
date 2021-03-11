using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalViewModels;

namespace Vodovoz.ReportsParameters.Production
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProducedProductionReport : SingleUoWWidgetBase, IParametersWidget
	{
		public ProducedProductionReport(
			IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository)
		{
			this.Build();
			
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			yenumcomboboxMonths.ItemsEnum = typeof(Month);
			yenumcomboboxMonths.SelectedItem = Month.September;
			
			ycomboboxProduction.SetRenderTextFunc<Warehouse>(x => x.Name);
			ycomboboxProduction.ItemsList = UoW.Session.QueryOver<Warehouse>().Where(x => x.TypeOfUse == WarehouseUsing.Production).List();

			entryreferenceNomenclature.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(typeof(Nomenclature),
					() =>
					{
						var nomenclatureFilter = new NomenclatureFilterViewModel();
						return new NomenclaturesJournalViewModel(
							nomenclatureFilter,
							UnitOfWorkFactory.GetDefaultFactory,
							ServicesConfig.CommonServices,
							new EmployeeService(),
							nomenclatureSelectorFactory, 
							counterpartySelectorFactory,
							nomenclatureRepository,
							UserSingletonRepository.GetInstance()
							);
					})
			);
			buttonCreateReport.Sensitive = true;
			buttonCreateReport.Clicked += OnButtonCreateReportClicked;
		}
		
		void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			OnUpdate (true);
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по произведенной продукции";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{
			
			CultureInfo ci = new CultureInfo("ru-RU");
			DateTimeFormatInfo mfi = ci.DateTimeFormat;

			var monthNum = MonthToDateTime_AddMonth((Month) yenumcomboboxMonths.SelectedItem, 0).Month;
			string strMonthName = mfi.GetMonthName(monthNum).ToString();
			
			var monthNumMinus1 = MonthToDateTime_AddMonth((Month) yenumcomboboxMonths.SelectedItem, -1).Month;
			string strMonthNameMinus1 = mfi.GetMonthName(monthNumMinus1).ToString();
			
			var monthNumMinus2 = MonthToDateTime_AddMonth((Month) yenumcomboboxMonths.SelectedItem, -2).Month;
			string strMonthNameMinus2 = mfi.GetMonthName(monthNumMinus2).ToString();
			
			return new ReportInfo {
				Identifier = "Production.ProducedProduction",
				Parameters = new Dictionary<string, object> {
					{ "month_start",         MonthStart(MonthToDateTime_AddMonth( (Month) yenumcomboboxMonths.SelectedItem,  0 )) },
					{ "month_end",             MonthEnd(MonthToDateTime_AddMonth( (Month) yenumcomboboxMonths.SelectedItem,  0 )) },
					{ "month_minus_1_start", MonthStart(MonthToDateTime_AddMonth( (Month) yenumcomboboxMonths.SelectedItem, -1 )) },
					{ "month_minus_1_end",     MonthEnd(MonthToDateTime_AddMonth( (Month) yenumcomboboxMonths.SelectedItem, -1 )) },
					{ "month_minus_2_start", MonthStart(MonthToDateTime_AddMonth( (Month) yenumcomboboxMonths.SelectedItem, -2 )) },
					{ "month_minus_2_end",     MonthEnd(MonthToDateTime_AddMonth( (Month) yenumcomboboxMonths.SelectedItem, -2 )) },
					{ "month_name",    strMonthName },
					{ "month_name_minus_1",  strMonthNameMinus1  },
					{ "month_name_minus_2",  strMonthNameMinus2  },
					
					{ "nomenclature_id", (entryreferenceNomenclature.Subject as Nomenclature)?.Id ?? -1},
					{ "warehouse_id", (ycomboboxProduction.SelectedItem as Warehouse)?.Id ?? -1 },
					{ "creation_date", DateTime.Now},
					
					{ "nomenclature_footer_name", (entryreferenceNomenclature.Subject as Nomenclature)?.Name ?? ""},
					{ "warehouse_footer_name", (ycomboboxProduction.SelectedItem as Warehouse)?.Name ?? "" },
					
				}
			};
		}

		void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));

		private DateTime MonthStart(DateTime date)
		{
			return new DateTime(date.Year,date.Month, 1);
		}
		private DateTime MonthEnd(DateTime date)
		{
			return new DateTime(date.Year,date.Month, DateTime.DaysInMonth(date.Year,date.Month));
		}
		
		private DateTime MonthToDateTime_AddMonth(Month month, int n)
		{
			switch (month)
			{
				case Month.September:
					var now1 = DateTime.Now;
					return new DateTime(now1.Year,9,now1.Day).AddMonths(n);
				case Month.October:
					var now2 = DateTime.Now;
					return new DateTime(now2.Year,10,now2.Day).AddMonths(n);
				case Month.November:
					var now3 = DateTime.Now;
					return new DateTime(now3.Year,11,now3.Day).AddMonths(n);
				case Month.December:
					var now4 = DateTime.Now;
					return new DateTime(now4.Year,12,now4.Day).AddMonths(n);
				case Month.January:
					var now5 = DateTime.Now;
					return new DateTime(now5.Year,1,now5.Day).AddMonths(n);
				case Month.February:
					var now6 = DateTime.Now;
					return new DateTime(now6.Year,2,now6.Day).AddMonths(n);
				case Month.March:
					var now7 = DateTime.Now;
					return new DateTime(now7.Year,3,now7.Day).AddMonths(n);
				case Month.April:
					var now8 = DateTime.Now;
					return new DateTime(now8.Year,4,now8.Day).AddMonths(n);
				case Month.May:
					var now9 = DateTime.Now;
					return new DateTime(now9.Year,5,now9.Day).AddMonths(n);
				case Month.June:
					var now10 = DateTime.Now;
					return new DateTime(now10.Year,6,now10.Day).AddMonths(n);
				case Month.July:
					var now11 = DateTime.Now;
					return new DateTime(now11.Year,7,now11.Day).AddMonths(n);
				case Month.August:
					var now12 = DateTime.Now;
					return new DateTime(now12.Year,8,now12.Day).AddMonths(n);
				default:
					throw new ArgumentOutOfRangeException(nameof(month), month, null);
			}
		}
		
	}
	
	enum Month
	{
		[Display ( Name = "Сентябрь")]
		September,
		[Display ( Name = "Октябрь")]
		October,
		[Display ( Name = "Ноябрь")]
		November,
		[Display ( Name = "Декабрь")]
		December,
		[Display ( Name = "Январь")]
		January,
		[Display ( Name = "Февраль")]
		February,
		[Display ( Name = "Март")]
		March,
		[Display ( Name = "Апрель")]
		April,
		[Display ( Name = "Май")]
		May,
		[Display ( Name = "Июнь")]
		June,
		[Display ( Name = "Июль")]
		July,
		[Display ( Name = "Август")]
		August
	}
}
