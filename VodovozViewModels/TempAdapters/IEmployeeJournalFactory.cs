using QS.Project.Journal.EntitySelector;

namespace Vodovoz.TempAdapters
{
    public interface IEmployeeJournalFactory
    {
        IEntityAutocompleteSelectorFactory CreateEmployeeAutocompleteSelectorFactory(bool multipleSelect = false);
    }
}