using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic.Cars
{
	public class CarOwnership : IDomainObject
	{
		public virtual int Id { get; }
		
		public virtual CarModel CarModel { get; set; }
		
		public virtual CarOwnershipType CarOwnershipType { get; set; }
		
		public virtual DateTime StartDate { get; set; }
		
		public virtual DateTime EndDate { get; set; }
	}
}
