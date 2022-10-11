using System;
using Vodovoz.Domain.Orders;
using Gtk;
using QSWidgetLib;
using System.Globalization;

namespace Vodovoz
{
	//FIXME Удалить если не будет использоваться совсем. Просто сделал, но зря, рука не поднимается удалить.
	public static class OrderPopupMenu
	{
		public static Gtk.Menu GetPopupMenu(Order[] selected)
		{
			Menu popupMenu = new Gtk.Menu();
			MenuItemId<Order[]> menuItemYandex = new MenuItemId<Order[]>("Открыть на Yandex картах(координаты)");
			menuItemYandex.Activated += MenuItemYandex_Activated; 
			menuItemYandex.ID = selected;
			popupMenu.Add(menuItemYandex);
			MenuItemId<Order[]> menuItemYandexAddress = new MenuItemId<Order[]>("Открыть на Yandex картах(адрес)");
			menuItemYandexAddress.Activated += MenuItemYandexAddress_Activated;
			menuItemYandexAddress.ID = selected;
			popupMenu.Add(menuItemYandexAddress);
			MenuItemId<Order[]> menuItemOSM = new MenuItemId<Order[]>("Открыть на карте OSM");
			menuItemOSM.Activated += MenuItemOSM_Activated;
			menuItemOSM.ID = selected;
			popupMenu.Add(menuItemOSM);
			return popupMenu;
		}

		static void MenuItemOSM_Activated (object sender, EventArgs e)
		{
			foreach(var order in (sender as MenuItemId<Order[]>).ID)
			{
				if (order.DeliveryPoint == null || order.DeliveryPoint.Latitude == null || order.DeliveryPoint.Longitude == null)
					continue;

				System.Diagnostics.Process.Start(String.Format(CultureInfo.InvariantCulture, "http://www.openstreetmap.org/#map=17/{1}/{0}", order.DeliveryPoint.Longitude, order.DeliveryPoint.Latitude));
			}
		}

		static void MenuItemYandexAddress_Activated (object sender, EventArgs e)
		{
			foreach(var order in (sender as MenuItemId<Order[]>).ID)
			{
				if (order.DeliveryPoint == null)
					continue;

				System.Diagnostics.Process.Start(
					String.Format(CultureInfo.InvariantCulture, 
						"https://maps.yandex.ru/?text={0} {1} {2}", 
						order.DeliveryPoint.City,
						order.DeliveryPoint.Street,
						order.DeliveryPoint.Building
					));
			}
		}

		static void MenuItemYandex_Activated (object sender, EventArgs e)
		{
			foreach(var order in (sender as MenuItemId<Order[]>).ID)
			{
				if (order.DeliveryPoint == null || order.DeliveryPoint.Latitude == null || order.DeliveryPoint.Longitude == null)
					continue;

				System.Diagnostics.Process.Start(String.Format(CultureInfo.InvariantCulture, "https://maps.yandex.ru/?ll={0},{1}&z=17", order.DeliveryPoint.Longitude, order.DeliveryPoint.Latitude));
			}
		} 
	}
}