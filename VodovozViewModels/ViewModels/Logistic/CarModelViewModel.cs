using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarModelViewModel : EntityTabViewModelBase<CarModel>, IAskSaveOnCloseViewModel
	{
		public CarModelViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICarManufacturerJournalFactory carManufacturerJournalFactory
		)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			CarManufacturerJournalFactory = carManufacturerJournalFactory
				?? throw new ArgumentNullException(nameof(carManufacturerJournalFactory));
		}

		public ICarManufacturerJournalFactory CarManufacturerJournalFactory { get; }
		public bool CanEdit => PermissionResult.CanUpdate || PermissionResult.CanCreate && Entity.Id == 0;
		public bool AskSaveOnClose => CanEdit;
	}
}
