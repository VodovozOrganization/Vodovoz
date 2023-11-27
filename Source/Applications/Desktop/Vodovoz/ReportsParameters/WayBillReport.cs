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
using Autofac;

namespace Vodovoz.ReportsParameters
{
	public partial class WayBillReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICarJournalFactory _carJournalFactory;

		public WayBillReport(
			ILifetimeScope lifetimeScope,
			IEmployeeJournalFactory employeeJournalFactory,
			ICarJournalFactory carJournalFactory)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_carJournalFactory = carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory));

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

			entryCar.SetEntityAutocompleteSelectorFactory(_carJournalFactory.CreateCarAutocompleteSelectorFactory(_lifetimeScope));
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Путевой лист";

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo
			{
				Identifier = "Logistic.WayBillReport",
				Parameters = new Dictionary<string, object>
				{
					{ "date", datepicker.Date },
					{ "driver_id", (entryDriver?.Subject as Employee)?.Id ?? -1 },
					{ "car_id", (entryCar?.Subject as Car)?.Id ?? -1 },
					{ "time", timeHourEntry.Text + ":" + timeMinuteEntry.Text },
					{ "need_date", !datepicker.IsEmpty }
				}
			};
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}
	}
}
