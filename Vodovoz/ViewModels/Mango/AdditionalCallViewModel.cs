using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;

namespace Vodovoz.ViewModels.Mango
{
	public class AdditionalCallViewModel : DialogTabViewModelBase
	{
		public AdditionalCallViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation) : base(unitOfWorkFactory, interactiveService, navigation)
		{

		}
	}
}
