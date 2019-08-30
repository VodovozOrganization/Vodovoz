using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.Config;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
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
	public class ComplaintsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<ComplaintJournalNode, ComplaintFilterViewModel>, IComplaintsInfoProvider
	{
		private readonly IEntityConfigurationProvider entityConfigurationProvider;
		private readonly ICommonServices commonServices;
		private readonly IUndeliveriesViewOpener undeliveriesViewOpener;
		private readonly IEmployeeService employeeService;
		private readonly IEntitySelectorFactory employeeSelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory counterpartySelectorFactory;
		private readonly IFilePickerService filePickerService;
		private readonly ISubdivisionRepository subdivisionRepository;
		private readonly IRouteListItemRepository routeListItemRepository;
		private readonly ISubdivisionService subdivisionService;
		private readonly IEmployeeRepository employeeRepository;
		private readonly IReportViewOpener reportViewOpener;
		private readonly IGtkTabsOpenerFromComplaintViewModel gtkDlgOpener;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		public DateTime? StartDate => null;

		public DateTime? EndDate => null;

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.ComplaintPanelView };

		public ComplaintsJournalViewModel(
			IEntityConfigurationProvider entityConfigurationProvider,
			ICommonServices commonServices,
			IUndeliveriesViewOpener undeliveriesViewOpener,
			IEmployeeService employeeService,
			IEntitySelectorFactory employeeSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			IRouteListItemRepository routeListItemRepository,
			ISubdivisionService subdivisionService,
			IEmployeeRepository employeeRepository,
			ComplaintFilterViewModel filterViewModel,
			IFilePickerService filePickerService,
			ISubdivisionRepository subdivisionRepository,
			IReportViewOpener reportViewOpener,
			IGtkTabsOpenerFromComplaintViewModel gtkDialogsOpener
		) : base(filterViewModel, entityConfigurationProvider, commonServices)
		{
			this.entityConfigurationProvider = entityConfigurationProvider ?? throw new ArgumentNullException(nameof(entityConfigurationProvider));
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

			FilterViewModel.SubdivisionService = subdivisionService;
			FilterViewModel.EmployeeRepository = employeeRepository;

			var currentEmployeeSubdivision = employeeRepository.GetEmployeeForCurrentUser(UoW).Subdivision;
			if(FilterViewModel.SubdivisionService.GetOkkId() != currentEmployeeSubdivision.Id)
				FilterViewModel.Subdivision = currentEmployeeSubdivision;
			else
				FilterViewModel.ComplaintStatus = ComplaintStatuses.Checking;

			RegisterComplaints();

			SetOrder(c => c.Id, true);

			FinishJournalConfiguration();

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
			this.ItemsListUpdated += (sender, e) => CurrentObjectChanged?.Invoke(sender, new CurrentObjectChangedArgs(null));
		}

		private IQueryOver<Complaint> GetComplaintQuery()
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
			ComplaintDiscussion dicussionAlias = null;
			Subdivision subdivisionAlias = null;

			var authorProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var workInSubdivisionsSubQuery = QueryOver.Of<Subdivision>(() => subdivisionAlias)
				.Where(() => subdivisionAlias.Id == dicussionAlias.Subdivision.Id)
				.Where(() => dicussionAlias.Status == ComplaintStatuses.InProcess)
				.Select(Projections.Conditional(
					Restrictions.IsNotNull(Projections.Property(() => subdivisionAlias.ShortName)),
					Projections.Property(() => subdivisionAlias.ShortName),
					Projections.Constant("?")
				)
			);

			var subdivisionsSubqueryProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				Projections.SubQuery(workInSubdivisionsSubQuery),
				Projections.Constant(", "));



			var okkProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1)"),
				NHibernateUtil.String,
				Projections.Conditional(
					Restrictions.Eq(Projections.Property(() => dicussionAlias.Status), ComplaintStatuses.Checking),
					Projections.Constant("ОКК"),
					Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "NULLIF(1,1)"),
						NHibernateUtil.String
					)
				)
			);

			var workInSubdivisionsProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(',', ?1, ?2)"),
				NHibernateUtil.String,
				subdivisionsSubqueryProjection,
				okkProjection
			);

			var plannedCompletionDateProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT DATE_FORMAT(?1, \"%d.%m.%Y\") SEPARATOR ?2)"),
				NHibernateUtil.String,
				Projections.Property(() => dicussionAlias.PlannedCompletionDate),
				Projections.Constant("\n"));

			var counterpartyWithAddressProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(', ', ?1, COMPILE_ADDRESS(?2))"),
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
						$"WHEN '{nameof(ComplaintGuiltyTypes.Employee)}' THEN ?2 " +
						$"WHEN '{nameof(ComplaintGuiltyTypes.Subdivision)}' THEN ?3 " +
						"ELSE '' " +
					"END" +
					" SEPARATOR ?4)"),
				NHibernateUtil.String,
				Projections.Property(() => complaintGuiltyItemAlias.GuiltyType),
				guiltyEmployeeProjection,
				Projections.Property(() => guiltySubdivisionAlias.Name),
				Projections.Constant("\n"));

			var finesProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT CONCAT(ROUND(?1, 2), ' р.')  SEPARATOR ?2)"),
				NHibernateUtil.String,
				Projections.Property(() => fineAlias.TotalMoney),
				Projections.Constant("\n"));

			var query = UoW.Session.QueryOver(() => complaintAlias)
				.Left.JoinAlias(() => complaintAlias.CreatedBy, () => authorAlias)
				.Left.JoinAlias(() => complaintAlias.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(() => complaintAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => complaintAlias.Guilties, () => complaintGuiltyItemAlias)
				.Left.JoinAlias(() => complaintAlias.Fines, () => fineAlias)
				.Left.JoinAlias(() => complaintAlias.ComplaintDiscussions, () => dicussionAlias)
				.Left.JoinAlias(() => dicussionAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Employee, () => guiltyEmployeeAlias)
				.Left.JoinAlias(() => complaintGuiltyItemAlias.Subdivision, () => guiltySubdivisionAlias);

			#region Filter

			var dicussionQuery = QueryOver.Of(() => dicussionAlias)
					.Select(Projections.Property<ComplaintDiscussion>(p => p.Id))
					.Where(() => dicussionAlias.Complaint.Id == complaintAlias.Id);

			if(FilterViewModel != null) {

				if(FilterViewModel.Subdivision != null) {
					var subdivisionFilterQuery = dicussionQuery.Where(() => dicussionAlias.Subdivision.Id == FilterViewModel.Subdivision.Id)
						.Where(() => FilterViewModel.StartDate == null || dicussionAlias.PlannedCompletionDate >= FilterViewModel.StartDate.Value)
						.And(() => dicussionAlias.PlannedCompletionDate <= FilterViewModel.EndDate);
					query.WithSubquery.WhereExists(subdivisionFilterQuery);
				}
				else if(employeeRepository.GetEmployeeForCurrentUser(UoW).Subdivision.Id == FilterViewModel.SubdivisionService.GetOkkId()) {
					query = query.Where(() => complaintAlias.PlannedCompletionDate <= FilterViewModel.EndDate)
						.And(() => FilterViewModel.StartDate == null || complaintAlias.PlannedCompletionDate >= FilterViewModel.StartDate.Value);
				}

				if(FilterViewModel.ComplaintType != null)
					query = query.Where(() => complaintAlias.ComplaintType == FilterViewModel.ComplaintType);
				if(FilterViewModel.ComplaintStatus != null)
					query = query.Where(() => complaintAlias.Status == FilterViewModel.ComplaintStatus);
				if(FilterViewModel.Employee != null)
					query = query.Where(() => complaintAlias.CreatedBy.Id == FilterViewModel.Employee.Id);
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
				.Select(counterpartyWithAddressProjection).WithAlias(() => resultAlias.ClientNameWithAddress)
				.Select(guiltiesProjection).WithAlias(() => resultAlias.Guilties)
				.Select(authorProjection).WithAlias(() => resultAlias.Author)
				.Select(finesProjection).WithAlias(() => resultAlias.Fines)
				.Select(() => complaintAlias.ComplaintText).WithAlias(() => resultAlias.ComplaintText)
				.Select(() => complaintAlias.ResultText).WithAlias(() => resultAlias.ResultText)
				.Select(() => complaintAlias.ActualCompletionDate).WithAlias(() => resultAlias.ActualCompletionDate)
			);

			query.TransformUsing(Transformers.AliasToBean<ComplaintJournalNode>())
				 .OrderBy(n => n.Id)
				 .Desc()
				 ;

			return query;
		}

		private void RegisterComplaints()
		{
			var complaintConfig = RegisterEntity<Complaint>(GetComplaintQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new CreateComplaintViewModel(
						EntityConstructorParam.ForCreate(),
						employeeService,
						counterpartySelectorFactory,
						subdivisionRepository,
						commonServices
					),
					//функция диалога открытия документа
					(ComplaintJournalNode node) => new ComplaintViewModel(
						EntityConstructorParam.ForOpen(node.Id),
						commonServices,
						undeliveriesViewOpener,
						employeeService,
						employeeSelectorFactory,
						counterpartySelectorFactory,
						entityConfigurationProvider,
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
					() => new CreateInnerComplaintViewModel(EntityConstructorParam.ForCreate(), employeeService, subdivisionRepository, commonServices),
					//функция диалога открытия документа
					(ComplaintJournalNode node) => new ComplaintViewModel(
						EntityConstructorParam.ForOpen(node.Id),
						commonServices,
						undeliveriesViewOpener,
						employeeService,
						employeeSelectorFactory,
						counterpartySelectorFactory,
						entityConfigurationProvider,
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

		protected override void BeforeItemsUpdated()
		{
			foreach(ComplaintJournalNode item in Items) {
				item.SequenceNumber = Items.IndexOf(item) + 1;
			}
			base.BeforeItemsUpdated();
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
								EntityConstructorParam.ForOpen(currentComplaintId.Value),
								commonServices,
								undeliveriesViewOpener,
								employeeService,
								employeeSelectorFactory,
								counterpartySelectorFactory,
								entityConfigurationProvider,
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
								EntityConstructorParam.ForOpen(currentComplaintId.Value),
								commonServices,
								undeliveriesViewOpener,
								employeeService,
								employeeSelectorFactory,
								counterpartySelectorFactory,
								entityConfigurationProvider,
								filePickerService,
								subdivisionRepository
							);
							currentComplaintVM.Entity.Close();
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
