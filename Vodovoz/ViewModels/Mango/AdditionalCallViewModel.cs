using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace Vodovoz.ViewModels.Mango
{
	public class AdditionalCallViewModel : WindowDialogViewModelBase
	{
		private IUnitOfWork UoW;
		public AdditionalCallViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			INavigationManager navigation)
			 : base(navigation)
		{
			WindowPosition = QS.Dialog.WindowGravity.RightBottom;
			IsModal = false;
		}
	}
}
