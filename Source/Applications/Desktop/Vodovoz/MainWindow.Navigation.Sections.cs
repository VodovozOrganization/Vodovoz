using QS.Dialog.GtkUI;
using QS.Navigation;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Journal;
using QS.Project.Services;
using System;
using Vodovoz;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.Representations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Accounting;
using Vodovoz.ViewModels.Dialogs.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

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

	protected void OnActionCarServiceAcivated(object sender, EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.car_service.xml");
	}

	protected void OnActionSuppliersActivated(object sender, EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.suppliers.xml");
	}

	protected void OnActionComplaintsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ComplaintsJournalsViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnActionCommentsForLogistsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OnecCommentsReport>(),
			() => new QSReport.ReportViewDlg(new OnecCommentsReport())
		);
	}

	protected void OpenRoutesListRegisterReport()
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.Logistic.RoutesListRegisterReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.Logistic.RoutesListRegisterReport())
		);
	}

	protected void OpenDriverRoutesListRegisterReport()
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<DriverRoutesListRegisterReport>(),
			() => new QSReport.ReportViewDlg(new DriverRoutesListRegisterReport())
		);
	}

	protected void OnActionTraineeActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			PermissionControlledRepresentationJournal.GenerateHashName<TraineeVM>(),
			() => new PermissionControlledRepresentationJournal(new TraineeVM())
		);
	}

	protected void OnActionCashRequestReportActivated(object sender, EventArgs e)
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
		NavigationManager.OpenViewModel<ComplaintsJournalViewModel, Action<ComplaintFilterViewModel>>(
			null,
			filter => filter.IsForRetail = true,
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
		var counterpartyJournalFactory = new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope());
		var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
		var employeeJournalFactory = new EmployeeJournalFactory();

		var orderJournalFilter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory, employeeJournalFactory)
		{
			IsForRetail = true
		};
		NavigationManager.OpenViewModel<RetailOrderJournalViewModel, OrderJournalFilterViewModel>(null, orderJournalFilter);
	}

	#region Заказы

	/// <summary>
	/// Журнал недовозов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnUndeliveredOrdersActionActivated(object sender, EventArgs e)
	{
		var undeliveredOrdersFilter = new UndeliveredOrdersFilterViewModel(ServicesConfig.CommonServices, new OrderSelectorFactory(),
			new EmployeeJournalFactory(), new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope()),
			new DeliveryPointJournalFactory(), new SubdivisionJournalFactory())
		{
			HidenByDefault = true,
			RestrictUndeliveryStatus = UndeliveryStatus.InProcess,
			RestrictNotIsProblematicCases = true
		};

		NavigationManager.OpenViewModel<UndeliveredOrdersJournalViewModel, UndeliveredOrdersFilterViewModel>(null, undeliveredOrdersFilter, OpenPageOptions.IgnoreHash);
	}

	#endregion
}
