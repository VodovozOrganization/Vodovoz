using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.BusinessTasks
{
	public class BusinessTask : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Counterparty counterparty;
		[Display(Name = "Клиент")]
		public virtual Counterparty Counterparty {
			get => counterparty;
			set => SetField(ref counterparty, value);
		}

		BusinessTaskStatus taskState;
		[Display(Name = "Статус")]
		public virtual BusinessTaskStatus TaskState {
			get => taskState;
			set => SetField(ref taskState, value);
		}

		DateTime creationDate;
		[Display(Name = "Дата создания задачи")]
		public virtual DateTime CreationDate {
			get => creationDate;
			set => SetField(ref creationDate, value);
		}

		DateTime? completeDate;
		[Display(Name = "Дата выполнения задачи")]
		public virtual DateTime? CompleteDate {
			get => completeDate;
			set => SetField(ref completeDate, value);
		}

		DateTime endActivePeriod;
		[Display(Name = "Срок выполнения задачи")]
		public virtual DateTime EndActivePeriod {
			get => endActivePeriod;
			set => SetField(ref endActivePeriod, value);
		}

		Employee assignedEmployee;
		[Display(Name = "Закреплено за")]
		public virtual Employee AssignedEmployee {
			get => assignedEmployee;
			set => SetField(ref assignedEmployee, value);
		}

		Employee taskCreator;
		[Display(Name = "Создатель задачи")]
		public virtual Employee TaskCreator {
			get => taskCreator;
			set => SetField(ref taskCreator, value);
		}

		bool isTaskComplete;
		[Display(Name = "Статус выполнения задачи")]
		public virtual bool IsTaskComplete {
			get => isTaskComplete;
			set {
				SetField(ref isTaskComplete, value);
				completeDate = isTaskComplete ? DateTime.Now as DateTime? : null;
			}
		}
	}

	public enum BusinessTaskStatus
	{
		[Display(Name = "Звонок")]
		Call,
		[Display(Name = "Задание")]
		Task,
		[Display(Name = "Сложный клиент")]
		DifficultClient,
		[Display(Name = "Первичка")]
		FirstClient,
		[Display(Name = "Cверка")]
		Reconciliation,
		[Display(Name = "Возврат залогов")]
		DepositReturn,
		[Display(Name = "Оплата")]
		Pay
	}

	public enum TaskSource
	{
		[Display(Name = "Боковая панель заказа")]
		OrderPanel,
		[Display(Name = "Автоматическое создание(из заказа)")]
		AutoFromOrder,
		[Display(Name = "Массовое создание(из журнала задолженостей)")]
		MassCreation,
		[Display(Name = "Создана вручную")]
		Handmade
	}

	public enum ImportanceDegreeType
	{
		[Display(Name = "Нет")]
		Nope,
		[Display(Name = "Важно")]
		Important
	}

	public class BusinessTaskStatusStringType : NHibernate.Type.EnumStringType
	{
		public BusinessTaskStatusStringType() : base(typeof(BusinessTaskStatus))
		{
		}
	}

	public class ImportanceDegreeStringType : NHibernate.Type.EnumStringType
	{
		public ImportanceDegreeStringType() : base(typeof(ImportanceDegreeType))
		{
		}
	}

	public class TaskSourceStringType : NHibernate.Type.EnumStringType
	{
		public TaskSourceStringType() : base(typeof(TaskSource))
		{
		}
	}
}

