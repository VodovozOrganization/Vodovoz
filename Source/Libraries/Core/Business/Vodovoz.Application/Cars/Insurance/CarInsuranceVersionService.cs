using System;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services.Cars.Insurance;

namespace Vodovoz.Application.Cars.Insurance
{
	public class CarInsuranceVersionService : ICarInsuranceVersionService
	{
		private readonly Car _car;
		private bool _isInsuranceEditingInProgress;

		public CarInsuranceVersionService(Car car)
		{
			_car = car ?? throw new System.ArgumentNullException(nameof(car));
		}

		public event EventHandler CarInsuranceAdded;
		public event EventHandler<EditCarInsuranceEventArgs> EditCarInsurenceSelected;
		public event EventHandler IsKaskoInsuranceNotRelevantChanged;
		public bool IsInsuranceEditingInProgress => _isInsuranceEditingInProgress;

		public void AddNewCarInsurance(CarInsuranceType insuranceType)
		{
			var insurance = new CarInsurance
			{
				Car = _car,
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
			if(_car.IsKaskoInsuranceNotRelevant != isKaskoInsuranceNotRelevant)
			{
				_car.IsKaskoInsuranceNotRelevant = isKaskoInsuranceNotRelevant;
				IsKaskoInsuranceNotRelevantChanged?.Invoke(this, null);
			}
		}

		public void InsuranceEditingCompleted(CarInsurance insurance)
		{
			if(insurance.Id == 0)
			{
				_car.CarInsurances.Add(insurance);
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
