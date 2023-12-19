using System;
using Autofac;
using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Logistic;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class DriversReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public DriversReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var driversMenuItem = _concreteMenuItemCreator.CreateMenuItem("Сотрудники");
			var driversMenu = new Menu();
			driversMenuItem.Submenu = driversMenu;

			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по опозданиям", OnDeliveriesLatePressed));
			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по незакрытым МЛ", OnRouteListsOnClosingPressed));
			driversMenu.Add(CreateSeparatorMenuItem());
			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Реестр маршрутных листов", OnRoutesListRegisterPressed));
			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Время погрузки", OnOnLoadTimePressed));
			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Время доставки", OnDeliveryTimeReportPressed));
			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Загрузка наших автомобилей", OnCompanyTrucksPressed));
			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по отгрузке", OnShipmentReportPressed));
			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по километражу", OnMileageReportPressed));
			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по водительскому телефону", OnDriverCallsPressed));
			driversMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по распределению водителей на районы",
				OnDriversToDistrictsAssignmentReportPressed));

			return driversMenuItem;
		}

		/// <summary>
		/// Отчет по опозданиям
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDeliveriesLatePressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.Logistic.DeliveriesLateReport>(),
				() => new QSReport.ReportViewDlg(new Vodovoz.Reports.Logistic.DeliveriesLateReport()));
		}

		/// <summary>
		/// Отчет по незакрытым МЛ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnRouteListsOnClosingPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<RouteListsOnClosingReport>(),
				() => new QSReport.ReportViewDlg(new RouteListsOnClosingReport()));
		}

		/// <summary>
		/// Реестр маршрутных листов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnRoutesListRegisterPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<DriverRoutesListRegisterReport>(),
				() => new QSReport.ReportViewDlg(new DriverRoutesListRegisterReport())
			);
		}

		/// <summary>
		/// Время погрузки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnOnLoadTimePressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<OnLoadTimeAtDayReport>(),
				() => new QSReport.ReportViewDlg(new OnLoadTimeAtDayReport()));
		}

		/// <summary>
		/// Время доставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDeliveryTimeReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(QSReport.ReportViewDlg.GenerateHashName<DeliveryTimeReport>(),
				() => new QSReport.ReportViewDlg(
					new DeliveryTimeReport(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.InteractiveService)));
		}

		/// <summary>
		/// Загрузка наших автомобилей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnCompanyTrucksPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<CompanyTrucksReport>(),
				() => new QSReport.ReportViewDlg(new CompanyTrucksReport()));
		}

		/// <summary>
		/// Отчёт по отгрузке автомобилей
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnShipmentReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<ShipmentReport>(),
				() => new QSReport.ReportViewDlg(new ShipmentReport()));
		}

		/// <summary>
		/// Отчёт по километражу
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnMileageReportPressed(object sender, ButtonPressEventArgs e)
		{
			var scope = Startup.AppDIContainer.BeginLifetimeScope();

			var report = scope.Resolve<MileageReport>();

			var tab = Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<MileageReport>(),
				() => new QSReport.ReportViewDlg(report));

			report.Destroyed += (_, _2) => scope?.Dispose();
		}

		/// <summary>
		/// Отчёт по водительскому телефону
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDriverCallsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<DrivingCallReport>(),
				() => new QSReport.ReportViewDlg(new DrivingCallReport()));
		}

		/// <summary>
		/// Отчет по распределению водителей на районы
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDriversToDistrictsAssignmentReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<DriversToDistrictsAssignmentReport>(),
				() => new QSReport.ReportViewDlg(new DriversToDistrictsAssignmentReport()));
		}
	}
}
