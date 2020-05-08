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
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.BusinessTasks;
using Vodovoz.JournalNodes;
using Vodovoz.Footers.ViewModels;

namespace Vodovoz.JournalViewModels
{
	public class BusinessTasksJournalViewModel : FilterableMultipleEntityJournalViewModelBase<BusinessTaskJournalNode, CallTaskFilterViewModel>
	{
		//private int taskCount = 2;

		readonly BusinessTasksJournalFooterViewModel footerViewModel;
		//private readonly IUnitOfWorkFactory unitOfWorkFactory;
		readonly ICommonServices commonServices;

		readonly IEmployeeRepository employeeRepository;
		readonly IBottlesRepository bottleRepository;
		readonly ICallTaskRepository callTaskRepository;
		readonly IPhoneRepository phoneRepository;

		public BusinessTasksJournalViewModel(
			CallTaskFilterViewModel filterViewModel,
			BusinessTasksJournalFooterViewModel footerViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository,
			IBottlesRepository bottleRepository,
			ICallTaskRepository callTaskRepository,
			IPhoneRepository phoneRepository
		) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал задач для обзвона";
			this.employeeRepository = employeeRepository;
			this.bottleRepository = bottleRepository;
			this.callTaskRepository = callTaskRepository;
			this.phoneRepository = phoneRepository;
			this.footerViewModel = footerViewModel;
			//this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			Footer = footerViewModel;

			RegisterTasks();

			var threadLoader = DataLoader as ThreadDataLoader<BusinessTaskJournalNode>;
			//threadLoader.MergeInOrderBy(x => x.Id, false);

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
			.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhonesAlias)
			.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhonesAlias)
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
			//.List<BusinessTasksJournalNode>();
			//taskCount = tasks.Count;
			//tasks = SortResult(tasks).ToList();
			//GetStatistics();

			return tasks;
		}

		private IQueryOver<ClientTask> GetPaymenTaskQuery(IUnitOfWork uow)
		{
			DeliveryPoint deliveryPointAlias = null;
			BottlesMovementOperation bottlesMovementAlias = null;
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
			.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhonesAlias)
			.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhonesAlias)
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
			.TransformUsing(Transformers.AliasToBean<BusinessTaskJournalNode<PaymentTask>>());
			//.List<BusinessTasksJournalNode>();
			//taskCount = tasks.Count;
			//tasks = SortResult(tasks).ToList();

			return tasks;
		}

		/*
		private IEnumerable<ClientTaskJournalNode> SortResult(IEnumerable<ClientTaskJournalNode> tasks)
		{
			IEnumerable<ClientTaskJournalNode> result;
			switch(FilterViewModel.SortingParam) {
				case SortingParamType.DebtByAddress:
					result = tasks.OrderBy(x => x.DebtByAddress);
					break;
				case SortingParamType.DebtByClient:
					result = tasks.OrderBy(x => x.DebtByClient);
					break;
				case SortingParamType.AssignedEmployee:
					result = tasks.OrderBy(x => x.AssignedEmployeeName);
					break;
				case SortingParamType.Client:
					result = tasks.OrderBy(x => x.ClientName);
					break;
				case SortingParamType.Deadline:
					result = tasks.OrderBy(x => x.Deadline);
					break;
				case SortingParamType.DeliveryPoint:
					result = tasks.OrderBy(x => x.AddressName);
					break;
				case SortingParamType.Id:
					result = tasks.OrderBy(x => x.Id);
					break;
				case SortingParamType.ImportanceDegree:
					result = tasks.OrderBy(x => x.ImportanceDegree);
					break;
				case SortingParamType.Status:
					result = tasks.OrderBy(x => x.TaskStatus);
					break;
				default:
					throw new NotImplementedException();
			}

			if(FilterViewModel.SortingDirection == SortingDirectionType.FromBiggerToSmaller)
				result = result.Reverse();

			return result;
		}
		*/

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

		public void ChangeEnitity(Action<ClientTask> action, BusinessTaskJournalNode[] tasks)
		{
			if(action == null)
				return;

			tasks.ToList().ForEach((taskNode) => {
				ClientTask task = UoW.GetById<ClientTask>(taskNode.Id);
				action(task);
				UoW.Save(task);
				UoW.Commit();
			});
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
						commonServices
					),
					//функция диалога открытия документа
					(BusinessTaskJournalNode node) => new ClientTaskViewModel(
						employeeRepository,
						bottleRepository,
						callTaskRepository,
						phoneRepository,
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						commonServices
					),
					//функция идентификации документа 
					(BusinessTaskJournalNode node) => {
						return node.EntityType == typeof(ClientTask);
					},
					"Клиентская задача",
					new JournalParametersForDocument { HideJournalForCreateDialog = true, HideJournalForOpenDialog = true })
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new ClientTaskViewModel(
						employeeRepository,
						bottleRepository,
						callTaskRepository,
						phoneRepository,
						EntityUoWBuilder.ForCreate(),
						UnitOfWorkFactory,
						commonServices
					),
					//функция диалога открытия документа
					(BusinessTaskJournalNode node) => new ClientTaskViewModel(
						employeeRepository,
						bottleRepository,
						callTaskRepository,
						phoneRepository,
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						commonServices
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

				/*
				if(task is ClientTask)
					return task as ClientTask;
					*/

				return null;
			}

			void SaveTask<UTask>(UTask task)
			{

			}

			PopupActionsList.Add(
				new JournalAction(
					"Отметить как важное",
					n => true,
					n => true,
					n => {
						var task = GetTask(n);

						if(task != null) {

							if(task is ClientTask)
								SaveTask(task as ClientTask);
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
			CreateDefaultAddActions();
			CreateDefaultEditAction();
			CreateDefaultDeleteAction();

			//NodeActionsList.Add(new JournalAction("Открыть печатную форму", x => true, x => true, selectedItems => reportViewOpener.OpenReport(this, FilterViewModel.GetReportInfo())));
		}
	}
}
