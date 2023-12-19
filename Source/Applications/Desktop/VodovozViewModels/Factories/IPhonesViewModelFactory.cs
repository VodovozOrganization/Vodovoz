using Autofac;
using QS.DomainModel.UoW;
using Vodovoz.ViewModels.ViewModels.Contacts;

namespace Vodovoz.Factories
{
	public interface IPhonesViewModelFactory
	{
		PhonesViewModel CreateNewPhonesViewModel(ILifetimeScope lifetimeScope, IUnitOfWork uow);
	}
}
