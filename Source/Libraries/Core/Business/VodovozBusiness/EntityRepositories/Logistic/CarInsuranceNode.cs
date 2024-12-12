using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class CarInsuranceNode
	{
		public CarTypeOfUse CarTypeOfUse { get; set; }
		public string CarRegNumber { get; set; }
		public string DriverGeography { get; set; }
		public CarInsuranceType InsuranceType { get; set; }
		public CarInsurance LastInsurance { get; set; }
		public bool IsKaskoNotRelevant { get; set; }

		public int DaysToExpire =>
			LastInsurance is null
			? 0
			: (int)(LastInsurance.EndDate - DateTime.Today).TotalDays;
	}
}
