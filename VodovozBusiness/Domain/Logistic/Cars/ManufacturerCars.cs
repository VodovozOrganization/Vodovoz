using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic.Cars
{
	public class ManufacturerCars : IDomainObject
	{
		public virtual int Id { get; }
		
		public virtual string Name { get; set; }
	}
}
