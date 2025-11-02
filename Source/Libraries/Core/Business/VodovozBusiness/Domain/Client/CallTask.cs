using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
		private string _comment;
		private DeliveryPoint _deliveryPoint;
		private Counterparty _counterparty;
		private CallTaskStatus _taskState;
		private ImportanceDegreeType _importanceDegree;
		private DateTime _creationDate;
		private DateTime? _completeDate;
		private DateTime _endActivePeriod;
		private Employee _assignedEmployee;
		private Employee _taskCreator;
		private bool _isTaskComplete;
		private int _tareReturn;
		private TaskSource? _source;
		private int? _sourceDocumentId;

		public virtual string Title => $" задача по обзвону : {DeliveryPoint?.ShortAddress}";

		public virtual int Id { get; set; }

		public virtual IList<Phone> Phones => DeliveryPoint.Phones;

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Aдрес клиента")]
		public virtual DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		[Display(Name = "Клиент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		[Display(Name = "Статус")]
		public virtual CallTaskStatus TaskState
		{
			get => _taskState;
			set => SetField(ref _taskState, value);
		}

		[Display(Name = "Срочность задачи")]
		public virtual ImportanceDegreeType ImportanceDegree
		{
			get => _importanceDegree;
			set => SetField(ref _importanceDegree, value);
		}

		[Display(Name = "Дата создания задачи")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		[Display(Name = "Дата выполнения задачи")]
		public virtual DateTime? CompleteDate
		{
			get => _completeDate;
			set => SetField(ref _completeDate, value);
		}

		[Display(Name = "Период активности задачи (начало)")]
		public virtual DateTime StartActivePeriod
		{
			get => EndActivePeriod.AddDays(-1);
			set { }
		}

		[Display(Name = "Срок выполнения задачи")]
		public virtual DateTime EndActivePeriod
		{
			get => _endActivePeriod;
			set => SetField(ref _endActivePeriod, value);
		}

		[Display(Name = "Закреплено за")]
		public virtual Employee AssignedEmployee
		{
			get => _assignedEmployee;
			set => SetField(ref _assignedEmployee, value);
		}

		[Display(Name = "Создатель задачи")]
		public virtual Employee TaskCreator
		{
			get => _taskCreator;
			set => SetField(ref _taskCreator, value);
		}

		[Display(Name = "Статус выполнения задачи")]
		public virtual bool IsTaskComplete
		{
			get => _isTaskComplete;
			set
			{
				SetField(ref _isTaskComplete, value);
				_completeDate = _isTaskComplete ? DateTime.Now as DateTime? : null;
			}
		}

		[Display(Name = "Количество тары на сдачу")]
		public virtual int TareReturn
		{
			get => _tareReturn;
			set => SetField(ref _tareReturn, value);
		}

		[Display(Name = "Источник")]
		public virtual TaskSource? Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}

		[Display(Name = "ID документа")]
		public virtual int? SourceDocumentId
		{
			get => _sourceDocumentId;
			set => SetField(ref _sourceDocumentId, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Counterparty == null)
			{
				yield return new ValidationResult("Должна быть выбранан контрагент", new[] { "Countrerparty" });
			}
		}

		public virtual void AddComment(IUnitOfWork UoW, string comment, out string lastComment, IEmployeeRepository employeeRepository)
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

		public virtual ReportInfo CreateReportInfoByClient(IReportInfoFactory reportInfoFactory)
		{
			return CreateReportInfo(reportInfoFactory, Counterparty.Id);
		}

		public virtual ReportInfo CreateReportInfoByDeliveryPoint(IReportInfoFactory reportInfoFactory)
		{
			return CreateReportInfo(reportInfoFactory, DeliveryPoint.Counterparty.Id, DeliveryPoint.Id);
		}

		private ReportInfo CreateReportInfo(IReportInfoFactory reportInfoFactory, int counterpartyId, int deliveryPointId = -1)
		{
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = "Акт по бутылям-залогам";
			reportInfo.Identifier = "Client.SummaryBottlesAndDeposits";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "startDate", null },
				{ "endDate", null },
				{ "client_id", counterpartyId},
				{ "delivery_point_id", deliveryPointId}
			};
			return reportInfo;
		}
	}
}
