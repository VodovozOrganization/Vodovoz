using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Logistics.Cars;

namespace VodovozBusiness.EntityRepositories.Logistic
{
	public class CarEventGroup
	{
		public CarEventType CarEventType { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Comment { get; set; }
		public List<int> DayIndices { get; set; }
	}
}
