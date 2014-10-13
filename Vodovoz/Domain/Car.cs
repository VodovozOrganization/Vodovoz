using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Автомобиль")]
	public class Car : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Model { get; set; }
		public virtual string RegistrationNumber { get; set; }
		public virtual double FuelConsumption { get; set; }
		public virtual Employee Driver { get; set; }
		#endregion

		public Car ()
		{
			Model = String.Empty;
			RegistrationNumber = String.Empty;
		}
	}
}

