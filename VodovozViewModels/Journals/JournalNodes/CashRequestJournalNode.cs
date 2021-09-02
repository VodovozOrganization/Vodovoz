using System;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using Vodovoz.Domain.Cash;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class CashRequestJournalNode<TEntity> : CashRequestJournalNode
		where TEntity : class, IDomainObject
	{
		public CashRequestJournalNode() : base(typeof(TEntity)) { }
	}

    public class CashRequestJournalNode : JournalEntityNodeBase
    {
        public DateTime Date { get; set; }
        public PayoutRequestState PayoutRequestState { get; set; }
        public string Author { get; set; }
        public string AccountablePerson { get; set; }
        public decimal Sum { get; set; }
        public string Basis { get; set; }
        public PayoutRequestDocumentType PayoutRequestDocumentType { get; set; }
        public string CounterpartyName { get; set; }

        protected CashRequestJournalNode(Type entityType) : base(entityType)
        {
        }
    }
}
