using System;
using QS.DomainModel.Entity;
using QS.Project.Journal;
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
		public DateTime Date { get; set; }
		public PayoutRequestState PayoutRequestState { get; set; }
		public string Author { get; set; }
		public string AccountablePerson { get; set; }
		public decimal Sum { get; set; }
		public string Basis { get; set; }
		public PayoutRequestDocumentType PayoutRequestDocumentType { get; set; }
		public string CounterpartyName { get; set; }

		protected PayoutRequestJournalNode(Type entityType) : base(entityType)
		{
		}
	}
}
