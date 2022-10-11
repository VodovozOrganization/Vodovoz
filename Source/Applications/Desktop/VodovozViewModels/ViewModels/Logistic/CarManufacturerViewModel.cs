using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarManufacturerViewModel : EntityTabViewModelBase<CarManufacturer>, IAskSaveOnCloseViewModel
	{
		public CarManufacturerViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{ }

		public bool CanEdit => PermissionResult.CanUpdate || Entity.Id == 0 && PermissionResult.CanCreate;
		public bool AskSaveOnClose => CanEdit;
	}
}
