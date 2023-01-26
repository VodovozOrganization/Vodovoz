//using System;
//namespace Vodovoz.ReportsParameters
//{
//	[System.ComponentModel.ToolboxItem(true)]
//	public partial class WayBillReportGroupPrint : Gtk.Bin
//	{
//		public WayBillReportGroupPrint()
//		{
//			this.Build();
//		}
//	}
//}

using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using Vodovoz.Domain.Employees;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ReportsParameters
{
	public partial class WayBillReportGroupPrint : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICarJournalFactory _carJournalFactory;
		private readonly IOrganizationJournalFactory _organizationJournalFactory;

		public WayBillReportGroupPrint(IEmployeeJournalFactory employeeJournalFactory, ICarJournalFactory carJournalFactory, IOrganizationJournalFactory organizationJournalFactory)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_carJournalFactory = carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory));
			_organizationJournalFactory = organizationJournalFactory ?? throw new ArgumentNullException(nameof(organizationJournalFactory));

			Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureSingleReport();
			ConfigureGroupReportForOneDay();
			ConfigureGroupReportForPeriod();
		}

		private void ConfigureSingleReport()
		{
			datepickerSingleReport.Date = DateTime.Today;
			timeHourEntrySingleReport.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntrySingleReport.Text = DateTime.Now.Minute.ToString("00.##");

			entryDriverSingleReport.SetEntityAutocompleteSelectorFactory(
				_employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());

			entryCarSingleReport.SetEntityAutocompleteSelectorFactory(_carJournalFactory.CreateCarAutocompleteSelectorFactory());
		}

		private void ConfigureGroupReportForOneDay()
		{
			//Дата по умолчанию
			datepickerGroupReportForOneDay.Date = DateTime.Today;

			// Тип автомобиля
			enumcheckCarTypeOfUseOneDayReport.EnumType = typeof(CarOwnType);
			enumcheckCarTypeOfUseOneDayReport.SelectAll();

			// Принадлежность автомобиля
			enumcheckCarOwnTypeOneDayReport.EnumType = typeof(CarTypeOfUse);
			enumcheckCarOwnTypeOneDayReport.SelectAll();

			//Выбор подразделения
			comboDepartment.SetRenderTextFunc<Subdivision>(x => x.Name);
			comboDepartment.ItemsList = UoW.GetAll<Subdivision>();
			comboDepartment.ShowSpecialStateAll = true;

			//Время отправления по умолчанию
			timeHourEntryGroupReportForOneDay.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntryGroupReportForPOneDay.Text = DateTime.Now.Minute.ToString("00.##");
		}

		private void ConfigureGroupReportForPeriod()
		{
			//Период по умолчанию
			datePickerGroupPeriodValue.StartDate = DateTime.Today;
			datePickerGroupPeriodValue.EndDate = DateTime.Today;

			// Тип автомобиля
			enumcheckCarTypeOfUseOneDayReport1.EnumType = typeof(CarOwnType);
			enumcheckCarTypeOfUseOneDayReport1.SelectAll();

			// Принадлежность автомобиля
			enumcheckCarOwnTypeOneDayReport1.EnumType = typeof(CarTypeOfUse);
			enumcheckCarOwnTypeOneDayReport1.SelectAll();

			//Выбор организации
			entryManufactures.SetEntityAutocompleteSelectorFactory(
				_organizationJournalFactory.CreateOrganizationAutocompleteSelectorFactory());

			entryManufactures.SetEntityAutocompleteSelectorFactory(_organizationJournalFactory.CreateOrganizationAutocompleteSelectorFactory());
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Путевой лист";

		#endregion

		private ReportInfo GetSingleReportInfo()
		{
			return new ReportInfo
			{
				Identifier = "Logistic.WayBillReport",
				Parameters = new Dictionary<string, object>
				{
					{ "date", datepickerSingleReport.Date },
					{ "driver_id", (entryDriverSingleReport?.Subject as Employee)?.Id ?? -1 },
					{ "car_id", (entryCarSingleReport?.Subject as Car)?.Id ?? -1 },
					{ "time", timeHourEntrySingleReport.Text + ":" + timeMinuteEntrySingleReport.Text },
					{ "need_date", !datepickerSingleReport.IsEmpty }
				}
			};
		}

		private ReportInfo GetSingleReportInfo(Dictionary<string, object> parameters)
		{
			return new ReportInfo
			{
				Identifier = "Logistic.WayBillReport",
				Parameters = parameters
			};
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetSingleReportInfo(), true));
		}
	}
}

