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
using QS.Project.Journal.Search;
using QS.Project.Journal.Search.Criterion;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.FilterViewModels;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalNodes;
using Vodovoz.Services;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.JournalViewModels
{
	public class ComplaintsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<ComplaintJournalNode, ComplaintFilterViewModel, CriterionSearchModel>, IComplaintsInfoProvider
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly ICommonServices commonServices;
		private readonly IUndeliveriesViewOpener undeliveriesViewOpener;
		private readonly IEmployeeService employeeService;
		private readonly IEntityAutocompleteSelectorFactory employeeSelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory counterpartySelectorFactory;
		private readonly IFilePickerService filePickerService;
		private readonly ISubdivisionRepository subdivisionRepository;
		private readonly IRouteListItemRepository routeListItemRepository;
		private readonly ISubdivisionService subdivisionService;
		private readonly IEmployeeRepository employeeRepository;
		private readonly IReportViewOpener reportViewOpener;
		private readonly IGtkTabsOpenerForRouteListViewAndOrderView gtkDlgOpener;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.ComplaintPanelView };

		public ComplaintFilterViewModel ComplaintsFilterViewModel => FilterViewModel;

		public ComplaintsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IUndeliveriesViewOpener undeliveriesViewOpener,
			IEmployeeService employeeService,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			IRouteListItemRepository routeListItemRepository,
			ISubdivisionService subdivisionService,
			IEmployeeRepository employeeRepository,
			ComplaintFilterViewModel filterViewModel,
			IFilePickerService filePickerService,
			ISubdivisionRepository subdivisionRepository,
			IReportViewOpener reportViewOpener,
			IGtkTabsOpenerForRouteListViewAndOrderView gtkDialogsOpener,
			SearchViewModelBase<CriterionSearchModel> searchViewModel
		) : base(filterViewModel, unitOfWorkFactory, commonServices, searchViewModel)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.undeliveriesViewOpener = undeliveriesViewOpener ?? throw new ArgumentNullException(nameof(undeliveriesViewOpener));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			this.filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			this.routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			this.subdivisionService = subdivisionService ?? throw new ArgumentNullException(nameof(subdivisionService));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
			this.gtkDlgOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));

			TabName = "Журнал жалоб";

			RegisterComplaints();

			var threadLoader = DataLoader as ThreadDataLoader<ComplaintJournalNode>;
			threadLoader.MergeInOrderBy(x => x.Id, true);

			FinishJournalConfiguration();

			FilterViewModel.SubdivisionService = subdivisionService;
			FilterViewModel.EmployeeRepository = employeeRepository;

			var currentEmployeeSubdivision = employeeRepository.GetEmployeeForCurrentUser(UoW).Subdivision;
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

				if(FilterViewModel.FilterDateType == DateFilterType.CreationDate && FilterViewModel.StartDate.HasValue) {
					query = query.Where(() => complaintAlias.CreationDate <= FilterViewModel.EndDate)
								.And(() => FilterViewModel.StartDate == null || complaintAlias.CreationDate >= FilterViewModel.StartDate.Value);

					if(dicussionQuery != null)
						query.WithSubquery.WhereExists(dicussionQuery);

				} else if(FilterViewModel.FilterDateType == DateFilterType.PlannedCompletionDate && FilterViewModel.StartDate.HasValue) {
					if(dicussionQuery == null) {
						query = query.Where(() => complaintAlias.PlannedCompletionDate <= FilterViewModel.EndDate)
									.And(() => FilterViewModel.StartDate == null || complaintAlias.PlannedCompletionDate >= FilterViewModel.StartDate.Value);
					} else {
						dicussionQuery = dicussionQuery
										.And(() => FilterViewModel.StartDate == null || discussionAlias.PlannedCompletionDate >= FilterViewModel.StartDate.Value)
										.And(() => discussionAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
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

				if(FilterViewModel.GuiltyItemVM?.Entity?.GuiltyType != null) {
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

			query.Where(CriterionSearchModel.ConfigureSearch()
				.AddSearchBy(() => complaintAlias.Id)
				.AddSearchBy(() => complaintAlias.ComplaintText)
				.AddSearchBy(() => complaintAlias.ResultText)
				.AddSearchBy(() => counterpartyAlias.Name)
				.AddSearchBy(() => deliveryPointAlias.CompiledAddress)
				.GetSearchCriterion()
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
						commonServices
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
						subdivisionRepository
					),
					//функция идентификации документа 
					(ComplaintJournalNode node) => {
						return node.EntityType == typeof(Complaint);
					},
					"Клиентская жалоба",
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
						subdivisionRepository
					),
					//функция идентификации документа 
					(ComplaintJournalNode node) => {
						return node.EntityType == typeof(Complaint);
					},
					"Внутренняя жалоба",
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
								subdivisionRepository
							);
							currentComplaintVM.AddFineCommand.Execute(this);
						}
					}
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Закрыть жалобу",
					n => n.OfType<ComplaintJournalNode>().FirstOrDefault()?.Status != ComplaintStatuses.Closed,
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
								subdivisionRepository
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
