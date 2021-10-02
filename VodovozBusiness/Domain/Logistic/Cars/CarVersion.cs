using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic.Cars
{
	public class CarVersion : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; }
		
		public virtual DateTime StartDate { get; set; }
		
		public virtual DateTime? EndDate { get; set; }
		
		public virtual OwnershipCar OwnershipCar { get; set; }

		private Car _car;
		public virtual Car Car 
		{ 
			get => _car;
			set => SetField(ref _car, value); 
		}
	}

	public enum OwnershipCar
	{
		[Display(Name = "ТС компании")]
		CompanyCar,
		[Display(Name = "ТС в раскате")]
		RaskatCar,
		[Display(Name = "ТС наёмников")]
		HiredCar
	}
}
