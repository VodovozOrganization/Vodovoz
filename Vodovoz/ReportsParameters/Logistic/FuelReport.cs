using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Report;
using QSProjectsLib;
using QSReport;
using Vodovoz.Core;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModel;

namespace Vodovoz.Reports
{
	public partial class FuelReport : SingleUoWWidgetBase, IParametersWidget
	{
		public FuelReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			var filterDriver = new EmployeeFilterViewModel();
			filterDriver.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.ShowFired = false
			);
			yentryreferenceDriver.RepresentationModel = new EmployeesVM(filterDriver);
			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Car, CarJournalViewModel, CarJournalFilterViewModel>(
					ServicesConfig.CommonServices,
					CriterionSearchFactory.GetMultipleEntryCriterionSearch()
				));
			entityviewmodelentryCar.CompletionPopupSetWidth(false);
			var filter = new EmployeeFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.ShowFired = false
			);
			yentryAuthor.RepresentationModel = new EmployeesVM(filter);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
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

			if(radioDriver.Active) {
				parameters.Add("car_id", -1);
				parameters.Add("driver_id", (yentryreferenceDriver.Subject as Employee)?.Id);
			}

			if(radioCar.Active) {
				parameters.Add("car_id", (entityviewmodelentryCar.Subject as Car)?.Id);
				parameters.Add("driver_id", -1);
			}

			if(radioSumm.Active) {
				parameters.Add("car_id", -1);
				parameters.Add("driver_id", -1);
				parameters.Add("author", yentryAuthor.Subject == null ? -1 : (yentryAuthor.Subject as Employee).Id);

				return new ReportInfo {
					Identifier = "Logistic.FuelReportSummary",
					UseUserVariables = true,
					Parameters = parameters
				};
			}
			 
			return new ReportInfo {
				Identifier = "Logistic.FuelReport",
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			string errorString = string.Empty;

			if(radioDriver.Active && (dateperiodpicker.StartDateOrNull == null || yentryreferenceDriver.Subject == null)) {
				errorString += "Не заполнена дата\n Не заполнен водитель\n";
			}

			if(radioCar.Active && (dateperiodpicker.StartDateOrNull == null | entityviewmodelentryCar.Subject == null)) {
				errorString += "Не заполнена дата\n Не заполнен автомобиль\n";
			}
				
			if(radioSumm.Active && dateperiodpicker.StartDateOrNull == null)
				errorString += "Не заполнена дата\n";
			if(!string.IsNullOrWhiteSpace(errorString)) {
				MessageDialogWorks.RunErrorDialog(errorString);
				return;
			}
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnRadioDriverToggled(object sender, EventArgs e)
		{
			hboxDriver.Visible = true;
			hboxCar.Visible = false;
			hboxAuthor.Visible = false;

			entityviewmodelentryCar.Subject = null;
			yentryAuthor.Subject = null;
		}

		protected void OnRadioCarToggled(object sender, EventArgs e)
		{
			hboxDriver.Visible = false;
			hboxCar.Visible = true;
			hboxAuthor.Visible = false;

			yentryAuthor.Subject = null;
			yentryreferenceDriver.Subject = null;
		}

		protected void OnRadioSummToggled(object sender, EventArgs e)
		{
			hboxDriver.Visible = false;
			hboxCar.Visible = false;
			hboxAuthor.Visible = true;

			entityviewmodelentryCar.Subject = null;
			yentryreferenceDriver.Subject = null;
		}

	}
}

