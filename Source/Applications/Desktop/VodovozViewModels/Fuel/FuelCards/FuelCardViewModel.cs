using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.ViewModels.Fuel.FuelCards
{
	public class FuelCardViewModel : EntityTabViewModelBase<FuelCard>
	{
		private readonly ILogger<FuelCardViewModel> _logger;

		public FuelCardViewModel(
			ILogger<FuelCardViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
		}
	}
}
