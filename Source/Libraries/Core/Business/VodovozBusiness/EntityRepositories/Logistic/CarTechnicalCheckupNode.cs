using System;
using Vodovoz.Domain.Logistic.Cars;

namespace VodovozBusiness.EntityRepositories.Logistic
{
	public class CarTechnicalCheckupNode
	{

		public CarTypeOfUse CarTypeOfUse { get; set; }
		public string CarRegNumber { get; set; }
		public string DriverGeography { get; set; }
		public DateTime? LastTechnicalCheckupDate { get; set; }
		public DateTime NextTechnicalCheckupDate { get; set; }
		public double DaysLeftToNextTechnicalCheckup =>
			(NextTechnicalCheckupDate - DateTime.Today).TotalDays;
	}
}
