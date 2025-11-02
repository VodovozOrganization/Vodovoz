using System;
using Autofac;
using Gtk;
using QS.Dialog;
using QS.Report;
using QS.Report.ViewModels;
using Vodovoz.ViewModels.ReportsParameters.Service;

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
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(MastersReportViewModel));
		}

		/// <summary>
		/// Отчёт по оборудованию
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEquipmentReportPressed(object sender, ButtonPressEventArgs e)
		{
			var reportInfoFactory = Startup.AppDIContainer.Resolve<IReportInfoFactory>();
			var interactiveService = Startup.AppDIContainer.Resolve<IInteractiveService>();
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.EquipmentReport>(),
				() => new QSReport.ReportViewDlg(new Vodovoz.Reports.EquipmentReport(reportInfoFactory, interactiveService)));
		}

		/// <summary>
		/// Отчёт по выездам мастеров
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMastersVisitReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(MastersVisitReportViewModel));
		}
	}
}
