using System;
using Gtk;
using QS.Navigation;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;

namespace Vodovoz.MainMenu.JournalsMenu.Logistics
{
	public class LogisticsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly DriverWarehouseEventsMenuItemCreator _driverWarehouseEventsMenuItemCreator;

		public LogisticsMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			DriverWarehouseEventsMenuItemCreator driverWarehouseEventsMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_driverWarehouseEventsMenuItemCreator =
				driverWarehouseEventsMenuItemCreator ?? throw new ArgumentNullException(nameof(driverWarehouseEventsMenuItemCreator));
		}

		public MenuItem Create()
		{
			var logisticsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Логистика");
			var logisticsMenu = new Menu();
			logisticsMenuItem.Submenu = logisticsMenu;

			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Графики доставки", OnDeliverySchedulesPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Правила для цен доставки", OnDeliveryPriceRulesPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Тарифные зоны", OnTariffZonesPressed));
			logisticsMenu.Add(CreateSeparatorMenuItem());
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("График работы водителя", OnDeliveryDaySchedulesPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Смены доставки", OnDeliveryShiftsPressed));
			logisticsMenu.Add(_driverWarehouseEventsMenuItemCreator.Create());
			logisticsMenu.Add(CreateSeparatorMenuItem());
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(Startup.MainWin.CarsJournalAction));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Виды топлива", OnFuelTypesPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Модели автомобилей", OnCarModelsPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Производители автомобилей", OnCarManufacturersPressed));
			logisticsMenu.Add(CreateSeparatorMenuItem());
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Колонки номенклатуры", OnRouteColumnsPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Причины опозданий водителей", OnLateArrivalReasonsPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Виды событий ТС", OnCarEventTypesPressed));

			return logisticsMenuItem;
		}
		
		/// <summary>
		/// Графики доставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDeliverySchedulesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<DeliveryScheduleJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Правила для цен доставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDeliveryPriceRulesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<DeliveryPriceRuleJournalViewModel>(null);
		}

		/// <summary>
		/// Тарифные зоны
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTariffZonesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<TariffZoneJournalViewModel>(null);
		}

		/// <summary>
		/// График работы водителя
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnDeliveryDaySchedulesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				OrmReference.GenerateHashName<DeliveryDaySchedule>(),
				() => new OrmReference(typeof(DeliveryDaySchedule))
			);
		}

		/// <summary>
		/// Смена доставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnDeliveryShiftsPressed(object sender, ButtonPressEventArgs e)
		{
			var refWin = new OrmReference(typeof(DeliveryShift));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Виды топлива
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFuelTypesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<FuelTypeJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Модели автомобилей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCarModelsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CarModelJournalViewModel>(null);
		}

		/// <summary>
		/// Производители автомобилей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCarManufacturersPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CarManufacturerJournalViewModel>(null);
		}

		/// <summary>
		/// Колонки номенклатуры
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnRouteColumnsPressed(object sender, ButtonPressEventArgs e)
		{
			var refWin = new OrmReference(typeof(RouteColumn));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Причины опозданий водителей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLateArrivalReasonsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<LateArrivalReasonsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Виды событий ТС
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCarEventTypesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CarEventTypeJournalViewModel>(null);
		}
	}
}
