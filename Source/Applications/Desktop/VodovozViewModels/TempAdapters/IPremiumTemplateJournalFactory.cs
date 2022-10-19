using QS.Project.Journal.EntitySelector;

namespace Vodovoz.TempAdapters
{
    public interface IPremiumTemplateJournalFactory
    {
        IEntityAutocompleteSelectorFactory CreatePremiumTemplateAutocompleteSelectorFactory();
    }
}