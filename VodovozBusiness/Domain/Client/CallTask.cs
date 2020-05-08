using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Report;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Задачи по обзвону",
		Nominative = "Задача по обзвону"
	)]
	[EntityPermission]
	[HistoryTrace]
	public class CallTask : PropertyChangedBase, IDomainObject, IValidatableObject
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
			set { SetField(ref deliveryPoint, value, () => DeliveryPoint);}
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

		private TaskSource? source;
		[Display(Name = "Источник")]
		public virtual TaskSource? Source {
			get => source;
			set => SetField(ref source, value);
		}

		private int? sourceDocumentId;
		[Display(Name = "ID документа")]
		public virtual int? SourceDocumentId {
			get => sourceDocumentId;
			set => SetField(ref sourceDocumentId, value);
		}


		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Counterparty == null)
				yield return new ValidationResult("Должна быть выбранан контрагент", new[] { "Countrerparty" });
		}

		public virtual void AddComment(IUnitOfWork UoW , string comment , out string lastComment, IEmployeeRepository employeeRepository)
		{
			var employee = employeeRepository.GetEmployeeForCurrentUser(UoW);
			comment = comment.Insert(0, employee.ShortName + $"({employee?.Subdivision?.ShortName ?? employee?.Subdivision?.Name})" + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + ": ");
			lastComment = comment;
			AddComment(UoW, comment);
		}

		public virtual void AddComment(IUnitOfWork UoW, string comment)
		{
			Comment += comment;
			Comment += Environment.NewLine;
		}

		public virtual void AddComment(IUnitOfWork UoW, string comment, IEmployeeRepository employeeRepository)
		{
			AddComment(UoW, comment, out string lastComment, employeeRepository);
		}

		public virtual ReportInfo CreateReportInfoByClient()
		{
			return CreateReportInfo(Counterparty.Id);
		}

		public virtual ReportInfo CreateReportInfoByDeliveryPoint()
		{
			return CreateReportInfo(DeliveryPoint.Counterparty.Id, DeliveryPoint.Id);
		}

		private ReportInfo CreateReportInfo(int counterpartyId, int deliveryPointId = -1)
		{
			var reportInfo = new ReportInfo {
				Title = "Акт по бутылям-залогам",
				Identifier = "Client.SummaryBottlesAndDeposits",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", null },
					{ "endDate", null },
					{ "client_id", counterpartyId},
					{ "delivery_point_id", deliveryPointId}
				}
			};
			return reportInfo;
		}
	}

	public enum CallTaskStatus
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
		DepositReturn
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

	public class TaskSourceStringType : NHibernate.Type.EnumStringType
	{
		public TaskSourceStringType() : base(typeof(TaskSource))
		{
		}
	}
}
