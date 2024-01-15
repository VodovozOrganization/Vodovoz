﻿using DateTimeHelpers;
using FluentNHibernate.Utils;
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
using QS.ViewModels.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.NHibernateProjections.Contacts;
using Vodovoz.NHibernateProjections.Goods;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Reports.Editing.Modifiers;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel : DialogTabViewModelBase
	{
		private readonly IIncludeExcludeSalesFilterFactory _includeExcludeSalesFilterFactory;
		private readonly ILeftRightListViewModelFactory _leftRightListViewModelFactory;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;

		private IncludeExludeFiltersViewModel _filterViewModel;

		private readonly bool _userIsSalesRepresentative;
		private readonly bool _userCanGetContactsInSalesReports;
		private DelegateCommand _showInfoCommand;
		private DateTime? _startDate;
		private DateTime? _endDate;
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
		private LeftRightListViewModel<GroupingNode> _groupViewModel;
		private bool _showContacts;
		private static OrderStatus[] _clientOneOrderStatuses = { OrderStatus.Canceled, OrderStatus.DeliveryCanceled, OrderStatus.NotDelivered };

		public TurnoverWithDynamicsReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ICommonServices commonServices,
			INavigationManager navigation,
			IncludeExludeFiltersViewModel filterViewModel,
			IIncludeExcludeSalesFilterFactory includeExcludeSalesFilterFactory,
			ILeftRightListViewModelFactory leftRightListViewModelFactory)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = commonServices.InteractiveService;
			_includeExcludeSalesFilterFactory = includeExcludeSalesFilterFactory ?? throw new ArgumentNullException(nameof(includeExcludeSalesFilterFactory));
			_leftRightListViewModelFactory = leftRightListViewModelFactory ?? throw new ArgumentNullException(nameof(leftRightListViewModelFactory));

			Title = "Отчет по оборачиваемости с динамикой";

			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();

			_userIsSalesRepresentative =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.User.IsSalesRepresentative)
				&& !_commonServices.UserService.GetCurrentUser().IsAdmin;

			_userCanGetContactsInSalesReports =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Report.Sales.CanGetContactsInSalesReports);

			StartDate = DateTime.Now.Date.AddDays(-6);
			EndDate = DateTime.Now.Date;

			_lastGenerationErrors = Enumerable.Empty<string>();

			ConfigureFilter();

			SetupGroupings();
		}

		public CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }

		public bool ShowContacts
		{
			get => _showContacts;
			set => SetField(ref _showContacts, value);
		}

		public bool CanShowContacts => _userCanGetContactsInSalesReports
			&& SelectedGroupings.Contains(GroupingType.Counterparty);

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
			ShowLastSale
			&& SelectedGroupings.LastOrDefault() == GroupingType.Nomenclature;

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

		public virtual LeftRightListViewModel<GroupingNode> GroupingSelectViewModel
		{
			get => _groupViewModel;
			set => SetField(ref _groupViewModel, value);
		}

		[PropertyChangedAlso(nameof(CanShowContacts))]
		[PropertyChangedAlso(nameof(CanShowResidueForNomenclaturesWithoutSales))]
		public IEnumerable<GroupingType> SelectedGroupings => GroupingSelectViewModel.GetRightItems().Select(x => x.GroupType);

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

			var additionalParams = new Dictionary<string, string>
			{
				{ "Самовывоз", "is_self_delivery" },
				{ "Клиенты с одним заказом", "with_one_order" },
			};

			_filterViewModel.AddFilter("Дополнительные фильтры", additionalParams);
		}

		private void SetupGroupings()
		{
			GroupingSelectViewModel = _leftRightListViewModelFactory.CreateSalesWithDynamicsReportGroupingsConstructor();

			GroupingSelectViewModel.RightItems.ListContentChanged += OnGroupingsRightItemsListContentChanged;
		}

		private void OnGroupingsRightItemsListContentChanged(object sender, EventArgs e)
		{
			if(SelectedGroupings.LastOrDefault() != GroupingType.Nomenclature)
			{
				ShowResidueForNomenclaturesWithoutSales = false;
			}
			OnPropertyChanged(nameof(SelectedGroupings));
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

			var includedTitles = FilterViewModel.GetIncludedElements<T>().Select(x => x.Title.Trim('\n'));
			var excludedTitles = FilterViewModel.GetExcludedElements<T>().Select(x => x.Title.Trim('\n'));

			if(includedTitles.Any())
			{
				stringBuilder.Append(title + " включены " +
					string.Join("\n\t", includedTitles));
			}

			if(excludedTitles.Any())
			{
				stringBuilder.Append(title + " исключены " +
					string.Join("\n\t", excludedTitles));
			}
		}

		public void GetBoolParamsValues(StringBuilder stringBuilder)
		{
			var includedBoolParams = FilterViewModel.GetFilter<IncludeExcludeBoolParamsFilter>();

			var includedList = includedBoolParams.FilteredElements.Where(x => x.Include).Select(x => x.Title);
			var excludedList = includedBoolParams.FilteredElements.Where(x => x.Exclude).Select(x => x.Title);

			if(includedList.Any())
			{
				stringBuilder.AppendLine(string.Concat(" Только: ", string.Join(", ", includedList)));
			}

			if(excludedList.Any())
			{
				stringBuilder.AppendLine(string.Concat(" Кроме: ", string.Join(", ", excludedList)));
			}
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
			GetParameterValues<CounterpartyCompositeClassification>(sb2);
			GetBoolParamsValues(sb2);

			filters = sb2.ToString().Trim('\n');

			var selectedGroupings = SelectedGroupings;

			if(!selectedGroupings.Any())
			{
				selectedGroupings = new List<GroupingType>() { GroupingType.Nomenclature };
			}

			return await Task.Run(() =>
			{
				return TurnoverWithDynamicsReport.Create(
					StartDate.Value,
					EndDate.Value,
					filters,
					selectedGroupings,
					SlicingType,
					MeasurementUnit,
					ShowDynamics,
					DynamicsIn,
					ShowLastSale,
					ShowResidueForNomenclaturesWithoutSales,
					ShowContacts,
					GetWarehouseBalance,
					GetData,
					cancellationToken);
			}, cancellationToken);
		}

		public void ExportReport(string path)
		{
			Report.Export(path);
		}

		private List<NomenclatureStockNode> GetWarehouseBalance(List<int> nomenclatureIds)
		{
			if(!ShowLastSale || nomenclatureIds.Count < 1)
			{
				return new List<NomenclatureStockNode>();
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
				.Where(Restrictions.In(Projections.Property(() => nomenclatureAlias.Id), nomenclatureIds))
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(balanceProjection).WithAlias(() => resultAlias.Stock))
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockNode>())
				.List<NomenclatureStockNode>()
				.ToList();

			return result;
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

			var nomenclaturesCategoriesFilter = FilterViewModel.GetFilter<IncludeExcludeEnumFilter<NomenclatureCategory>>();
			var includedNomenclatureCategories = nomenclaturesCategoriesFilter.GetIncluded().ToArray();
			var excludedNomenclatureCategories = nomenclaturesCategoriesFilter.GetExcluded().ToArray();

			var nomenclaturesFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Nomenclature>>();
			var includedNomenclatures = nomenclaturesFilter.GetIncluded().ToArray();
			var excludedNomenclatures = nomenclaturesFilter.GetExcluded().ToArray();

			var productGroupsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityWithHierarchyFilter<ProductGroup>>();
			var includedProductGroups = productGroupsFilter.GetIncluded().ToArray();
			var excludedProductGroups = productGroupsFilter.GetExcluded().ToArray();

			var counterpartiesFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Counterparty>>();
			var includedCounterparties = counterpartiesFilter.GetIncluded().ToArray();
			var excludedCounterparties = counterpartiesFilter.GetExcluded().ToArray();

			#region CounterpartyTypes

			var includedCounterpartyTypeElements = FilterViewModel.GetIncludedElements<CounterpartyType>();
			var excludedCounterpartyTypeElements = FilterViewModel.GetExcludedElements<CounterpartyType>();

			var includedCounterpartyTypes = includedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<CounterpartyType, CounterpartyType>)
				.Select(x => (x as IncludeExcludeElement<CounterpartyType, CounterpartyType>).Id)
				.ToArray();

			var excludedCounterpartyTypes = excludedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<CounterpartyType, CounterpartyType>)
				.Select(x => (x as IncludeExcludeElement<CounterpartyType, CounterpartyType>).Id)
				.ToArray();

			var includedCounterpartySubtypes = includedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<int, CounterpartySubtype>)
				.Select(x => (x as IncludeExcludeElement<int, CounterpartySubtype>).Id)
				.ToArray();

			var excludedCounterpartySubtypes = excludedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<int, CounterpartySubtype>)
				.Select(x => (x as IncludeExcludeElement<int, CounterpartySubtype>).Id)
				.ToArray();

			#endregion CounterpartyTypes

			var organizationsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Organization>>();
			var includedOrganizations = organizationsFilter.GetIncluded().ToArray();
			var excludedOrganizations = organizationsFilter.GetExcluded().ToArray();

			var discountReasonsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<DiscountReason>>();
			var includedDiscountReasons = discountReasonsFilter.GetIncluded().ToArray();
			var excludedDiscountReasons = discountReasonsFilter.GetExcluded().ToArray();

			var subdivisionsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Subdivision>>();
			var includedSubdivisions = subdivisionsFilter.GetIncluded().ToArray();
			var excludedSubdivisions = subdivisionsFilter.GetExcluded().ToArray();

			var employeesFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Employee>>();
			var includedEmployees = employeesFilter.GetIncluded().ToArray();
			var excludedEmployees = employeesFilter.GetExcluded().ToArray();

			var geoGroupsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<GeoGroup>>();
			var includedGeoGroups = geoGroupsFilter.GetIncluded().ToArray();
			var excludedGeoGroups = geoGroupsFilter.GetExcluded().ToArray();

			#region PaymentTypes

			var includedPaymentTypeElements = FilterViewModel.GetIncludedElements<PaymentType>();
			var excludedPaymentTypeElements = FilterViewModel.GetExcludedElements<PaymentType>();

			var includedPaymentTypes = includedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<PaymentType, PaymentType>)
				.Select(x => (x as IncludeExcludeElement<PaymentType, PaymentType>).Id)
				.ToArray();

			var excludedPaymentTypes = excludedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<PaymentType, PaymentType>)
				.Select(x => (x as IncludeExcludeElement<PaymentType, PaymentType>).Id)
				.ToArray();

			var includedPaymentFroms = includedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<int, PaymentFrom>)
				.Select(x => (x as IncludeExcludeElement<int, PaymentFrom>).Id)
				.ToArray();

			var excludedPaymentFroms = excludedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<int, PaymentFrom>)
				.Select(x => (x as IncludeExcludeElement<int, PaymentFrom>).Id)
				.ToArray();

			var includedPaymentByTerminalSources = includedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>)
				.Select(x => (x as IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>).Id)
				.ToArray();

			var excludedPaymentByTerminalSources = excludedPaymentTypeElements
				.Where(x => x is IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>)
				.Select(x => (x as IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>).Id)
				.ToArray();

			#endregion PaymentTypes

			var promotionalSetsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<PromotionalSet>>();
			var includedPromotionalSets = promotionalSetsFilter.GetIncluded().ToArray();
			var excludedPromotionalSets = promotionalSetsFilter.GetExcluded().ToArray();

			var orderStatusesFilter = FilterViewModel.GetFilter<IncludeExcludeEnumFilter<OrderStatus>>();
			var includedOrderStatuses = orderStatusesFilter.GetIncluded().ToArray();
			var excludedOrderStatuses = orderStatusesFilter.GetExcluded().ToArray();

			var counterpartyClassificationsFilter = FilterViewModel.GetFilter<IncludeExcludeEnumFilter<CounterpartyCompositeClassification>>();
			var includedCounterpartyClassifications = counterpartyClassificationsFilter.GetIncluded().ToArray();
			var excludedCounterpartyClassifications = counterpartyClassificationsFilter.GetExcluded().ToArray();

			var includedBoolParams = FilterViewModel.GetFilter<IncludeExcludeBoolParamsFilter>();

			#endregion Сбор параметров

			Order orderAlias = null;
			Order orderCountAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			GeoGroup geographicGroupAlias = null;
			Employee authorAlias = null;
			Subdivision subdivisionAlias = null;
			PromotionalSet promotionalSetAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			Counterparty counterpartyAlias = null;
			CounterpartySubtype counterpartySubtypeAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			Organization organizationAlias = null;
			Phone phoneAlias = null;
			Phone orderContactPhoneAlias = null;
			Email emailAlias = null;
			PaymentFrom paymentFromAlias = null;
			CounterpartyClassification counterpartyClassificationAlias = null;

			TurnoverWithDynamicsReport.OrderItemNode resultNodeAlias = null;

			IList<TurnoverWithDynamicsReport.OrderItemNode> nomenclaturesEmptyNodes = new List<TurnoverWithDynamicsReport.OrderItemNode>();

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

				#region BoolParams

				foreach(var param in includedBoolParams.FilteredElements)
				{
					if(!param.Include && !param.Exclude)
					{
						continue;
					}

					switch(param.Number)
					{
						case "is_self_delivery":
							nomenclaturesEmptyQuery.Where(() => orderAlias.SelfDelivery == param.Include);
							break;
						case "with_one_order":
						{
							var subQueryOrdersCount = QueryOver.Of(() => orderCountAlias)
								.Where(() => orderCountAlias.Client.Id == counterpartyAlias.Id)
								.WhereRestrictionOn(() => orderCountAlias.OrderStatus).Not.IsIn(_clientOneOrderStatuses)
								.Select(Projections.GroupProperty(Projections.Property<Order>(o => o.Client.Id)));

							var countProjection = Projections.CountDistinct(() => orderCountAlias.Id);

							subQueryOrdersCount.Where(param.Include
								? Restrictions.Eq(countProjection, 1)
								: Restrictions.Not(Restrictions.Eq(countProjection, 1)));

							nomenclaturesEmptyQuery.WithSubquery
								.WhereProperty(() => counterpartyAlias.Id)
								.In(subQueryOrdersCount);
							
							break;
						}							
						default:
							throw new NotSupportedException(param.Number);
					}
				}

				#endregion

				nomenclaturesEmptyNodes = nomenclaturesEmptyQuery
					.SelectList(list => list.SelectGroup(() => nomenclatureAlias.Id)
						.Select(Projections.Constant(0).WithAlias(() => resultNodeAlias.Id))
						.Select(Projections.Constant(0m).WithAlias(() => resultNodeAlias.Price))
						.Select(Projections.Constant(0m).WithAlias(() => resultNodeAlias.ActualSum))
						.Select(Projections.Constant(0m).WithAlias(() => resultNodeAlias.Count))
						.Select(Projections.Constant(0m).WithAlias(() => resultNodeAlias.ActualCount))
						.Select(Projections.Property(() => nomenclatureAlias.Id).WithAlias(() => resultNodeAlias.NomenclatureId))
						.Select(Projections.Property(() => nomenclatureAlias.OfficialName).WithAlias(() => resultNodeAlias.NomenclatureOfficialName))
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultNodeAlias.NomenclatureCategory)
						.Select(Projections.Constant(0).WithAlias(() => resultNodeAlias.OrderId))
						.Select(Projections.Max(() => orderAlias.DeliveryDate).WithAlias(() => resultNodeAlias.OrderDeliveryDate))
						.Select(Projections.Property(() => productGroupAlias.Id).WithAlias(() => resultNodeAlias.ProductGroupId))
						.Select(ProductGroupProjections.GetProductGroupNameWithEnclosureProjection().WithAlias(() => resultNodeAlias.ProductGroupName)))
					.SetTimeout(0)
					.TransformUsing(Transformers.AliasToBean<TurnoverWithDynamicsReport.OrderItemNode>()).ReadOnly().List<TurnoverWithDynamicsReport.OrderItemNode>();
			}

			var lastCalculationSettingsId = _unitOfWork.GetAll<CounterpartyClassification>()
				.Select(c => c.ClassificationCalculationSettingsId)
				.OrderByDescending(d => d)
				.FirstOrDefault();

			var query = _unitOfWork.Session.QueryOver(() => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.PromoSet, () => promotionalSetAlias)
				.JoinEntityAlias(() => orderAlias, () => orderItemAlias.Order.Id == orderAlias.Id)
				.Left.JoinAlias(() => orderAlias.ContactPhone, () => orderContactPhoneAlias)
				.Left.JoinAlias(() => orderAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => authorAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => counterpartyAlias.CounterpartySubtype, () => counterpartySubtypeAlias)
				.Left.JoinAlias(() => orderAlias.Contract, () => counterpartyContractAlias)
				.Left.JoinAlias(() => counterpartyContractAlias.Organization, () => organizationAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => orderAlias.PaymentByCardFrom, () => paymentFromAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicGroupAlias)
				.Inner.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.JoinEntityAlias(
					() => counterpartyClassificationAlias,
					() => counterpartyAlias.Id == counterpartyClassificationAlias.CounterpartyId
						&& counterpartyClassificationAlias.ClassificationCalculationSettingsId == lastCalculationSettingsId,
					JoinType.LeftOuterJoin);

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
						Projections.Property(() => emailAlias.Address),
						separator: ",\n"));

			var notDeliveredStatuses = RouteListItem.GetNotDeliveredStatuses();

			RouteListItem routeListItemAlias = null;

			var routeListIdSubquery = QueryOver.Of(() => routeListItemAlias)
				.Where(() => routeListItemAlias.Order.Id == orderAlias.Id)
				.AndRestrictionOn(() => routeListItemAlias.Status).Not.IsIn(notDeliveredStatuses)
				.Select(x => x.RouteList.Id)
				.Take(1);

			#region Classifications Restrictions

			var classificationByBottlesCountProjection =
				Projections.Property(() => counterpartyClassificationAlias.ClassificationByBottlesCount);

			var classificationByOrdersCountProjection =
				Projections.Property(() => counterpartyClassificationAlias.ClassificationByOrdersCount);

			var classificationIsAXRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.A),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.X));

			var classificationIsAYRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.A),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Y));

			var classificationIsAZRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.A),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Z));

			var classificationIsBXRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.B),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.X));

			var classificationIsBYRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.B),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Y));

			var classificationIsBZRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.B),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Z));

			var classificationIsCXRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.C),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.X));

			var classificationIsCYRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.C),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Y));

			var classificationIsCZRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.C),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Z));

			#endregion Classifications Restrictions

			var counterpartyClassificationProjection =
				Projections.Conditional(
					classificationIsAXRestriction,  Projections.Constant(CounterpartyCompositeClassification.AX),
						Projections.Conditional(classificationIsAYRestriction, Projections.Constant(CounterpartyCompositeClassification.AY),
						Projections.Conditional(classificationIsAZRestriction, Projections.Constant(CounterpartyCompositeClassification.AZ),
						Projections.Conditional(classificationIsBXRestriction, Projections.Constant(CounterpartyCompositeClassification.BX),
						Projections.Conditional(classificationIsBYRestriction, Projections.Constant(CounterpartyCompositeClassification.BY),
						Projections.Conditional(classificationIsBZRestriction, Projections.Constant(CounterpartyCompositeClassification.BZ),
						Projections.Conditional(classificationIsCXRestriction, Projections.Constant(CounterpartyCompositeClassification.CX),
						Projections.Conditional(classificationIsCYRestriction, Projections.Constant(CounterpartyCompositeClassification.CY),
						Projections.Conditional(classificationIsCZRestriction, Projections.Constant(CounterpartyCompositeClassification.CZ),
					Projections.Constant(CounterpartyCompositeClassification.New))))))))));

			#region filter parameters

			#region NomenclatureCategories

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

			#endregion NomenclatureCategories

			#region Nomenclatures

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

			#endregion Nomenclatures

			#region ProductGroups

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

			#endregion ProductGroups

			#region PaymentTypes

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

			#endregion PaymentTypes

			#region Counterparties

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

			#endregion Counterparties

			#region CounterpartyTypes

			if(includedCounterpartyTypes.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => counterpartyAlias.CounterpartyType),
					includedCounterpartyTypes));
			}

			if(excludedCounterpartyTypes.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => counterpartyAlias.CounterpartyType),
					excludedCounterpartyTypes)));
			}

			#endregion CounterpartyTypes

			#region CounterpartySubtypes

			if(includedCounterpartySubtypes.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => counterpartyAlias.CounterpartySubtype.Id),
					includedCounterpartySubtypes));
			}

			if(excludedCounterpartySubtypes.Any())
			{
				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => counterpartyAlias.CounterpartySubtype.Id),
						excludedCounterpartySubtypes)))
					.Add(Restrictions.IsNull(Projections.Property(() => counterpartyAlias.CounterpartySubtype.Id))));
			}

			#endregion CounterpartySubtypes

			#region Organizations

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

			#endregion Organizations

			#region DiscountReasons

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

			#endregion DiscountReasons

			#region Subdivisions

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

			#endregion Subdivisions

			#region Employees

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

			#endregion Employees

			#region GeoGroups

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

			#endregion GeoGroups

			#region PromotionalSets

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

			var promotinalSetNameProjection = Projections.Conditional(Restrictions.IsNotNull(Projections.Property(() => promotionalSetAlias.Name)),
				Projections.Property(() => promotionalSetAlias.Name),
				Projections.Constant("Без промонабора"));

			var promotinalSetIdProjection = Projections.Conditional(
				Restrictions.IsNull(Projections.Property(() => promotionalSetAlias.Id)),
				Projections.Constant(0), 
				Projections.Property(() => promotionalSetAlias.Id));

			#endregion PromotionalSets

			#region OrderStatuses

			if(includedOrderStatuses.Any())
			{
				query.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(includedOrderStatuses);
			}

			if(excludedPromotionalSets.Any())
			{
				query.WhereRestrictionOn(() => orderAlias.OrderStatus).Not.IsIn(excludedOrderStatuses);
			}

			#endregion OrderStatuses

			#region CounterpartyClassifications

			if(includedCounterpartyClassifications.Any())
			{
				var includeRestriction = Restrictions.Disjunction();

				foreach(var classification in includedCounterpartyClassifications)
				{
					if(classification == CounterpartyCompositeClassification.New)
					{
						includeRestriction.Add(
							Restrictions.IsNull(Projections.Property(() => counterpartyClassificationAlias.ClassificationByBottlesCount)));

						includeRestriction.Add(
							Restrictions.IsNull(Projections.Property(() => counterpartyClassificationAlias.ClassificationByOrdersCount)));

						continue;
					}

					includeRestriction.Add(Restrictions.And(
						Restrictions.Eq(
							Projections.Property(() => counterpartyClassificationAlias.ClassificationByBottlesCount),
							CounterpartyClassification.ConvertToClassificationByBottlesCount(classification)),
						Restrictions.Eq(
							Projections.Property(() => counterpartyClassificationAlias.ClassificationByOrdersCount),
							CounterpartyClassification.ConvertToClassificationByOrdersCount(classification))));
				}

				query.Where(includeRestriction);
			}

			if(excludedCounterpartyClassifications.Any())
			{
				var excludeRestriction = Restrictions.Conjunction();

				foreach(var classification in excludedCounterpartyClassifications)
				{
					if(classification == CounterpartyCompositeClassification.New)
					{
						excludeRestriction.Add(
							Restrictions.IsNotNull(Projections.Property(() => counterpartyClassificationAlias.ClassificationByBottlesCount)));

						excludeRestriction.Add(
							Restrictions.IsNotNull(Projections.Property(() => counterpartyClassificationAlias.ClassificationByOrdersCount)));

						continue;
					}

					excludeRestriction.Add(Restrictions.Disjunction()
						.Add(Restrictions.Not(Restrictions.Eq(
								Projections.Property(() => counterpartyClassificationAlias.ClassificationByBottlesCount),
								CounterpartyClassification.ConvertToClassificationByBottlesCount(classification))))
						.Add(Restrictions.Not(Restrictions.Eq(
								Projections.Property(() => counterpartyClassificationAlias.ClassificationByOrdersCount),
								CounterpartyClassification.ConvertToClassificationByOrdersCount(classification))))
						.Add(Restrictions.IsNull(Projections.Property(() => counterpartyClassificationAlias.ClassificationByBottlesCount)))
						.Add(Restrictions.IsNull(Projections.Property(() => counterpartyClassificationAlias.ClassificationByOrdersCount))));
				}

				query.Where(excludeRestriction);
			}

			#endregion CounterpartyClassifications

			#region BoolParams

			foreach(var param in includedBoolParams.FilteredElements)
			{
				if(!param.Include && !param.Exclude)
				{
					continue;
				}

				switch(param.Number)
				{
					case "is_self_delivery":
						query.Where(() => orderAlias.SelfDelivery == param.Include);
						break;
					case "with_one_order":
					{
						var subQueryOrdersCount = QueryOver.Of(() => orderCountAlias)
							.Where(() => orderCountAlias.Client.Id == counterpartyAlias.Id)
							.WhereRestrictionOn(() => orderCountAlias.OrderStatus).Not.IsIn(_clientOneOrderStatuses)
							.Select(Projections.GroupProperty(Projections.Property<Order>(o => o.Client.Id)));

						var countProjection = Projections.CountDistinct(() => orderCountAlias.Id);

						subQueryOrdersCount.Where(param.Include
							? Restrictions.Eq(countProjection, 1)
							: Restrictions.Not(Restrictions.Eq(countProjection, 1)));

						query.WithSubquery
							.WhereProperty(() => counterpartyAlias.Id)
							.In(subQueryOrdersCount);
						
						break;
					}
					default:
						throw new NotSupportedException(param.Number);
				}
			}

			#endregion

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
						.Select(() => nomenclatureAlias.OfficialName).WithAlias(() => resultNodeAlias.NomenclatureOfficialName)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultNodeAlias.NomenclatureCategory)
						.Select(() => counterpartyAlias.Id).WithAlias(() => resultNodeAlias.CounterpartyId)
						.Select(() => counterpartyAlias.CounterpartyType).WithAlias(() => resultNodeAlias.CounterpartyType)
						.Select(() => counterpartySubtypeAlias.Id).WithAlias(() => resultNodeAlias.CounterpartySubtypeId)
						.Select(() => counterpartySubtypeAlias.Name).WithAlias(() => resultNodeAlias.CounterpartySubtype)
						.SelectSubQuery(counterpartyPhonesSubquery).WithAlias(() => resultNodeAlias.CounterpartyPhones)
						.SelectSubQuery(counterpartyEmailsSubquery).WithAlias(() => resultNodeAlias.CounterpartyEmails)
						.Select(Projections.Conditional(
							Restrictions.IsNull(Projections.Property(() => orderAlias.ContactPhone)),
							Projections.Constant(string.Empty),
							PhoneProjections.GetOrderContactDigitNumberLeadsWith8())).WithAlias(() => resultNodeAlias.OrderContactPhone)
						.Select(() => counterpartyAlias.FullName).WithAlias(() => resultNodeAlias.CounterpartyFullName)
						.Select(() => organizationAlias.Id).WithAlias(() => resultNodeAlias.OrganizationId)
						.Select(() => organizationAlias.Name).WithAlias(() => resultNodeAlias.OrganizationName)
						.Select(() => subdivisionAlias.Id).WithAlias(() => resultNodeAlias.SubdivisionId)
						.Select(() => subdivisionAlias.Name).WithAlias(() => resultNodeAlias.SubdivisionName)
						.Select(() => orderAlias.PaymentType).WithAlias(() => resultNodeAlias.PaymentType)
						.Select(() => orderAlias.Id).WithAlias(() => resultNodeAlias.OrderId)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultNodeAlias.OrderDeliveryDate)
						.SelectSubQuery(routeListIdSubquery).WithAlias(() => resultNodeAlias.RouteListId)
						.Select(() => productGroupAlias.Id).WithAlias(() => resultNodeAlias.ProductGroupId)
						.Select(counterpartyClassificationProjection).WithAlias(() => resultNodeAlias.CounterpartyClassification)
						.Select(ProductGroupProjections.GetProductGroupNameWithEnclosureProjection()).WithAlias(() => resultNodeAlias.ProductGroupName)
						.Select(promotinalSetIdProjection).WithAlias(() => resultNodeAlias.PromotionalSetId)
						.Select(promotinalSetNameProjection).WithAlias(() => resultNodeAlias.PromotionalSetName))
				.SetTimeout(0)
				.TransformUsing(Transformers.AliasToBean<TurnoverWithDynamicsReport.OrderItemNode>())
				.List<TurnoverWithDynamicsReport.OrderItemNode>();

			return nomenclaturesEmptyNodes.Union(result).ToList();
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
