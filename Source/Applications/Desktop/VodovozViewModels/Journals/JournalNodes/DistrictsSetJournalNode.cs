using System;
using QS.Project.Journal;
using QS.Utilities.Text;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.JournalNodes
{
    public class DistrictsSetJournalNode : JournalEntityNodeBase<DistrictsSet>
    {
		public override string Title => Name;
        public string Name { get; set; }
		public DateTime DateCreated { get; set; }
        public DateTime? DateActivated { get; set; }
        public DateTime? DateClosed { get; set; }
        public DistrictsSetStatus Status { get; set; }
        public string Comment { get; set; }
        
        public string AuthorName { get; set; }
        public string AuthorLastName { get; set; }
        public string AuthorPatronymic { get; set; }
        public string Author => PersonHelper.PersonNameWithInitials(AuthorLastName, AuthorName, AuthorPatronymic);
    }
}
