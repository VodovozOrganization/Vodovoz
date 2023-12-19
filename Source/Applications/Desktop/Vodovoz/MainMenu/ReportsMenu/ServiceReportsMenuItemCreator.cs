using System;
using Gtk;
using QS.Project.Services;
using Vodovoz.ReportsParameters;
using Vodovoz.TempAdapters;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class ServiceReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public ServiceReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var serviceMenuItem = _concreteMenuItemCreator.CreateMenuItem("Сервисный центр");
			var serviceMenu = new Menu();
			serviceMenuItem.Submenu = serviceMenu;
			
			serviceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по мастерам", OnMastersReportPressed));
			serviceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по оборудованию", OnEquipmentReportPressed));
			serviceMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по выездам мастеров", OnMastersVisitReportPressed));

			return serviceMenuItem;
		}
		
		/// <summary>
		/// Отчёт по мастерам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMastersReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<MastersReport>(),
				() => new QSReport.ReportViewDlg(new MastersReport(Startup.MainWin.NavigationManager)));
		}

		/// <summary>
		/// Отчёт по оборудованию
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEquipmentReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.EquipmentReport>(),
				() => new QSReport.ReportViewDlg(new Vodovoz.Reports.EquipmentReport(ServicesConfig.InteractiveService)));
		}

		/// <summary>
		/// Отчёт по выездам мастеров
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMastersVisitReportPressed(object sender, ButtonPressEventArgs e)
		{
			var employeeFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager);
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<MastersVisitReport>(),
				() => new QSReport.ReportViewDlg(new MastersVisitReport(employeeFactory)));
		}
	}
}
