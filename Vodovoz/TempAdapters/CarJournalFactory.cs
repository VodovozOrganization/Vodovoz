using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.TempAdapters
{
    public class CarJournalFactory : ICarJournalFactory
    {
        public IEntityAutocompleteSelectorFactory CreateCarAutocompleteSelectorFactory()
        {
            return new DefaultEntityAutocompleteSelectorFactory<Car, CarJournalViewModel, CarJournalFilterViewModel>(ServicesConfig.CommonServices);
        }
    }
}