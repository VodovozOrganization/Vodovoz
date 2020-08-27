using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;

namespace Vodovoz.ViewModels.Mango
{
	public class AdditionalCallViewModel : ModalDialogViewModelBase
	{
		private IUnitOfWork UoW;
		public AdditionalCallViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation)
			 : base(navigation)
		{

		}
	}
}
