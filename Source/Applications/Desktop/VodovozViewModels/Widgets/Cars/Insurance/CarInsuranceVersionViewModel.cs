using QS.Commands;
using QS.DomainModel.Entity;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.Insurance
{
	public class CarInsuranceVersionViewModel : EntityWidgetViewModelBase<Car>
	{
		private CarInsuranceType? _insuranceType;
		private CarInsurance _selectedCarInsurance;
		private bool _isInsuranceNotRelevantForCar;

		public CarInsuranceVersionViewModel(
			Car entity,
			ICommonServices commonServices)
			: base(entity, commonServices)
		{
			AddCarInsuranceCommand = new DelegateCommand(AddCarInsurance, () => CanAddCarInsurance);
			EditCarInsuranceCommand = new DelegateCommand(EditCarInsurance, () => CanEditCarInsurance);
		}

		public event EventHandler<AddCarInsuranceEventArgs> AddCarInsurenceClicked;
		public event EventHandler<EditCarInsuranceEventArgs> EditCarInsurenceClicked;

		public DelegateCommand AddCarInsuranceCommand { get; }
		public DelegateCommand EditCarInsuranceCommand { get; }

		[PropertyChangedAlso(nameof(CanAddCarInsurance))]
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

		public bool IsInsuranceNotRelevantForCar
		{
			get => _isInsuranceNotRelevantForCar;
			set => SetField(ref _isInsuranceNotRelevantForCar, value);
		}

		public IList<CarInsurance> Insurances =>
			Entity.CarInsurances.Where(o => !InsuranceType.HasValue || o.InsuranceType == InsuranceType.Value)
			.ToList();

		public bool CanSetInsuranceNotRelevantForCar =>
			PermissionResult.CanUpdate
			&& InsuranceType.HasValue
			&& InsuranceType.Value == CarInsuranceType.Kasko;

		public bool CanAddCarInsurance => PermissionResult.CanUpdate && InsuranceType.HasValue;
		public bool CanEditCarInsurance => PermissionResult.CanUpdate && SelectedCarInsurance != null;

		private void AddCarInsurance()
		{
			if(!InsuranceType.HasValue)
			{
				return;
			}

			AddCarInsurenceClicked?.Invoke(this, new AddCarInsuranceEventArgs(InsuranceType.Value));
		}

		private void EditCarInsurance()
		{
			if(SelectedCarInsurance is null)
			{
				return;
			}

			EditCarInsurenceClicked?.Invoke(this, new EditCarInsuranceEventArgs(SelectedCarInsurance));
		}
	}
}
