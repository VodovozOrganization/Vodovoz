using System;
using Gtk;
using QS.Navigation;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Fuel.FuelCards;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.Logistic.MileagesWriteOff;

namespace Vodovoz.MainMenu.JournalsMenu.Logistics
{
	/// <summary>
	/// Создатель меню Справочники - Логистика
	/// </summary>
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

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var logisticsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Логистика");
			var logisticsMenu = new Menu();
			logisticsMenuItem.Submenu = logisticsMenu;

			AddFirstSection(logisticsMenu);
			logisticsMenu.Add(CreateSeparatorMenuItem());
			AddSecondSection(logisticsMenu);
			logisticsMenu.Add(CreateSeparatorMenuItem());
			AddThirdSection(logisticsMenu);

			return logisticsMenuItem;
		}

		#region FirstSection

		private void AddFirstSection(Menu logisticsMenu)
		{
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Графики доставки", OnDeliverySchedulesPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Правила для цен доставки", OnDeliveryPriceRulesPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Тарифные зоны", OnTariffZonesPressed));
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
		
		#endregion

		#region SecondSection

		private void AddSecondSection(Menu logisticsMenu)
		{
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("График работы водителя", OnDeliveryDaySchedulesPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Смены доставки", OnDeliveryShiftsPressed));
			logisticsMenu.Add(_driverWarehouseEventsMenuItemCreator.Create());
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
		
		#endregion

		#region ThirdSection

		private void AddThirdSection(Menu logisticsMenu)
		{
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Колонки номенклатуры", OnRouteColumnsPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Причины опозданий водителей", OnLateArrivalReasonsPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Топливные карты", OnFuelCardsPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Причины списания километража", OnMileageWriteOffReasonsPressed));
		}
		
		/// <summary>
		/// Колонки номенклатуры
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRouteColumnsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RouteColumnJournalViewModel>(null, OpenPageOptions.IgnoreHash);
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
		/// Топливные карты
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFuelCardsPressed(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<FuelCardJournalViewModel>(null);
		}

		/// <summary>
		/// Причины списания километража
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnMileageWriteOffReasonsPressed(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<MileageWriteOffReasonJournalViewModel>(null);
		}
		
		#endregion
	}
}
