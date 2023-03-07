using Vodovoz.Domain.Roboats;
using Vodovoz.ViewModels.Dialogs.Roboats;

namespace Vodovoz.Factories
{
	public interface IRoboatsViewModelFactory
	{
		RoboatsEntityViewModel CreateViewModel(IRoboatsEntity entity);
	}
}