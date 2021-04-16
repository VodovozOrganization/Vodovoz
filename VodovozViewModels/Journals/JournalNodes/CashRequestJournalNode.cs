using System;
using System.Collections.Generic;
using QS.Project.Journal;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
    public class CashRequestJournalNode: JournalEntityNodeBase<CashRequest>
    {
        public DateTime Date { get; set; }
        public CashRequest.States State { get; set; }
        public string Author { get; set; }
        public string AccountablePerson { get; set; }
        public decimal Sum { get; set; }
        public string Basis { get; set; }
        public CashRequest.DocumentTypes DocumentType { get; set; }
    }
}