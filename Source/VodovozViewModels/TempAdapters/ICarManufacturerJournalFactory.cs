using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface ICarManufacturerJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateCarManufacturerAutocompleteSelectorFactory(bool multipleSelect = false);
	}
}
