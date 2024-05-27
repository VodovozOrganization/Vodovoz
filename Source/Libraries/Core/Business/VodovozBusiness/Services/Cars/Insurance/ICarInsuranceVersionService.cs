using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Services.Cars.Insurance
{
	public interface ICarInsuranceVersionService
	{
		void AddNewCarInsurance(CarInsuranceType insuranceType);
		void EditCarInsurance(CarInsurance insurance);
		void InsuranceEditingCompleted(CarInsurance insurance);
		void ResetIsInsuranceEditingInProgress();
		void SetIsInsuranceEditingInProgress();
	}
}
