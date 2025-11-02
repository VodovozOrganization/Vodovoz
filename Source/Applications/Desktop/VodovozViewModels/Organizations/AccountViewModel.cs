using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;

namespace Vodovoz.ViewModels.Organizations
{
	public class AccountViewModel : EntityTabViewModelBase<Account>
	{
		public AccountViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
		}
	}
}
