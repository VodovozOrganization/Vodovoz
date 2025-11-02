using System;
using Gtk;
using QS.Navigation;
using QS.Report.ViewModels;
using Vodovoz.Presentation.ViewModels.Logistic.Reports;
using Vodovoz.ViewModels.ReportsParameters.Logistic.CarOwnershipReport;
using Vodovoz.ViewModels.Transport.Reports.IncorrectFuel;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.Cars.ExploitationReport;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics.AverageFlowDiscrepanciesReport;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class TransportReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private MenuItem _carOwnershipReportItem;

		public TransportReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var transportMenuItem = _concreteMenuItemCreator.CreateMenuItem("Транспорт");
			var transportMenu = new Menu();
			transportMenuItem.Submenu = transportMenu;

			AddFirstSection(transportMenu);
			transportMenu.Add(CreateSeparatorMenuItem());
			AddSecondSection(transportMenu);
			transportMenu.Add(CreateSeparatorMenuItem());
			AddThirdSection(transportMenu);
			
			Configure();
			
			return transportMenuItem;
		}

		#region FirstSection

		private void AddFirstSection(Menu transportMenu)
		{
			transportMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Затраты при эксплуатации ТС", OnCostCarExploitationReportPressed));
		}

		/// <summary>
		/// Затраты при эксплуатации ТС
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCostCarExploitationReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CostCarExploitationReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		#endregion
		
		#region SecondSection

		private void AddSecondSection(Menu transportMenu)
		{
			transportMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Аналитика эксплуатации ТС", OnCarsExploitationReportPressed));
		}
		
		/// <summary>
		/// Аналитика эксплуатации ТС
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCarsExploitationReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CarsExploitationReportViewModel>(null);
		}

		#endregion

		#region ThirdSection

		private void AddThirdSection(Menu transportMenu)
		{
			transportMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по километражу", OnCarsExploitationReportPressed));
			transportMenu.Add(_concreteMenuItemCreator.CreateMenuItem(Startup.MainWin.WayBillReportAction));

			_carOwnershipReportItem = _concreteMenuItemCreator.CreateMenuItem("Отчет о принадлежности ТС", OnCarOwnershipReportActionActivated);
			transportMenu.Add(_carOwnershipReportItem);
			transportMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчёт по простою", OnCarIsNotAtLineReportPressed));
			transportMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по расходу топлива", OnAverageFlowDiscrepancyReportPressed));
			transportMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Отчет по заправкам некорректным типом топлива", OnIncorrectFuelReportPressed));
		}
		
		/// <summary>
		/// Отчет о принадлежности ТС
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCarOwnershipReportActionActivated(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(CarOwnershipReportViewModel));
		}

		/// <summary>
		/// Отчёт по простою
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCarIsNotAtLineReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<CarIsNotAtLineReportParametersViewModel>(null);
		}
		
		/// <summary>
		/// Отчет по расходу топлива
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAverageFlowDiscrepancyReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<AverageFlowDiscrepanciesReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		/// <summary>
		/// Отчет по заправкам некорректным типом топлива
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIncorrectFuelReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<IncorrectFuelReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		#endregion

		private void Configure()
		{
			_carOwnershipReportItem.Sensitive =
				Startup.MainWin.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car
					.HasAccessToCarOwnershipReport);
		}
	}
}
