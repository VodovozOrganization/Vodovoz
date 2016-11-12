using System;
using System.Collections.Generic;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz
{
	public partial class RoutesAtDayDlg : TdiTabBase
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private readonly GMapOverlay addressesOverlay = new GMapOverlay("addresses");
		IList<Order> ordersAtDay;
		IList<RouteList> routesAtDay;
		int addressesWithoutCoordinats;

		public override string TabName
		{
			get
			{
				return String.Format("Маршруты на {0:d}", ydateForRoutes.Date);
			}
			protected set
			{
				throw new InvalidOperationException("Установка протеворечит логике работы.");
			}
		}

		public RoutesAtDayDlg()
		{
			this.Build();

			//Configure map
			gmapWidget.MapProvider = GMapProviders.OpenStreetMap;
			gmapWidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapWidget.HeightRequest = 150;
			gmapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(addressesOverlay);

			yenumcomboMapType.ItemsEnum = typeof(MapProviders);

			ydateForRoutes.Date = DateTime.Today;
		}

		void FillDialogAtDay()
		{
			logger.Info("Загружаем заказы на {0:d}...", ydateForRoutes.Date);
			ordersAtDay = Repository.OrderRepository.GetAcceptedOrdersForDateQuery(ydateForRoutes.Date)
				.GetExecutableQueryOver(uow.Session)
				.Fetch(x => x.DeliveryPoint).Eager
				.Fetch(x => x.OrderItems).Eager
				.List();

			routesAtDay = Repository.Logistics.RouteListRepository.GetRoutesAtDay(ydateForRoutes.Date)
				.GetExecutableQueryOver(uow.Session)
				.Fetch(x => x.Addresses).Eager
				.List();

			UpdateAddressesOnMap();
		}

		void UpdateAddressesOnMap()
		{
			logger.Info("Обновляем адреса на карте...");
			addressesWithoutCoordinats = 0;
			addressesOverlay.Clear();

			foreach(var order in ordersAtDay)
			{
				if (order.DeliveryPoint.Latitude.HasValue && order.DeliveryPoint.Longitude.HasValue)
				{
					GMarkerGoogleType type;
					type = GMarkerGoogleType.gray_small;
					var addressMarker = new GMarkerGoogle(new PointLatLng((double)order.DeliveryPoint.Latitude, (double)order.DeliveryPoint.Longitude),	type);
					addressMarker.ToolTipText = String.Format("{0}\nБутылей: {1}",
						order.DeliveryPoint.ShortAddress,
						order.TotalDeliveredBottles
					);
					addressesOverlay.Markers.Add(addressMarker);
				}
				else
					addressesWithoutCoordinats++;
			}
			UpdateOrdersInfo();
			logger.Info("Ок.");
		}

		protected void OnYdateForRoutesDateChanged(object sender, EventArgs e)
		{
			FillDialogAtDay();
			OnTabNameChanged();
		}

		protected void OnYenumcomboMapTypeChangedByUser(object sender, EventArgs e)
		{
			gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)yenumcomboMapType.SelectedItem);
		}

		void UpdateOrdersInfo()
		{
			var text = new List<string>();
			text.Add(RusNumber.FormatCase(ordersAtDay.Count, "На день {0} заказ", "На день {0} заказа", "На день {0} заказов"));
			if (addressesWithoutCoordinats > 0)
				text.Add(String.Format("Из них {0} без координат", addressesWithoutCoordinats));
			text.Add(RusNumber.FormatCase(routesAtDay.Count, "Всего {0} маршрутный лист", "Всего {0} маршрутных листа", "Всего {0} маршрутных листов") );

			textOrdersInfo.Buffer.Text = String.Join("\n", text);
		}
	}
}

