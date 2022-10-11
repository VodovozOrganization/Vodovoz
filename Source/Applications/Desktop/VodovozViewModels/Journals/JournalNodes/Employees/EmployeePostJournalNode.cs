using System;
using QS.Project.Journal;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
    public class EmployeePostJournalNode : JournalEntityNodeBase<EmployeePost>
    {
        public override string Title => EmployeePostName;

        public string EmployeePostName { get; set; }
    }
}