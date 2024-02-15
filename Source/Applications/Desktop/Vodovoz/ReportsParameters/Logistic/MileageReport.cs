using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using QS.Widgets;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;
using Autofac;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class MileageReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICarJournalFactory _carJournalFactory;
		private readonly ILifetimeScope _lifetimeScope;

		public MileageReport(
			IEmployeeJournalFactory employeeJournalFactory,
			ICarJournalFactory carJournalFactory,
			ILifetimeScope lifetimeScope)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_carJournalFactory = carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			Build();
			Configure();
		}

		private void Configure()
		{
			var uowFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();
			UoW = uowFactory.CreateWithoutRoot();

			ConfigureEntries();

			ycheckbutton1.Toggled += (sender, args) =>
			{
				entityviewmodelentryCar.Sensitive = !ycheckbutton1.Active;
				entityviewmodelentryEmployee.Sensitive = !ycheckbutton1.Active;
				entityviewmodelentryCar.Subject = null;
				entityviewmodelentryEmployee.Subject = null;
			};

			validatedentryDifference.ValidationMode = ValidationType.Numeric;
		}

		private void ConfigureEntries()
		{
			entityviewmodelentryEmployee.SetEntityAutocompleteSelectorFactory(
				_employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(_carJournalFactory.CreateCarAutocompleteSelectorFactory(_lifetimeScope));
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по километражу";

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{ "our_cars_only", ycheckbutton1.Active },
				{ "any_status", checkAnyStatus.Active },
				{ "car_id", (entityviewmodelentryCar.Subject as Car)?.Id ?? 0 },
				{ "employee_id", (entityviewmodelentryEmployee.Subject as Employee)?.Id ?? 0 },
				{ "difference_km", validatedentryDifference.Text }
			};

			return new ReportInfo
			{
				Identifier = "Logistic.MileageReport",
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		private void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}

		private void UpdateCanRun()
		{
			buttonCreateReport.Sensitive =
				(dateperiodpicker.EndDateOrNull != null && dateperiodpicker.StartDateOrNull != null);
		}

		private void OnDateperiodpickerPeriodChanged(object sender, EventArgs e)
		{
			UpdateCanRun();
		}
	}
}
