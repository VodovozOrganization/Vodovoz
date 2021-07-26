using QS.DomainModel.UoW;
using Vodovoz.ViewModels.ViewModels;

namespace Vodovoz.Factories
{
	public interface IPhonesViewModelFactory
	{
		PhonesViewModel CreateNewPhonesViewModel(IUnitOfWork uow);
	}
}