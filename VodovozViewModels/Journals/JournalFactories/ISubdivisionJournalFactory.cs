using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface ISubdivisionJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateDefaultSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory);

		IEntityAutocompleteSelectorFactory CreateLogisticSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory);
	}
}
