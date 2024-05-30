using System;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services.Cars.Insurance;

namespace Vodovoz.Application.Cars.Insurance
{
	public class CarInsuranceVersionService : ICarInsuranceVersionService
	{
		private bool _isInsuranceEditingInProgress;
		private Car _car;

		public event EventHandler CarInsuranceAdded;
		public event EventHandler<EditCarInsuranceEventArgs> EditCarInsurenceSelected;
		public event EventHandler IsKaskoInsuranceNotRelevantChanged;

		public Car Car {
			get => _car;
			set
			{
				if(_car is null)
				{
					_car = value;
					return;
				}

				throw new InvalidOperationException($"Свойство {nameof(Car)} уже установлено");
			}
		}

		public bool IsInsuranceEditingInProgress => _isInsuranceEditingInProgress;

		public void AddNewCarInsurance(CarInsuranceType insuranceType)
		{
			var insurance = new CarInsurance
			{
				Car = Car,
				InsuranceType = insuranceType
			};

			EditCarInsurance(insurance);
		}

		public void EditCarInsurance(CarInsurance insurance)
		{
			if(_isInsuranceEditingInProgress)
			{
				return;
			}

			SetIsInsuranceEditingInProgress();
			EditCarInsurenceSelected?.Invoke(this, new EditCarInsuranceEventArgs(insurance));
		}

		public void SetIsKaskoInsuranceNotRelevant(bool isKaskoInsuranceNotRelevant)
		{
			if(Car.IsKaskoInsuranceNotRelevant != isKaskoInsuranceNotRelevant)
			{
				Car.IsKaskoInsuranceNotRelevant = isKaskoInsuranceNotRelevant;
				IsKaskoInsuranceNotRelevantChanged?.Invoke(this, null);
			}
		}

		public void InsuranceEditingCompleted(CarInsurance insurance)
		{
			if(insurance.Id == 0)
			{
				Car.CarInsurances.Add(insurance);
				CarInsuranceAdded?.Invoke(this, null);
			}

			ResetIsInsuranceEditingInProgress();
		}

		public void InsuranceEditingCancelled()
		{
			ResetIsInsuranceEditingInProgress();
		}

		private void SetIsInsuranceEditingInProgress()
		{
			_isInsuranceEditingInProgress = true;
		}

		private void ResetIsInsuranceEditingInProgress()
		{
			_isInsuranceEditingInProgress = false;
		}
	}
}
