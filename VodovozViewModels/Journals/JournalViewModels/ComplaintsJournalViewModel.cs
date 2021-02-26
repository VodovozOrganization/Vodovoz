using System;
using System.Collections;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.FilterViewModels;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Services;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Journals.JournalViewModels
{
	public class ComplaintsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<ComplaintJournalNode, ComplaintFilterViewModel>, IComplaintsInfoProvider
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly ICommonServices commonServices;
		private readonly IUndeliveriesViewOpener undeliveriesViewOpener;
		private readonly IEmployeeService employeeService;
		private readonly IEntityAutocompleteSelectorFactory employeeSelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory counterpartySelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory;
		private readonly IFilePickerService filePickerService;
		private readonly ISubdivisionRepository subdivisionRepository;
		private readonly IRouteListItemRepository routeListItemRepository;
		private readonly ISubdivisionService subdivisionService;
		private readonly IReportViewOpener reportViewOpener;
		private readonly IGtkTabsOpenerForRouteListViewAndOrderView gtkDlgOpener;
		private readonly INomenclatureRepository nomenclatureRepository;
		private readonly IUserRepository userRepository;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		private bool canCloseComplaint = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_complaints");

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.ComplaintPanelView };

		public ComplaintFilterViewModel ComplaintsFilterViewModel => FilterViewModel;

		public ComplaintsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IUndeliveriesViewOpener undeliveriesViewOpener,
			IEmployeeService employeeService,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory,
			IRouteListItemRepository routeListItemRepository,
			ISubdivisionService subdivisionService,
			ComplaintFilterViewModel filterViewModel,
			IFilePickerService filePickerService,
			ISubdivisionRepository subdivisionRepository,
			IReportViewOpener reportViewOpener,
			IGtkTabsOpenerForRouteListViewAndOrderView gtkDialogsOpener,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository
		) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.undeliveriesViewOpener = undeliveriesViewOpener ?? throw new ArgumentNullException(nameof(undeliveriesViewOpener));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			this.nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			this.routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			this.subdivisionService = subdivisionService ?? throw new ArgumentNullException(nameof(subdivisionService));
			this.reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
			this.gtkDlgOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

			TabName = "Журнал рекламаций";

			RegisterComplaints();

			var threadLoader = DataLoader as ThreadDataLoader<ComplaintJournalNode>;
			threadLoader.MergeInOrderBy(x => x.Id, true);

			FinishJournalConfiguration();

			FilterViewModel.SubdivisionService = subdivisionService;
			FilterViewModel.EmployeeService = employeeService;

			var currentEmployeeSubdivision = employeeService.GetEmployeeForUser(UoW, commonServices.UserService.CurrentUserId).Subdivision;
			if(currentEmployeeSubdivision != null) {
				if(FilterViewModel.SubdivisionService.GetOkkId() != currentEmployeeSubdivision.Id)
					FilterViewModel.Subdivision = currentEmployeeSubdivision;
				else
					FilterViewModel.ComplaintStatus = ComplaintStatuses.Checking;
			}

			UpdateOnChanges(
				typeof(Complaint),
				typeof(ComplaintGuiltyItem),
				typeof(ComplaintResult),
				typeof(Subdivision),
				typeof(ComplaintDiscussion),
				typeof(DeliveryPoint),
				typeof(Fine),
				typeof(Order),
				typeof(RouteList),
				typeof(RouteListItem)
			);
			this.DataLoader.ItemsListUpdated += (sender, e) => CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(null));

			DataLoader.PostLoadProcessingFunc = BeforeItemsUpdated;
		}

		private IQueryOver<Complaint> GetComplaintQuery(IUnitOfWork uow)
		{
			ComplaintJournalNode resultAlias = null;

			Complaint complaintAlias = null;
			Employee authorAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			ComplaintGuiltyItem complaintGuiltyItemAlias = null;
			Employee guiltyEmployeeAlias = null;
			Subdivision guiltySubdivisionAlias = null;
			Fine fineAlias = null;
			Order orderAlias = null;
			ComplaintDiscussion discussionAlias = null;
			Subdivision subdivisionAlias = null;
			ComplaintKind complaintKindAlias = null;
			Subdivision superspecialAlias = null;

			var authorProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var workInSubdivisionsSubQuery = QueryOver.Of<Subdivision>(() => subdivisionAlias)
				.Where(() => subdivisionAlias.Id == discussionAlias.Subdivision.Id)
				.Where(() => discussionAlias.Status == ComplaintStatuses.InProcess)
				.Select(Projections.Conditional(
					Restrictions.IsNotNull(Projections.Property(() => subdivisionAlias.ShortName)),
					Projections.Property(() => subdivisionAlias.ShortName),
					Projections.Constant("?")
				)
			);

			var subdivisionsSubqueryProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				Projections.SubQuery(workInSubdivisionsSubQuery),
				Projections.Constant(", "));

			var okkProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1)"),
				NHibernateUtil.String,
				Projections.Conditional(
					Restrictions.Eq(Projections.Property(() => discussionAlias.Status), ComplaintStatuses.Checking),
					Projections.Constant("ОКК"),
					Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "NULLIF(1,1)"),
						NHibernateUtil.String
					)
				)
			);

			string okkSubdivision = uow.GetById<Subdivision>(subdivisionService.GetOkkId()).ShortName ?? "?";

			var workInSubdivisionsProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(',', ?1, IF(?2 = 'Checking',?3, ''))"),
				NHibernateUtil.String,
				subdivisionsSubqueryProjection,
				Projections.Property(() => complaintAlias.Status),
				Projections.Constant(okkSubdivision)
			);

			var plannedCompletionDateProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT DATE_FORMAT(?1, \"%d.%m.%Y\") SEPARATOR ?2)"),
				NHibernateUtil.String,
				Projections.Property(() => discussionAlias.PlannedCompletionDate),
				Projections.Constant("\n"));

			var lastPlannedCompletionDateProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.DateTime, "MAX(DISTINCT ?1)"),
				NHibernateUtil.DateTime,
				Projections.Property(() => discussionAlias.PlannedCompletionDate));

			var counterpartyWithAddressProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS('\n', ?1, COMPILE_ADDRESS(?2))"),
				NHibernateUtil.String,
				Projections.Property(() => counterpartyAlias.Name),
				Projections.Property(() => deliveryPointAlias.Id));

			var guiltyEmployeeProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => guiltyEmployeeAlias.LastName),
				Projections.Property(() => guiltyEmployeeAlias.Name),
				Projections.Property(() => guiltyEmployeeAlias.Patronymic)
			);

			var guiltiesProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT " +
					"CASE ?1 " +
						$"WHEN '{nameof(ComplaintGuiltyTypes.Client)}' THEN 'Клиент' " +
						$"WHEN '{nameof(ComplaintGuiltyTypes.None)}' THEN 'Нет' " +
						$"WHEN '{nameof(ComplaintGuiltyTypes.Employee)}' THEN CONCAT('(',?5,')', ?2)" +
						$"WHEN '{nameof(ComplaintGuiltyTypes.Subdivision)}' THEN ?3 " +
						"ELSE '' " +
					"END" +
					" SEPARATOR ?4)"),
				NHibernateUtil.String,
				Projections.Property(() => complaintGuiltyItemAlias.GuiltyType),
				guiltyEmployeeProjection,
				Projections.Property(() => guiltySubdivisionAlias.ShortName),
				Projections.Constant("\n"),
				Projections.Property(() => superspecialAlias.ShortName));

			var finesProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT CONCAT(ROUND(?1, 2), ' р.')  SEPARATOR ?2)"),
				NHibernateUtil.String,
				Projections.Property(() => fineAlias.TotalMoney),
				Projections.Constant("\n"));

			var query = uow.Session.QueryOver(() => complaintAlias)
				.Left.JoinAlias(() => complaintAlias.CreatedBy, () => authorAlias)
				.Left.JoinAlias(() => complaintAlias.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(() => complaintAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => complaintAlias.Guilties, () => complaintGuiltyItemAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintKind, () => complaintKindAlias)
				.Left.JoinAlias(() => complaintAlias.Fines, () => fineAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintDiscussions, () => discussionAlias)
				.Left.JoinAlias(() => discussionAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Employee, () => guiltyEmployeeAlias)
				.Left.JoinAlias(() => guiltyEmployeeAlias.Subdivision, () => superspecialAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Subdivision, () => guiltySubdivisionAlias);

			#region Filter

			if(FilterViewModel != null) {
				if (FilterViewModel.IsForRetail != null)
				{
					query.Where(() => counterpartyAlias.IsForRetail == FilterViewModel.IsForRetail);
				}

				FilterViewModel.EndDate = FilterViewModel.EndDate.Date.AddHours(23).AddMinutes(59);
				if(FilterViewModel.StartDate.HasValue)
					FilterViewModel.StartDate = FilterViewModel.StartDate.Value.Date;

				QueryOver<ComplaintDiscussion, ComplaintDiscussion> dicussionQuery = null;

				if(FilterViewModel.Subdivision != null) {
					dicussionQuery = QueryOver.Of(() => discussionAlias)
						.Select(Projections.Property<ComplaintDiscussion>(p => p.Id))
						.Where(() => discussionAlias.Subdivision.Id == FilterViewModel.Subdivision.Id)
						.And(() => discussionAlias.Complaint.Id == complaintAlias.Id);
				}

				if(FilterViewModel.StartDate.HasValue) {
					switch (FilterViewModel.FilterDateType) {
						case DateFilterType.PlannedCompletionDate:
							if(dicussionQuery == null) {
								query = query.Where(() => complaintAlias.PlannedCompletionDate <= FilterViewModel.EndDate)
									.And(() => FilterViewModel.StartDate == null || complaintAlias.PlannedCompletionDate >= FilterViewModel.StartDate.Value);
							} else {
								dicussionQuery = dicussionQuery
									.And(() => FilterViewModel.StartDate == null || discussionAlias.PlannedCompletionDate >= FilterViewModel.StartDate.Value)
									.And(() => discussionAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
							}
							break;
						case DateFilterType.ActualCompletionDate:
							query = query.Where(() => complaintAlias.ActualCompletionDate <= FilterViewModel.EndDate)
								.And(() => FilterViewModel.StartDate == null || complaintAlias.ActualCompletionDate >= FilterViewModel.StartDate.Value);
							break;
						case DateFilterType.CreationDate:
							query = query.Where(() => complaintAlias.CreationDate <= FilterViewModel.EndDate)
								.And(() => FilterViewModel.StartDate == null || complaintAlias.CreationDate >= FilterViewModel.StartDate.Value);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				if(dicussionQuery != null)
					query.WithSubquery.WhereExists(dicussionQuery);
				if(FilterViewModel.ComplaintType != null)
					query = query.Where(() => complaintAlias.ComplaintType == FilterViewModel.ComplaintType);
				if(FilterViewModel.ComplaintStatus != null)
					query = query.Where(() => complaintAlias.Status == FilterViewModel.ComplaintStatus);
				if(FilterViewModel.Employee != null)
					query = query.Where(() => complaintAlias.CreatedBy.Id == FilterViewModel.Employee.Id);

				if (FilterViewModel.CurrentUserSubdivision != null 
					&& FilterViewModel.ComplaintCurrentUserSubdivisionStatus != null)
                {
					query = query.Where(() => discussionAlias.Subdivision.Id == FilterViewModel.CurrentUserSubdivision.Id)
						.And(() => discussionAlias.Status == FilterViewModel.ComplaintCurrentUserSubdivisionStatus);
				}

				if (FilterViewModel.GuiltyItemVM?.Entity?.GuiltyType != null) {
					var subquery = QueryOver.Of<ComplaintGuiltyItem>()
											.Where(g => g.GuiltyType == FilterViewModel.GuiltyItemVM.Entity.GuiltyType.Value);
					switch(FilterViewModel.GuiltyItemVM.Entity.GuiltyType) {
						case ComplaintGuiltyTypes.None:
						case ComplaintGuiltyTypes.Client:
							break;
						case ComplaintGuiltyTypes.Employee:
							if(FilterViewModel.GuiltyItemVM.Entity.Employee != null)
								subquery.Where(g => g.Employee.Id == FilterViewModel.GuiltyItemVM.Entity.Employee.Id);
							break;
						case ComplaintGuiltyTypes.Subdivision:
							if(FilterViewModel.GuiltyItemVM.Entity.Subdivision != null)
								subquery.Where(g => g.Subdivision.Id == FilterViewModel.GuiltyItemVM.Entity.Subdivision.Id);
							break;
						default:
							break;
					}
					query.WithSubquery.WhereProperty(x => x.Id).In(subquery.Select(x => x.Complaint));
				}

				if(FilterViewModel.ComplaintKind != null)
					query.Where(() => complaintAlias.ComplaintKind.Id == FilterViewModel.ComplaintKind.Id);
			}

			#endregion Filter

			query.Where(
					GetSearchCriterion(
					() => complaintAlias.Id,
					() => complaintAlias.ComplaintText,
					() => complaintAlias.ResultText,
					() => counterpartyAlias.Name,
					() => deliveryPointAlias.CompiledAddress
				)
			);

			query.SelectList(list => list
				.SelectGroup(() => complaintAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(() => complaintAlias.CreationDate).WithAlias(() => resultAlias.Date)
				.Select(() => complaintAlias.ComplaintType).WithAlias(() => resultAlias.Type)
				.Select(() => complaintAlias.Status).WithAlias(() => resultAlias.Status)
				.Select(workInSubdivisionsProjection).WithAlias(() => resultAlias.WorkInSubdivision)
				.Select(plannedCompletionDateProjection).WithAlias(() => resultAlias.PlannedCompletionDate)
				.Select(lastPlannedCompletionDateProjection).WithAlias(() => resultAlias.LastPlannedCompletionDate)
				.Select(counterpartyWithAddressProjection).WithAlias(() => resultAlias.ClientNameWithAddress)
				.Select(guiltiesProjection).WithAlias(() => resultAlias.Guilties)
				.Select(authorProjection).WithAlias(() => resultAlias.Author)
				.Select(finesProjection).WithAlias(() => resultAlias.Fines)
				.Select(() => complaintAlias.ComplaintText).WithAlias(() => resultAlias.ComplaintText)
				.Select(() => complaintKindAlias.Name).WithAlias(() => resultAlias.ComplaintKindString)
				.Select(() => complaintKindAlias.IsArchive).WithAlias(() => resultAlias.ComplaintKindIsArchive)
				.Select(() => complaintAlias.ResultText).WithAlias(() => resultAlias.ResultText)
				.Select(() => complaintAlias.ActualCompletionDate).WithAlias(() => resultAlias.ActualCompletionDate)
			);

			var result = query.TransformUsing(Transformers.AliasToBean<ComplaintJournalNode>())
				 .OrderBy(n => n.Id)
				 .Desc().List<ComplaintJournalNode>()
				 ;

			return query;
		}

		private void RegisterComplaints()
		{
			var complaintConfig = RegisterEntity<Complaint>(GetComplaintQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new CreateComplaintViewModel(
						EntityUoWBuilder.ForCreate(),
						unitOfWorkFactory,
						employeeService,
						employeeSelectorFactory,
						counterpartySelectorFactory,
						subdivisionRepository,
						commonServices,
						nomenclatureSelectorFactory,
						nomenclatureRepository,
						userRepository
					),
					//функция диалога открытия документа
					(ComplaintJournalNode node) => new ComplaintViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						unitOfWorkFactory,
						commonServices,
						undeliveriesViewOpener,
						employeeService,
						employeeSelectorFactory,
						counterpartySelectorFactory,
						filePickerService,
						subdivisionRepository,
						nomenclatureSelectorFactory,
						nomenclatureRepository,
						userRepository
					),
					//функция идентификации документа 
					(ComplaintJournalNode node) => {
						return node.EntityType == typeof(Complaint);
					},
					"Клиентская рекламация",
					new JournalParametersForDocument() { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new CreateInnerComplaintViewModel(
						EntityUoWBuilder.ForCreate(),
						unitOfWorkFactory,
						employeeService,
						subdivisionRepository,
						commonServices,
						employeeSelectorFactory
					),
					//функция диалога открытия документа
					(ComplaintJournalNode node) => new ComplaintViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						unitOfWorkFactory,
						commonServices,
						undeliveriesViewOpener,
						employeeService,
						employeeSelectorFactory,
						counterpartySelectorFactory,
						filePickerService,
						subdivisionRepository,
						nomenclatureSelectorFactory,
						nomenclatureRepository,
						userRepository
					),
					//функция идентификации документа 
					(ComplaintJournalNode node) => {
						return node.EntityType == typeof(Complaint);
					},
					"Внутренняя рекламация",
					new JournalParametersForDocument() { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);

			//завершение конфигурации
			complaintConfig.FinishConfiguration();
		}

		protected void BeforeItemsUpdated(IList items, uint start)
		{
			foreach(var item in items.Cast<ComplaintJournalNode>().Skip((int)start)) {
				item.SequenceNumber = items.IndexOf(item) + 1;
			}
		}

		protected override void CreatePopupActions()
		{
			Complaint GetComplaint(object[] objs)
			{
				var selectedNodes = objs.Cast<ComplaintJournalNode>();
				if(selectedNodes.Count() != 1)
					return null;
				var complaint = UoW.GetById<Complaint>(selectedNodes.FirstOrDefault().Id);
				return complaint;
			}

			Order GetOrder(object[] objs)
			{
				var complaint = GetComplaint(objs);
				return GetComplaint(objs)?.Order;
			}

			RouteList GetRouteList(object[] objs)
			{
				var order = GetOrder(objs);
				if(order == null)
					return null;
				var rl = routeListItemRepository.GetRouteListItemForOrder(UoW, order)?.RouteList;
				return rl;
			}

			bool HasOrder(object[] objs) => GetOrder(objs) != null;

			bool HasRouteList(object[] objs) => GetRouteList(objs) != null;

			PopupActionsList.Add(
				new JournalAction(
					"Открыть заказ",
					HasOrder,
					n => true,
					n => gtkDlgOpener.OpenOrderDlg(this, GetOrder(n).Id)
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Открыть маршрутный лист",
					HasRouteList,
					n => true,
					n => gtkDlgOpener.OpenCreateRouteListDlg(this, GetRouteList(n).Id)
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Создать штраф",
					n => EntityConfigs[typeof(Complaint)].PermissionResult.CanUpdate,
					n => EntityConfigs[typeof(Complaint)].PermissionResult.CanUpdate,
					n => {
						var currentComplaintId = n.OfType<ComplaintJournalNode>().FirstOrDefault()?.Id;
						ComplaintViewModel currentComplaintVM = null;
						if(currentComplaintId.HasValue) {
							currentComplaintVM = new ComplaintViewModel(
								EntityUoWBuilder.ForOpen(currentComplaintId.Value),
								unitOfWorkFactory,
								commonServices,
								undeliveriesViewOpener,
								employeeService,
								employeeSelectorFactory,
								counterpartySelectorFactory,
								filePickerService,
								subdivisionRepository,
								nomenclatureSelectorFactory,
								nomenclatureRepository,
								userRepository
							);
							currentComplaintVM.AddFineCommand.Execute(this);
						}
					}
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Закрыть рекламацию",
					n => n.OfType<ComplaintJournalNode>().FirstOrDefault()?.Status != ComplaintStatuses.Closed && canCloseComplaint,
					n => EntityConfigs[typeof(Complaint)].PermissionResult.CanUpdate && canCloseComplaint,
					n => {
						var currentComplaintId = n.OfType<ComplaintJournalNode>().FirstOrDefault()?.Id;
						ComplaintViewModel currentComplaintVM = null;
						if(currentComplaintId.HasValue) {
							currentComplaintVM = new ComplaintViewModel(
								EntityUoWBuilder.ForOpen(currentComplaintId.Value),
								unitOfWorkFactory,
								commonServices,
								undeliveriesViewOpener,
								employeeService,
								employeeSelectorFactory,
								counterpartySelectorFactory,
								filePickerService,
								subdivisionRepository,
								nomenclatureSelectorFactory,
								nomenclatureRepository,
								userRepository
							);
							string msg = string.Empty;
							if(!currentComplaintVM.Entity.Close(ref msg))
								ShowWarningMessage(msg, "Не удалось закрыть");
							else
								currentComplaintVM.Save();
						}
					}
				)
			);
		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();
			NodeActionsList.Add(new JournalAction("Открыть печатную форму", x => true, x => true, selectedItems => reportViewOpener.OpenReport(this, FilterViewModel.GetReportInfo())));
		}
	}
}
