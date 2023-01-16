﻿using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using System.Collections;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Reports.ComplaintsJournalReport;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Journals.JournalViewModels
{
	public class ComplaintsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<ComplaintJournalNode, ComplaintFilterViewModel>, IComplaintsInfoProvider
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly IUndeliveredOrdersJournalOpener _undeliveredOrdersJournalOpener;
		private readonly IEmployeeService _employeeService;
		private readonly ICounterpartyJournalFactory _counterpartySelectorFactory;
		private readonly IFileDialogService _fileDialogService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
		private readonly IGtkTabsOpener _gtkDlgOpener;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUserRepository _userRepository;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly IComplaintParametersProvider _complaintParametersProvider;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		private bool canCloseComplaint = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_complaints");
		private readonly ISalesPlanJournalFactory _salesPlanJournalFactory;
		private readonly INomenclatureJournalFactory _nomenclatureSelector;
		private readonly IEmployeeSettings _employeeSettings;

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.ComplaintPanelView };

		public ComplaintFilterViewModel ComplaintsFilterViewModel => FilterViewModel;

		public ComplaintsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			IEmployeeService employeeService,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			IRouteListItemRepository routeListItemRepository,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			ComplaintFilterViewModel filterViewModel,
			IFileDialogService fileDialogService,
			ISubdivisionRepository subdivisionRepository,
			IGtkTabsOpener gtkDialogsOpener,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			ISalesPlanJournalFactory salesPlanJournalFactory,
			INomenclatureJournalFactory nomenclatureSelector,
			IEmployeeSettings employeeSettings,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IComplaintParametersProvider complaintParametersProvider) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			this._unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this._commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_undeliveredOrdersJournalOpener = undeliveredOrdersJournalOpener ?? throw new ArgumentNullException(nameof(undeliveredOrdersJournalOpener));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			_gtkDlgOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_deliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_salesPlanJournalFactory = salesPlanJournalFactory ?? throw new ArgumentNullException(nameof(salesPlanJournalFactory));
			_nomenclatureSelector = nomenclatureSelector ?? throw new ArgumentNullException(nameof(nomenclatureSelector));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_undeliveredOrdersRepository =
				undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_complaintParametersProvider = complaintParametersProvider ?? throw new ArgumentNullException(nameof(complaintParametersProvider));
			TabName = "Журнал рекламаций";

			RegisterComplaints();

			var threadLoader = DataLoader as ThreadDataLoader<ComplaintJournalNode>;
			threadLoader.MergeInOrderBy(x => x.Id, true);

			FinishJournalConfiguration();

			FilterViewModel.EmployeeService = employeeService;

			var currentUserSettings = userRepository.GetUserSettings(UoW, commonServices.UserService.CurrentUserId);
			var defaultSubdivision = currentUserSettings.DefaultSubdivision;
			var currentEmployeeSubdivision = employeeService.GetEmployeeForUser(UoW, commonServices.UserService.CurrentUserId).Subdivision;

			FilterViewModel.CurrentUserSubdivision = currentEmployeeSubdivision;

			if (currentUserSettings.UseEmployeeSubdivision)
			{
				FilterViewModel.Subdivision = currentEmployeeSubdivision;
			}
			else
			{
				FilterViewModel.Subdivision = defaultSubdivision;
			}

			FilterViewModel.ComplaintStatus = currentUserSettings.DefaultComplaintStatus;

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
				typeof(ComplaintObject)
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
			ComplaintObject complaintObjectAlias = null;
			ComplaintResultOfCounterparty resultOfCounterpartyAlias = null;
			ComplaintResultOfEmployees resultOfEmployeesAlias = null;
			Responsible responsibleAlias = null;

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

			string subdivisionQualityServiceId = uow.GetById<Subdivision>(_subdivisionParametersProvider.QualityServiceSubdivisionId).ShortName ?? "?"; // СК
			string subdivisionAuditDepartmentId = uow.GetById<Subdivision>(_subdivisionParametersProvider.AuditDepartmentSubdivisionId).ShortName ?? "?"; // КРО

			var workInSubdivisionsCheckingProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(',', ?1, IF(?2 = 'Checking',?3, ''))"),
				NHibernateUtil.String,
				subdivisionsSubqueryProjection,
				Projections.Property(() => complaintAlias.Status),
				Projections.Constant(subdivisionQualityServiceId)
			);

			var workInSubdivisionsWaitingForReactionProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(',', ?1, IF(?2 = 'WaitingForReaction',?3, ''))"),
				NHibernateUtil.String,
				subdivisionsSubqueryProjection,
				Projections.Property(() => complaintAlias.Status),
				Projections.Constant(subdivisionAuditDepartmentId)
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

			var resultOfCounterpartySubquery = QueryOver.Of(() => resultOfCounterpartyAlias)
				.Where(() => resultOfCounterpartyAlias.Id == complaintAlias.ComplaintResultOfCounterparty.Id)
				.Select(Projections.Property(() => resultOfCounterpartyAlias.Name));

			var resultOfEmployeesSubquery = QueryOver.Of(() => resultOfEmployeesAlias)
				.Where(() => resultOfEmployeesAlias.Id == complaintAlias.ComplaintResultOfEmployees.Id)
				.Select(Projections.Property(() => resultOfEmployeesAlias.Name));

			var query = uow.Session.QueryOver(() => complaintAlias)
				.Left.JoinAlias(() => complaintAlias.CreatedBy, () => authorAlias)
				.Left.JoinAlias(() => complaintAlias.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(() => complaintAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => complaintAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => complaintAlias.Guilties, () => complaintGuiltyItemAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintKind, () => complaintKindAlias)
				.Left.JoinAlias(() => complaintAlias.Fines, () => fineAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintDiscussions, () => discussionAlias)
				.Left.JoinAlias(() => discussionAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Employee, () => guiltyEmployeeAlias)
				.Left.JoinAlias(() => guiltyEmployeeAlias.Subdivision, () => superspecialAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Subdivision, () => guiltySubdivisionAlias)
				.Left.JoinAlias(() => complaintKindAlias.ComplaintObject, () => complaintObjectAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Responsible, () => responsibleAlias);

			#region Filter

			if(FilterViewModel != null) {
				if (FilterViewModel.IsForRetail != null)
				{
					query.Where(() => counterpartyAlias.IsForRetail == FilterViewModel.IsForRetail);
				}

				FilterViewModel.EndDate = FilterViewModel.EndDate.Date.AddHours(23).AddMinutes(59);

				QueryOver<ComplaintDiscussion, ComplaintDiscussion> dicussionQuery = null;

				if(FilterViewModel.Subdivision != null) {
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
							if(!FilterViewModel.StartDate.HasValue)
							{
								query.Where(() => complaintAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
							}
							else
							{
								query.Where(() => complaintAlias.PlannedCompletionDate <= FilterViewModel.EndDate)
									.And(() => complaintAlias.PlannedCompletionDate >= FilterViewModel.StartDate.Value);
							}
						}
						else
						{
							if(!FilterViewModel.StartDate.HasValue)
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
							}
							else
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate >= FilterViewModel.StartDate.Value)
									.And(() => discussionAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
							}
						}
						break;
					case DateFilterType.ActualCompletionDate:
						if(!FilterViewModel.StartDate.HasValue)
						{
							query.Where(() => complaintAlias.ActualCompletionDate <= FilterViewModel.EndDate);
						}
						else
						{
							query.Where(() => complaintAlias.ActualCompletionDate <= FilterViewModel.EndDate)
								.And(() => complaintAlias.ActualCompletionDate >= FilterViewModel.StartDate.Value);
						}
						break;
					case DateFilterType.CreationDate:
						if(!FilterViewModel.StartDate.HasValue)
						{
							query.Where(() => complaintAlias.CreationDate <= FilterViewModel.EndDate);
						}
						else
						{
							query.Where(() => complaintAlias.CreationDate <= FilterViewModel.EndDate)
								.And(() => complaintAlias.CreationDate >= FilterViewModel.StartDate);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if(dicussionQuery != null)
					query.WithSubquery.WhereExists(dicussionQuery);
				if(FilterViewModel.ComplaintType != null)
					query = query.Where(() => complaintAlias.ComplaintType == FilterViewModel.ComplaintType);
				if(FilterViewModel.ComplaintStatus != null)
					query = query.Where(() => complaintAlias.Status == FilterViewModel.ComplaintStatus);
				if(FilterViewModel.Employee != null)
					query = query.Where(() => complaintAlias.CreatedBy.Id == FilterViewModel.Employee.Id);
				if(FilterViewModel.Counterparty != null)
				{
					query = query.Where(() => complaintAlias.Counterparty.Id == FilterViewModel.Counterparty.Id);
				}

				if(FilterViewModel.CurrentUserSubdivision != null && FilterViewModel.ComplaintDiscussionStatus != null)
				{
					query = query.Where(() => discussionAlias.Subdivision.Id == FilterViewModel.CurrentUserSubdivision.Id)
						.And(() => discussionAlias.Status == FilterViewModel.ComplaintDiscussionStatus);
				}

				if (FilterViewModel.GuiltyItemVM?.Entity?.Responsible != null) {
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
					query.Where(() => complaintAlias.ComplaintKind.Id == FilterViewModel.ComplaintKind.Id);

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
				.Select(workInSubdivisionProjection).WithAlias(() => resultAlias.WorkInSubdivision)
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
				.Select(() => complaintObjectAlias.Name).WithAlias(() => resultAlias.ComplaintObjectString)
				.Select(() => complaintAlias.Arrangement).WithAlias(() => resultAlias.ArrangementText)
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

			var query = uow.Session.QueryOver(() => complaintAlias)
				.Left.JoinAlias(() => complaintAlias.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintKind, () => complaintKindAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintDiscussions, () => discussionAlias)
				.Left.JoinAlias(() => complaintKindAlias.ComplaintObject, () => complaintObjectAlias);

			#region Filter

			if(FilterViewModel != null)
			{
				if(FilterViewModel.IsForRetail != null)
				{
					query.Where(() => counterpartyAlias.IsForRetail == FilterViewModel.IsForRetail);
				}

				FilterViewModel.EndDate = FilterViewModel.EndDate.Date.AddHours(23).AddMinutes(59);

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
							if(!FilterViewModel.StartDate.HasValue)
							{
								query.Where(() => complaintAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
							}
							else
							{
								query.Where(() => complaintAlias.PlannedCompletionDate <= FilterViewModel.EndDate)
									.And(() => complaintAlias.PlannedCompletionDate >= FilterViewModel.StartDate.Value);
							}
						}
						else
						{
							if(!FilterViewModel.StartDate.HasValue)
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
							}
							else
							{
								dicussionQuery.And(() => discussionAlias.PlannedCompletionDate >= FilterViewModel.StartDate.Value)
									.And(() => discussionAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
							}
						}
						break;
					case DateFilterType.ActualCompletionDate:
						if(!FilterViewModel.StartDate.HasValue)
						{
							query.Where(() => complaintAlias.ActualCompletionDate <= FilterViewModel.EndDate);
						}
						else
						{
							query.Where(() => complaintAlias.ActualCompletionDate <= FilterViewModel.EndDate)
								.And(() => complaintAlias.ActualCompletionDate >= FilterViewModel.StartDate.Value);
						}
						break;
					case DateFilterType.CreationDate:
						if(!FilterViewModel.StartDate.HasValue)
						{
							query.Where(() => complaintAlias.CreationDate <= FilterViewModel.EndDate);
						}
						else
						{
							query.Where(() => complaintAlias.CreationDate <= FilterViewModel.EndDate)
								.And(() => complaintAlias.CreationDate >= FilterViewModel.StartDate);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if(dicussionQuery != null)
					query.WithSubquery.WhereExists(dicussionQuery);
				if(FilterViewModel.ComplaintType != null)
					query = query.Where(() => complaintAlias.ComplaintType == FilterViewModel.ComplaintType);
				if(FilterViewModel.ComplaintStatus != null)
					query = query.Where(() => complaintAlias.Status == FilterViewModel.ComplaintStatus);
				if(FilterViewModel.Employee != null)
					query = query.Where(() => complaintAlias.CreatedBy.Id == FilterViewModel.Employee.Id);
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
					query.Where(() => complaintAlias.ComplaintKind.Id == FilterViewModel.ComplaintKind.Id);

				if(FilterViewModel.ComplaintObject != null)
				{
					query.Where(() => complaintObjectAlias.Id == FilterViewModel.ComplaintObject.Id);
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
					() => new CreateComplaintViewModel(
						EntityUoWBuilder.ForCreate(),
						_unitOfWorkFactory,
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
					),
					//функция диалога открытия документа
					(ComplaintJournalNode node) => new ComplaintViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						_unitOfWorkFactory,
						_commonServices,
						_undeliveredOrdersJournalOpener,
						_employeeService,
						_counterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory(),
						_fileDialogService,
						_subdivisionRepository,
						_userRepository,
						_orderSelectorFactory,
						_employeeJournalFactory,
						_counterpartyJournalFactory,
						_deliveryPointJournalFactory,
						_salesPlanJournalFactory,
						_nomenclatureSelector,
						_employeeSettings,
						new ComplaintResultsRepository(),
						_subdivisionParametersProvider
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
						_unitOfWorkFactory,
						_employeeService,
						_subdivisionRepository,
						_commonServices,
						_employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory(),
						_fileDialogService,
						new UserRepository(),
						_subdivisionParametersProvider
					),
					//функция диалога открытия документа
					(ComplaintJournalNode node) => new ComplaintViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						_unitOfWorkFactory,
						_commonServices,
						_undeliveredOrdersJournalOpener,
						_employeeService,
						_counterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory(),
						_fileDialogService,
						_subdivisionRepository,
						_userRepository,
						_orderSelectorFactory,
						_employeeJournalFactory,
						_counterpartyJournalFactory,
						_deliveryPointJournalFactory,
						_salesPlanJournalFactory,
						_nomenclatureSelector,
						_employeeSettings,
						new ComplaintResultsRepository(),
						_subdivisionParametersProvider
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
					n => _gtkDlgOpener.OpenOrderDlg(this, GetOrder(n).Id)
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Открыть маршрутный лист",
					HasRouteList,
					n => true,
					n => _gtkDlgOpener.OpenRouteListCreateDlg(this, GetRouteList(n).Id)
				)
			);

			PopupActionsList.Add(
				new JournalAction(
					"Открыть диалог закрытия МЛ",
					n => GetRouteList(n)?.CanBeOpenedInClosingDlg ?? false,
					n => true,
					n => _gtkDlgOpener.OpenRouteListClosingDlg(this, GetRouteList(n).Id)
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
								_unitOfWorkFactory,
								_commonServices,
								_undeliveredOrdersJournalOpener,
								_employeeService,
								_counterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory(),
								_fileDialogService,
								_subdivisionRepository,
								_userRepository,
								_orderSelectorFactory,
								_employeeJournalFactory,
								_counterpartyJournalFactory,
								_deliveryPointJournalFactory,
								_salesPlanJournalFactory,
								_nomenclatureSelector,
								_employeeSettings,
								new ComplaintResultsRepository(),
								_subdivisionParametersProvider
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
								_unitOfWorkFactory,
								_commonServices,
								_undeliveredOrdersJournalOpener,
								_employeeService,
								_counterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory(),
								_fileDialogService,
								_subdivisionRepository,
								_userRepository,
								_orderSelectorFactory,
								_employeeJournalFactory,
								_counterpartyJournalFactory,
								_deliveryPointJournalFactory,
								_salesPlanJournalFactory,
								_nomenclatureSelector,
								_employeeSettings,
								new ComplaintResultsRepository(),
								_subdivisionParametersProvider
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
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateEditAction();
			CreateDefaultDeleteAction();
			CreateExportAction();
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

		protected void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<ComplaintJournalNode>().ToList();
					if(selectedNodes.Count != 1) {
						return false;
					}
					ComplaintJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<ComplaintJournalNode>().ToList();
					if(selectedNodes.Count != 1) {
						return;
					}
					ComplaintJournalNode selectedNode = selectedNodes.First();
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
	}
}
