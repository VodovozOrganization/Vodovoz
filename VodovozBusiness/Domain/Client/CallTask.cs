using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSContacts;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "задачи",
		Nominative = "задача",
		Prepositional = "задаче",
		PrepositionalPlural = "задачах"
	)]
	public class CallTask : PropertyChangedBase, ITask , IValidatableObject
	{
		public virtual string Title {
			get { return String.Format(" задача по обзвону : {0}", DeliveryPoint?.ShortAddress); }
		}

		public virtual int Id { get; set; }

		public virtual IList<Phone> Phones { get { return DeliveryPoint.Phones; } }

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
			set {   
					SetField(ref deliveryPoint, value, () => DeliveryPoint);
					Counterparty = deliveryPoint.Counterparty; 
				}
		}

		Counterparty counterparty;
		[Display(Name = "Клиент")]
		public virtual Counterparty Counterparty{
			get { return counterparty; }
			set { SetField(ref counterparty, value, () => Counterparty); }
		}

		CallTaskStatus taskState;
		[Display(Name = "Статус")]
		public virtual CallTaskStatus TaskState {
			get { return taskState; }
			set { SetField(ref taskState, value, () => TaskState); }
		}

		ImportanceDegreeType importanceDegree;
		[Display(Name = "Срочность задачи")]
		public virtual ImportanceDegreeType ImportanceDegree {
			get { return importanceDegree; }
			set { SetField(ref importanceDegree, value, () => ImportanceDegree); }
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
			CallTask task = new CallTask 
			{
				DeliveryPoint = DeliveryPoint,
				Counterparty = Counterparty,
				CreationDate = DateTime.Now,
				EndActivePeriod = DateTime.Now.AddDays(1),
				AssignedEmployee = AssignedEmployee
			};
			return task;
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

	public enum ImportanceDegreeType
	{
		[Display(Name = "Нет")]
		Nope,
		[Display(Name = "Важно")]
		Important
	}

	public class CallTaskStatusStringType : NHibernate.Type.EnumStringType
	{
		public CallTaskStatusStringType() : base(typeof(CallTaskStatus))
		{
		}
	}

	public class ImportanceDegreeStringType : NHibernate.Type.EnumStringType
	{
		public ImportanceDegreeStringType() : base(typeof(ImportanceDegreeType))
		{
		}
	}

	public interface ITask : IDomainObject
	{
		Counterparty Counterparty { get; set; }

		DeliveryPoint DeliveryPoint { get; set; }

		DateTime CreationDate { get; set; }

		DateTime? CompleteDate { get; set; }

		DateTime StartActivePeriod { get; set; }

		DateTime EndActivePeriod{ get; set; }

		bool IsTaskComplete { get; set; }

		Employee AssignedEmployee { get; set; }

		Employee TaskCreator { get; set; }

		string Comment { get; set; }

		IList<Phone> Phones { get; }
	}
}
