using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubjectAttributes("Автомобили")]
	public class Car : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		[Required(ErrorMessage = "Модель автомобиля должна быть заполнена.")]
		public virtual string Model { get; set; }
		[Required(ErrorMessage = "Гос. номер автомобиля должен быть заполнен.")]
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

