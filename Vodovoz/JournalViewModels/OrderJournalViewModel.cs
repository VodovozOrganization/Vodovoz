using System;
using System.Globalization;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog.Gtk;
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
using Vodovoz.JournalViewers;
using Vodovoz.Repositories;
using NomenclatureRepository = Vodovoz.EntityRepositories.Goods.NomenclatureRepository;
using VodovozOrder = Vodovoz.Domain.Orders.Order;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using QS.Project.Journal.DataLoader;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.JournalViewModels
{
	public class OrderJournalViewModel : FilterableMultipleEntityJournalViewModelBase<OrderJournalNode, OrderJournalFilterViewModel>
	{
		private readonly ICommonServices commonServices;
		private readonly IEmployeeService employeeService;
		private readonly INomenclatureRepository nomenclatureRepository;
		private readonly IUserRepository userRepository;
		private readonly IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory counterpartySelectorFactory;

		public OrderJournalViewModel(
			OrderJournalFilterViewModel filterViewModel, 
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			IEmployeeService employeeService,
			IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			this.nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			
			TabName = "Журнал заказов";

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

			Nomenclature sanitizationNomenclature = nomenclatureRepository.GetSanitisationNomenclature(uow);

			var query = uow.Session.QueryOver<VodovozOrder>(() => orderAlias);

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

			if(FilterViewModel.RestrictDeliveryPoint != null) {
				query.Where(o => o.DeliveryPoint == FilterViewModel.RestrictDeliveryPoint);
			}

			if(FilterViewModel.RestrictStartDate != null) {
				query.Where(o => o.DeliveryDate >= FilterViewModel.RestrictStartDate);
			}

			if(FilterViewModel.RestrictEndDate != null) {
				query.Where(o => o.DeliveryDate <= FilterViewModel.RestrictEndDate.Value.AddDays(1).AddTicks(-1));
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

			if(FilterViewModel.RestrictHideService != null) {
				query.Where(o => o.IsService != FilterViewModel.RestrictHideService);
			}

			if(FilterViewModel.RestrictOnlyService != null) {
				query.Where(o => o.IsService == FilterViewModel.RestrictOnlyService);
			}
			
			if(FilterViewModel.OrderPaymentStatus != null) {
				query.Where(o => o.OrderPaymentStatus == FilterViewModel.OrderPaymentStatus);
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
														new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, ?2) * ?3 - ?4"),
														NHibernateUtil.Decimal,
														Projections.Property<OrderItem>(x => x.ActualCount),
														Projections.Property<OrderItem>(x => x.Count),
														Projections.Property<OrderItem>(x => x.Price),
														Projections.Property<OrderItem>(x => x.DiscountMoney)
													)
												)
											);

			query.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				 .Left.JoinAlias(o => o.DeliverySchedule, () => deliveryScheduleAlias)
				 .Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				 .Left.JoinAlias(o => o.Author, () => authorAlias)
				 .Left.JoinAlias(o => o.LastEditor, () => lastEditorAlias)
				 .Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias);

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
					() => new OrderDlg(),
					//функция диалога открытия документа
					(OrderJournalNode node) => new OrderDlg(node.Id),
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
				|| FilterViewModel.RestrictDeliveryPoint != null
				|| FilterViewModel.RestrictOnlyService != null
				|| FilterViewModel.RestrictOnlySelfDelivery != null
				|| FilterViewModel.RestrictLessThreeHours == true
				|| FilterViewModel.OrderPaymentStatus != null)
			{
				query.Where(o => o.Id == -1);
			}
			
			if(FilterViewModel.RestrictStartDate != null) {
				query.Where(o => o.CreateDate >= FilterViewModel.RestrictStartDate);
			}

			if(FilterViewModel.RestrictEndDate != null) {
				query.Where(o => o.CreateDate <= FilterViewModel.RestrictEndDate.Value.AddDays(1).AddTicks(-1));
			}
			
			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
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
						commonServices
					),
					//функция диалога открытия документа
					(OrderJournalNode node) => new OrderWithoutShipmentForDebtViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						commonServices
					),
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
			    || FilterViewModel.RestrictDeliveryPoint != null
			    || FilterViewModel.RestrictOnlyService != null
				|| FilterViewModel.RestrictOnlySelfDelivery != null
			    || FilterViewModel.RestrictLessThreeHours == true
			    || FilterViewModel.OrderPaymentStatus != null)
			{
				query.Where(o => o.Id == -1);
			}
			
			if(FilterViewModel.RestrictStartDate != null) {
				query.Where(o => o.CreateDate >= FilterViewModel.RestrictStartDate);
			}

			if(FilterViewModel.RestrictEndDate != null) {
				query.Where(o => o.CreateDate <= FilterViewModel.RestrictEndDate.Value.AddDays(1).AddTicks(-1));
			}

			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
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
						commonServices
					),
					//функция диалога открытия документа
					(OrderJournalNode node) => new OrderWithoutShipmentForPaymentViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						commonServices
					),
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
			    || FilterViewModel.RestrictDeliveryPoint != null
			    || FilterViewModel.RestrictOnlyService != null
			    || FilterViewModel.RestrictOnlySelfDelivery != null
			    || FilterViewModel.RestrictLessThreeHours == true
			    || FilterViewModel.OrderPaymentStatus != null)
			{
				query.Where(o => o.Id == -1);
			}
			
			if(FilterViewModel.RestrictStartDate != null) {
				query.Where(o => o.CreateDate >= FilterViewModel.RestrictStartDate);
			}

			if(FilterViewModel.RestrictEndDate != null) {
				query.Where(o => o.CreateDate <= FilterViewModel.RestrictEndDate.Value.AddDays(1).AddTicks(-1));
			}
			
			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
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
						commonServices,
						employeeService,
						nomenclatureSelectorFactory,
						counterpartySelectorFactory,
						nomenclatureRepository,
						userRepository
					),
					//функция диалога открытия документа
					(OrderJournalNode node) => new OrderWithoutShipmentForAdvancePaymentViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						commonServices,
						employeeService,
						nomenclatureSelectorFactory,
						counterpartySelectorFactory,
						nomenclatureRepository,
						userRepository
					),
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
			bool IsOrder(object[] objs) 
			{
				var selectedNodes = objs.Cast<OrderJournalNode>();
				if(selectedNodes.Count() != 1)
					return false;

				return selectedNodes.FirstOrDefault().EntityType == typeof(VodovozOrder);
			}

			PopupActionsList.Add(
				new JournalAction(
					"Перейти в маршрутный лист",
					selectedItems => selectedItems.Any(
						x => AccessRouteListKeeping((x as OrderJournalNode).Id)) && IsOrder(selectedItems),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var addresses = UoW.Session.QueryOver<RouteListItem>()
							.Where(x => x.Order.Id.IsIn(selectedNodes.Select(n => n.Id).ToArray())).List();

						var routes = addresses.GroupBy(x => x.RouteList.Id);

						var tdiMain = MainClass.MainWin.TdiMain;

						foreach(var route in routes) {
							tdiMain.OpenTab(
								DialogHelper.GenerateDialogHashName<RouteList>(route.Key),
								() => new RouteListKeepingDlg(route.Key, route.Select(x => x.Order.Id).ToArray())
							);
						}
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Перейти в недовоз",
					(selectedItems) => selectedItems.Any(
						o => UndeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, (o as OrderJournalNode).Id).Any()) && IsOrder(selectedItems),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var order = UoW.GetById<VodovozOrder>(selectedNodes.FirstOrDefault().Id);
						UndeliveriesView dlg = new UndeliveriesView();
						dlg.HideFilterAndControls();
						dlg.UndeliveredOrdersFilter.SetAndRefilterAtOnce(
							x => x.ResetFilter(),
							x => x.RestrictOldOrder = order,
							x => x.RestrictOldOrderStartDate = order.DeliveryDate,
							x => x.RestrictOldOrderEndDate = order.DeliveryDate
						);
						MainClass.MainWin.TdiMain.AddTab(dlg);
					}
				)
			);
			PopupActionsList.Add(
				new JournalAction(
					"Открыть диалог закрытия",
					(selectedItems) => selectedItems.Any(
						x => AccessToRouteListClosing((x as OrderJournalNode).Id)) && IsOrder(selectedItems),
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var routeListIds = selectedNodes.Select(x => x.Id).ToArray();
						var addresses = UoW.Session.QueryOver<RouteListItem>()
							.Where(x => x.Order.Id.IsIn(routeListIds)).List();

						var routes = addresses.GroupBy(x => x.RouteList.Id);
						var tdiMain = MainClass.MainWin.TdiMain;

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
					IsOrder,
					selectedItems => true,
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
					IsOrder,
					selectedItems => true,
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
					IsOrder,
					selectedItems => true,
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
					IsOrder,
					selectedItems => true,
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var order = UoW.GetById<VodovozOrder>(selectedNodes.FirstOrDefault().Id);
					
						var dlg = new OrderDlg();
						dlg.CopyLesserOrderFrom(order.Id);
						var tdiMain = MainClass.MainWin.TdiMain;
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
	}
}
