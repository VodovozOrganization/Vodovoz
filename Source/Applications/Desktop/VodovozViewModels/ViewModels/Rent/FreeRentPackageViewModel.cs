using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.RentPackages;

namespace Vodovoz.ViewModels.ViewModels.Rent
{
	public class FreeRentPackageViewModel : EntityTabViewModelBase<FreeRentPackage>
	{
		private readonly IRentPackageRepository _rentPackageRepository;

		public FreeRentPackageViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IRentPackageRepository rentPackageRepository)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_rentPackageRepository = rentPackageRepository ?? throw new ArgumentNullException(nameof(rentPackageRepository));
			
			ConfigureValidateContext();
		}

		private void ConfigureValidateContext()
		{
			ValidationContext.ServiceContainer.AddService(typeof(IRentPackageRepository), _rentPackageRepository);
		}
	}
}
