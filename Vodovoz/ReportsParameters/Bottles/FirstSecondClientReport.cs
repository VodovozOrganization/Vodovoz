using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Repositories.Orders;
using Vodovoz.ViewModel;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FirstSecondClientReport : SingleUoWWidgetBase, IParametersWidget
	{
		public FirstSecondClientReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			var reasons = OrderSingletonRepository.GetInstance().GetDiscountReasons(UoW);
			yCpecCmbDiscountReason.ItemsList = reasons;
			daterangepicker.StartDate = DateTime.Now.AddDays(-7);
			daterangepicker.EndDate = DateTime.Now.AddDays(1);
			
			var filter = new EmployeeFilterViewModel();
			filter.Status = EmployeeStatus.IsWorking;
			filter.Category = EmployeeCategory.office;
			yentryEmployer.RepresentationModel = new EmployeesVM(filter);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title { get { return "Отчёт по первичным/вторичным заказам"; } }

		#endregion

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Bottles.FirstSecondClients",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", daterangepicker.StartDateOrNull },
					{ "end_date", daterangepicker.EndDateOrNull },
					{ "discount_id", (yCpecCmbDiscountReason.SelectedItem as DiscountReason)?.Id ?? 0},
					{ "show_only_client_wiht_one_order" , ycheckbutton1.Active},
					{ "author_employer_id" , (yentryEmployer.Subject as Employee)?.Id ?? 0}
				}
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}
	}
}
