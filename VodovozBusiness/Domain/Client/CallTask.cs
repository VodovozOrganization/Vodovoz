using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Client
{
	public class CallTask : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		public virtual int DebtByAddress { get; set; }

		public virtual int DebtByClient { get; set; }

		public virtual int ClientId { get { return Address.Counterparty.Id; } }

		public virtual string Phones { get {
				string phones = null;
				foreach(var phone in Address.Phones) {
					if(phones == null)
						phones = phone.Number;
					else
						phones +=Environment.NewLine + phone.Number;
				}
				return phones;
			} 
		}

		string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		DeliveryPoint address;
		[Display(Name = "Aдрес клиента")]
		public virtual DeliveryPoint Address {
			get { return address; }
			set { SetField(ref address, value, () => Address); }
		}


		CallTaskStatus taskState;
		[Display(Name = "Статус")]
		public virtual CallTaskStatus TaskState {
			get { return taskState; }
			set { SetField(ref taskState, value, () => TaskState); }
		}

		DateTime dateOfTaskCreation;
		[Display(Name = "Дата создания задачи")]
		public virtual DateTime DateOfTaskCreation {
			get { return dateOfTaskCreation; }
			set { SetField(ref dateOfTaskCreation, value, () => DateOfTaskCreation); }
		}

		DateTime deadline;
		[Display(Name = "Срок выполнения задачи")]
		public virtual DateTime Deadline {
			get { return deadline; }
			set { SetField(ref deadline, value, () => Deadline); }
		}

		Employee assignedEmployee;
		[Display(Name = "Закреплено за")]
		public virtual Employee AssignedEmployee {
			get { return assignedEmployee; }
			set { SetField(ref assignedEmployee, value, () => AssignedEmployee); }
		}

		bool isTaskComplete;
		[Display(Name = "Статус выполнения задачи")]
		public virtual bool IsTaskComplete {
			get { return isTaskComplete; }
			set { SetField(ref isTaskComplete, value, () => IsTaskComplete); }
		}		

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Address == null)
				yield return new ValidationResult("Должна быть выбрана точка доставки", new[] { "Address" });
		}

		public virtual CallTask CreateNewTask() //TODO : Возможно переделать на фабрику
		{
			CallTask task = new CallTask();
			task.Address = Address;
			task.DateOfTaskCreation = DateTime.Now;
			task.Deadline = DateTime.Now.AddDays(1);
			task.AssignedEmployee = AssignedEmployee;
			return task;
		}

		public virtual CallTask CreateCopy() //TODO : Возможно переделать на фабрику
		{
			CallTask copy = new CallTask {
				Address = Address,
				AssignedEmployee = AssignedEmployee,
				Comment = Comment,
				DateOfTaskCreation = DateOfTaskCreation,
				Deadline = Deadline,
				DebtByAddress = DebtByAddress,
				DebtByClient = DebtByClient,
				Id = Id,
				IsTaskComplete = IsTaskComplete,
				TaskState = TaskState
			};
			return copy;
		}

		public virtual void LoadPreviousState(CallTask prevState)
		{
			if(prevState == null)
				return;

			Address = prevState.Address;
			AssignedEmployee = prevState.AssignedEmployee;
			Comment = prevState.Comment;
			DateOfTaskCreation = prevState.DateOfTaskCreation;
			Deadline = prevState.Deadline;
			DebtByAddress = prevState.DebtByAddress;
			DebtByClient = prevState.DebtByClient;
			IsTaskComplete = prevState.IsTaskComplete;
			TaskState = prevState.TaskState;
		}
	}

	public enum CallTaskStatus
	{
		[Display(Name = "Звонок")]
		Call,
		[Display(Name = "Задание")]
		Task,
		[Display(Name = "Сложный клиент")]
		DifficultClient
	}

	public class CallTaskStatusStringType : NHibernate.Type.EnumStringType
	{
		public CallTaskStatusStringType() : base(typeof(CallTaskStatus))
		{
		}
	}
}
