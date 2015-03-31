using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject ("Автомобили")]
	public class Car : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string model;

		[Required (ErrorMessage = "Модель автомобиля должна быть заполнена.")]
		public virtual string Model {
			get { return model; }
			set { SetField (ref model, value, () => Model); }
		}

		string registrationNumber;

		[Required (ErrorMessage = "Гос. номер автомобиля должен быть заполнен.")]
		public virtual string RegistrationNumber {
			get { return registrationNumber; }
			set { SetField (ref registrationNumber, value, () => RegistrationNumber); }
		}

		double fuelConsumption;

		public virtual double FuelConsumption {
			get { return fuelConsumption; }
			set { SetField (ref fuelConsumption, value, () => FuelConsumption); }
		}

		Employee driver;

		public virtual Employee Driver {
			get { return driver; }
			set { SetField (ref driver, value, () => Driver); }
		}

		bool isArchive;

		public virtual bool IsArchive {
			get { return isArchive; }
			set { SetField (ref isArchive, value, () => IsArchive); }
		}

		#endregion

		public string DriverInfo { get { return Driver.FullName; } }

		public Car ()
		{
			Model = String.Empty;
			RegistrationNumber = String.Empty;
		}
	}
}

