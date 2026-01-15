using Autofac;
using DateTimeHelpers;
using Gamma.Widgets;
using MoreLinq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DateTimeHelpers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Dialogs.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using Vodovoz.ViewModels.ViewModels.Reports.Orders;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;
using VodovozOrder = Vodovoz.Domain.Orders.Order;
using QS.Deletion;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Settings.Delivery;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.Documents;

namespace Vodovoz.JournalViewModels
{
	public class OrderJournalViewModel : FilterableMultipleEntityJournalViewModelBase<OrderJournalNode, OrderJournalFilterViewModel>
	{
		private const int _minLengthLikeSearch = 3;

		private readonly ICommonServices _commonServices;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly bool _userHasAccessToRetail = false;
		private readonly bool _userCanExportOrdersToExcel = false;
		private readonly IGtkTabsOpener _gtkDialogsOpener;
		private readonly bool _userHasOnlyAccessToWarehouseAndComplaints;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly IFileDialogService _fileDialogService;
		private readonly int _closingDocumentDeliveryScheduleId;
		private readonly bool _userCanPrintManyOrdersDocuments;
		private ILifetimeScope _lifetimeScope;
		private bool _isOrdersExportToExcelInProcess;

		public OrderJournalViewModel(
			OrderJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			INomenclatureRepository nomenclatureRepository,
			IGtkTabsOpener gtkDialogsOpener,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IFileDialogService fileDialogService,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			Action<OrderJournalFilterViewModel> filterConfiguration = null) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_gtkDialogsOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));
			_undeliveredOrdersRepository =
				undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_closingDocumentDeliveryScheduleId =
				(deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings)))
				.ClosingDocumentDeliveryScheduleId;

			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			TabName = "Журнал заказов";

			_userHasAccessToRetail = commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");
			_userHasOnlyAccessToWarehouseAndComplaints =
				commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
				&& !commonServices.UserService.GetCurrentUser().IsAdmin;
			_userCanPrintManyOrdersDocuments = commonServices.CurrentPermissionService.ValidatePresetPermission("can_print_many_orders_documents");
			_userCanExportOrdersToExcel = commonServices.CurrentPermissionService.ValidatePresetPermission("can_export_orders_to_excel");

			SearchEnabled = false;

			filterViewModel.Journal = this;
			JournalFilter = filterViewModel;

			UseSlider = false;

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

			if(filterConfiguration != null)
			{
				FilterViewModel.ConfigureWithoutFiltering(filterConfiguration);
			}
		}

		public bool IsOrdersExportToExcelInProcess
		{
			get => _isOrdersExportToExcelInProcess;
			private set
			{
				SetField(ref _isOrdersExportToExcelInProcess, value);
				UpdateJournalActions();
			}
		}

		public ILifetimeScope Scope => _lifetimeScope;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateCustomEditAction();
			CreateCustomDeleteAction();
			CreatePrintOrdersDocumentsAction();
			CreateExportToExcelAction();
		}

		private void CreateCustomDeleteAction()
		{
			var deleteAction = new JournalAction("Удалить",
				(selected) => {
					var selectedNodes = selected.OfType<OrderJournalNode>();
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
					var selectedNodes = selected.OfType<OrderJournalNode>();
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

		private void CreatePrintOrdersDocumentsAction()
		{
			var printOrdersDocumentsAction = new JournalAction(
				"Печать документов",
				(selected) => _userCanPrintManyOrdersDocuments,
				(selected) => _userCanPrintManyOrdersDocuments,
				(selected) => 
				{
					var ordersCount = 
						GetOrdersQuery(UoW)
						.Take(101)
						.List<OrderJournalNode>()
						.Count();

					if(ordersCount < 1)
					{
						_commonServices.InteractiveService.ShowMessage(
							ImportanceLevel.Info,
							"Для отправки на печать в списке должны быть заказы");

						return;
					}

					if(ordersCount > 100)
					{
						_commonServices.InteractiveService.ShowMessage(
							ImportanceLevel.Info,
							"Слишком много заказов в списке. Количество заказов не должно превышать 100");

						return;
					}

					var fileredOrderIds = GetOrdersQuery(UoW)
						.List<OrderJournalNode>()
						.Select(n => n.Id);

					var orders = UoW.GetAll<VodovozOrder>()
						.Where(o => fileredOrderIds.Contains(o.Id))
						.OrderByDescending(o => o.CreateDate)
						.ToList();

					var clientsCount = orders
						.Select(o => o.Client.Id)
						.Distinct()
						.Count();

					if(clientsCount > 1)
					{
						_commonServices.InteractiveService.ShowMessage(
							ImportanceLevel.Info,
							"В списке присутствуют заказы разных контрагентов. Необходимо выбрать заказы одного контрагента");

						return;
					}

					NavigationManager.OpenViewModel<PrintOrdersDocumentsViewModel, IList<VodovozOrder>>(null, orders);
				}
			);

			NodeActionsList.Add(printOrdersDocumentsAction);
		}

		private void CreateExportToExcelAction()
		{
			var createExportToExcelAction = new JournalAction(
				"Выгрузить в Excel",
				(selected) => !IsOrdersExportToExcelInProcess,
				(selected) => _userCanExportOrdersToExcel,
				async (selected) => await ExportToExcel()
			);
			NodeActionsList.Add(createExportToExcelAction);
		}

		private async Task ExportToExcel()
		{
			if(FilterViewModel.StartDate == null || FilterViewModel.EndDate == null)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Слишком много данных. Выберете временной диапазон для формирования выгрузки");

				return;
			}

			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{Title} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

			var saveDialogResul = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(!saveDialogResul.Successful)
			{
				return;
			}

			IsOrdersExportToExcelInProcess = true;

			await Task.Run(() =>
			{
				var nodes = GetReportData();

				var ordersReport = new OrdersReport(
						FilterViewModel.StartDate.Value,
						FilterViewModel.EndDate.Value,
						nodes);

				ordersReport.Export(saveDialogResul.Path);
			});

			IsOrdersExportToExcelInProcess = false;
		}

		private IEnumerable<OrderJournalNode> GetReportData()
		{
			var ordersRows = 
				GetOrdersQuery(UoW).List<OrderJournalNode<VodovozOrder>>();

			var ordersWithoutShipmentForDebtRows = 
				GetOrdersWithoutShipmentForDebtQuery(UoW).List<OrderJournalNode<OrderWithoutShipmentForDebt>>();

			var ordersWithoutShipmentForPaymentRows = 
				GetOrdersWithoutShipmentForPaymentQuery(UoW).List<OrderJournalNode<OrderWithoutShipmentForPayment>>();

			var ordersWithoutShipmentForAdvancePaymentRows = 
				GetOrdersWithoutShipmentForAdvancePaymentQuery(UoW).List<OrderJournalNode<OrderWithoutShipmentForAdvancePayment>>();

			IEnumerable<OrderJournalNode> orderJournalNodes = new List<OrderJournalNode>();
			orderJournalNodes = orderJournalNodes.SortedMerge(
					OrderByDirection.Descending,
					Comparer<OrderJournalNode>.Create((x, y) => x.CreateDate > y.CreateDate ? 1 : x.CreateDate < y.CreateDate ? -1 : 0),
					ordersRows,
					ordersWithoutShipmentForDebtRows,
					ordersWithoutShipmentForPaymentRows,
					ordersWithoutShipmentForAdvancePaymentRows);

			return orderJournalNodes;
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
			GeoGroup selfDeliveryGeographicalGroupAlias = null;
			EdoContainer edoContainerAlias = null;
			EdoContainer innerEdoContainerAlias = null;
			FormalEdoRequest edoRequestAlias = null;
			FormalEdoRequest edoRequestAlias2 = null;
			OrderEdoDocument orderEdoDocumentAlias = null;
			OrderEdoDocument orderEdoDocumentAlias2 = null;
			Employee salesManagerAlias = null;
			OrderDocument orderDocumentAlias = null;
			DocumentOrganizationCounter documentOrganizationCounterAlias = null;
			
			var query = uow.Session.QueryOver<VodovozOrder>(() => orderAlias)
				.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => counterpartyAlias.SalesManager, () => salesManagerAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicalGroupAlias)
				.Left.JoinAlias(() => orderAlias.SelfDeliveryGeoGroup, () => selfDeliveryGeographicalGroupAlias)
				.Left.JoinAlias(
					o => o.OrderDocuments,
					() => orderDocumentAlias, 
					Restrictions.And(
						Restrictions.Or(
							Restrictions.Where(() => orderDocumentAlias.GetType() == typeof(UPDDocument)),
							Restrictions.Where(() => orderDocumentAlias.GetType() == typeof(SpecialUPDDocument))
							),
							Restrictions.Where(() => orderDocumentAlias.Order.Id == orderAlias.Id)))
				.Left.JoinAlias(
					() => orderDocumentAlias.DocumentOrganizationCounter,
					() => documentOrganizationCounterAlias
				);

			if (FilterViewModel.SalesManager != null)
			{
				query.Where(() => salesManagerAlias.Id == FilterViewModel.SalesManager.Id);
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

			var startDate = FilterViewModel.StartDate;
			if(startDate != null)
			{
				if(FilterViewModel.FilterDateType == OrdersDateFilterType.DeliveryDate)
				{
					query.Where(o => o.DeliveryDate >= startDate);
				}
				else 
				{ 
					query.Where(o => o.CreateDate >= startDate); 
				}
			}

			var endDate = FilterViewModel.EndDate;
			if(endDate != null)
			{
				if(FilterViewModel.FilterDateType == OrdersDateFilterType.DeliveryDate)
				{ 
					query.Where(o => o.DeliveryDate <= endDate.Value.LatestDayTime());
				}
				else
				{ 
					query.Where(o => o.CreateDate <= endDate.Value.LatestDayTime());
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
				query.Where(() => (!orderAlias.SelfDelivery && geographicalGroupAlias.Id == FilterViewModel.GeographicGroup.Id)
						|| (orderAlias.SelfDelivery && selfDeliveryGeographicalGroupAlias.Id == FilterViewModel.GeographicGroup.Id));
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
				query.Where(() => orderAlias.OnlinePaymentNumber == FilterViewModel.OnlineOrderId);
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

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.CounterpartyInn))
			{
				query.Where(() => counterpartyAlias.INN == FilterViewModel.CounterpartyInn);
			}

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.UpdDocumentNumber))
			{
				query.Where(Restrictions.Like(
						Projections.Property(() => documentOrganizationCounterAlias.DocumentNumber), 
						FilterViewModel.UpdDocumentNumber, 
						MatchMode.Anywhere))
					.And(Restrictions.IsNotNull(
						Projections.Property(() => orderDocumentAlias.Id)));
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

			var edoUpdLastRecordIdByOrderSubquery = QueryOver.Of(()=> innerEdoContainerAlias)
				.Where(() => innerEdoContainerAlias.Order.Id == orderAlias.Id)
				.And(() => innerEdoContainerAlias.Type == DocumentContainerType.Upd)
				.Select(Projections.Max(()=> innerEdoContainerAlias.Id));

			var edoUpdLastStatusSubquery = QueryOver.Of(() => edoContainerAlias)
					.Where(() => edoContainerAlias.Order.Id == orderAlias.Id)
					.And(() => edoContainerAlias.Type == DocumentContainerType.Upd)
					.WithSubquery.WhereProperty(() => edoContainerAlias.Id).Eq(edoUpdLastRecordIdByOrderSubquery)
					.Select(Projections.Property(() => edoContainerAlias.EdoDocFlowStatus));

			var edoUpdLastStatusNewDocflowSubquery = QueryOver.Of(() => edoRequestAlias)
				.JoinEntityAlias(
					() => edoRequestAlias2,
					() => edoRequestAlias2.Order.Id == edoRequestAlias.Order.Id
						&& edoRequestAlias2.Id > edoRequestAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => orderEdoDocumentAlias, () => edoRequestAlias.Task.Id == orderEdoDocumentAlias.DocumentTaskId)
				.JoinEntityAlias(
					() => orderEdoDocumentAlias2,
					() => orderEdoDocumentAlias2.DocumentTaskId == orderEdoDocumentAlias.DocumentTaskId
						&& orderEdoDocumentAlias2.Id > orderEdoDocumentAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(() => edoRequestAlias.Order.Id == orderAlias.Id)
				.And(() => edoRequestAlias.DocumentType == EdoDocumentType.UPD)
				.And(() => edoRequestAlias2.Id == null)
				.And(() => orderEdoDocumentAlias2.Id == null)
				.Select(Projections.Property(() => orderEdoDocumentAlias.Status))
				.Take(1);

			if(FilterViewModel.EdoDocFlowStatus is EdoDocFlowStatus edoDocFlowStatus)
			{
				var edoFlowStateRestriction = Restrictions.Disjunction()
					.Add(Restrictions.Eq(Projections.SubQuery(edoUpdLastStatusSubquery), edoDocFlowStatus.ToString()))
					.Add(Restrictions.Eq(Projections.SubQuery(edoUpdLastStatusNewDocflowSubquery), edoDocFlowStatus.ToString()));

				query.Where(edoFlowStateRestriction);
			}

			if(FilterViewModel.EdoDocFlowStatus is SpecialComboState specialComboState && specialComboState == SpecialComboState.Not)
			{
				var edoFlowStateRestriction = Restrictions.Conjunction()
					.Add(Restrictions.IsNull(Projections.SubQuery(edoUpdLastStatusSubquery)))
					.Add(Restrictions.IsNull(Projections.SubQuery(edoUpdLastStatusNewDocflowSubquery)));

				query.Where(edoFlowStateRestriction);
			}

			query.Left.JoinAlias(o => o.DeliverySchedule, () => deliveryScheduleAlias)
					.Left.JoinAlias(o => o.Author, () => authorAlias)
					.Left.JoinAlias(o => o.LastEditor, () => lastEditorAlias)
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
					.Select(() => documentOrganizationCounterAlias.DocumentNumber).WithAlias(() => resultAlias.UpdDocumentName)
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
					.SelectSubQuery(edoUpdLastStatusSubquery).WithAlias(() => resultAlias.EdoDocFlowStatus)
					.SelectSubQuery(edoUpdLastStatusNewDocflowSubquery).WithAlias(() => resultAlias.NewEdoDocFlowStatus)
				)
				.OrderBy(x => x.CreateDate).Desc;

			if(FilterViewModel.FilterDateType == OrdersDateFilterType.DeliveryDate
				&& FilterViewModel.StartDate != null
				&& FilterViewModel.EndDate != null)
			{
				resultQuery.OrderBy(x => x.DeliveryDate).Desc();
			}

			resultQuery
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
			Employee salesManagerAlias = null;
			
			var query = uow.Session.QueryOver(() => orderWSDAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => counterpartyAlias.SalesManager, () => salesManagerAlias);

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
				|| FilterViewModel.SearchByAddressViewModel?.SearchValues?.Length > 0
				|| FilterViewModel.FilterClosingDocumentDeliverySchedule.HasValue)
			{
				query.Where(o => o.Id == -1);
			}

			var startDate = FilterViewModel.StartDate;
			if (startDate != null) {
				query.Where(o => o.CreateDate >= startDate);
			}

			var endDate = FilterViewModel.EndDate;
			if(endDate != null) {
				query.Where(o => o.CreateDate <= endDate.Value.LatestDayTime());
			}
			
			if (FilterViewModel.SalesManager != null)
			{
				query.Where(() => salesManagerAlias.Id == FilterViewModel.SalesManager.Id);
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
					() => _lifetimeScope.Resolve<OrderWithoutShipmentForDebtViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForCreate())),
					//функция диалога открытия документа
					(OrderJournalNode node) => _lifetimeScope.Resolve<OrderWithoutShipmentForDebtViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(node.Id))),
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
			Employee salesManagerAlias = null;
			
			var query = uow.Session.QueryOver(() => orderWSPAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.Left.JoinAlias(o => o.Author, () => authorAlias)
				.Left.JoinAlias(() => counterpartyAlias.SalesManager, () => salesManagerAlias);

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
				|| FilterViewModel.SearchByAddressViewModel?.SearchValues?.Length > 0
				|| FilterViewModel.FilterClosingDocumentDeliverySchedule.HasValue)
			{
				query.Where(o => o.Id == -1);
			}
			
			var startDate = FilterViewModel.StartDate;
			if (startDate != null) {
				query.Where(o => o.CreateDate >= startDate);
			}

			var endDate = FilterViewModel.EndDate;
			if(endDate != null) {
				query.Where(o => o.CreateDate <= endDate.Value.LatestDayTime());
			}

			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
			}
			
			if (FilterViewModel.SalesManager != null)
			{
				query.Where(() => salesManagerAlias.Id == FilterViewModel.SalesManager.Id);
			}

			if(FilterViewModel.Author != null)
			{
				query.Where(o => o.Author == FilterViewModel.Author);
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
					.Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
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
					() => _lifetimeScope.Resolve<OrderWithoutShipmentForPaymentViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForCreate())),
					//функция диалога открытия документа
					(OrderJournalNode node) => _lifetimeScope.Resolve<OrderWithoutShipmentForPaymentViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(node.Id))),
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
			Employee salesManagerAlias = null;

			var query = uow.Session.QueryOver(() => orderWSAPAlias)
				.Left.JoinAlias(o => o.Client, () => counterpartyAlias)
				.Left.JoinAlias(o => o.Author, () => authorAlias)
				.Left.JoinAlias(() => counterpartyAlias.SalesManager, () => salesManagerAlias);

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
				|| FilterViewModel.SearchByAddressViewModel?.SearchValues?.Length > 0
				|| FilterViewModel.FilterClosingDocumentDeliverySchedule.HasValue)
			{
				query.Where(o => o.Id == -1);
			}

			var startDate = FilterViewModel.StartDate;
			if (startDate != null) {
				query.Where(o => o.CreateDate >= startDate);
			}

			var endDate = FilterViewModel.EndDate;
			if(endDate != null) {
				query.Where(o => o.CreateDate <= endDate.Value.LatestDayTime());
			}
			
			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
			}
			
			if (FilterViewModel.SalesManager != null)
			{
				query.Where(() => salesManagerAlias.Id == FilterViewModel.SalesManager.Id);
			}

			if(FilterViewModel.Author != null)
			{
				query.Where(o => o.Author == FilterViewModel.Author);
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
					.Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
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
					() => _lifetimeScope.Resolve<OrderWithoutShipmentForAdvancePaymentViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForCreate())),
					//функция диалога открытия документа
					(OrderJournalNode node) => _lifetimeScope.Resolve<OrderWithoutShipmentForAdvancePaymentViewModel>(
						new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(node.Id))),
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
						o => _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, (o as OrderJournalNode).Id).Any()) 
							&& IsOrder(GetSelectedNode(selectedItems))
							&& !_userHasOnlyAccessToWarehouseAndComplaints,
					selectedItems => selectedItems.All(x => (x as OrderJournalNode).Sensitive),
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<OrderJournalNode>();
						var order = UoW.GetById<VodovozOrder>(selectedNodes.FirstOrDefault().Id);

						var page = NavigationManager.OpenViewModel<UndeliveredOrdersJournalViewModel, Action<UndeliveredOrdersFilterViewModel>>(this, filter =>
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
									"https://maps.yandex.ru/?text={0} {1} {2} {3}",
									order.DeliveryPoint.City,
									order.DeliveryPoint.StreetType,
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
						var selectedNode = selectedItems.Cast<OrderJournalNode>().FirstOrDefault();

						if(selectedNode is null)
						{
							return;
						}
						
						var order = UoW.GetById<VodovozOrder>(selectedNode.Id);
						_gtkDialogsOpener.OpenCopyLesserOrderDlg(this, order.Id);
					}
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Создать рекламацию",
					selectedItems => CanCreateComplaint(selectedItems),
					selectedItems => true,
					selectedItems =>
					{
						var selectedNodes = selectedItems.OfType<OrderJournalNode>().ToList();
						
						if(selectedNodes.Count != 1)
						{
							return;
						}
						
						var selectedOrder = selectedNodes.First();

						var complaintViewModel =
							NavigationManager.OpenViewModel<CreateComplaintViewModel, IEntityUoWBuilder>(
								this, EntityUoWBuilder.ForCreate()).ViewModel;
						complaintViewModel.SetOrder(selectedOrder.Id);
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

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
