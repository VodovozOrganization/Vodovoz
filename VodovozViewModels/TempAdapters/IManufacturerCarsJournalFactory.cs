using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IManufacturerCarsJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateManufacturerCarsAutocompleteSelectorFactory(bool multipleSelect = false);
	}
}
