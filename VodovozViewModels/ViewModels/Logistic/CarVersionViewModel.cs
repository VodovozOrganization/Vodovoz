using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarVersionViewModel : EntityTabViewModelBase<CarVersion>
	{
		public CarVersionViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null) 
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			
		}
	}
}
