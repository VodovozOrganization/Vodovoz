using System;
using QS.Navigation;
using QS.Project.Services;
using Vodovoz;
using Vodovoz.Domain.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

public partial class MainWindow
{
	private void SwitchToUI(string uiResource)
	{
		if(lastUiId > 0)
		{
			this.UIManager.RemoveUi(lastUiId);
			lastUiId = 0;
		}
		lastUiId = this.UIManager.AddUiFromResource(uiResource);
		this.UIManager.EnsureUpdate();
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
			SwitchToUI("retail.xml");
	}


	protected void OnActionArchiveToggled(object sender, EventArgs e)
	{
		if(ActionArchive.Active)
			SwitchToUI("archive.xml");
	}

	protected void OnActionStaffToggled(object sender, EventArgs e)
	{
		if(ActionStaff.Active)
			SwitchToUI("Vodovoz.toolbars.staff.xml");
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
