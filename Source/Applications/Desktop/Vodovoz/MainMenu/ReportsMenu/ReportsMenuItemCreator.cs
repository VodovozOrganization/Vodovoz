using System;
using Gtk;
using QS.Project.Services;
using Vodovoz.Core.Domain.Permissions;

namespace Vodovoz.MainMenu.ReportsMenu
{
	/// <summary>
	/// Создатель меню Отчеты
	/// </summary>
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
		private readonly bool _userIsSalesRepresentative;

		private MenuItem _orderReportsMenuItem;
		private MenuItem _warehouseReportsMenuItem;
		private MenuItem _oskOkkReportsMenuItem;
		private MenuItem _logisticReportsMenuItem;
		private MenuItem _employeesReportsMenuItem;
		private MenuItem _driversReportsMenuItem;
		private MenuItem _serviceReportsMenuItem;
		private MenuItem _accountingDepReportsMenuItem;
		private MenuItem _cashRegisterDepReportsMenuItem;
		private MenuItem _manufacturingReportsMenuItem;
		private MenuItem _retailReportsMenuItem;
		private MenuItem _transportReportsMenuItem;

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

			var commonServices = ServicesConfig.CommonServices;
			_userIsSalesRepresentative = 
				commonServices.CurrentPermissionService.ValidatePresetPermission(UserPermissions.IsSalesRepresentative)
					&& !commonServices.UserService.GetCurrentUser().IsAdmin;
		}

		///<inheritdoc/>
		public override MenuItem Create()
		{
			var reportsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Отчеты");
			var reportsMenu = new Menu();
			reportsMenuItem.Submenu = reportsMenu;

			_orderReportsMenuItem = _orderReportsMenuItemCreator.Create();
			reportsMenu.Add(_orderReportsMenuItem);
			
			reportsMenu.Add(_salesReportsMenuItemCreator.Create());
			reportsMenu.Add(CreateSeparatorMenuItem());

			_warehouseReportsMenuItem = _warehouseReportsMenuItemCreator.Create();
			reportsMenu.Add(_warehouseReportsMenuItem);

			_oskOkkReportsMenuItem = _oskOkkReportsMenuItemCreator.Create();
			reportsMenu.Add(_oskOkkReportsMenuItem);

			_logisticReportsMenuItem = _logisticReportsMenuItemCreator.Create();
			reportsMenu.Add(_logisticReportsMenuItem);
			reportsMenu.Add(CreateSeparatorMenuItem());

			_employeesReportsMenuItem = _employeesReportsMenuItemCreator.Create();
			reportsMenu.Add(_employeesReportsMenuItem);

			_driversReportsMenuItem = _driversReportsMenuItemCreator.Create();
			reportsMenu.Add(_driversReportsMenuItem);
			reportsMenu.Add(CreateSeparatorMenuItem());

			_serviceReportsMenuItem = _serviceReportsMenuItemCreator.Create();
			reportsMenu.Add(_serviceReportsMenuItem);

			_accountingDepReportsMenuItem = _accountingDepReportsMenuItemCreator.Create();
			reportsMenu.Add(_accountingDepReportsMenuItem);

			_cashRegisterDepReportsMenuItem = _cashRegisterDepReportsMenuItemCreator.Create();
			reportsMenu.Add(_cashRegisterDepReportsMenuItem);

			_manufacturingReportsMenuItem = _manufacturingReportsMenuItemCreator.Create();
			reportsMenu.Add(_manufacturingReportsMenuItem);

			_retailReportsMenuItem = _retailReportsMenuItemCreator.Create();
			reportsMenu.Add(_retailReportsMenuItem);

			_transportReportsMenuItem = _transportReportsMenuItemCreator.Create();
			reportsMenu.Add(_transportReportsMenuItem);

			Configure();

			return reportsMenuItem;
		}

		private void Configure()
		{
			_orderReportsMenuItem.Visible = !_userIsSalesRepresentative;
			_warehouseReportsMenuItem.Visible = !_userIsSalesRepresentative;
			_oskOkkReportsMenuItem.Visible = !_userIsSalesRepresentative;
			_logisticReportsMenuItem.Visible = !_userIsSalesRepresentative;
			_employeesReportsMenuItem.Visible = !_userIsSalesRepresentative;
			_driversReportsMenuItem.Visible = !_userIsSalesRepresentative;
			_serviceReportsMenuItem.Visible = !_userIsSalesRepresentative;
			_accountingDepReportsMenuItem.Visible = !_userIsSalesRepresentative;
			_cashRegisterDepReportsMenuItem.Visible = !_userIsSalesRepresentative;
			_retailReportsMenuItem.Visible = !_userIsSalesRepresentative;
			_transportReportsMenuItem.Visible = !_userIsSalesRepresentative;
			_manufacturingReportsMenuItem.Visible = !_userIsSalesRepresentative;
		}
	}
}
