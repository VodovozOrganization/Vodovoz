using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using QS.Project.Services;
using Vodovoz.Domain.Logistic;
using QS.DomainModel.UoW;
using Vodovoz.JournalViewModels;
using Vodovoz.TempAdapters;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WayBillReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		
		public WayBillReport(IEmployeeJournalFactory employeeJournalFactory)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			
			Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			Configure();
		}

		private void Configure()
		{
			datepicker.Date = DateTime.Today;
			timeHourEntry.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntry.Text = DateTime.Now.Minute.ToString("00.##");
			
			entryDriver.SetEntityAutocompleteSelectorFactory(
				_employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());

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
					{ "time", timeHourEntry.Text + ":" + timeMinuteEntry.Text },
					{ "need_date", !datepicker.IsEmpty }
				}
			};
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}
	}
}
