using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using Vodovoz.Domain.BusinessTasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.BusinessTasks;
using Vodovoz.JournalNodes;
using Vodovoz.Footers.ViewModels;
using Vodovoz.Models;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Journals.JournalFactories;
using CounterpartyContractFactory = Vodovoz.Factories.CounterpartyContractFactory;
using QS.Navigation;

namespace Vodovoz.JournalViewModels
{
	public class BusinessTasksJournalViewModel : FilterableMultipleEntityJournalViewModelBase<BusinessTaskJournalNode, CallTaskFilterViewModel>
	{
		readonly BusinessTasksJournalFooterViewModel footerViewModel;
		readonly ICommonServices commonServices;

		readonly IEmployeeRepository employeeRepository;
		readonly IBottlesRepository bottleRepository;
		readonly ICallTaskRepository callTaskRepository;
		readonly IPhoneRepository phoneRepository;
		private readonly IOrganizationProvider organizationProvider;
		private readonly ICounterpartyContractRepository counterpartyContractRepository;
		private readonly CounterpartyContractFactory counterpartyContractFactory;
		private readonly RoboatsJournalsFactory _roboAtsCounterpartyJournalFactory;
		private readonly IContactParametersProvider _contactsParameters;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly INavigationManager _navigationManager;

		public BusinessTasksJournalActionsViewModel actionsViewModel { get; set; }

		public BusinessTasksJournalViewModel(
			CallTaskFilterViewModel filterViewModel,
			BusinessTasksJournalFooterViewModel footerViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository,
			IBottlesRepository bottleRepository,
			ICallTaskRepository callTaskRepository,
			IPhoneRepository phoneRepository,
			IOrganizationProvider organizationProvider,
			ICounterpartyContractRepository counterpartyContractRepository,
			CounterpartyContractFactory counterpartyContractFactory,
			RoboatsJournalsFactory roboAtsCounterpartyJournalFactory,
			IContactParametersProvider contactsParameters,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			INavigationManager navigationManager
		) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал задач для обзвона";
			this.employeeRepository = employeeRepository;
			this.bottleRepository = bottleRepository;
			this.callTaskRepository = callTaskRepository;
			this.phoneRepository = phoneRepository;
			this.organizationProvider = organizationProvider ?? throw new ArgumentNullException(nameof(organizationProvider));
			this.counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			this.counterpartyContractFactory = counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
			this.footerViewModel = footerViewModel;
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_roboAtsCounterpartyJournalFactory = roboAtsCounterpartyJournalFactory ?? throw new ArgumentNullException(nameof(roboAtsCounterpartyJournalFactory));
			_contactsParameters = contactsParameters ?? throw new ArgumentNullException(nameof(contactsParameters));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			actionsViewModel = new BusinessTasksJournalActionsViewModel(new EmployeeJournalFactory());

			RegisterTasks();

			var threadLoader = DataLoader as ThreadDataLoader<BusinessTaskJournalNode>;

			FinishJournalConfiguration();

			UpdateOnChanges(
				typeof(ClientTask),
				typeof(PaymentTask)
			);

			DataLoader.ItemsListUpdated += (sender, e) => GetStatistics();
		}

