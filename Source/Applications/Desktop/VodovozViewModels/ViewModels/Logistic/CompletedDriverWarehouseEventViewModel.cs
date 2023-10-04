using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CompletedDriverWarehouseEventViewModel : EntityTabViewModelBase<CompletedDriverWarehouseEvent>
	{
		public CompletedDriverWarehouseEventViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			
		}
	}
}
