using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "модель автомобиля",
		NominativePlural = "модели автомобилей",
		GenitivePlural = "моделей автомобиля")]
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
		private int _teсhInspectInterval;

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

		[Display(Name = "Интервал техосмотра")]
		public virtual int TeсhInspectInterval
		{
			get => _teсhInspectInterval;
			set => _teсhInspectInterval = value;
		}

		public virtual IList<CarFuelVersion> CarFuelVersions
		{
			get => _carFuelVersions;
			set => SetField(ref _carFuelVersions, value);
		}

		public virtual GenericObservableList<CarFuelVersion> ObservableCarFuelVersions => _observableCarFuelVersions
			?? (_observableCarFuelVersions = new GenericObservableList<CarFuelVersion>(CarFuelVersions));

		public virtual string Title => $"{CarManufacturer.Name} {Name}";

		public virtual CarFuelVersion GetCarFuelVersionOnDate(DateTime date)
		{
			return ObservableCarFuelVersions.FirstOrDefault(x =>
					x.StartDate <= date && (x.EndDate == null || x.EndDate >= date));
		}

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

			if(TeсhInspectInterval == 0)
			{
				yield return new ValidationResult("Интервал техосмотра должен быть заполнен.", new[] { nameof(TeсhInspectInterval) });
			}
		}
	}
}
