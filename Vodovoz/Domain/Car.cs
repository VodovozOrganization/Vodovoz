using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "автомобили",
		Nominative = "автомобиль")]
	public class Car : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string model;

		[Required (ErrorMessage = "Модель автомобиля должна быть заполнена.")]
		[Display (Name = "Модель")]
		public virtual string Model {
			get { return model; }
			set { SetField (ref model, value, () => Model); }
		}

		string registrationNumber;

		[Display (Name = "Гос. номер")]
		[Required (ErrorMessage = "Гос. номер автомобиля должен быть заполнен.")]
		public virtual string RegistrationNumber {
			get { return registrationNumber; }
			set { SetField (ref registrationNumber, value, () => RegistrationNumber); }
		}

		double fuelConsumption;

		[Display (Name = "Расход топлива")]
		public virtual double FuelConsumption {
			get { return fuelConsumption; }
			set { SetField (ref fuelConsumption, value, () => FuelConsumption); }
		}

		Employee driver;

		[Display (Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField (ref driver, value, () => Driver); }
		}

		bool isArchive;

		[Display (Name = "Архивный")]
		public virtual bool IsArchive {
			get { return isArchive; }
			set { SetField (ref isArchive, value, () => IsArchive); }
		}

		#endregion

		public virtual string Title { 
			get { return String.Format ("{0} ({1})", Model, RegistrationNumber); } 
		}

		public string DriverInfo { get { return Driver == null ? String.Empty : Driver.FullName; } }

		public Car ()
		{
			Model = String.Empty;
			RegistrationNumber = String.Empty;
		}
	}
}

