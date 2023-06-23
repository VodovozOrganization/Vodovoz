using ClosedXML.Report;
using DateTimeHelpers;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.SqlCommand;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.NHibernateProjections.Contacts;
using Vodovoz.NHibernateProjections.Goods;
using Vodovoz.NHibernateProjections.Orders;
using static Vodovoz.ViewModels.Reports.Sales.TurnoverWithDynamicsReportViewModel.TurnoverWithDynamicsReport;
using Order = Vodovoz.Domain.Orders.Order;
using VodovozCounterparty = Vodovoz.Domain.Client.Counterparty;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel : DialogTabViewModelBase
	{
		private const string _includeSuffix = "_include";
		private const string _excludeSuffix = "_exclude";

		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;

		private readonly string _templatePath = @".\Reports\Sales\TurnoverReport.xlsx";
		private readonly string _templateWithDynamicsPath = @".\Reports\Sales\TurnoverWithDynamicsReport.xlsx";
		private readonly string _templateFinancePath = @".\Reports\Sales\TurnoverFinanceReport.xlsx";
		private readonly string _templateWithDynamicsFinancePath = @".\Reports\Sales\TurnoverWithDynamicsFinanceReport.xlsx";
		private readonly string _templateByCounterpartyPath = @".\Reports\Sales\TurnoverByCounterpartyReport.xlsx";
		private readonly string _templateByCounterpartyWithDynamicsPath = @".\Reports\Sales\TurnoverByCounterpartyWithDynamicsReport.xlsx";
		private readonly string _templateByCounterpartyFinancePath = @".\Reports\Sales\TurnoverByCounterpartyFinanceReport.xlsx";
		private readonly string _templateByCounterpartyWithDynamicsFinancePath = @".\Reports\Sales\TurnoverByCounterpartyWithDynamicsFinanceReport.xlsx";
		
		private readonly SelectableParametersReportFilter _filter;
		private readonly bool _userIsSalesRepresentative;
		private SelectableParameterReportFilterViewModel _filterViewModel;
		private DelegateCommand _showInfoCommand;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private GroupingByEnum _groupingBy;
		private DateTimeSliceType _slice;
		private MeasurementUnitEnum _measurementUnit;
		private DynamicsInEnum _dynamicsIn;
		private bool _showDynamics;
		private bool _showLastSale;
		private bool _showResidueForNomenclaturesWithoutSales;
		private TurnoverWithDynamicsReport _report;
		private bool _isSaving;
		private bool _canSave;
		private bool _isGenerating;
		private bool _canCancelGenerate;
		private IEnumerable<string> _lastGenerationErrors;

		public TurnoverWithDynamicsReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ICommonServices commonServices,
			INavigationManager navigation,
			IEmployeeRepository employeeRepository)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = commonServices.InteractiveService;

			Title = "Отчет по оборачиваемости с динамикой";

			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();

			_filter = new SelectableParametersReportFilter(_unitOfWork);

			_userIsSalesRepresentative =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("user_is_sales_representative")
				&& !_commonServices.UserService.GetCurrentUser(_unitOfWork).IsAdmin;

			StartDate = DateTime.Now.Date.AddDays(-6);
			EndDate = DateTime.Now.Date;

			_lastGenerationErrors = Enumerable.Empty<string>();

			ConfigureFilter();
		}

		public CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }

		public virtual SelectableParameterReportFilterViewModel FilterViewModel
		{
			get => _filterViewModel;
			set => SetField(ref _filterViewModel, value);
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

		public bool ShowDynamics
		{
			get => _showDynamics;
			set => SetField(ref _showDynamics, value);
		}

		[PropertyChangedAlso(nameof(CanShowResidueForNomenclaturesWithoutSales))]
		public GroupingByEnum GroupingBy
		{
			get => _groupingBy;
			set
			{
				if(SetField(ref _groupingBy, value)
					&& value == GroupingByEnum.Counterparty)
				{
					ShowResidueForNomenclaturesWithoutSales = false;
				}
			}
		}

		public MeasurementUnitEnum MeasurementUnit
		{
			get => _measurementUnit;
			set => SetField(ref _measurementUnit, value);
		}

		public DateTimeSliceType SlicingType
		{
			get => _slice;
			set => SetField(ref _slice, value);
		}

		public DynamicsInEnum DynamicsIn
		{
			get => _dynamicsIn;
			set => SetField(ref _dynamicsIn, value);
		}

		[PropertyChangedAlso(nameof(CanShowResidueForNomenclaturesWithoutSales))]
		public bool ShowLastSale
		{
			get => _showLastSale;
			set
			{
				if(SetField(ref _showLastSale, value) && !value)
				{
					ShowResidueForNomenclaturesWithoutSales = false;
				}
			}
		}

		public bool CanShowResidueForNomenclaturesWithoutSales =>
			ShowLastSale && GroupingBy == GroupingByEnum.Nomenclature;

		public bool ShowResidueForNomenclaturesWithoutSales
		{
			get => _showResidueForNomenclaturesWithoutSales;
			set => SetField(ref _showResidueForNomenclaturesWithoutSales, value);
		}

		public TurnoverWithDynamicsReport Report
		{
			get => _report;
			set
			{
				SetField(ref _report, value);
				CanSave = _report != null;
			}
		}

		public bool CanSave
		{
			get => _canSave;
			set => SetField(ref _canSave, value);
		}

		public bool IsSaving
		{
			get => _isSaving;
			set
			{
				SetField(ref _isSaving, value);
				CanSave = !IsSaving;
			}
		}

		public bool CanGenerate => !IsGenerating;

		public bool CanCancelGenerate
		{
			get => _canCancelGenerate;
			set => SetField(ref _canCancelGenerate, value);
		}

		public bool IsGenerating
		{
			get => _isGenerating;
			set
			{
				SetField(ref _isGenerating, value);
				OnPropertyChanged(nameof(CanGenerate));
				CanCancelGenerate = IsGenerating;
			}
		}

		public IEnumerable<string> LastGenerationErrors
		{
			get => _lastGenerationErrors;
			set => SetField(ref _lastGenerationErrors, value);
		}

		public DelegateCommand ShowInfoCommand
		{
			get
			{
				if(_showInfoCommand is null)
				{
					_showInfoCommand = new DelegateCommand(ShowInfo);
				}
				return _showInfoCommand;
			}
		}

		public async Task<TurnoverWithDynamicsReport> ActionGenerateReport(CancellationToken cancellationToken)
		{
			try
			{
				var report = await Generate(cancellationToken);
				return report;
			}
			finally
			{
				UoW.Session.Clear();
			}
		}

		private void ConfigureFilter()
		{
			var nomenclatureTypeParam = _filter.CreateParameterSet(
				"Типы номенклатур",
				nameof(NomenclatureCategory),
				new ParametersEnumFactory<NomenclatureCategory>());

			var nomenclatureParam = _filter.CreateParameterSet(
				"Номенклатуры",
				nameof(Nomenclature),
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<Nomenclature>()
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
				}));

			nomenclatureParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() =>
				{
					var selectedValues = nomenclatureTypeParam.GetSelectedValues().ToArray();
					return !selectedValues.Any()
						? null
						: nomenclatureTypeParam.FilterType == SelectableFilterType.Include
							? Restrictions.On<Nomenclature>(x => x.Category).IsIn(selectedValues)
							: Restrictions.On<Nomenclature>(x => x.Category).Not.IsIn(selectedValues);
				});

			//Предзагрузка. Для избежания ленивой загрузки
			_unitOfWork.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			_filter.CreateParameterSet(
				"Группы товаров",
				nameof(ProductGroup),
				new RecursiveParametersFactory<ProductGroup>(
					_unitOfWork,
					(filters) =>
					{
						var query = _unitOfWork.Session.QueryOver<ProductGroup>()
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
					x => x.Childs));

			_filter.CreateParameterSet(
				"Контрагенты",
				nameof(VodovozCounterparty),
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<VodovozCounterparty> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<VodovozCounterparty>()
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
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<VodovozCounterparty>>());
					return query.List<SelectableParameter>();
				}));

			_filter.CreateParameterSet(
				"Организации",
				nameof(Organization),
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<Organization> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<Organization>();
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
				}));

			_filter.CreateParameterSet(
				"Основания скидок",
				nameof(DiscountReason),
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<DiscountReason> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<DiscountReason>();
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
				}));

			_filter.CreateParameterSet(
				"Подразделения",
				nameof(Subdivision),
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<Subdivision> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<Subdivision>();
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
				}));

			if(!_userIsSalesRepresentative)
			{
				_filter.CreateParameterSet(
					"Авторы заказов",
					nameof(Employee),
					new ParametersFactory(_unitOfWork, (filters) =>
					{
						SelectableEntityParameter<Employee> resultAlias = null;
						var query = _unitOfWork.Session.QueryOver<Employee>();

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
					}));
			}

			_filter.CreateParameterSet(
				"Части города",
				nameof(GeoGroup),
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<GeoGroup> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<GeoGroup>();

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
				}));


			_filter.CreateParameterSet(
				"Тип оплаты",
				nameof(CombinedPaymentNode),
				new RecursiveParametersFactory<CombinedPaymentNode>(
					_unitOfWork,
					(filters) =>
					{
						var paymentFromList = _unitOfWork.Session.QueryOver<PaymentFrom>().List();
						var paymentFromCombinedNodes = paymentFromList.Select(x => new CombinedPaymentNode { Id = x.Id, Title = x.Name }).ToList();

						var paymentTypeList = Enum.GetValues(typeof(PaymentType)).Cast<PaymentType>();
						var combinedNodesTypesWithChildFroms = paymentTypeList.Select(x => new CombinedPaymentNode { Title = x.GetEnumTitle(), PaymentType = x, IsTopLevel = true }).ToList();
						combinedNodesTypesWithChildFroms.Single(x => x.PaymentType == PaymentType.PaidOnline).Childs = paymentFromCombinedNodes;

						var paymentByTerminalSources = Enum.GetValues(typeof(PaymentByTerminalSource)).Cast<PaymentByTerminalSource>();
						var paymentByTerminalSourceNodes = paymentByTerminalSources.Select(x => new CombinedPaymentNode { Title = x.GetEnumTitle() }).ToList();
						combinedNodesTypesWithChildFroms.Single(x => x.PaymentType == PaymentType.Terminal).Childs = paymentByTerminalSourceNodes;

						return combinedNodesTypesWithChildFroms;
					},
					x => x.Title,
					x => x.Childs,
					true));

			_filter.CreateParameterSet(
				"Промонаборы",
				nameof(PromotionalSet),
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<PromotionalSet> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<PromotionalSet>()
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
				}));

			FilterViewModel = new SelectableParameterReportFilterViewModel(_filter);
		}

		private void ShowInfo()
		{
			var info = "1. Подсчет отчёта по оборачиваемости с динамикой ведется на основе заказов. В отчёте учитываются заказы со статусами:\r\n" +
				"    'Принят'\r\n" +
				"    'В маршрутном листе'\r\n" +
				"    'На погрузке'\r\n" +
				"    'В пути'\r\n" +
				"    'Доставлен'\r\n" +
				"    'Выгрузка на складе'\r\n" +
				"    'Закрыт'\r\n" +
				"    'Ожидание оплаты' и заказ - самовывоз с оплатой после отгрузки.\r\n" +
				"В отчет не попадают заказы, являющиеся закрывашками по контракту.\r\n" +
				"Фильтр по дате отсекает заказы, если дата доставки не входит в выбранный период.\r\n" +
				"2. Настройки отчёта:\r\n" +
				"«В разрезе» - Выбор разбивки по периодам. В отчет попадают периоды согласно выбранного разреза, но не выходя за границы выставленного периода.\r\n" +
				"«Единица измерения» - величина, в которой будет сформирован отчёт, а именно в штуках или рублях.\r\n" +
				"«В динамике» - показывает изменения по отношению к предыдущему столбцу, в процентах или ед. измерения.\r\n" +
				"«Показать последнюю продажу» - добавляется информация о дате последней продажи, кол-ве дней от последней продажи до текущей даты, остатках по всем складам на текущую дату.\r\n" +
				"2.1 В отчете доступна группировка по:\r\n" +
				"    'Номенклатура'\r\n" +
				"    'Контрагент'\r\n" +
				"2.2 При выборе группировки по \"Контрагенту\"\r\n" +
				"    - В отчете выводится имя контрагента и дополнительный столбец \"Телефоны контрагента\"\r\n" +
				"    - Галочка \"Показывать товары на остатках без продаж\" становится недоступной для выбора\r\n" +
				"    - При выборе галочки \"Показывать последнюю продажу\" не выводится в отчете последний столбец \"Остатки по всем складам\"";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}

		public void ShowWarning(string message)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Warning, message);
		}

		private string GetSelectedParametersTitles(IDictionary<string, string> selectedParametersTitles, StringBuilder sb)
		{
			sb.Clear();

			var notSetValues = new string[]{
				"Все",
				"Нет"
			};

			if(selectedParametersTitles.Any())
			{
				foreach(var item in selectedParametersTitles)
				{
					if(!notSetValues.Contains(item.Value))
					{
						sb.AppendLine($"{item.Key}{item.Value.Trim('\n', '\r')}");
					}
				}
			}

			return sb.ToString().TrimEnd('\n');
		}

		public async Task<TurnoverWithDynamicsReport> Generate(CancellationToken cancellationToken)
		{
			var errors = ValidateParameters();
			if(errors.Any())
			{
				LastGenerationErrors = errors;
				IsGenerating = false;
				ReportGenerationCancelationTokenSource.Cancel();
			}

			var filters = string.Empty;

			var sb2 = new StringBuilder();

			var sb = new StringBuilder();

			sb2.Append(GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(NomenclatureCategory)), sb));
			sb2.Append(GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(Nomenclature)), sb));
			sb2.Append(GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(ProductGroup)), sb));
			sb2.Append(GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(VodovozCounterparty)), sb));
			sb2.Append(GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(Organization)), sb));
			sb2.Append(GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(DiscountReason)), sb));
			sb2.Append(GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(Subdivision)), sb));
			sb2.Append(GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(Employee)), sb));
			sb2.Append(GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(GeoGroup)), sb));
			sb2.Append(GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(PaymentType)), sb));
			sb2.Append(GetSelectedParametersTitles(_filter.GetSelectedParametersTitlesFromParameterSet(nameof(PromotionalSet)), sb));

			filters = sb2.ToString().Trim('\n');

			return await Task.Run(() =>
			{
				return TurnoverWithDynamicsReport.Create(
					StartDate.Value,
					EndDate.Value,
					filters,
					GroupingBy,
					SlicingType,
					MeasurementUnit,
					ShowDynamics,
					DynamicsIn,
					ShowLastSale,
					ShowResidueForNomenclaturesWithoutSales,
					GetWarehouseBalance,
					GetData);
			}, cancellationToken);
		}

		public void ExportReport(string path)
		{
			string templatePath = GetTrmplatePath();

			var template = new XLTemplate(templatePath);

			template.AddVariable(Report);
			template.Generate();

			template.SaveAs(path);
		}

		private string GetTrmplatePath()
		{
			if(Report.GroupingBy == GroupingByEnum.Nomenclature)
			{
				if(Report.ShowDynamics)
				{
					if(Report.MeasurementUnit == MeasurementUnitEnum.Amount)
					{
						return _templateWithDynamicsPath;
					}
					else
					{
						return _templateWithDynamicsFinancePath;
					}
				}
				else
				{
					if(Report.MeasurementUnit == MeasurementUnitEnum.Amount)
					{
						return _templatePath;
					}
					else
					{
						return _templateFinancePath;
					}
				}
			}
			else if(Report.GroupingBy == GroupingByEnum.Counterparty)
			{
				if(Report.ShowDynamics)
				{
					if(Report.MeasurementUnit == MeasurementUnitEnum.Amount)
					{
						return _templateByCounterpartyWithDynamicsPath;
					}
					else
					{
						return _templateByCounterpartyWithDynamicsFinancePath;
					}
				}
				else
				{
					if(Report.MeasurementUnit == MeasurementUnitEnum.Amount)
					{
						return _templateByCounterpartyPath;
					}
					else
					{
						return _templateByCounterpartyFinancePath;
					}
				}
			}
			throw new InvalidOperationException("Что-то пошло не так. Не достижимая ветка ветвления");
		}

		private decimal GetWarehouseBalance(int nomenclatureId)
		{
			if(!ShowLastSale)
			{
				return 0;
			}
			
			WarehouseBulkGoodsAccountingOperation operationAlias = null;
			Nomenclature nomenclatureAlias = null;
			NomenclatureStockNode resultAlias = null;

			var balanceProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0)"),
				NHibernateUtil.Decimal,
				Projections.Sum(() => operationAlias.Amount));

			var result = _unitOfWork.Session.QueryOver(() => nomenclatureAlias)
				.JoinEntityAlias(() => operationAlias,
					() => nomenclatureAlias.Id == operationAlias.Nomenclature.Id,
					JoinType.LeftOuterJoin)
				.Where(() => nomenclatureAlias.Id == nomenclatureId)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(balanceProjection).WithAlias(() => resultAlias.Stock))
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockNode>())
				.SingleOrDefault<NomenclatureStockNode>();

			return result.Stock;
		}

		private IList<TurnoverWithDynamicsReport.OrderItemNode> GetData(TurnoverWithDynamicsReport report)
		{
			var filterOrderStatusInclude = new OrderStatus[]
			{
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};

			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			GeoGroup geographicGroupAlias = null;
			Employee authorAlias = null;
			PromotionalSet promotionalSetAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			VodovozCounterparty counterpartyAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			Phone phoneAlias = null;
			Phone orderContactPhoneAlias = null;
			Email emailAlias = null;

			OrderItemNode resultNodeAlias = null;

			var parameters = _filter.GetParameters();

			IList<OrderItemNode> nomenclaturesEmptyNodes = new List<OrderItemNode>();

			if(ShowResidueForNomenclaturesWithoutSales)
			{
				var nomenclaturesEmptyQuery = _unitOfWork.Session.QueryOver(() => nomenclatureAlias)
					.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
					.JoinEntityAlias(() => orderItemAlias, () => orderItemAlias.Nomenclature.Id == nomenclatureAlias.Id, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.JoinEntityAlias(() => orderAlias, () => orderItemAlias.Order.Id == orderAlias.Id, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.Where(() => !nomenclatureAlias.IsArchive)
					.And(Restrictions.Le(Projections.Property(() => orderAlias.DeliveryDate), EndDate));

				if(parameters.ContainsKey(nameof(NomenclatureCategory) + _includeSuffix)
				&& parameters[nameof(NomenclatureCategory) + _includeSuffix] is object[] nomenclatureTypesInclude2
				&& nomenclatureTypesInclude2[0] != "0")
				{
					nomenclaturesEmptyQuery.Where(Restrictions.In(
						Projections.Property(() => nomenclatureAlias.Category),
						nomenclatureTypesInclude2));
				}

				if(parameters.ContainsKey(nameof(NomenclatureCategory) + _excludeSuffix)
					&& parameters[nameof(NomenclatureCategory) + _excludeSuffix] is object[] nomenclatureTypesExclude2
					&& nomenclatureTypesExclude2[0] != "0")
				{
					nomenclaturesEmptyQuery.Where(Restrictions.Not(Restrictions.In(
						Projections.Property(() => nomenclatureAlias.Category),
						nomenclatureTypesExclude2)));
				}

				if(parameters.ContainsKey(nameof(Nomenclature) + _includeSuffix)
					&& parameters[nameof(Nomenclature) + _includeSuffix] is object[] nomenclaturesInclude2
					&& nomenclaturesInclude2[0] != "0")
				{
					nomenclaturesEmptyQuery.Where(Restrictions.In(
						Projections.Property(() => nomenclatureAlias.Id),
						nomenclaturesInclude2));
				}

				if(parameters.ContainsKey(nameof(Nomenclature) + _excludeSuffix)
					&& parameters[nameof(Nomenclature) + _excludeSuffix] is object[] nomenclaturesExclude2
					&& nomenclaturesExclude2[0] != "0")
				{
					nomenclaturesEmptyQuery.Where(Restrictions.Not(Restrictions.In(
						Projections.Property(() => nomenclatureAlias.Id),
						nomenclaturesExclude2)));
				}

				if(parameters.ContainsKey(nameof(ProductGroup) + _includeSuffix)
					&& parameters[nameof(ProductGroup) + _includeSuffix] is object[] productGroupsInclude2
					&& productGroupsInclude2[0] != "0")
				{
					nomenclaturesEmptyQuery.Where(Restrictions.In(
						Projections.Property(() => productGroupAlias.Id),
						productGroupsInclude2));
				}

				if(parameters.ContainsKey(nameof(ProductGroup) + _excludeSuffix)
					&& parameters[nameof(ProductGroup) + _excludeSuffix] is object[] productGroupsExclude2
					&& productGroupsExclude2[0] != "0")
				{
					nomenclaturesEmptyQuery.Where(Restrictions.Disjunction()
						.Add(Restrictions.Not(Restrictions.In(
							Projections.Property(() => productGroupAlias.Id),
							productGroupsExclude2)))
						.Add(Restrictions.IsNull(Projections.Property(() => productGroupAlias.Id))));
				}

				nomenclaturesEmptyNodes = nomenclaturesEmptyQuery.SelectList(list => list.SelectGroup(() => nomenclatureAlias.Id)
							.Select(Projections.Constant(0).WithAlias(() => resultNodeAlias.Id))
							.Select(Projections.Constant(0m).WithAlias(() => resultNodeAlias.Price))
							.Select(Projections.Constant(0m).WithAlias(() => resultNodeAlias.ActualSum))
							.Select(Projections.Constant(0m).WithAlias(() => resultNodeAlias.Count))
							.Select(Projections.Constant(0m).WithAlias(() => resultNodeAlias.ActualCount))
							.Select(Projections.Property(() => nomenclatureAlias.Id).WithAlias(() => resultNodeAlias.NomenclatureId))
							.Select(Projections.Property(() => nomenclatureAlias.OfficialName).WithAlias(() => resultNodeAlias.NomenclatureOfficialName))
							.Select(Projections.Constant(0).WithAlias(() => resultNodeAlias.OrderId))
							.Select(Projections.Max(() => orderAlias.DeliveryDate).WithAlias(() => resultNodeAlias.OrderDeliveryDate))
							.Select(Projections.Property(() => productGroupAlias.Id).WithAlias(() => resultNodeAlias.ProductGroupId))
							.Select(ProductGroupProjections.GetProductGroupNameWithEnclosureProjection().WithAlias(() => resultNodeAlias.ProductGroupName)))
					.SetTimeout(0)
					.TransformUsing(Transformers.AliasToBean<OrderItemNode>()).ReadOnly().List<OrderItemNode>();
			}

			var query = _unitOfWork.Session.QueryOver(() => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.PromoSet, () => promotionalSetAlias)
				.JoinEntityAlias(() => orderAlias, () => orderItemAlias.Order.Id == orderAlias.Id)
				.Left.JoinAlias(() => orderAlias.ContactPhone, () => orderContactPhoneAlias)
				.Left.JoinAlias(() => orderAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => orderAlias.Contract, () => counterpartyContractAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicGroupAlias)
				.Inner.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias);

			var counterpartyPhonesSubquery = QueryOver.Of(() => phoneAlias)
				.Where(() => phoneAlias.Counterparty.Id == orderAlias.Client.Id)
				.AndNot(() => phoneAlias.IsArchive)
				.Select(
					CustomProjections.GroupConcat(
						PhoneProjections.GetDigitNumberLeadsWith8(),
						separator: ",\n"));

			var counterpartyEmailsSubquery = QueryOver.Of(() => emailAlias)
				.Where(() => emailAlias.Counterparty.Id == orderAlias.Client.Id)
				.Select(
					CustomProjections.GroupConcat(
						Projections.Property(()=>emailAlias.Address),
						separator: ",\n"));

			#region filter parameters
			if(parameters.ContainsKey(nameof(NomenclatureCategory) + _includeSuffix)
				&& parameters[nameof(NomenclatureCategory) + _includeSuffix] is object[] nomenclatureTypesInclude
				&& nomenclatureTypesInclude[0] != "0")
			{
				query.Where(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Category),
					nomenclatureTypesInclude));
			}

			if(parameters.ContainsKey(nameof(NomenclatureCategory) + _excludeSuffix)
				&& parameters[nameof(NomenclatureCategory) + _excludeSuffix] is object[] nomenclatureTypesExclude
				&& nomenclatureTypesExclude[0] != "0")
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Category),
					nomenclatureTypesExclude)));
			}

			if(parameters.ContainsKey(nameof(Nomenclature) + _includeSuffix)
				&& parameters[nameof(Nomenclature) + _includeSuffix] is object[] nomenclaturesInclude
				&& nomenclaturesInclude[0] != "0")
			{
				query.Where(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Id),
					nomenclaturesInclude));
			}

			if(parameters.ContainsKey(nameof(Nomenclature) + _excludeSuffix)
				&& parameters[nameof(Nomenclature) + _excludeSuffix] is object[] nomenclaturesExclude
				&& nomenclaturesExclude[0] != "0")
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Id),
					nomenclaturesExclude)));
			}

			if(parameters.ContainsKey(nameof(ProductGroup) + _includeSuffix)
				&& parameters[nameof(ProductGroup) + _includeSuffix] is object[] productGroupsInclude
				&& productGroupsInclude[0] != "0")
			{
				query.Where(Restrictions.In(
					Projections.Property(() => productGroupAlias.Id),
					productGroupsInclude));
			}

			if(parameters.ContainsKey(nameof(ProductGroup) + _excludeSuffix)
				&& parameters[nameof(ProductGroup) + _excludeSuffix] is object[] productGroupsExclude
				&& productGroupsExclude[0] != "0")
			{
				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => productGroupAlias.Id),
						productGroupsExclude)))
					.Add(Restrictions.IsNull(Projections.Property(() => productGroupAlias.Id))));
			}

			if(parameters.ContainsKey(nameof(CombinedPaymentNode) + _includeSuffix) 
			   && parameters[nameof(CombinedPaymentNode) + _includeSuffix] is object[] paymentNodesInclude
			   && paymentNodesInclude[0] != "0")
			{
				var paymentNodes = paymentNodesInclude.Cast<CombinedPaymentNode>().ToList();
				var paymentTypes = paymentNodes.Where(x => x.IsTopLevel).Select(x => x.PaymentType).ToArray();
				var paymentCardForms = paymentNodes.Where(x => !x.IsTopLevel).Select(x => x.Id).ToArray();

				query.Where(
					Restrictions.Disjunction()
						.Add(Restrictions.In(Projections.Property(() => orderAlias.PaymentType), paymentTypes))
						.Add(Restrictions.In(Projections.Property(() => orderAlias.PaymentByCardFrom), paymentCardForms))
					);
			}

			if(parameters.ContainsKey(nameof(CombinedPaymentNode) + _excludeSuffix) 
			   && parameters[nameof(CombinedPaymentNode) + _excludeSuffix] is object[] paymentNodesExclude
			   && paymentNodesExclude[0] != "0")
			{
				var paymentNodes = paymentNodesExclude.Cast<CombinedPaymentNode>().ToList();
				var paymentTypes = paymentNodes.Where(x => x.IsTopLevel).Select(x => x.PaymentType).ToArray();
				var paymentCardForms = paymentNodes.Where(x => !x.IsTopLevel).Select(x => x.Id).ToArray();

				query.Where(
					Restrictions.Not(
						Restrictions.Disjunction()
							.Add(Restrictions.Conjunction()
								.Add(Restrictions.IsNotNull(Projections.Property(() => orderAlias.PaymentByCardFrom)))
								.Add(Restrictions.In(Projections.Property(() => orderAlias.PaymentByCardFrom), paymentCardForms)))
							.Add(Restrictions.In(Projections.Property(() => orderAlias.PaymentType), paymentTypes))
						)
					);
			}

			if(parameters.ContainsKey(nameof(VodovozCounterparty) + _includeSuffix)
				&& parameters[nameof(VodovozCounterparty) + _includeSuffix] is object[] counterpartiesInclude
				&& counterpartiesInclude[0] != "0")
			{
				query.Where(Restrictions.In(
					Projections.Property(() => orderAlias.Client.Id),
					counterpartiesInclude));
			}

			if(parameters.ContainsKey(nameof(VodovozCounterparty) + _excludeSuffix)
				&& parameters[nameof(VodovozCounterparty) + _excludeSuffix] is object[] counterpartiesExclude
				&& counterpartiesExclude[0] != "0")
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => orderAlias.Client.Id),
					counterpartiesExclude)));
			}

			if(parameters.ContainsKey(nameof(Organization) + _includeSuffix)
				&& parameters[nameof(Organization) + _includeSuffix] is object[] organizationsInclude
				&& organizationsInclude[0] != "0")
			{
				query.Where(Restrictions.In(
					Projections.Property(() => counterpartyContractAlias.Organization.Id),
					organizationsInclude));
			}

			if(parameters.ContainsKey(nameof(Organization) + _excludeSuffix)
				&& parameters[nameof(Organization) + _excludeSuffix] is object[] organizationsExclude
				&& organizationsExclude[0] != "0")
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => counterpartyContractAlias.Organization.Id),
					organizationsExclude)));
			}

			if(parameters.ContainsKey(nameof(DiscountReason) + _includeSuffix)
				&& parameters[nameof(DiscountReason) + _includeSuffix] is object[] discountReasonsInclude
				&& discountReasonsInclude[0] != "0")
			{
				query.Where(Restrictions.In(
					Projections.Property(() => orderItemAlias.DiscountReason.Id),
					discountReasonsInclude));
			}

			if(parameters.ContainsKey(nameof(DiscountReason) + _excludeSuffix)
				&& parameters[nameof(DiscountReason) + _excludeSuffix] is object[] discountReasonsExclude
				&& discountReasonsExclude[0] != "0")
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => orderItemAlias.DiscountReason.Id),
					discountReasonsExclude)));
			}

			if(parameters.ContainsKey(nameof(Subdivision) + _includeSuffix)
				&& parameters[nameof(Subdivision) + _includeSuffix] is object[] subdivisionsInclude
				&& subdivisionsInclude[0] != "0")
			{
				query.Where(Restrictions.In(
					Projections.Property(() => authorAlias.Subdivision.Id),
					subdivisionsInclude));
			}

			if(parameters.ContainsKey(nameof(Subdivision) + _excludeSuffix)
				&& parameters[nameof(Subdivision) + _excludeSuffix] is object[] subdivisionsExclude
				&& subdivisionsExclude[0] != "0")
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => authorAlias.Subdivision.Id),
					subdivisionsExclude)));
			}

			if(parameters.ContainsKey(nameof(Employee) + _includeSuffix)
				&& parameters[nameof(Employee) + _includeSuffix] is object[] authorsInclude
				&& authorsInclude[0] != "0")
			{
				query.Where(Restrictions.In(
					Projections.Property(() => authorAlias.Id),
					authorsInclude));
			}

			if(parameters.ContainsKey(nameof(Employee) + _excludeSuffix)
				&& parameters[nameof(Employee) + _excludeSuffix] is object[] authorsExclude
				&& authorsExclude[0] != "0")
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => authorAlias.Id),
					authorsExclude)));
			}

			if(parameters.ContainsKey(nameof(GeoGroup) + _includeSuffix)
				&& parameters[nameof(GeoGroup) + _includeSuffix] is object[] geographicGroupsInclude
				&& geographicGroupsInclude[0] != "0")
			{
				query.Where(Restrictions.In(
					Projections.Property(() => geographicGroupAlias.Id),
					geographicGroupsInclude));
			}

			if(parameters.ContainsKey(nameof(GeoGroup) + _excludeSuffix)
				&& parameters[nameof(GeoGroup) + _excludeSuffix] is object[] geographicGroupsExclude
				&& geographicGroupsExclude[0] != "0")
			{
				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => geographicGroupAlias.Id),
						geographicGroupsExclude)))
					.Add(Restrictions.IsNull(Projections.Property(() => geographicGroupAlias.Id))));
			}

			if(parameters.ContainsKey(nameof(PaymentType) + _includeSuffix)
				&& parameters[nameof(PaymentType) + _includeSuffix] is object[] paymentTypesInclude
				&& paymentTypesInclude[0] != "0")
			{
				query.Where(Restrictions.In(
					Projections.Property(() => orderAlias.PaymentType),
					paymentTypesInclude));
			}

			if(parameters.ContainsKey(nameof(PaymentType) + _excludeSuffix)
				&& parameters[nameof(PaymentType) + _excludeSuffix] is object[] paymentTypesExclude
				&& paymentTypesExclude[0] != "0")
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => orderAlias.PaymentType),
					paymentTypesExclude)));
			}

			if(parameters.ContainsKey(nameof(PromotionalSet) + _includeSuffix)
				&& parameters[nameof(PromotionalSet) + _includeSuffix] is object[] promotionalSetsInclude
				&& promotionalSetsInclude[0] != "0")
			{
				query.Where(Restrictions.In(
					Projections.Property(() => promotionalSetAlias.Id),
					promotionalSetsInclude));
			}

			if(parameters.ContainsKey(nameof(PromotionalSet) + _excludeSuffix)
				&& parameters[nameof(PromotionalSet) + _excludeSuffix] is object[] promotionalSetsExclude
				&& promotionalSetsExclude[0] != "0")
			{

				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => promotionalSetAlias.Id),
						promotionalSetsExclude)))
					.Add(Restrictions.IsNull(Projections.Property(() => promotionalSetAlias.Id))));
			}
			#endregion

			var result = query.Where(GetOrderCriterion(filterOrderStatusInclude, orderAlias))
				.SelectList(list =>
					list.SelectGroup(() => orderItemAlias.Id)
						.Select(() => orderItemAlias.Id).WithAlias(() => resultNodeAlias.Id)
						.Select(() => orderItemAlias.Price).WithAlias(() => resultNodeAlias.Price)
						.Select(OrderProjections.GetOrderItemSumProjection()).WithAlias(() => resultNodeAlias.ActualSum)
						.Select(() => orderItemAlias.Count).WithAlias(() => resultNodeAlias.Count)
						.Select(() => orderItemAlias.ActualCount).WithAlias(() => resultNodeAlias.ActualCount)
						.Select(() => nomenclatureAlias.Id).WithAlias(() => resultNodeAlias.NomenclatureId)
						.Select(() => counterpartyAlias.Id).WithAlias(() => resultNodeAlias.CounterpartyId)
						.Select(() => counterpartyAlias.FullName).WithAlias(() => resultNodeAlias.CounterpartyFullName)
						.SelectSubQuery(counterpartyPhonesSubquery).WithAlias(() => resultNodeAlias.CounterpartyPhones)
						.SelectSubQuery(counterpartyEmailsSubquery).WithAlias(() => resultNodeAlias.CounterpartyEmails)
						.Select(Projections.Conditional(
							Restrictions.IsNull(Projections.Property(() => orderAlias.ContactPhone)),
							Projections.Constant(string.Empty),
							PhoneProjections.GetOrderContactDigitNumberLeadsWith8())).WithAlias(() => resultNodeAlias.OrderContactPhone)
						.Select(() => nomenclatureAlias.OfficialName).WithAlias(() => resultNodeAlias.NomenclatureOfficialName)
						.Select(() => orderAlias.Id).WithAlias(() => resultNodeAlias.OrderId)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultNodeAlias.OrderDeliveryDate)
						.Select(() => productGroupAlias.Id).WithAlias(() => resultNodeAlias.ProductGroupId)
						.Select(ProductGroupProjections.GetProductGroupNameWithEnclosureProjection()).WithAlias(() => resultNodeAlias.ProductGroupName))
				.SetTimeout(0)
				.TransformUsing(Transformers.AliasToBean<OrderItemNode>()).List<OrderItemNode>();

			return nomenclaturesEmptyNodes.Union(result.AsEnumerable()).ToList();
		}

		private AbstractCriterion GetOrderCriterion(OrderStatus[] filterOrderStatusInclude, Order orderAlias)
		{
			return Restrictions.And(
						Restrictions.And(
							Restrictions.Or(
								Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), filterOrderStatusInclude),
								Restrictions.And(
									Restrictions.Eq(Projections.Property(() => orderAlias.OrderStatus), OrderStatus.WaitForPayment),
									Restrictions.And(
										Restrictions.Eq(Projections.Property(() => orderAlias.SelfDelivery), true),
										Restrictions.Eq(Projections.Property(() => orderAlias.PayAfterShipment), true)))),
							Restrictions.NotEqProperty(Projections.Property(() => orderAlias.IsContractCloser), Projections.Constant(true))),
						Restrictions.Between(Projections.Property(() => orderAlias.DeliveryDate), StartDate, EndDate));
		}

		private IEnumerable<string> ValidateParameters()
		{
			if(StartDate == null || StartDate == default(DateTime)
			|| EndDate == null || EndDate == default(DateTime))
			{
				yield return "Заполните дату.";
			}

			var deltaTime = EndDate - StartDate;

			if(SlicingType == DateTimeSliceType.Day && deltaTime?.TotalDays >= 62)
			{
				yield return "Для разреза день нельзя выбрать интервал более 62х дней";
			}

			if((SlicingType == DateTimeSliceType.Week)
			&& (StartDate?.DayOfWeek == DayOfWeek.Monday ? deltaTime?.TotalDays / 7 >= 54 : deltaTime?.TotalDays / 7 > 54))
			{
				yield return "Для разреза неделя нельзя выбрать интервал более 54х недель";
			}

			var monthBetweenDates = 0;
			for(DateTime monthDate = StartDate.Value; monthDate < EndDate; monthDate = monthDate.AddMonths(1))
			{
				monthBetweenDates++;
			}

			if((SlicingType == DateTimeSliceType.Month)
			&& (StartDate?.Day == 1 ? monthBetweenDates >= 60 : monthBetweenDates > 60))
			{
				yield return "Для разреза месяц нельзя выбрать интервал более 60х месяцев";
			}
		}

		public class CombinedPaymentNode : IDomainObject
		{
			public bool IsTopLevel { get; set; }
			public PaymentType? PaymentType { get; set; }
			public int Id { get; set; }
			public string Title { get; set; }
			public IList<CombinedPaymentNode> Childs { get; set; }
		}

		public override void Dispose()
		{
			ReportGenerationCancelationTokenSource?.Dispose();
			base.Dispose();
		}
	}
}
