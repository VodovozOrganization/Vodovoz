using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using QS.ViewModels.Widgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Vodovoz.Controllers;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Reports.Editing;
using Vodovoz.Reports.Editing.Modifiers;
using Vodovoz.ViewModels.Factories;

namespace Vodovoz.ViewModels.ReportsParameters.Profitability
{
	/// <summary>
	/// !!!Важно!!! Если поменяется расчет в отчете, то нужно менять и в контроллере
	/// <see cref="RouteListProfitabilityController.CalculateRouteListProfitabilityGrossMargin"/>
	/// логику расчета и наоборот, при смене алгоритма в контроллере менять его механизм в отчете
	/// </summary>
	public class ProfitabilitySalesReportViewModel : ReportParametersViewModelBase
	{
		private Dictionary<string, object> _parameters = new Dictionary<string, object>();
		private LeftRightListViewModel<GroupingNode> _groupViewModel;
		private readonly bool _userIsSalesRepresentative;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IIncludeExcludeSalesFilterFactory _includeExcludeSalesFilterFactory;
		private readonly ILeftRightListViewModelFactory _leftRightListViewModelFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;
		private DelegateCommand _loadReportCommand;
		private DelegateCommand _showInfoCommand;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _isDetailed;
		private string _source;
		private IncludeExludeFiltersViewModel _filterViewModel;

		public ProfitabilitySalesReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeRepository employeeRepository,
			ICommonServices commonServices,
			IIncludeExcludeSalesFilterFactory includeExcludeSalesFilterFactory,
			ILeftRightListViewModelFactory leftRightListViewModelFactory)
			: base(rdlViewerViewModel)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			if(commonServices is null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_includeExcludeSalesFilterFactory = includeExcludeSalesFilterFactory ?? throw new ArgumentNullException(nameof(includeExcludeSalesFilterFactory));
			_leftRightListViewModelFactory = leftRightListViewModelFactory ?? throw new ArgumentNullException(nameof(leftRightListViewModelFactory));
			_interactiveService = commonServices.InteractiveService;

			Title = "Отчет по продажам с рентабельностью";

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot();

			_userIsSalesRepresentative =
				commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.User.IsSalesRepresentative)
				&& !commonServices.UserService.GetCurrentUser().IsAdmin;

			StartDate = DateTime.Today;
			EndDate = DateTime.Today;

			SetupFilter();

