using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Employees;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Filters.ViewModels;
using QS.Project.Services;
using Vodovoz.Domain.Logistic;
using QS.DomainModel.UoW;
using Vodovoz.JournalViewModels;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WayBillReport : SingleUoWWidgetBase, IParametersWidget
	{
		public WayBillReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			Configure();
		}

		private void Configure()
		{
			datepicker.Date = DateTime.Today;
			timeHourEntry.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntry.Text = DateTime.Now.Minute.ToString("00.##");
			
			entryDriver.SetEntityAutocompleteSelectorFactory(new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee), () => {
				var filter = new EmployeeFilterViewModel();
				filter.Status = EmployeeStatus.IsWorking;
				filter.Category = EmployeeCategory.driver;
				return new EmployeesJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			}));

			entryCar.SetEntityAutocompleteSelectorFactory(new DefaultEntityAutocompleteSelectorFactory
				<Car, CarJournalViewModel, CarJournalFilterViewModel>(ServicesConfig.CommonServices));
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Путевой лист";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Logistic.WayBillReport",
				Parameters = new Dictionary<string, object>
				{
					{ "date", datepicker.Date },
					{ "driver_id", (entryDriver?.Subject  as Employee)?.Id ?? -1 },
					{ "car_id", (entryCar?.Subject as Car)?.Id ?? -1 },
					{ "time", timeHourEntry.Text + ":" + timeMinuteEntry.Text }
					
				}
			};
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			string errorString = string.Empty;
			if(datepicker.Date == DateTime.MinValue)
				errorString += "Не заполнена дата\n";
			if(!string.IsNullOrWhiteSpace(errorString)) {
				MessageDialogHelper.RunErrorDialog(errorString);
				return;
			}
			OnUpdate(true);
		}
	}
}
