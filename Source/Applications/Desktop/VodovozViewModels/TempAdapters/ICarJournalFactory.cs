using Autofac;
using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface ICarJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateCarAutocompleteSelectorFactory(bool multipleSelect = false);
		IEntityAutocompleteSelectorFactory CreateCarAutocompleteSelectorFactoryForCarsExploitationReport(
			ILifetimeScope scope, bool multipleSelect = false);
	}
}