			SetupGroupings();
		}

		protected override Dictionary<string, object> Parameters => _parameters;

		public override ReportInfo ReportInfo
		{
			get
			{
				var reportInfo = new ReportInfo
				{
					Source = _source,
					Parameters = Parameters,
					Title = Title,
					UseUserVariables = true
				};
				return reportInfo;
			}
		}

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public virtual bool IsDetailed
		{
			get => _isDetailed;
			set => SetField(ref _isDetailed, value);
		}

		public virtual LeftRightListViewModel<GroupingNode> GroupingSelectViewModel
		{
			get => _groupViewModel;
			set => SetField(ref _groupViewModel, value);
		}

		private void SetupFilter()
		{
			_filterViewModel = _includeExcludeSalesFilterFactory.CreateSalesReportIncludeExcludeFilter(_unitOfWork, _userIsSalesRepresentative);
		}

		private void SetupGroupings()
		{
			GroupingSelectViewModel = _leftRightListViewModelFactory.CreateSalesReportGroupingsConstructor();
		}

		public DelegateCommand LoadReportCommand
		{
			get
			{
				if(_loadReportCommand == null)
				{
					_loadReportCommand = new DelegateCommand(GenerateReport);
				}
				return _loadReportCommand;
			}
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

			var groupingTitle = string.Empty;
			var groupingTitleCommaSplitted = string.Empty;

			if(GroupingSelectViewModel.RightItems.Any())
			{
				groupingTitle = string
					.Join(" | ", GroupingSelectViewModel
					.GetRightItems()
					.Select(x => x.GroupType.GetEnumTitle()));
				groupingTitleCommaSplitted = string
					.Join(", ", GroupingSelectViewModel
					.GetRightItems()
					.Select(x => x.GroupType.GetEnumTitle()));
			}
			else
			{
				groupingTitle = GroupingType.Nomenclature.GetEnumTitle();
				groupingTitleCommaSplitted = GroupingType.Nomenclature.GetEnumTitle();
			}

			_parameters.Add("grouping_title", groupingTitle);
			_parameters.Add("grouping_title_comma_splitrted", groupingTitleCommaSplitted);

			_parameters.Add("groups_count", groupParameters.Count());

			_source = GetReportSource();

			LoadReport();
		}

		private string GetReportSource()
		{
			var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var fileName = IsDetailed ? "ProfitabilitySalesReportDetail.rdl" : "ProfitabilitySalesReport.rdl";
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
				var modifier = new ProfitabilityDetailReportModifier();
				modifier.Setup(groupParameters.Select(x => (GroupingType)x.Value));
				result = modifier;
				
			}
			else
			{
				var isRouteListGroupingTypeSelected = groupParameters.Select(x => (GroupingType)x.Value).First() == GroupingType.RouteList;
				var isOnlyOneGroupingTypeSelected = groupParameters.Count() == 1;
				var isShowRouteListInfo = isRouteListGroupingTypeSelected && isOnlyOneGroupingTypeSelected;

				var modifier = new ProfitabilityReportModifier();
				modifier.Setup(groupParameters.Select(x => (GroupingType)x.Value), isShowRouteListInfo);
				result = modifier;
			}
			return result;
		}

		private IEnumerable<KeyValuePair<string, object>> GetGroupingParameters()
		{
			var result = new List<KeyValuePair<string, object>>();
			var groupItems = GroupingSelectViewModel.GetRightItems().ToList();
			if(!groupItems.Any())
			{
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

		public DelegateCommand ShowInfoCommand
		{
			get
			{
				if(_showInfoCommand == null)
				{
					_showInfoCommand = new DelegateCommand(ShowInfo);
				}
				return _showInfoCommand;
			}
		}

		public IncludeExludeFiltersViewModel FilterViewModel => _filterViewModel;

		private void ShowInfo()
		{
			var info =
$@"
Подсчет продаж ведется на основе заказов. 
В отчете учитываются заказы со статусами:
{OrderStatus.Accepted.GetEnumTitle()}
{OrderStatus.InTravelList.GetEnumTitle()}
{OrderStatus.OnLoading.GetEnumTitle()}
{OrderStatus.OnTheWay.GetEnumTitle()}
{OrderStatus.Shipped.GetEnumTitle()}
{OrderStatus.UnloadingOnStock.GetEnumTitle()}
{OrderStatus.Closed.GetEnumTitle()}
{OrderStatus.WaitForPayment.GetEnumTitle()}
Если выбран статус {OrderStatus.WaitForPayment.GetEnumTitle()}, то выбираются только заказы самовывозы с оплатой после отгрузки.

В отчет <b>не попадают</b> заказы, являющиеся закрывашками по контракту.
Фильтр по дате отсекает заказы, если дата доставки не входит в выбранный период.

Детальный отчет отличается от обычного тем, что у него подробно разбиты затраты и всегда есть группировка по товарам.

Цена продажи - Сумма продажи фактического количества товара с учетом скидки в пересчете на 1 единицу товара
Сумма продажи - Сумма продажи фактического количества товара с учетом скидки

Затраты:
	Производство или закупка - Если товар учавствует в групповой установке себестоимости, то это затраты на себестоимость, 
		а если нет, то это затраты на закупку.
	Фура - Стоимость доставки единицы товара с производства на склад
	Доставка - Стоимость доставки товара на адрес в пересчете на вес единицы товара
	Склад - Складские расходы в пересчете на вес единицы товара
	ОХР - административные расходы в пересчете на вес единицы товара
	Затраты на единицу - Сумма всех затрат на единицу товара
	Сумма затрат - затраты на все количество товара

Группировки:
	В отчете можно выбрать различные группировки, по которым будут собираться данные. 
	Можно выбрать максимум 3 группировки в любом порядке.
";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Справка по работе с отчетом");
		}
	}
}
