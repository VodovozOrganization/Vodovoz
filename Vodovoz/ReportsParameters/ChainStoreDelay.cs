using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ChainStoreDelayReport : SingleUoWWidgetBase, IParametersWidget
	{
		public ChainStoreDelayReport()
		{
			this.Build();
			
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ydatepicker.Date = DateTime.Now.Date;
			entityviewmodelentryCounterparty.SetEntityAutocompleteSelectorFactory(new DefaultEntityAutocompleteSelectorFactory<Counterparty,
				CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices));
			entityviewmodelentrySellManager.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
					() =>
					{
						var employeeFilter = new EmployeeFilterViewModel{
							Status = EmployeeStatus.IsWorking,
							Category = EmployeeCategory.office
						};
						return new EmployeesJournalViewModel(employeeFilter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
					})
			);
			
			entityviewmodelentryOrderAuthor.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
					() =>
					{
						var employeeFilter = new EmployeeFilterViewModel{
							Status = EmployeeStatus.IsWorking,
							Category = EmployeeCategory.office
						};
						return new EmployeesJournalViewModel(employeeFilter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
					})
			);
			ydatepicker.Date = DateTime.Now;
			buttonRun.Sensitive = true;
			buttonRun.Clicked += OnButtonCreateReportClicked;
		}

		// buttonRun.Sensitive = daterangepicker.EndDateOrNull.HasValue && daterangepicker.StartDateOrNull.HasValue;
		
		void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			OnUpdate (true);
		}

		#region IParametersWidget implementation

		public string Title => "Отсрочка сети";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		private ReportInfo GetReportInfo()
		{
			var ctrptyId = (entityviewmodelentryCounterparty.Subject as Counterparty)?.Id ?? -1;
			var ctrptyId2 = (entityviewmodelentrySellManager.Subject as Employee)?.Id ?? -1;
			var ctrptyId3 = (entityviewmodelentryOrderAuthor.Subject as Employee)?.Id ?? -1;
			
			return new ReportInfo {
				Identifier = "Payments.PaymentsDelayNetwork",
				Parameters = new Dictionary<string, object> {
					{ "date", ydatepicker.Date },
					{ "counterparty_id", (entityviewmodelentryCounterparty.Subject as Counterparty)?.Id ?? -1},
					{ "sell_manager_id", (entityviewmodelentrySellManager.Subject as Employee)?.Id ?? -1},
					{ "order_author_id", (entityviewmodelentryOrderAuthor.Subject as Employee)?.Id ?? -1}
				}
			};
		}

		void OnUpdate(bool hide = false) => LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
	}
}
