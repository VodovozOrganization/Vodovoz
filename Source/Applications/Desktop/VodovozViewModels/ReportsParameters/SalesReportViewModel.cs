using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report.ViewModels;
using QS.ViewModels.Widgets;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Gamma.Utilities;
using Vodovoz.Domain.Logistic;
using QS.Report;
using System.Linq;
using Vodovoz.Reports.Editing.Modifiers;
using QS.Commands;
using System.Reflection;
using System.IO;
using Vodovoz.Reports.Editing;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.ReportsParameters
{
	public class SalesReportViewModel : ReportParametersViewModelBase
	{
		private Dictionary<string, object> _parameters = new Dictionary<string, object>();

		private readonly IUnitOfWork _unitOfWork;

		private string _source;

		private readonly IIncludeExcludeSalesFilterFactory _includeExcludeSalesFilterFactory;
		private readonly ILeftRightListViewModelFactory _leftRightListViewModelFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IInteractiveService _interactiveService;

		private IncludeExludeFiltersViewModel _filterViewModel;
		private LeftRightListViewModel<GroupingNode> _groupViewModel;

		private readonly bool _userIsSalesRepresentative;
		private readonly bool _canSeePhones;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _showPhones;
		private bool _isDetailed;

		public SalesReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeRepository employeeRepository,
			IInteractiveService interactiveService,
			IIncludeExcludeSalesFilterFactory includeExcludeSalesFilterFactory,
			ILeftRightListViewModelFactory leftRightListViewModelFactory)
			: base(rdlViewerViewModel)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_includeExcludeSalesFilterFactory = includeExcludeSalesFilterFactory ?? throw new ArgumentNullException(nameof(includeExcludeSalesFilterFactory));
			_leftRightListViewModelFactory = leftRightListViewModelFactory ?? throw new ArgumentNullException(nameof(leftRightListViewModelFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

			StartDate = DateTime.Today;
			EndDate = DateTime.Today;

			Title = "Отчет по продажам";

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot();
			_unitOfWork.Session.DefaultReadOnly = true;

			_userIsSalesRepresentative =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.User.IsSalesRepresentative)
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;

			_canSeePhones = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Report.Sales.CanGetContactsInSalesReports);

			SetupFilter();

			SetupGroupings();

			ShowInfoCommand = new DelegateCommand(ShowInfoWindow);
			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public virtual LeftRightListViewModel<GroupingNode> GroupingSelectViewModel => _groupViewModel;

		public IncludeExludeFiltersViewModel FilterViewModel => _filterViewModel;

		public DelegateCommand ShowInfoCommand { get; }

		public DelegateCommand GenerateReportCommand { get; }

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public bool ShowPhones
		{
			get => _showPhones;
			set => SetField(ref _showPhones, value);
		}

		[PropertyChangedAlso(nameof(CanShowPhones))]
		public bool IsDetailed
		{
			get => _isDetailed;
			set
			{
				if(SetField(ref _isDetailed, value)
					&& !value)
				{
					ShowPhones = false;
				}
			}
		}

		public bool CanShowPhones => IsDetailed && _canSeePhones;

		protected override Dictionary<string, object> Parameters => _parameters;

		public override ReportInfo ReportInfo
		{
			get
			{
				var reportInfo = new ReportInfo
				{
					Source = _source,
					Parameters = Parameters,
					Title = Title
				};
				return reportInfo;
			}
		}

		private void SetupFilter()
		{
			_filterViewModel = _includeExcludeSalesFilterFactory.CreateSalesReportIncludeExcludeFilter(_unitOfWork, _userIsSalesRepresentative);
		}

		private void SetupGroupings()
		{
			_groupViewModel = _leftRightListViewModelFactory.CreateSalesReportGroupingsConstructor();
		}

		private void ShowInfoWindow()
		{
			var info =
				"<b>1.</b> Подсчет продаж ведется на основе заказов. В отчете учитываются заказы со статусами:\n" +
				$"\t'{OrderStatus.Accepted.GetEnumTitle()}'\n" +
				$"\t'{OrderStatus.InTravelList.GetEnumTitle()}'\n" +
				$"\t'{OrderStatus.OnLoading.GetEnumTitle()}'\n" +
				$"\t'{OrderStatus.OnTheWay.GetEnumTitle()}'\n" +
				$"\t'{OrderStatus.Shipped.GetEnumTitle()}'\n" +
				$"\t'{OrderStatus.UnloadingOnStock.GetEnumTitle()}'\n" +
				$"\t'{OrderStatus.Closed.GetEnumTitle()}'\n" +
				$"\t'{OrderStatus.WaitForPayment.GetEnumTitle()}' и заказ - самовывоз с оплатой после отгрузки.\n" +
				"В отчет <b>не попадают</b> заказы, являющиеся закрывашками по контракту.\n" +
				"Фильтр по дате отсекает заказы, если дата доставки не входит в выбранный период.\n\n" +
				"<b>2.</b> Подсчет тары ведется следующим образом:\n" +
				"\tПлановое значение - сумма бутылей на возврат попавших в отчет заказов;\n" +
				"\tФактическое значение - сумма фактически возвращенных бутылей по адресам маршрутного листа.\n" +
				"\t\tФактическое значение возвращенных бутылей по адресу зависит от того, доставлен<b>(*)</b> заказ или нет:\n" +
				"\t\t\t <b>-</b> Если да - берется кол-во бутылей, которое по факту забрал водитель. " +
				"Это кол-во может быть вручную указано при закрытии МЛ;\n" +

				"\t\t\t <b>-</b> Если не доставлен - берется кол-во бутылей на возврат из заказа;\n" +
				"\t\t\t <b>-</b> Если заказ является самовывозом - берется значение возвращенной тары, указанное в отпуске самовывоза;\n" +
				$"\t\t <b>*</b> Заказ считается доставленным, если его статус в МЛ: '{RouteListItemStatus.Completed.GetEnumTitle()}' или " +
				$"'{RouteListItemStatus.EnRoute.GetEnumTitle()}' и статус МЛ '{RouteListStatus.Closed.GetEnumTitle()}' " +
				$"или '{RouteListStatus.OnClosing.GetEnumTitle()}'.\n" +
				$"По умолчаению используется группировка Тип номенклатуры | Номенклатура\n\n" +
				"Детальный отчет аналогичен обычному, лишь предоставляет расширенную информацию.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Справка по работе с отчетом");
		}

		private IEnumerable<KeyValuePair<string, object>> GetGroupingParameters()
		{
			var result = new List<KeyValuePair<string, object>>();
			var groupItems = GroupingSelectViewModel.GetRightItems().ToList();

			if(!groupItems.Any())
			{
				groupItems.Add(new GroupingNode { GroupType = GroupingType.NomenclatureType });
				groupItems.Add(new GroupingNode { GroupType = GroupingType.Nomenclature });
			}

			if(groupItems.Count > 3)
			{
				throw new InvalidOperationException("Нельзя использовать более трех группировок");
			}

			var groupCounter = 1;

			foreach(var item in groupItems)
			{
				result.Add(new KeyValuePair<string, object>($"group{groupCounter}", item.GroupType));
				groupCounter++;
			}

            return result;
		}

		private void GenerateReport()
		{
			if(StartDate == null || StartDate == default(DateTime))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заполните дату.");
			}

			_parameters = FilterViewModel.GetReportParametersSet();
			_parameters.Add("start_date", StartDate);
			_parameters.Add("end_date", EndDate);
			_parameters.Add("creation_date", DateTime.Now);
			_parameters.Add("show_phones", ShowPhones);

			if(_userIsSalesRepresentative)
			{
				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(_unitOfWork);

				_parameters.Add("Employee_include", new[] { currentEmployee.Id.ToString() });
				_parameters.Add("Employee_exclude", new[] { "0" });
			}

			var groupParameters = GetGroupingParameters();

			foreach(var groupParameter in groupParameters)
			{
				_parameters.Add(groupParameter.Key, groupParameter.Value.ToString());
			}

			_parameters.Add("groups_count", groupParameters.Count());

			var groupingTitle = string
                .Join(" | ", GroupingSelectViewModel
                    .GetRightItems()
				    .Select(x => x.GroupType.GetEnumTitle()));

            _parameters.Add("grouping_title", groupingTitle);

            _source = GetReportSource();

			LoadReport();
		}

		private string GetReportSource()
		{
			var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var fileName = IsDetailed ? "SalesReportDetail.rdl" : "SalesReport.rdl";
			var path = Path.Combine(root, "Reports", "Sales", fileName);

			return ModifyReport(path);
		}

		private string ModifyReport(string path)
		{
			var modifier = GetReportModifier();

			using(ReportController reportController = new ReportController(path))
			using(var reportStream = new MemoryStream())
			{
				reportController.AddModifier(modifier);

				reportController.Modify();
				reportController.Save(reportStream);

				using(var reader = new StreamReader(reportStream))
				{
					reportStream.Position = 0;
					var outputSource = reader.ReadToEnd();
					return outputSource;
				}
			}
		}

		private ReportModifierBase GetReportModifier()
		{
			ReportModifierBase result;
			var groupParameters = GetGroupingParameters();
			if(IsDetailed)
			{
				var modifier = new SalesDetailReportModifier();
				modifier.Setup(groupParameters.Select(x => (GroupingType)x.Value), !ShowPhones);
				result = modifier;
			}
			else
			{
				var modifier = new SalesReportModifier();
				modifier.Setup(groupParameters.Select(x => (GroupingType)x.Value));
				result = modifier;
			}
			return result;
		}
	}
}
