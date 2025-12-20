using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;
using Vodovoz.Settings.Employee;

namespace Vodovoz.Tools.CallTasks
{
	public class CallTaskWorker : ICallTaskWorker
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _uowfactory;
		private IEmployeeSettings _employeeSettings;

		public ITaskCreationInteractive TaskCreationInteractive { get; set; }

		private ICallTaskFactory callTaskFactory { get; }
		private ICallTaskRepository callTaskRepository { get; }
		private IOrderRepository orderRepository { get; }
		private IEmployeeRepository employeeRepository { get; }
		private IUserService userService { get; }
		private IErrorReporter errorReporter { get; }

		public CallTaskWorker(
			IUnitOfWorkFactory uowfactory,
			ICallTaskFactory callTaskFactory,
			ICallTaskRepository callTaskRepository,
			IOrderRepository orderRepository,
			IEmployeeRepository employeeRepository,
			IEmployeeSettings employeeSettings,
			IUserService userService,
			IErrorReporter errorReporter,
			ITaskCreationInteractive taskCreationInteractive = null
			)
		{
			_uowfactory = uowfactory ?? throw new ArgumentNullException(nameof(uowfactory));
			this.callTaskFactory = callTaskFactory ?? throw new ArgumentNullException(nameof(callTaskFactory));
			this.callTaskRepository = callTaskRepository ?? throw new ArgumentNullException(nameof(callTaskRepository));
			this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			this.userService = userService;
			this.errorReporter = errorReporter;
			TaskCreationInteractive = taskCreationInteractive;
		}

		public void CreateTasks(Order order)
		{
			//Выполняется синхронно, т.к может выводить окно TaskCreationInteractive
			if(order.OrderStatus == OrderStatus.Accepted)
				UpdateCallTask(order);
			if(order.OrderStatus == OrderStatus.Shipped)
				UpdateDepositReturnTask(order);

			Task.Run(() =>
			{
				using(var uow = _uowfactory.CreateWithoutRoot())
				{
					try
					{
						switch(order.OrderStatus)
						{
							case OrderStatus.Accepted:
								CreateTaskIfCounterpartyRelocated(order, uow);
								break;
							case OrderStatus.DeliveryCanceled:
								TryDeleteTask(order, uow);
								break;
						}

						uow.Commit();
					}
					catch(Exception ex)
					{
						var currUser = userService?.GetCurrentUser();
						errorReporter?.AutomaticSendErrorReport($"Ошибка в {nameof(CallTaskWorker)}", currUser, ex);
					}
				}
			});
		}

		//Создаёт задачу если клиент переехал
		private void CreateTaskIfCounterpartyRelocated(Order order, IUnitOfWork uow)
		{
			if( //Есть тара на возврат
				!order.BottlesReturn.HasValue || order.BottlesReturn.Value == 0
				//Указана точка доставки
				|| order.SelfDelivery || order.DeliveryPoint == null
				//Нет заказов на указанную точку доставки
				|| uow.Session.QueryOver<Order>().Where(x => x.DeliveryPoint == order.DeliveryPoint).List().Any(x => x.Id != order.Id)
				//Нет созданных CallTask на указанную точку доставки
				|| uow.Session.QueryOver<CallTask>().Where(x => x.DeliveryPoint == order.DeliveryPoint).List().Any()
				//Есть заказы на другую точку доставки
				|| !uow.Session.QueryOver<Order>()
						.Where(x => x.Client == order.Client)
						.Where(x => !x.SelfDelivery)
						.Where(x => x.DeliveryPoint != order.DeliveryPoint)
						.List().Any(x => x.Id != order.Id)
			) {
				return;
			}

			var newTask = new CallTask();
			callTaskFactory.FillNewTask(uow, newTask, employeeRepository);
			newTask.AssignedEmployee = uow.GetById<Employee>(_employeeSettings.DefaultEmployeeForCallTask);
			newTask.TaskState = CallTaskStatus.Task;
			newTask.DeliveryPoint = order.DeliveryPoint;
			newTask.Counterparty = order.Client;
			newTask.EndActivePeriod = DateTime.Now.Date.AddHours(23).AddMinutes(59);
			newTask.SourceDocumentId = order.Id;
			newTask.Source = TaskSource.AutoFromOrder;
			uow.Save(newTask);
		}

