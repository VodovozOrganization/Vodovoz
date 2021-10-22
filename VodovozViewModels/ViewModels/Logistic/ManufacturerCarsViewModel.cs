using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class ManufacturerCarsViewModel: EntityTabViewModelBase<ManufacturerCars>
	{
		public ManufacturerCarsViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			
		}
		
		public string ManufacturedName
		{
			get => Entity.Name;
			set
			{
				Entity.Name = value;
				OnPropertyChanged(nameof(ManufacturedName));
			}
		}

		public bool CanEdit => PermissionResult.CanUpdate;
	}
}
