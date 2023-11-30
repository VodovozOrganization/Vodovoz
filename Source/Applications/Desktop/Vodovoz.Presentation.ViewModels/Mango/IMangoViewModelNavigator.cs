using QS.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vodovoz.Presentation.ViewModels.Mango
{
	public interface IMangoViewModelNavigator
	{
		IPage OpenCounterpartyTalkViewModel();
		IPage OpenUnknowTalkViewModel();
	}
}
