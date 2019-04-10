using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Client
{
	public class CallTask : PropertyChangedBase, ITask
	{
		public virtual int Id { get; set; }

		public virtual int DebtByAddress { get; set; }

		public virtual int DebtByClient { get; set; }

		public virtual int ClientId { get { return Client != null ? Client.Id : -1; } }

		public virtual string Phones { get {
				string phones = null;
				foreach(var phone in DeliveryPoint.Phones) {
					if(phones == null)
						phones = phone.Number;
					else
						phones += Environment.NewLine + phone.Number;
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

		DeliveryPoint deliveryPoint;
		[Display(Name = "Aдрес клиента")]
		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { SetField(ref deliveryPoint, value, () => DeliveryPoint); }
		}

		[Display(Name = "Клиент")]
		public virtual Counterparty Client { get { return DeliveryPoint?.Counterparty; } set { } }

		CallTaskStatus taskState;
		[Display(Name = "Статус")]
		public virtual CallTaskStatus TaskState {
			get { return taskState; }
			set { SetField(ref taskState, value, () => TaskState); }
		}

		DateTime creationDate;
		[Display(Name = "Дата создания задачи")]
		public virtual DateTime CreationDate {
			get { return creationDate; }
			set { SetField(ref creationDate, value, () => CreationDate); }
		}

		DateTime? completeDate;
		[Display(Name = "Дата выполнения задачи")]
		public virtual DateTime? CompleteDate {
			get { return completeDate; }
			set { SetField(ref completeDate, value, () => CompleteDate); }
		}

		[Display(Name = "Период активности задачи (начало)")]
		public virtual DateTime StartActivePeriod { get { return EndActivePeriod.AddDays(-1); } set { } }

		DateTime endActivePeriod;
		[Display(Name = "Срок выполнения задачи")]
		public virtual DateTime EndActivePeriod {
			get { return endActivePeriod; }
			set { SetField(ref endActivePeriod, value, () => EndActivePeriod); }
		}

		Employee assignedEmployee;
		[Display(Name = "Закреплено за")]
		public virtual Employee AssignedEmployee {
			get { return assignedEmployee; }
			set { SetField(ref assignedEmployee, value, () => AssignedEmployee); }
		}

		Employee taskCreator;
		[Display(Name = "Создатель задачи")]
		public virtual Employee TaskCreator {
			get { return taskCreator; }
			set { SetField(ref taskCreator, value, () => TaskCreator); }
		}

		bool isTaskComplete;
		[Display(Name = "Статус выполнения задачи")]
		public virtual bool IsTaskComplete {
			get { return isTaskComplete; }
			set { 
				SetField(ref isTaskComplete, value, () => IsTaskComplete);
				completeDate =  isTaskComplete ? DateTime.Now as DateTime?  : null ;
			}
		}

		int tareReturn;
		[Display(Name = "Количество тары на сдачу")]
		public virtual int TareReturn {
			get { return tareReturn; }
			set { SetField(ref tareReturn, value, () => TareReturn); }
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(DeliveryPoint == null)
				yield return new ValidationResult("Должна быть выбрана точка доставки", new[] { "Address" });
		}

		public virtual CallTask CreateNewTask()
		{
			CallTask task = new CallTask {
				DeliveryPoint = DeliveryPoint,
				CreationDate = DateTime.Now,
				EndActivePeriod = DateTime.Now.AddDays(1),
				AssignedEmployee = AssignedEmployee
			};
			return task;
		}

		public virtual CallTask CreateCopy()
		{
			CallTask copy = new CallTask {
				DeliveryPoint = DeliveryPoint,
				AssignedEmployee = AssignedEmployee,
				Comment = Comment,
				CreationDate = CreationDate,
				EndActivePeriod = EndActivePeriod,
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

			DeliveryPoint = prevState.DeliveryPoint;
			AssignedEmployee = prevState.AssignedEmployee;
			Comment = prevState.Comment;
			CreationDate = prevState.CreationDate;
			EndActivePeriod = prevState.EndActivePeriod;
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

	public interface ITask : IDomainObject
	{
		Counterparty Client { get; set; }

		DeliveryPoint DeliveryPoint { get; set; }

		DateTime CreationDate { get; set; }

		DateTime? CompleteDate { get; set; }

		DateTime StartActivePeriod { get; set; }

		DateTime EndActivePeriod{ get; set; }

		bool IsTaskComplete { get; set; }

		Employee AssignedEmployee { get; set; }

		Employee TaskCreator { get; set; }

		string Comment { get; set; }

		string Phones { get; }
	}
}
