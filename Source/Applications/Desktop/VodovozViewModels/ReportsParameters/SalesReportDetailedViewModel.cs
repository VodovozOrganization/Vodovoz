using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;
using Vodovoz.Reports.Editing.Modifiers;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewModels.Services.SalesReport;
using Vodovoz.ViewModels.Widgets;
using VodovozBusiness.Nodes.SalesReport;

namespace Vodovoz.ViewModels.ReportsParameters
{
	public class SalesReportDetailedViewModel : DialogTabViewModelBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly ISalesReportService _salesReportService;
		private readonly IFileDialogService _fileDialogService;
		private readonly ILogger<SalesReportDetailedViewModel> _logger;
		private readonly IIncludeExcludeSalesFilterFactory _includeExcludeSalesFilterFactory;
		private readonly ILeftRightListViewModelFactory _leftRightListViewModelFactory;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IUserService _userService;

		private int _ordersCount;
		private IncludeExludeFiltersViewModel _filterViewModel;
		private LeftRightListViewModel<GroupingNode> _groupViewModel;
		private SalesReportTreeNode _nodes;
		private ObservableList<SalesReportDisplayNode> _displayNodes;
		private BottlesDataNode _bottlesDataNode;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _showPhones;
		private bool _isLoading;
		private bool _canSeePhones;
		private bool _userIsSalesRepresentative;
		private bool _canAccessSalesReports;
		private readonly bool _canViewReportSalesWithCashReceipts;

