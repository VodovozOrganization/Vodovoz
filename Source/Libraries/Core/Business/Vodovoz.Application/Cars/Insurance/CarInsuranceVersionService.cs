using System;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services.Cars.Insurance;
using Vodovoz.ViewModels.Widgets.Cars.Insurance;

namespace Vodovoz.Application.Cars.Insurance
{
	public class CarInsuranceVersionService : ICarInsuranceVersionService
	{
		private readonly Car _car;

		public CarInsuranceVersionService(Car car)
		{
			_car = car ?? throw new System.ArgumentNullException(nameof(car));
		}

		public event EventHandler<EditCarInsuranceEventArgs> EditCarInsurence;

		public bool IsInsuranceEditingInProgress { get; private set; }

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
			if(IsInsuranceEditingInProgress)
			{
				return;
			}

			SetIsInsuranceEditingInProgress();
			EditCarInsurence?.Invoke(null, new EditCarInsuranceEventArgs(insurance));
		}

		public void InsuranceEditingCompleted(CarInsurance insurance)
		{
			if(insurance.Id == 0)
			{
				_car.CarInsurances.Add(insurance);
			}

			ResetIsInsuranceEditingInProgress();
		}

		public void SetIsInsuranceEditingInProgress()
		{
			IsInsuranceEditingInProgress = true;
		}

		public void ResetIsInsuranceEditingInProgress()
		{
			IsInsuranceEditingInProgress = false;
		}
	}
}
