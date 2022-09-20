using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface IProductGroupJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateProductGroupAutocompleteSelectorFactory();
		IEntityAutocompleteSelector CreateProductGroupAutocompleteSelector(bool multipleSelection = false);
	}
}
