using Autofac;
using QS.Dialog.GtkUI;
using QS.Navigation;
using QS.Project.Journal;
using QS.Report;
using QS.Report.ViewModels;
using System;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.ViewModels.Accounting;
using Vodovoz.ViewModels.Dialogs.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Logistic.MileagesWriteOff;
using Vodovoz.ViewModels.ReportsParameters.Logistics;
using Vodovoz.ViewModels.TrueMark.CodesPool;
using Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis;

public partial class MainWindow
{
	private void SwitchToUI(string uiResource)
	{
		if(_lastUiId > 0)
		{
			UIManager.RemoveUi(_lastUiId);
			_lastUiId = 0;
		}
		_lastUiId = UIManager.AddUiFromResource(uiResource);
		UIManager.EnsureUpdate();
	}

	protected void OnActionOrdersToggled(object sender, EventArgs e)
	{
		if(ActionOrders.Active)
		{
			SwitchToUI("Vodovoz.toolbars.orders.xml");
		}
	}

	protected void OnActionServicesToggled(object sender, EventArgs e)
	{
		if(ActionServices.Active)
		{
			SwitchToUI("Vodovoz.toolbars.services.xml");
		}
	}

	protected void OnActionLogisticsToggled(object sender, EventArgs e)
	{
		if(ActionLogistics.Active)
		{
			SwitchToUI("logistics.xml");
		}
	}

	protected void OnActionStockToggled(object sender, EventArgs e)
	{
		if(ActionStock.Active)
		{
			SwitchToUI("warehouse.xml");
		}
	}

	protected void OnActionCRMActivated(object sender, EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.CRM.xml");
	}

	protected void OnActionGeneralActivated(object sender, EventArgs e)
	{
		SwitchToUI("general.xml");
	}

	protected void OnActionCashToggled(object sender, EventArgs e)
	{
		if(ActionCash.Active)
		{
			SwitchToUI("cash.xml");
		}
	}

	protected void OnActionAccountingToggled(object sender, EventArgs e)
	{
		if(ActionAccounting.Active)
		{
			SwitchToUI("accounting.xml");
		}
	}

	protected void OnActionRetailActivated(object sender, EventArgs e)
	{
		if(ActionRetail.Active)
		{
			SwitchToUI("retail.xml");
		}
	}


	protected void OnActionArchiveToggled(object sender, EventArgs e)
	{
		if(ActionArchive.Active)
		{
			SwitchToUI("archive.xml");
		}
	}

	protected void OnActionStaffToggled(object sender, EventArgs e)
	{
		if(ActionStaff.Active)
		{
			SwitchToUI("Vodovoz.toolbars.staff.xml");
		}
	}

	protected void OnActionSalesDepartmentAcivated(System.Object sender, System.EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.sales_department.xml");
	}

	protected void OnAction1SWorkAcivated(System.Object sender, System.EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.1s_work.xml");
	}

	protected void OnActionCarServiceAcivated(object sender, EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.car_service.xml");
	}

	protected void OnActionSuppliersActivated(object sender, EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.suppliers.xml");
	}

	protected void OnActionTrueMarkActivated(object sender, EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.true_mark.xml");
	}

	#region Логистика

	/// <summary>
	/// Журнал МЛ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ActionRouteListTable_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<RouteListJournalViewModel, Action<RouteListJournalFilterViewModel>>(null, filter =>
		{
			filter.StartDate = DateTime.Today.AddMonths(-2);
			filter.EndDate = DateTime.Today;
		});
	}

	/// <summary>
	/// На работе
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ActionAtWorks_Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenTdiTab<AtWorksDlg>(null);
	}

	/// <summary>
	/// Формирование МЛ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	void ActionRouteListsAtDay_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<RouteListsOnDayViewModel>(null);
	}

	#endregion Логистика

	protected void OnActionComplaintsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ComplaintsJournalsViewModel, Action<ComplaintFilterViewModel>>(
			null,
			filter =>
			{
				filter.StartDate = DateTime.Today.AddMonths(-2);
				filter.EndDate = DateTime.Today;
			},
			OpenPageOptions.IgnoreHash);
	}

	protected void OnOrdersRatingsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OrdersRatingsJournalViewModel>(null);
	}

	protected void OnActionCashRequestJournalActivated(object sender, EventArgs e)
	{
		var page = NavigationManager.OpenViewModel<PayoutRequestsJournalViewModel, bool, bool, Action<EmployeeFilterViewModel>>(
			null,
			false,
			false,
			employeeFilter => employeeFilter.Status = EmployeeStatus.IsWorking,
			OpenPageOptions.IgnoreHash);

		page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
	}

	protected void OnActionWayBillJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<WayBillGeneratorViewModel, Action<EmployeeFilterViewModel>>(null, filter => filter.Status = EmployeeStatus.IsWorking);
	}

	protected void OnActionRetailComplaintsJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ComplaintsJournalsViewModel, Action<ComplaintFilterViewModel>>(
			null,
			filter =>
			{
				filter.IsForRetail = true;
				filter.StartDate = DateTime.Today.AddMonths(-2);
				filter.EndDate = DateTime.Today;
			},
			OpenPageOptions.IgnoreHash);
	}

	protected void OnActionRetailUndeliveredOrdersJournalActivated(object sender, EventArgs e)
	{
		MessageDialogHelper.RunInfoDialog("Журнал недовозов");
	}

	protected void OnActionRetailCounterpartyJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RetailCounterpartyJournalViewModel, Action<CounterpartyJournalFilterViewModel>>(null, filter => filter.IsForRetail = true);
	}

	protected void OnActionRetailOrdersJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RetailOrderJournalViewModel, Action<OrderJournalFilterViewModel>>(null, filter =>
		{
			filter.IsForRetail = true;
		});
	}

	protected void ActionAnalyseCounterpartyDiscrepancies_Activated(object sender, System.EventArgs e)
	{
		NavigationManager.OpenViewModel<PaymentsDiscrepanciesAnalysisViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	#region Честный знак

	protected void OnActionCodesPoolActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CodesPoolViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnActionEdoProcessJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EdoProcessJournalViewModel>(null);
	}
	
	/// <summary>
	/// Журнал проблем документооборота с клиентами
	/// </summary>
	protected void OnActionEdoProblemJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EdoProblemJournalViewModel>(null);
	}

	#endregion Честный знак

	#region Заказы

	/// <summary>
	/// Журнал недовозов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnUndeliveredOrdersActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UndeliveredOrdersJournalViewModel, Action<UndeliveredOrdersFilterViewModel>>(
			null,
			filter =>
			{
				filter.HidenByDefault = true;
				filter.RestrictUndeliveryStatus = UndeliveryStatus.InProcess;
				filter.RestrictNotIsProblematicCases = true;
			},
			OpenPageOptions.IgnoreHash);
	}

	#endregion

	#region ТРО
	/// <summary>
	/// Пробег без МЛ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	void OnActionMileageWriteOffJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<MileageWriteOffJournalViewModel, Action<MileageWriteOffJournalFilterViewModel>>(
			   null,
			   filter =>
			   {
				   filter.WriteOffDateFrom = DateTime.Today.AddMonths(-1);
				   filter.WriteOffDateTo = DateTime.Today;
			   },
			   OpenPageOptions.IgnoreHash);
	}
	#endregion
}
