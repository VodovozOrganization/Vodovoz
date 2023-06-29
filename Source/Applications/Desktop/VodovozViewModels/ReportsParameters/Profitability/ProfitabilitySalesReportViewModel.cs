using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.DB;
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
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.Reports.Editing;
using Vodovoz.Reports.Editing.Modifiers;
using Vodovoz.ViewModels.Reports;

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
		private readonly SelectableParametersReportFilter _filter;
		private SelectableParameterReportFilterViewModel _filterViewModel;
		private LeftRightListViewModel<GroupingNode> _groupViewModel;
		private readonly bool _userIsSalesRepresentative;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _uow;
		private DelegateCommand _loadReportCommand;
		private DelegateCommand _showInfoCommand;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _isDetailed;
		private string _source;

		public ProfitabilitySalesReportViewModel(RdlViewerViewModel rdlViewerViewModel, IEmployeeRepository employeeRepository, ICommonServices commonServices) : base(rdlViewerViewModel)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = commonServices.InteractiveService;

			Title = "Отчет по продажам с рентабельностью";

			_uow = UnitOfWorkFactory.CreateWithoutRoot();
			_filter = new SelectableParametersReportFilter(_uow);

			_userIsSalesRepresentative =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("user_is_sales_representative")
				&& !_commonServices.UserService.GetCurrentUser(_uow).IsAdmin;

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

		public virtual SelectableParameterReportFilterViewModel FilterViewModel
		{
			get => _filterViewModel;
			set => SetField(ref _filterViewModel, value);
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
			var nomenclatureTypeParam = _filter.CreateParameterSet(
				"Типы номенклатур",
				"nomenclature_type",
				new ParametersEnumFactory<NomenclatureCategory>()
			);

			var nomenclatureParam = _filter.CreateParameterSet(
				"Номенклатуры",
				"nomenclature",
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = _uow.Session.QueryOver<Nomenclature>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							var filterCriterion = f();
							if(filterCriterion != null)
							{
								query.Where(filterCriterion);
							}
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.OfficialName).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Nomenclature>>());
					return query.List<SelectableParameter>();
				})
			);

			nomenclatureParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() =>
				{
					var selectedValues = nomenclatureTypeParam.GetSelectedValues().ToArray();
					return !selectedValues.Any()
						? null
						: nomenclatureTypeParam.FilterType == SelectableFilterType.Include
							? Restrictions.On<Nomenclature>(x => x.Category).IsIn(selectedValues)
							: Restrictions.On<Nomenclature>(x => x.Category).Not.IsIn(selectedValues);
				}
			);

			
			ProductGroup productGroupChildAlias = null;
			//Предзагрузка. Для избежания ленивой загрузки
			_uow.Session.QueryOver<ProductGroup>()
				.Left.JoinAlias(p => p.Childs,
					() => productGroupChildAlias,
					() => !productGroupChildAlias.IsArchive)
				.Fetch(SelectMode.Fetch, () => productGroupChildAlias)
				.List();

			_filter.CreateParameterSet(
				"Группы товаров",
				"product_group",
				new RecursiveParametersFactory<ProductGroup>(_uow,
					(filters) =>
					{
						var query = _uow.Session.QueryOver<ProductGroup>()
							.Where(p => p.Parent == null)
							.And(p => !p.IsArchive);

						if(filters != null && filters.Any())
						{
							foreach(var f in filters)
							{
								query.Where(f());
							}
						}

						return query.List();
					},
					x => x.Name,
					x => x.Childs)
			);

			_filter.CreateParameterSet(
				"Контрагенты",
				"counterparty",
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<Counterparty> resultAlias = null;
					var query = _uow.Session.QueryOver<Counterparty>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.FullName).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Counterparty>>());
					return query.List<SelectableParameter>();
				})
			);

			_filter.CreateParameterSet(
				"Организации",
				"organization",
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<Organization> resultAlias = null;
					var query = _uow.Session.QueryOver<Organization>();
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.FullName).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Organization>>());
					return query.List<SelectableParameter>();
				})
			);

			_filter.CreateParameterSet(
				"Основания скидок",
				"discount_reason",
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<DiscountReason> resultAlias = null;
					var query = _uow.Session.QueryOver<DiscountReason>();
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<DiscountReason>>());
					return query.List<SelectableParameter>();
				})
			);

			_filter.CreateParameterSet(
				"Подразделения",
				"subdivision",
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<Subdivision> resultAlias = null;
					var query = _uow.Session.QueryOver<Subdivision>();
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Subdivision>>());
					return query.List<SelectableParameter>();
				})
			);

			if(!_userIsSalesRepresentative)
			{
				_filter.CreateParameterSet(
					"Авторы заказов",
					"order_author",
					new ParametersFactory(_uow, (filters) =>
					{
						SelectableEntityParameter<Employee> resultAlias = null;
						var query = _uow.Session.QueryOver<Employee>();

						if(filters != null && filters.Any())
						{
							foreach(var f in filters)
							{
								query.Where(f());
							}
						}

						var authorProjection = CustomProjections.Concat_WS(
							" ",
							Projections.Property<Employee>(x => x.LastName),
							Projections.Property<Employee>(x => x.Name),
							Projections.Property<Employee>(x => x.Patronymic)
						);

						query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(authorProjection).WithAlias(() => resultAlias.EntityTitle)
						);
						query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Employee>>());
						var paremetersSet = query.List<SelectableParameter>();

						return paremetersSet;
					})
				);
			}

			_filter.CreateParameterSet(
				"Части города",
				"geographic_group",
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<GeoGroup> resultAlias = null;
					var query = _uow.Session.QueryOver<GeoGroup>();

					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<GeoGroup>>());
					return query.List<SelectableParameter>();
				})
			);

			_filter.CreateParameterSet(
				"Тип оплаты",
				"payment_type",
				new RecursiveParametersFactory<CombinedPaymentNode>(
					_uow,
					(filters) =>
					{
						var paymentByTerminalSources = Enum.GetValues(typeof(PaymentByTerminalSource)).Cast<PaymentByTerminalSource>();
						var paymentByTerminalSourceNodes = paymentByTerminalSources.Select(x =>
							new CombinedPaymentNode
							{
								Id = (int)x,
								Title = x.GetEnumTitle(),
								PaymentType = PaymentType.Terminal,
								PaymentSubType = x.ToString(),
								TopLevelId = (int)PaymentType.Terminal
							})
						.ToList();

						var paymentTypeList = Enum.GetValues(typeof(PaymentType)).Cast<PaymentType>();
						var combinedNodesTypesWithChildFroms = paymentTypeList.Select(x =>
							new CombinedPaymentNode
							{
								Id = (int)x,
								Title = x.GetEnumTitle(),
								PaymentType = x,
								IsTopLevel = true
							})
						.ToList();

						combinedNodesTypesWithChildFroms.Single(x => x.PaymentType == PaymentType.Terminal).Childs = paymentByTerminalSourceNodes;

						return combinedNodesTypesWithChildFroms;
					},
					x => x.Title,
					x => x.Childs,
					true)
			);

			_filter.CreateParameterSet(
				"Промонаборы",
				"promotional_set",
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<PromotionalSet> resultAlias = null;
					var query = _uow.Session.QueryOver<PromotionalSet>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					query.SelectList(list => list
						.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
						.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
					);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<PromotionalSet>>());
					return query.List<SelectableParameter>();
				})
			);

			_filter.CreateParameterSet(
				"Статусы заказов",
				"order_status",
				new ParametersEnumFactory<OrderStatus>()
			);

			var statusesToSelect = new[] { 
				OrderStatus.Accepted, 
				OrderStatus.InTravelList, 
				OrderStatus.OnLoading, 
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.WaitForPayment,
				OrderStatus.Closed };
			var orderStatusFilter = _filter.ParameterSets.Single(x => x.ParameterName == "order_status");
			orderStatusFilter.UpdateOutputParameters();
			foreach(var filter in orderStatusFilter.OutputParameters)
			{
				if(!(filter.Value is OrderStatus))
				{
					throw new InvalidOperationException("Не правильно настроен фильтр статусов заказа. В фильтре присутствует объект отличающийся по типу от статуса заказа.");
				}
				var statusFilter = (OrderStatus)filter.Value;
				if(statusesToSelect.Contains(statusFilter))
				{
					filter.Selected = true;
				}
			}

			FilterViewModel = new SelectableParameterReportFilterViewModel(_filter);
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

			_parameters = new Dictionary<string, object>
			{
				{ "start_date", StartDate },
				{ "end_date", EndDate },
				{ "creation_date", DateTime.Now },
			};

			if(_userIsSalesRepresentative)
			{
				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(_uow);

				_parameters.Add("order_author_include", new[] { currentEmployee.Id.ToString() });
				_parameters.Add("order_author_exclude", new[] { "0" });
			}

			var groupParameters = GetGroupingParameters();
			foreach(var groupParameter in groupParameters)
			{
				_parameters.Add(groupParameter.Key, groupParameter.Value.ToString());
			}
			_parameters.Add("groups_count", groupParameters.Count());

			foreach(var item in _filter.GetParameters())
			{
				_parameters.Add(item.Key, item.Value);
			}

			UpdatePaymentTypeParameters(ref _parameters);

			_source = GetReportSource();
			LoadReport();
		}

		private void UpdatePaymentTypeParameters(ref Dictionary<string, object> parameters)
		{
			parameters.Add("payment_terminal_subtype_include", new string[] { "0" });
			parameters.Add("payment_terminal_subtype_exclude", new string[] { "0" });

			if(parameters["payment_type_include"] is IEnumerable<object> includedPaymentTypesObject)
			{
				var selectedPaymentTypes =
					includedPaymentTypesObject
					.Where(x => x is CombinedPaymentNode)
					.Select(x => (CombinedPaymentNode)x)
					.ToList();

				if(selectedPaymentTypes.Count > 0)
				{
					var topLevelSelectedPaymmentTypes = CombinedPaymentNode.GetTopLevelPaymentTypesNames(selectedPaymentTypes);

					parameters["payment_type_include"] =
						(topLevelSelectedPaymmentTypes.Length > 0)
						? topLevelSelectedPaymmentTypes
						: new string[] { "0" };

					var selectedTerminalSubtypes = CombinedPaymentNode.GetTerminalPaymentTypeSubtypesNames(selectedPaymentTypes);

					if(selectedTerminalSubtypes.Length > 0)
					{
						parameters["payment_terminal_subtype_include"] = selectedTerminalSubtypes;
					}
				}
			}

			if(parameters["payment_type_exclude"] is IEnumerable<object> excludedPaymentTypesObject)
			{
				var selectedPaymentTypes =
					excludedPaymentTypesObject
					.Where(x => x is CombinedPaymentNode)
					.Select(x => (CombinedPaymentNode)x)
					.ToList();

				if(selectedPaymentTypes.Count > 0)
				{
					var topLevelSelectedPaymmentTypes = CombinedPaymentNode.GetTopLevelPaymentTypesNames(selectedPaymentTypes);

					parameters["payment_type_exclude"] =
						(topLevelSelectedPaymmentTypes.Length > 0)
						? topLevelSelectedPaymmentTypes
						: new string[] { "0" };

					var selectedTerminalSubtypes = CombinedPaymentNode.GetTerminalPaymentTypeSubtypesNames(selectedPaymentTypes);

					if(selectedTerminalSubtypes.Length > 0)
					{
						parameters["payment_terminal_subtype_exclude"] = selectedTerminalSubtypes;
					}
				}
			}
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
