using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "модель автомобиля",
		NominativePlural = "модели автомобилей")]
	[EntityPermission]
	[HistoryTrace]
	public class CarModel : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private CarManufacturer _carManufacturer;
		private CarTypeOfUse _carTypeOfUse;
		private bool _isArchive;
		private int _maxWeight;
		private decimal _maxVolume;
		private IList<CarFuelVersion> _carFuelVersions = new List<CarFuelVersion>();
		private GenericObservableList<CarFuelVersion> _observableCarFuelVersions;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Производитель")]
		public virtual CarManufacturer CarManufacturer
		{
			get => _carManufacturer;
			set => SetField(ref _carManufacturer, value);
		}

		[Display(Name = "Тип")]
		public virtual CarTypeOfUse CarTypeOfUse
		{
			get => _carTypeOfUse;
			set => SetField(ref _carTypeOfUse, value);
		}

		[Display(Name = "Архивная")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Грузоподъёмность")]
		public virtual int MaxWeight
		{
			get => _maxWeight;
			set => SetField(ref _maxWeight, value);
		}

		[Display(Name = "Объём")]
		public virtual decimal MaxVolume
		{
			get => _maxVolume;
			set => SetField(ref _maxVolume, value);
		}

		public virtual IList<CarFuelVersion> CarFuelVersions
		{
			get => _carFuelVersions;
			set => SetField(ref _carFuelVersions, value);
		}

		public virtual GenericObservableList<CarFuelVersion> ObservableCarFuelVersions => _observableCarFuelVersions
			?? (_observableCarFuelVersions = new GenericObservableList<CarFuelVersion>(CarFuelVersions));

		public virtual string Title => $"{CarManufacturer.Name} {Name}";

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название должно быть заполнена", new[] { nameof(Name) });
			}
			if(Name?.Length > 100)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина названия ({Name.Length}/100).",
					new[] { nameof(Name) });
			}
			if(CarManufacturer == null)
			{
				yield return new ValidationResult("Производитель должен быть заполнен", new[] { nameof(CarManufacturer) });
			}
		}
	}

	public enum CarTypeOfUse
	{
		[Display(Name = "Фургон (Ларгус)")]
		Largus,
		[Display(Name = "Фура")]
		Truck,
		[Display(Name = "Грузовой")]
		GAZelle
	}
}