		private IQueryOver<ClientTask> GetClientTaskQuery(IUnitOfWork uow)
		{
			DeliveryPoint deliveryPointAlias = null;
			BottlesMovementOperation bottlesMovementAlias = null;
			ClientTask callTaskAlias = null;
			BusinessTaskJournalNode<ClientTask> resultAlias = null;
			Counterparty counterpartyAlias = null;
			Employee employeeAlias = null;
			Phone deliveryPointPhonesAlias = null;
			Phone counterpartyPhonesAlias = null;
			Domain.Orders.Order orderAlias = null;

			var tasksQuery = UoW.Session.QueryOver(() => callTaskAlias)
						.Left.JoinAlias(() => callTaskAlias.DeliveryPoint, () => deliveryPointAlias);

			switch(FilterViewModel.DateType) {
				case TaskFilterDateType.CreationTime:
					tasksQuery.Where(x => x.CreationDate >= FilterViewModel.StartDate.Date)
							  .And(x => x.CreationDate <= FilterViewModel.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
					break;
				case TaskFilterDateType.CompleteTaskDate:
					tasksQuery.Where(x => x.CompleteDate >= FilterViewModel.StartDate.Date)
							  .And(x => x.CompleteDate <= FilterViewModel.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
					break;
				default:
					tasksQuery.Where(x => x.EndActivePeriod >= FilterViewModel.StartDate.Date)
							  .And(x => x.EndActivePeriod <= FilterViewModel.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
					break;
			}

			if(FilterViewModel.Employee != null)
				tasksQuery.Where(x => x.AssignedEmployee == FilterViewModel.Employee);
			else if(FilterViewModel.ShowOnlyWithoutEmployee)
				tasksQuery.Where(x => x.AssignedEmployee == null);

			if(FilterViewModel.HideCompleted)
				tasksQuery.Where(x => !x.IsTaskComplete);

			if(FilterViewModel.DeliveryPointCategory != null)
				tasksQuery.Where(() => deliveryPointAlias.Category == FilterViewModel.DeliveryPointCategory);

			var bottleDebtByAddressQuery = UoW.Session.QueryOver(() => bottlesMovementAlias)
			.JoinAlias(() => bottlesMovementAlias.Order, () => orderAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
			.And(() => bottlesMovementAlias.DeliveryPoint.Id == deliveryPointAlias.Id || orderAlias.SelfDelivery)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			var bottleDebtByClientQuery = UoW.Session.QueryOver(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered) }
				));

			tasksQuery.Where(
					GetSearchCriterion(
					() => callTaskAlias.Id,
					() => deliveryPointAlias.ShortAddress,
					() => counterpartyAlias.Name,
					() => callTaskAlias.TaskState
				)
			);

			var tasks = tasksQuery
			.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhonesAlias, () => !deliveryPointPhonesAlias.IsArchive)
			.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhonesAlias, () => !counterpartyPhonesAlias.IsArchive)
			.Left.JoinAlias(() => callTaskAlias.Counterparty, () => counterpartyAlias)
			.Left.JoinAlias(() => callTaskAlias.AssignedEmployee, () => employeeAlias)
			.SelectList(list => list
				   .SelectGroup(() => callTaskAlias.Id)
				   .Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.AddressName)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.ClientName)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmployeeName)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeLastName)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
				   .Select(() => callTaskAlias.EndActivePeriod).WithAlias(() => resultAlias.Deadline)
				   .Select(() => callTaskAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
				   .Select(() => callTaskAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => callTaskAlias.TaskState).WithAlias(() => resultAlias.TaskStatus)
				   .Select(() => callTaskAlias.ImportanceDegree).WithAlias(() => resultAlias.ImportanceDegree)
				   .Select(() => callTaskAlias.IsTaskComplete).WithAlias(() => resultAlias.IsTaskComplete)
				   .Select(() => callTaskAlias.TareReturn).WithAlias(() => resultAlias.TareReturn)
				   .Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( CONCAT(?2 , ?1) SEPARATOR ?3 )"),
					   NHibernateUtil.String,
					   Projections.Property(() => deliveryPointPhonesAlias.DigitsNumber),
					   Projections.Constant("+7"),
					   Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.DeliveryPointPhones)
				   .Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( CONCAT(?2 , ?1) SEPARATOR ?3 )"),
					   NHibernateUtil.String,
					   Projections.Property(() => counterpartyPhonesAlias.DigitsNumber),
					   Projections.Constant("+7"),
					   Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.CounterpartyPhones)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByAddressQuery).WithAlias(() => resultAlias.DebtByAddress)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByClientQuery).WithAlias(() => resultAlias.DebtByClient)
				)
			.TransformUsing(Transformers.AliasToBean<BusinessTaskJournalNode<ClientTask>>());

			return tasks;
		}

		private IQueryOver<ClientTask> GetPaymenTaskQuery(IUnitOfWork uow)
		{
			DeliveryPoint deliveryPointAlias = null;
			ClientTask callTaskAlias = null;
			BusinessTaskJournalNode<PaymentTask> resultAlias = null;
			Counterparty counterpartyAlias = null;
			Employee employeeAlias = null;
			Phone deliveryPointPhonesAlias = null;
			Phone counterpartyPhonesAlias = null;
			Domain.Orders.Order orderAlias = null;

			var tasksQuery = UoW.Session.QueryOver(() => callTaskAlias)
						.Left.JoinAlias(() => callTaskAlias.DeliveryPoint, () => deliveryPointAlias);

			switch(FilterViewModel.DateType) {
				case TaskFilterDateType.CreationTime:
					tasksQuery.Where(x => x.CreationDate >= FilterViewModel.StartDate.Date)
							  .And(x => x.CreationDate <= FilterViewModel.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
					break;
				case TaskFilterDateType.CompleteTaskDate:
					tasksQuery.Where(x => x.CompleteDate >= FilterViewModel.StartDate.Date)
							  .And(x => x.CompleteDate <= FilterViewModel.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
					break;
				default:
					tasksQuery.Where(x => x.EndActivePeriod >= FilterViewModel.StartDate.Date)
							  .And(x => x.EndActivePeriod <= FilterViewModel.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
					break;
			}

			if(FilterViewModel.Employee != null)
				tasksQuery.Where(x => x.AssignedEmployee == FilterViewModel.Employee);
			else if(FilterViewModel.ShowOnlyWithoutEmployee)
				tasksQuery.Where(x => x.AssignedEmployee == null);

			if(FilterViewModel.HideCompleted)
				tasksQuery.Where(x => !x.IsTaskComplete);

			if(FilterViewModel.DeliveryPointCategory != null)
				tasksQuery.Where(() => deliveryPointAlias.Category == FilterViewModel.DeliveryPointCategory);

			tasksQuery.Where(
					GetSearchCriterion(
					() => callTaskAlias.Id,
					() => deliveryPointAlias.ShortAddress,
					() => counterpartyAlias.Name,
					() => callTaskAlias.TaskState
				)
			);

			var tasks = tasksQuery
			.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhonesAlias, () => !deliveryPointPhonesAlias.IsArchive)
			.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhonesAlias, () => !counterpartyPhonesAlias.IsArchive)
			.Left.JoinAlias(() => callTaskAlias.Counterparty, () => counterpartyAlias)
			.Left.JoinAlias(() => callTaskAlias.AssignedEmployee, () => employeeAlias)
			.SelectList(list => list
				   .SelectGroup(() => callTaskAlias.Id)
				   .Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.AddressName)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.ClientName)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmployeeName)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeLastName)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
				   .Select(() => callTaskAlias.EndActivePeriod).WithAlias(() => resultAlias.Deadline)
				   .Select(() => callTaskAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
				   .Select(() => callTaskAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => callTaskAlias.TaskState).WithAlias(() => resultAlias.TaskStatus)
				   .Select(() => callTaskAlias.ImportanceDegree).WithAlias(() => resultAlias.ImportanceDegree)
				   .Select(() => callTaskAlias.IsTaskComplete).WithAlias(() => resultAlias.IsTaskComplete))
			.TransformUsing(Transformers.AliasToBean<BusinessTaskJournalNode<PaymentTask>>());

			return tasks;
		}

		public void GetStatistics()
		{
			DateTime start = FilterViewModel.StartDate.Date;
			DateTime end = FilterViewModel.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
			CallTask tasksAlias = null;

			var baseQuery = UoW.Session.QueryOver(() => tasksAlias)
				.Where(() => tasksAlias.CompleteDate >= start)
				.And(() => tasksAlias.CompleteDate <= end);

			if(FilterViewModel.Employee != null)
				baseQuery.And(() => tasksAlias.AssignedEmployee.Id == FilterViewModel.Employee.Id);

			var callTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.Call);
			footerViewModel.RingCount = callTaskQuery.RowCount();

			var difTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.DifficultClient);
			footerViewModel.HardClientsCount = difTaskQuery.RowCount();

			var jobTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.Task);
			footerViewModel.TasksCount = jobTaskQuery.RowCount();

			footerViewModel.Tasks = DataLoader.Items.Count;

			footerViewModel.TareReturn = DataLoader.Items.OfType<BusinessTaskJournalNode>().Sum(x => x.TareReturn);
		}

		private void RegisterTasks()
		{
			var taskConfig = RegisterEntity(GetClientTaskQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new ClientTaskViewModel(
						employeeRepository,
						bottleRepository,
						callTaskRepository,
						phoneRepository,
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory,
						organizationProvider,
						counterpartyContractRepository,
						counterpartyContractFactory,
						_contactsParameters,
						commonServices,
						_roboAtsCounterpartyJournalFactory,
						_counterpartyJournalFactory
					),
					//функция диалога открытия документа
					(BusinessTaskJournalNode node) => new ClientTaskViewModel(
						employeeRepository,
						bottleRepository,
						callTaskRepository,
						phoneRepository,
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						organizationProvider,
						counterpartyContractRepository,
						counterpartyContractFactory,
						_contactsParameters,
						commonServices,
						_roboAtsCounterpartyJournalFactory,
						_counterpartyJournalFactory
					),
					//функция идентификации документа
					(BusinessTaskJournalNode node) => {
						return node.EntityType == typeof(ClientTask);
					},
					"Клиентская задача",
					new JournalParametersForDocument { HideJournalForCreateDialog = true, HideJournalForOpenDialog = true })
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new PaymentTaskViewModel(
						employeeRepository,
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory,
						commonServices,
						_counterpartyJournalFactory
					),
					//функция диалога открытия документа
					(BusinessTaskJournalNode node) => new PaymentTaskViewModel(
						employeeRepository,
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						commonServices,
						_counterpartyJournalFactory
					),
					//функция идентификации документа
					(BusinessTaskJournalNode node) => {
						return node.EntityType == typeof(PaymentTask);
					},
					"Задача по платежам",
					new JournalParametersForDocument { HideJournalForCreateDialog = true, HideJournalForOpenDialog = true });

			//завершение конфигурации
			taskConfig.FinishConfiguration();
		}

		protected override void CreatePopupActions()
		{
			object GetTask(object[] objs)
			{
				var selectedNodes = objs.Cast<BusinessTaskJournalNode>();

				if(selectedNodes.Count() != 1)
					return null;

				var node = selectedNodes.FirstOrDefault();

				var task = UoW.GetById(node.NodeType, node.Id);


				return null;
			}

			bool HasTask(object[] objs) => GetTask(objs) != null;

			PopupActionsList.Add(
				new JournalAction(
					"Отметить как важное",
					HasTask,
					n => true,
					n => {
						var task = GetTask(n);

						if(task != null) {

							if(task is ClientTask)
								UoW.Save(task as ClientTask);
							//else
							//	SaveTask(task as PaymentTask);
						}
					}
				)
			);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			//CreateDefaultAddActions2();
			//CreateDefaultEditAction2();
			CreateDefaultDeleteAction();
		}

		public void ChangeAssignedEmployee(object[] selectedObjs, Employee employee)
		{
			var nodes = selectedObjs.OfType<BusinessTaskJournalNode>().ToLookup(key => key.EntityType, val => val.Id);

			foreach(var tasks in nodes) {

				foreach(int task in tasks) {
					Type type = tasks.Key;

					//Type obj = UoW.GetById(type, task);
				}
			}

		}

		public void ChangeTasksState(object[] selectedObjs, BusinessTaskStatus status)
		{
			var nodes = selectedObjs.OfType<BusinessTaskJournalNode>().ToLookup(key => key.EntityType, val => val.Id);

			foreach(var tasks in nodes) {

				foreach(int task in tasks) {
					Type type = tasks.Key;

					var obj = UoW.GetById(type, task);
				}
			}
		}

		public void CompleteSelectedTasks(object[] selectedObjs)
		{
			var nodes = selectedObjs.OfType<BusinessTaskJournalNode>().ToLookup(key => key.EntityType, val => val.Id);

			foreach(var tasks in nodes) {

				foreach(int task in tasks) {
					Type type = tasks.Key;

					var obj = UoW.GetById(type, task);
				}
			}

		}

		public void ChangeDeadlineDate(object[] selectedObjs, DateTime date)
		{
			var nodes = selectedObjs.OfType<BusinessTaskJournalNode>().ToLookup(key => key.EntityType, val => val.Id);

			foreach(var tasks in nodes) {

				foreach(int task in tasks) {
					Type type = tasks.Key;

					var obj = UoW.GetById(type, task);
				}
			}

		}


		/*
		public void ChangeEnitity(BusinessTaskJournalNode[] tasks)
		{
			tasks.ToList().ForEach((taskNode) => {
				Type task = taskNode.EntityType;
				action(task);
				UoW.Save(task);
				UoW.Commit();
			});
		}
		*/
	}
}
