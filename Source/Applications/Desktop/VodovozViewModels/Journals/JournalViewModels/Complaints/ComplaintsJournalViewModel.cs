using Autofac;
using ClosedXML.Report;
using DateTimeHelpers;
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
using QS.Tdi;
using System;
using System.Collections;
using System.Linq;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.NHibernateProjections.Employees;
using Vodovoz.Services;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Complaints;
using Vodovoz.Settings.Organizations;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Dialogs;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports.ComplaintsJournalReport;
using static Vodovoz.FilterViewModels.ComplaintFilterViewModel;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Journals.JournalViewModels
{
	public class ComplaintsJournalViewModel :
		FilterableMultipleEntityJournalViewModelBase<ComplaintJournalNode, ComplaintFilterViewModel>,
		IComplaintsInfoProvider,
		IChangeComplaintJournal
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly ISubdivisionSettings _subdivisionSettings;
		private readonly IGtkTabsOpener _gtkDlgOpener;
		private readonly IComplaintSettings _complaintSettings;
		private readonly IGeneralSettings _generalSettingsSettings;
		private bool _canCloseComplaint;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.ComplaintPanelView };

		public ComplaintFilterViewModel ComplaintsFilterViewModel => FilterViewModel;

		public ComplaintsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IEmployeeService employeeService,
			IRouteListItemRepository routeListItemRepository,
			ISubdivisionSettings subdivisionSettings,
			ComplaintFilterViewModel filterViewModel,
			IFileDialogService fileDialogService,
			IGtkTabsOpener gtkDialogsOpener,
			IUserRepository userRepository,
			IComplaintSettings complaintSettings,
			IGeneralSettings generalSettingsSettings,
			Action<ComplaintFilterViewModel> filterConfig = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices, navigationManager)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_subdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			_gtkDlgOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));
			_complaintSettings = complaintSettings ?? throw new ArgumentNullException(nameof(complaintSettings));
			_generalSettingsSettings = generalSettingsSettings ?? throw new ArgumentNullException(nameof(generalSettingsSettings));

			TabName = "Журнал рекламаций";

			Configure(commonServices, employeeService, userRepository, filterConfig);

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
		}
		
		private void Configure(
			ICommonServices commonServices,
			IEmployeeService employeeService,
			IUserRepository userRepository,
			Action<ComplaintFilterViewModel> filterConfig)
		{
			_canCloseComplaint = commonServices.CurrentPermissionService.ValidatePresetPermission("can_close_complaints");

			SubdivisionQualityServiceShortName =
				UoW.GetById<Subdivision>(_subdivisionSettings.QualityServiceSubdivisionId).ShortName ?? "?";
			SubdivisionAuditDepartmentShortName =
				UoW.GetById<Subdivision>(_subdivisionSettings.AuditDepartmentSubdivisionId)?.ShortName ?? "?";
			
			RegisterComplaints();

			var threadLoader = DataLoader as ThreadDataLoader<ComplaintJournalNode>;
			threadLoader.MergeInOrderBy(x => x.Id, true);

			FinishJournalConfiguration();

			ConfigureFilter(commonServices, employeeService, userRepository);

			DataLoader.ItemsListUpdated += (sender, e) =>
				CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(null));

			DataLoader.PostLoadProcessingFunc = BeforeItemsUpdated;
			UseSlider = false;
			
			if(filterConfig != null)
			{
				FilterViewModel.SetAndRefilterAtOnce(filterConfig);
			}
		}

		private void ConfigureFilter(ICommonServices commonServices, IEmployeeService employeeService, IUserRepository userRepository)
		{
			FilterViewModel.EmployeeService = employeeService;

			var currentUserSettings = userRepository.GetUserSettings(UoW, commonServices.UserService.CurrentUserId);
			var defaultSubdivision = currentUserSettings.DefaultSubdivisionId.HasValue ? UoW.GetById<Subdivision>(currentUserSettings.DefaultSubdivisionId.Value) : null;
			var currentEmployeeSubdivision = employeeService.GetEmployeeForUser(UoW, commonServices.UserService.CurrentUserId).Subdivision;

			if(FilterViewModel.CurrentUserSubdivision == null)
			{
				FilterViewModel.CurrentUserSubdivision = currentEmployeeSubdivision;
			}

			if(FilterViewModel.Subdivision == null)
			{
				FilterViewModel.Subdivision = currentUserSettings.UseEmployeeSubdivision ? currentEmployeeSubdivision : defaultSubdivision;
			}

			if(FilterViewModel.ComplaintStatus == null)
			{
				FilterViewModel.ComplaintStatus = currentUserSettings.DefaultComplaintStatus;
			}
		}

		private string SubdivisionQualityServiceShortName { get; set; } // СК
		private string SubdivisionAuditDepartmentShortName { get; set; } // КРО

		#region Основной запрос

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
			Employee resultCommentAuthorAlias = null;

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
					$"WHEN '{_complaintSettings.EmployeeResponsibleId}' THEN CONCAT('(',?5,')', ?2)" +
					$"WHEN '{_complaintSettings.SubdivisionResponsibleId}' THEN ?3 " +
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

			var authorsOfResultCommentsProjection = Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						EmployeeProjections.GetEmployeeFullNameProjection(),
						Projections.Constant(", ")
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

			var authorsOfResultCommentsSubquery = QueryOver.Of(() => resultCommentAuthorAlias)
				.JoinEntityAlias(() => resultOfComplaintResultCommentAlias,
					() => resultOfComplaintResultCommentAlias.Author.Id == resultCommentAuthorAlias.Id)
				.Where(() => resultOfComplaintResultCommentAlias.Complaint.Id == complaintAlias.Id)
				.Select(authorsOfResultCommentsProjection);

			var isNeedWorkSubquery = QueryOver.Of(() => discussionAlias)
				.Where(() => discussionAlias.Status == ComplaintDiscussionStatuses.InProcess)
				.Where(() => discussionAlias.Complaint.Id == complaintAlias.Id)
				.Select(Projections.Id())
				.Take(1);

			var isNeedWorkProjection = Projections.Conditional(
				Restrictions.Gt(Projections.SubQuery(isNeedWorkSubquery),0),
				Projections.Constant(true),
				Projections.Constant(false));

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
				
				var endDate = FilterViewModel.EndDate;
				endDate = endDate?.LatestDayTime();

				QueryOver<ComplaintDiscussion, ComplaintDiscussion> dicussionQuery = null;

				if(FilterViewModel.Subdivision != null)
				{
					dicussionQuery = QueryOver.Of(() => discussionAlias)
						.Select(Projections.Property<ComplaintDiscussion>(p => p.Id))
						.Where(() => discussionAlias.Subdivision.Id == FilterViewModel.Subdivision.Id)
						.And(() => discussionAlias.Complaint.Id == complaintAlias.Id);
				}

				var startDate = FilterViewModel.StartDate;

				switch(FilterViewModel.FilterDateType)
				{
					case DateFilterType.PlannedCompletionDate:
						if(dicussionQuery == null)
						{
							if(startDate.HasValue)
							{
								query.Where(() => complaintAlias.PlannedCompletionDate >= startDate);
							}
							if(endDate.HasValue)
							{
								query.Where(() => complaintAlias.PlannedCompletionDate <= endDate);
							}
						}
						else
						{
							if(startDate.HasValue)
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate >= startDate);
							}
							if(endDate.HasValue)
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate <= endDate);
							}
						}
						break;
					case DateFilterType.ActualCompletionDate:
						if(startDate.HasValue)
						{
							query.Where(() => complaintAlias.ActualCompletionDate >= startDate);
						}
						if(endDate.HasValue)
						{
							query.Where(() => complaintAlias.ActualCompletionDate <= endDate);
						}
						break;
					case DateFilterType.CreationDate:
						if(startDate.HasValue)
						{
							query.Where(() => complaintAlias.CreationDate >= startDate);
						}
						if(endDate.HasValue)
						{
							query.Where(() => complaintAlias.CreationDate <= endDate);
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
				.SelectSubQuery(authorsOfResultCommentsSubquery).WithAlias(() => resultAlias.ResultCommentsAuthors)
				.Select(() => complaintAlias.ActualCompletionDate).WithAlias(() => resultAlias.ActualCompletionDate)
				.Select(() => complaintObjectAlias.Name).WithAlias(() => resultAlias.ComplaintObjectString)
				.SelectSubQuery(resultOfArrangementCommentsSubquery).WithAlias(() => resultAlias.ArrangementText)
				.SelectSubQuery(resultOfCounterpartySubquery).WithAlias(() => resultAlias.ResultOfCounterparty)
				.SelectSubQuery(resultOfEmployeesSubquery).WithAlias(() => resultAlias.ResultOfEmployees)				
				.Select(isNeedWorkProjection).WithAlias(() => resultAlias.IsNeedWork)
			);

			query.TransformUsing(Transformers.AliasToBean<ComplaintJournalNode>())
				 .OrderBy(n => n.Id)
				 .Desc();

			return query;
		}

		#endregion

		#region запрос подсчета количества

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

				var endDate = FilterViewModel.EndDate;
				endDate = endDate?.LatestDayTime();

				QueryOver<ComplaintDiscussion, ComplaintDiscussion> dicussionQuery = null;

				if(FilterViewModel.Subdivision != null)
				{
					dicussionQuery = QueryOver.Of(() => discussionAlias)
						.Select(Projections.Property<ComplaintDiscussion>(p => p.Id))
						.Where(() => discussionAlias.Subdivision.Id == FilterViewModel.Subdivision.Id)
						.And(() => discussionAlias.Complaint.Id == complaintAlias.Id);
				}
				
				var startDate = FilterViewModel.StartDate;

				switch(FilterViewModel.FilterDateType)
				{
					case DateFilterType.PlannedCompletionDate:
						if(dicussionQuery == null)
						{
							if(startDate.HasValue)
							{
								query.Where(() => complaintAlias.PlannedCompletionDate >= startDate);
							}
							if(endDate.HasValue)
							{
								query.Where(() => complaintAlias.PlannedCompletionDate <= endDate);
							}
						}
						else
						{
							if(startDate.HasValue)
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate >= startDate);
							}
							if(endDate.HasValue)
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate <= endDate);
							}
						}
						break;
					case DateFilterType.ActualCompletionDate:
						if(startDate.HasValue)
						{
							query.Where(() => complaintAlias.ActualCompletionDate >= startDate);
						}
						if(endDate.HasValue)
						{
							query.Where(() => complaintAlias.ActualCompletionDate <= endDate);
						}
						break;
					case DateFilterType.CreationDate:
						if(startDate.HasValue)
						{
							query.Where(() => complaintAlias.CreationDate >= startDate);
						}
						if(endDate.HasValue)
						{
							query.Where(() => complaintAlias.CreationDate <= endDate);
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

		#endregion

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
					n => _gtkDlgOpener.OpenOrderDlgFromViewModelByNavigator(this, GetOrder(n).Id)
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Открыть маршрутный лист",
					HasRouteList,
					n => true,
					n => NavigationManager.OpenViewModel<RouteListCreateViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(GetRouteList(n).Id))
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Открыть диалог закрытия МЛ",
					n => GetRouteList(n)?.CanBeOpenedInClosingDlg ?? false,
					n => true,
					n => _gtkDlgOpener.OpenRouteListClosingDlgFromViewModelByNavigator(this, GetRouteList(n).Id)
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Создать штраф",
					n => EntityConfigs[typeof(Complaint)].PermissionResult.CanUpdate,
					n => EntityConfigs[typeof(Complaint)].PermissionResult.CanUpdate,
					n =>
					{
						var selectedNode = n.OfType<ComplaintJournalNode>().FirstOrDefault();

						if(selectedNode == null)
						{
							return;
						}

						var currentComplaint = UoW.GetById<Complaint>(selectedNode.Id);

						CreateFineAndAddToComplaint(currentComplaint.Id, currentComplaint.GetFineReason());						
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
						var selectedNode = n.OfType<ComplaintJournalNode>().FirstOrDefault();

						if(selectedNode == null)
						{
							return;
						}

						using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot("Закрытие рекламации"))
						{
							var currentComplaint = uowLocal.GetById<Complaint>(selectedNode.Id);

							var interserctedSubdivisionsToInformIds =
								_generalSettingsSettings.SubdivisionsToInformComplaintHasNoDriver
									.Intersect(currentComplaint.Guilties.Select(cgi => cgi.Subdivision.Id));

							var intersectedSubdivisionsNames = currentComplaint.Guilties
								.Select(g => g.Subdivision)
								.Where(s => interserctedSubdivisionsToInformIds.Contains(s.Id))
								.Select(s => s.Name);

							if(currentComplaint.ComplaintResultOfEmployees?.Id == _complaintSettings.ComplaintResultOfEmployeesIsGuiltyId
								&& interserctedSubdivisionsToInformIds.Any()
								&& currentComplaint.Driver is null
								&& !_commonServices.InteractiveService.Question(
									$"Вы хотите закрыть рекламацию на отдел {string.Join(", ", intersectedSubdivisionsNames)} без указания водителя?", "Вы уверены?"))
							{
								return;
							}

							var msg = string.Empty;
							if(!currentComplaint.Close(ref msg))
							{
								ShowWarningMessage(msg, "Не удалось закрыть");
							}
							else
							{
								CreateFineAndAddToComplaint(currentComplaint.Id, currentComplaint.GetFineReason());

								uowLocal.Save(currentComplaint);
								uowLocal.Commit();								
							}
						}
					}
				)
			);
		}

		private void CreateFineAndAddToComplaint(int complaintId, string fineReason)
		{
			var finePage = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());

			finePage.ViewModel.FineReasonString = fineReason;

			finePage.ViewModel.EntitySaved += (s, e) =>
			{
				using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot("Прикрепление штрафа к рекламации"))
				{
					var fine = e.Entity as Fine;

					var complaint = uowLocal.GetById<Complaint>(complaintId);

					complaint.AddFine(fine);

					uowLocal.Save(complaint);
					uowLocal.Commit();
				}			
			};
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateAddActions();
			CreateEditAction();
			CreateDefaultDeleteAction();
			CreateExportAction();
			OpenWithDepartmentsReactionViewAction();
			CreateComplaintClassificationSummaryAction();
		}

		public Action<Type> ChangeView { get; set; }

		private void OpenWithDepartmentsReactionViewAction()
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
			NodeActionsList.Add(new JournalAction(
				"Сводка по классификации рекламаций",
				x => true,
				x => true,
				selectedItems =>
				{
					try
					{
						if(!FilterViewModel.EndDate.HasValue)
						{
							_interactiveService.ShowMessage(ImportanceLevel.Error, "Не выбран интервал");
							return;
						}

						var nodes = GetComplaintQuery(UoW).List<ComplaintJournalNode>();
						var report = ComplaintClassificationSummaryReport.Generate(nodes, FilterViewModel);
						ExportComplaintClassificationSummaryReport(report);
					}
					catch(Exception e)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Error, $"Не удалось создать отчет: {e.Message}");
					}
				}));
		}

		public void ExportComplaintClassificationSummaryReport(ComplaintClassificationSummaryReport report)
		{
			var dialogSettingsFactory = new DialogSettingsFactory();
			var dialogSettings = dialogSettingsFactory.CreateForExcelExport();

			dialogSettings.FileName = report.FileName;

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(result.Successful)
			{
				SaveReport(ComplaintClassificationSummaryReport.TemplatePath, result.Path, report);
			}
		}

		private void SaveReport(string templatePath, string path, ComplaintClassificationSummaryReport report)
		{
			var template = new XLTemplate(templatePath);
			template.AddVariable(report);
			template.Generate();
			template.SaveAs(path);
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
						NavigationManager.OpenViewModel<ComplaintViewModel, IEntityUoWBuilder>(
							this, EntityUoWBuilder.ForOpen(selectedNode.Id));
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		public override void Dispose()
		{
			FilterViewModel = null;
			base.Dispose();
		}
	}
}
