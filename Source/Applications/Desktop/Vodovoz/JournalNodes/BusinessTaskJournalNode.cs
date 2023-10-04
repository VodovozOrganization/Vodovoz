using System;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Utilities.Text;
using Vodovoz.Domain.BusinessTasks;

namespace Vodovoz.JournalNodes
{
	public class BusinessTaskJournalNode : JournalEntityNodeBase
	{
		public BusinessTaskJournalNode(Type entityType) : base(entityType) => NodeType = entityType;

		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";

		public Type NodeType { get; set; }

		public BusinessTaskStatus TaskStatus { get; set; }

		public string ClientName { get; set; }

		public string AddressName { get; set; }

		public int DebtByAddress { get; set; }

		public int DebtByClient { get; set; }

		public string DeliveryPointPhones { get; set; }

		public string CounterpartyPhones { get; set; }

		public string Phones => string.IsNullOrWhiteSpace(DeliveryPointPhones) ? CounterpartyPhones : DeliveryPointPhones;

		public string EmployeeLastName { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }

		public string AssignedEmployeeName => PersonHelper.PersonNameWithInitials(EmployeeLastName, EmployeeName, EmployeePatronymic);

		public DateTime Deadline { get; set; }

		public DateTime CreationDate { get; set; }

		public ImportanceDegreeType ImportanceDegree { get; set; }

		public bool IsTaskComplete { get; set; }

		public int TareReturn { get; set; }
	}

	public class BusinessTaskJournalNode<TEntity> : BusinessTaskJournalNode
		where TEntity : class, IDomainObject
	{
		public BusinessTaskJournalNode() : base(typeof(TEntity)) { }
	}
}
