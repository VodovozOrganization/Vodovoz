using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using System;
using System.Globalization;
using System.Linq;
using Autofac;
using DateTimeHelpers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using VodovozOrder = Vodovoz.Domain.Orders.Order;
using QS.Deletion;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.JournalViewModels
{
	public class RetailOrderJournalViewModel : FilterableMultipleEntityJournalViewModelBase<RetailOrderJournalNode, OrderJournalFilterViewModel>
	{
		private const int _minLengthLikeSearch = 3;

		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly int _closingDocumentDeliveryScheduleId;
		private ILifetimeScope _lifetimeScope;

		public RetailOrderJournalViewModel(
			ILifetimeScope lifetimeScope,
			OrderJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			INomenclatureRepository nomenclatureRepository,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			Action<OrderJournalFilterViewModel> filterConfig = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));

			_undeliveredOrdersRepository =
				undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_closingDocumentDeliveryScheduleId =
				(deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings)))
				.ClosingDocumentDeliveryScheduleId;

			filterViewModel.Journal = this;

			TabName = "Журнал заказов";

			SearchEnabled = false;

			RegisterOrders();
			RegisterOrdersWithoutShipmentForDebt();
			RegisterOrdersWithoutShipmentForPayment();
			RegisterOrdersWithoutShipmentForAdvancePayment();

			var threadLoader = DataLoader as ThreadDataLoader<RetailOrderJournalNode>;
			threadLoader.MergeInOrderBy(x => x.CreateDate, true);

			FinishJournalConfiguration();

			UpdateOnChanges(
				typeof(VodovozOrder),
				typeof(OrderWithoutShipmentForDebt),
				typeof(OrderWithoutShipmentForPayment),
				typeof(OrderWithoutShipmentForAdvancePayment),
				typeof(OrderWithoutShipmentForPaymentItem),
				typeof(OrderWithoutShipmentForAdvancePaymentItem),
				typeof(OrderItem));

			if(filterConfig != null)
			{
				FilterViewModel.ConfigureWithoutFiltering(filterConfig);
			}
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateEditAction();
			CreateCustomDeleteAction();
		}

		private void CreateCustomDeleteAction()
		{
			var deleteAction = new JournalAction("Удалить",
				(selected) => {
					var selectedNodes = selected.OfType<RetailOrderJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					var selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanDelete;
				},
				(selected) => EntityConfigs.Any(config => config.Value.PermissionResult.CanDelete),
				(selected) => {
					var selectedNodes = selected.OfType<RetailOrderJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					var selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					if(config.PermissionResult.CanDelete)
					{
						DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
					}
				},
				"Delete"
			);
			NodeActionsList.Add(deleteAction);
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<RetailOrderJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					RetailOrderJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<RetailOrderJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					RetailOrderJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog) {
						HideJournal(TabParent);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None) {
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		private IQueryOver<VodovozOrder> GetOrdersQuery(IUnitOfWork uow)
		{
			RetailOrderJournalNode resultAlias = null;
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
			
			var query = uow.Session.QueryOver<VodovozOrder>(() => orderAlias);

			if(FilterViewModel != null && FilterViewModel.IsForRetail != null)
			{
				query.JoinEntityAlias(
					() => counterpartyAlias,
					() => counterpartyAlias.Id == orderAlias.Client.Id
					   && counterpartyAlias.IsForRetail == FilterViewModel.IsForRetail,
					NHibernate.SqlCommand.JoinType.InnerJoin);
			}
			else
			{
				query.Left.JoinAlias(o => o.Client, () => counterpartyAlias);
			}

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

			if(FilterViewModel.OnlineOrderId != null)
			{
				query.Where(() => orderAlias.OnlinePaymentNumber == FilterViewModel.OnlineOrderId);
			}

			var startDate = FilterViewModel.StartDate;
			if(startDate != null) {
				query.Where(o => o.DeliveryDate >= startDate);
			}

			var endDate = FilterViewModel.EndDate;
			if(endDate != null) {
				query.Where(o => o.DeliveryDate <= endDate.Value.LatestDayTime());
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

			if(FilterViewModel.OrderId != null)
			{
				query.Where(() => orderAlias.Id == FilterViewModel.OrderId.Value);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.CounterpartyNameLike) && FilterViewModel.CounterpartyNameLike.Length >= _minLengthLikeSearch)
			{
				query.Where(Restrictions.Like(Projections.Property(() => counterpartyAlias.FullName), FilterViewModel.CounterpartyNameLike, MatchMode.Anywhere));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyInn))
			{
				query.Where(() => counterpartyAlias.INN == FilterViewModel.CounterpartyInn);
			}

			query.Where(FilterViewModel?.SearchByAddressViewModel?.GetSearchCriterion(
				() => deliveryPointAlias.CompiledAddress
			));

			var bottleCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var sanitisationCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id && nomenclatureAlias.IsNeedSanitisation)
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

			if(FilterViewModel.FilterClosingDocumentDeliverySchedule.HasValue)
			{
				if(!FilterViewModel.FilterClosingDocumentDeliverySchedule.Value)
				{
					query.Where(o => o.DeliverySchedule.Id == null || o.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId);
				}
				else
				{
					query.Where(o => o.DeliverySchedule.Id == _closingDocumentDeliveryScheduleId);
				}
			}

			query.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(o => o.DeliverySchedule, () => deliveryScheduleAlias)
				.Left.JoinAlias(o => o.Author, () => authorAlias)
				.Left.JoinAlias(o => o.LastEditor, () => lastEditorAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(o => o.Contract, () => contractAlias);

			query.Where(GetSearchCriterion(
				() => orderAlias.Id,
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.CompiledAddress,
				() => authorAlias.LastName,
				() => orderAlias.DriverCallId,
				() => orderAlias.OnlinePaymentNumber,
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
				   .Select(() => orderAlias.WaitUntilTime).WithAlias(() => resultAlias.WaitUntilTime)				   
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
				   .Select(() => orderAlias.OnlinePaymentNumber).WithAlias(() => resultAlias.OnlineOrder)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
				   .Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
				   .Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
				   .Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompilledAddress)
				   .Select(() => deliveryPointAlias.City).WithAlias(() => resultAlias.City)
				   .Select(() => deliveryPointAlias.Street).WithAlias(() => resultAlias.Street)
				   .Select(() => deliveryPointAlias.Building).WithAlias(() => resultAlias.Building)
				   .Select(() => orderAlias.EShopOrder).WithAlias(() => resultAlias.EShopOrder)
				   .Select(() => orderAlias.OrderPaymentStatus).WithAlias(() => resultAlias.OrderPaymentStatus)
				   .SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.Sum)
				   .SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
				   .SelectSubQuery(sanitisationCountSubquery).WithAlias(() => resultAlias.SanitisationAmount)
				)
				.OrderBy(x => x.CreateDate).Desc
				.SetTimeout(60)
				.TransformUsing(Transformers.AliasToBean<RetailOrderJournalNode<VodovozOrder>>());

			return resultQuery;
		}

		private void RegisterOrders()
		{
			var ordersConfig = RegisterEntity<VodovozOrder>(GetOrdersQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new OrderDlg() { IsForRetail = FilterViewModel.IsForRetail },
					//функция диалога открытия документа
					(RetailOrderJournalNode node) => new OrderDlg(node.Id),
					//функция идентификации документа 
					(RetailOrderJournalNode node) => node.EntityType == typeof(VodovozOrder),
					"Заказ",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);
				
			//завершение конфигурации
			ordersConfig.FinishConfiguration();
		}

		private IQueryOver<OrderWithoutShipmentForDebt> GetOrdersWithoutShipmentForDebtQuery(IUnitOfWork uow)
		{
			RetailOrderJournalNode resultAlias = null;
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
				|| FilterViewModel.SearchByAddressViewModel?.SearchValues?.Length > 0
				|| FilterViewModel.FilterClosingDocumentDeliverySchedule.HasValue)
			{
				query.Where(o => o.Id == -1);
			}

			if(FilterViewModel != null && FilterViewModel.IsForRetail != null)
			{
				query.JoinEntityAlias(
					() => counterpartyAlias,
					() => counterpartyAlias.Id == orderWSDAlias.Client.Id
					   && counterpartyAlias.IsForRetail == FilterViewModel.IsForRetail,
					NHibernate.SqlCommand.JoinType.InnerJoin);
			}
			else
			{
				query.Left.JoinAlias(o => o.Client, () => counterpartyAlias);
			}

			var startDate = FilterViewModel.StartDate;
			if(startDate != null) {
				query.Where(o => o.CreateDate >= startDate);
			}

			var endDate = FilterViewModel.EndDate;
			if(endDate != null) {
				query.Where(o => o.CreateDate <= endDate.Value.LatestDayTime());
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

			if(!string.IsNullOrWhiteSpace(FilterViewModel.CounterpartyNameLike) && FilterViewModel.CounterpartyNameLike.Length >= _minLengthLikeSearch)
			{
				query.Where(Restrictions.Like(Projections.Property(() => counterpartyAlias.FullName), FilterViewModel.CounterpartyNameLike, MatchMode.Anywhere));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyInn))
			{
				query.Where(() => counterpartyAlias.INN == FilterViewModel.CounterpartyInn);
			}

			query.Left.JoinAlias(o => o.Author, () => authorAlias);

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
				   .Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
				   .Select(() => orderWSDAlias.DebtSum).WithAlias(() => resultAlias.Sum)
				)
				.OrderBy(x => x.CreateDate).Desc
				.SetTimeout(60)
				.TransformUsing(Transformers.AliasToBean<RetailOrderJournalNode<OrderWithoutShipmentForDebt>>());

			return resultQuery;
		}

		private void RegisterOrdersWithoutShipmentForDebt()
		{
			var ordersConfig = RegisterEntity<OrderWithoutShipmentForDebt>(GetOrdersWithoutShipmentForDebtQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => _lifetimeScope.Resolve<OrderWithoutShipmentForDebtViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForCreate())),
					//функция диалога открытия документа
					(RetailOrderJournalNode node) => _lifetimeScope.Resolve<OrderWithoutShipmentForDebtViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(node.Id))),
					//функция идентификации документа 
					(RetailOrderJournalNode node) => node.EntityType == typeof(OrderWithoutShipmentForDebt),
					"Счет без отгрузки на долг",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);

			//завершение конфигурации
			ordersConfig.FinishConfiguration();
		}

		private IQueryOver<OrderWithoutShipmentForPayment> GetOrdersWithoutShipmentForPaymentQuery(IUnitOfWork uow)
		{
			RetailOrderJournalNode resultAlias = null;
			OrderWithoutShipmentForPayment orderWSPAlias = null;
			OrderWithoutShipmentForPaymentItem orderWSPItemAlias = null;
			VodovozOrder orderAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			Employee authorAlias = null;

			var query = uow.Session.QueryOver(() => orderWSPAlias)
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
				|| FilterViewModel.SearchByAddressViewModel?.SearchValues?.Length > 0
				|| FilterViewModel.FilterClosingDocumentDeliverySchedule.HasValue)
			{
				query.Where(o => o.Id == -1);
			}

			if(FilterViewModel != null && FilterViewModel.IsForRetail != null)
			{
				query.JoinEntityAlias(
					() => counterpartyAlias,
					() => counterpartyAlias.Id == orderWSPAlias.Client.Id
					   && counterpartyAlias.IsForRetail == FilterViewModel.IsForRetail,
					NHibernate.SqlCommand.JoinType.InnerJoin);
			}
			else
			{
				query.Left.JoinAlias(o => o.Client, () => counterpartyAlias);
			}

			var startDate = FilterViewModel.StartDate;
			if(startDate != null) {
				query.Where(o => o.CreateDate >= startDate);
			}

			var endDate = FilterViewModel.EndDate;
			if(endDate != null) {
				query.Where(o => o.CreateDate <= endDate.Value.LatestDayTime());
			}

			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
			}

			if(FilterViewModel.Author != null)
			{
				query.Where(o => o.Author == FilterViewModel.Author);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.CounterpartyNameLike) && FilterViewModel.CounterpartyNameLike.Length >= _minLengthLikeSearch)
			{
				query.Where(Restrictions.Like(Projections.Property(() => counterpartyAlias.FullName), FilterViewModel.CounterpartyNameLike, MatchMode.Anywhere));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyInn))
			{
				query.Where(() => counterpartyAlias.INN == FilterViewModel.CounterpartyInn);
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
					.Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
				   	.SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.Sum)
				   	.SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
				)
				.OrderBy(x => x.CreateDate).Desc
				.SetTimeout(60)
				.TransformUsing(Transformers.AliasToBean<RetailOrderJournalNode<OrderWithoutShipmentForPayment>>());

			return resultQuery;
		}

		private void RegisterOrdersWithoutShipmentForPayment()
		{
			var ordersConfig = RegisterEntity<OrderWithoutShipmentForPayment>(GetOrdersWithoutShipmentForPaymentQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => _lifetimeScope.Resolve<OrderWithoutShipmentForPaymentViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForCreate())),
					//функция диалога открытия документа
					(RetailOrderJournalNode node) => _lifetimeScope.Resolve<OrderWithoutShipmentForPaymentViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(node.Id))),
					//функция идентификации документа 
					(RetailOrderJournalNode node) => node.EntityType == typeof(OrderWithoutShipmentForPayment),
					"Счет без отгрузки на постоплату",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);

			//завершение конфигурации
			ordersConfig.FinishConfiguration();
		}

		private IQueryOver<OrderWithoutShipmentForAdvancePayment> GetOrdersWithoutShipmentForAdvancePaymentQuery(IUnitOfWork uow)
		{
			RetailOrderJournalNode resultAlias = null;
			OrderWithoutShipmentForAdvancePayment orderWSAPAlias = null;
			OrderWithoutShipmentForAdvancePaymentItem orderWSAPItemAlias = null;
			Counterparty counterpartyAlias = null;
			Employee authorAlias = null;
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver(() => orderWSAPAlias)
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
				|| FilterViewModel.SearchByAddressViewModel?.SearchValues?.Length > 0
				|| FilterViewModel.FilterClosingDocumentDeliverySchedule.HasValue)
			{
				query.Where(o => o.Id == -1);
			}

			if(FilterViewModel != null && FilterViewModel.IsForRetail != null)
			{
				query.JoinEntityAlias(
					() => counterpartyAlias,
					() => counterpartyAlias.Id == orderWSAPAlias.Client.Id
					   && counterpartyAlias.IsForRetail == FilterViewModel.IsForRetail,
					NHibernate.SqlCommand.JoinType.InnerJoin);
			}
			else
			{
				query.Left.JoinAlias(o => o.Client, () => counterpartyAlias);
			}

			var startDate = FilterViewModel.StartDate;
			if(startDate != null) {
				query.Where(o => o.CreateDate >= startDate);
			}

			var endDate = FilterViewModel.EndDate;
			if(endDate != null) {
				query.Where(o => o.CreateDate <= endDate.Value.LatestDayTime());
			}
			
			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
			}

			if(FilterViewModel.Author != null)
			{
				query.Where(o => o.Author == FilterViewModel.Author);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel.CounterpartyNameLike) && FilterViewModel.CounterpartyNameLike.Length >= _minLengthLikeSearch)
			{
				query.Where(Restrictions.Like(Projections.Property(() => counterpartyAlias.FullName), FilterViewModel.CounterpartyNameLike, MatchMode.Anywhere));
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyInn))
			{
				query.Where(() => counterpartyAlias.INN == FilterViewModel.CounterpartyInn);
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
				   .Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
				   .SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.Sum)
				   .SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
				)
				.OrderBy(x => x.CreateDate).Desc
				.SetTimeout(60)
				.TransformUsing(Transformers.AliasToBean<RetailOrderJournalNode<OrderWithoutShipmentForAdvancePayment>>());

			return resultQuery;
		}

		private void RegisterOrdersWithoutShipmentForAdvancePayment()
		{
			var ordersConfig = RegisterEntity<OrderWithoutShipmentForAdvancePayment>(GetOrdersWithoutShipmentForAdvancePaymentQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => _lifetimeScope.Resolve<OrderWithoutShipmentForAdvancePaymentViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForCreate())),
					//функция диалога открытия документа
					(RetailOrderJournalNode node) => _lifetimeScope.Resolve<OrderWithoutShipmentForAdvancePaymentViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(node.Id))),
					//функция идентификации документа 
					(RetailOrderJournalNode node) => node.EntityType == typeof(OrderWithoutShipmentForAdvancePayment),
					"Счет без отгрузки на предоплату",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);

			//завершение конфигурации
			ordersConfig.FinishConfiguration();
		}

		protected override void CreatePopupActions()
		{
			RetailOrderJournalNode GetSelectedNode(object[] selectedItems)
			{
				var selectedNodes = selectedItems.OfType<RetailOrderJournalNode>();
				return selectedNodes.Count() != 1 ? null : selectedNodes.FirstOrDefault();
			}

			bool IsOrder(RetailOrderJournalNode selectedNode)
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
						x => AccessRouteListKeeping((x as RetailOrderJournalNode).Id)) && IsOrder(GetSelectedNode(selectedItems)),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RetailOrderJournalNode>();
						var addresses = UoW.Session.QueryOver<RouteListItem>()
							.Where(x => x.Order.Id.IsIn(selectedNodes.Select(n => n.Id).ToArray())).List();

						var routes = addresses.GroupBy(x => x.RouteList.Id);

						var tdiMain = Startup.MainWin.TdiMain;

						foreach(var route in routes) {
							var page = NavigationManager.OpenViewModel<RouteListKeepingViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(route.Key));

							page.ViewModel.SelectOrdersById(route.Select(x => x.Order.Id).ToArray());
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Перейти в недовоз",
					(selectedItems) => selectedItems.Any(
						o => _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(
							UoW, (o as RetailOrderJournalNode).Id).Any()) && IsOrder(GetSelectedNode(selectedItems)),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RetailOrderJournalNode>();
						var order = UoW.GetById<VodovozOrder>(selectedNodes.FirstOrDefault().Id);

						NavigationManager.OpenViewModel<UndeliveredOrdersJournalViewModel, Action<UndeliveredOrdersFilterViewModel>>(this, filter =>
						{
							filter.HidenByDefault = true;
							filter.RestrictOldOrder = order;
							filter.RestrictOldOrderStartDate = order.DeliveryDate;
							filter.RestrictOldOrderEndDate = order.DeliveryDate;
						});
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть диалог закрытия",
					(selectedItems) => selectedItems.Any(
						x => AccessToRouteListClosing((x as RetailOrderJournalNode).Id)) && IsOrder(GetSelectedNode(selectedItems)),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RetailOrderJournalNode>();
						var routeListIds = selectedNodes.Select(x => x.Id).ToArray();
						var addresses = UoW.Session.QueryOver<RouteListItem>()
							.Where(x => x.Order.Id.IsIn(routeListIds)).List();

						var routes = addresses.GroupBy(x => x.RouteList.Id);
						var tdiMain = Startup.MainWin.TdiMain;

						foreach(var rl in routes) {
							tdiMain.OpenTab(
								DialogHelper.GenerateDialogHashName<RouteList>(rl.Key),
								() => new RouteListClosingDlg(rl.Key)
							);
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть на Yandex картах(координаты)",
					selectedItems => IsOrder(GetSelectedNode(selectedItems)),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RetailOrderJournalNode>();
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
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RetailOrderJournalNode>();
						foreach(var sel in selectedNodes) {
							var order = UoW.GetById<VodovozOrder>(sel.Id);
							if(order.DeliveryPoint == null)
								continue;

							System.Diagnostics.Process.Start(
								string.Format(CultureInfo.InvariantCulture,
									"https://maps.yandex.ru/?text={0} {1} {2} {3}",
									order.DeliveryPoint.City,
									order.DeliveryPoint.StreetType,
									order.DeliveryPoint.Street,
									order.DeliveryPoint.Building
								));
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть на карте OSM",
					selectedItems => IsOrder(GetSelectedNode(selectedItems)),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RetailOrderJournalNode>();
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
					CanCreateOrder,
					selectedItems => true,
					(selectedItems) => {
						var selectedNode = selectedItems.Cast<RetailOrderJournalNode>().FirstOrDefault();

						if(selectedNode is null)
						{
							return;
						}
						
						var order = UoW.GetById<VodovozOrder>(selectedNode.Id);
					
						var dlg = new OrderDlg();
						dlg.CopyLesserOrderFrom(order.Id);
						var tdiMain = Startup.MainWin.TdiMain;
						tdiMain.OpenTab(
							DialogHelper.GenerateDialogHashName<Domain.Orders.Order>(65656),
							() => dlg
						);
					}
				)
			);
		}

		bool AccessToRouteListClosing(int orderId)
		{
			var orderIdArr = new[] { orderId };
			var routeListItems = UoW.Session.QueryOver<RouteListItem>()
						.Where(x => x.Order.Id.IsIn(orderIdArr)).List();

			if(routeListItems.Any()) {
				var validStates = new RouteListStatus[] {
											RouteListStatus.OnClosing,
											RouteListStatus.MileageCheck,
											RouteListStatus.Closed
								  };
				return validStates.Contains(routeListItems.First().RouteList.Status);
			}
			return false;
		}

		bool AccessRouteListKeeping(int orderId)
		{
			var orderIdArr = new[] { orderId };
			var routeListItems = UoW.Session.QueryOver<RouteListItem>()
						.Where(x => x.Order.Id.IsIn(orderIdArr)).List();

			if(routeListItems.Any()) {
				return true;
			}
			return false;
		}

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
