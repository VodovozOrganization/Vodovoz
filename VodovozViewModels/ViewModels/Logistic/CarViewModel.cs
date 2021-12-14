using QS.Attachments.ViewModels.Widgets;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Factories;
using Vodovoz.Services;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarViewModel : EntityTabViewModelBase<Car>
	{
		private const string _canChangeCarsVolumeWeightConsumptionPermissionName = "can_change_cars_volume_weight_consumption";
		private bool _haveChangeCarsVolumeWeightConsumptionPermissionGranted;

		private const string _canChangeBottlesFromAddressPermissionName = "can_change_cars_bottles_from_address";
		private bool _canChangeBottlesFromAddress;

		private const string _canChangeCarIsRaskatPermissionName = "can_change_car_is_raskat";
		private bool _haveChangeCarIsRaskatPermissionGranted;

		private readonly ICarRepository _carRepository;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private AttachmentsViewModel _attachmentsViewModel;
		private string _driverInfoText;
		private int _currentUserId;

		private IEnumerable<DriverCarKind> _driverCarKinds;

		public CarViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			IAttachmentsViewModelFactory attachmentsViewModelFactory,
			ICarRepository carRepository,
			IGeographicGroupParametersProvider geographicGroupParametersProvider)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Автомобиль";
			_employeeJournalFactory = employeeJournalFactory;
			AttachmentsViewModel = attachmentsViewModelFactory.CreateNewAttachmentsViewModel(Entity.ObservableAttachments);
			_carRepository = carRepository;

			DriverCarKinds = UoW.GetAll<DriverCarKind>();

			_currentUserId = commonServices.UserService.CurrentUserId;
			_haveChangeCarsVolumeWeightConsumptionPermissionGranted =
				commonServices.PermissionService.ValidateUserPresetPermission(_canChangeCarsVolumeWeightConsumptionPermissionName, _currentUserId);

			CanChangeBottlesFromAddress = commonServices.PermissionService.ValidateUserPresetPermission(_canChangeBottlesFromAddressPermissionName, _currentUserId);
			_haveChangeCarIsRaskatPermissionGranted = commonServices.CurrentPermissionService.ValidatePresetPermission(_canChangeCarIsRaskatPermissionName);
			EastGeographicGroupId =
				(geographicGroupParametersProvider ?? throw new ArgumentNullException(nameof(geographicGroupParametersProvider)))
				.EastGeographicGroupId;
		}

		public string DriverInfoText
		{
			get => _driverInfoText;
			set => SetField(ref _driverInfoText, value);
		}

		public bool CanChangeBottlesFromAddress
		{
			get => _canChangeBottlesFromAddress;
			set => SetField(ref _canChangeBottlesFromAddress, value);
		}

		public AttachmentsViewModel AttachmentsViewModel
		{
			get => _attachmentsViewModel;
			set => SetField(ref _attachmentsViewModel, value);
		}

		public IEnumerable<DriverCarKind> DriverCarKinds
		{
			get => _driverCarKinds;
			set => SetField(ref _driverCarKinds, value);
		}
		
		public int EastGeographicGroupId { get; }

		public IEmployeeJournalFactory EmployeeJournalFactory => _employeeJournalFactory;

		public bool IsEntityNew => Entity.Id == 0;

		public bool CanChangeVolumeWeightConsumption => _haveChangeCarsVolumeWeightConsumptionPermissionGranted
			|| IsEntityNew
			|| !(Entity.IsCompanyCar || Entity.IsRaskat);

		public bool CanChangeIsRaskat => IsEntityNew || _haveChangeCarIsRaskatPermissionGranted;

		public bool IsRaskatChangeValid => IsEntityNew || !_carRepository.IsInAnyRouteList(UoW, Entity);

		public bool CanChangeRaskatType => IsEntityNew;

		public bool CanChangeCarType => IsEntityNew;

		public bool CanChangeTypeOfUse => IsEntityNew;

		public bool CanChangeDriverCarKind => Entity.TypeOfUse.HasValue && !Entity.IsCompanyCar;

		public string CarPhotoFilename => $"{Entity.Model}({Entity.RegistrationNumber})";

		public void OnEntryDriverChanged(object sender, EventArgs e)
		{
			if(Entity.Driver != null)
			{
				var docs = Entity.Driver.GetMainDocuments();
				if(docs.Any())
				{
					DriverInfoText = $"\tПаспорт: {docs.First().PassportSeria} № {docs.First().PassportNumber}\n" +
						$"\tАдрес регистрации: {Entity.Driver.AddressRegistration}";
				}
				else
				{
					DriverInfoText = "Главный документ отсутствует";
				}
			}
		}

		public void OnIsRaskatToggled(object sender, EventArgs e)
		{
			if(Entity.Id != 0 && _carRepository.IsInAnyRouteList(UoW, Entity))
			{
				Entity.IsRaskat = !Entity.IsRaskat;
				CommonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Warning, "На данном автомобиле есть МЛ, смена типа невозможна");
			}
		}

		public void OnTypeOfUseChangedByUser(object sender, EventArgs e)
		{
			OnPropertyChanged(() => CanChangeDriverCarKind);

			if(Entity.IsCompanyCar)
			{
				Entity.Driver = null;
				Entity.DriverCarKind = null;
			}

			if(IsEntityNew)
			{
				Entity.IsRaskat = false;
			}
		}
	}
}
