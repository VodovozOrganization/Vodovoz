using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services.Cars.Insurance;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class CarInsuranceVersionViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;

		private Car _car;
		private ICarInsuranceVersionService _carInsuranceVersionService;
		private CarInsuranceType? _insuranceType;
		private CarInsurance _selectedCarInsurance;
		private bool _isInsuranceNotRelevantForCar;

		public CarInsuranceVersionViewModel(ICommonServices commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = commonServices.InteractiveService;

			AddCarInsuranceCommand = new DelegateCommand(AddCarInsurance, () => CanAddCarInsurance);
			EditCarInsuranceCommand = new DelegateCommand(EditCarInsurance, () => CanEditCarInsurance);
		}

		public DelegateCommand AddCarInsuranceCommand { get; }
		public DelegateCommand EditCarInsuranceCommand { get; }
		public DelegateCommand SetIsKaskoInsuranceNotRelevantCommand { get; }

		public Car Car
		{
			get => _car;
			private set
			{
				if(_car != null)
				{
					throw new InvalidOperationException($"Свойство {nameof(Car)} уже установлено");
				}

				SetField(ref _car, value);
			}
		}

		public ICarInsuranceVersionService CarInsuranceVersionService
		{
			get => _carInsuranceVersionService;
			private set
			{
				if(_carInsuranceVersionService != null)
				{
					throw new InvalidOperationException($"Свойство {nameof(CarInsuranceVersionService)} уже установлено");
				}

				SetField(ref _carInsuranceVersionService, value);

				_carInsuranceVersionService.CarInsuranceAdded += OnCarInsuranceAdded;
				_carInsuranceVersionService.IsKaskoInsuranceNotRelevantChanged += OnIsKaskoInsuranceNotRelevantChanged;
			}
		}

		[PropertyChangedAlso(nameof(CanAddCarInsurance), nameof(IsInsurancesSensitive))]
		public CarInsuranceType? InsuranceType
		{
			get => _insuranceType;
			private set
			{
				if(_insuranceType != null)
				{
					throw new InvalidOperationException($"Свойство {nameof(InsuranceType)} уже установлено");
				}

				SetField(ref _insuranceType, value);
			}
		}

		[PropertyChangedAlso(nameof(CanEditCarInsurance))]
		public CarInsurance SelectedCarInsurance
		{
			get => _selectedCarInsurance;
			set => SetField(ref _selectedCarInsurance, value);
		}

		[PropertyChangedAlso(nameof(IsInsurancesSensitive))]
		public bool IsInsuranceNotRelevantForCar
		{
			get => _isInsuranceNotRelevantForCar;
			private set
			{
				if(!SetField(ref _isInsuranceNotRelevantForCar, value))
				{
					return;
				}

				SetIsKaskoInsuranceNotRelevant();
			}
		}

		public IList<CarInsurance> Insurances =>
			Car.CarInsurances
			.Where(o => !InsuranceType.HasValue || o.InsuranceType == InsuranceType.Value)
			.OrderByDescending(o => o.EndDate)
			.ToList();

		public bool IsUserCanEditCarEntity =>
			_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

		public bool CanSetInsuranceNotRelevantForCar =>
			IsUserCanEditCarEntity
			&& Car != null
			&& CarInsuranceVersionService != null
			&& InsuranceType.HasValue
			&& InsuranceType.Value == CarInsuranceType.Kasko;

		public bool IsInsurancesSensitive =>
			Car != null
			&& CarInsuranceVersionService != null
			&& InsuranceType.HasValue
			&& (InsuranceType != CarInsuranceType.Kasko || !IsInsuranceNotRelevantForCar);


		public bool CanAddCarInsurance => IsUserCanEditCarEntity && InsuranceType.HasValue;
		public bool CanEditCarInsurance => IsUserCanEditCarEntity && SelectedCarInsurance != null;

		public void Initialize(
			ICarInsuranceVersionService carInsuranceVersionService,
			Car car,
			CarInsuranceType insuranceType,
			bool isInsuranceNotRelevantForCar)
		{
			if(carInsuranceVersionService is null)
			{
				throw new ArgumentNullException(nameof(carInsuranceVersionService));
			}

			if(car is null)
			{
				throw new ArgumentNullException(nameof(car));
			}

			CarInsuranceVersionService = carInsuranceVersionService;
			Car = car;
			InsuranceType = insuranceType;
			IsInsuranceNotRelevantForCar = isInsuranceNotRelevantForCar;

		}

		private void AddCarInsurance()
		{
			if(!InsuranceType.HasValue
				|| IsInsuranceEditingInProgress()
				|| IsNewInsuranceAlreadyAdded())
			{
				return;
			}

			CarInsuranceVersionService.AddNewCarInsurance(InsuranceType.Value);
		}

		private void EditCarInsurance()
		{
			if(SelectedCarInsurance is null || IsInsuranceEditingInProgress())
			{
				return;
			}

			CarInsuranceVersionService.EditCarInsurance(SelectedCarInsurance);
		}

		private void SetIsKaskoInsuranceNotRelevant()
		{
			CarInsuranceVersionService.SetIsKaskoInsuranceNotRelevant(IsInsuranceNotRelevantForCar);
		}

		private bool IsNewInsuranceAlreadyAdded()
		{
			var isNewInsuranceAdded = Insurances.Count(item => item.Id == 0) > 0;

			if(isNewInsuranceAdded)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					 "Новая страховка уже была добавлена. Сначала сохраните существующие изменения.");

				return true;
			}

			return false;
		}

		private bool IsInsuranceEditingInProgress()
		{
			if(CarInsuranceVersionService.IsInsuranceEditingInProgress)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					 "В данный момент уже выполняется редактирование страховки.\nСохраните редактируемую страховку или отмените редактирование");

				return true;
			}

			return false;
		}

		private void OnCarInsuranceAdded(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(Insurances));
		}

		private void OnIsKaskoInsuranceNotRelevantChanged(object sender, EventArgs e)
		{
			if(IsInsuranceNotRelevantForCar != Car.IsKaskoInsuranceNotRelevant)
			{
				IsInsuranceNotRelevantForCar = Car.IsKaskoInsuranceNotRelevant;
			}
		}

		public void Dispose()
		{
			_carInsuranceVersionService.CarInsuranceAdded -= OnCarInsuranceAdded;
			_carInsuranceVersionService.IsKaskoInsuranceNotRelevantChanged -= OnIsKaskoInsuranceNotRelevantChanged;
		}
	}
}
