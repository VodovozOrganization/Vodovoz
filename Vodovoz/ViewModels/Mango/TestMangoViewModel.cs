using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Infrastructure.Mango;

namespace Vodovoz.ViewModels.Mango
{
	public class TestMangoViewModel : DialogViewModelBase
	{
		MangoManager manager;
		public TestMangoViewModel(
			INavigationManager navigation,
			MangoManager manager) : base(navigation) { this.manager = manager; }

		public void HangUp()
		{
			manager.HangUp();
		}


		public void GetAllVPBXEmploies()
		{
			manager.GetAllVPBXEmploies();
		}

	}
}
