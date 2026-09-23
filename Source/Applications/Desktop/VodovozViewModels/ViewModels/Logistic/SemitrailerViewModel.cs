using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Widgets.Cars.CarVersions;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class SemitrailerViewModel : EntityTabViewModelBase<Car>, IAskSaveOnCloseViewModel
	{
		private readonly CarVersionsManagementViewModel _carVersionsManagementViewModel;
		private readonly IServiceProvider _serviceProvider;
		private readonly ICarRepository _carRepository;

		public SemitrailerViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ViewModelEEVMBuilder<CarModel> carModelEEVMBuilder,
			CarVersionsManagementViewModel carVersionsManagementViewModel,
			IAdditionalFuelTypeManagementViewModelFactory additionalFuelTypeManagementViewModelFactory,
			IServiceProvider serviceProvider,
			ICarRepository carRepository)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager == null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			if(additionalFuelTypeManagementViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(additionalFuelTypeManagementViewModelFactory));
			}

			_carVersionsManagementViewModel = carVersionsManagementViewModel ?? throw new ArgumentNullException(nameof(carVersionsManagementViewModel));

			TabName = Entity.Id == 0 ? $"Новый полуприцеп" : $"Полуприцеп {Entity.VIN}";

			_carVersionsManagementViewModel.Initialize(Entity, this);
			CarVersionsViewModel = _carVersionsManagementViewModel.CarVersionsViewModel;
			CarVersionEditingViewModel = _carVersionsManagementViewModel.CarVersionEditingViewModel;

			SetPermissions();
			_carVersionsManagementViewModel.CanEditCarCard = CanEditCarCard;

			CarModelViewModel = carModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.CarModel)
				.UseViewModelJournalAndAutocompleter<CarModelJournalViewModel, CarModelJournalFilterViewModel>(filter =>
				{
					filter.ExcludedCarTypesOfUse = Enum.GetValues(typeof(CarTypeOfUse))
						.Cast<CarTypeOfUse>()
						.Where(t => t != CarTypeOfUse.Semitrailer)
						.ToArray();
				})
				.UseViewModelDialog<CarModelViewModel>()
				.Finish();

			SaveCommand = new DelegateCommand(SaveAndClose);
			CloseCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
		}

		public bool CanEdit { get; private set; }
		public bool CanEditCarCard { get; private set; }
		
		public bool AskSaveOnClose { get; private set; }

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseCommand { get; }
		
		public bool IsArchive
		{
			get => Entity.IsArchive;
			set
			{
				var oldValue = Entity.IsArchive;
				
				if(!CanChangeCompositionCompanyTransportPark)
				{
					const string message = "Невозможно изменить архивацию авто. У Вас нет права менять состав автопарка компании";
					
					if(oldValue != value)
					{
						var activeVersion = Entity.GetActiveCarVersionOnDate();

						if(activeVersion != null && (activeVersion.IsCompanyCar || activeVersion.IsRaskat))
						{
							CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, message);
							OnPropertyChanged();
							return;
						}
					}
				}
				
				Entity.IsArchive = value;
			}
		}

		public bool CanChangeCarModel { get; private set; }

		public IEntityEntryViewModel CarModelViewModel { get; }

		public CarVersionsViewModel CarVersionsViewModel { get; }
		public CarVersionEditingViewModel CarVersionEditingViewModel { get; }
		
		private bool CanChangeCompositionCompanyTransportPark { get; set; }

		protected override bool BeforeValidation()
		{
			ValidationContext = new ValidationContext(Entity, _serviceProvider, new Dictionary<object, object>
			{
				{ nameof(ICarRepository), _carRepository }
			});

			return base.BeforeValidation();
		}

		protected override bool BeforeSave()
		{

			var result = base.BeforeSave();

			UpdateArchivingDate();

			return result;
		}

		private void SetPermissions()
		{
			var canEditCarCardPermission = CommonServices.CurrentPermissionService.ValidatePresetPermission(LogisticPermissions.Car.CanEditCarCard);
			CanEdit = (Entity.Id == 0 && PermissionResult.CanCreate)
				|| (Entity.Id != 0 && (PermissionResult.CanUpdate || canEditCarCardPermission));
			CanEditCarCard = CanEdit && (Entity.Id == 0 || canEditCarCardPermission);
			AskSaveOnClose = CanEdit;
			
			CanChangeCarModel =
				Entity.Id == 0
				|| CanEditCarCard
				|| CommonServices.CurrentPermissionService.ValidatePresetPermission(LogisticPermissions.Car.CanChangeCarModel);
			
			CanChangeCompositionCompanyTransportPark =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(CarPermissions.CanChangeCompositionCompanyTransportPark);
		}

		private void UpdateArchivingDate()
		{
			if(IsArchive && Entity.ArchivingDate == null)
			{
				Entity.ArchivingDate = DateTime.Now;
			}

			if(!IsArchive && Entity.ArchivingDate != null)
			{
				Entity.ArchivingDate = null;
			}
		}
	}
}
