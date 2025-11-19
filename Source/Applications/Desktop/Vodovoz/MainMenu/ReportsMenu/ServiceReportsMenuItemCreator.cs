using System;
using Autofac;
using Gtk;
using QS.Navigation;
using QS.Report.ViewModels;
using QSReport;
using Vodovoz.Reports;
using Vodovoz.ViewModels.ReportsParameters.Service;

namespace Vodovoz.MainMenu.ReportsMenu
{
	/// <summary>
	/// Создатель меню Отчеты - Сервисный центр
	/// </summary>
	public class ServiceReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public ServiceReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		///<inheritdoc/>
		public override MenuItem Create()
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
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<EquipmentReport>().As<IParametersWidget>());
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
