using Gamma.Utilities;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QS.ViewModels.Widgets;
using QSReport;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.Reports
{
	public partial class SalesReport : SingleUoWWidgetBase, IParametersWidget
	{
		private IncludeExludeFiltersViewModel _filterViewModel;
		private LeftRightListViewModel<GroupingNode> _groupViewModel;
		private readonly bool _userIsSalesRepresentative;
		private readonly IIncludeExcludeSalesFilterFactory _includeExcludeSalesFilterFactory;
		private readonly ILeftRightListViewModelFactory _leftRightListViewModelFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IInteractiveService _interactiveService;

		private readonly bool _canSeePhones;

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public string Title => "Отчет по продажам";

		public SalesReport(
			IEmployeeRepository employeeRepository,
			IInteractiveService interactiveService,
			IIncludeExcludeSalesFilterFactory includeExcludeSalesFilterFactory,
			ILeftRightListViewModelFactory leftRightListViewModelFactory)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_includeExcludeSalesFilterFactory = includeExcludeSalesFilterFactory ?? throw new ArgumentNullException(nameof(includeExcludeSalesFilterFactory));
			_leftRightListViewModelFactory = leftRightListViewModelFactory ?? throw new ArgumentNullException(nameof(leftRightListViewModelFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

			Build();

			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			UoW.Session.DefaultReadOnly = true;

			_userIsSalesRepresentative =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Permissions.User.IsSalesRepresentative)
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;

			_canSeePhones = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Permissions.Report.SalesReport.CanGenerateDetailedReportWithPhones);

			ConfigureDlg();
		}

		public virtual LeftRightListViewModel<GroupingNode> GroupingSelectViewModel => _groupViewModel;

		public IncludeExludeFiltersViewModel FilterViewModel => _filterViewModel;

		private void ConfigureDlg()
		{
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;
			buttonInfo.Clicked += (sender, args) => ShowInfoWindow();

			ycheckbuttonDetail.Toggled += (sender, args) =>
			{
				ycheckbuttonPhones.Sensitive = _canSeePhones && ycheckbuttonDetail.Active;
			};

			SetupFilter();

			SetupGroupings();

			leftrightlistview.ViewModel = GroupingSelectViewModel;
		}

		private void SetupGroupings()
		{
			_groupViewModel = _leftRightListViewModelFactory.CreateSalesReportGroupingsConstructor();
		}

		private void ShowInfoWindow()
		{
			var info =
				"<b>1.</b> Подсчет продаж ведется на основе заказов. В отчете учитываются заказы со статусами:" +
				$"\n\t'{OrderStatus.Accepted.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.InTravelList.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.OnLoading.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.OnTheWay.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.Shipped.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.UnloadingOnStock.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.Closed.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.WaitForPayment.GetEnumTitle()}' и заказ - самовывоз с оплатой после отгрузки." +
				"\nВ отчет <b>не попадают</b> заказы, являющиеся закрывашками по контракту." +
				"\nФильтр по дате отсекает заказы, если дата доставки не входит в выбранный период." +

				"\n\n<b>2.</b> Подсчет тары ведется следующим образом:" +
				"\n\tПлановое значение - сумма бутылей на возврат попавших в отчет заказов;" +
				"\n\tФактическое значение - сумма фактически возвращенных бутылей по адресам маршрутного листа." +
				"\n\t\tФактическое значение возвращенных бутылей по адресу зависит от того, доставлен<b>(*)</b> заказ или нет:" +
				"\n\t\t\t <b>-</b> Если да - берется кол-во бутылей, которое по факту забрал водитель. " +
				"Это кол-во может быть вручную указано при закрытии МЛ;" +

				"\n\t\t\t <b>-</b> Если не доставлен - берется кол-во бутылей на возврат из заказа;" +
				"\n\t\t\t <b>-</b> Если заказ является самовывозом - берется значение возвращенной тары, указанное в отпуске самовывоза;" +
				$"\n\t\t <b>*</b> Заказ считается доставленным, если его статус в МЛ: '{RouteListItemStatus.Completed.GetEnumTitle()}' или " +
				$"'{RouteListItemStatus.EnRoute.GetEnumTitle()}' и статус МЛ '{RouteListStatus.Closed.GetEnumTitle()}' " +
				$"или '{RouteListStatus.OnClosing.GetEnumTitle()}'." +
				"\n\nДетальный отчет аналогичен обычному, лишь предоставляет расширенную информацию.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Справка по работе с отчетом");
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = FilterViewModel.GetReportParametersSet();

			parameters.Add("start_date", dateperiodpicker.StartDateOrNull);
			parameters.Add("end_date", dateperiodpicker.EndDateOrNull);
			parameters.Add("creation_date", DateTime.Now);
			parameters.Add("show_phones", ycheckbuttonPhones.Active);

			if(_userIsSalesRepresentative)
			{
				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);

				parameters["Employee_include"] = new[] { currentEmployee.Id.ToString() };
				parameters["Employee_exclude"] = new[] { "0" };
			}

			return new ReportInfo
			{
				Identifier = ycheckbuttonDetail.Active ? "Sales.SalesReportDetail" : "Sales.SalesReport",
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDate != default)
			{
				OnUpdate(true);
			}
			else
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заполните дату.");
			}
		}

		private void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private void SetupFilter()
		{
			_filterViewModel = _includeExcludeSalesFilterFactory.CreateSalesReportIncludeExcludeFilter(UoW, _userIsSalesRepresentative);

			var filterView = new IncludeExludeFiltersView(FilterViewModel);

			vboxParameters.Add(filterView);
			filterView.Show();
		}
	}
}
