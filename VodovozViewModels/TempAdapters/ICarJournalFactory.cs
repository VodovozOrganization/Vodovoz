using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;

namespace Vodovoz.TempAdapters
{
    public interface ICarJournalFactory
    {
        IEntityAutocompleteSelectorFactory CreateCarAutocompleteSelectorFactory();
    }
}