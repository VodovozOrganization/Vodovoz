using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using QS.Dialog;
using System;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class MobileAppNomenclatureOnlineCatalogViewModel : NomenclatureOnlineCatalogViewModel
	{
		public MobileAppNomenclatureOnlineCatalogViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager)
			: base(uowBuilder, typeof(MobileAppNomenclatureOnlineCatalog), uowFactory, commonServices.InteractiveService, navigationManager)
		{

		}
	}
}
