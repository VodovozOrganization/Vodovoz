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
	public class CarInsuranceVersionViewModel : EntityWidgetViewModelBase<Car>
	{
		private IInteractiveService _interactiveService;
		private readonly ICarInsuranceVersionService _carInsuranceVersionService;
		private CarInsuranceType? _insuranceType;
		private CarInsurance _selectedCarInsurance;
		private bool _isInsuranceNotRelevantForCar;

		public CarInsuranceVersionViewModel(
			Car entity,
			ICommonServices commonServices,
			ICarInsuranceVersionService carInsuranceVersionService)
			: base(entity, commonServices)
		{
			AddCarInsuranceCommand = new DelegateCommand(AddCarInsurance, () => CanAddCarInsurance);
			EditCarInsuranceCommand = new DelegateCommand(EditCarInsurance, () => CanEditCarInsurance);
			_carInsuranceVersionService = carInsuranceVersionService ?? throw new System.ArgumentNullException(nameof(carInsuranceVersionService));

			_interactiveService = CommonServices.InteractiveService;

			IsInsuranceNotRelevantForCar = Entity.IsKaskoInsuranceNotRelevant;

			_carInsuranceVersionService.CarInsuranceAdded += OnCarInsuranceAdded;
			_carInsuranceVersionService.IsKaskoInsuranceNotRelevantChanged += OnIsKaskoInsuranceNotRelevantChanged;
		}

		public DelegateCommand AddCarInsuranceCommand { get; }
		public DelegateCommand EditCarInsuranceCommand { get; }
		public DelegateCommand SetIsKaskoInsuranceNotRelevantCommand { get; }

		[PropertyChangedAlso(nameof(CanAddCarInsurance), nameof(IsInsurancesSensitive))]
		public CarInsuranceType? InsuranceType
		{
			get => _insuranceType;
			set
			{
				if(_insuranceType != null)
				{
					return;
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
			set
			{
				if(!SetField(ref _isInsuranceNotRelevantForCar, value))
				{
					return;
				}

				SetIsKaskoInsuranceNotRelevant();
			}
		}

		public IList<CarInsurance> Insurances =>
			Entity.CarInsurances
			.Where(o => !InsuranceType.HasValue || o.InsuranceType == InsuranceType.Value)
			.OrderByDescending(o => o.EndDate)
			.ToList();

		public bool CanSetInsuranceNotRelevantForCar =>
			PermissionResult.CanUpdate
			&& InsuranceType.HasValue
			&& InsuranceType.Value == CarInsuranceType.Kasko;

		public bool IsInsurancesSensitive =>
			InsuranceType.HasValue
			&& (InsuranceType != CarInsuranceType.Kasko || !IsInsuranceNotRelevantForCar);


		public bool CanAddCarInsurance => PermissionResult.CanUpdate && InsuranceType.HasValue;
		public bool CanEditCarInsurance => PermissionResult.CanUpdate && SelectedCarInsurance != null;

		private void AddCarInsurance()
		{
			if(!InsuranceType.HasValue
				|| IsInsuranceEditingInProgress()
				|| IsNewInsuranceAlreadyAdded())
			{
				return;
			}

			_carInsuranceVersionService.AddNewCarInsurance(InsuranceType.Value);
		}

		private void EditCarInsurance()
		{
			if(SelectedCarInsurance is null || IsInsuranceEditingInProgress())
			{
				return;
			}

			_carInsuranceVersionService.EditCarInsurance(SelectedCarInsurance);
		}

		private void SetIsKaskoInsuranceNotRelevant()
		{
			_carInsuranceVersionService.SetIsKaskoInsuranceNotRelevant(IsInsuranceNotRelevantForCar);
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
			if(_carInsuranceVersionService.IsInsuranceEditingInProgress)
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
			if(IsInsuranceNotRelevantForCar != Entity.IsKaskoInsuranceNotRelevant)
			{
				IsInsuranceNotRelevantForCar = Entity.IsKaskoInsuranceNotRelevant;
			}
		}
	}
}
