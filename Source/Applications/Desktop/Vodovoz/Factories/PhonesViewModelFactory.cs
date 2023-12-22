using Autofac;
using QS.DomainModel.UoW;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.ViewModels.Contacts;

namespace Vodovoz.Factories
{
	public class PhonesViewModelFactory : IPhonesViewModelFactory
	{
		public PhonesViewModel CreateNewPhonesViewModel(ILifetimeScope lifetimeScope, IUnitOfWork uow) =>
			lifetimeScope.Resolve<PhonesViewModel>(
				new TypedParameter(typeof(IUnitOfWork), uow),
				new TypedParameter(typeof(RoboatsJournalsFactory), lifetimeScope.Resolve<RoboatsJournalsFactory>()));
	}
}