		private bool UpdateCallTask(Order order)
		{
			IEnumerable<CallTask> tasks;
			if(order.SelfDelivery)
				tasks = callTaskRepository.GetActiveSelfDeliveryTaskByCounterparty(order.UoW, order.Client, CallTaskStatus.Call);
			else
				tasks = callTaskRepository.GetActiveTaskByDeliveryPoint(order.UoW, order.DeliveryPoint, CallTaskStatus.Call);
			if(tasks?.FirstOrDefault() == null)
				return false;

			DateTime? dateTime = null;
			if(TaskCreationInteractive != null) {
				if(TaskCreationInteractive.RunQuestion(ref dateTime) == CreationTaskResult.Cancel)
					return false;
			}

			bool autoDate = false;
			if(dateTime == null) {
				autoDate = true;
				int? ordersCount;
				double dif = orderRepository.GetAvgRangeBetweenOrders(order.UoW, order.DeliveryPoint, out ordersCount);
				dateTime = (ordersCount.HasValue && ordersCount.Value >= 3) ? order.DeliveryDate.Value.AddDays(dif) : DateTime.Now.AddMonths(1);
			}

			var newTask = new CallTask();
			callTaskFactory.FillNewTask(order.UoW, newTask, employeeRepository);
			newTask.AssignedEmployee = tasks?.OrderBy(x => x.EndActivePeriod).LastOrDefault().AssignedEmployee;
			newTask.TaskState = CallTaskStatus.Call;
			newTask.DeliveryPoint = order.DeliveryPoint;
			newTask.Counterparty = order.Client;
			newTask.EndActivePeriod = dateTime.Value;
			newTask.SourceDocumentId = order.Id;
			newTask.Source = TaskSource.AutoFromOrder;
			order.UoW.Save(newTask);

			if(tasks?.FirstOrDefault() != null) {
				foreach(var task in tasks) {
					string comment;
					task.IsTaskComplete = true;

					if(autoDate)
						comment = $"Автоперенос задачи на {dateTime?.ToString("dd/MM/yyyy")}";
					else
						comment = $"Ручной перенос задачи на {dateTime?.ToString("dd/MM/yyyy")}";

					task.AddComment(order.UoW, comment);
					order.UoW.Save(task);
				}
			}
			return true;
		}

		private bool UpdateDepositReturnTask(Order order)
		{
			bool createTask = false;
			var equipmentToClient = order.OrderEquipments.Where(x => x.Direction == Direction.Deliver);
			var equipmentFromClient = order.OrderEquipments.Where(x => x.Direction == Direction.PickUp);

			foreach(var item in equipmentFromClient.ToList()) {
				if(!equipmentToClient.ToList().Any(x => x.Nomenclature.Id == item.Nomenclature.Id))
					createTask = true;
			}

			if(!createTask)
				return false;

			CallTask activeTask;

			if(order.SelfDelivery)
				activeTask = callTaskRepository.GetActiveSelfDeliveryTaskByCounterparty(order.UoW, order.Client, CallTaskStatus.DepositReturn, 1)?.FirstOrDefault();
			else
				activeTask = callTaskRepository.GetActiveTaskByDeliveryPoint(order.UoW, order.DeliveryPoint, CallTaskStatus.DepositReturn, 1)?.FirstOrDefault();

			if(activeTask != null)
				return false;

			var newTask = new CallTask();
			callTaskFactory.FillNewTask(order.UoW, newTask, employeeRepository);
			newTask.AssignedEmployee = order.UoW.GetById<Employee>(_employeeSettings.DefaultEmployeeForDepositReturnTask);
			newTask.TaskState = CallTaskStatus.DepositReturn;
			newTask.DeliveryPoint = order.DeliveryPoint;
			newTask.Counterparty = order.Client;
			newTask.EndActivePeriod = DateTime.Now.Date.AddHours(23).AddMinutes(59);
			newTask.SourceDocumentId = order.Id;
			newTask.Source = TaskSource.AutoFromOrder;
			order.UoW.Save(newTask);

			return true;
		}

		private bool TryDeleteTask(Order order, IUnitOfWork uow)
		{
			var tasks = callTaskRepository.GetAutoGeneratedTask(uow, order, CallTaskStatus.DepositReturn);

			if(tasks?.FirstOrDefault() == null)
				return false;

			foreach(var task in tasks)
				uow.Delete(task);

			return true;
		}
	}
}
