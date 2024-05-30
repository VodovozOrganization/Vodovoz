using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Services.Cars.Insurance
{
	public interface ICarInsuranceVersionService
	{
		event EventHandler CarInsuranceAdded;
		event EventHandler<EditCarInsuranceEventArgs> EditCarInsurenceSelected;
		event EventHandler IsKaskoInsuranceNotRelevantChanged;
		Car Car { get; set; }
		bool IsInsuranceEditingInProgress { get; }
		void AddNewCarInsurance(CarInsuranceType insuranceType);
		void EditCarInsurance(CarInsurance insurance);
		void SetIsKaskoInsuranceNotRelevant(bool isKaskoInsuranceNotRelevant);
		void InsuranceEditingCompleted(CarInsurance insurance);
		void InsuranceEditingCancelled();
	}
}
