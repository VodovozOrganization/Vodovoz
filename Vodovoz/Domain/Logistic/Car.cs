using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
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

		FuelType fuelType;

		[Display (Name = "Вид топлива")]
		public virtual FuelType FuelType {
			get { return fuelType; }
			set { SetField (ref fuelType, value, () => FuelType); }
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

		byte[] photo;

		[Display (Name = "Фотография")]
		public virtual byte[] Photo {
			get { return photo; }
			set { SetField (ref photo, value, () => Photo); }
		}

		#endregion

		public virtual string Title { 
			get { return String.Format ("{0} ({1})", Model, RegistrationNumber); } 
		}

		public Car ()
		{
			Model = String.Empty;
			RegistrationNumber = String.Empty;
		}
	}
}

