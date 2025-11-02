using QS.Commands;
using QS.DomainModel.Entity;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class CarInsuranceVersionViewModel : WidgetViewModelBase
	{
		private Car _car;
		private CarInsuranceType? _insuranceType;
		private CarInsurance _selectedCarInsurance;
		private bool _isInsuranceNotRelevantForCar;
		private IList<CarInsurance> _insurances = new List<CarInsurance>();

		public CarInsuranceVersionViewModel()
		{
			AddCarInsuranceCommand = new DelegateCommand(OnAddCarInsuranceClicked, () => CanAddCarInsurance);
			EditCarInsuranceCommand = new DelegateCommand(OnEditCarInsuranceClicked, () => CanEditCarInsurance);
			ChangeIsKaskoNotRelevantCommand = new DelegateCommand(OnChangeIsKaskoNotRelevantClicked, () => CanChangeInsuranceNotRelevantForCar);
		}

		public event EventHandler<AddCarInsuranceEventArgs> AddCarInsuranceClicked;
		public event EventHandler<EditCarInsuranceEventArgs> EditCarInsurenceClicked;
		public event EventHandler<ChangeIsKaskoNotRelevantClickedEventArgs> ChangeIsKaskoNotRelevantClicked;

		public DelegateCommand AddCarInsuranceCommand { get; }
		public DelegateCommand EditCarInsuranceCommand { get; }
		public DelegateCommand ChangeIsKaskoNotRelevantCommand { get; }

		[PropertyChangedAlso(
			nameof(CanAddCarInsurance),
			nameof(CanChangeInsuranceNotRelevantForCar),
			nameof(IsInsurancesSensitive))]
		public CarInsuranceType? InsuranceType
		{
			get => _insuranceType;
			set
			{
				if(!(_insuranceType is null))
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
			set => SetField(ref _isInsuranceNotRelevantForCar, value);
		}

		public IList<CarInsurance> Insurances
		{
			get => _insurances;
			set => SetField(ref _insurances, value);
		}

		public bool CanAddCarInsurance =>
			InsuranceType.HasValue;

		public bool CanEditCarInsurance =>
			!(SelectedCarInsurance is null);

		public bool CanChangeInsuranceNotRelevantForCar =>
			InsuranceType.HasValue
			&& InsuranceType.Value == CarInsuranceType.Kasko;

		public bool IsInsurancesSensitive =>
			InsuranceType.HasValue
			&& (InsuranceType != CarInsuranceType.Kasko || !IsInsuranceNotRelevantForCar);

		private void OnAddCarInsuranceClicked()
		{
			if(!CanAddCarInsurance)
			{
				return;
			}

			AddCarInsuranceClicked?.Invoke(this, new AddCarInsuranceEventArgs(InsuranceType.Value));
		}

		private void OnEditCarInsuranceClicked()
		{
			if(!CanEditCarInsurance)
			{
				return;
			}

			EditCarInsurenceClicked?.Invoke(this, new EditCarInsuranceEventArgs(SelectedCarInsurance));
		}

		private void OnChangeIsKaskoNotRelevantClicked()
		{
			if(!CanChangeInsuranceNotRelevantForCar)
			{
				return;
			}

			ChangeIsKaskoNotRelevantClicked?.Invoke(this, new ChangeIsKaskoNotRelevantClickedEventArgs(IsInsuranceNotRelevantForCar));
		}
	}
}
