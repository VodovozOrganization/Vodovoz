using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Domain.Cash;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class PayoutRequestJournalNode<TEntity> : PayoutRequestJournalNode
		where TEntity : class, IDomainObject
	{
		public PayoutRequestJournalNode() : base(typeof(TEntity))
		{
		}
	}

	public class PayoutRequestJournalNode : JournalEntityNodeBase
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";

		public DateTime Date { get; set; }
		public DateTime? PaymentDatePlanned { get; set; } = null;
		public PayoutRequestState PayoutRequestState { get; set; }
		public string AuthorFullName { get; set; }
		public string AuthorName { get; set; }
		public string AuthorLastName { get; set; }
		public string AuthorPatronymic { get; set; }
		public string AccountablePersonFullName { get; set; }
		public string AccountablePersonName { get; set; }
		public string AccountablePersonLastName { get; set; }
		public string AccountablePersonPatronymic { get; set; }
		public decimal Sum { get; set; }
		public decimal SumGived { get; set; }
		public decimal SumResidue => Sum - SumGived;
		public string Basis { get; set; }
		public string ExpenseCategory { get; set; } = string.Empty;
		public bool HaveReceipt { get; set; } = false;
		public PayoutRequestDocumentType PayoutRequestDocumentType { get; set; }
		public string CounterpartyName { get; set; }
		public string AuthorWithInitials => PersonHelper.PersonNameWithInitials(AuthorLastName, AuthorName, AuthorPatronymic);
		public string AccountablePersonWithInitials => PersonHelper.PersonNameWithInitials(AccountablePersonLastName, AccountablePersonName, AccountablePersonPatronymic);
		public DateTime MoneyTransferDate { get; set; }
		public bool IsImidiatelyBill { get; set; } = false;

		protected PayoutRequestJournalNode(Type entityType) : base(entityType)
		{
		}
	}
}
