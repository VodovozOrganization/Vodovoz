using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.TempAdapters;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FirstSecondClientReport : SingleUoWWidgetBase, IParametersWidget
	{
		public FirstSecondClientReport(INavigationManager navigationManager, IDiscountReasonRepository discountReasonRepository)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			if(discountReasonRepository == null)
			{
				throw new ArgumentNullException(nameof(discountReasonRepository));
			}
			
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			var reasons = discountReasonRepository.GetDiscountReasons(UoW);
			yCpecCmbDiscountReason.ItemsList = reasons;
			daterangepicker.StartDate = DateTime.Now.AddDays(-7);
			daterangepicker.EndDate = DateTime.Now.AddDays(1);

			var employeeFactory = new EmployeeJournalFactory(navigationManager);
			evmeAuthor.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory());
			buttonCreateReport.Clicked += (sender, e) => OnUpdate(true);
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчёт по первичным/вторичным заказам";

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
					{ "discount_id", (yCpecCmbDiscountReason.SelectedItem as DiscountReason)?.Id ?? 0 },
					{ "show_only_client_with_one_order", ycheckbutton1.Active },
					{ "author_employer_id", (evmeAuthor.Subject as Employee)?.Id ?? 0 },
					{ "has_promo_set", chkHasPromoSet.Active }
				}
			};
		}
	}
}
