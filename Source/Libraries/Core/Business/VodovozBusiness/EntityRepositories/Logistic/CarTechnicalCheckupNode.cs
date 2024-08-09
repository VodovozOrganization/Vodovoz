using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace VodovozBusiness.EntityRepositories.Logistic
{
	public class CarTechnicalCheckupNode
	{

		public CarTypeOfUse CarTypeOfUse { get; set; }
		public string CarRegNumber { get; set; }
		public string DriverGeography { get; set; }
		public CarEvent LastCarTechnicalCheckupEvent { get; set; }
		public int? DaysLeftToNextTechnicalCheckup =>
			LastCarTechnicalCheckupEvent?.CarTechnicalCheckupEndingDate is null
			? null
			: (int?)(LastCarTechnicalCheckupEvent.CarTechnicalCheckupEndingDate.Value - DateTime.Today).TotalDays;
	}
}
