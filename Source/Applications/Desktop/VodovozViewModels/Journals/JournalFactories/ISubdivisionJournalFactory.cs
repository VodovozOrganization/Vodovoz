using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface ISubdivisionJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory = null);
		
		IEntityAutocompleteSelectorFactory CreateDefaultSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory = null);

		IEntityAutocompleteSelectorFactory CreateLogisticSubdivisionAutocompleteSelectorFactory(
			IEntityAutocompleteSelectorFactory employeeSelectorFactory = null);

		IEntityAutocompleteSelectorFactory CreateCashSubdivisionAutocompleteSelectorFactory();
	}
}
