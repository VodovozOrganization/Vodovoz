using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog.GtkUI;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.ReportsParameters
{
	public partial class PlanImplementationReport : Gtk.Bin, IParametersWidget
	{
		private readonly EmployeeFilterViewModel filter = new EmployeeFilterViewModel() {Status = EmployeeStatus.IsWorking};
		public PlanImplementationReport(bool orderById = false)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			dateperiodpicker.StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			dateperiodpicker.EndDate = dateperiodpicker.StartDate.AddMonths(1).AddTicks(-1);

			var availablePlansToUse = new[] { WageParameterItemTypes.SalesPlan };
			lstCmbPlanType.SetRenderTextFunc<WageParameterItemTypes>(t => t.GetEnumTitle());
			lstCmbPlanType.ItemsList = availablePlansToUse;
			lstCmbPlanType.SelectedItem = availablePlansToUse.FirstOrDefault();
			lstCmbPlanType.Changed += LstCmbPlanType_Changed;
			LstCmbPlanType_Changed(this, EventArgs.Empty);
			var employeeFactory = new EmployeeJournalFactory(filter);
			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeEmployee.ChangedByUser += (sender, e) => {
				var actualWageParameter = (evmeEmployee.Subject as Employee)?.GetActualWageParameter(DateTime.Now);
				if(actualWageParameter == null || actualWageParameter.WageParameterItem.WageParameterItemType != WageParameterItemTypes.SalesPlan) {
					return;
				}

				lblEmployeePlan.Markup = actualWageParameter.Title;
			};
			comboTypeOfDate.ItemsEnum = typeof(OrderDateType);
			comboTypeOfDate.SelectedItem = OrderDateType.CreationDate;
			buttonCreateReport.Clicked += OnButtonCreateReportClicked;
		}

		void LstCmbPlanType_Changed(object sender, EventArgs e)
		{
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictWageParameterItemType = lstCmbPlanType.SelectedItem as WageParameterItemTypes?
			);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчёт о выполнении плана";

		#endregion

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private ReportInfo GetReportInfo()
		{
			int employeeId = (evmeEmployee.Subject as Employee)?.Id ?? 0;
			return new ReportInfo {
				Identifier = employeeId > 0 ? "Sales.PlanImplementationByEmployeeReport" : "Sales.PlanImplementationFullReport",
				Parameters = new Dictionary<string, object>
				{
					{"start_date", dateperiodpicker.StartDateOrNull},
					{"end_date", dateperiodpicker.EndDateOrNull.Value.AddDays(1).AddTicks(-1)},
					{"employee_id", employeeId},
					{"is_creation_date", (OrderDateType)comboTypeOfDate.SelectedItem == OrderDateType.CreationDate}
				}
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDateOrNull == null) {
				MessageDialogHelper.RunErrorDialog("Необходимо выбрать дату");
				return;
			}
			OnUpdate(true);
		}
	}
}
