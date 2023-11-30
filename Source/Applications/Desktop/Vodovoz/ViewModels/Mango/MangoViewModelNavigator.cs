using QS.Navigation;
using System;
using Vodovoz.Presentation.ViewModels.Mango;
using Vodovoz.ViewModels.Dialogs.Mango.Talks;

namespace Vodovoz.ViewModels.Mango
{
	public class MangoViewModelNavigator : IMangoViewModelNavigator
	{
		private readonly INavigationManager _navigationManager;

		public MangoViewModelNavigator(INavigationManager navigationManager)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}

		public IPage OpenCounterpartyTalkViewModel()
		{
			return _navigationManager.OpenViewModel<CounterpartyTalkViewModel>(null);
		}

		public IPage OpenUnknowTalkViewModel()
		{
			return _navigationManager.OpenViewModel<UnknowTalkViewModel>(null);
		}
	}
}
