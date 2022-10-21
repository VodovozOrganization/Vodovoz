using QS.Project.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Domain.Retail;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
    public class SalesChannelJournalNode : JournalEntityNodeBase<SalesChannel>
    {
        public override string Title => Name;

        public string Name { get; set; }
    }
}
