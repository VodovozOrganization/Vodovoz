using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "автомобили",
		Nominative = "автомобиль")]
	public class Car : BusinessObjectBase<Car>, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string model;

//		[Required (ErrorMessage = "Модель автомобиля должна быть заполнена.")]
		[Display (Name = "Модель")]
		public virtual string Model {
			get { return model; }
			set { SetField (ref model, value, () => Model); }
		}

		string registrationNumber;

		[Display (Name = "Гос. номер")]
//		[Required (ErrorMessage = "Гос. номер автомобиля должен быть заполнен.")]
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

		private bool isCompanyHavings;

		[Display(Name = "Имущество компании")]
		public virtual bool IsCompanyHavings
		{
			get { return isCompanyHavings; }
			set { SetField(ref isCompanyHavings, value, () => IsCompanyHavings); }
		}

		private bool isTruck;

		[Display(Name = "Фура")]
		public virtual bool IsTruck {
			get { return isTruck; }
			set { SetField(ref isTruck, value, () => IsTruck); }
		}

		double maxVolume;

		[Display(Name = "Объём")]
		public virtual double MaxVolume {
			get { return maxVolume; }
			set { SetField(ref maxVolume, value, () => MaxVolume); }
		}

		int maxWeight;

		[Display(Name = "Грузоподъёмность")]
		public virtual int MaxWeight {
			get { return maxWeight; }
			set { SetField(ref maxWeight, value, () => MaxWeight); }
		}

		int minBottles;

		[Display(Name = "Минимум бутылей")]
		public virtual int MinBottles {
			get { return minBottles; }
			set { SetField(ref minBottles, value, () => MinBottles); }
		}

		int maxBottles;

		[Display(Name = "Максимум бутылей")]
		public virtual int MaxBottles {
			get { return maxBottles; }
			set { SetField(ref maxBottles, value, () => MaxBottles); }
		}

		int minRouteAddresses;

		[Display(Name = "Минимум адресов")]
		public virtual int MinRouteAddresses {
			get { return minRouteAddresses; }
			set { SetField(ref minRouteAddresses, value, () => MinRouteAddresses); }
		}

		int maxRouteAddresses;

		[Display(Name = "Максимум адресов")]
		public virtual int MaxRouteAddresses {
			get { return maxRouteAddresses; }
			set { SetField(ref maxRouteAddresses, value, () => MaxRouteAddresses); }
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

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (string.IsNullOrWhiteSpace(Model))
				yield return new ValidationResult ("Модель автомобиля должна быть заполнена", new[] { "Model" });
			
			if (string.IsNullOrWhiteSpace(RegistrationNumber))
				yield return new ValidationResult ("Гос. номер автомобиля должен быть заполнен", new[] { "RegistrationNumber" });
			
			if (FuelType == null)
				yield return new ValidationResult ("Тип топлива должен быть заполнен", new[] { "FuelType" });
			
			if (FuelConsumption <= 0)
				yield return new ValidationResult ("Расход топлива должен быть больше 0", new[] { "FuelConsumption" });

			var cars = UoW.Session.QueryOver<Car>()
				.Where(c => c.RegistrationNumber == this.RegistrationNumber)
				.WhereNot(c => c.Id == this.Id)
				.List();

			if (cars.Count > 0)
			{
				yield return new ValidationResult ("Автомобиль уже существует", new[] { "Duplication" });
			}
		}

		#endregion
	}
}

