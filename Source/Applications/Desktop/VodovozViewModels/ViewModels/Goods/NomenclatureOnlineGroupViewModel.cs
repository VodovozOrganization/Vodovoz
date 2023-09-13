using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class NomenclatureOnlineGroupViewModel : EntityTabViewModelBase<NomenclatureOnlineGroup>
	{
		public NomenclatureOnlineGroupViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			
		}
		
		public bool CanShowId => !UoW.IsNew;
		public string IdString => Entity.Id.ToString();
	}
}
