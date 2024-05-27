using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Services.Cars.Insurance
{
	public interface ICarInsuranceVersionService
	{
		event EventHandler CarInsuranceAdded;
		event EventHandler<EditCarInsuranceEventArgs> EditCarInsurenceSelected;
		bool IsInsuranceEditingInProgress { get; }
		void AddNewCarInsurance(CarInsuranceType insuranceType);
		void EditCarInsurance(CarInsurance insurance);
		void InsuranceEditingCompleted(CarInsurance insurance);
		void InsuranceEditingCancelled();
	}
}
