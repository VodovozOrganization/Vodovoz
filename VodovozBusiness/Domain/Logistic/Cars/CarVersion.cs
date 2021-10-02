using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic.Cars
{
	public class CarVersion : IDomainObject, INotifyPropertyChanged
	{
		public virtual int Id { get; }
		
		public virtual DateTime StartDate { get; set; }
		
		public virtual DateTime? EndDate { get; set; }
		
		public virtual OwnershipCar OwnershipCar { get; set; }
		
		public virtual Car Car { get; set; }
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
