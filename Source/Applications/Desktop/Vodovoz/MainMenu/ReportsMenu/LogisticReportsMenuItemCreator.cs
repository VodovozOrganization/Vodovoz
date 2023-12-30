using System;
using Gtk;
using QS.Navigation;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics;

namespace Vodovoz.MainMenu.ReportsMenu
{
	public class LogisticReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public LogisticReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		public MenuItem Create()
		{
			var logisticsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Логистика");
			var logisticsMenu = new Menu();
			logisticsMenuItem.Submenu = logisticsMenu;

			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Заказы по районам и интервалам",
				OnOrdersByDistrictsAndDeliverySchedulesPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по выдаче топлива по МЛ", OnFuelConsumptionReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по времени приема заказов", OnOrdersCreationTimeReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(Startup.MainWin.WayBillReportAction));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по незакрытым МЛ за период", OnNonClosedRLByPeriodReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"График выхода на линию за смену", OnScheduleOnLinePerShiftReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Статистика по дням недели", OnOrderStatisticByWeekReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Аналитика эксплуатации ТС", OnCarsExploitationReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Основная информация по ЗП", OnLogisticsGeneralSalaryInfoPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Выгрузка по водителям", OnDriversInfoExportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по переплатам за адрес", OnAddressesOverpaymentsReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Аналитика объёмов доставки", OnDeliveryAnalyticsPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Аналитика по недовозам", OnAnalyticsForUndeliveryPressed));
			logisticsMenu.Add(
				_concreteMenuItemCreator.CreateMenuItem("Отчёт по продажам с доставкой за час", OnFastDeliverySalesReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчёт по дозагрузке МЛ", OnFastDeliveryAdditionalLoadingReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Доступность услуги \"Доставка за час\"",
				OnFastDeliveryPercentCoverageReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по событиям нахождения волителей на складе",
				OnDriversWarehousesEventsReportPressed));

			return logisticsMenuItem;
		}

		/// <summary>
		/// Заказы по районам и интервалам доставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrdersByDistrictsAndDeliverySchedulesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<OrdersByDistrictsAndDeliverySchedulesReport>(),
				() => new QSReport.ReportViewDlg(new OrdersByDistrictsAndDeliverySchedulesReport()));
		}

		/// <summary>
		/// Отчет по выдаче топлива по МЛ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFuelConsumptionReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<FuelConsumptionReport>(),
				() => new QSReport.ReportViewDlg(new FuelConsumptionReport())
			);
		}

		/// <summary>
		/// Отчет по времени приема заказов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrdersCreationTimeReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<OrdersCreationTimeReport>(),
				() => new QSReport.ReportViewDlg(new OrdersCreationTimeReport()));
		}

		/// <summary>
		/// Отчет по незакрытым МЛ за период
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNonClosedRLByPeriodReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<NonClosedRLByPeriodReport>(),
				() => new QSReport.ReportViewDlg(new NonClosedRLByPeriodReport()));
		}

		/// <summary>
		/// График выхода на линию за смену
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnScheduleOnLinePerShiftReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<FuelConsumptionReport>(),
				() => new QSReport.ReportViewDlg(new ScheduleOnLinePerShiftReport()));
		}

		/// <summary>
		/// Статистика по дням недели
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrderStatisticByWeekReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<OrderStatisticByWeekReport>(),
				() => new QSReport.ReportViewDlg(new OrderStatisticByWeekReport()));
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

		/// <summary>
		/// Основная информация по ЗП
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLogisticsGeneralSalaryInfoPressed(object sender, ButtonPressEventArgs e)
		{
			var filter = new EmployeeFilterViewModel
			{
				Category = EmployeeCategory.driver
			};

			var employeeJournalFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager, filter);

			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<GeneralSalaryInfoReport>(),
				() => new QSReport.ReportViewDlg(new GeneralSalaryInfoReport(employeeJournalFactory, ServicesConfig.InteractiveService)));
		}

		/// <summary>
		/// Выгрузка по водителям
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDriversInfoExportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<DriversInfoExportViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Отчет по переплатам за адрес
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAddressesOverpaymentsReportPressed(object sender, ButtonPressEventArgs e)
		{
			var driverFilter = new EmployeeFilterViewModel { RestrictCategory = EmployeeCategory.driver };
			var employeeJournalFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager, driverFilter);

			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<AddressesOverpaymentsReport>(),
				() => new QSReport.ReportViewDlg(new AddressesOverpaymentsReport(
					employeeJournalFactory,
					ServicesConfig.InteractiveService)));
		}

		/// <summary>
		/// Аналитика объёмов доставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDeliveryAnalyticsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<DeliveryAnalyticsViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Аналитика по недовозам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAnalyticsForUndeliveryPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName<AnalyticsForUndeliveryReport>(),
				() => new QSReport.ReportViewDlg(new AnalyticsForUndeliveryReport()));
		}

		/// <summary>
		/// Отчёт по продажам с доставкой за час
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFastDeliverySalesReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<FastDeliverySalesReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Отчёт по дозагрузке МЛ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFastDeliveryAdditionalLoadingReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<FastDeliveryAdditionalLoadingReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Доступность услуги "Доставка за час"
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFastDeliveryPercentCoverageReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<FastDeliveryPercentCoverageReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Отчет по событиям нахождения волителей на складе
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDriversWarehousesEventsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<DriversWarehousesEventsReportViewModel>(null);
		}
	}
}
