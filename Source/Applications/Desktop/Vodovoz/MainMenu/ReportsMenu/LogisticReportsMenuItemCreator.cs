using System;
using Autofac;
using Gtk;
using QS.Navigation;
using QS.Report.ViewModels;
using QSReport;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.ReportsParameters.Logistics;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics.ChangingPaymentTypeByDriversReport;

namespace Vodovoz.MainMenu.ReportsMenu
{
	/// <summary>
	/// Создатель меню Отчеты - Логистика
	/// </summary>
	public class LogisticReportsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;

		public LogisticReportsMenuItemCreator(ConcreteMenuItemCreator concreteMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
		}

		///<inheritdoc/>
		public override MenuItem Create()
		{
			var logisticsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Логистика");
			var logisticsMenu = new Menu();
			logisticsMenuItem.Submenu = logisticsMenu;

			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Заказы по районам и интервалам доставки",
				OnOrdersByDistrictsAndDeliverySchedulesPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по выдаче топлива по МЛ", OnFuelConsumptionReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Отчет по времени приема заказов", OnOrdersCreationTimeReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по незакрытым МЛ за период", OnNonClosedRLByPeriodReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"График выхода на линию за смену", OnScheduleOnLinePerShiftReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Статистика по дням недели", OnOrderStatisticByWeekReportPressed));
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
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по событиям нахождения водителей на складе",
				OnDriversWarehousesEventsReportPressed));
			logisticsMenu.Add(_concreteMenuItemCreator.CreateMenuItem(
				"Отчет по изменению формы оплаты водителями",
				OnChangingFormOfPaymentByDriversReportPressed));

			return logisticsMenuItem;
		}

		/// <summary>
		/// Заказы по районам и интервалам доставки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrdersByDistrictsAndDeliverySchedulesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin
				.NavigationManager
				.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(OrdersByDistrictsAndDeliverySchedulesReportViewModel));
		}

		/// <summary>
		/// Отчет по выдаче топлива по МЛ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFuelConsumptionReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(FuelConsumptionReportViewModel));
		}

		/// <summary>
		/// Отчет по времени приема заказов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrdersCreationTimeReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<OrdersCreationTimeReport>().As<IParametersWidget>());
		}

		/// <summary>
		/// Отчет по незакрытым МЛ за период
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNonClosedRLByPeriodReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(NonClosedRLByPeriodReportViewModel));
		}

		/// <summary>
		/// График выхода на линию за смену
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnScheduleOnLinePerShiftReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ScheduleOnLinePerShiftReportViewModel));
		}

		/// <summary>
		/// Статистика по дням недели
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOrderStatisticByWeekReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder => builder.RegisterType<OrderStatisticByWeekReport>().As<IParametersWidget>());
		}

		/// <summary>
		/// Основная информация по ЗП
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLogisticsGeneralSalaryInfoPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder =>
				{
					builder.Register(c => new EmployeeFilterViewModel
					{
						Category = EmployeeCategory.driver
					});
					builder.RegisterType<GeneralSalaryInfoReport>().As<IParametersWidget>();
				});
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
			Startup.MainWin.NavigationManager.OpenTdiTab<ReportViewDlg>(
				null,
				OpenPageOptions.IgnoreHash,
				addingRegistrations: builder =>
				{
					builder.Register(c => new EmployeeFilterViewModel
					{
						RestrictCategory = EmployeeCategory.driver
					});
					builder.RegisterType<AddressesOverpaymentsReport>().As<IParametersWidget>();
				});
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
			Startup.MainWin.NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(AnalyticsForUndeliveryReportViewModel));
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
		/// Отчет по событиям нахождения водителей на складе
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDriversWarehousesEventsReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<DriversWarehousesEventsReportViewModel>(null);
		}
		
		/// <summary>
		/// Отчет по изменению формы оплаты водителями
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnChangingFormOfPaymentByDriversReportPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ChangingPaymentTypeByDriversReportViewModel>(null, OpenPageOptions.IgnoreHash);
		}
	}
}
