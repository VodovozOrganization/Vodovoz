using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using VodovozBusiness.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class GroupGtinViewModel : EntityTabViewModelBase<GroupGtin>
	{
		public GroupGtinViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
		}
	}
}
