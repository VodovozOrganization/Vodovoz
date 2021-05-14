using System;
using QS.Project.Journal;
using Vodovoz.Domain.Cash;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
    public class OrganisationCashTransferDocumentJournalNode: JournalEntityNodeBase<OrganisationCashTransferDocument>
    {
        public DateTime DocumentDate { get; set; }

        public string Author { get; set; }

        public string OrganizationFrom { get; set; }

        public string OrganizationTo { get; set; }

        public decimal TransferedSum { get; set; }
    }
}
