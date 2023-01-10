﻿using System;
using System.Globalization;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using VodovozOrder = Vodovoz.Domain.Orders.Order;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using QS.Project.Journal.DataLoader;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using QS.Project.Domain;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Controllers;
using Vodovoz.Domain;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.EntityRepositories.Subdivisions;
using QS.Project.Services.FileDialog;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Services;

namespace Vodovoz.JournalViewModels
{
	public class OrderJournalViewModel : FilterableMultipleEntityJournalViewModelBase<OrderJournalNode, OrderJournalFilterViewModel>
	{
		private const int _minLengthLikeSearch = 3;

		private readonly ICommonServices _commonServices;
		private readonly IEmployeeService _employeeService;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUserRepository _userRepository;
		private readonly ICounterpartyJournalFactory _counterpartySelectorFactory;
		private readonly bool _userHasAccessToRetail = false;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		private readonly IGtkTabsOpener _gtkDialogsOpener;
		private readonly IUndeliveredOrdersJournalOpener _undeliveredOrdersJournalOpener;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;
		private readonly bool _userHasOnlyAccessToWarehouseAndComplaints;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IFileDialogService _fileDialogService;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private readonly int _closingDocumentDeliveryScheduleId;

		public OrderJournalViewModel(
			OrderJournalFilterViewModel filterViewModel, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			IEmployeeService employeeService,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IGtkTabsOpener gtkDialogsOpener,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			ISubdivisionRepository subdivisionRepository,
			IFileDialogService fileDialogService,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			IDeliveryScheduleParametersProvider deliveryScheduleParametersProvider,
			IRDLPreviewOpener rdlPreviewOpener) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_deliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_gtkDialogsOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));

			_counterpartySelectorFactory = _counterpartyJournalFactory;

			_undeliveredOrdersJournalOpener =
				undeliveredOrdersJournalOpener ?? throw new ArgumentNullException(nameof(undeliveredOrdersJournalOpener));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			_undeliveredOrdersRepository =
				undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_closingDocumentDeliveryScheduleId =
				(deliveryScheduleParametersProvider ?? throw new ArgumentNullException(nameof(deliveryScheduleParametersProvider)))
				.ClosingDocumentDeliveryScheduleId;
			TabName = "Журнал заказов";

			_userHasAccessToRetail = commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");
			_userHasOnlyAccessToWarehouseAndComplaints =
				commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !commonServices.UserService.GetCurrentUser(UoW).IsAdmin;

			SearchEnabled = false;

			RegisterOrders();
			RegisterOrdersWithoutShipmentForDebt();
			RegisterOrdersWithoutShipmentForPayment();
			RegisterOrdersWithoutShipmentForAdvancePayment();

			var threadLoader = DataLoader as ThreadDataLoader<OrderJournalNode>;
			threadLoader.MergeInOrderBy(x => x.CreateDate, true);

			FinishJournalConfiguration();

			UpdateOnChanges(
				typeof(VodovozOrder),
				typeof(OrderWithoutShipmentForDebt),
				typeof(OrderWithoutShipmentForPayment),
				typeof(OrderWithoutShipmentForAdvancePayment),
				typeof(OrderWithoutShipmentForPaymentItem),
				typeof(OrderWithoutShipmentForAdvancePaymentItem),
				typeof(OrderItem)
			);
		}

        protected override void CreateNodeActions()
        {
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateCustomEditAction();
			CreateDefaultDeleteAction();
		}

		private void CreateCustomEditAction()
        {
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<OrderJournalNode>();
					if (selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					OrderJournalNode selectedNode = selectedNodes.First();
					if (!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => selected.All(x => (x as OrderJournalNode).Sensitive),
				(selected) => {
					if(!selected.All(x => (x as OrderJournalNode).Sensitive))
                    {
						return;
                    }
					var selectedNodes = selected.OfType<OrderJournalNode>();
					if (selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					OrderJournalNode selectedNode = selectedNodes.First();
					if (!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);
					if (foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				}
			);
			if (SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		private IQueryOver<VodovozOrder> GetOrdersQuery(IUnitOfWork uow)
		{
			OrderJournalNode resultAlias = null;
			VodovozOrder orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			District districtAlias = null;
			CounterpartyContract contractAlias = null;
			PaymentFrom paymentFromAlias = null;
			GeoGroup geographicalGroupAlias = null;

			Nomenclature sanitizationNomenclature = _nomenclatureRepository.GetSanitisationNomenclature(uow);

			var query = uow.Session.QueryOver<VodovozOrder>(() => orderAlias)
				.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicalGroupAlias);

			if (FilterViewModel.ViewTypes != ViewTypes.Order && FilterViewModel.ViewTypes != ViewTypes.All)
			{
				query.Where(o => o.Id == -1);
			}

			if(FilterViewModel.RestrictStatus != null) {
				query.Where(o => o.OrderStatus == FilterViewModel.RestrictStatus);
			}

			if(FilterViewModel.RestrictPaymentType != null) {
				query.Where(o => o.PaymentType == FilterViewModel.RestrictPaymentType);
			}

			if(FilterViewModel.HideStatuses != null) {
				query.WhereRestrictionOn(o => o.OrderStatus).Not.IsIn(FilterViewModel.HideStatuses);
			}

			if(FilterViewModel.RestrictOnlySelfDelivery != null) {
				query.Where(o => o.SelfDelivery == FilterViewModel.RestrictOnlySelfDelivery);
			}

			if(FilterViewModel.RestrictWithoutSelfDelivery != null) {
				query.Where(o => o.SelfDelivery != FilterViewModel.RestrictWithoutSelfDelivery);
			}

			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
			}

			if(FilterViewModel.DeliveryPoint != null) {
				query.Where(o => o.DeliveryPoint == FilterViewModel.DeliveryPoint);
			}

			if(FilterViewModel.Author != null)
			{
				query.Where(o => o.Author == FilterViewModel.Author);
			}

			if(FilterViewModel.StartDate != null)
			{
				if(FilterViewModel.FilterDateType == OrdersDateFilterType.DeliveryDate)
				{
					query.Where(o => o.DeliveryDate >= FilterViewModel.StartDate);
				}
				else 
				{ 
					query.Where(o => o.CreateDate >= FilterViewModel.StartDate); 
				}
			}

			if(FilterViewModel.EndDate != null)
			{
				if(FilterViewModel.FilterDateType == OrdersDateFilterType.DeliveryDate)
				{ 
					query.Where(o => o.DeliveryDate <= FilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));
				}
				else
				{ 
					query.Where(o => o.CreateDate <= FilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));
				}
			}

			if(FilterViewModel.RestrictLessThreeHours == true) {
				query.Where(Restrictions
							.GtProperty(Projections.SqlFunction(
											new SQLFunctionTemplate(NHibernateUtil.Time, "ADDTIME(?1, ?2)"),
											NHibernateUtil.Time,
											Projections.Property(() => deliveryScheduleAlias.From),
											Projections.Constant("3:0:0")),
											Projections.Property(() => deliveryScheduleAlias.To)));
			}

			if(FilterViewModel.RestrictHideService != null) 
			{
				if(FilterViewModel.RestrictHideService.Value)
				{
					query.Where(o => o.OrderAddressType != OrderAddressType.Service);
				}
				else
				{
					query.Where(o => o.OrderAddressType == OrderAddressType.Service);
				}
			}

			if(FilterViewModel.RestrictOnlyService != null) 
			{
				if(FilterViewModel.RestrictOnlyService.Value)
				{
					query.Where(o => o.OrderAddressType == OrderAddressType.Service);
				}
				else
				{
					query.Where(o => o.OrderAddressType != OrderAddressType.Service);
				}
			}

			if(FilterViewModel.OrderPaymentStatus != null) {
				query.Where(o => o.OrderPaymentStatus == FilterViewModel.OrderPaymentStatus);
			}

			if (FilterViewModel.Organisation != null) {
				query.Where(() => contractAlias.Organization.Id == FilterViewModel.Organisation.Id);
			}
			
			if (FilterViewModel.PaymentByCardFrom != null) {
				query.Where(o => o.PaymentByCardFrom.Id == FilterViewModel.PaymentByCardFrom.Id);
			}

			if (FilterViewModel.GeographicGroup != null)
			{
				query.Where(o => !o.SelfDelivery)
					.And(() => geographicalGroupAlias.Id == FilterViewModel.GeographicGroup.Id);
			}
			
			if(FilterViewModel.SortDeliveryDate != null)
			{
				if(FilterViewModel.SortDeliveryDate.Value)
				{
					query = query.OrderBy(o => o.DeliveryDate.Value).Desc;
				}
				else
				{
					query = query.OrderBy(o => o.Id).Desc;
				}
			}

			if(FilterViewModel.OrderId != null)
			{
				query.Where(() => orderAlias.Id == FilterViewModel.OrderId.Value);
			}

			if(FilterViewModel.OnlineOrderId != null)
			{
				query.Where(() => orderAlias.OnlineOrder == FilterViewModel.OnlineOrderId);
			}

			if(FilterViewModel.IsForSalesDepartment != null)
			{
				query.Where(() => counterpartyAlias.IsForSalesDepartment == FilterViewModel.IsForSalesDepartment.Value);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.CounterpartyPhone))
			{
				Phone counterpartyPhoneAlias = null;

				var counterpartyPhonesSubquery = QueryOver.Of<Phone>(() => counterpartyPhoneAlias)
					.Where(() => counterpartyPhoneAlias.Counterparty.Id == counterpartyAlias.Id)
					.And(() => counterpartyPhoneAlias.DigitsNumber == FilterViewModel.CounterpartyPhone)
					.And(() => !counterpartyPhoneAlias.IsArchive)
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(counterpartyPhonesSubquery.DetachedCriteria));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.DeliveryPointPhone))
			{
				Phone deliveryPointPhoneAlias = null;

				var deliveryPointPhonesSubquery = QueryOver.Of<Phone>(() => deliveryPointPhoneAlias)
					.Where(() => deliveryPointPhoneAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
					.And(() => deliveryPointPhoneAlias.DigitsNumber == FilterViewModel.DeliveryPointPhone)
					.And(() => !deliveryPointPhoneAlias.IsArchive)
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(deliveryPointPhonesSubquery.DetachedCriteria));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.CounterpartyNameLike) && FilterViewModel.CounterpartyNameLike.Length >= _minLengthLikeSearch)
			{
				query.Where(Restrictions.Like(Projections.Property(() => counterpartyAlias.FullName), FilterViewModel.CounterpartyNameLike, MatchMode.Anywhere));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.DeliveryPointAddressLike) && FilterViewModel.DeliveryPointAddressLike.Length >= _minLengthLikeSearch)
			{
				query.Where(Restrictions.Like(Projections.Property(() => deliveryPointAlias.CompiledAddress), FilterViewModel.DeliveryPointAddressLike, MatchMode.Anywhere));
			}
			
			if(FilterViewModel.ExcludeClosingDocumentDeliverySchedule)
			{
				query.Where(o => o.DeliverySchedule.Id == null || o.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId);
			}

			var bottleCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var sanitisationCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
													 .Where(() => orderAlias.Id == orderItemAlias.Order.Id)
													 .Where(() => orderItemAlias.Nomenclature.Id == sanitizationNomenclature.Id)
													 .Select(Projections.Sum(() => orderItemAlias.Count));

			var orderSumSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
											.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
											.Select(
												Projections.Sum(
													Projections.SqlFunction(
														new SQLFunctionTemplate(NHibernateUtil.Decimal, "ROUND(IFNULL(?1, ?2) * ?3 - ?4, 2)"),
														NHibernateUtil.Decimal,
														Projections.Property<OrderItem>(x => x.ActualCount),
														Projections.Property<OrderItem>(x => x.Count),
														Projections.Property<OrderItem>(x => x.Price),
														Projections.Property<OrderItem>(x => x.DiscountMoney)
													)
												)
											);
			query.Left.JoinAlias(o => o.DeliverySchedule, () => deliveryScheduleAlias)
					.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
					.Left.JoinAlias(o => o.Author, () => authorAlias)
					.Left.JoinAlias(o => o.LastEditor, () => lastEditorAlias)
					.Left.JoinAlias(o => o.Contract, () => contractAlias);
			
			query.Where(GetSearchCriterion(
				() => orderAlias.Id,
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.CompiledAddress,
				() => authorAlias.LastName,
				() => orderAlias.DriverCallId,
				() => orderAlias.OnlineOrder,
				() => orderAlias.EShopOrder,
				() => orderAlias.OrderPaymentStatus
			));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => orderAlias.SelfDelivery).WithAlias(() => resultAlias.IsSelfDelivery)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
					.Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.CreateDate)
					.Select(() => deliveryScheduleAlias.Name).WithAlias(() => resultAlias.DeliveryTime)
					.Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.StatusEnum)
					.Select(() => orderAlias.Address1c).WithAlias(() => resultAlias.Address1c)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorLastName)
					.Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
					.Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
					.Select(() => orderAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
					.Select(() => orderAlias.DriverCallId).WithAlias(() => resultAlias.DriverCallId)
					.Select(() => orderAlias.OnlineOrder).WithAlias(() => resultAlias.OnlineOrder)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
					.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompilledAddress)
					.Select(() => deliveryPointAlias.City).WithAlias(() => resultAlias.City)
					.Select(() => deliveryPointAlias.Street).WithAlias(() => resultAlias.Street)
					.Select(() => deliveryPointAlias.Building).WithAlias(() => resultAlias.Building)
					.Select(() => orderAlias.EShopOrder).WithAlias(() => resultAlias.EShopOrder)
					.Select(() => orderAlias.OrderPaymentStatus).WithAlias(() => resultAlias.OrderPaymentStatus)
					.Select(
						Projections.Conditional(
							Restrictions.Or(
								Restrictions.Eq(Projections.Constant(true), _userHasAccessToRetail),
								Restrictions.Not(Restrictions.Eq(Projections.Property(() => counterpartyAlias.IsForRetail), true))
								),
							Projections.Constant(true),
							Projections.Constant(false)
						)).WithAlias(() => resultAlias.Sensitive
					)
					.SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.Sum)
					.SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
					.SelectSubQuery(sanitisationCountSubquery).WithAlias(() => resultAlias.SanitisationAmount)
				)
				.OrderBy(x => x.CreateDate).Desc
				.SetTimeout(60)
				.TransformUsing(Transformers.AliasToBean<OrderJournalNode<VodovozOrder>>());

			return resultQuery;
		}

		private void RegisterOrders()
		{
			var ordersConfig = RegisterEntity<VodovozOrder>(GetOrdersQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() =>_gtkDialogsOpener.CreateOrderDlg(FilterViewModel.IsForRetail, FilterViewModel.IsForSalesDepartment),
					//функция диалога открытия документа
					(OrderJournalNode node) => _gtkDialogsOpener.CreateOrderDlg(node.Id),
					//функция идентификации документа 
					(OrderJournalNode node) => node.EntityType == typeof(VodovozOrder),
					"Заказ",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);
				
			//завершение конфигурации
			ordersConfig.FinishConfiguration();
		}

		private IQueryOver<OrderWithoutShipmentForDebt> GetOrdersWithoutShipmentForDebtQuery(IUnitOfWork uow)
		{
			OrderJournalNode resultAlias = null;
			OrderWithoutShipmentForDebt orderWSDAlias = null;
			Counterparty counterpartyAlias = null;
			Employee authorAlias = null;

			var query = uow.Session.QueryOver(() => orderWSDAlias);

			if (FilterViewModel.ViewTypes != ViewTypes.OrderWSFD && FilterViewModel.ViewTypes != ViewTypes.All
				|| FilterViewModel.RestrictStatus != null && FilterViewModel.RestrictStatus != OrderStatus.Closed
				|| FilterViewModel.RestrictPaymentType != null
				|| FilterViewModel.DeliveryPoint != null
				|| FilterViewModel.OnlineOrderId != null
				|| FilterViewModel.RestrictOnlyService != null
				|| FilterViewModel.RestrictOnlySelfDelivery != null
				|| FilterViewModel.RestrictLessThreeHours == true
				|| FilterViewModel.OrderPaymentStatus != null
				|| FilterViewModel.Organisation != null
				|| FilterViewModel.PaymentByCardFrom != null
				|| FilterViewModel.SortDeliveryDate == true
				|| !string.IsNullOrWhiteSpace(FilterViewModel.DeliveryPointAddressLike)
				|| FilterViewModel.ExcludeClosingDocumentDeliverySchedule)
			{
				query.Where(o => o.Id == -1);
			}

			if (FilterViewModel.StartDate != null) {
				query.Where(o => o.CreateDate >= FilterViewModel.StartDate);
			}

			if(FilterViewModel.EndDate != null) {
				query.Where(o => o.CreateDate <= FilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));
			}
			
			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
			}

			if(FilterViewModel.Author != null)
			{
				query.Where(o => o.Author == FilterViewModel.Author);
			}

			if(FilterViewModel.OrderId != null)
			{
				query.Where(() => orderWSDAlias.Id == FilterViewModel.OrderId.Value);
			}

			if(!String.IsNullOrWhiteSpace(FilterViewModel.CounterpartyPhone))
			{
				Phone counterpartyPhoneAlias = null;

				var counterpartyPhonesSubquery = QueryOver.Of<Phone>(() => counterpartyPhoneAlias)
					.Where(() => counterpartyPhoneAlias.Counterparty.Id == counterpartyAlias.Id)
					.And(() => counterpartyPhoneAlias.DigitsNumber == FilterViewModel.CounterpartyPhone)
					.And(() => !counterpartyPhoneAlias.IsArchive)
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(counterpartyPhonesSubquery.DetachedCriteria));
			}

			if(!String.IsNullOrWhiteSpace(FilterViewModel.DeliveryPointPhone))
			{
				query.Where(x => x.Id == null);
			}

			if(FilterViewModel.IsForSalesDepartment != null)
			{
				query.Where(() => counterpartyAlias.IsForSalesDepartment == FilterViewModel.IsForSalesDepartment.Value);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.CounterpartyNameLike) && FilterViewModel.CounterpartyNameLike.Length >= _minLengthLikeSearch)
			{
				query.Where(Restrictions.Like(Projections.Property(() => counterpartyAlias.FullName), FilterViewModel.CounterpartyNameLike, MatchMode.Anywhere));
			}

			query.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				 .Left.JoinAlias(o => o.Author, () => authorAlias);

			query.Where(GetSearchCriterion(
				() => orderWSDAlias.Id,
				() => counterpartyAlias.Name,
				() => authorAlias.LastName
			));

			var resultQuery = query
				.SelectList(list => list
				   .Select(() => orderWSDAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => orderWSDAlias.CreateDate).WithAlias(() => resultAlias.CreateDate)
				   .Select(() => orderWSDAlias.CreateDate).WithAlias(() => resultAlias.Date)
				   .Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
				   .Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
				   .Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
				   .Select(() => orderWSDAlias.DebtSum).WithAlias(() => resultAlias.Sum)
				   .Select(
						Projections.Conditional(
							Restrictions.Or(
								Restrictions.Eq(Projections.Constant(true), _userHasAccessToRetail),
								Restrictions.Not(Restrictions.Eq(Projections.Property(() => counterpartyAlias.IsForRetail), true))
								),
							Projections.Constant(true),
							Projections.Constant(false)
						)).WithAlias(() => resultAlias.Sensitive
					)
				)
				.OrderBy(x => x.CreateDate).Desc
				.SetTimeout(60)
				.TransformUsing(Transformers.AliasToBean<OrderJournalNode<OrderWithoutShipmentForDebt>>());

			return resultQuery;
		}

		private void RegisterOrdersWithoutShipmentForDebt()
		{
			var ordersConfig = RegisterEntity<OrderWithoutShipmentForDebt>(GetOrdersWithoutShipmentForDebtQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new OrderWithoutShipmentForDebtViewModel(
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory,
						_commonServices,
						_employeeService,
						new CommonMessages(_commonServices.InteractiveService),
						_rdlPreviewOpener),
					//функция диалога открытия документа
					(OrderJournalNode node) => new OrderWithoutShipmentForDebtViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						_commonServices,
						_employeeService,
						new CommonMessages(_commonServices.InteractiveService),
						_rdlPreviewOpener),
					//функция идентификации документа 
					(OrderJournalNode node) => node.EntityType == typeof(OrderWithoutShipmentForDebt),
					"Счет без отгрузки на долг",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);

			//завершение конфигурации
			ordersConfig.FinishConfiguration();
		}

		private IQueryOver<OrderWithoutShipmentForPayment> GetOrdersWithoutShipmentForPaymentQuery(IUnitOfWork uow)
		{
			OrderJournalNode resultAlias = null;
			OrderWithoutShipmentForPayment orderWSPAlias = null;
			OrderWithoutShipmentForPaymentItem orderWSPItemAlias = null;
			VodovozOrder orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			Employee authorAlias = null;

			var query = uow.Session.QueryOver(() => orderWSPAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.Left.JoinAlias(o => o.Author, () => authorAlias);

			if (FilterViewModel.ViewTypes != ViewTypes.OrderWSFP && FilterViewModel.ViewTypes != ViewTypes.All
			    || FilterViewModel.RestrictStatus != null && FilterViewModel.RestrictStatus != OrderStatus.Closed
			    || FilterViewModel.RestrictPaymentType != null
			    || FilterViewModel.DeliveryPoint != null
				|| FilterViewModel.OnlineOrderId != null
				|| FilterViewModel.RestrictOnlyService != null
				|| FilterViewModel.RestrictOnlySelfDelivery != null
			    || FilterViewModel.RestrictLessThreeHours == true
			    || FilterViewModel.OrderPaymentStatus != null
			    || FilterViewModel.Organisation != null
			    || FilterViewModel.PaymentByCardFrom != null
			    || FilterViewModel.SortDeliveryDate == true
				|| !string.IsNullOrWhiteSpace(FilterViewModel.DeliveryPointAddressLike)
				|| FilterViewModel.ExcludeClosingDocumentDeliverySchedule)
			{
				query.Where(o => o.Id == -1);
			}

			if (FilterViewModel.StartDate != null) {
				query.Where(o => o.CreateDate >= FilterViewModel.StartDate);
			}

			if(FilterViewModel.EndDate != null) {
				query.Where(o => o.CreateDate <= FilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));
			}

			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
			}

			if(FilterViewModel.Author != null)
			{
				query.Where(o => o.Author == FilterViewModel.Author);
			}

			var bottleCountSubquery = QueryOver.Of(() => orderWSPItemAlias)
				.Where(() => orderWSPAlias.Id == orderWSPItemAlias.OrderWithoutDeliveryForPayment.Id)
				.Left.JoinAlias(() => orderWSPItemAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var orderSumSubquery = QueryOver.Of(() => orderWSPItemAlias)
											.Where(() => orderWSPAlias.Id == orderWSPItemAlias.OrderWithoutDeliveryForPayment.Id)
											.Left.JoinAlias(() => orderWSPItemAlias.Order, () => orderAlias)
											.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
											.Select(
												Projections.Sum(
													Projections.SqlFunction(
														new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 * ?2 - IF(?3 IS NULL OR ?3 = 0, IFNULL(?4, 0), ?3)"),
														NHibernateUtil.Decimal,
														Projections.Property(() => orderItemAlias.Count),
														Projections.Property(() => orderItemAlias.Price),
														Projections.Property(() => orderItemAlias.DiscountMoney),
														Projections.Property(() => orderItemAlias.OriginalDiscountMoney)
													   )
												   )
											   );

			if(FilterViewModel.OrderId != null)
			{
				query.Where(() => orderWSPAlias.Id == FilterViewModel.OrderId.Value);
			}

			if(!String.IsNullOrWhiteSpace(FilterViewModel.CounterpartyPhone))
			{
				Phone counterpartyPhoneAlias = null;

				var counterpartyPhonesSubquery = QueryOver.Of<Phone>(() => counterpartyPhoneAlias)
					.Where(() => counterpartyPhoneAlias.Counterparty.Id == counterpartyAlias.Id)
					.And(() => counterpartyPhoneAlias.DigitsNumber == FilterViewModel.CounterpartyPhone)
					.And(() => !counterpartyPhoneAlias.IsArchive)
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(counterpartyPhonesSubquery.DetachedCriteria));
			}

			if(!String.IsNullOrWhiteSpace(FilterViewModel.DeliveryPointPhone))
			{
				query.Where(x => x.Id == null);
			}

			if(FilterViewModel.IsForSalesDepartment != null)
			{
				query.Where(() => counterpartyAlias.IsForSalesDepartment == FilterViewModel.IsForSalesDepartment.Value);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.CounterpartyNameLike) && FilterViewModel.CounterpartyNameLike.Length >= _minLengthLikeSearch)
			{
				query.Where(Restrictions.Like(Projections.Property(() => counterpartyAlias.FullName), FilterViewModel.CounterpartyNameLike, MatchMode.Anywhere));
			}

			query.Where(GetSearchCriterion(
				() => orderWSPAlias.Id,
				() => counterpartyAlias.Name,
				() => authorAlias.LastName
			));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => orderWSPAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => orderWSPAlias.CreateDate).WithAlias(() => resultAlias.CreateDate)
					.Select(() => orderWSPAlias.CreateDate).WithAlias(() => resultAlias.Date)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
					.Select(
						Projections.Conditional(
							Restrictions.Or(
								Restrictions.Eq(Projections.Constant(true), _userHasAccessToRetail),
								Restrictions.Not(Restrictions.Eq(Projections.Property(() => counterpartyAlias.IsForRetail), true))
								),
							Projections.Constant(true),
							Projections.Constant(false)
						)).WithAlias(() => resultAlias.Sensitive
					)
					.SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.Sum)
					.SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
				)
				.OrderBy(x => x.CreateDate).Desc
				.SetTimeout(60)
				.TransformUsing(Transformers.AliasToBean<OrderJournalNode<OrderWithoutShipmentForPayment>>());

			return resultQuery;
		}

		private void RegisterOrdersWithoutShipmentForPayment()
		{
			var ordersConfig = RegisterEntity<OrderWithoutShipmentForPayment>(GetOrdersWithoutShipmentForPaymentQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new OrderWithoutShipmentForPaymentViewModel(
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory,
						_commonServices,
						_employeeService,
						new ParametersProvider(),
						new CommonMessages(_commonServices.InteractiveService),
						_rdlPreviewOpener),
					//функция диалога открытия документа
					(OrderJournalNode node) => new OrderWithoutShipmentForPaymentViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						_commonServices,
						_employeeService,
						new ParametersProvider(),
						new CommonMessages(_commonServices.InteractiveService),
						_rdlPreviewOpener),
					//функция идентификации документа 
					(OrderJournalNode node) => node.EntityType == typeof(OrderWithoutShipmentForPayment),
					"Счет без отгрузки на постоплату",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);

			//завершение конфигурации
			ordersConfig.FinishConfiguration();
		}

		private IQueryOver<OrderWithoutShipmentForAdvancePayment> GetOrdersWithoutShipmentForAdvancePaymentQuery(IUnitOfWork uow)
		{
			OrderJournalNode resultAlias = null;
			OrderWithoutShipmentForAdvancePayment orderWSAPAlias = null;
			OrderWithoutShipmentForAdvancePaymentItem orderWSAPItemAlias = null;
			Counterparty counterpartyAlias = null;
			Employee authorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver(() => orderWSAPAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.Left.JoinAlias(o => o.Author, () => authorAlias);

			if (FilterViewModel.ViewTypes != ViewTypes.OrderWSFAP && FilterViewModel.ViewTypes != ViewTypes.All
			    || FilterViewModel.RestrictStatus != null && FilterViewModel.RestrictStatus != OrderStatus.Closed
			    || FilterViewModel.RestrictPaymentType != null
			    || FilterViewModel.DeliveryPoint != null
				|| FilterViewModel.OnlineOrderId != null
				|| FilterViewModel.RestrictOnlyService != null
			    || FilterViewModel.RestrictOnlySelfDelivery != null
			    || FilterViewModel.RestrictLessThreeHours == true
			    || FilterViewModel.OrderPaymentStatus != null
			    || FilterViewModel.Organisation != null
			    || FilterViewModel.PaymentByCardFrom != null
			    || FilterViewModel.SortDeliveryDate == true
				|| !string.IsNullOrWhiteSpace(FilterViewModel.DeliveryPointAddressLike)
				|| FilterViewModel.ExcludeClosingDocumentDeliverySchedule)
			{
				query.Where(o => o.Id == -1);
			}

			if (FilterViewModel.StartDate != null) {
				query.Where(o => o.CreateDate >= FilterViewModel.StartDate);
			}

			if(FilterViewModel.EndDate != null) {
				query.Where(o => o.CreateDate <= FilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));
			}
			
			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
			}

			if(FilterViewModel.Author != null)
			{
				query.Where(o => o.Author == FilterViewModel.Author);
			}

			var bottleCountSubquery = QueryOver.Of(() => orderWSAPItemAlias)
				.Where(() => orderWSAPAlias.Id == orderWSAPItemAlias.OrderWithoutDeliveryForAdvancePayment.Id)
				.JoinAlias(() => orderWSAPItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderWSAPItemAlias.Count));

			var orderSumSubquery = QueryOver.Of(() => orderWSAPItemAlias)
											.Where(() => orderWSAPItemAlias.OrderWithoutDeliveryForAdvancePayment.Id == orderWSAPAlias.Id)
											.Select(Projections.Sum(
														Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "(?1 * ?2 - ?3)"),
														NHibernateUtil.Decimal, new IProjection[] {
														Projections.Property(() => orderWSAPItemAlias.Price),
														Projections.Property(() => orderWSAPItemAlias.Count),
														Projections.Property(() => orderWSAPItemAlias.DiscountMoney)})
												   )
										   );

			if(FilterViewModel.OrderId != null)
			{
				query.Where(() => orderWSAPAlias.Id == FilterViewModel.OrderId.Value);
			}

			if(!String.IsNullOrWhiteSpace(FilterViewModel.CounterpartyPhone))
			{
				Phone counterpartyPhoneAlias = null;

				var counterpartyPhonesSubquery = QueryOver.Of<Phone>(() => counterpartyPhoneAlias)
					.Where(() => counterpartyPhoneAlias.Counterparty.Id == counterpartyAlias.Id)
					.And(() => counterpartyPhoneAlias.DigitsNumber == FilterViewModel.CounterpartyPhone)
					.And(() => !counterpartyPhoneAlias.IsArchive)
					.Select(x => x.Id);

				query.Where(Subqueries.Exists(counterpartyPhonesSubquery.DetachedCriteria));
			}

			if(!String.IsNullOrWhiteSpace(FilterViewModel.DeliveryPointPhone))
			{
				query.Where(x => x.Id == null);
			}

			if(FilterViewModel.IsForSalesDepartment != null)
			{
				query.Where(() => counterpartyAlias.IsForSalesDepartment == FilterViewModel.IsForSalesDepartment.Value);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.CounterpartyNameLike) && FilterViewModel.CounterpartyNameLike.Length >= _minLengthLikeSearch)
			{
				query.Where(Restrictions.Like(Projections.Property(() => counterpartyAlias.FullName), FilterViewModel.CounterpartyNameLike, MatchMode.Anywhere));
			}

			query.Where(GetSearchCriterion(
				() => orderWSAPAlias.Id,
				() => counterpartyAlias.Name,
				() => authorAlias.LastName
			));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => orderWSAPAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => orderWSAPAlias.CreateDate).WithAlias(() => resultAlias.CreateDate)
					.Select(() => orderWSAPAlias.CreateDate).WithAlias(() => resultAlias.Date)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
					.Select(
						Projections.Conditional(
							Restrictions.Or(
								Restrictions.Eq(Projections.Constant(true), _userHasAccessToRetail),
								Restrictions.Not(Restrictions.Eq(Projections.Property(() => counterpartyAlias.IsForRetail), true))
								),
							Projections.Constant(true),
							Projections.Constant(false)
						)).WithAlias(() => resultAlias.Sensitive
					)
					.SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.Sum)
					.SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
				)
				.OrderBy(x => x.CreateDate).Desc
				.SetTimeout(60)
				.TransformUsing(Transformers.AliasToBean<OrderJournalNode<OrderWithoutShipmentForAdvancePayment>>());

			return resultQuery;
		}

		private void RegisterOrdersWithoutShipmentForAdvancePayment()
		{
			var ordersConfig = RegisterEntity<OrderWithoutShipmentForAdvancePayment>(GetOrdersWithoutShipmentForAdvancePaymentQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new OrderWithoutShipmentForAdvancePaymentViewModel(
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory,
						_commonServices,
						_employeeService,
						_nomenclatureSelectorFactory,
						_counterpartySelectorFactory,
						_nomenclatureRepository,
						_userRepository,
						new DiscountReasonRepository(),
						new ParametersProvider(),
						new OrderDiscountsController(new NomenclatureFixedPriceController(
							new NomenclatureFixedPriceFactory(), new WaterFixedPricesGenerator(_nomenclatureRepository))),
						new CommonMessages(_commonServices.InteractiveService),
						_rdlPreviewOpener),
					//функция диалога открытия документа
					(OrderJournalNode node) => new OrderWithoutShipmentForAdvancePaymentViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						_commonServices,
						_employeeService,
						_nomenclatureSelectorFactory,
						_counterpartySelectorFactory,
						_nomenclatureRepository,
						_userRepository,
						new DiscountReasonRepository(),
						new ParametersProvider(),
						new OrderDiscountsController(new NomenclatureFixedPriceController(
							new NomenclatureFixedPriceFactory(), new WaterFixedPricesGenerator(_nomenclatureRepository))),
						new CommonMessages(_commonServices.InteractiveService),
						_rdlPreviewOpener),
					//функция идентификации документа 
					(OrderJournalNode node) => node.EntityType == typeof(OrderWithoutShipmentForAdvancePayment),
					"Счет без отгрузки на предоплату",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);

			//завершение конфигурации
			ordersConfig.FinishConfiguration();
		}

		protected override void CreatePopupActions()
		{
			bool IsOrder(OrderJournalNode selectedNode)
			{
				if(selectedNode == null)
				{
					return false;
				}

				return selectedNode.EntityType == typeof(VodovozOrder);
			}

			bool CanCreateOrder(object[] selectedItems)
			{
				var selectedNode = GetSelectedNode(selectedItems);
				return IsOrder(selectedNode) && EntityConfigs[selectedNode.EntityType].PermissionResult.CanCreate;
			}

			PopupActionsList.Add(
				new JournalAction(
					"Перейти в маршрутный лист",
					selectedItems => selectedItems.Any(
						x => AccessRouteListKeeping((x as OrderJournalNode).Id)) && IsOrder(GetSelectedNode(selectedItems)),
					selectedItems => selectedItems.All(x => (x as OrderJournalNode).Sensitive),
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var addresses = UoW.Session.QueryOver<RouteListItem>()
							.Where(x => x.Order.Id.IsIn(selectedNodes.Select(n => n.Id).ToArray())).List();

						var routes = addresses.GroupBy(x => x.RouteList.Id);

						foreach(var route in routes)
						{
							_gtkDialogsOpener.OpenRouteListKeepingDlg(this, route.Key, route.Select(x => x.Order.Id)
								.ToArray());
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Перейти в недовоз",
					(selectedItems) => selectedItems.Any(
						o => _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, (o as OrderJournalNode).Id).Any()) 
					        && IsOrder(GetSelectedNode(selectedItems))
							&& !_userHasOnlyAccessToWarehouseAndComplaints,
					selectedItems => selectedItems.All(x => (x as OrderJournalNode).Sensitive),
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var order = UoW.GetById<VodovozOrder>(selectedNodes.FirstOrDefault().Id);

						var undeliveredOrdersFilter = new UndeliveredOrdersFilterViewModel(
							_commonServices,
							_orderSelectorFactory,
							_employeeJournalFactory, 
							_counterpartyJournalFactory, 
							_deliveryPointJournalFactory, 
							_subdivisionJournalFactory)
						{
							HidenByDefault = true,
							RestrictOldOrder = order,
							RestrictOldOrderStartDate = order.DeliveryDate,
							RestrictOldOrderEndDate = order.DeliveryDate
						};

						var dlg = new UndeliveredOrdersJournalViewModel(
							undeliveredOrdersFilter,
							UnitOfWorkFactory,
							_commonServices,
							_gtkDialogsOpener,
							_employeeJournalFactory,
							_employeeService,
							_undeliveredOrdersJournalOpener,
							_orderSelectorFactory,
							_undeliveredOrdersRepository,
							new EmployeeSettings(new ParametersProvider()),
							_subdivisionParametersProvider
						);

						TabParent.AddTab(dlg, this, false);
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть диалог закрытия",
					(selectedItems) => selectedItems.Any(
						x => AccessToRouteListClosing((x as OrderJournalNode).Id)) && IsOrder(GetSelectedNode(selectedItems)),
					selectedItems => selectedItems.All(x => (x as OrderJournalNode).Sensitive),
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var routeListIds = selectedNodes.Select(x => x.Id).ToArray();
						var addresses = UoW.Session.QueryOver<RouteListItem>()
							.Where(x => x.Order.Id.IsIn(routeListIds)).List();

						var routes = addresses.GroupBy(x => x.RouteList.Id);

						foreach(var rl in routes)
						{
							_gtkDialogsOpener.OpenRouteListClosingDlg(this, rl.Key);
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть на Yandex картах(координаты)",
					selectedItems => IsOrder(GetSelectedNode(selectedItems)),
					selectedItems => selectedItems.All(x => (x as OrderJournalNode).Sensitive),
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						foreach(var sel in selectedNodes) {
							var order = UoW.GetById<VodovozOrder>(sel.Id);
							if(order.DeliveryPoint == null || order.DeliveryPoint.Latitude == null || order.DeliveryPoint.Longitude == null)
								continue;

							System.Diagnostics.Process.Start(
								string.Format(
									CultureInfo.InvariantCulture,
									"https://maps.yandex.ru/?ll={0},{1}&z=17",
									order.DeliveryPoint.Longitude,
									order.DeliveryPoint.Latitude
								)
							);
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть на Yandex картах(адрес)",
					selectedItems => IsOrder(GetSelectedNode(selectedItems)),
					selectedItems => selectedItems.All(x => (x as OrderJournalNode).Sensitive),
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						foreach(var sel in selectedNodes) {
							var order = UoW.GetById<VodovozOrder>(sel.Id);
							if(order.DeliveryPoint == null)
								continue;

							System.Diagnostics.Process.Start(
								string.Format(CultureInfo.InvariantCulture,
									"https://maps.yandex.ru/?text={0} {1} {2}",
									order.DeliveryPoint.City,
									order.DeliveryPoint.Street,
									order.DeliveryPoint.Building
								)
							);
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть на карте OSM",
					selectedItems => IsOrder(GetSelectedNode(selectedItems)),
					selectedItems => selectedItems.All(x => (x as OrderJournalNode).Sensitive),
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						foreach(var sel in selectedNodes) {
							var order = UoW.GetById<VodovozOrder>(sel.Id);
							if(order.DeliveryPoint == null || order.DeliveryPoint.Latitude == null || order.DeliveryPoint.Longitude == null)
								continue;

							System.Diagnostics.Process.Start(string.Format(CultureInfo.InvariantCulture, "http://www.openstreetmap.org/#map=17/{1}/{0}", order.DeliveryPoint.Longitude, order.DeliveryPoint.Latitude));
						}
					}
				)
			);
			
			PopupActionsList.Add(
				new JournalAction(
					"Повторить заказ",
					selectedItems => CanCreateOrder(selectedItems) && !_userHasOnlyAccessToWarehouseAndComplaints,
					selectedItems => selectedItems.All(x => (x as OrderJournalNode).Sensitive),
					(selectedItems) =>
					{
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var order = UoW.GetById<VodovozOrder>(selectedNodes.FirstOrDefault().Id);
						_gtkDialogsOpener.OpenCopyOrderDlg(this, order.Id);
					}
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Создать рекламацию",
					selectedItems => CanCreateComplaint(selectedItems),
					selectedItems => true,
					selectedItems => {
						var selectedNodes = selectedItems.OfType<OrderJournalNode>().ToList();
						if(selectedNodes.Count != 1)
						{
							return;
						}
						var selectedOrder = selectedNodes.First();

						var complaintViewModel = new CreateComplaintViewModel(
							EntityUoWBuilder.ForCreate(),
							UnitOfWorkFactory,
							_employeeService,
							_subdivisionRepository,
							_commonServices,
							_userRepository,
							_fileDialogService,
							_orderSelectorFactory,
							_employeeJournalFactory,
							_counterpartyJournalFactory,
							_deliveryPointJournalFactory,
							_subdivisionParametersProvider
						);
						var order = complaintViewModel.UoW.GetById<VodovozOrder>(selectedOrder.Id);
						complaintViewModel.Entity.Counterparty = order.Client;
						complaintViewModel.Entity.Order = order;
						complaintViewModel.Entity.DeliveryPoint = order.DeliveryPoint;
						TabParent.OpenTab(() => complaintViewModel, this);
					}
				)
			);
		}

		private bool CanCreateComplaint(object[] selectedItems)
		{
			var selectedNode = GetSelectedNode(selectedItems);
			if(selectedNode?.EntityType != typeof(VodovozOrder))
			{
				return false;
			}
			switch(selectedNode.StatusEnum)
			{
				case OrderStatus.Shipped:
				case OrderStatus.OnTheWay:
				case OrderStatus.Closed:
				case OrderStatus.UnloadingOnStock:
				case OrderStatus.WaitForPayment:
					return true;
				default:
					return false;
			}
		}

		private OrderJournalNode GetSelectedNode(object[] selectedItems)
		{
			var selectedNodes = selectedItems.OfType<OrderJournalNode>();
			return selectedNodes.Count() != 1 ? null : selectedNodes.FirstOrDefault();
		}

		private bool AccessToRouteListClosing(int orderId)
		{
			var orderIdArr = new[] { orderId };
			var routeListItems = UoW.Session.QueryOver<RouteListItem>()
						.Where(x => x.Order.Id.IsIn(orderIdArr)).List();

			if(routeListItems.Any() && !_userHasOnlyAccessToWarehouseAndComplaints)
			{
				var validStates = new RouteListStatus[] {
											RouteListStatus.OnClosing,
											RouteListStatus.MileageCheck,
											RouteListStatus.Closed
								  };
				return validStates.Contains(routeListItems.First().RouteList.Status);
			}
			return false;
		}

		private bool AccessRouteListKeeping(int orderId)
		{
			var orderIdArr = new[] { orderId };
			var routeListItems = UoW.Session.QueryOver<RouteListItem>()
						.Where(x => x.Order.Id.IsIn(orderIdArr)).List();

			if(routeListItems.Any() && !_userHasOnlyAccessToWarehouseAndComplaints)
			{
				return true;
			}
			return false;
		}
	}
}