		public SalesReportDetailedViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeRepository employeeRepository,
			IInteractiveService interactiveService,
			ISalesReportService salesReportService,
			IFileDialogService fileDialogService,
			IIncludeExcludeSalesFilterFactory includeExcludeSalesFilterFactory,
			ILeftRightListViewModelFactory leftRightListViewModelFactory,
			IUserService userService,
			ICurrentPermissionService currentPermissionService,
			INavigationManager navigation,
			ILogger<SalesReportDetailedViewModel> logger) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_salesReportService = salesReportService ?? throw new ArgumentNullException(nameof(salesReportService));
			_fileDialogService = fileDialogService ?? throw new ArgumentException(nameof(fileDialogService));
			_includeExcludeSalesFilterFactory = includeExcludeSalesFilterFactory ?? throw new ArgumentNullException(nameof(includeExcludeSalesFilterFactory));
			_leftRightListViewModelFactory = leftRightListViewModelFactory ?? throw new ArgumentNullException(nameof(leftRightListViewModelFactory));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_canAccessSalesReports = _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ReportPermissions.Sales.CanAccessSalesReports);
			_canViewReportSalesWithCashReceipts = _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ReportPermissions.Sales.CanViewReportSalesWithCashReceipts);

			StartDate = DateTime.Today;
			EndDate = DateTime.Today;

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot();
			_unitOfWork.Session.DefaultReadOnly = true;

			_userIsSalesRepresentative =
				_currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.IsSalesRepresentative)
				&& !_userService.GetCurrentUser().IsAdmin;

			_canSeePhones = _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ReportPermissions.Sales.CanGetContactsInReports);

			Title = "Подробный отчет по продажам";

			SetupFilter();
			SetupGroupings();

			ShowInfoCommand = new DelegateCommand(ShowInfoWindow);
			GenerateReportCommand = new DelegateCommand(GenerateReport);
			ExportToExcelCommand = new DelegateCommand(ExportToExcel);
		}

		public virtual LeftRightListViewModel<GroupingNode> GroupingSelectViewModel => _groupViewModel;

		public IList<SalesReportGrouping> SelectedGroupings { get; private set; }

		public ObservableList<SalesReportDisplayNode> DisplayNodes
		{
			get => _displayNodes;
			set => SetField(ref _displayNodes, value);
		}

		public IncludeExludeFiltersViewModel FilterViewModel => _filterViewModel;

		public OrderDateFilterViewModel OrderDateFilterViewModel { get; private set; } = new OrderDateFilterViewModel();

		public DelegateCommand ShowInfoCommand { get; }

		public DelegateCommand GenerateReportCommand { get; }

		public DelegateCommand ExportToExcelCommand { get; }

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

		public bool IsLoading
		{
			get => _isLoading;
			set => SetField(ref _isLoading, value);
		}

		public bool CanShowPhones => _canSeePhones;

		public bool CanAccessSalesReports => _canAccessSalesReports;

		private void SetupFilter()
		{
			var onlyCurrentEmployee = _userIsSalesRepresentative || !_canAccessSalesReports;
			_filterViewModel = _includeExcludeSalesFilterFactory.CreateSalesReportIncludeExcludeFilter(
				_unitOfWork,
				onlyCurrentEmployee ? (int?)_employeeRepository.GetEmployeeForCurrentUser(_unitOfWork).Id : null);

			var additionalParams = new Dictionary<string, string>
			{
				{ "Самовывоз", "is_self_delivery" },
			};

			if(_canViewReportSalesWithCashReceipts)
			{
				additionalParams.Add("Только с чеками", "only_with_cash_receipts");
			}

			additionalParams.Add("Только заказы в МЛ", "only_orders_from_route_lists");

			_filterViewModel.AddFilter("Дополнительные фильтры", additionalParams);
			_filterViewModel.SelectionChanged += OnFilterViewModelSelectionChanged;
		}

		private void OnFilterViewModelSelectionChanged(object sender, EventArgs e)
		{
			CheckAndRefreshSelectedGroupsForCashReceiptOnly();
		}

		private void CheckAndRefreshSelectedGroupsForCashReceiptOnly()
		{
			if(!(_filterViewModel.ActiveFilter is IncludeExcludeBoolParamsFilter))
			{
				return;
			}

			var parameters = FilterViewModel.GetReportParametersSet(out var sb);

			if(!parameters.TryGetValue("only_with_cash_receipts", out object value))
			{
				return;
			}

			if(!(value is bool included) || !included)
			{
				return;
			}

			try
			{
				GroupingSelectViewModel.RightItems.ContentChanged -= OnGroupingsRightItemsListContentChanged;

				_leftRightListViewModelFactory.SetDefaultLeftItemsForSalesWithDynamicsReportGroupings(GroupingSelectViewModel);

				var leftGroupingItems = GroupingSelectViewModel.LeftItems;

				var organizationGroup = leftGroupingItems
					.FirstOrDefault(x => (x as LeftRightListItemViewModel<GroupingNode>).Content.GroupType == GroupingType.Organization);

				var nomenclatureGroup = leftGroupingItems
					.FirstOrDefault(x => (x as LeftRightListItemViewModel<GroupingNode>).Content.GroupType == GroupingType.Nomenclature);

				foreach(var item in GroupingSelectViewModel.LeftItems.ToArray())
				{
					if(item != organizationGroup)
					{
						continue;
					}

					if(!GroupingSelectViewModel.RightItems.Contains(item))
					{
						GroupingSelectViewModel.RightItems.Add(item);
					}

					if(GroupingSelectViewModel.LeftItems.Contains(item))
					{
						GroupingSelectViewModel.LeftItems.Remove(item);
					}
				}

				foreach(var item in GroupingSelectViewModel.LeftItems.ToArray())
				{
					if(item != nomenclatureGroup)
					{
						continue;
					}

					if(!GroupingSelectViewModel.RightItems.Contains(item))
					{
						GroupingSelectViewModel.RightItems.Add(item);
					}

					if(GroupingSelectViewModel.LeftItems.Contains(item))
					{
						GroupingSelectViewModel.LeftItems.Remove(item);
					}
				}
			}
			finally
			{
				GroupingSelectViewModel.RightItems.ContentChanged += OnGroupingsRightItemsListContentChanged;
			}
		}

		private void OnGroupingsRightItemsListContentChanged(object sender, EventArgs e)
		{
			CheckAndRefreshSelectedGroupsForCashReceiptOnly();
		}

		private void SetupGroupings()
		{
			_groupViewModel = _leftRightListViewModelFactory.CreateSalesReportGroupingsConstructor();
			_groupViewModel.RightItems.ContentChanged += OnGroupingsRightItemsListContentChanged;
		}

		private void GenerateReport()
		{
			if(StartDate is null || StartDate == default)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заполните дату начала выборки");
				return;
			}

			if(EndDate is null || EndDate == default)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заполните дату окончания выборки");
				return;
			}

			GenerateDetailedReportAsync().GetAwaiter().GetResult();
		}

		private async Task GenerateDetailedReportAsync()
		{
			try
			{
				IsLoading = true;

				var filters = BuildFiltersFromViewModel();

				UpdateSelectedGroupings();

				var data = await _salesReportService.GetSalesReportDataAsync(
					_unitOfWork,
					StartDate.Value,
					EndDate.Value,
					OrderDateFilterViewModel.SelectedOrderDateFilterType,
					filters);

				var countOfOrders = data.Select(d => d.OrderId).Distinct();

				_bottlesDataNode = await _salesReportService.GetBottlesDataAsync(
					_unitOfWork, countOfOrders);

				var tree = BuildTree(data, SelectedGroupings, 0);

				var totalNode = new SalesReportTreeNode
				{
					Name = "Итого:",
					Level = 0,
					Children = tree,
					TotalCount = tree.Sum(n => n.TotalCount),
					TotalSum = tree.Sum(n => n.TotalSum),
					IsTotalNode = true
				};

				_nodes = totalNode;
				_ordersCount = countOfOrders.Count();

				DisplayNodes = FlattenTreeToDisplayNodes(_nodes);

				OnPropertyChanged(nameof(DisplayNodes));
			}
			catch(Exception ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Ошибка: {ex.Message}");
			}
			finally
			{
				IsLoading = false;
			}
		}

		private void ExportToExcel()
		{
			if(_nodes is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Нет данных для экспорта");
				return;
			}

			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить отчет",
				DefaultFileExtention = ".xlsx",
				FileName = $"Отчет_по_продажам_{StartDate:dd.MM.yyyy}_{EndDate:dd.MM.yyyy}.xlsx"
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(result.Successful)
			{
				try
				{
					var groupingTitle = string.Join(" | ", SelectedGroupings.Select(g => g.Type.GetEnumTitle()));
					_salesReportService.ExportToExcel(
						_nodes.Children,
						StartDate.Value,
						EndDate.Value,
						groupingTitle,
						_ordersCount,
						_bottlesDataNode.Plan,
						_bottlesDataNode.Fact,
						result.Path)
						.GetAwaiter()
						.GetResult();

					_interactiveService.ShowMessage(ImportanceLevel.Info, "Файл успешно сохранен");
				}
				catch(Exception ex)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, $"Ошибка при экспорте:\n{ex.Message}");
					_logger.LogError(ex, "Ошибка при экспорте отчета по продажам");
				}
			}
		}

		private ObservableList<SalesReportDisplayNode> FlattenTreeToDisplayNodes(SalesReportTreeNode rootNode)
		{
			var result = new ObservableList<SalesReportDisplayNode>();

			if(rootNode != null)
			{
				result.Add(new SalesReportDisplayNode
				{
					Level = rootNode.Level,
					Code = string.Empty,
					Counterparty = string.Empty,
					DeliveryPoint = string.Empty,
					OrderDetails = string.Empty,
					Nomenclature = rootNode.Name,
					Count = rootNode.TotalCount,
					Sum = rootNode.TotalSum
				});
			}

			foreach(var child in rootNode.Children)
			{
				if(child.Children != null && child.Children.Any())
				{
					result.Add(new SalesReportDisplayNode
					{
						Level = child.Level + 1,
						Code = string.Empty,
						Counterparty = string.Empty,
						DeliveryPoint = string.Empty,
						OrderDetails = string.Empty,
						Nomenclature = child.Name,
						Count = child.TotalCount,
						Sum = child.TotalSum
					});

					result.AddRange(FlattenChildrenToDisplayNodes(child, child.Level + 2));
				}
				else if(child.Data != null)
				{
					var data = child.Data;
					result.Add(new SalesReportDisplayNode
					{
						Level = child.Level + 1,
						Code = data.CounterpartyId.ToString(),
						Counterparty = data.Counterparty,
						DeliveryPoint = data.DeliveryPoint,
						OrderDetails = data.OrdDetails,
						Nomenclature = data.NomenclatureName,
						Count = data.TotalCount,
						Sum = data.TotalSum
					});
				}
			}

			return result;
		}

		private List<SalesReportDisplayNode> FlattenChildrenToDisplayNodes(SalesReportTreeNode node, int level)
		{
			var result = new List<SalesReportDisplayNode>();

			foreach(var child in node.Children)
			{
				if(child.Children != null && child.Children.Any())
				{
					result.Add(new SalesReportDisplayNode
					{
						Level = level,
						Code = string.Empty,
						Counterparty = string.Empty,
						DeliveryPoint = string.Empty,
						OrderDetails = string.Empty,
						Nomenclature = child.Name,
						Count = child.TotalCount,
						Sum = child.TotalSum
					});

					result.AddRange(FlattenChildrenToDisplayNodes(child, level + 1));
				}
				else if(child.Data != null)
				{
					var data = child.Data;
					result.Add(new SalesReportDisplayNode
					{
						Level = level,
						Code = data.CounterpartyId.ToString(),
						Counterparty = data.Counterparty,
						DeliveryPoint = data.DeliveryPoint,
						OrderDetails = data.OrdDetails,
						Nomenclature = data.NomenclatureName,
						Count = data.TotalCount,
						Sum = data.TotalSum
					});
				}
			}

			return result;
		}

		private SalesReportFilters BuildFiltersFromViewModel()
		{
			var filters = new SalesReportFilters();

			if(FilterViewModel is null)
			{
				return filters;
			}

			foreach(var filter in FilterViewModel.Filters)
			{
				switch(filter)
				{
					case IncludeExcludeEnumFilter<NomenclatureCategory> categoryFilter:
						filters.NomenclatureCategoryInclude = categoryFilter.IncludedElements
							.Select(x => (NomenclatureCategory)Enum.Parse(typeof(NomenclatureCategory), x.Number))
							.ToArray();
						filters.NomenclatureCategoryExclude = categoryFilter.ExcludedElements
							.Select(x => (NomenclatureCategory)Enum.Parse(typeof(NomenclatureCategory), x.Number))
							.ToArray();
						break;

					case IncludeExcludeEntityFilter<Nomenclature> nomenclatureFilter:
						filters.NomenclatureInclude = nomenclatureFilter.IncludedElements.Select(e => int.Parse(e.Number)).ToArray();
						filters.NomenclatureExclude = nomenclatureFilter.ExcludedElements.Select(e => int.Parse(e.Number)).ToArray();
						break;

					case IncludeExcludeEntityFilter<ProductGroup> productGroupFilter:
						filters.ProductGroupInclude = productGroupFilter.IncludedElements.Select(e => int.Parse(e.Number)).ToArray();
						filters.ProductGroupExclude = productGroupFilter.ExcludedElements.Select(e => int.Parse(e.Number)).ToArray();
						break;

					case IncludeExcludeEnumFilter<CounterpartyType> counterpartyTypeFilter:
						var typesToInclude = counterpartyTypeFilter.IncludedElements.Where(x => x.Parent == null).ToArray();
						var subtypesToInclude = counterpartyTypeFilter.IncludedElements.Where(x => x.Parent != null).ToArray();

						if(typesToInclude.Any())
						{
							filters.CounterpartyTypeInclude = typesToInclude
								.Select(x => (CounterpartyType)Enum.Parse(typeof(CounterpartyType), x.Number))
								.ToArray();
						}

						if(subtypesToInclude.Any())
						{
							filters.CounterpartySubtypeInclude = subtypesToInclude
								.Select(x => int.Parse(x.Number))
								.ToArray();
						}

						var typesToExclude = counterpartyTypeFilter.ExcludedElements.Where(x => x.Parent == null).ToArray();
						var subtypesToExclude = counterpartyTypeFilter.ExcludedElements.Where(x => x.Parent != null).ToArray();

						if(typesToExclude.Any())
						{
							filters.CounterpartyTypeExclude = typesToExclude
								.Select(x => (CounterpartyType)Enum.Parse(typeof(CounterpartyType), x.Number))
								.ToArray();
						}

						if(subtypesToExclude.Any())
						{
							filters.CounterpartySubtypeExclude = subtypesToExclude
								.Select(x => int.Parse(x.Number))
								.ToArray();
						}
						break;

					case IncludeExcludeEntityFilter<Counterparty> counterpartyFilter:
						filters.CounterpartyInclude = counterpartyFilter.IncludedElements.Select(e => int.Parse(e.Number)).ToArray();
						filters.CounterpartyExclude = counterpartyFilter.ExcludedElements.Select(e => int.Parse(e.Number)).ToArray();
						break;

					case IncludeExcludeEntityFilter<Organization> organizationFilter:
						filters.OrganizationInclude = organizationFilter.IncludedElements.Select(e => int.Parse(e.Number)).ToArray();
						filters.OrganizationExclude = organizationFilter.ExcludedElements.Select(e => int.Parse(e.Number)).ToArray();
						break;

					case IncludeExcludeEntityFilter<DiscountReason> discountReasonFilter:
						filters.DiscountReasonInclude = discountReasonFilter.IncludedElements.Select(e => int.Parse(e.Number)).ToArray();
						filters.DiscountReasonExclude = discountReasonFilter.ExcludedElements.Select(e => int.Parse(e.Number)).ToArray();
						break;

					case IncludeExcludeEntityFilter<Subdivision> subdivisionFilter:
						filters.SubdivisionInclude = subdivisionFilter.IncludedElements.Select(e => int.Parse(e.Number)).ToArray();
						filters.SubdivisionExclude = subdivisionFilter.ExcludedElements.Select(e => int.Parse(e.Number)).ToArray();
						break;

					case IncludeExcludeEntityFilter<Employee> employeeFilter when filter.Title == "Авторы заказов":
						filters.OrderAuthorInclude = employeeFilter.IncludedElements.Select(e => int.Parse(e.Number)).ToArray();
						filters.OrderAuthorExclude = employeeFilter.ExcludedElements.Select(e => int.Parse(e.Number)).ToArray();
						break;

					case IncludeExcludeEntityFilter<GeoGroup> geoGroupFilter:
						filters.GeoGroupInclude = geoGroupFilter.IncludedElements.Select(e => int.Parse(e.Number)).ToArray();
						filters.GeoGroupExclude = geoGroupFilter.ExcludedElements.Select(e => int.Parse(e.Number)).ToArray();
						break;

					case IncludeExcludeEnumFilter<PaymentType> paymentTypeFilter:
						var paymentTypesToInclude = paymentTypeFilter.IncludedElements.Where(x => x.Parent == null).ToArray();
						var terminalSourcesToInclude = paymentTypeFilter.IncludedElements
							.Where(x => x.Parent != null && x.Parent.Number == PaymentType.Terminal.ToString())
							.ToArray();
						var paymentFromsToInclude = paymentTypeFilter.IncludedElements
							.Where(x => x.Parent != null && x.Parent.Number == PaymentType.PaidOnline.ToString())
							.ToArray();

						if(paymentTypesToInclude.Any())
						{
							filters.PaymentTypeInclude = paymentTypesToInclude
								.Select(x => (PaymentType)Enum.Parse(typeof(PaymentType), x.Number))
								.ToArray();
						}

						if(terminalSourcesToInclude.Any())
						{
							filters.PaymentByTerminalSourceInclude = terminalSourcesToInclude
								.Select(x => (PaymentByTerminalSource)Enum.Parse(typeof(PaymentByTerminalSource), x.Number))
								.ToArray();
						}

						if(paymentFromsToInclude.Any())
						{
							filters.PaymentFromInclude = paymentFromsToInclude
								.Select(x => int.Parse(x.Number))
								.ToArray();
						}

						var paymentTypesToExclude = paymentTypeFilter.ExcludedElements.Where(x => x.Parent == null).ToArray();
						var terminalSourcesToExclude = paymentTypeFilter.ExcludedElements
							.Where(x => x.Parent != null && x.Parent.Number == PaymentType.Terminal.ToString())
							.ToArray();
						var paymentFromsToExclude = paymentTypeFilter.ExcludedElements
							.Where(x => x.Parent != null && x.Parent.Number == PaymentType.PaidOnline.ToString())
							.ToArray();

						if(paymentTypesToExclude.Any())
						{
							filters.PaymentTypeExclude = paymentTypesToExclude
								.Select(x => (PaymentType)Enum.Parse(typeof(PaymentType), x.Number))
								.ToArray();
						}

						if(terminalSourcesToExclude.Any())
						{
							filters.PaymentByTerminalSourceExclude = terminalSourcesToExclude
								.Select(x => (PaymentByTerminalSource)Enum.Parse(typeof(PaymentByTerminalSource), x.Number))
								.ToArray();
						}

						if(paymentFromsToExclude.Any())
						{
							filters.PaymentFromExclude = paymentFromsToExclude
								.Select(x => int.Parse(x.Number))
								.ToArray();
						}
						break;

					case IncludeExcludeEntityFilter<PromotionalSet> promotionalSetFilter:
						filters.PromotionalSetInclude = promotionalSetFilter.IncludedElements.Select(e => int.Parse(e.Number)).ToArray();
						filters.PromotionalSetExclude = promotionalSetFilter.ExcludedElements.Select(e => int.Parse(e.Number)).ToArray();
						break;

					case IncludeExcludeEnumFilter<OrderStatus> orderStatusFilter:
						filters.OrderStatusInclude = orderStatusFilter.IncludedElements
							.Select(x => (OrderStatus)Enum.Parse(typeof(OrderStatus), x.Number))
							.ToArray();
						filters.OrderStatusExclude = orderStatusFilter.ExcludedElements
							.Select(x => (OrderStatus)Enum.Parse(typeof(OrderStatus), x.Number))
							.ToArray();
						break;

					case IncludeExcludeEnumFilter<CounterpartyCompositeClassification> classificationFilter:
						filters.CounterpartyCompositeClassificationInclude = classificationFilter.IncludedElements
							.Select(x => (CounterpartyCompositeClassification)Enum.Parse(typeof(CounterpartyCompositeClassification), x.Number))
							.ToArray();
						filters.CounterpartyCompositeClassificationExclude = classificationFilter.ExcludedElements
							.Select(x => (CounterpartyCompositeClassification)Enum.Parse(typeof(CounterpartyCompositeClassification), x.Number))
							.ToArray();
						break;

					case IncludeExcludeEntityFilter<Employee> managerFilter when filter.Title == "Менеджеры КА":
						filters.SalesManagerInclude = managerFilter.IncludedElements.Select(e => int.Parse(e.Number)).ToArray();
						filters.SalesManagerExclude = managerFilter.ExcludedElements.Select(e => int.Parse(e.Number)).ToArray();
						break;

					case IncludeExcludeBoolParamsFilter boolParamsFilter:
						foreach(var element in boolParamsFilter.IncludedElements)
						{
							switch(element.Number)
							{
								case "is_self_delivery":
									filters.IsSelfDelivery = true;
									break;
								case "only_with_cash_receipts":
									filters.OnlyWithCashReceipts = true;
									break;
								case "only_orders_from_route_lists":
									filters.OnlyOrdersFromRouteLists = true;
									break;
							}
						}
						foreach(var element in boolParamsFilter.ExcludedElements)
						{
							switch(element.Number)
							{
								case "is_self_delivery":
									filters.IsSelfDelivery = false;
									break;
								case "only_with_cash_receipts":
									filters.OnlyWithCashReceipts = false;
									break;
								case "only_orders_from_route_lists":
									filters.OnlyOrdersFromRouteLists = false;
									break;
							}
						}
						break;
				}
			}

			return filters;
		}

		private void UpdateSelectedGroupings()
		{
			SelectedGroupings = GroupingSelectViewModel.GetRightItems()
				.Select(item => new SalesReportGrouping { Type = item.GroupType })
				.ToList();

			if(!SelectedGroupings.Any())
			{
				SelectedGroupings = new List<SalesReportGrouping>
				{
					new SalesReportGrouping { Type = GroupingType.NomenclatureType },
					new SalesReportGrouping { Type = GroupingType.Nomenclature }
				};
			}
		}

		private IList<SalesReportTreeNode> BuildTree(
			IEnumerable<SalesReportDataNode> data,
			IList<SalesReportGrouping> groupings,
			int level)
		{
			if(level >= groupings.Count)
			{
				return data.Select(item => new SalesReportTreeNode
				{
					Name = $"{item.NomenclatureName} | {item.TotalCount} шт. | {item.TotalSum:C}",
					Data = item,
					Level = level,
					TotalCount = item.TotalCount,
					TotalSum = item.TotalSum
				}).ToList();
			}

			var grouping = groupings[level];
			var groups = data.GroupBy(x => grouping.GetGroupKey(x));

			var nodes = new List<SalesReportTreeNode>();

			foreach(var group in groups)
			{
				var children = BuildTree(group, groupings, level + 1);

				var node = new SalesReportTreeNode
				{
					Name = group.Key,
					Level = level,
					Children = children,
					TotalCount = children.Sum(c => c.TotalCount),
					TotalSum = children.Sum(c => c.TotalSum)
				};

				nodes.Add(node);
			}

			return nodes;
		}

		private void ShowInfoWindow()
		{
			var info =
$@"<b>1.</b> Подсчет продаж ведется на основе заказов. В отчете учитываются заказы со статусами:
	'{OrderStatus.Accepted.GetEnumTitle()}'
	'{OrderStatus.InTravelList.GetEnumTitle()}'
	'{OrderStatus.OnLoading.GetEnumTitle()}'
	'{OrderStatus.OnTheWay.GetEnumTitle()}'
	'{OrderStatus.Shipped.GetEnumTitle()}'
	'{OrderStatus.UnloadingOnStock.GetEnumTitle()}'
	'{OrderStatus.Closed.GetEnumTitle()}'
В отчет <b>не попадают</b> заказы, являющиеся закрывашками по контракту.

«Только заказы в МЛ» - выбираются заказы только в МЛ где авто не фура, для получения схожих данных с отчетом по статистике по дням недели
<b>2.</b> Подсчет тары ведется следующим образом:
	Плановое значение - сумма бутылей на возврат попавших в отчет заказов;
	Фактическое значение - сумма фактически возвращенных бутылей по адресам маршрутного листа.
		Фактическое значение возвращенных бутылей по адресу зависит от того, доставлен<b>(*)</b> заказ или нет:
			 <b>-</b> Если да - берется кол-во бутылей, которое по факту забрал водитель. Это кол-во может быть вручную указано при закрытии МЛ;
			 <b>-</b> Если не доставлен - берется кол-во бутылей на возврат из заказа;
			 <b>-</b> Если заказ является самовывозом - берется значение возвращенной тары, указанное в отпуске самовывоза;
		 <b>*</b> Заказ считается доставленным, если его статус в МЛ: '{RouteListItemStatus.Completed.GetEnumTitle()}' или '{RouteListItemStatus.EnRoute.GetEnumTitle()}' и статус МЛ '{RouteListStatus.Closed.GetEnumTitle()}' или '{RouteListStatus.OnClosing.GetEnumTitle()}'.
По умолчанию используется группировка Тип номенклатуры | Номенклатура

Фильтр по типу даты:
 <b>-</b> Создания: в отчет попадают заказы по дате создания заказа.
 <b>-</b> Доставки: в отчет попадают заказы по дате доставки
 <b>-</b> Оплаты:
		- Для форм оплаты: Наличная, Терминал (оба вида)
		  Дата оплаты = Дата доставки в заказе
		- Для форм оплаты: Бартер, Контрактная документация и источников онлайн оплаты:
			Сайт, Приложение, ВК, Маркетплейс, Кулер Сейл, Сайт Я.Сплит, МП Я.Сплит
		  Дата оплаты = Дата создания заказа
		- Для форм оплаты: SMS (QR), МП водителя (QR) и источников онлайн оплаты:
			Сайт по QR, Авангард по карте, МП по QR
		  Дата оплаты = дата оплаты быстрого платежа
		- Для форм оплаты: 
			-Безналичная
		  Дата оплаты = дата платежа из банка		

Детальный отчет аналогичен обычному, лишь предоставляет расширенную информацию.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Справка по работе с отчетом");
		}
	}
}
