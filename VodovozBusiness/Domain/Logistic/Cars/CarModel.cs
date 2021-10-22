using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "модели автомобиля",
		Nominative = "модель автомобиль")]
	[EntityPermission]
	[HistoryTrace]
	public class CarModel : BusinessObjectBase<CarModel>, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; }

		public virtual string Name { get; set; }

		public virtual ManufacturerCars ManufacturerCars { get; set; }
		
		public virtual CarTypeOfUse? TypeOfUse { get; set; }

		public virtual bool IsArchive { get; set; }
		
		public virtual int MaxWeight { get; set; }
		
		public virtual double MaxVolume { get; set; }

		public static CarTypeOfUse[] GetCompanyHavingsTypes() => new [] { CarTypeOfUse.GAZelle, CarTypeOfUse.Largus, CarTypeOfUse.Truck };
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult("Модель автомобиля должна быть заполнена", new[] { "Name" });
			
			if(!TypeOfUse.HasValue)
				yield return new ValidationResult("Вид автомобиля должен быть заполнен", new[] { nameof(TypeOfUse) });

		}
	}
	
	public enum CarTypeOfUse
	{
		[Display(Name = "Ларгус")]
		Largus,
		[Display(Name = "Фура")]
		Truck,
		[Display(Name = "ГАЗель")]
		GAZelle
	}
}
