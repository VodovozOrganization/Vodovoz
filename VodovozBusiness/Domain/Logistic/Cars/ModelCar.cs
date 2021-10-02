using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic.Cars
{
	public class ModelCar : BusinessObjectBase<ModelCar>, IDomainObject
	{
		public virtual int Id { get; }

		public virtual string Name { get; set; }

		public virtual ManufacturerCars ManufacturerCars { get; set; }

		public virtual bool IsArchive { get; set; }
		
		public virtual decimal MaxWeigth { get; set; }
		
		public virtual decimal MaxVolume { get; set; }
	}
}
