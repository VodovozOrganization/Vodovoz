using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Core.Application.Logistics.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Dialogs.Fuel;

namespace Vodovoz.ViewModels.Widgets.Cars
{
	public class AdditionalFuelTypeManagementViewModel : EntityWidgetViewModelBase<Car>
	{
		private readonly AdditionalFuelTypeManagementService _additionalFuelTypeManagementService;

		public AdditionalFuelTypeManagementViewModel(
			Car entity,
			DialogViewModelBase parentDialog,
			ICommonServices commonServices,
			ViewModelEEVMBuilder<FuelType> fuelTypeEEVMBuilder,
			AdditionalFuelTypeManagementService additionalFuelTypeManagementService
			) : base(entity, commonServices)
		{
			if(parentDialog is null)
			{
				throw new ArgumentNullException(nameof(parentDialog));
			}

			if(fuelTypeEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(fuelTypeEEVMBuilder));
			}

			_additionalFuelTypeManagementService = additionalFuelTypeManagementService
				?? throw new ArgumentNullException(nameof(additionalFuelTypeManagementService));

			FuelTypeViewModel = fuelTypeEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(parentDialog)
				.ForProperty(Entity, x => x.FuelType)
				.UseViewModelJournalAndAutocompleter<FuelTypeJournalViewModel>()
				.UseViewModelDialog<FuelTypeViewModel>()
				.Finish(); ;

			_additionalFuelTypeManagementService.Initialize(Entity);
		}

		public IEntityEntryViewModel FuelTypeViewModel { get; }
	}
}
