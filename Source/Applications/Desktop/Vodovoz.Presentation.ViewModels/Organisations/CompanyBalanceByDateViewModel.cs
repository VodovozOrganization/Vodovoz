using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace Vodovoz.Presentation.ViewModels.Organisations
{
	public class CompanyBalanceByDateViewModel : UowDialogViewModelBase
	{
		public CompanyBalanceByDateViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation) : base(unitOfWorkFactory, navigation)
		{

		}
	}
}
