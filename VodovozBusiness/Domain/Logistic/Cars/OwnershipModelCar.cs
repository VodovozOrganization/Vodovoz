using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic.Cars
{
	public class OwnershipModelCar : IDomainObject
	{
		public virtual int Id { get; }
		
		public virtual ModelCar ModelCar { get; set; }
		
		public virtual OwnershipCar OwnershipCar { get; set; }
		
		public virtual DateTime StartDate { get; set; }
		
		public virtual DateTime EndDate { get; set; }
	}
}
