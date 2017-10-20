using System;
using QSOrmProject;
using QSReport;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Reports
{
	public partial class FuelReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public FuelReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			yentryreferenceCar.SubjectType = typeof(Car);
			yentryAuthor.ItemsQuery = Repository.EmployeeRepository.OfficeWorkersQuery();
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		public object EntityObject {
			get {
				return null;
			}
		}

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title	{ 
			get {
				return "Отчет по выдаче топлива";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{ 
			var parameters = new Dictionary<string, object>();

			parameters.Add("start_date", dateperiodpicker.StartDateOrNull);
			parameters.Add("end_date", dateperiodpicker.EndDate.AddDays(1).AddTicks(-1));

			if(yentryreferenceCar.Subject != null){
				parameters.Add("car_id", (yentryreferenceCar.Subject as Car)?.Id);
				parameters.Add("driver_id", (yentryreferenceCar.Subject as Car)?.IsCompanyHavings == true
						   ? -1 : (yentryreferenceCar.Subject as Car)?.Driver?.Id);
			}
			else {
				parameters.Add("car_id", -1);
				parameters.Add("driver_id", -1);
				parameters.Add("author", yentryAuthor.Subject == null ? -1 : (yentryAuthor.Subject as Employee).Id);

				return new ReportInfo {
					Identifier = "Logistic.FuelReportSummary",
					UseUserVariables = true,
					Parameters = parameters
				};
			}

			return new ReportInfo
			{
				Identifier = "Logistic.FuelReport",
				UseUserVariables = true,
				Parameters = parameters
			};
		}	

		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		void CanRun()
		{
			buttonCreateReport.Sensitive = 
				(dateperiodpicker.EndDateOrNull != null && dateperiodpicker.StartDateOrNull != null);
		}

		protected void OnDateperiodpickerPeriodChanged(object sender, EventArgs e)
		{
			CanRun();
		}

		protected void OnYentryreferenceCarChanged(object sender, EventArgs e)
		{
			if(yentryreferenceCar.Subject != null)
			{
				yentryAuthor.Subject = null;
			}
		}

		protected void OnYentryAuthorChanged(object sender, EventArgs e)
		{
			if(yentryAuthor.Subject != null)
			{
				yentryreferenceCar.Subject = null;
			}
		}
	}
}

