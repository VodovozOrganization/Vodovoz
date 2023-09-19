using ClosedXML.Report;
using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
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
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Extensions;
using Vodovoz.NHibernateProjections.Contacts;
using Vodovoz.NHibernateProjections.Goods;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Factories;
using static Vodovoz.ViewModels.Reports.Sales.TurnoverWithDynamicsReportViewModel.TurnoverWithDynamicsReport;
using Enum = System.Enum;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel : DialogTabViewModelBase
	{
		private readonly IIncludeExcludeSalesFilterFactory _includeExcludeSalesFilterFactory;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;

		private IncludeExludeFiltersViewModel _filterViewModel;

		private readonly string _templatePath = @".\Reports\Sales\TurnoverReport.xlsx";
		private readonly string _templateWithDynamicsPath = @".\Reports\Sales\TurnoverWithDynamicsReport.xlsx";
		private readonly string _templateFinancePath = @".\Reports\Sales\TurnoverFinanceReport.xlsx";
		private readonly string _templateWithDynamicsFinancePath = @".\Reports\Sales\TurnoverWithDynamicsFinanceReport.xlsx";
		private readonly string _templateByCounterpartyPath = @".\Reports\Sales\TurnoverByCounterpartyReport.xlsx";
		private readonly string _templateByCounterpartyWithDynamicsPath = @".\Reports\Sales\TurnoverByCounterpartyWithDynamicsReport.xlsx";
		private readonly string _templateByCounterpartyFinancePath = @".\Reports\Sales\TurnoverByCounterpartyFinanceReport.xlsx";
		private readonly string _templateByCounterpartyWithDynamicsFinancePath = @".\Reports\Sales\TurnoverByCounterpartyWithDynamicsFinanceReport.xlsx";
		private readonly string _templateByCounterpartyWithContactsPath = @".\Reports\Sales\TurnoverByCounterpartyWithContactsReport.xlsx";
		private readonly string _templateByCounterpartyWithDynamicsWithContactsPath = @".\Reports\Sales\TurnoverByCounterpartyWithContactsWithDynamicsReport.xlsx";
		private readonly string _templateByCounterpartyFinanceWithContactsPath = @".\Reports\Sales\TurnoverByCounterpartyWithContactsFinanceReport.xlsx";
		private readonly string _templateByCounterpartyWithDynamicsFinanceWithContactsPath = @".\Reports\Sales\TurnoverByCounterpartyWithContactsWithDynamicsFinanceReport.xlsx";

		private readonly bool _userIsSalesRepresentative;
		private readonly bool _userCanGetContactsInSalesReports;
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
			IncludeExludeFiltersViewModel filterViewModel,
			IIncludeExcludeSalesFilterFactory includeExcludeSalesFilterFactory)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = commonServices.InteractiveService;
			_includeExcludeSalesFilterFactory = includeExcludeSalesFilterFactory ?? throw new ArgumentNullException(nameof(includeExcludeSalesFilterFactory));

			Title = "Отчет по оборачиваемости с динамикой";

			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();

			_userIsSalesRepresentative =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.User.IsSalesRepresentative)
				&& !_commonServices.UserService.GetCurrentUser().IsAdmin;

			_userCanGetContactsInSalesReports =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Sales.CanGetContactsInSalesReports);

			StartDate = DateTime.Now.Date.AddDays(-6);
			EndDate = DateTime.Now.Date;

			_lastGenerationErrors = Enumerable.Empty<string>();

			ConfigureFilter();

		}

		public CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }

		public bool UserCanGetContactsInSalesReports => _userCanGetContactsInSalesReports;

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
					&& (value == GroupingByEnum.Counterparty || value == GroupingByEnum.CounterpartyShowContacts))
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

		public IncludeExludeFiltersViewModel FilterViewModel => _filterViewModel;

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
			_filterViewModel = _includeExcludeSalesFilterFactory.CreateSalesReportIncludeExcludeFilter(_unitOfWork, _userIsSalesRepresentative);
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

		public void GetParameterValues<T>(StringBuilder stringBuilder)
		{
			var title = typeof(T).GetClassUserFriendlyName().NominativePlural.CapitalizeSentence();

			stringBuilder.Append(title + " включены " +
				string.Join("\n\t", FilterViewModel.GetIncludedElements<T>().Select(x => x.Title.Trim('\n'))));
			stringBuilder.Append(title + " исключены " +
				string.Join("\n\t", FilterViewModel.GetExcludedElements<T>().Select(x => x.Title.Trim('\n'))));
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

			GetParameterValues<NomenclatureCategory>(sb2);
			GetParameterValues<Nomenclature>(sb2);
			GetParameterValues<ProductGroup>(sb2);
			GetParameterValues<Counterparty>(sb2);
			GetParameterValues<Organization>(sb2);
			GetParameterValues<DiscountReason>(sb2);
			GetParameterValues<Subdivision>(sb2);
			GetParameterValues<Employee>(sb2);
			GetParameterValues<GeoGroup>(sb2);
			GetParameterValues<PaymentType>(sb2);
			GetParameterValues<PromotionalSet>(sb2);

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
			else if(Report.GroupingBy == GroupingByEnum.CounterpartyShowContacts)
			{
				if(Report.ShowDynamics)
				{
					if(Report.MeasurementUnit == MeasurementUnitEnum.Amount)
					{
						return _templateByCounterpartyWithDynamicsWithContactsPath;
					}
					else
					{
						return _templateByCounterpartyWithDynamicsFinanceWithContactsPath;
					}
				}
				else
				{
					if(Report.MeasurementUnit == MeasurementUnitEnum.Amount)
					{
						return _templateByCounterpartyWithContactsPath;
					}
					else
					{
						return _templateByCounterpartyFinanceWithContactsPath;
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

			#region Сбор параметров

			var includedNomenclatureCategories = FilterViewModel.GetIncludedElements<NomenclatureCategory>().Select(x => Enum.Parse(typeof(NomenclatureCategory), x.Number)).Cast<NomenclatureCategory>().ToArray();
			var excludedNomenclatureCategories = FilterViewModel.GetExcludedElements<NomenclatureCategory>().Select(x => Enum.Parse(typeof(NomenclatureCategory), x.Number)).Cast<NomenclatureCategory>().ToArray();
			var includedNomenclatures = FilterViewModel.GetIncludedElements<Nomenclature>().Select(x => int.Parse(x.Number)).ToArray();
			var excludedNomenclatures = FilterViewModel.GetExcludedElements<Nomenclature>().Select(x => int.Parse(x.Number)).ToArray();
			var includedProductGroups = FilterViewModel.GetIncludedElements<ProductGroup>().Select(x => int.Parse(x.Number)).ToArray();
			var excludedProductGroups = FilterViewModel.GetExcludedElements<ProductGroup>().Select(x => int.Parse(x.Number)).ToArray();
			var includedCounterparties = FilterViewModel.GetIncludedElements<Counterparty>().Select(x => int.Parse(x.Number)).ToArray();
			var excludedCounterparties = FilterViewModel.GetExcludedElements<Counterparty>().Select(x => int.Parse(x.Number)).ToArray();
			var includedOrganizations = FilterViewModel.GetIncludedElements<Organization>().Select(x => int.Parse(x.Number)).ToArray();
			var excludedOrganizations = FilterViewModel.GetExcludedElements<Organization>().Select(x => int.Parse(x.Number)).ToArray();
			var includedDiscountReasons = FilterViewModel.GetIncludedElements<DiscountReason>().Select(x => int.Parse(x.Number)).ToArray();
			var excludedDiscountReasons = FilterViewModel.GetExcludedElements<DiscountReason>().Select(x => int.Parse(x.Number)).ToArray();
			var includedSubdivisions = FilterViewModel.GetIncludedElements<Subdivision>().Select(x => int.Parse(x.Number)).ToArray();
			var excludedSubdivisions = FilterViewModel.GetExcludedElements<Subdivision>().Select(x => int.Parse(x.Number)).ToArray();
			var includedEmployees = FilterViewModel.GetIncludedElements<Employee>().Select(x => int.Parse(x.Number)).ToArray();
			var excludedEmployees = FilterViewModel.GetExcludedElements<Employee>().Select(x => int.Parse(x.Number)).ToArray();
			var includedGeoGroups = FilterViewModel.GetIncludedElements<GeoGroup>().Select(x => int.Parse(x.Number)).ToArray();
			var excludedGeoGroups = FilterViewModel.GetExcludedElements<GeoGroup>().Select(x => int.Parse(x.Number)).ToArray();

			var includedPaymentTypeElements = FilterViewModel.GetIncludedElements<PaymentType>();
			var excludedPaymentTypeElements = FilterViewModel.GetExcludedElements<PaymentType>();

			var includedPaymentTypes = includedPaymentTypeElements.Where(x => x is IncludeExcludeElement<PaymentType, PaymentType>).Select(x => Enum.Parse(typeof(PaymentType), x.Number)).Cast<PaymentType>().ToArray();
			var excludedPaymentTypes = excludedPaymentTypeElements.Where(x => x is IncludeExcludeElement<PaymentType, PaymentType>).Select(x => Enum.Parse(typeof(PaymentType), x.Number)).Cast<PaymentType>().ToArray();

			var includedPaymentFroms = includedPaymentTypeElements.Where(x => x is IncludeExcludeElement<int, PaymentFrom>).Select(x => int.Parse(x.Number)).ToArray();
			var excludedPaymentFroms = excludedPaymentTypeElements.Where(x => x is IncludeExcludeElement<int, PaymentFrom>).Select(x => int.Parse(x.Number)).ToArray();

			var includedPaymentByTerminalSources = includedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>)
				.Select(x => Enum.Parse(typeof(PaymentByTerminalSource), x.Number))
				.Cast<PaymentByTerminalSource>()
				.ToArray();

			var excludedPaymentByTerminalSources = excludedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>)
				.Select(x => Enum.Parse(typeof(PaymentByTerminalSource), x.Number))
				.Cast<PaymentByTerminalSource>()
				.ToArray();

			var includedPromotionalSets = FilterViewModel.GetIncludedElements<PromotionalSet>().Select(x => int.Parse(x.Number)).ToArray();
			var excludedPromotionalSets = FilterViewModel.GetExcludedElements<PromotionalSet>().Select(x => int.Parse(x.Number)).ToArray();

			#endregion Сбор параметров

			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			GeoGroup geographicGroupAlias = null;
			Employee authorAlias = null;
			PromotionalSet promotionalSetAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			Counterparty counterpartyAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			Phone phoneAlias = null;
			Phone orderContactPhoneAlias = null;
			Email emailAlias = null;
			PaymentFrom paymentFromAlias = null;

			OrderItemNode resultNodeAlias = null;

			IList<OrderItemNode> nomenclaturesEmptyNodes = new List<OrderItemNode>();

			if(ShowResidueForNomenclaturesWithoutSales)
			{
				var nomenclaturesEmptyQuery = _unitOfWork.Session.QueryOver(() => nomenclatureAlias)
					.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
					.JoinEntityAlias(() => orderItemAlias, () => orderItemAlias.Nomenclature.Id == nomenclatureAlias.Id, JoinType.LeftOuterJoin)
					.JoinEntityAlias(() => orderAlias, () => orderItemAlias.Order.Id == orderAlias.Id, JoinType.LeftOuterJoin)
					.Where(Restrictions.Le(Projections.Property(() => orderAlias.DeliveryDate), EndDate));

				if(!FilterViewModel.ShowArchived)
				{
					nomenclaturesEmptyQuery.Where(() => !nomenclatureAlias.IsArchive);
				}

				if(includedNomenclatureCategories.Any())
				{
					nomenclaturesEmptyQuery.Where(Restrictions.In(
						Projections.Property(() => nomenclatureAlias.Category),
						includedNomenclatureCategories));
				}

				if(excludedNomenclatureCategories.Any())
				{
					nomenclaturesEmptyQuery.Where(Restrictions.Not(Restrictions.In(
						Projections.Property(() => nomenclatureAlias.Category),
						excludedNomenclatureCategories)));
				}

				if(includedNomenclatures.Any())
				{
					nomenclaturesEmptyQuery.Where(Restrictions.In(
						Projections.Property(() => nomenclatureAlias.Id),
						includedNomenclatures));
				}

				if(excludedNomenclatures.Any())
				{
					nomenclaturesEmptyQuery.Where(Restrictions.Not(Restrictions.In(
						Projections.Property(() => nomenclatureAlias.Id),
						excludedNomenclatures)));
				}

				if(includedProductGroups.Any())
				{
					nomenclaturesEmptyQuery.Where(Restrictions.In(
						Projections.Property(() => productGroupAlias.Id),
						includedProductGroups));
				}

				if(excludedProductGroups.Any())
				{
					nomenclaturesEmptyQuery.Where(Restrictions.Disjunction()
						.Add(Restrictions.Not(Restrictions.In(
							Projections.Property(() => productGroupAlias.Id),
								excludedProductGroups)))
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
				.Left.JoinAlias(() => orderAlias.PaymentByCardFrom, () => paymentFromAlias)
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

			if(includedNomenclatureCategories.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Category),
					includedNomenclatureCategories));
			}

			if(excludedNomenclatureCategories.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Category),
					excludedNomenclatureCategories)));
			}

			if(includedNomenclatures.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Id),
					includedNomenclatures));
			}

			if(excludedNomenclatures.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Id),
					excludedNomenclatures)));
			}

			if(includedProductGroups.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => productGroupAlias.Id),
					includedProductGroups));
			}

			if(excludedProductGroups.Any())
			{
				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => productGroupAlias.Id),
						excludedProductGroups)))
					.Add(Restrictions.IsNull(Projections.Property(() => productGroupAlias.Id))));
			}

			if(includedPaymentTypeElements.Any())
			{
				var paymentTerminalFormsRestriction =
					Restrictions.And(
						Restrictions.Eq(Projections.Property(() => orderAlias.PaymentType), PaymentType.Terminal),
						Restrictions.In(Projections.Property(() => orderAlias.PaymentByTerminalSource), includedPaymentByTerminalSources));

				var paymentFromRestriction =
					Restrictions.And(
						Restrictions.Eq(Projections.Property(() => orderAlias.PaymentType), PaymentType.PaidOnline),
						Restrictions.In(Projections.Property(() => paymentFromAlias.Id), includedPaymentFroms));

				query.Where(
					Restrictions.Disjunction()
						.Add(Restrictions.In(Projections.Property(() => orderAlias.PaymentType), includedPaymentTypes))
						.Add(paymentTerminalFormsRestriction)
						.Add(paymentFromRestriction));
			}

			if(excludedPaymentTypeElements.Any())
			{
				var paymentTerminalFormsRestriction =
					Restrictions.And(
						Restrictions.Eq(Projections.Property(() => orderAlias.PaymentType), PaymentType.Terminal),
						Restrictions.In(Projections.Property(() => orderAlias.PaymentByTerminalSource), excludedPaymentByTerminalSources));

				var paymentFromRestriction =
					Restrictions.And(
						Restrictions.Eq(Projections.Property(() => orderAlias.PaymentType), PaymentType.PaidOnline),
						Restrictions.In(Projections.Property(() => paymentFromAlias.Id), excludedPaymentFroms));

				query.Where(
					Restrictions.Not(
						Restrictions.Disjunction()
							.Add(Restrictions.In(Projections.Property(() => orderAlias.PaymentType), excludedPaymentTypes))
							.Add(paymentFromRestriction)
							.Add(paymentTerminalFormsRestriction)));
			}

			if(includedCounterparties.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => orderAlias.Client.Id),
					includedCounterparties));
			}

			if(excludedCounterparties.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => orderAlias.Client.Id),
					excludedCounterparties)));
			}

			if(includedOrganizations.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => counterpartyContractAlias.Organization.Id),
					includedOrganizations));
			}

			if(excludedOrganizations.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => counterpartyContractAlias.Organization.Id),
					excludedOrganizations)));
			}

			if(includedDiscountReasons.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => orderItemAlias.DiscountReason.Id),
					includedDiscountReasons));
			}

			if(excludedDiscountReasons.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => orderItemAlias.DiscountReason.Id),
					excludedDiscountReasons)));
			}

			if(includedSubdivisions.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => authorAlias.Subdivision.Id),
					includedSubdivisions));
			}

			if(excludedSubdivisions.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => authorAlias.Subdivision.Id),
					excludedSubdivisions)));
			}

			if(includedEmployees.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => authorAlias.Id),
					includedEmployees));
			}

			if(excludedEmployees.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => authorAlias.Id),
					excludedEmployees)));
			}

			if(includedGeoGroups.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => geographicGroupAlias.Id),
					includedGeoGroups));
			}

			if(excludedGeoGroups.Any())
			{
				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => geographicGroupAlias.Id),
						excludedGeoGroups)))
					.Add(Restrictions.IsNull(Projections.Property(() => geographicGroupAlias.Id))));
			}

			if(includedPromotionalSets.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => promotionalSetAlias.Id),
					includedPromotionalSets));
			}

			if(excludedPromotionalSets.Any())
			{
				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => promotionalSetAlias.Id),
						excludedPromotionalSets)))
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
			if(StartDate == null
				|| StartDate == default(DateTime)
				|| EndDate == null
				|| EndDate == default(DateTime))
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

		public override void Dispose()
		{
			ReportGenerationCancelationTokenSource?.Dispose();
			base.Dispose();
		}
	}
}
