using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.Tdi;
using QS.ViewModels.Dialog;
using System;
using System.Collections;
using System.Linq;
using DateTimeHelpers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.NHibernateProjections.Employees;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Reports.ComplaintsJournalReport;
using static Vodovoz.FilterViewModels.ComplaintFilterViewModel;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Journals.JournalViewModels
{
	public class ComplaintsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<ComplaintJournalNode, ComplaintFilterViewModel>, IComplaintsInfoProvider
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly IEmployeeService _employeeService;
		private readonly IFileDialogService _fileDialogService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
		private readonly IGtkTabsOpener _gtkDlgOpener;
		private readonly IUserRepository _userRepository;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly IComplaintParametersProvider _complaintParametersProvider;
		private readonly IGeneralSettingsParametersProvider _generalSettingsParametersProvider;
		private readonly ILifetimeScope _scope;
		private string _subdivisionQualityServiceShortName;
		private string _subdivisionAuditDepartmentShortName;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		private bool _canCloseComplaint = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_complaints");

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.ComplaintPanelView };

		public ComplaintFilterViewModel ComplaintsFilterViewModel => FilterViewModel;

		public ComplaintsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeService employeeService,
			IRouteListItemRepository routeListItemRepository,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			ComplaintFilterViewModel filterViewModel,
			IFileDialogService fileDialogService,
			ISubdivisionRepository subdivisionRepository,
			IGtkTabsOpener gtkDialogsOpener,
			IUserRepository userRepository,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			IComplaintParametersProvider complaintParametersProvider,
			IGeneralSettingsParametersProvider generalSettingsParametersProvider,
			ILifetimeScope scope,
			Action<ComplaintFilterViewModel> filterConfig = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			_gtkDlgOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_deliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_complaintParametersProvider = complaintParametersProvider ?? throw new ArgumentNullException(nameof(complaintParametersProvider));
			_generalSettingsParametersProvider = generalSettingsParametersProvider ?? throw new ArgumentNullException(nameof(generalSettingsParametersProvider));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			ParentTab = (ITdiTab)this;

			TabName = "Журнал рекламаций";

			RegisterComplaints();

			var threadLoader = DataLoader as ThreadDataLoader<ComplaintJournalNode>;
			threadLoader.MergeInOrderBy(x => x.Id, true);

			FinishJournalConfiguration();

			FilterViewModel.EmployeeService = employeeService;

			var currentUserSettings = userRepository.GetUserSettings(UoW, commonServices.UserService.CurrentUserId);
			var defaultSubdivision = currentUserSettings.DefaultSubdivision;
			var currentEmployeeSubdivision = employeeService.GetEmployeeForUser(UoW, commonServices.UserService.CurrentUserId).Subdivision;

			if(FilterViewModel.CurrentUserSubdivision == null)
			{
				FilterViewModel.CurrentUserSubdivision = currentEmployeeSubdivision;
			}

			if(FilterViewModel.Subdivision == null)
			{
				if(currentUserSettings.UseEmployeeSubdivision)
				{
					FilterViewModel.Subdivision = currentEmployeeSubdivision;
				}
				else
				{
					FilterViewModel.Subdivision = defaultSubdivision;
				}
			}

			if(FilterViewModel.ComplaintStatus == null)
			{
				FilterViewModel.ComplaintStatus = currentUserSettings.DefaultComplaintStatus;
			}

			UpdateOnChanges(
				typeof(Complaint),
				typeof(ComplaintGuiltyItem),
				typeof(ComplaintResultOfCounterparty),
				typeof(ComplaintResultOfEmployees),
				typeof(Subdivision),
				typeof(ComplaintDiscussion),
				typeof(DeliveryPoint),
				typeof(Fine),
				typeof(Order),
				typeof(RouteList),
				typeof(RouteListItem),
				typeof(ComplaintObject),
				typeof(ComplaintKind),
				typeof(ComplaintDetalization));

			DataLoader.ItemsListUpdated += (sender, e) => CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(null));

			DataLoader.PostLoadProcessingFunc = BeforeItemsUpdated;
			UseSlider = false;

			if(filterConfig != null)
			{
				FilterViewModel.SetAndRefilterAtOnce(filterConfig);
			}
		}

		private ITdiTab _parrentTab;
		public ITdiTab ParentTab
		{
			get => _parrentTab;
			set
			{
				_parrentTab = value;
				FilterViewModel.JournalViewModel = (DialogViewModelBase)_parrentTab;
			}
		}

		private string SubdivisionQualityServiceShortName =>
			_subdivisionQualityServiceShortName ??
				(_subdivisionQualityServiceShortName =
					UoW.GetById<Subdivision>(_subdivisionParametersProvider.QualityServiceSubdivisionId).ShortName ?? "?"); // СК

		private string SubdivisionAuditDepartmentShortName =>
			_subdivisionAuditDepartmentShortName ??
				(_subdivisionAuditDepartmentShortName =
					UoW.GetById<Subdivision>(_subdivisionParametersProvider.AuditDepartmentSubdivisionId).ShortName ?? "?"); // КРО

		private IQueryOver<Complaint> GetComplaintQuery(IUnitOfWork uow)
		{
			ComplaintJournalNode resultAlias = null;

			Complaint complaintAlias = null;
			Employee authorAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			ComplaintGuiltyItem complaintGuiltyItemAlias = null;
			Employee guiltyEmployeeAlias = null;
			Employee driverAlias = null;
			Subdivision guiltySubdivisionAlias = null;
			Fine fineAlias = null;
			Order orderAlias = null;
			ComplaintDiscussion discussionAlias = null;
			Subdivision subdivisionAlias = null;
			ComplaintKind complaintKindAlias = null;
			ComplaintDetalization complaintDelatizationAlias = null;
			Subdivision superspecialAlias = null;
			ComplaintObject complaintObjectAlias = null;
			ComplaintResultOfCounterparty resultOfCounterpartyAlias = null;
			ComplaintResultOfEmployees resultOfEmployeesAlias = null;
			Responsible responsibleAlias = null;
			ComplaintArrangementComment resultOfComplaintArrangemenCommentAlias = null;
			ComplaintResultComment resultOfComplaintResultCommentAlias = null;

			var authorProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var workInSubdivisionsSubQuery = QueryOver.Of<Subdivision>(() => subdivisionAlias)
				.Where(() => subdivisionAlias.Id == discussionAlias.Subdivision.Id)
				.Where(() => discussionAlias.Status == ComplaintDiscussionStatuses.InProcess)
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

			var workInSubdivisionsCheckingProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(',', ?1, IF(?2 = 'Checking',?3, ''))"),
				NHibernateUtil.String,
				subdivisionsSubqueryProjection,
				Projections.Property(() => complaintAlias.Status),
				Projections.Constant(SubdivisionQualityServiceShortName)
			);

			var workInSubdivisionsWaitingForReactionProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(',', ?1, IF(?2 = 'WaitingForReaction',?3, ''))"),
				NHibernateUtil.String,
				subdivisionsSubqueryProjection,
				Projections.Property(() => complaintAlias.Status),
				Projections.Constant(SubdivisionAuditDepartmentShortName)
			);

			var workInSubdivisionProjection = Projections.Conditional(
				Restrictions.Eq(Projections.Property(() => complaintAlias.Status), ComplaintStatuses.Checking),
				workInSubdivisionsCheckingProjection,
				Projections.Conditional(
					Restrictions.Eq(Projections.Property(() => complaintAlias.Status), ComplaintStatuses.WaitingForReaction),
					workInSubdivisionsWaitingForReactionProjection,
					subdivisionsSubqueryProjection));

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
					$"WHEN '{_complaintParametersProvider.EmployeeResponsibleId}' THEN CONCAT('(',?5,')', ?2)" +
					$"WHEN '{_complaintParametersProvider.SubdivisionResponsibleId}' THEN ?3 " +
					$"ELSE ?6 " +
					"END " +
					" SEPARATOR ?4)"),
				NHibernateUtil.String,
				Projections.Property(() => responsibleAlias.Id),
				guiltyEmployeeProjection,
				Projections.Property(() => guiltySubdivisionAlias.ShortName),
				Projections.Constant("\n"),
				Projections.Property(() => superspecialAlias.ShortName),
				Projections.Property(() => responsibleAlias.Name));

			var finesProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT CONCAT(ROUND(?1, 2), ' р.')  SEPARATOR ?2)"),
				NHibernateUtil.String,
				Projections.Property(() => fineAlias.TotalMoney),
				Projections.Constant("\n"));

			var arrangementCommentProjection = Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.Property(nameof(resultOfComplaintArrangemenCommentAlias.Comment)),
						Projections.Constant(" || ")
						);

			var resultCommentProjection = Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.Property(nameof(resultOfComplaintResultCommentAlias.Comment)),
						Projections.Constant(" || ")
						);

			var resultOfCounterpartySubquery = QueryOver.Of(() => resultOfCounterpartyAlias)
				.Where(() => resultOfCounterpartyAlias.Id == complaintAlias.ComplaintResultOfCounterparty.Id)
				.Select(Projections.Property(() => resultOfCounterpartyAlias.Name));

			var resultOfEmployeesSubquery = QueryOver.Of(() => resultOfEmployeesAlias)
				.Where(() => resultOfEmployeesAlias.Id == complaintAlias.ComplaintResultOfEmployees.Id)
				.Select(Projections.Property(() => resultOfEmployeesAlias.Name));

			var resultOfArrangementCommentsSubquery = QueryOver.Of(() => resultOfComplaintArrangemenCommentAlias)
				.Where(() => resultOfComplaintArrangemenCommentAlias.Complaint.Id == complaintAlias.Id)
				.Select(arrangementCommentProjection);

			var resultOfResultCommentsSubquery = QueryOver.Of(() => resultOfComplaintResultCommentAlias)
				.Where(() => resultOfComplaintResultCommentAlias.Complaint.Id == complaintAlias.Id)
				.Select(resultCommentProjection);

			var query = uow.Session.QueryOver(() => complaintAlias)
				.Left.JoinAlias(() => complaintAlias.CreatedBy, () => authorAlias)
				.Left.JoinAlias(() => complaintAlias.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(() => complaintAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => complaintAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => complaintAlias.Guilties, () => complaintGuiltyItemAlias)
				.Left.JoinAlias(() => complaintAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintKind, () => complaintKindAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintDetalization, () => complaintDelatizationAlias)
				.Left.JoinAlias(() => complaintAlias.Fines, () => fineAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintDiscussions, () => discussionAlias)
				.Left.JoinAlias(() => discussionAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Employee, () => guiltyEmployeeAlias)
				.Left.JoinAlias(() => guiltyEmployeeAlias.Subdivision, () => superspecialAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Subdivision, () => guiltySubdivisionAlias)
				.Left.JoinAlias(() => complaintKindAlias.ComplaintObject, () => complaintObjectAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Responsible, () => responsibleAlias);

			#region Filter

			if(FilterViewModel != null)
			{
				if(FilterViewModel.IsForRetail != null)
				{
					query.Where(() => counterpartyAlias.IsForRetail == FilterViewModel.IsForRetail);
				}

				if(FilterViewModel.EndDate != null)
				{
					FilterViewModel.EndDate = FilterViewModel.EndDate.Value.LatestDayTime();
				}

				QueryOver<ComplaintDiscussion, ComplaintDiscussion> dicussionQuery = null;

				if(FilterViewModel.Subdivision != null)
				{
					dicussionQuery = QueryOver.Of(() => discussionAlias)
						.Select(Projections.Property<ComplaintDiscussion>(p => p.Id))
						.Where(() => discussionAlias.Subdivision.Id == FilterViewModel.Subdivision.Id)
						.And(() => discussionAlias.Complaint.Id == complaintAlias.Id);
				}

				switch(FilterViewModel.FilterDateType)
				{
					case DateFilterType.PlannedCompletionDate:
						if(dicussionQuery == null)
						{
							if(FilterViewModel.StartDate.HasValue)
							{
								query.Where(() => complaintAlias.PlannedCompletionDate >= FilterViewModel.StartDate);
							}
							if(FilterViewModel.EndDate.HasValue)
							{
								query.Where(() => complaintAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
							}
						}
						else
						{
							if(FilterViewModel.StartDate.HasValue)
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate >= FilterViewModel.StartDate);
							}
							if(FilterViewModel.EndDate.HasValue)
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
							}
						}
						break;
					case DateFilterType.ActualCompletionDate:
						if(FilterViewModel.StartDate.HasValue)
						{
							query.Where(() => complaintAlias.ActualCompletionDate >= FilterViewModel.StartDate);
						}
						if(FilterViewModel.EndDate.HasValue)
						{
							query.Where(() => complaintAlias.ActualCompletionDate <= FilterViewModel.EndDate);
						}
						break;
					case DateFilterType.CreationDate:
						if(FilterViewModel.StartDate.HasValue)
						{
							query.Where(() => complaintAlias.CreationDate >= FilterViewModel.StartDate);
						}
						if(FilterViewModel.EndDate.HasValue)
						{
							query.Where(() => complaintAlias.CreationDate <= FilterViewModel.EndDate);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if(dicussionQuery != null)
				{
					query.WithSubquery.WhereExists(dicussionQuery);
				}

				if(FilterViewModel.ComplaintType != null)
				{
					query = query.Where(() => complaintAlias.ComplaintType == FilterViewModel.ComplaintType);
				}

				if(FilterViewModel.ComplaintStatus != null)
				{
					query = query.Where(() => complaintAlias.Status == FilterViewModel.ComplaintStatus);
				}

				if(FilterViewModel.Employee != null)
				{
					query = query.Where(() => complaintAlias.CreatedBy.Id == FilterViewModel.Employee.Id);
				}

				if(FilterViewModel.Counterparty != null)
				{
					query = query.Where(() => complaintAlias.Counterparty.Id == FilterViewModel.Counterparty.Id);
				}

				if(FilterViewModel.CurrentUserSubdivision != null && FilterViewModel.ComplaintDiscussionStatus != null)
				{
					query = query.Where(() => discussionAlias.Subdivision.Id == FilterViewModel.CurrentUserSubdivision.Id)
						.And(() => discussionAlias.Status == FilterViewModel.ComplaintDiscussionStatus);
				}

				if(FilterViewModel.GuiltyItemVM?.Entity?.Responsible != null)
				{
					var subquery = QueryOver.Of<ComplaintGuiltyItem>()
											.Where(g => g.Responsible.Id == FilterViewModel.GuiltyItemVM.Entity.Responsible.Id);

					if(FilterViewModel.GuiltyItemVM.Entity.Responsible.IsEmployeeResponsible && FilterViewModel.GuiltyItemVM.Entity.Employee != null)
					{
						subquery.Where(g => g.Employee.Id == FilterViewModel.GuiltyItemVM.Entity.Employee.Id);
					}

					if(FilterViewModel.GuiltyItemVM.Entity.Responsible.IsSubdivisionResponsible && FilterViewModel.GuiltyItemVM.Entity.Subdivision != null)
					{
						subquery.Where(g => g.Subdivision.Id == FilterViewModel.GuiltyItemVM.Entity.Subdivision.Id);
					}

					query.WithSubquery.WhereProperty(x => x.Id).In(subquery.Select(x => x.Complaint));
				}

				if(FilterViewModel.ComplainDetalization != null)
				{
					query.Where(() => complaintAlias.ComplaintDetalization.Id == FilterViewModel.ComplainDetalization.Id);
				}

				if(FilterViewModel.ComplaintKind != null)
				{
					query.Where(() => complaintAlias.ComplaintKind.Id == FilterViewModel.ComplaintKind.Id);
				}

				if(FilterViewModel.ComplaintObject != null)
				{
					query.Where(() => complaintObjectAlias.Id == FilterViewModel.ComplaintObject.Id);
				}
			}

			#endregion Filter

			query.Where(
					GetSearchCriterion(
					() => complaintAlias.Id,
					() => complaintAlias.ComplaintText,
					() => counterpartyAlias.Name,
					() => deliveryPointAlias.CompiledAddress
				)
			);

			query.SelectList(list => list
				.SelectGroup(() => complaintAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(() => complaintAlias.CreationDate).WithAlias(() => resultAlias.Date)
				.Select(() => complaintAlias.ComplaintType).WithAlias(() => resultAlias.Type)
				.Select(() => complaintAlias.Status).WithAlias(() => resultAlias.Status)
				.Select(workInSubdivisionProjection).WithAlias(() => resultAlias.WorkInSubdivision)
				.Select(plannedCompletionDateProjection).WithAlias(() => resultAlias.PlannedCompletionDate)
				.Select(lastPlannedCompletionDateProjection).WithAlias(() => resultAlias.LastPlannedCompletionDate)
				.Select(counterpartyWithAddressProjection).WithAlias(() => resultAlias.ClientNameWithAddress)
				.Select(guiltiesProjection).WithAlias(() => resultAlias.Guilties)
				.Select(EmployeeProjections.GetDriverFullNameProjection()).WithAlias(() => resultAlias.Driver)
				.Select(authorProjection).WithAlias(() => resultAlias.Author)
				.Select(finesProjection).WithAlias(() => resultAlias.Fines)
				.Select(() => complaintAlias.ComplaintText).WithAlias(() => resultAlias.ComplaintText)
				.Select(() => complaintKindAlias.Name).WithAlias(() => resultAlias.ComplaintKindString)
				.Select(() => complaintKindAlias.IsArchive).WithAlias(() => resultAlias.ComplaintKindIsArchive)
				.Select(() => complaintDelatizationAlias.Name).WithAlias(() => resultAlias.ComplaintDetalizationString)
				.Select(() => complaintDelatizationAlias.IsArchive).WithAlias(() => resultAlias.ComplaintDetalizationIsArchive)
				.SelectSubQuery(resultOfResultCommentsSubquery).WithAlias(() => resultAlias.ResultText)
				.Select(() => complaintAlias.ActualCompletionDate).WithAlias(() => resultAlias.ActualCompletionDate)
				.Select(() => complaintObjectAlias.Name).WithAlias(() => resultAlias.ComplaintObjectString)
				.SelectSubQuery(resultOfArrangementCommentsSubquery).WithAlias(() => resultAlias.ArrangementText)
				.SelectSubQuery(resultOfCounterpartySubquery).WithAlias(() => resultAlias.ResultOfCounterparty)
				.SelectSubQuery(resultOfEmployeesSubquery).WithAlias(() => resultAlias.ResultOfEmployees)
			);

			query.TransformUsing(Transformers.AliasToBean<ComplaintJournalNode>())
				 .OrderBy(n => n.Id)
				 .Desc();

			return query;
		}

		private int GetItemsCount(IUnitOfWork uow)
		{
			Complaint complaintAlias = null;
			Counterparty counterpartyAlias = null;
			ComplaintDiscussion discussionAlias = null;
			ComplaintKind complaintKindAlias = null;
			ComplaintObject complaintObjectAlias = null;
			ComplaintDetalization complaintDelatizationAlias = null;

			var query = uow.Session.QueryOver(() => complaintAlias)
				.Left.JoinAlias(() => complaintAlias.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintKind, () => complaintKindAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintDetalization, () => complaintDelatizationAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintDiscussions, () => discussionAlias)
				.Left.JoinAlias(() => complaintKindAlias.ComplaintObject, () => complaintObjectAlias);

			#region Filter

			if(FilterViewModel != null)
			{
				if(FilterViewModel.IsForRetail != null)
				{
					query.Where(() => counterpartyAlias.IsForRetail == FilterViewModel.IsForRetail);
				}

				if(FilterViewModel.EndDate != null)
				{
					FilterViewModel.EndDate = FilterViewModel.EndDate.Value.LatestDayTime();
				}

				QueryOver<ComplaintDiscussion, ComplaintDiscussion> dicussionQuery = null;

				if(FilterViewModel.Subdivision != null)
				{
					dicussionQuery = QueryOver.Of(() => discussionAlias)
						.Select(Projections.Property<ComplaintDiscussion>(p => p.Id))
						.Where(() => discussionAlias.Subdivision.Id == FilterViewModel.Subdivision.Id)
						.And(() => discussionAlias.Complaint.Id == complaintAlias.Id);
				}

				switch(FilterViewModel.FilterDateType)
				{
					case DateFilterType.PlannedCompletionDate:
						if(dicussionQuery == null)
						{
							if(FilterViewModel.StartDate.HasValue)
							{
								query.Where(() => complaintAlias.PlannedCompletionDate >= FilterViewModel.StartDate);
							}
							if(FilterViewModel.EndDate.HasValue)
							{
								query.Where(() => complaintAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
							}
						}
						else
						{
							if(FilterViewModel.StartDate.HasValue)
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate >= FilterViewModel.StartDate);
							}
							if(FilterViewModel.EndDate.HasValue)
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
							}
						}
						break;
					case DateFilterType.ActualCompletionDate:
						if(FilterViewModel.StartDate.HasValue)
						{
							query.Where(() => complaintAlias.ActualCompletionDate >= FilterViewModel.StartDate);
						}
						if(FilterViewModel.EndDate.HasValue)
						{
							query.Where(() => complaintAlias.ActualCompletionDate <= FilterViewModel.EndDate);
						}
						break;
					case DateFilterType.CreationDate:
						if(FilterViewModel.StartDate.HasValue)
						{
							query.Where(() => complaintAlias.CreationDate >= FilterViewModel.StartDate);
						}
						if(FilterViewModel.EndDate.HasValue)
						{
							query.Where(() => complaintAlias.CreationDate <= FilterViewModel.EndDate);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if(dicussionQuery != null)
				{
					query.WithSubquery.WhereExists(dicussionQuery);
				}

				if(FilterViewModel.ComplaintType != null)
				{
					query = query.Where(() => complaintAlias.ComplaintType == FilterViewModel.ComplaintType);
				}

				if(FilterViewModel.ComplaintStatus != null)
				{
					query = query.Where(() => complaintAlias.Status == FilterViewModel.ComplaintStatus);
				}

				if(FilterViewModel.Employee != null)
				{
					query = query.Where(() => complaintAlias.CreatedBy.Id == FilterViewModel.Employee.Id);
				}

				if(FilterViewModel.Counterparty != null)
				{
					query = query.Where(() => complaintAlias.Counterparty.Id == FilterViewModel.Counterparty.Id);
				}

				if(FilterViewModel.CurrentUserSubdivision != null
					&& FilterViewModel.ComplaintDiscussionStatus != null)
				{
					query = query.Where(() => discussionAlias.Subdivision.Id == FilterViewModel.CurrentUserSubdivision.Id)
						.And(() => discussionAlias.Status == FilterViewModel.ComplaintDiscussionStatus);
				}

				if(FilterViewModel.GuiltyItemVM?.Entity?.Responsible != null)
				{
					var subquery = QueryOver.Of<ComplaintGuiltyItem>()
						.Where(g => g.Responsible.Id == FilterViewModel.GuiltyItemVM.Entity.Responsible.Id);


					if(FilterViewModel.GuiltyItemVM.Entity.Responsible.IsEmployeeResponsible && FilterViewModel.GuiltyItemVM.Entity.Employee != null)
					{
						subquery.Where(g => g.Employee.Id == FilterViewModel.GuiltyItemVM.Entity.Employee.Id);
					}

					if(FilterViewModel.GuiltyItemVM.Entity.Responsible.IsSubdivisionResponsible && FilterViewModel.GuiltyItemVM.Entity.Subdivision != null)
					{
						subquery.Where(g => g.Subdivision.Id == FilterViewModel.GuiltyItemVM.Entity.Subdivision.Id);
					}

					query.WithSubquery.WhereProperty(x => x.Id).In(subquery.Select(x => x.Complaint));
				}

				if(FilterViewModel.ComplaintKind != null)
				{
					query.Where(() => complaintAlias.ComplaintKind.Id == FilterViewModel.ComplaintKind.Id);
				}

				if(FilterViewModel.ComplaintObject != null)
				{
					query.Where(() => complaintObjectAlias.Id == FilterViewModel.ComplaintObject.Id);
				}

				if(FilterViewModel.ComplainDetalization != null)
				{
					query.Where(() => complaintAlias.ComplaintDetalization.Id == FilterViewModel.ComplainDetalization.Id);
				}
			}

			#endregion Filter

			query.Select(Projections.GroupProjection(() => complaintAlias.Id));

			return query.List<int>().Count;

		}

		private void RegisterComplaints()
		{
			var complaintConfig = RegisterEntity<Complaint>(GetComplaintQuery, GetItemsCount)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => NavigationManager.OpenViewModel<CreateComplaintViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel,
					//функция диалога открытия документа
					(ComplaintJournalNode node) =>
						NavigationManager.OpenViewModel<ComplaintViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					//функция идентификации документа
					(ComplaintJournalNode node) =>
					{
						return node.EntityType == typeof(Complaint);
					},
					"Клиентская рекламация",
					new JournalParametersForDocument() { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => NavigationManager.OpenViewModel<CreateInnerComplaintViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel,
					//функция диалога открытия документа
					(ComplaintJournalNode node) =>
						NavigationManager.OpenViewModel<ComplaintViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(node.Id)).ViewModel,
					//функция идентификации документа
					(ComplaintJournalNode node) =>
					{
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
			foreach(var item in items.Cast<ComplaintJournalNode>().Skip((int)start))
			{
				item.SequenceNumber = items.IndexOf(item) + 1;
			}
		}

		protected override void CreatePopupActions()
		{
			Complaint GetComplaint(object[] objs)
			{
				var selectedNodes = objs.Cast<ComplaintJournalNode>().ToList();
				if(selectedNodes.Count != 1)
				{
					return null;
				}

				var complaint = UoW.GetById<Complaint>(selectedNodes.First().Id);
				return complaint;
			}

			Order GetOrder(object[] objs)
			{
				return GetComplaint(objs)?.Order;
			}

			RouteList GetRouteList(object[] objs)
			{
				var order = GetOrder(objs);
				if(order == null)
				{
					return null;
				}

				var rl = _routeListItemRepository.GetRouteListItemForOrder(UoW, order)?.RouteList;
				return rl;
			}

			bool HasOrder(object[] objs) => GetOrder(objs) != null;

			bool HasRouteList(object[] objs) => GetRouteList(objs) != null;

			PopupActionsList.Add(
				new JournalAction(
					"Открыть заказ",
					HasOrder,
					n => true,
					n => _gtkDlgOpener.OpenOrderDlg(ParentTab, GetOrder(n).Id)
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Открыть маршрутный лист",
					HasRouteList,
					n => true,
					n => _gtkDlgOpener.OpenRouteListCreateDlg(ParentTab, GetRouteList(n).Id)
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Открыть диалог закрытия МЛ",
					n => GetRouteList(n)?.CanBeOpenedInClosingDlg ?? false,
					n => true,
					n => _gtkDlgOpener.OpenRouteListClosingDlg(ParentTab, GetRouteList(n).Id)
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Создать штраф",
					n => EntityConfigs[typeof(Complaint)].PermissionResult.CanUpdate,
					n => EntityConfigs[typeof(Complaint)].PermissionResult.CanUpdate,
					n =>
					{
						var currentComplaintId = n.OfType<ComplaintJournalNode>().FirstOrDefault()?.Id;
						ComplaintViewModel currentComplaintVM = null;
						if(currentComplaintId.HasValue)
						{
							currentComplaintVM = _scope.Resolve<ComplaintViewModel>(new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(currentComplaintId.Value)));
							currentComplaintVM.AddFineCommand.Execute(ParentTab);
						}
					}
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Закрыть рекламацию",
					n => n.OfType<ComplaintJournalNode>().FirstOrDefault()?.Status != ComplaintStatuses.Closed && _canCloseComplaint,
					n => EntityConfigs[typeof(Complaint)].PermissionResult.CanUpdate && _canCloseComplaint,
					n =>
					{
						var currentComplaintId = n.OfType<ComplaintJournalNode>().FirstOrDefault()?.Id;
						ComplaintViewModel currentComplaintVM = null;
						if(currentComplaintId.HasValue)
						{
							currentComplaintVM = _scope.Resolve<ComplaintViewModel>(new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(currentComplaintId.Value)));

							var interserctedSubdivisionsToInformIds = _generalSettingsParametersProvider.SubdivisionsToInformComplaintHasNoDriver
									.Intersect(currentComplaintVM.Entity.Guilties.Select(cgi => cgi.Subdivision.Id));

							var intersectedSubdivisionsNames = currentComplaintVM.Entity.Guilties
								.Select(g => g.Subdivision)
								.Where(s => interserctedSubdivisionsToInformIds.Contains(s.Id))
								.Select(s => s.Name);

							if(currentComplaintVM.Entity.ComplaintResultOfEmployees?.Id == _complaintParametersProvider.ComplaintResultOfEmployeesIsGuiltyId
								&& interserctedSubdivisionsToInformIds.Any()
								&& currentComplaintVM.Entity.Driver is null
								&& !AskQuestion($"Вы хотите закрыть рекламацию на отдел {string.Join(", ", intersectedSubdivisionsNames)} без указания водителя?",
								"Вы уверены?"))
							{
								return;
							}

							currentComplaintVM.AddFineCommand.Execute(ParentTab);

							string msg = string.Empty;
							if(!currentComplaintVM.Entity.Close(ref msg))
							{
								ShowWarningMessage(msg, "Не удалось закрыть");
							}
							else
							{
								currentComplaintVM.Save();
							}
						}
					}
				)
			);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateAddActions();
			CreateEditAction();
			CreateDefaultDeleteAction();
			CreateExportAction();
			OpenWithDepartmentsReacrionViewAction();
			CreateComplaintClassificationSummaryAction();
		}

		public Action<Type> ChangeView { get; set; }

		private void OpenWithDepartmentsReacrionViewAction()
		{
			var openStandartView = new JournalAction("Отобразить время реакции отделов",
				(selected) => true,
				(selected) => true,
				(selected) =>
				{
					ChangeView?.Invoke(typeof(ComplaintsWithDepartmentsReactionJournalViewModel));
				}
			);
			NodeActionsList.Add(openStandartView);
		}

		private void CreateExportAction()
		{
			NodeActionsList.Add(new JournalAction("Экспорт в Excel", x => true, x => true,
				selectedItems =>
				{
					var nodes = GetComplaintQuery(UoW).List<ComplaintJournalNode>();

					var report = new ComplaintJournalReport(nodes, _fileDialogService);
					report.Export();
				}));
		}

		private void CreateComplaintClassificationSummaryAction()
		{
			NodeActionsList.Add(new JournalAction("Сводка по классификации рекламаций", x => true, x => true,
				selectedItems =>
				{
					var nodes = GetComplaintQuery(UoW).List<ComplaintJournalNode>();
					var report = new ComplaintClassificationSummaryReport(nodes, FilterViewModel, _fileDialogService);
					report.Export();
				}));
		}

		private void CreateAddActions()
		{
			if(!EntityConfigs.Any())
			{
				return;
			}

			var totalCreateDialogConfigs = EntityConfigs
				.Where(x => x.Value.PermissionResult.CanCreate)
				.Sum(x => x.Value.EntityDocumentConfigurations
							.Select(y => y.GetCreateEntityDlgConfigs().Count())
							.Sum());

			if(EntityConfigs.Values.Count(x => x.PermissionResult.CanRead) > 1 || totalCreateDialogConfigs > 1)
			{
				var addParentNodeAction = new JournalAction("Добавить", (selected) => true, (selected) => true, (selected) => { });
				foreach(var entityConfig in EntityConfigs.Values)
				{
					foreach(var documentConfig in entityConfig.EntityDocumentConfigurations)
					{
						foreach(var createDlgConfig in documentConfig.GetCreateEntityDlgConfigs())
						{
							var childNodeAction = new JournalAction(createDlgConfig.Title,
								(selected) => entityConfig.PermissionResult.CanCreate,
								(selected) => entityConfig.PermissionResult.CanCreate,
								(selected) =>
								{
									createDlgConfig.OpenEntityDialogFunction.Invoke();
								}
							);
							addParentNodeAction.ChildActionsList.Add(childNodeAction);
						}
					}
				}
				NodeActionsList.Add(addParentNodeAction);
			}
			else
			{
				var entityConfig = EntityConfigs.First().Value;
				var addAction = new JournalAction("Добавить",
					(selected) => entityConfig.PermissionResult.CanCreate,
					(selected) => entityConfig.PermissionResult.CanCreate,
					(selected) =>
					{
						var docConfig = entityConfig.EntityDocumentConfigurations.First();
						ITdiTab tab = docConfig.GetCreateEntityDlgConfigs().First().OpenEntityDialogFunction.Invoke();

						if(tab is ITdiDialog)
						{
							((ITdiDialog)tab).EntitySaved += Tab_EntitySaved;
						}
					},
					"Insert"
					);
				NodeActionsList.Add(addAction);
			};
		}

		protected void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<ComplaintJournalNode>().ToList();
					if(selectedNodes.Count != 1)
					{
						return false;
					}
					ComplaintJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<ComplaintJournalNode>().ToList();
					if(selectedNodes.Count != 1)
					{
						return;
					}
					ComplaintJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];

					if(selectedNode.EntityType == typeof(Complaint))
					{
						NavigationManager.OpenViewModel<ComplaintViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(selectedNode.Id));
						return;
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}
	}
}
