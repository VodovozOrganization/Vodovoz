using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Widgets;
using QS.Widgets.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModel;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MileageReport : SingleUoWWidgetBase, IParametersWidget
	{
		public MileageReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			entityviewmodelentryEmployee.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
					() =>
					{
						var employeeFilter = new EmployeeFilterViewModel{
							Status = EmployeeStatus.IsWorking,
							Category = EmployeeCategory.driver
						};
						return new EmployeesJournalViewModel(employeeFilter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
					})
				);
			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Car, CarJournalViewModel, CarJournalFilterViewModel>(ServicesConfig.CommonServices));

			ycheckbutton1.Toggled += (sender, args) =>
			{
				entityviewmodelentryCar.Sensitive = !ycheckbutton1.Active;
				entityviewmodelentryEmployee.Sensitive = !ycheckbutton1.Active;
				entityviewmodelentryCar.Subject = null;
				entityviewmodelentryEmployee.Subject = null;
			};

			validatedentryDifference.ValidationMode = ValidationType.Numeric;
			
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по километражу";
			}
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();

			parameters.Add("start_date", dateperiodpicker.StartDateOrNull);
			parameters.Add("end_date", dateperiodpicker.EndDateOrNull);
			parameters.Add("our_cars_only", ycheckbutton1.Active);
			parameters.Add("car_id", (entityviewmodelentryCar.Subject as Car)?.Id ?? 0);
			parameters.Add("employee_id", (entityviewmodelentryEmployee.Subject as Employee)?.Id ?? 0);
			
			int temp = 0;
			if (!String.IsNullOrEmpty(validatedentryDifference.Text) && validatedentryDifference.Text.All(char.IsDigit))
			{
				temp = int.Parse(validatedentryDifference.Text);
			}
			parameters.Add("difference_km", validatedentryDifference.Text);
			
			return new ReportInfo {
				Identifier = "Logistic.MileageReport",
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
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

		#endregion
	}
}