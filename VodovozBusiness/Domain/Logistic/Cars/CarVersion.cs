using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic.Cars
{
	public class CarVersion : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; }
		
		public virtual DateTime StartDate { get; set; }
		
		public virtual DateTime? EndDate { get; set; }
		
		public virtual CarOwnershipType CarOwnershipType { get; set; }

		private Car _car;
		public virtual Car Car 
		{ 
			get => _car;
			set => SetField(ref _car, value); 
		}

		[Display(Name = "Имущество компании")] public virtual bool IsCompanyCar => CarOwnershipType == CarOwnershipType.CompanyCar;
		
		public virtual bool IsRaskat => CarOwnershipType == CarOwnershipType.RaskatCar;
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (IsRaskat && Car.CarModel.TypeOfUse == null)
				yield return new ValidationResult("Для раскатного авто необходимо указать тип раската", new[] { nameof(IsRaskat), nameof(CarOwnershipType) });
		}
	}

	public enum CarOwnershipType
	{
		[Display(Name = "ТС компании")]
		CompanyCar,
		[Display(Name = "ТС в раскате")]
		RaskatCar,
		[Display(Name = "ТС наёмников")]
		HiredCar
	}
	
	public class OwnershipCarStringType : NHibernate.Type.EnumStringType
	{
		public OwnershipCarStringType() : base(typeof(CarOwnershipType)) { }
	}
}
