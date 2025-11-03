using System;
using Autofac;
using Gtk;
using QS.Navigation;
using QS.Report.ViewModels;
using QSReport;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.ViewModels.ReportsParameters.Logistic;
using Vodovoz.ViewModels.ReportsParameters.Logistics;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics.LastRouteListReport;

namespace Vodovoz.MainMenu.ReportsMenu
{
	/// <summary>
	/// Создатель меню Отчеты - Водители
	/// </summary>
	public class DriversReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public DriversReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		///<inheritdoc/>
		public override MenuItem Create()
		{
			var driversMenuItem = _concreteMenuItemCreator.CreateMenuItem("Водители");
			var driversMenu = new Menu();
			driversMenuItem.Submenu = driversMenu;

			AddFirstSection(driversMenu);
			driversMenu.Add(CreateSeparatorMenuItem());
			AddSecondSection(driversMenu);

			return driversMenuItem;
		}

		#region FirstSection

		private void AddFirstSection(Menu driversMenu)
		{
			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по опозданиям", OnDeliveriesLatePressed));
			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по незакрытым МЛ", OnRouteListsOnClosingPressed));
		}
		
		/// <summary>
		/// Отчет по опозданиям
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDeliveriesLatePressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DeliveriesLateReportViewModel));
		}

		/// <summary>
		/// Отчет по незакрытым МЛ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRouteListsOnClosingPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(RouteListsOnClosingReportViewModel));
		}

		#endregion

		#region SecondSection
        
        private void AddSecondSection(Menu driversMenu)
        {
        	driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Реестр маршрутных листов", OnRoutesListRegisterPressed));
        	driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Время погрузки", OnOnLoadTimePressed));
        	driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Время доставки", OnDeliveryTimeReportPressed));
        	driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Загрузка наших автомобилей", OnCompanyTrucksPressed));
        	driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по отгрузке", OnShipmentReportPressed));
        	driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по километражу", OnMileageReportPressed));
        	driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по водительскому телефону", OnDriverCallsPressed));
        	driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по распределению водителей на районы",
        		OnDriversToDistrictsAssignmentReportPressed));
        	driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по последнему МЛ по водителям", OnLastRouteListReportPressed));
        }

        /// <summary>
        /// Реестр маршрутных листов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRoutesListRegisterPressed(object sender, ButtonPressEventArgs e)
        {
        	Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DriverRoutesListRegisterReportViewModel));
        }

        /// <summary>
        /// Время погрузки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOnLoadTimePressed(object sender, ButtonPressEventArgs e)
        {
        	Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OnLoadTimeAtDayReportViewModel));
        }

        /// <summary>
        /// Время доставки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeliveryTimeReportPressed(object sender, ButtonPressEventArgs e)
        {
        	Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DeliveryTimeReportViewModel));
        }

        /// <summary>
        /// Загрузка наших автомобилей
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCompanyTrucksPressed(object sender, ButtonPressEventArgs e)
        {
        	Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(CompanyTrucksReportViewModel));
        }

        /// <summary>
        /// Отчёт по отгрузке автомобилей
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShipmentReportPressed(object sender, ButtonPressEventArgs e)
        {
        	Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ShipmentReportViewModel));
        }

        /// <summary>
        /// Отчёт по километражу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMileageReportPressed(object sender, ButtonPressEventArgs e)
        {
			var dlg = Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
					null,
					options: OpenPageOptions.IgnoreHash,
					addingRegistrations: builder => builder.RegisterType<MileageReport>().As<IParametersWidget>())
				.TdiTab;

			var report = (dlg as ReportViewDlg).ParametersWidget;
			(report as MileageReport).ParentTab = dlg;
        }

        /// <summary>
        /// Отчёт по водительскому телефону
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDriverCallsPressed(object sender, ButtonPressEventArgs e)
        {
        	Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DrivingCallReportViewModel));
        }

        /// <summary>
        /// Отчет по распределению водителей на районы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDriversToDistrictsAssignmentReportPressed(object sender, ButtonPressEventArgs e)
        {
        	Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DriversToDistrictsAssignmentReportViewModel));
        }
        
		/// <summary>
		/// Отчет по последнему МЛ по водителям
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnLastRouteListReportPressed(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<LastRouteListReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}
        
        #endregion
	}
}
