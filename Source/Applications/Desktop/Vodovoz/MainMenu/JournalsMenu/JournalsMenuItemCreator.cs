using System;
using Gtk;
using Vodovoz.MainMenu.JournalsMenu.Banks;
using Vodovoz.MainMenu.JournalsMenu.Counterparties;
using Vodovoz.MainMenu.JournalsMenu.Financies;
using Vodovoz.MainMenu.JournalsMenu.Helpers;
using Vodovoz.MainMenu.JournalsMenu.Logistics;
using Vodovoz.MainMenu.JournalsMenu.Orders;
using Vodovoz.MainMenu.JournalsMenu.Organization;
using Vodovoz.MainMenu.JournalsMenu.Products;
using Vodovoz.MainMenu.JournalsMenu.Transports;

namespace Vodovoz.MainMenu.JournalsMenu
{
	public class JournalsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly OrganizationMenuItemCreator _organizationMenuItemCreator;
		private readonly ProductsMenuItemCreator _productsMenuItemCreator;
		private readonly BanksMenuItemCreator _banksMenuItemCreator;
		private readonly FinancesMenuItemCreator _financesMenuItemCreator;
		private readonly CounterpartiesMenuItemCreator _counterpartiesMenuItemCreator;
		private readonly LogisticsMenuItemCreator _logisticsMenuItemCreator;
		private readonly HelpersMenuItemCreator _helpersMenuItemCreator;
		private readonly OrdersMenuItemCreator _ordersMenuItemCreator;
		private readonly TransportMenuItemCreator _transportMenuItemCreator;

		public JournalsMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			OrganizationMenuItemCreator organizationMenuItemCreator,
			ProductsMenuItemCreator productsMenuItemCreator,
			BanksMenuItemCreator banksMenuItemCreator,
			FinancesMenuItemCreator financesMenuItemCreator,
			CounterpartiesMenuItemCreator counterpartiesMenuItemCreator,
			LogisticsMenuItemCreator logisticsMenuItemCreator,
			HelpersMenuItemCreator helpersMenuItemCreator,
			OrdersMenuItemCreator ordersMenuItemCreator,
			TransportMenuItemCreator transportMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_organizationMenuItemCreator =
				organizationMenuItemCreator ?? throw new ArgumentNullException(nameof(organizationMenuItemCreator));
			_productsMenuItemCreator = productsMenuItemCreator ?? throw new ArgumentNullException(nameof(productsMenuItemCreator));
			_banksMenuItemCreator = banksMenuItemCreator ?? throw new ArgumentNullException(nameof(banksMenuItemCreator));
			_financesMenuItemCreator = financesMenuItemCreator ?? throw new ArgumentNullException(nameof(financesMenuItemCreator));
			_counterpartiesMenuItemCreator =
				counterpartiesMenuItemCreator ?? throw new ArgumentNullException(nameof(counterpartiesMenuItemCreator));
			_logisticsMenuItemCreator = logisticsMenuItemCreator ?? throw new ArgumentNullException(nameof(logisticsMenuItemCreator));
			_helpersMenuItemCreator = helpersMenuItemCreator ?? throw new ArgumentNullException(nameof(helpersMenuItemCreator));
			_ordersMenuItemCreator = ordersMenuItemCreator ?? throw new ArgumentNullException(nameof(ordersMenuItemCreator));
			_transportMenuItemCreator = transportMenuItemCreator ?? throw new ArgumentNullException(nameof(transportMenuItemCreator));
		}

		public MenuItem Create()
		{
			var journalsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Справочники");
			var journalsMenu = new Menu();
			journalsMenuItem.Submenu = journalsMenu;

			journalsMenu.Add(_organizationMenuItemCreator.Create());
			journalsMenu.Add(_productsMenuItemCreator.Create());
			journalsMenu.Add(_banksMenuItemCreator.Create());
			journalsMenu.Add(_financesMenuItemCreator.Create());
			journalsMenu.Add(_counterpartiesMenuItemCreator.Create());
			journalsMenu.Add(_logisticsMenuItemCreator.Create());
			journalsMenu.Add(_helpersMenuItemCreator.Create());
			journalsMenu.Add(_ordersMenuItemCreator.Create());
			journalsMenu.Add(_transportMenuItemCreator.Create());

			return journalsMenuItem;
		}
	}
}
