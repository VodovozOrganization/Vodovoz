using Microsoft.Extensions.DependencyInjection;
using Vodovoz.MainMenu;
using Vodovoz.MainMenu.AdministrationMenu;
using Vodovoz.MainMenu.BaseMenu;
using Vodovoz.MainMenu.HelpMenu;
using Vodovoz.MainMenu.JournalsMenu;
using Vodovoz.MainMenu.JournalsMenu.Banks;
using Vodovoz.MainMenu.JournalsMenu.Counterparties;
using Vodovoz.MainMenu.JournalsMenu.Financies;
using Vodovoz.MainMenu.JournalsMenu.Helpers;
using Vodovoz.MainMenu.JournalsMenu.Logistics;
using Vodovoz.MainMenu.JournalsMenu.Orders;
using Vodovoz.MainMenu.JournalsMenu.Organization;
using Vodovoz.MainMenu.JournalsMenu.Products;
using Vodovoz.MainMenu.ProposalsMenu;
using Vodovoz.MainMenu.ReportsMenu;
using Vodovoz.MainMenu.ViewMenu;

namespace Vodovoz
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddMainMenuDependencies(this IServiceCollection services) => services
			.AddSingleton<MainMenuBarCreator>()
			.AddSingleton<ConcreteMenuItemCreator>()
			.AddSingleton<BaseMenuItemCreator>()
			.AddSingleton<ViewMenuItemCreator>()
			.AddSingleton<MainPanelMenuItemHandler>()
			.AddSingleton<TabsMenuItemHandler>()
			.AddSingleton<ThemesAppMenuItemHandler>()
			.AddSingleton<JournalsMenuItemCreator>()
			.AddSingleton<OrganizationMenuItemCreator>()
			.AddSingleton<WageMenuItemCreator>()
			.AddSingleton<ComplaintResultsMenuItemCreator>()
			.AddSingleton<ComplaintClassificationMenuItemCreator>()
			.AddSingleton<UndeliveryClassificationMenuItemCreator>()
			.AddSingleton<ProductsMenuItemCreator>()
			.AddSingleton<InventoryAccountingMenuItemCreator>()
			.AddSingleton<ExternalSourcesMenuItemCreator>()
			.AddSingleton<ExternalSourceCatalogsMenuItemCreator>()
			.AddSingleton<BanksMenuItemCreator>()
			.AddSingleton<FinancesMenuItemCreator>()
			.AddSingleton<CounterpartiesMenuItemCreator>()
			.AddSingleton<LogisticsMenuItemCreator>()
			.AddSingleton<DriverWarehouseEventsMenuItemCreator>()
			.AddSingleton<HelpersMenuItemCreator>()
			.AddSingleton<OrdersMenuItemCreator>()
			.AddSingleton<ReportsMenuItemCreator>()
			.AddSingleton<OrderReportsMenuItemCreator>()
			.AddSingleton<SalesReportsMenuItemCreator>()
			.AddSingleton<WarehouseReportsMenuItemCreator>()
			.AddSingleton<OskOkkReportsMenuItemCreator>()
			.AddSingleton<LogisticReportsMenuItemCreator>()
			.AddSingleton<EmployeesReportsMenuItemCreator>()
			.AddSingleton<DriversReportsMenuItemCreator>()
			.AddSingleton<ServiceReportsMenuItemCreator>()
			.AddSingleton<AccountingDepReportsMenuItemCreator>()
			.AddSingleton<CashRegisterDepReportsMenuItemCreator>()
			.AddSingleton<ManufacturingReportsMenuItemCreator>()
			.AddSingleton<RetailReportsMenuItemCreator>()
			.AddSingleton<TransportReportsMenuItemCreator>()
			.AddSingleton<AdministrationMenuItemCreator>()
			.AddSingleton<AdminServiceMenuItemCreator>()
			.AddSingleton<HelpMenuItemCreator>()
			.AddSingleton<ProposalsMenuItemCreator>()
		;
	}
}
