using QS.DomainModel.UoW;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewModels.Factories
{
	public interface ISelectableParametersFilterViewModelFactory
	{
		SelectableParametersFilterViewModel CreateProductGroupsSelectableParametersFilterViewModel(IUnitOfWork uow, string title);
		SelectableParametersFilterViewModel CreateWarehousesSelectableParametersFilterViewModel(IUnitOfWork uow, string title);
		SelectableParametersFilterViewModel CreateCarEventTypesSelectableParametersFilterViewModel(IUnitOfWork uow, string title);
	}
}
