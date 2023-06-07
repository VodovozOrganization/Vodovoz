using System;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using Vodovoz.Domain.Cash;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
    public class OrganizationCashTransferDocumentJournalNode: JournalEntityNodeBase<OrganizationCashTransferDocument>
    {
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";

        public DateTime DocumentDate { get; set; }

		public string Author { get; set; }

        public string OrganizationFrom { get; set; }

        public string OrganizationTo { get; set; }

        public decimal TransferedSum { get; set; }

		public string Comment { get; set; }
	}
}
