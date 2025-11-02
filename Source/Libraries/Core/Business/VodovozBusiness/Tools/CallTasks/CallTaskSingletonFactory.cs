using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Settings.Employee;

namespace Vodovoz.Tools.CallTasks
{
	public class CallTaskSingletonFactory : ICallTaskFactory
	{
		private static CallTaskSingletonFactory instance;

		public static CallTaskSingletonFactory GetInstance()
		{
			if(instance == null)
				instance = new CallTaskSingletonFactory();
			return instance;
		}

		public CallTaskSingletonFactory() { }

		public CallTask CreateTask(IUnitOfWork uow, IEmployeeRepository employeeRepository, IEmployeeSettings employeeSettings, CallTask newTask = null, object source = null, string creationComment = null)
		{
			CallTask callTask = newTask ?? new CallTask();
			FillNewTask(uow, callTask, employeeRepository);

			switch(source) {
				case Order order:
					FillFromOrder(uow, callTask, employeeSettings, order);
					break;
			}

			if(creationComment != null)
				callTask.AddComment(uow, creationComment, employeeRepository);
			return callTask;
		}

		private void FillFromOrder(IUnitOfWork uow, CallTask callTask, IEmployeeSettings employeeSettings, Order order)
		{
			callTask.Counterparty = uow.GetById<Counterparty>(order.Client.Id);
			callTask.DeliveryPoint = uow.GetById<DeliveryPoint>(order.DeliveryPoint.Id);
			callTask.TaskState = CallTaskStatus.Reconciliation;
			callTask.AssignedEmployee = uow.GetById<Employee>(employeeSettings.DefaultEmployeeForCallTask);
			callTask.SourceDocumentId = order.Id;
		}

		public CallTask FillNewTask(IUnitOfWork uow,CallTask callTask ,IEmployeeRepository employeeRepository)
		{
			callTask.CreationDate = DateTime.Now;
			callTask.TaskCreator = employeeRepository.GetEmployeeForCurrentUser(uow);
			callTask.EndActivePeriod = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
			return callTask;
		}
	}
}
