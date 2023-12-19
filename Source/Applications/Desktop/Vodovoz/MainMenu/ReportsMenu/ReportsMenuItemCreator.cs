using System;
using Gtk;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class ReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly OrderReportsMenuItemCreator _orderReportsMenuItemCreator;
		private readonly SalesReportsMenuItemCreator _salesReportsMenuItemCreator;
		private readonly WarehouseReportsMenuItemCreator _warehouseReportsMenuItemCreator;
		private readonly OskOkkReportsMenuItemCreator _oskOkkReportsMenuItemCreator;
		private readonly LogisticReportsMenuItemCreator _logisticReportsMenuItemCreator;
		private readonly EmployeesReportsMenuItemCreator _employeesReportsMenuItemCreator;
		private readonly DriversReportsMenuItemCreator _driversReportsMenuItemCreator;
		private readonly ServiceReportsMenuItemCreator _serviceReportsMenuItemCreator;
		private readonly AccountingDepReportsMenuItemCreator _accountingDepReportsMenuItemCreator;
		private readonly CashRegisterDepReportsMenuItemCreator _cashRegisterDepReportsMenuItemCreator;
		private readonly ManufacturingReportsMenuItemCreator _manufacturingReportsMenuItemCreator;
		private readonly RetailReportsMenuItemCreator _retailReportsMenuItemCreator;
		private readonly TransportReportsMenuItemCreator _transportReportsMenuItemCreator;

		public ReportsMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			OrderReportsMenuItemCreator orderReportsMenuItemCreator,
			SalesReportsMenuItemCreator salesReportsMenuItemCreator,
			WarehouseReportsMenuItemCreator warehouseReportsMenuItemCreator,
			OskOkkReportsMenuItemCreator oskOkkReportsMenuItemCreator,
			LogisticReportsMenuItemCreator logisticReportsMenuItemCreator,
			EmployeesReportsMenuItemCreator employeesReportsMenuItemCreator,
			DriversReportsMenuItemCreator driversReportsMenuItemCreator,
			ServiceReportsMenuItemCreator serviceReportsMenuItemCreator,
			AccountingDepReportsMenuItemCreator accountingDepReportsMenuItemCreator,
			CashRegisterDepReportsMenuItemCreator cashRegisterDepReportsMenuItemCreator,
			ManufacturingReportsMenuItemCreator manufacturingReportsMenuItemCreator,
			RetailReportsMenuItemCreator retailReportsMenuItemCreator,
			TransportReportsMenuItemCreator transportReportsMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator;
			_orderReportsMenuItemCreator =
				orderReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(orderReportsMenuItemCreator));
			_salesReportsMenuItemCreator =
				salesReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(salesReportsMenuItemCreator));
			_warehouseReportsMenuItemCreator =
				warehouseReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(warehouseReportsMenuItemCreator));
			_oskOkkReportsMenuItemCreator =
				oskOkkReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(oskOkkReportsMenuItemCreator));
			_logisticReportsMenuItemCreator =
				logisticReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(logisticReportsMenuItemCreator));
			_employeesReportsMenuItemCreator =
				employeesReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(employeesReportsMenuItemCreator));
			_driversReportsMenuItemCreator =
				driversReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(driversReportsMenuItemCreator));
			_serviceReportsMenuItemCreator =
				serviceReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(serviceReportsMenuItemCreator));
			_accountingDepReportsMenuItemCreator =
				accountingDepReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(accountingDepReportsMenuItemCreator));
			_cashRegisterDepReportsMenuItemCreator =
				cashRegisterDepReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(cashRegisterDepReportsMenuItemCreator));
			_manufacturingReportsMenuItemCreator =
				manufacturingReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(manufacturingReportsMenuItemCreator));
			_retailReportsMenuItemCreator =
				retailReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(retailReportsMenuItemCreator));
			_transportReportsMenuItemCreator =
				transportReportsMenuItemCreator ?? throw new ArgumentNullException(nameof(transportReportsMenuItemCreator));
		}

		public MenuItem Create()
		{
			var reportsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Отчеты");
			var reportsMenu = new Menu();
			reportsMenuItem.Submenu = reportsMenu;

			reportsMenu.Add(_orderReportsMenuItemCreator.Create());
			reportsMenu.Add(_salesReportsMenuItemCreator.Create());
			reportsMenu.Add(CreateSeparatorMenuItem());
			reportsMenu.Add(_warehouseReportsMenuItemCreator.Create());
			reportsMenu.Add(_oskOkkReportsMenuItemCreator.Create());
			reportsMenu.Add(_logisticReportsMenuItemCreator.Create());
			reportsMenu.Add(CreateSeparatorMenuItem());
			reportsMenu.Add(_employeesReportsMenuItemCreator.Create());
			reportsMenu.Add(_driversReportsMenuItemCreator.Create());
			reportsMenu.Add(CreateSeparatorMenuItem());
			reportsMenu.Add(_serviceReportsMenuItemCreator.Create());
			reportsMenu.Add(_accountingDepReportsMenuItemCreator.Create());
			reportsMenu.Add(_cashRegisterDepReportsMenuItemCreator.Create());
			reportsMenu.Add(_manufacturingReportsMenuItemCreator.Create());
			reportsMenu.Add(_retailReportsMenuItemCreator.Create());
			reportsMenu.Add(_transportReportsMenuItemCreator.Create());

			return reportsMenuItem;
		}
	}
}
