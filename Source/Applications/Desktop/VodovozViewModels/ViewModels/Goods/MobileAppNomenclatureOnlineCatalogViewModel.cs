using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class MobileAppNomenclatureOnlineCatalogViewModel : EntityTabViewModelBase<MobileAppNomenclatureOnlineCatalog>
	{
		public MobileAppNomenclatureOnlineCatalogViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager) : base(uowBuilder, uowFactory, commonServices, navigationManager)
		{

		}
	}
}
