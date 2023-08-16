using Gamma.Utilities;
using NHibernate.Linq;
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
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Extensions;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Reports.Editing;
using Vodovoz.Reports.Editing.Modifiers;

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
		private readonly IncludeExludeFiltersViewModel _filterViewModel;
		private readonly IGenericRepository<Nomenclature> _nomenclatureRepository;
		private readonly IGenericRepository<ProductGroup> _productGroupRepository;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;
		private readonly IGenericRepository<Organization> _organizationRepository;
		private readonly IGenericRepository<DiscountReason> _discountReasonRepository;
		private readonly IGenericRepository<Subdivision> _subdivisionRepository;
		private readonly IGenericRepository<PaymentFrom> _paymentFromRepository;
		private readonly IGenericRepository<Employee> _employeeGenericRepository;
		private readonly IGenericRepository<GeoGroup> _geographicalGroupRepository;
		private readonly IGenericRepository<PromotionalSet> _promotionalSetRepository;
		private LeftRightListViewModel<GroupingNode> _groupViewModel;
		private readonly bool _userIsSalesRepresentative;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;
		private DelegateCommand _loadReportCommand;
		private DelegateCommand _showInfoCommand;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _isDetailed;
		private string _source;

		public ProfitabilitySalesReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IEmployeeRepository employeeRepository,
			ICommonServices commonServices,
			IncludeExludeFiltersViewModel includeExludeFiltersViewModel,
			IGenericRepository<Nomenclature> nomenclatureRepository,
			IGenericRepository<ProductGroup> productGroupRepository,
			IGenericRepository<Counterparty> counterpartyRepository,
			IGenericRepository<Organization> organizationRepository,
			IGenericRepository<DiscountReason> discountReasonRepository,
			IGenericRepository<Subdivision> subdivisionRepository,
			IGenericRepository<PaymentFrom> paymentFromRepository,
			IGenericRepository<Employee> employeeGenericRepository,
			IGenericRepository<GeoGroup> geographicalGroupRepository,
			IGenericRepository<PromotionalSet> promotionalSetRepository)
			: base(rdlViewerViewModel)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_filterViewModel = includeExludeFiltersViewModel ?? throw new ArgumentNullException(nameof(includeExludeFiltersViewModel));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_productGroupRepository = productGroupRepository ?? throw new ArgumentNullException(nameof(productGroupRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_interactiveService = commonServices.InteractiveService;

			Title = "Отчет по продажам с рентабельностью";

			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();

			_userIsSalesRepresentative =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.User.IsSalesRepresentative)
				&& !_commonServices.UserService.GetCurrentUser().IsAdmin;

			StartDate = DateTime.Today;
			EndDate = DateTime.Today;

			SetupFilter();

			var groupingNodes = GetGroupingNodes();
			LeftRightListViewModel<GroupingNode> leftRightListViewModel = new LeftRightListViewModel<GroupingNode>();
			leftRightListViewModel.LeftLabel = "Доступные группировки";
			leftRightListViewModel.RightLabel = "Выбранные группировки (макс. 3)";
			leftRightListViewModel.RightItemsMaximum = 3;
			leftRightListViewModel.SetLeftItems(groupingNodes, x => x.Name);
			GroupingSelectViewModel = leftRightListViewModel;
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_paymentFromRepository = paymentFromRepository ?? throw new ArgumentNullException(nameof(paymentFromRepository));
			_employeeGenericRepository = employeeGenericRepository ?? throw new ArgumentNullException(nameof(employeeGenericRepository));
			_geographicalGroupRepository = geographicalGroupRepository ?? throw new ArgumentNullException(nameof(geographicalGroupRepository));
			_promotionalSetRepository = promotionalSetRepository;
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
					Title = Title
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

		private IEnumerable<GroupingNode> GetGroupingNodes()
		{
			return new[] { 
				new GroupingNode{ Name = "Заказ", GroupType = GroupingType.Order },
				new GroupingNode{ Name = "Контрагент", GroupType = GroupingType.Counterparty },
				new GroupingNode{ Name = "Подразделение", GroupType = GroupingType.Subdivision },
				new GroupingNode{ Name = "Дата доставки", GroupType = GroupingType.DeliveryDate },
				new GroupingNode{ Name = "Маршрутный лист", GroupType = GroupingType.RouteList },
				new GroupingNode{ Name = "Номенклатура", GroupType = GroupingType.Nomenclature },
				new GroupingNode{ Name = "Группа уровень 1", GroupType = GroupingType.NomenclatureGroup1 },
				new GroupingNode{ Name = "Группа уровень 2", GroupType = GroupingType.NomenclatureGroup2 },
				new GroupingNode{ Name = "Группа уровень 3", GroupType = GroupingType.NomenclatureGroup3 }
			};
		}

		public class GroupingNode
		{
			public string Name { get; set; }
			public GroupingType GroupType { get; set; }
		}

		private void SetupFilter()
		{
			FilterViewModel.AddFilter<NomenclatureCategory>(config =>
			{
				config.IncludedElements.CollectionChanged += (s, e) => UpdateNomenclaturesSpecification();
				config.ExcludedElements.CollectionChanged += (s, e) => UpdateNomenclaturesSpecification();
			});

			FilterViewModel.AddFilter(_unitOfWork, _nomenclatureRepository);

			FilterViewModel.AddFilter(_unitOfWork, _productGroupRepository, config =>
			{
				config.IncludedElements.CollectionChanged += (s, e) => UpdateNomenclaturesSpecification();
				config.ExcludedElements.CollectionChanged += (s, e) => UpdateNomenclaturesSpecification();
			});

			FilterViewModel.AddFilter(_unitOfWork, _counterpartyRepository);

			FilterViewModel.AddFilter(_unitOfWork, _organizationRepository);

			FilterViewModel.AddFilter(_unitOfWork, _discountReasonRepository);

			FilterViewModel.AddFilter(_unitOfWork, _subdivisionRepository);

			if(!_userIsSalesRepresentative)
			{
				FilterViewModel.AddFilter(_unitOfWork, _employeeGenericRepository, config =>
				{
					config.Title = "Авторы заказов";
				});
			}

			FilterViewModel.AddFilter(_unitOfWork, _geographicalGroupRepository);

			FilterViewModel.AddFilter<PaymentType>(filterConfig =>
			{
				filterConfig.RefreshFunc = (filter) =>
				{
					var values = Enum.GetValues(typeof(PaymentType));

					filter.FilteredElements.Clear();

					var terminalValues = Enum.GetValues(typeof(PaymentByTerminalSource))
						.Cast<PaymentByTerminalSource>()
						.Where(x => string.IsNullOrWhiteSpace(FilterViewModel.CurrentSearchString)
							|| x.GetEnumTitle().ToLower().Contains(FilterViewModel.CurrentSearchString.ToLower()));

					var paymentValues = _paymentFromRepository.Get(_unitOfWork, paymentFrom =>
						(FilterViewModel.ShowArchived || !paymentFrom.IsArchive)
						&& (string.IsNullOrWhiteSpace(FilterViewModel.CurrentSearchString))
							|| paymentFrom.Name.ToLower().Like($"%{FilterViewModel.CurrentSearchString.ToLower()}%"));

					// Заполнение начального списка

					foreach(var value in values)
					{
						if(value is PaymentType enumElement
							&& !filter.HideElements.Contains(enumElement)
							&& (string.IsNullOrWhiteSpace(FilterViewModel.CurrentSearchString)
								|| enumElement.GetEnumTitle().ToLower().Contains(FilterViewModel.CurrentSearchString.ToLower())
								|| (enumElement == PaymentType.Terminal && terminalValues.Any())
								|| (enumElement == PaymentType.PaidOnline && paymentValues.Any())))
						{
							filter.FilteredElements.Add(new IncludeExcludeElement<PaymentType, PaymentType>()
							{
								Id = enumElement,
								Title = enumElement.GetEnumTitle(),
							});
						}
					}

					// Заполнение группы Терминал

					var terminalNode = filter.FilteredElements
						.Where(x => x.Number == nameof(PaymentType.Terminal))
						.FirstOrDefault();

					if(terminalValues.Any())
					{
						foreach(var value in terminalValues)
						{
							if(value is PaymentByTerminalSource enumElement)
							{
								terminalNode.Children.Add(new IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>()
								{
									Id = enumElement,
									Parent = terminalNode,
									Title = enumElement.GetEnumTitle(),
								});
							}
						}
					}

					// Заполнение подгруппы Оплачено онлайн

					var paidOnlineNode = filter.FilteredElements
						.Where(x => x.Number == nameof(PaymentType.PaidOnline))
						.FirstOrDefault();

					if(paymentValues.Any())
					{
						var paymentFromValues = paymentValues
							.Select(x => new IncludeExcludeElement<int, PaymentFrom>
							{
								Id = x.Id,
								Parent = paidOnlineNode,
								Title = x.Name,
							});

						foreach(var element in paymentFromValues)
						{
							paidOnlineNode.Children.Add(element);
						}
					}
				};

				filterConfig.GetReportParametersFunc = (filter) =>
				{
					var result = new Dictionary<string, object>();

					// Тип оплаты

					var includePaymentTypeValues = filter.IncludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentType, PaymentType>))
						.Select(x => x.Number)
						.ToArray();

					if(includePaymentTypeValues.Length > 0)
					{
						result.Add(typeof(PaymentType).Name + "_include", includePaymentTypeValues);
					}
					else
					{
						result.Add(typeof(PaymentType).Name + "_include", new object[] { "0" });
					}

					var excludePaymentTypeValues = filter.ExcludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentType, PaymentType>))
						.Select(x => x.Number)
						.ToArray();

					if(excludePaymentTypeValues.Length > 0)
					{
						result.Add(typeof(PaymentType).Name + "_exclude", excludePaymentTypeValues);
					}
					else
					{
						result.Add(typeof(PaymentType).Name + "_exclude", new object[] { "0" });
					}

					// Оплата по термииналу

					var includePaymentByTerminalSourceValues = filter.IncludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>))
						.Select(x => x.Number)
						.ToArray();

					if(includePaymentByTerminalSourceValues.Length > 0)
					{
						result.Add(typeof(PaymentByTerminalSource).Name + "_include", includePaymentByTerminalSourceValues);
					}
					else
					{
						result.Add(typeof(PaymentByTerminalSource).Name + "_include", new object[] { "0" });
					}

					var excludePaymentByTerminalSourceValues = filter.ExcludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>))
						.Select(x => x.Number)
						.ToArray();

					if(excludePaymentByTerminalSourceValues.Length > 0)
					{
						result.Add(typeof(PaymentByTerminalSource).Name + "_exclude", excludePaymentByTerminalSourceValues);
					}
					else
					{
						result.Add(typeof(PaymentByTerminalSource).Name + "_exclude", new object[] { "0" });
					}

					// Оплачено онлайн

					var includePaymentFromValues = filter.IncludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<int, PaymentFrom>))
						.Select(x => x.Number)
						.ToArray();

					if(includePaymentFromValues.Length > 0)
					{
						result.Add(typeof(PaymentFrom).Name + "_include", includePaymentFromValues);
					}
					else
					{
						result.Add(typeof(PaymentFrom).Name + "_include", new object[] { "0" });
					}

					var excludePaymentFromValues = filter.ExcludedElements
						.Where(x => x.GetType() == typeof(IncludeExcludeElement<int, PaymentFrom>))
						.Select(x => x.Number)
						.ToArray();

					if(excludePaymentFromValues.Length > 0)
					{
						result.Add(typeof(PaymentFrom).Name + "_exclude", excludePaymentFromValues);
					}
					else
					{
						result.Add(typeof(PaymentFrom).Name + "_exclude", new object[] { "0" });
					}

					return result;
				};
			});

			FilterViewModel.AddFilter(_unitOfWork, _promotionalSetRepository);

			var statusesToSelect = new[] {
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.WaitForPayment,
				OrderStatus.Closed };

			FilterViewModel.AddFilter<OrderStatus>(config =>
			{
				foreach(var element in config.FilteredElements)
				{
					if(statusesToSelect.Any(x => element.Number == x.GetEnumTitle()))
					{
						element.Include = true;
					}
				}
			});
		}
		private void UpdateNomenclaturesSpecification()
		{
			var nomenclauresFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Nomenclature>>();

			nomenclauresFilter.Specification = null;

			nomenclauresFilter.ClearIncludesCommand.Execute();
			nomenclauresFilter.ClearExcludesCommand.Execute();

			var nomenclatureCategoryFilter = FilterViewModel.GetFilter<IncludeExcludeEnumFilter<NomenclatureCategory>>();

			if(nomenclatureCategoryFilter != null)
			{
				var nomenclatureCategoryIncluded = nomenclatureCategoryFilter?.GetIncluded().ToArray();

				var nomenclatureCategoryExcluded = nomenclatureCategoryFilter?.GetExcluded().ToArray();

				if(nomenclatureCategoryIncluded.Length > 0)
				{
					nomenclauresFilter.Specification = nomenclauresFilter.Specification.CombineWith(nomenclature => nomenclatureCategoryIncluded.Contains(nomenclature.Category));
				}

				if(nomenclatureCategoryExcluded.Length > 0)
				{
					nomenclauresFilter.Specification = nomenclauresFilter.Specification.CombineWith(nomenclature => !nomenclatureCategoryExcluded.Contains(nomenclature.Category));
				}
			}

			var productGroupFilter = FilterViewModel.GetFilter<IncludeExcludeEntityWithHierarchyFilter<ProductGroup>>();

			if(productGroupFilter != null)
			{
				var productGroupIncluded = productGroupFilter.GetIncluded().ToArray();

				var productGroupExcluded = productGroupFilter.GetExcluded().ToArray();

				if(productGroupIncluded.Length > 0)
				{
					nomenclauresFilter.Specification = nomenclauresFilter.Specification.CombineWith(nomenclature => productGroupIncluded.Contains(nomenclature.ProductGroup.Id));
				}

				if(productGroupExcluded.Length > 0)
				{
					nomenclauresFilter.Specification = nomenclauresFilter.Specification.CombineWith(nomenclature => !productGroupExcluded.Contains(nomenclature.ProductGroup.Id));
				}
			}
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
				var modifier = new ProfitabilityReportModifier();
				modifier.Setup(groupParameters.Select(x => (GroupingType)x.Value));
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
