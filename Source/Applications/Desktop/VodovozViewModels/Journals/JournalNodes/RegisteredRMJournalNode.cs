using QS.Project.Journal;
using Vodovoz.Domain.Security;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
    public class RegisteredRMJournalNode : JournalEntityNodeBase<RegisteredRM>
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Domain { get; set; }
        public string SID { get; set; }
        public bool IsActive { get; set; }
    }
}
